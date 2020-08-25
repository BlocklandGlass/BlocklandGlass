function GMM_ImportPage::init() {
  new ScriptObject(GMM_ImportPage);
  GlassGroup.add(GMM_ImportPage);
}

function GMM_ImportPage::open(%this) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 0 255 0";
    position = "0 0";
    extent = "635 498";
  };

  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 498";
  };

  %this.container = %container;
  %this.body = %body;
  %container.add(%body);

  %this._unopened = false;

  GlassModManager::placeCall("rtb", "", "GMM_ImportPage.handleResults");
  GlassModManagerGui.loadContext(false);

  return %container;
}

function GMM_ImportPage::close(%this) {

}

function GMM_ImportPage::handleResults(%this, %obj) {
  if(%obj.status !$= "success" && !%this._unopened) {
    %this.handleNonSuccess(%obj);
    return;
  }

  %container = %this.container;
  %body      = %this.body;

  %addons    = %obj.addons;

  %this.imports = 0;

  for(%j = 0; %j < %addons.length; %j++) {
    %obj = %addons.value[%j];
    %this.importData[%obj.id] = %obj;
  }

  %pattern = "Add-ons/*/rtbInfo.txt";
  %fo = new FileObject();
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-20);
    if(strPos(%name, "/") > -1) continue;
    if(strPos(%name, "_") == -1) continue;
    if(!isFile("Add-Ons/" @ %name @ ".zip")) continue;

    if(isFile(filePath(%file) @ "/glass.json"))
      continue;

    %fo.openForRead(%file);

    %id = false;
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      if(getWord(%line, 0) $= "id:") {
        %id = getWord(%line, 1);
      }
    }

    %fo.close();

    if(!%id) {
      GlassLog::error("\c3Found rtbInfo.txt but no id for " @ %name);
      continue;
    }

    if(%this.importData[%id]) {

      %this.import[%this.imports+0]     = %this.importData[%id];
      %this.importName[%this.imports+0] = %name;
      %this.imports++;

    }
  }
  %fo.delete();

  // super hacky but not worth it to redesign mod manager at the moment
  if(%this._unopened) {
    if(%this.imports > 0)
      glassMessageBoxYesNo("RTB Imports Available", "You have " @ %this.imports @ " RTB " @ (%this.imports == 1 ? "add-on" : "add-ons") @ " that can be updated to a new version. Would you like to do so now?", "GlassOverlay::openModManager(true); GlassModManagerGui.openPage(GMM_ImportPage);");
    return;
  }
  GlassModManagerGui.pageDidLoad(%this);

  if(%this.imports > 0) {
    %button = new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonWhiteProfile";
      position = "10 10";
      extent = "120 35";
      bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

      text = "Download";

      command = "GMM_ImportPage.downloadClick();";

      mColor = "84 217 140 255";
    };
    %this.button = %button;
    %body.add(%button);
    %last = %button;
  }

  for(%i = 0; %i < %this.imports; %i++) {
    %import = %this.import[%i];
    %name   = %this.importName[%i];

    %swatch = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "595 62";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "240 240 240 255";
      import = %import;
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 9";
      extent = "356 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
      text = "<font:verdana bold:14><bitmap:Add-Ons/System_BlocklandGlass/image/icon/bricks> <color:e74c3c>" @ %name @ ".zip<color:666666><font:verdana:14> is now<font:verdana bold:14><color:2ecc71> " @ %import.glass_name;
    };

    %swatch.progress = new GuiProgressCtrl() {
      profile = "GuiProgressProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 29";
      extent = "575 23";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
    };

    %swatch.progresstext = new GuiMLTextCtrl() {
       profile = "GuiMLTextProfile";
       horizSizing = "center";
       vertSizing = "center";
       position = "0 5";
       extent = "575 14";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       lineSpacing = "2";
       allowColorChars = "0";
       maxChars = "-1";
       maxBitmapHeight = "-1";
       selectable = "1";
       autoResize = "1";
       text = "<shadow:1:1><just:center><font:verdana:12><color:999999>Waiting...";
    };
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.progress);
    %swatch.progress.add(%swatch.progresstext);

    %this.importSwatch[%i] = %swatch;

    %body.add(%swatch);
    if(%last !$= "")
      %swatch.placeBelow(%last, 10);

    %last = %swatch;
  }

  if(%this.imports == 0) {
    %text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 9";
      extent = "595 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
      text = "<font:verdana bold:14><just:center>No RTB Imports!";
    };
    %body.add(%text);
  }


  %body.verticalMatchChildren(0, 10);
  %container.verticalMatchChildren(0, 10);
  GlassModManagerGui.resizePage();

  %this.data = %obj;
}

function GMM_ImportPage::downloadClick(%this) {
  %this.button.enabled = false;
  %this.button.mColor = "150 150 150 128";
  %this.doDownload();
}

function GMM_ImportPage::doDownload(%this) {
  %this.downloadIndex = 0;
  %this.nextDownload(true);
}

function GMM_ImportPage::nextDownload(%this, %first) {
  %this.downloadIndex += %first ? 0 : 1;

  if(%this.downloadIndex >= %this.imports) {
    %this.downloadsDone();
    return;
  }

  %data = %this.import[%this.downloadIndex];

  %dl = GlassDownloadManager::newDownload(%data.glass_id, 1);

  %dl.addHandle("done",       "GMM_ImportPage_downloadDone"      );
  %dl.addHandle("progress",   "GMM_ImportPage_downloadProgress"  );
  %dl.addHandle("failed",     "GMM_ImportPage_downloadFailed"    );
  %dl.addHandle("unwritable", "GMM_ImportPage_downloadUnwritable");

  %dl.startDownload();
}

function GMM_ImportPage::downloadsDone(%this) {
  if(%this.wasError) {
    %errorClause = "<br><br><color:e74c3c>Warning: One or more add-ons failed to download properly. If this issue persists, please report it.";
  }

  if(%this.restart) {
    glassMessageBoxYesNo("Downloads Complete", "One or more of the imported add-ons has client scripts. These won't load until you have restarted Blockland. Would you like to do so now?" @ %errorClause, "quit();", "setModPaths(getModPaths());");
  } else {
    glassMessageBoxOk("Downloads Complete", "All downloads have completed." @ %errorClause, "setModPaths(getModPaths());");
  }

}

function GMM_ImportPage_downloadProgress(%dl, %float, %tcp) {
  %this = GMM_ImportPage;
  %swatch = %this.importSwatch[%this.downloadIndex];
  %swatch.progress.setValue(%float);
  %swatch.progresstext.setValue(mFloor(%float*100) @ " %");
}

function GMM_ImportPage_downloadDone(%dl, %err, %tcp) {
  %this = GMM_ImportPage;
  %originalPath = "Add-Ons/" @ %this.importName[%this.downloadIndex] @ ".zip";
  if(%tcp.savePath !$= %originalPath) {
    fileDelete(%originalPath);
    GlassLog::log("\c1Deleting original file \c0" @ %originalPath);
  } else {
    GlassLog::log("\c1Replaced \c0" @ %originalPath);
  }

  discoverFile(%tcp.savePath);
  %dir = %tcp.savePath;
  %dir = getSubStr(%dir, 0, strlen(%dir)-4);
  if(isFile(%dir @ "/client.cs")) {
    %this.restart = true;
  }

  %swatch = %this.importSwatch[%this.downloadIndex];
  %swatch.progress.setValue(1);
  %swatch.progresstext.setValue("<shadow:1:1><just:center><font:verdana:12><color:54d98c>Done");

  GMM_ImportPage.nextDownload();
}

function GMM_ImportPage_downloadFailed(%dl, %error) {
  GlassLog::error("Import download failed for " @ %this.importName[%this.downloadIndex]);
  %this = GMM_ImportPage;
  %swatch = %this.importSwatch[%this.downloadIndex];
  %swatch.progresstext.setValue("<shadow:1:1><just:center><font:verdana:12><color:ed7669>FAILED");

  GMM_ImportPage.nextDownload();
}

function GMM_ImportPage_downloadUnwritable(%dl) {
  GlassLog::error("Path unwritable for " @ %this.importName[%this.downloadIndex]);
  %this = GMM_ImportPage;
  %swatch = %this.importSwatch[%this.downloadIndex];
  %swatch.progresstext.setValue("<shadow:1:1><just:center><font:verdana:12><ed7669:54d98c>PATH UNWRITABLE");

  GMM_ImportPage.nextDownload();
}

function GlassModManagerGui::openRTBImport(%addons) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "495 64";
  };

  for(%i = 0; %i < %addons.length; %i++) {
    %import = %addons.value[%i];

    %swatch = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "465 62";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "230 230 230 255";
      import = %import;
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 9";
      extent = "356 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
      text = "<font:verdana bold:14><bitmap:Add-Ons/System_BlocklandGlass/image/icon/bricks> <color:dd0000>" @ %import.filename @ ".zip<color:666666> -> <color:00cc66>" @ %import.glass_name;
    };

    %swatch.progress = new GuiProgressCtrl() {
      profile = "GuiProgressProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 29";
      extent = "444 23";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
    };

    %swatch.progresstext = new GuiMLTextCtrl() {
       profile = "GuiMLTextProfile";
       horizSizing = "center";
       vertSizing = "center";
       position = "44 5";
       extent = "356 14";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       lineSpacing = "2";
       allowColorChars = "0";
       maxChars = "-1";
       maxBitmapHeight = "-1";
       selectable = "1";
       autoResize = "1";
       text = "<shadow:1:1><just:center><font:verdana:12><color:999999>Waiting...";
    };
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.progress);
    %swatch.progress.add(%swatch.progresstext);

    %container.add(%swatch);
    if(%last !$= "")
      %swatch.placeBelow(%last, 10);

    %last = %swatch;
  }

  glassMessageBoxOk("RTB Reclamation", "<font:verdana:12>Some of your old RTB add-ons have updates available through Glass! We'll go ahead and fetch those for you!", "GlassModManager::doRTBImport();");

  %container.verticalMatchChildren(0, 10);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;
  GlassModManagerGui_MainDisplay.setVisible(true);

  //%container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
}

function GlassModManager::doRTBImport() {
  %container = GlassModManagerGui_MainDisplay.getObject(0);
  for(%i = 0; %i < %container.getCount(); %i++) {
    %swatch = %container.getObject(%i);
    GlassModManager::downloadAddon(%swatch.import.glass_id, false, %swatch.progress);
  }
}
