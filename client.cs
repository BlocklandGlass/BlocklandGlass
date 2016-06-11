//================================================================
//=	Title: 	Blockland Glass (i3)								                 =
//=	Author:	Jincux (9789)								                     		 =
//=	If you're looking at this, go you. either that, or you're a  =
//=	little skiddy trying to 'troll'						              		 =
//================================================================

if($Pref::PreLoadScriptLauncherVersion != 1) {
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
	exec("./support/Support_Markdown.cs");
	exec("./support/DynamicGui.cs");

	echo(" ===                 Loading Interface                  ===");
	exec("./client/gui/profiles.cs");
	exec("./client/gui/GlassDownloadGui.gui");
	exec("./client/gui/GlassVerifyAccountGui.gui"); //need to rename/move
	exec("./client/gui/GlassModManagerGui.gui");
	exec("./client/gui/GlassModManagerImage.gui");
	exec("./client/gui/GlassServerControlGui.gui");

	echo(" ===              Executing Important Stuff             ===");
	exec("./client/GlassFontManager.cs");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassRTBSupport.cs");
	exec("./common/GlassUpdaterSupport.cs");
	exec("./common/GlassResourceManager.cs");
	exec("./common/GlassStatistics.cs");

	exec("./client/GlassDownloadInterface.cs");

	exec("./client/GlassClientManager.cs");

	exec("./client/GlassAuth.cs");
	exec("./client/GlassModManager.cs");
	exec("./client/GlassPreferencesBridge.cs");
	exec("./client/GlassServerControl.cs");
	exec("./client/GlassNotificationManager.cs");

	echo(" ===                   Starting it up                   ===");
	GlassFontManager::init();

	GlassDownloadInterface::init();
	GlassAuth::init();
	GlassDownloadManager::init();
	GlassRTBSupport::init();
	GlassUpdaterSupport::verifyInstall();
	GlassServerControlC::init();
	GlassClientManager::init();
	GlassNotificationManager::init();

	GlassModManager::init();
	GlassStatistics::reportMods();

  GlassModManagerGui_Prefs_Keybind.setText("\c4" @ strupr(getField(GlassSettings.get("MM::Keybind"), 1)));

	%bind = GlassSettings.get("MM::Keybind");
	echo(%bind);
	GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassModManager_keybind");

	exec("./feedback.cs");
}

function Glass::doWelcomeMessage() {
	if(!GlassSettings.cacheFetch("MM::WelcomeMessage")) messageBoxOk("Welcome to Blockland Glass", "<font:arial bold:20>Welcome to Blockland Glass<font:arial: 14><br><br>Thank you very much for downloading Blockland Glass!<br><br>To get started, press <font:arial bold:14>CTRL M<font:arial:14>!", "Glass::welcomeMessageSeen();");
}

function Glass::welcomeMessageSeen() {
	GlassSettings.cachePut("MM::WelcomeMessage", true);
}

function clientCmdGlassHandshake(%ver) {
  ServerConnection.hasGlass = true;
  ServerConnection._glassVersion = %ver;
	commandToServer('GlassHandshake', Glass.version);
}

Glass::init("client");

package GlassPrefs {
	function onExit() {
		parent::onExit();
	}

	function MM_AuthBar::blinkSuccess(%this) {
		Glass::doWelcomeMessage();
		Glass::openFeedbackPrompt();
		GlassResourceManager.prompt();
		GlassNotificationManager::newNotification("Mod Manager", "Press <color:ff3333>" @ strupr(getField(GlassSettings.get("MM::Keybind"), 1)) @ "<color:000000> to open the mod manager!", "module", 1, "canvas.pushDialog(GlassModManagerGui);");
		parent::blinkSuccess(%this);
	}
};
activatePackage(GlassPrefs);
