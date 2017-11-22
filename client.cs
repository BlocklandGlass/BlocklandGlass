if(!isUnlocked()) {
  error("Demo mode is not supported, please buy the game.");
  return;
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

if($Pref::PreLoadScriptLauncherVersion < 2) {
	echo("Installing pre-loader.");
	fileCopy_hack("Add-Ons/System_BlocklandGlass/support/preloader.cs", "config/main.cs");
  $PreLoaderInstalled = true;
}

exec("./core.cs");

function Glass::execClient() {
  echo(" ===  Blockland Glass v" @ Glass.version @ " starting.  ===");
  exec("./client/GlassSettings.cs");

  exec("./support/jettison.cs");
  exec("./support/Support_TCPClient.cs");
  exec("./support/Support_MetaTCP.cs");
  exec("./support/Support_Markdown.cs");
  exec("./support/Support_SemVer.cs");
  exec("./support/Support_DigestAccessAuthentication.cs");
  exec("./support/DynamicGui.cs");

  echo(" ===                 Loading Interface                  ===");
  exec("./client/gui/profiles.cs");
  exec("./client/gui/messageboxes.cs");
  exec("./client/gui/GlassDownloadGui.gui");
  exec("./client/gui/GlassVerifyAccountGui.gui");
  exec("./client/gui/GlassModManagerGui.gui");
  exec("./client/gui/GlassModManagerImage.gui");
  exec("./client/gui/GlassModManagerGroupGui.gui");
  exec("./client/gui/GlassServerControlGui.gui");
  exec("./client/gui/GlassChatroomGui.gui");
  exec("./client/gui/GlassClientGui.gui");
  exec("./client/gui/GlassBanGui.gui");
  exec("./client/gui/GlassServerPreviewGui.gui");
  exec("./client/gui/GlassJoinServerGui.gui");
  exec("./client/gui/GlassManualGui.gui");
  exec("./client/gui/GlassModeratorGui.gui");
  exec("./client/gui/GlassIconSelectorGui.gui");
  exec("./client/gui/GlassOverlayGui.gui");
  exec("./client/gui/GlassSettingsGui.gui");
  exec("./client/gui/GlassBugReportGui.gui");
  exec("./client/gui/GlassPasswordGui.gui");
  exec("./client/gui/elements/GlassHighlightSwatch.cs");

  GlassSettings::init();
  exec("./runonce/settingConversion.cs");

  echo(" ===              Executing Important Stuff             ===");
  exec("./common/GlassFileData.cs");
  exec("./common/GlassDownloadManager.cs");
  exec("./common/GlassResourceManager.cs");
  exec("./common/GlassStatistics.cs");
  exec("./common/GlassApi.cs");

  exec("./client/GlassAudio.cs");

  exec("./client/GlassDownloadInterface.cs");
  exec("./client/GlassUpdaterSupport.cs");

  exec("./client/GlassClientManager.cs");

  exec("./client/GlassAuth.cs");
  exec("./client/GlassOverlay.cs");
  exec("./client/GlassLive.cs");
  exec("./client/GlassModManager.cs");

  exec("./client/GlassPreferencesBridge.cs");
  exec("./client/GlassServerControl.cs");
  exec("./client/GlassGraphs.cs");

  exec("./client/GlassNotificationManager.cs");
  exec("./client/GlassServers.cs");
  exec("./client/GlassManual.cs");

  exec("./client/GlassCompatibility.cs");
  exec("./client/GlassBugReport.cs");

  %date = getDateTime();
  %month = getSubStr(%date, 0, 2);
  %day = getSubStr(%date, strpos(%date, "/")+1, 2);
  if(%month == 12 && %day >= 21 && !$Pref::Client::NoSnowflakes) {
    exec("./client/GlassSnowflakes.cs");
  	GlassSnowflakes::doSnow(GlassOverlay, mFloor(getWord(getRes(), 0)/150));
  }

  echo(" ===                   Starting it up                   ===");

  //GlassResourceManager::execResource("Support_Preferences", "client");
  GlassResourceManager::execResource("Support_Updater", "client");
  GlassResourceManager::loadPreferences("client");

  GlassApi::init();
  GlassDownloadInterface::init();
  GlassAuth::init();
  GlassAudio::init();
  // move this somewhere else:
  GlassAudio::add("bell", false);
  GlassAudio::add("friendRequest", false);
  GlassAudio::add("friendRemoved", false);
  GlassAudio::add("friendInvite", false);
  GlassAudio::add("chatroomMsg1", true);
  GlassAudio::add("chatroomMsg2", true);
  GlassAudio::add("friendOnline", true);
  GlassAudio::add("friendOffline", true);
  GlassAudio::add("userMsgSent", true);
  GlassAudio::add("userMsgReceived", true);
  //
  GlassLive::init();
  GlassDownloadManager::init();
  GlassServerControlC::init();
  GlassClientManager::init();
  GlassNotificationManager::init();

  GlassModManager::init();
  GlassServers::init();
  GlassManual::init();

  GlassGraphs::init();

  GlassSettingsGui_Prefs_Keybind.setText("\c4" @ strupr(getField(GlassSettings.get("Live::Keybind"), 1)));

  Glass.useWindowTheme(!GlassSettings.get("Glass::UseDefaultWindows"));

  %bind = GlassSettings.get("MM::Keybind");
  if(%bind !$= "") {
  	GlassSettings.update("Live::Keybind", %bind);
  	GlassSettings.update("MM::Keybind", "");
  }

  %bind = GlassSettings.get("Live::Keybind");
  GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassLive_keybind");

  $ServerSettingsGui::UseBLG = GlassSettings.get("Server::UseBLG");
  Glass::useBLG();
}

function Glass::useBLG() {
  GlassSettings.update("Server::UseBLG", $ServerSettingsGui::UseBLG);
  $AddOn__System_BlocklandGlass = ($ServerSettingsGui::UseBLG ? 1 : -1);
}

function clientCmdGlassHandshake(%ver) {
  if(%ver $= "")
    return;

  if(ServerConnection.hasGlass $= "") {
    GlassLog::log("\c4Glass Handshake Received...");

    %ver = expandEscape(stripMlControlChars(%ver));

    GlassLog::log("\c4Glass Server: " @ %ver @ " | " @ "Glass Client: " @ Glass.version);

    %semver = semanticVersionCompare(%ver, Glass.version);

    switch(%semver) {
      case 0:
        // echo("\c4Glass version matched.");
      case 1:
        GlassLog::log("\c2Glass Client is out of date.");
      case 2:
        GlassLog::log("\c2Glass Server is out of date.");
    }

    ServerConnection.hasGlass = true;
    ServerConnection._glassVersion = %ver;

    commandToServer('GlassHandshake', Glass.version);
  }
}

function Glass::updateWindowSetting() {
	Glass.useWindowTheme(!GlassSettings.get("Glass::UseDefaultWindows"));
}

function Glass::useWindowTheme(%this, %bool) {
	%this.useWindowTheme = %bool;

	for(%i = 0; %i < getFieldCount(%this.windows); %i++) {
		%window = getField(%this.windows, %i);
		if(!isObject(%window))
			continue;

		if(%bool) {
			%window.setProfile(GlassWindowProfile);
		} else {
			%window.setProfile(BlockWindowProfile);
		}
	}

	for(%i = 0; %i < canvas.getCount(); %i++) {
		%o = canvas.getObject(%i);
		canvas.schedule(0, popDialog, %o);
		canvas.schedule(1, pushDialog, %o);
	}
}

function Glass::writeCrashLock() {
  Glass::setCrashable(false);
}

function Glass::detectCrash() {
  if(isFile("config/client/blg/game.lock")) {
    %fo = new FileObject();
    %fo.openForRead("config/client/blg/game.lock");
    %bool = %fo.readLine();
    %fo.close();
    %fo.delete();

    if(%bool $= "1")
      Glass.wasCrash = true;
    else
      Glass.wasCrash = false;
  } else {
    Glass.wasCrash = false;
  }
}

function Glass::setCrashable(%bool) {
    %fo = new FileObject();
    %fo.openForWrite("config/client/blg/game.lock");
    %fo.writeLine(%bool ? "1" : "0");
    %fo.close();
    %fo.delete();
}


if(!isObject(Glass))
	Glass::init("client");

Glass::detectCrash();
Glass::writeCrashLock();

package GlassClientPackage {
  function Canvas::setContent(%this, %content) {
    parent::setContent(%this, %content);

    if(!isObject(MM_GlassVersion)) {
      %mm = new GuiTextCtrl(MM_GlassVersion) {
        profile = "BlockWindowProfile";
        horizSizing = "relative";
        vertSizing = "relative";
        position = "5 75";
        extent = "165 20";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        text = (Glass.dev ? "\c4" : "") @ "Glass v" @ Glass.version;
        maxLength = "255";
      };

      MainMenuButtonsGui.add(%mm);
    }
  }

  function onExit() {
    fileDelete("config/client/blg/game.lock");
    parent::onExit();
  }

  function MM_AuthBar::blinkSuccess(%this) {
    parent::blinkSuccess(%this);
    if(Glass.wasCrash) {
      schedule(50, 0, glassMessageBoxYesNo, "Crash Report", "<font:verdana bold:13>Crash Report<font:verdana:13><br><br>We detected a crash while you were using Glass! We can only fix these issues if we know about them. Would you like to submit a bug report?", "GlassBugReport::crashPromptYes();");
      Glass.wasCrash = false;
    }

    GlassLog::cleanOld("blockland");
  }
};
activatePackage(GlassClientPackage);
