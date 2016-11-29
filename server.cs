//================================================================
//=	Title: 	Blockland Glass (i3)								                 =
//=	Author:	Jincux (9789)								                     		 =
//=	If you're looking at this, go you. either that, or you're a  =
//=	little skiddy trying to 'troll'						              		 =
//================================================================

if($Pref::PreLoadScriptLauncherVersion < 1) {
	fileCopy("Add-Ons/System_BlocklandGlass/support/preloader.cs", "config/main.cs");
}

exec("./core.cs");

function Glass::execServer() {
	echo(" ===                Loading Preferences                 ===");
	exec("./common/GlassSettings.cs");
	GlassSettings::init("server");

  if(isFile("config/BLG/client/mm.cs")) {
    exec("./runonce/settingConversion.cs");
  }

	echo(" ===  Blockland Glass v" @ Glass.version @ " suiting up.  ===");
	exec("./support/Support_TCPClient.cs");
	exec("./support/Support_Markdown.cs");
	// exec("./support/Support_SemVer.cs");
	exec("./support/jettison.cs");

	echo(" ===              Executing Important Stuff             ===");
	exec("./common/GlassFileData.cs");
	exec("./common/GlassDownloadManager.cs");
	exec("./common/GlassResourceManager.cs");

	exec("./server/GlassAuth.cs");
	exec("./server/GlassServerControl.cs");
	exec("./server/GlassClientSupport.cs");

	echo(" ===                   Starting it up                   ===");

	GlassResourceManager::execResource("Support_Preferences", "server");
	GlassResourceManager::execResource("Support_Updater", "server");

	GlassAuthS::init();
}

if($Server::Dedicated) {
	Glass::init("dedicated");
} else {
	Glass::init("server");
}

function serverCmdGlassHandshake(%client, %ver) {
  if(%ver $= "")
    return;

  if(%client.hasGlass $= "") {
    %ver = expandEscape(stripMlControlChars(%ver));

    %client.hasGlass = true;
    %client._glassVersion = %ver;
  }
}