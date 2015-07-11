//GlassRTBSupport
// scan files, import to BLG system

function GlassRTBSupport::init() {
	%this = new ScriptObject(GlassRTBSupport);
	%this.scanFiles();
}

function GlassRTBSupport::scanFiles(%this) {
	%pattern = "Add-ons/*/rtbInfo.txt";
	%idArrayLen = 0;
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
		echo("Detected \"" @ getsubstr(%file, 8, strlen(%file)-20) @ "\" as an old RTB add-on!");
	}

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

function GlassRTBSupportTCP::onDone(%this, %error) {
	echo("done - " @ %this.buffer);
	if(!%error) {
		%array = parseJSON(%this.buffer);
		if(getJSONType(%array) $= "array") {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.item[%i];
				echo(%obj.get("glassId"));
				%addonObj = %obj.get("addonData");
				%addonData = GlassFileData::create(%addonObj.get("name"),
						%addonObj.get("id"),
						1,
						%addonObj.get("filename"));
				GlassDownloadManager.fetchAddon(%addonData);
				echo("Porting \"" @ %addonData.filename @ "\" from RTB to Glass");
			}
		}
	}
}

function GlassRTBSupportTCP::handleText(%this, %line) {
	echo("RTBSupport - " @ %line);
	%this.buffer = trim(%this.buffer NL %line);
}