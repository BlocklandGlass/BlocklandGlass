//================================================================
//=	Title: 	Blockland Glass (i3)								 =
//=	Author:	Jincux (9789)										 =
//=	If you're looking at this, go you. either that, or you're a  =
//=	little skiddy trying to 'troll'								 =
//================================================================
// DISCLAIMER: Through majority of production, I was drunk


//Object-based structure, for data's sake
function BLG::init() {
	new ScriptObject(BLG) {
		version = "nightly.2015.07.20";
		//address = "192.168.1.2";
		//netAddress = "192.168.1.2";
		address = "api.blocklandglass.com";
		netAddress = "blocklandglass.com";

		enableCLI = true;
	};

	//GlobalActionMap.bind("keyboard", ,"RTB_toggleOverlay");
}

function BLG::exec() {
	BLG::init();
	echo(" === Blockland Glass v" @ BLG.version @ " suiting up. ===");
	exec("./support/Support_TCPClient.cs");
	exec("./support/Support_UpdaterMigration.cs");

	echo(" ===                 Loading Interface                  ===");
	exec("./client/gui/profiles.cs");
	exec("./BLG_VerifyAccount.gui"); //need to move
	exec("./GlassModManagerGui.gui"); //need to move
	exec("./GlassModManagerImage.gui"); //need to move

	echo(" ===              Executing Important Stuff             ===");
	exec("./auth.cs");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassRTBSupport.cs");


	exec("./client/GlassAuth.cs");

	exec("./client/GlassModManager.cs");

	echo(" ===                   Starting it up                   ===");
	BLG_Con.pollServer();
	GlassAuth::init();
	GlassDownloadManager::init();
	GlassRTBSupport::init();

	GlassModManager::init();

	echo(" ===            Drunkenly staggering forward            ===");

	exec("config/BLG/client/mm.cs");

	//tests
	if($BLG::MM::Colorset $= "") {
		$BLG::MM::Colorset = "Add-Ons/System_BlocklandGlass/colorset_default.txt";
	}

	if($BLG::MM::Keybind $= "") {
		$BLG::MM::Keybind = "keyboard\tctrl m";
	}

	%bind = $BLG::MM::Keybind;
	GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassModManager_keybind");
}

BLG::exec();

package GlassPrefs {
	function onExit() {
		export("$BLG::MM::*", "config/BLG/client/mm.cs");
		parent::onExit();
	}
};
activatePackage(GlassPrefs);
