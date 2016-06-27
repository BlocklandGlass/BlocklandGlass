//GlassRTBSupport
// scan files, import to BLG system

function GlassRTBSupport::init() {
	%this = new ScriptObject(GlassRTBSupport);
	%this.files = new ScriptGroup(GlassRTBSupportFiles);
	%this.scanFiles();
}

function GlassRTBSupport::scanFiles(%this) {
	%pattern = "Add-ons/*/rtbInfo.txt";
	%idArrayLen = 0;
	echo("\c1Looking for RTB Add-Ons");
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
		%fo = new FileObject();
		%fo.openForRead(%file);

		while(!%fo.isEOF()) {
			%line = %fo.readLine();
			if(getWord(%line, 0) $= "id:") {
				%rtbId = getWord(%line, 1);
				break;
			}
		}

		%fo.close();
		%fo.delete();

		%idArray[%idArrayLen] = %rtbId;
		%idArrayLen++;

		echo(" \c1+ Found \c4\"" @ getsubstr(%file, 8, strlen(%file)-20) @ "\"");
	}

	if($Server::Dedicated)
		echo("\c1Use \c5GlassRTBSupport::doUpdates(); \c1to download the latests versions");

	//4k string limit, so... send 25 at a time
	%remaining = %idArrayLen;
	for(%j = 0; %j < %idArrayLen/25; %j++) {
		%rtbIdStr = "";
		for(%i = 0; %i < 25; %i++) {
			%remaining--;
			if(%i > 0) {
				%rtbIdStr = %rtbIdStr @ "-";
			}
			%rtbIdStr = %rtbIdStr @ %idArray[(%j*25) + %i];
		}

		%url = "http://" @ Glass.address @ "/api/rtbConversion.php?mods=" @ %rtbIdStr;
		%method = "GET";
		%downloadPath = "";
		%className = "GlassRTBSupportTCP";
		%tcp = connectToURL(%url, %method, %downloadPath, %className);

		if(%remaining < 0) {
			%tcp.last = true;
		}
	}
}

function GlassRTBSupport::doUpdates(%visual) {
	echo("\c1Doing RTB to Glass conversions");
	echo("\c3Available: " @ GlassRTBSupportFiles.getCount());
	for(%i = 0; %i < GlassRTBSupportFiles.getCount(); %i++) {
		%file = GlassRTBSupportFiles.getObject(%i);
		if(%visual) {
			%file.rtbImport = true;
			%file.rtbImportId = %i;
		}
		echo("\c1 + Fetching \c4" @ %file.filename);
		GlassDownloadManager.fetchAddon(%file);
	}
}

function GlassRTBSupport::closeGui() {
	canvas.popDialog(GlassUpdatesGui);
}

function GlassRTBSupport::openGui() {
	%rtb = GlassDownloadInterface::openContext("RTB Imports", "We've found updated versions of some of your old RTB add-ons!");

	for(%i = 0; %i < GlassRTBSupportFiles.getCount(); %i++) {
		%file = GlassRTBSupportFiles.getObject(%i);
		%handler = %rtb.addDownload("<font:verdana bold:16>" @ %file.name @ " <font:verdana:14>" @ %file.filename, "bricks", "");
		%file.downloadHandler = %handler;
		if(GlassRTBSupportFiles.getCount()-1 == %i) {
			%handler.last = true;
		}
	}

  %rtb.registerCallback("GlassRTBSupport::updateCallback");
}

function GlassRTBSupport::updateCallback(%code) {
	//1 = accept, -1 = decline, 2 = done
	if(%code == 1) {
		GlassRTBSupport::doUpdates(true);
	}

	if(%code == 2) {
		messageBoxOk("Restart", "You may need to restart Blockland for these add-ons to take effect.");
	}
}

function GlassRTBSupport::updateProgressBar(%handle, %float) {
	%handle.setProgress(%float);
	if(%float == 1 && %handle.last) {
		GlassDownloadGui.onDone();
	}
}

function GlassRTBSupportTCP::onDone(%this, %error) {
	if($Glass::Debug) {
		Glass::debug(%this.buffer);
	}
	if(!%error) {
		jettisonParse(collapseEscape(%this.buffer));
		%array = $JSON::Value;
		if(getJSONType(%array) $= "array" && %array.length) {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.value[%i];
				%addonObj = %obj.get("addonData");
				%addonData = GlassFileData::create(%addonObj.get("name"),
						%addonObj.get("id"),
						1,
						%addonObj.get("filename"));
				GlassRTBSupport.files.add(%addonData);
				//GlassDownloadManager.fetchAddon(%addonData);
				//echo("Porting \"" @ %addonData.filename @ "\" from RTB to Glass");
			}
		}
		if(GlassRTBSupport.files.getCount() > 0 && %this.last) {
			GlassRTBSupport::openGui();
		}
	}
	%this.buffer = "";
}

function GlassRTBSupportTCP::handleText(%this, %line) {
	%this.buffer = trim(%this.buffer NL %line);
}
