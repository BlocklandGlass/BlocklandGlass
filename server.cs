if(!isUnlocked()) {
  error("Demo mode is not supported, please buy the game.");
  return;
}

//filecopy doesnt like zips
function filecopy_hack(%source, %destination) {
  %fo_source = new FileObject();
  %fo_dest = new FileObject();
  %fo_source.openForRead(%source);
  %fo_dest.openForWrite(%destination);
  while(!%fo_source.isEOF()) {
    %fo_dest.writeLine(%fo_source.readLine());
  }
  %fo_source.close();
  %fo_dest.close();
  %fo_source.delete();
  %fo_dest.delete();
}

if($Pref::PreLoadScriptLauncherVersion < 1) {
  echo("Installed pre-loader.");
	fileCopy_hack("Add-Ons/System_BlocklandGlass/support/preloader.cs", "config/main.cs");
  $PreLoaderInstalled = true;
}

exec("./core.cs");

function Glass::execServer() {
  echo(" ===  Blockland Glass v" @ Glass.version @ " preparing for startup.  ===");
  exec("./support/Support_TCPClient.cs");
  exec("./support/Support_Markdown.cs");
  exec("./support/Support_SemVer.cs");
  exec("./support/jettison.cs");

  echo(" ===              Executing Important Stuff             ===");
  exec("./common/GlassResourceManager.cs");

  exec("./server/GlassAuth.cs");
  exec("./server/GlassServerControl.cs");
  exec("./server/GlassClientSupport.cs");
  exec("./server/GlassServerImage.cs");
  exec("./server/GlassServerGraphs.cs");

  echo(" ===                 Loading Resources                  ===");

  //GlassResourceManager::execResource("Support_Preferences", "server");
  GlassResourceManager::execResource("Support_Updater", "server");
  GlassResourceManager::loadPreferences("server");

  GlassAuthS::init();
  GlassServerGraphing::init();

  Glass.serverLoaded = true;
}

function Glass::serverCleanup() {
	GlassAuthS.delete();
  if(isObject(GlassClientSupport))
	 GlassClientSupport.delete();

	$Glass::Hook::RequiredClient = false;
	$Glass::Modules::Prefs = false;

	Glass.serverLoaded = false;
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
