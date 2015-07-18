//================================================================
//=	Title: 	Blockland Glass (i3)								 =
//=	Author:	Jincux (9789)										 =
//=	If you're looking at this, go you. either that, or you're a  =
//=	little skiddy trying to 'troll'								 =
//================================================================
// DISCLAIMER: Through majority of production, I was drunk


//Object-based structure, for data's sake
function BLG::init() {

	%fo = new FileObject();
	%fo.openForRead("Add-ons/System_BlocklandGlass/version.txt");
	while(!%fo.isEOF()) {
		%line = %fo.readLine();
		if(getField(%line, 0) $= "version") {
			%version = getField(%line, 1);
		}
		break;
	}
	%fo.close();

	new ScriptObject(BLG) {
		version = %version;
		//address = "192.168.1.2";
		//netAddress = "192.168.1.2";
		address = "api.blocklandglass.com";
		netAddress = "blocklandglass.com";

		enableCLI = true;
	};
}

function BLG::fuckBitches() {
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

	echo(" ===                   Sticking it in                   ===");
	BLG_Con.pollServer();
	GlassAuth::init();
	GlassDownloadManager::init();
	GlassRTBSupport::init();

	GlassModManager::init();

	echo(" ===            Drunkenly staggering forward            ===");

	//tests
}

BLG::fuckBitches();
