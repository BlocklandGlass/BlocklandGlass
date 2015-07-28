function GlassUpdaterSupport::pushItem(%item) {
  if(!isObject(GlassUpdatesGroup)) {
    new ScriptGroup(GlassUpdatesGroup);
  }

  %up = new ScriptObject() {
    class = "GlassUpdate";
    addonHandler = %item;
  };

  GlassUpdatesGroup.add(%up);
}

function GlassUpdaterSupport::verifyInstall() {
  if(!isObject(updater)) {
    if(!isfile("Add-Ons/Support_Updater.zip")) {
      echo("Not found! Downloading Support_Updater!");
      %url = "http://mods.greek2me.us/storage/Support_Updater.zip";
    	%method = "GET";
    	%downloadPath = "Add-Ons/Support_Updater.zip";
    	%className = "GlassUpdaterSupportTCP";

    	connectToURL(%url, %method, %downloadPath, %className);

    	messageBoxOK("Blockland Glass", "Blockland Glass required:\n\nDownloading Support_Updater.");
    } else {
      echo("Support_Updater not loaded, but will.");
    }
  } else {
    echo("Support_Updater already loaded and active.");
  }
}

function GlassUpdaterSupport::pushGlassUpdater(%force) {
  if(GlassUpdatesGui.getObject(0).text !$= "Add-On Updates" && GlassUpdatesGui.isAwake() && !%force) {
		echo("\c2Add-On Updates is currently open, setting the close button to open RTB Support and redrawing");
    GlassUpdatesGui_Decline.command = "GlassRTBSupport::openGui(true);";
  } else {
    GlassUpdatesGui_Decline.command = "GlassUpdaterSupport::close();";
  }

  GlassUpdatesGui.getObject(0).setText("Add-On Updates");
  GlassUpdatesGui_Accept.command = "GlassUpdaterSupport::doUpdates();";
  GlassUpdatesGui_Accept.text = "Update";

  GlassUpdatesGui_Scroll.clear();
  %currentY = 1;
  for(%i = 0; %i < GlassUpdatesGroup.getCount(); %i++) {
    %glassUpdate = GlassUpdatesGroup.getObject(%i);
    %queueObj = %glassUpdate.addonHandler;
    
    %swatch = GlassUpdaterSupport::buildSwatch(%queueObj, %currentY);
    %queueObj.glassSwatch = %swatch;
    %currentY += 31;

    GlassUpdatesGui_Changelog.setText(%queueObj.updateChangeLogText);
    GlassUpdatesGui_Changelog.setVisible(true);
    GlassUpdatesGui_Changelog.getGroup().scrollToTop();
  }
  canvas.pushDialog(GlassUpdatesGui);
}

function GlassUpdaterSupport::buildSwatch(%queueObj, %initY) {
  %textX = 10;
  %name = %queueObj.name;
  if(isFile("Add-Ons/" @ %name @ "/glass.json")) {
    %fo = new FileObject();
    %fo.openForRead("Add-Ons/" @ %name @ "/glass.json");
    %buffer = "";
    while(!%fo.isEOF()) {
      %buffer = %buffer NL %fo.readLine();
    }
    %fo.close();
    %fo.delete();

    %glassDat = parseJSON(%buffer);

    %title = %glassDat.get("title");
    %text = "<font:arial bold:16>" @ %title @ " <font:arial:14>" @ %name;

    %boardId = %glassDat.get("board");
    if($BLG::MM::BoardCache::Image[%boardId] !$= "") {
      %boardImage = $BLG::MM::BoardCache::Image[%boardId];
      %textX = 28;
    }
  } else {
    %text = "<font:arial bold:16>" @ %name;
  }
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

  if(%boardImage !$= "") {
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
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %boardImage @ ".png";
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
  }

  %mouse = new GuiMouseEventCtrl(GlassUpdatesGui_Mouse) {
    queueObj = %queueObj;
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = "280 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";
  };

  %gui.add(%mouse);

  GlassUpdatesGui_Scroll.add(%gui);
  return %gui;
}

function GlassUpdatesGui_Mouse::onMouseEnter(%this) {
  %gs = %this.queueObj.glassSwatch;
  %gs.color = vectorAdd(%gs.color, "50 30 50") SPC 255;
}

function GlassUpdatesGui_Mouse::onMouseLeave(%this) {
  %gs = %this.queueObj.glassSwatch;
  %gs.color = vectorAdd(%gs.color, "-50 -30 -50") SPC 255;
}

function GlassUpdatesGui_Mouse::onMouseDown(%this, %a, %b, %c) {
  %queueObj = %this.queueObj;
  GlassUpdatesGui_Changelog.setText("<just:center><font:arial bold:20>" @ %queueObj.name @ "<just:left><font:arial:12><br><br>" @ %queueObj.updateChangeLogText);
  GlassUpdatesGui_Changelog.setVisible(true);
  GlassUpdatesGui_Changelog.getGroup().scrollToTop();
}

function GlassUpdatesGui_Mouse::onRightMouseDown(%this) {
  GlassUpdaterSupport::removeFromQueue(%this.queueObj);
}

function GlassUpdaterSupport::resize() {
  %currentY = 1;
  for(%i = 0; %i < GlassUpdatesGui_Scroll.getCount(); %i++) {
    %obj = GlassUpdatesGui_Scroll.getObject(%i);
    %obj.position = 1 SPC %currentY;
    %currentY += 31;
  }
  GlassUpdatesGui_Scroll.setVisible(true);
  GlassUpdatesGui_Scroll.scrollToTop();
}

function GlassUpdaterSupport::removeFromQueue(%obj) {
  if(%obj.name $= "System_BlocklandGlass") {
    //must update!
    messageBoxOk("Uh oh", "Updates for Blockland Glass are mandatory. It'll only take a minute!\n\nSorry about that.");
    return;
  }
  %obj.glassSwatch.delete();
  %obj.delete();
  GlassUpdaterSupport::resize();
}

function GlassUpdaterSupport::doUpdates() {
  updater.doUpdates();
}

function GlassUpdaterSupport::close() {
  %glassUpdate = false;
  for(%i = 0; %i < updater.queue.getCount(); %i++) {
    %queueObj = updater.queue.getObject(%i);
    if(%queueObj.name $= "System_BlocklandGlass") {
      %glassUpdate = true;
    } else {
      GlassUpdaterSupport::removeFromQueue(%queueObj);
    }
  }

  if(%glassUpdate) {
    messageBoxOk("Uh oh", "Updates for Blockland Glass are mandatory. It'll only take a minute!\n\nSorry about that.");
  } else {
    canvas.popDialog(GlassUpdatesGui);
  }
}

function GlassUpdaterSupport::updateProgressBar(%queueObj, %float) {
  %swatch = %queueObj.glassSwatch;

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

function GlassUpdaterSupportTCP::onDone(%this, %error) {
  if(%error) {
    messageBoxOK("Uh oh", "There was an error downloading Support_Updater. If the problem persists, please contact Scout31/Jincux");
  } else {
    messageBoxOK("Please Restart", "Please restart Blockland", "quit();");
  }
}


//function updaterDlg::clickQueueItemSkip(%this, %queueObj)
//{
//	%this.removeAddOnSwatch(%queueObj.guiSwatch);
//	%queueObj.delete();
//
//	if(updater.queue.getCount() < 1)
//		canvas.popDialog(%this);
//}

//function updaterDlg::clickUpdate(%this)
//{
//	updaterDlgBackButton.setText(" << HIDE");
//	updaterDlgUpdateButton.setActive(0);

//	updater.doUpdates();
//}

//function updaterDlg::clickQueueItem(%this, %queueObj)
//{
//	%count = updaterDlgAddOnSwatch.getCount();
//	for(%i = 0; %i < %count; %i ++)
//		updaterDlgAddOnSwatch.getObject(%i).setColor("0 0 0 110");
//	%queueObj.guiSwatch.setColor("255 255 255 110");
//
//	%this.viewItem = %queueObj;
//
//	%info = "<color:ffffff><h1>" @ %queueObj.name @ "</h1>"
//		NL "<ul><li><b>Version:</b>" SPC %queueObj.updateVersion @ "</li>"
//		NL "<b>Restart Required?</b>" SPC (%queueObj.updateRestartRequired ? "Yes" : "No") @ "</li>"
//		NL "<b>Repository:</b>" SPC %queueObj.repository.url @ "</li></ul>";
//	if(strLen(%queueObj.updateDescription))
//		%info = %info NL "\n<h2>Description:</h2>" @ %queueObj.updateDescription;
//	%info = parseCustomTML(%info, %text, "default");
//	updaterDlgInfoText.setText(%info);
//
//	%this.viewChangeLog(%queueObj);
//}

if($BLG::MM::UseUpdaterDefault $= "") {
  $BLG::MM::UseUpdaterDefault = false; //use BLG skinned
}

//updaterInterfacePushItem(%item) //Called when an item is added to the display.
//updaterInterfacePopItem(%item) //Called when an item is removed from the display.
//updaterInterfaceSelectItem(%item) //Called when the updater selects an item to display in detail (changelog, etc).
//updaterInterfaceOnQueueEmpty() //Called when the queue is empty.
//updaterInterfaceDisplay(%refreshing) //Called when updates are ready to be displayed to the user.

package GlassUpdaterSupportPackage {
  function canvas::pushDialog(%this, %dlg) {
    if(%dlg.getName() $= "updaterDlg" && !$BLG::MM::UseUpdaterDefault) {
      echo("Pushing glass updater instead!");
      GlassUpdaterSupport::pushGlassUpdater();
      return;
    } else {
      return parent::pushDialog(%this, %dlg);
    }
  }

  function updaterInterfacePushItem(%item) {
    GlassUpdaterSupport::pushItem(%item);
    parent::updaterInterfacePushItem(%item);
  }

  function updaterInterfaceDisplay(%refreshing) {
    GlassUpdaterSupport::pushGlassUpdater(false);
    parent::updaterInterfaceDisplay(%refreshing);
  }

  function UpdaterDownloadTCP::setProgressBar(%this, %value) {
    if(!$BLG::MM::UseUpdaterDefault) {
	     %queueObj = updater.queue.currentDownload;
      GlassUpdaterSupport::updateProgressBar(%queueObj, %value);
    }

    parent::setProgressBar(%this, %value);
  }

  function doSupportUpdaterInstallNotify() {
    return;
  }
};
activatePackage(GlassUpdaterSupportPackage);
