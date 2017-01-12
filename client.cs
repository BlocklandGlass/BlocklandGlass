if($Pref::PreLoadScriptLauncherVersion != 2) {
	echo("Installing pre-loader!");
	fileCopy("Add-Ons/System_BlocklandGlass/support/preloader.cs", "config/main.cs");
}

exec("./core.cs");

function Glass::execClient() {
  echo(" ===                Loading Preferences                 ===");
  exec("./common/GlassSettings.cs");

  exec("./runonce/settingConversion.cs");

  echo(" ===  Blockland Glass v" @ Glass.version @ " starting.  ===");
  exec("./support/jettison.cs");
  exec("./support/Support_TCPClient.cs");
  exec("./support/Support_MetaTCP.cs");
  exec("./support/Support_Markdown.cs");
  exec("./support/Support_SemVer.cs");
  exec("./support/DynamicGui.cs");

  echo(" ===                 Loading Interface                  ===");
  exec("./client/gui/profiles.cs");
  exec("./client/gui/messageboxes.cs");
  exec("./client/gui/GlassDownloadGui.gui");
  exec("./client/gui/GlassVerifyAccountGui.gui"); //need to rename/move
  exec("./client/gui/GlassModManagerGui.gui");
  exec("./client/gui/GlassModManagerImage.gui");
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
  exec("./client/gui/elements/GlassHighlightSwatch.cs");

  GlassSettings::init();

  echo(" ===              Executing Important Stuff             ===");
  exec("./common/GlassFileData.cs");
  exec("./common/GlassDownloadManager.cs");
  exec("./common/GlassResourceManager.cs");
  exec("./common/GlassStatistics.cs");

  exec("./client/GlassDownloadInterface.cs");
  exec("./client/GlassUpdaterSupport.cs");

  exec("./client/GlassClientManager.cs");

  exec("./client/GlassAuth.cs");
  exec("./client/GlassOverlay.cs");
  exec("./client/GlassLive.cs");
  exec("./client/GlassModManager.cs");
  exec("./client/GlassPreferencesBridge.cs");
  exec("./client/GlassServerControl.cs");
  exec("./client/GlassNotificationManager.cs");
  exec("./client/GlassServers.cs");
  exec("./client/GlassManual.cs");

  exec("./client/GlassCompatibility.cs");

  %date = getDateTime();
  %month = getSubStr(%date, 0, 2);
  %day = getSubStr(%date, strpos(%date, "/")+1, 2);
  if(%month == 12 && %day >= 21 && !$Pref::Client::NoSnowflakes) {
  exec("./client/GlassSnowflakes.cs");
  	GlassSnowflakes::doSnow(GlassOverlay, mFloor(getWord(getRes(), 0)/150));
  }

  echo(" ===                   Starting it up                   ===");

  GlassResourceManager::execResource("Support_Preferences", "client");
  GlassResourceManager::execResource("Support_Updater", "client");

  GlassDownloadInterface::init();
  GlassAuth::init();
  GlassLive::init();
  GlassDownloadManager::init();
  GlassServerControlC::init();
  GlassClientManager::init();
  GlassNotificationManager::init();

  GlassModManager::init();
  GlassServers::init();
  GlassManual::init();

  GlassSettingsGui_Prefs_Keybind.setText("\c4" @ strupr(getField(GlassSettings.get("Live::Keybind"), 1)));

  Glass.useWindowTheme(!GlassSettings.get("Glass::UseDefaultWindows"));

  %bind = GlassSettings.get("MM::Keybind");
  if(%bind !$= "") {
  	GlassSettings.update("Live::Keybind", %bind);
  	GlassSettings.update("MM::Keybind", "");
  }

  %bind = GlassSettings.get("Live::Keybind");
  GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassLive_keybind");
}

function clientCmdGlassHandshake(%ver) {
  if(%ver $= "")
    return;

  if(ServerConnection.hasGlass $= "") {
    echo("\c4Glass Handshake Received...");

    %ver = expandEscape(stripMlControlChars(%ver));

    echo("\c4Glass Server: " @ %ver @ " | " @ "Glass Client: " @ Glass.version);

    %semver = semanticVersionCompare(%ver, Glass.version);

    switch(%semver) {
      // case 0:
        // echo("\c4Glass version matched.");
      case 1:
        echo("\c2Glass Client is out of date.");
      case 2:
        echo("\c2Glass Server is out of date.");
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

function strcap(%str) {
	return strupr(getsubstr(%str, 0, 1)) @ strlwr(getsubstr(%str, 1, strlen(%str)-1));
}

if(!isObject(Glass))
	Glass::init("client");

package GlassMainMenu {
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
};
activatePackage(GlassMainMenu);
