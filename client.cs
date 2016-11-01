//================================================================
//=	Title: 	Blockland Glass (i3)																 =
//=	Author:	Jincux (9789)																				 =
//=	If you're looking at this, go you. either that, or you're a	 =
//=	little skiddy trying to 'troll'															 =
//================================================================

if($Pref::PreLoadScriptLauncherVersion != 2) {
	echo("Installing pre-loader!");
	fileCopy("Add-Ons/System_BlocklandGlass/support/preloader.cs", "config/main.cs");
}

exec("./core.cs");

function Glass::execClient() {

	echo(" ===                Loading Preferences                 ===");
	exec("./common/GlassSettings.cs");
	GlassSettings::init("client");

  if(isFile("config/BLG/client/mm.cs")) {
    exec("./runonce/settingConversion.cs");
  }

	echo(" ===  Blockland Glass v" @ Glass.version @ " suiting up.  ===");
	exec("./support/jettison.cs");
	exec("./support/Support_TCPClient.cs");
	exec("./support/Support_MetaTCP.cs");
	exec("./support/Support_Markdown.cs");
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

	echo(" ===              Executing Important Stuff             ===");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassResourceManager.cs");
	exec("./common/GlassStatistics.cs");

	exec("./client/GlassDownloadInterface.cs");
	exec("./client/GlassUpdaterSupport.cs");

	exec("./client/GlassClientManager.cs");

	exec("./client/GlassAuth.cs");
	exec("./client/GlassLive.cs");
	exec("./client/GlassModManager.cs");
	exec("./client/GlassPreferencesBridge.cs");
	exec("./client/GlassServerControl.cs");
	exec("./client/GlassNotificationManager.cs");

  exec("./client/GlassCompatibility.cs");

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

  GlassModManagerGui_Prefs_Keybind.setText("\c4" @ strupr(getField(GlassSettings.get("Live::Keybind"), 1)));

	%bind = GlassSettings.get("MM::Keybind");
	if(%bind !$= "") {
		GlassSettings.update("Live::Keybind", %bind);
		GlassSettings.update("MM::Keybind", "");
	}

	%bind = GlassSettings.get("Live::Keybind");
	GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassLive_keybind");
}

function clientCmdGlassHandshake(%ver, %tries) {
  %server_ver = getsubstr(stripChars(%ver, "."), 0, 3);

  // if(%server_ver + 0 !$= %server_ver || %server_ver < 0)
    // return;

  %client_ver = getsubstr(stripChars(Glass.version, "."), 0, 3);

  if(ServerConnection.hasGlass $= "") {
    if(isObject(NewChatSO)) {
      echo("\c4Glass Handshake Received...");

      echo("\c4Glass Server: " @ %ver @ " | " @ "Glass Client: " @ Glass.version);

      if(%server_ver > %client_ver) {
        NewChatSO.schedule(500, addLine, "You are running \c6" @ Glass.version @ "\c0 of \c3Blockland Glass\c0, update to \c6" @ %ver);

        echo("\c2Glass Server -> Client version mismatch, this client's Blockland Glass installation is out of date!");
      } else if(%client_ver > %server_ver) {
        NewChatSO.schedule(500, addLine, "This server is running an outdated version of \c3Blockland Glass\c0, please inform the host.");

        echo("\c2Glass Client -> Server version mismatch, this server's Blockland Glass installation is out of date!");
      } else if(%client_ver == %server_ver) {
        echo("\c4Glass Server <-> Client version match.");
      }

      ServerConnection.hasGlass = true;
      ServerConnection._glassVersion = %ver;

      commandToServer('GlassHandshake', Glass.version);
    } else {
      if(%tries <= 5) {
        schedule(1000, 0, clientCmdGlassHandshake, %ver, %tries++);
      }
    }
  }
}

Glass::init("client");

function strcap(%str) {
	return strupr(getsubstr(%str, 0, 1)) @ strlwr(getsubstr(%str, 1, strlen(%str)-1));
}

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
        text = "Glass" SPC Glass.version;
        maxLength = "255";
      };

      MainMenuButtonsGui.add(%mm);
    }
  }

};
activatePackage(GlassMainMenu);

// package GlassPrefs {
	// function onExit() {
		// parent::onExit();
	// }

	// function MM_AuthBar::blinkSuccess(%this) {
		// parent::blinkSuccess(%this);
	// }
// };
// activatePackage(GlassPrefs);
