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
	exec("./support/Support_MetaTCP.cs");
	exec("./support/Support_Markdown.cs");
	exec("./support/DynamicGui.cs");

	echo(" ===                 Loading Interface                  ===");
	exec("./client/gui/profiles.cs");
	exec("./client/gui/GlassDownloadGui.gui");
	exec("./client/gui/GlassVerifyAccountGui.gui"); //need to rename/move
	exec("./client/gui/GlassModManagerGui.gui");
	exec("./client/gui/GlassModManagerImage.gui");
	exec("./client/gui/GlassServerControlGui.gui");
	exec("./client/gui/GlassChatroomGui.gui");

	echo("");
	exec("./client/GlassInstallWizard.cs");
	if(!GlassInstallWizard::hasRun()) {
		echo(" ===              Launching Install Wizard              ===");
		GlassInstallWizard::run();
		return;
	}

	echo(" ===              Executing Important Stuff             ===");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassRTBSupport.cs");
	exec("./common/GlassUpdaterSupport.cs");
	exec("./common/GlassResourceManager.cs");
	exec("./common/GlassStatistics.cs");

	exec("./client/GlassDownloadInterface.cs");

	exec("./client/GlassClientManager.cs");

	exec("./client/GlassAuth.cs");
	exec("./client/GlassLive.cs");
	exec("./client/GlassModManager.cs");
	exec("./client/GlassPreferencesBridge.cs");
	exec("./client/GlassServerControl.cs");
	exec("./client/GlassNotificationManager.cs");

	echo(" ===                   Starting it up                   ===");

	GlassDownloadInterface::init();
	GlassAuth::init();
	GlassLive::init();
	GlassDownloadManager::init();
	//GlassRTBSupport::init();
	GlassServerControlC::init();
	GlassClientManager::init();
	GlassNotificationManager::init();

	GlassModManager::init();

  GlassModManagerGui_Prefs_Keybind.setText("\c4" @ strupr(getField(GlassSettings.get("MM::Keybind"), 1)));

	%bind = GlassSettings.get("MM::Keybind");
	GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassModManager_keybind");

	%bind = GlassSettings.get("Live::Keybind");
	GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassLive_keybind");
}

function clientCmdGlassHandshake(%ver) {
  ServerConnection.hasGlass = true;
  ServerConnection._glassVersion = %ver;
	commandToServer('GlassHandshake', Glass.version);
}

Glass::init("client");

function strcap(%str) {
	return strupr(getsubstr(%str, 0, 1)) @ strlwr(getsubstr(%str, 1, strlen(%str)-1));
}

package GlassPrefs {
	function onExit() {
		parent::onExit();
	}

	function MM_AuthBar::blinkSuccess(%this) {
		parent::blinkSuccess(%this);
	}
};
activatePackage(GlassPrefs);
