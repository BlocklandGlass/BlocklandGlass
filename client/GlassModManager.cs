function GlassModManager::init() {
  exec("./GlassModManagerGui.cs");

  // TODO this can probably be cleaned up
  GMM_ActivityPage::init();
  GMM_AddonPage::init();
  GMM_BoardsPage::init();
  GMM_BoardPage::init();
  GMM_ColorsetsPage::init();
  GMM_ErrorPage::init();
  GMM_MyAddonsPage::init();
  GMM_SearchPage::init();

  GMM_RTBAddonPage::init();
  GMM_RTBBoardsPage::init();
  GMM_RTBBoardPage::init();
  GMM_ImportPage::init();

  GMM_Navigation::init();

  if(isObject(GlassModManager)) {
    return;
  }

  new ScriptObject(GlassModManager);

  GlassModManager::scanForRTB();
}

function getLongUTF8String(%str) {
  %res = "";
  %word = "";
  for(%i = 0; %i < strlen(%str); %i++) {
    %char = getSubStr(%str, %i, 1);
    if(%char $= " " || %char $= "\n" || %char $= "\t" || %i == strlen(%str)-1) {
      %res = %res @ getUTF8String(%word) @ %char;
      %word = "";
    } else {
      %word = %word @ %char;
    }
  }
  return %res;
}

function getLongASCIIString(%str) {
  %res = "";
  %word = "";
  for(%i = 0; %i < strlen(%str); %i++) {
    %char = getSubStr(%str, %i, 1);
    if(%char $= " " || %char $= "\n" || %char $= "\t" || %i == strlen(%str)-1) {
      %res = %res @ getASCIIString(%word) @ %char;
      %word = "";
    } else {
      %word = %word @ %char;
    }
  }
  return %res;
}

function GlassModManagerImageMouse::onMouseDown(%this) {
  canvas.popDialog(GlassModManagerImage);
}

function GlassModManagerGui_Window::onWake(%this) {
  if(!GlassModManagerGui.firstWake) {
    GlassModManagerGui.loadContext("activity");
    GlassModManagerGui.firstWake = true;
  }
}

//
// TODO possible name change or relocation
//
function GlassModManager::changeKeybind(%this) {
  GlassModManagerGui_KeybindText.setText("<font:verdana:16><just:center><color:111111>Press any key ...");
  GlassModManagerGui_KeybindOverlay.setVisible(true);
  %remapper = new GuiInputCtrl(GlassModManager_Remapper);
  GlassOverlayGui.add(%remapper);
  %remapper.makeFirstResponder(1);

  %bind = GlassSettings.get("Live::Keybind");
  GlobalActionMap.unbind(getField(%bind, 0), getField(%bind, 1));
  //swatch
}


//
// TODO possible name change or relocation
//

function GlassModManager_Remapper::onInputEvent(%this, %device, %key) {
  if(%device $= "mouse0") {
    return;
  }

  if(strlen(%key) == 1) {
    %badChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789[]\\/{};:'\"<>,./?!@#$%^&*-_=+`~";
    if(strpos(%badChar, strupr(%key)) >= 0) {
      GlassModManagerGui_KeybindText.setText("<font:Verdana Bold:15><just:center><color:111111>Invalid Character <font:verdana:16><br>Please try again");
      return;
    }
  } else {
    if(%key $= "ESCAPE") {
      GlassModManagerGui_KeybindOverlay.setVisible(false);

      %bind = GlassSettings.get("Live::Keybind");
      GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassLive_keybind");
      GlassModManager_Remapper.delete();
      return;
    }
  }

  GlassModManagerGui_KeybindOverlay.setVisible(false);

  %bind = GlassSettings.get("Live::Keybind");

  GlobalActionMap.unbind(getField(%bind, 0), getField(%bind, 1));
  GlobalActionMap.bind(%device, %key, "GlassLive_keybind");
  GlassModManager_Remapper.delete();
  GlassSettingsGui_Prefs_Keybind.setText("\c4" @ strupr(%key));
  GlassSettings.update("Live::Keybind", %device TAB %key);
}

//====================================
// Communications
//====================================

function GlassModManager::placeCall(%call, %params, %uniqueReturn) {
  if(GlassAuth.ident !$= "") {
    if(%params !$= "")
      for(%i = 0; %i < getLineCount(%params); %i++) {
        %line = getLine(%params, %i);
        %paramText = %paramText @ "&" @ urlenc(getField(%line, 0)) @ "=" @ urlenc(getField(%line, 1));
      }

    %parameters = "call=" @ %call @ %paramText;

    Glass::debug("Calling url: " @ %url);

    %className = "GlassModManagerTCP";

    %tcp = GlassApi.request("mm", %parameters, %className, true);
    %tcp.glass_call = %call;
    %tcp.glass_params = %params;
    %tcp.glass_uniqueReturn = %uniqueReturn;
    return %tcp;
  } else {
    GlassAuth.heartbeat();
    return false;
  }
}

function GlassModManagerTCP::handleText(%this, %line) {
  %this.buffer = %this.buffer NL %line;
}

function GlassModManagerTCP::onDone(%this, %error, %object) {
  if(!%error) {
    if(!%object) {
      Glass::debug(%this.buffer);
      GlassModManagerGui.openPage(GMM_ErrorPage, "jettisonError", $JSON::Error @ " : " @ $JSON::Index);
      return;
    }
    %ret = %object;

    if(%ret.action $= "auth") {
      GlassAuth.ident = "";
      GlassAuth.heartbeat();
      error("Call was killed due to auth heartbeat, need to resend");
    }

    if(%this.glass_uniqueReturn !$= "") {
      eval(%this.glass_uniqueReturn @ "(%ret);");
      return;
    } else {
      error("GlassModManager call handled in a poor way: " @ %ret.action);
    }

    if(%ret.status $= "success") {

    } else {
      GlassModManagerGui.openPage(GMM_ErrorPage, "status_" @ %ret.status, %this.buffer);
    }
	} else {
    GlassModManagerGui.openPage(GMM_ErrorPage, "tcpclient_" @ %error);
  }
}

//====================================
// RTB
//====================================

function GlassModManager::scanForRTB() {
  %pattern = "Add-ons/*/rtbInfo.txt";
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %path = filePath(%file);
    %name = getsubstr(%path, 8, strlen(%path)-8);

    if(strpos(%name, "_") == -1 || strpos(%name, "/") > -1) {
      warn("Skipping " @ %name @ ", invalid add-on name");
      continue;
    }

    if(isFile(%path @ "/glass.json")) {
      continue;
    }

    %fo = new FileObject();
    %fo.openForRead(%file);
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      if(getWord(%line, 0) $= "id:") {
        %id = getWord(%line, 1);
        GlassModManager.rtbAddon[%id] = %name;
        GlassModManager.isRTBAddon[%name] = true;
      }
    }
    %fo.close();
    %fo.delete();
  }
}

//====================================
// My Add-Ons
//====================================

function isDefaultAddOn(%file) {
  %list = "Add-Ons/System_BlocklandGlass/resources/default_addons.txt";

  if(!isFile(%list)) {
    error(%list SPC "not found.");
    return;
  }

  if(%file $= "")
    return;

  %fo = new FileObject();
  %fo.openForRead(%list);
  while(!%fo.isEOF()) {
    if(strlwr(%fo.readLine()) $= strlwr(%file)) {
      %fo.close();
      %fo.delete();
      return true;
    }
  }
  %fo.close();
  %fo.delete();
  return false;
}

//
// TODO this shouldn't include GUI functions, should be wrapped instead
//

function GlassModManager::deleteAddOn(%this, %addon) {
  %name = %addon.name;

  if(isObject(ServerConnection)) {
    error("Will not delete while on a server.");
    return;
  }

  if(isDefaultAddon(%name)) {
    error("Will not delete default add-ons.");
    return;
  }

  if((%name $= "System_BlocklandGlass") || (%name $= "Support_Preferences") || (%name $= "Support_Updater")) {
    error("Will not delete essential add-ons.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs"))
      error("Will not delete folders.");
    else
      error(%dir @ %name @ ".zip not found.");
    return;
  }

  fileDelete(%dir @ %name @ ".zip");

  glassMessageBoxOk("Add-On Deleted", "<font:verdana bold:13>" @ %name @ "<font:verdana:13> has been deleted.");

  GMM_MyAddonsPage.container.delete();
  GMM_MyAddonsPage.populateAddons();
  GlassModManagerGui.schedule(1, "openPage", GMM_MyAddonsPage);
}


//
// TODO possible name change or relocation
//

function GlassModManagerGui_AddonDelete::onMouseUp(%this) {
  %name = %this.addon.name;

  if(isObject(ServerConnection)) {
    glassMessageBoxOk("Delete Add-On", "Please discontinue playing on a server before trying to delete add-ons.");
    return;
  }

  if(isDefaultAddon(%name)) {
    glassMessageBoxOk("Delete Add-On", "Sorry, you may not delete default add-ons from the Glass Mod Manager.");
    return;
  }

  if((%name $= "System_BlocklandGlass") || (%name $= "Support_Preferences") || (%name $= "Support_Updater")) {
    glassMessageBoxOk("Delete Add-On", "Sorry, you may not delete essential add-ons required for Blockland Glass' operation from the Glass Mod Manager.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs")) {
      glassMessageBoxOk("Delete Add-On", "Sorry, you may not delete add-ons packaged as folders from the Glass Mod Manager.");
    }
    return;
  }

  glassMessageBoxYesNo("Delete Add-On", "Are you sure you want to delete <font:verdana bold:13>" @ %name, "GlassModManager.deleteAddon(\"" @ %this.addon @ "\");");
}


//
// TODO possible name change or relocation
//

function GlassModManagerGui_AddonRedirect::onMouseUp(%this) {
  GlassModManagerGui.loadContext("boards");
  GlassModManagerGui.openPage(GMM_AddonPage, %this.addon.glassdata.id);
}

//====================================
// Colorsets
//====================================

//
// TODO this function should still be used to delete a colorset, but a wrapper
// should be made around it to do gui updates
//

function GlassModManager::deleteColorset(%this, %colorset) {

  %name = %colorset.name;

  if(isDefaultAddon(%name)) {
    error("Will not delete default colorsets.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs")) {
      error("Will not delete folders.");
    } else {
      error(%dir @ %name @ ".zip not found.");
    }
    return;
  }

  if(GlassSettings.get("MM::Colorset") $= ("Add-Ons/" @ %name @ "/colorSet.txt")) {
    GlassSettings.update("MM::Colorset", "Add-Ons/Colorset_Default/colorSet.txt");
  }

  fileDelete(%dir @ %name @ ".zip");
}


//
// TODO possible name change or relocation
//

function GlassModManager_ColorsetDelete::onMouseUp(%this) {
  %name = %this.colorset.name;

  if(isDefaultAddon(%name)) {
    glassMessageBoxOk("Delete Colorset", "Sorry, you may not delete the default colorset from the Glass Mod Manager.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs")) {
      glassMessageBoxOk("Delete Colorset", "Sorry, you may not delete colorsets packaged as folders from the Glass Mod Manager.");
    }
    return;
  }

  glassMessageBoxYesNo("Delete Colorset", "Are you sure you want to delete <font:verdana bold:13>" @ %name, "GlassModManager.deleteColorset(\"" @ %this.colorset @ "\");");
}

//
// TODO possible name change or relocation
//

function GlassModManager_ColorsetRedirect::onMouseUp(%this) {
  GlassModManagerGui.loadContext("boards");
  GlassModManagerGui.openPage(GMM_AddonPage, %this.colorset.glassdata.id);
}

//filecopy doesnt like zips
function filecopy_hack(%source, %destination) {
  %fo_source = new FileObject();
  %fo_dest = new FileObject();
  %fo_source.openForRead(%source);
  %fo_dest.openForWrite(%destination);
  while(!%fo_source.isEOF()) {
    %fo_dest.writeLine(%fo_source.readLine());
  }
  %fo_source.close();
  %fo_dest.close();
  %fo_source.delete();
  %fo_dest.delete();
}

//====================================
// Downloading
//====================================

// id - addon id
// beta - bool
// progressBar - additional progress bar to update other than mod manager
function GlassModManager::downloadAddon(%this, %id, %progressBar, %progressText) {
  error("Deprecated GlassModManager::downloadAddon - use GlassDownloadManager directly");
  if(GlassModManager.getId() != %this.getId()) {
    error("Legacy download addon");
    return;
  }

  if(!isObject(GlassModManagerQueue)) {
    new ScriptGroup(GlassModManagerQueue);
  }

  %dl = GlassDownloadManager::newDownload(%id, 1);
  %dl.progressBar = %progressBar;
  %dl.progressText = %progressText;

  %dl.addHandle("done", "GlassModManagerQueue_Done");
  %dl.addHandle("progress", "GlassModManagerQueue_Progress");
  %dl.addHandle("failed", "GlassModManagerQueue_Failed");

  GlassModManagerQueue.add(%dl);
  GlassModManagerQueue.next();
}


//
// TODO possible name change or relocation
// is this needed if GlassDownloadManager is localized?
//

function GlassModManagerQueue::next(%this) {
  if(isObject(%this.current) && %this.isMember(%this.current))
    return; //downloading

  if(%this.getCount() == 0) {
    GlassModManagerGui::setProgress(1, "All Downloads Finished");
  	GlassModManagerGui.progressSch = GlassModManagerGui.schedule(2000, setProgress);
    CustomGameGui.populateAddOnList();
    return;
  }

  %this.current = %this.getObject(0);
  %this.current.startDownload();

  //GlassModManagerGui::setProgress(0, "Connecting..." @ " (" @ GlassModManagerQueue.getCount() @ " remaining)");
}

function GlassModManagerQueue_Done(%this) {
  GlassLog::log("Downloaded " @ %this.filename);

  GlassModManagerQueue.remove(%this);
  GlassModManagerQueue.next();
}

function GlassModManagerQueue_Progress(%this, %float, %tcp) {
  cancel(GlassModManagerGui.progressSch);

  %fileSize = %tcp.headerField["Content-Length"];

  %this.progressBar.setValue(%float);
  %this.progressText.setText("Downloaded " @ stringifyFileSize(%float*%fileSize, 2));
}

function GlassModManagerQueue_Failed(%this, %error) {
  error("Failed to download add-on " @ %this.addonId);

  //clean-up gui

  GlassModManagerQueue.remove(%this);
  GlassModManagerQueue.next();
}


//
// TODO possible name change or relocation
//

function stringifyFileSize(%size, %dec) {
  %osize = %size;
  %num = " KMG";
  %base = 0;
  while(%size >= 1024) {
    %size /= 1024;
    %base++;
  }

  %letter = trim(getSubStr(%num, %base, 1));

  return mFloatLength(%size, %dec) @ " " @ %letter @ "b";
}

//====================================
// RTB Import
//====================================


function GlassModManager::checkImports(%this) {
  // this is poorly structured
  // and will probably be replaced in glass 5
  GMM_ImportPage._unopened = true;
  GlassModManager::placeCall("rtb", "", "GMM_ImportPage.handleResults");
}


//====================================
// Package
//====================================

package GlassModManager {
  // TODO this needs to be cleaned up
  function GuiMLTextCtrl::onURL(%this, %url) {
  	// User links
  	if(getSubStr(%url, 0, 17) $= "gamelink_glass://") {
      %blid = getSubStr(%url, 22, strLen(%url) - 1);
      if(isObject(%window = GlassLiveUser::getFromBlid(%blid).window))
        %window.delete();
      else
        GlassLive::openUserWindow(%blid);
      return;
  	}

	// Mod Manager links
    if(strpos(%url, "glass://") != -1) {
      //%url = stripChars(%url, "[]\\{};'\"<>,.@#%^*+`~");
      %link = getsubstr(%url, 8, strlen(%url)-8);

      %idx = strPos(%link, "=");
      if(%idx < 0) {
        %idx = strlen(%link);
      }
      %type = getSubStr(%link, 0, %idx);

      switch$(%type) {
        case "board":
          if(strpos(%link, "&page=") != -1) {
            %board = getsubstr(%link, 6, strpos(%link, "&")-6);
            %page = getsubstr(%link, 12+strlen(%board), strlen(%link)-12-strlen(%board));
            GlassModManagerGui.openPage(GMM_BoardPage, %board, %page);
            return;
          }

        case "rtbBoard":
          if(strpos(%link, "&page=") != -1) {
            %board = getsubstr(%link, 9, strpos(%link, "&")-9);
            %board = strreplace(%board, "_", " ");
            %page = getsubstr(%link, 15+strlen(%board), strlen(%link)-15-strlen(%board));

            GlassModManagerGui.openPage(GMM_RTBBoardPage, %board, %page);
            return;
          }

        case "invite":
          %addr = getSubStr(%link, 7, strlen(%link));
          GlassLive::inviteClick(%addr);
          return;
      }
      if(strpos(%link, "aid-") != -1) {
        %id = getsubstr(%link, 4, strlen(%link)-4);
      }
    } else if(strpos(%url, "blocklandglass.com/addons/addon.php?id=") != -1) {
      %id = getsubstr(%url, strpos(%url, "=") + 1, strlen(%url));
    } else {
      return parent::onURL(%this, %url);
    }

    if(GlassModManagerGui.getCount() > 0) {
      GlassModManagerGui.firstWake = true;
      GlassOverlayGui.add(GlassModManagerGui_Window);
      GlassModManagerGui_Window.forceCenter();
      GlassModManagerGui_Window.visible = true;
    } else {
      GlassModManagerGui_Window.setVisible(true);
    }

    GlassOverlay::open();

    GlassOverlayGui.pushToBack(GlassModManagerGui_Window);

    if(%id+0 $= %id || %id > 0) {
      GMM_Navigation.clear();

      GlassModManagerGui.openPage(GMM_AddonPage, %id);
    }

    if((%board+0 $= %board || %board > 0) || (%page+0 $= %page || %page > 0)) {
      GMM_Navigation.clear();

      GlassModManagerGui.loadContext("boards");
      GlassModManagerGui.openPage(GMM_BoardPage, %board, %page);
    }

    if(%link $= "activity") {
      GlassModManagerGui.loadContext("activity");
    } else if(%link $= "boards") {
      GlassModManagerGui.loadContext("boards");
    } else if(%link $= "search") {
      GlassModManagerGui.loadContext("search");
    }
  }

  function newChatHud_AddLine(%line) {
    for(%i = 0; %i < getWordCount(%line); %i++) {
      %word = getWord(%line, %i);
      if(strpos(%word, "glass://") != -1) {
        %line = setWord(%line, %i, "<sPush><a:" @ %word @ ">" @ %word @ "</a><sPop>");
      } else if(getsubStr(%word, 0, 14) $= "<bitmap:glass-") {
      	%line = setWord(%line, %i, "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/" @ getSubStr(%word, 14, strLen(%word)));
      }
    }
   return parent::newChatHud_AddLine(%line);
  }

  function GlassAuth::onAuthSuccess(%this) {
    if(!%this.firstAuth)
      GlassModManager.checkImports();

    parent::onAuthSuccess(%this);
  }
};
activatePackage(GlassModManager);
