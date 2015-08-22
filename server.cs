//================================================================
//=	Title: 	Blockland Glass (i3)								                 =
//=	Author:	Jincux (9789)								                     		 =
//=	If you're looking at this, go you. either that, or you're a  =
//=	little skiddy trying to 'troll'						              		 =
//================================================================


//Object-based structure, for data's sake
function BLG::init() {
	new ScriptObject(BLG) {
		version = "1.1.0-alpha.0+nightly.8.22.15";
		address = "api.blocklandglass.com";
		netAddress = "blocklandglass.com";

		enableCLI = true;
	};
}

function BLG::exec() {
	BLG::init();
	echo(" === Blockland Glass v" @ BLG.version @ " suiting up. ===");
	exec("./support/Support_TCPClient.cs");
	exec("./support/Support_Markdown.cs");

	echo(" ===              Executing Important Stuff             ===");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassRTBSupport.cs");
	exec("./common/GlassUpdaterSupport.cs");

	exec("./server/GlassPreferences.cs");

	echo(" ===                   Starting it up                   ===");


}

function serverCmdGlassHandshake(%client, %ver) {
  %client.hasGlass = true;
  %client._glassVersion = %ver;
}

function BLG::reload() {
  discoverFile("*");

  exec("./server/GlassPreferences.cs");
}

BLG::exec();
