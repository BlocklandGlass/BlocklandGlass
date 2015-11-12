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

function Glass::exec() {

	echo(" ===                Loading Preferences                 ===");
	exec("./common/GlassSettings.cs");
	GlassSettings::init("client");

	echo(" ===  Blockland Glass v" @ Glass.version @ " suiting up.  ===");
	exec("./support/jettison.cs");
	exec("./support/Support_TCPClient.cs");
	exec("./support/Support_Markdown.cs");

	echo(" ===                 Loading Interface                  ===");
	exec("./client/gui/profiles.cs");
	exec("./client/gui/GlassUpdatesGui.gui");
	exec("./BLG_VerifyAccount.gui"); //need to rename/move
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

	exec("./client/GlassAuth.cs");
	exec("./client/GlassModManager.cs");
	exec("./client/GlassPreferencesBridge.cs");
	exec("./client/GlassServerControl.cs");

	echo(" ===                   Starting it up                   ===");
	GlassFontManager::init();

	GlassAuth::init();
	GlassDownloadManager::init();
	GlassRTBSupport::init();
	GlassUpdaterSupport::verifyInstall();
	GlassServerControlC::init();

	GlassModManager::init();

	//tests
	if($BLG::MM::Colorset $= "") {
		$BLG::MM::Colorset = "Add-Ons/System_BlocklandGlass/colorset_default.txt";
	}

  GlassModManagerGui_Prefs_Keybind.setText("\c4" @ strupr(getField($BLG::MM::Keybind, 1)));

	%bind = $BLG::MM::Keybind;
	GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassModManager_keybind");

	exec("./feedback.cs");
}

function Glass::doWelcomeMessage() {
	if(!$BLG::MM::WelcomeMessage) messageBoxOk("Welcome to Blockland Glass", "<font:arial bold:20>Welcome to Blockland Glass<font:arial: 14><br><br>Thank you very much for downloading Blockland Glass!<br><br>To get started, press <font:arial bold:14>CTRL M<font:arial:14>!", "Glass::welcomeMessageSeen();");
}

function Glass::welcomeMessageSeen() {
	$BLG::MM::WelcomeMessage = true;
	export("$BLG::MM::*", "config/BLG/client/mm.cs");
}

function clientCmdGlassHandshake(%ver) {
  ServerConnection.hasGlass = true;
  ServerConnection._glassVersion = %ver;
	commandToServer('GlassHandshake', Glass.version);
}

Glass::init();

package GlassPrefs {
	function onExit() {
		export("$BLG::MM::*", "config/BLG/client/mm.cs");
		parent::onExit();
	}

	function MM_AuthBar::blinkSuccess(%this) {
		Glass::doWelcomeMessage();
		Glass::openFeedbackPrompt();
		GlassResourceManager.prompt();
		parent::blinkSuccess(%this);
	}
};
activatePackage(GlassPrefs);
