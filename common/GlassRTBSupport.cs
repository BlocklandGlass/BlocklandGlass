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

		%url = "http://" @ BLG.netAddress @ "/api/rtbConversion.php?mods=" @ %rtbIdStr;
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

function GlassRTBSupport::openGui(%force) {
	if(GlassUpdatesGui.getObject(0).getValue() $= "Add-On Updates" && GlassUpdatesGui.isAwake() && !%force) {
		echo("\c2Add-On Updates is currently open, setting the close button to open RTB Support");
		GlassUpdatesGui_Decline.command = "GlassRTBSupport::openGui(true);";
		//rewrite close command
	} else {
		GlassUpdatesGui.getObject(0).setText("RTB Imports");
		GlassUpdatesGui_Changelog.setText("<font:Arial:16>Blockland Glass automatically finds your old RTB files, then imports them!<br><br>Here's the updates that we have available for you!");
		GlassRTBSupport.render();

		GlassUpdatesGui_Decline.command = "GlassRTBSupport::closeGui();";
		GlassUpdatesGui_Accept.command = "GlassRTBSupport::doUpdates(true);";
		GlassUpdatesGui_Accept.text = "Import";

		canvas.pushDialog(GlassUpdatesGui);
	}
}

function GlassRTBSupport::render(%this) {
	%currentY = 0;
	GlassUpdatesGui_Scroll.clear();
	for(%i = 0; %i < GlassRTBSupportFiles.getCount(); %i++) {
		%file = GlassRTBSupportFiles.getObject(%i);
		GlassRTBSupport::buildSwatch(%file, %currentY);
		%currentY += 31;
		//echo("\c1 + Fetching \c4" @ %file.filename);
		//GlassDownloadManager.fetchAddon(%file);
	}
}

function GlassRTBSupport::buildSwatch(%file, %initY) {
  %textX = 28;
  %text = "<font:arial bold:16>" @ %file.name @ " <font:arial:14>" @ %file.filename;
  %gui = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = 1 SPC %initY;
    extent = "280 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "200 220 200 255";

    new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "center";
      position = %textX SPC 7;
      extent = "280 20";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      text = %text;
    };
  };

  %img = new GuiBitmapCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "7 7";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bricks.png";
    wrap = "0";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    keepCached = "0";
    mColor = "255 255 255 255";
    mMultiply = "0";
  };
  %gui.add(%img);

  GlassUpdatesGui_Scroll.add(%gui);
  return %gui;
}

function GlassRTBSupport::updateProgressBar(%id, %float) {
  %swatch = GlassUpdatesGui_Scroll.getObject(%id);

  if(%swatch.getObject(0).getClassName() !$= "GuiProgressCtrl") {
    %swatch.clear();
    %progress = new GuiProgressCtrl() {
      profile = "GuiProgressProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "5 5";
      extent = "265 20";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
    };
    %swatch.add(%progress);
  }
  %swatch.getObject(0).setValue(%float);
}

function GlassRTBSupportTCP::onDone(%this, %error) {
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
		if(GlassRTBSupport.files.getCount() > 0 && %this.last) {
			GlassRTBSupport::openGui();
		}
	}
}

function GlassRTBSupportTCP::handleText(%this, %line) {
	%this.buffer = trim(%this.buffer NL %line);
}
