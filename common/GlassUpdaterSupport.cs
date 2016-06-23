function GlassUpdaterSupport::pushItem(%item) {
  if(!isObject(GlassUpdatesGroup)) {
    new ScriptGroup(GlassUpdatesGroup);
  }

  %up = new ScriptObject() {
    class = "GlassUpdate";
    addonHandler = %item;
  };

  GlassUpdatesGroup.add(%up);
  GlassUpdaterSupport::pushGlassUpdater(false);
}

function GlassUpdaterSupport::downloadGui(%code) {
  if(%code == 1) {
    GlassUpdaterSupport::doUpdates();
  } else if(%code == -1) {

  }
}

function GlassUpdaterSupport::pushGlassUpdater(%force) {
  if(isObject(GlassUpdatesGroup.ctx)) return;

  %ctx = GlassDownloadInterface::openContext("Add-On Updates", "Updates are available! Click on an add-on to view it's change-log, or right click to prevent it from updating.");
  %ctx.registerCallback("GlassUpdaterSupport::downloadGui");
  GlassUpdatesGroup.ctx = %ctx;

  for(%i = 0; %i < GlassUpdatesGroup.getCount(); %i++) {
    %glassUpdate = GlassUpdatesGroup.getObject(%i);
    %queueObj = %glassUpdate.addonHandler;


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

      jettisonParse(%buffer);
      %glassDat = $JSON::Value;

      %title = %glassDat.get("title");
      %text = "<font:verdana bold:16>" @ %title @ " <font:verdana:14>" @ %name;

      %boardId = %glassDat.get("board");
      if(GlassSettings.cacheFetch("MM::BoardImage[" @ %boardId @ "]") !$= "") {
        %boardImage = GlassSettings.cacheFetch("MM::BoardImage[" @ %boardId @ "]");
      }
    } else {
      %text = "<font:arial bold:16>" @ %name;
      %boardImage = "";
    }

    %glassUpdate.dlHandler = %ctx.addDownload(%text, %boardImage, "GlassUpdaterSupport::interact");
    %glassUpdate.dlHandler.glassUpdate = %glassUpdate;
    %glassUpdate.dlHandler.queueObj = %queueObj;

    %queueObj.dlHandler = %glassUpdate.dlHandler;
  }
}

function GlassUpdaterSupport::interact(%obj, %right) {
  if(%right) {
    GlassUpdaterSupport::removeFromQueue(%obj.queueObj);
  } else {
    GlassDownloadGui_Text.setText("<just:center><font:arial bold:20>" @ %obj.queueObj.name @ "<just:left><font:arial:12><br><br>" @ %obj.queueObj.updateChangeLogText);
    GlassDownloadGui_Text.setVisible(true);
    GlassDownloadGui_Text.getGroup().scrollToTop();
  }
}

function GlassUpdaterSupport::removeFromQueue(%glassObj) {
  %obj = %glassObj.addonHandler;
  if(%obj.name $= "System_BlocklandGlass") {
    //must update!
    messageBoxOk("Uh oh", "Updates for Blockland Glass are mandatory. It'll only take a minute!\n\nSorry about that.");
    return;
  }
  if(%obj.name $= "Support_Updater") {
    //must update!
    messageBoxOk("Uh oh", "Updates for Support_Updater are mandatory. It'll only take a minute!\n\nSorry about that.");
    return;
  }

  %glassObj.dlHandler.cancelDownload();
  %glassObj.delete();
}

function GlassUpdaterSupport::doUpdates() {
  updater.doUpdates();
  GlassUpdatesGroup.didUpdate = true;
}

function GlassUpdaterSupport::close() {
  %glassUpdate = false;
  for(%i = 0; %i < GlassUpdatesGroup.getCount(); %i++) {
    %glassObj = GlassUpdatesGroup.getObject(%i);
    %queueObj = %glassObj.addonHandler;
    if(%queueObj.name $= "System_BlocklandGlass") {
      %glassUpdate = true;
    } else {
      GlassUpdaterSupport::removeFromQueue(%glassObj);
    }
  }

  if(%glassUpdate && !GlassUpdatesGroup.didUpdate) {
    %ctx.inhibitClose(true);
    messageBoxOk("Uh oh", "Updates for Blockland Glass are mandatory. It'll only take a minute!\n\nSorry about that.");
  } else {
    %ctx.inhibitClose(false);
  }
}

function GlassUpdaterSupport::updateProgressBar(%queueObj, %float) {
  %dm = %queueObj.dlHandler;
  %dm.setProgress(%float);
}

if(!$Server::isDedicated)
  GlassModManagerGui_Prefs_Updater.setValue(GlassSettings.get("MM::UseDefault"));

function GlassUpdaterSupport::updateSetting() {
  %i = GlassModManagerGui_Prefs_Updater.getValue();
  GlassSettings.update("MM::UseDefault", %i);
  if(%i) {
    echo("Using default Support_Updater dialogs");
  } else {
    echo("Using Glass updater interface");
  }
}

package GlassUpdaterSupportPackage {
  function updaterInterfacePushItem(%item) {
    if(!GlassSettings.get("MM::UseDefault"))
      GlassUpdaterSupport::pushItem(%item);

    parent::updaterInterfacePushItem(%item);
  }

  function updaterInterfaceDisplay(%refreshing) {
    //not called!
    if(!GlassSettings.get("MM::UseDefault"))
      GlassUpdaterSupport::pushGlassUpdater(false);

    parent::updaterInterfaceDisplay(%refreshing);
  }

  function canvas::pushDialog(%cvs, %dlg) {
    if(%dlg !$= "UpdaterDlg" || GlassSettings.get("MM::UseDefault")) {
      parent::pushDialog(%cvs, %dlg);
    }
  }

  function UpdaterDownloadTCP::setProgressBar(%this, %value) {
    if(!GlassSettings.get("MM::UseDefault")) {
      %queueObj = updater.fileDownloader.currentDownload;
      GlassUpdaterSupport::updateProgressBar(%queueObj, %value);
    }

    parent::setProgressBar(%this, %value);
  }

  function doSupportUpdaterInstallNotify() {
    return parent::doSupportUpdaterInstallNotify();
  }
};
activatePackage(GlassUpdaterSupportPackage);
