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
		address = "localhost";
		netAddress = "localhost";
	};
}

function BLG::fuckBitches() {
	BLG::init();
	echo(" === Blockland Glass v" @ BLG.version @ " suiting up. ===");
	exec("./support/Support_TCPClient.cs");
	exec("./support/Support_UpdaterMigration.cs");

	echo(" ===                 Loading Interface                  ===");
	exec("./support/BLG_VerifyAccount.gui");

	echo(" ===              Executing Important Stuff             ===");
	exec("./auth.cs");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassRTBSupport.cs");

	echo(" ===                   Sticking it in                   ===");
	BLG_Con.pollServer();
	GlassDownloadManager::init();
	GlassRTBSupport::init();

	echo(" ===            Drunkenly staggering forward            ===");

	//tests
	%fileData = GlassFileData::create("TDM Server Pack", 15, 1, "Server_TDMServerPack.zip");
	GlassDownloadManager.fetchAddon(%fileData);
}

BLG::fuckBitches();