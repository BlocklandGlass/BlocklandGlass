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

	if(BLG.enableCLI)
		echo("\c1Use \c5GlassRTBSupport::doUpdates(); \c1to download the latests versions");

	%rtbIdStr = "";
	for(%i = 0; %i < %idArrayLen; %i++) {
		if(%i > 0) {
			%rtbIdStr = %rtbIdStr @ "-";
		}
		%rtbIdStr = %rtbIdStr @ %idArray[%i];
	}

	%url = "http://" @ BLG.netAddress @ "/api/rtbConversion.php?mods=" @ %rtbIdStr;
	%method = "GET";
	%downloadPath = "";
	%className = "GlassRTBSupportTCP";
	%tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassRTBSupport::doUpdates(%this) {
	echo("\c1Doing RTB to Glass conversions");
	echo("\c3Available: " @ GlassRTBSupportFiles.getCount());
	echo("");
	for(%i = 0; %i < GlassRTBSupportFiles.getCount(); %i++) {
		%file = GlassRTBSupportFiles.getObject(%i);
		echo("\c1 + Fetching \c4" @ %file.filename);
		GlassDownloadManager.fetchAddon(%file);
	}
}

function GlassRTBSupportTCP::onDone(%this, %error) {
	echo("done - " @ %this.buffer);
	if(!%error) {
		%array = parseJSON(%this.buffer);
		if(getJSONType(%array) $= "array" && %array.length) {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.item[%i];
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
	}
}

function GlassRTBSupportTCP::handleText(%this, %line) {
	echo("RTBSupport - " @ %line);
	%this.buffer = trim(%this.buffer NL %line);
}
