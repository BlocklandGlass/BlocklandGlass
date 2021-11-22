//Object and group container
if(!isObject(GlassGroup))
  new ScriptGroup(GlassGroup);

exec("./common/GlassLog.cs");
GlassLog::init();
GlassLog::startSessionLog();

exec("./common/GlassResourceManager.cs");
GlassResourceManager::execResource("Support_Updater", "preload");
GlassResourceManager::loadPreferences("preload");


if(!$Server::Dedicated && !$ClientLoaded) {
  if($AddOn__System_BlocklandGlass == 1) {
    $ServerSettingsGui::UseBLG = 1;
  } else if($AddOn__System_BlocklandGlass == -1) {
    $ServerSettingsGui::UseBLG = 0;
  }

  $ClientLoaded = true;
}

package GlassPreload {
  //all packages are disabled post-preload and pre-normal load
  //we need to re-enable any packages
  //
  //we can also use this to make sure Glass loads early
  function deactivateServerPackages() {
		parent::deactivateServerPackages();
    activatePackage(GlassPreload);

    if(Glass.serverLoaded) {
      //this means the server is actually shutting down
      Glass::serverCleanup();
      return;
    }

    if(!$Server::Dedicated) {
      //obey settings
      if($ServerSettingsGui::UseBLG) {
        exec("./server.cs");
      }
    } else {
      //execute
      exec("./server.cs");
    }
	}
};
activatePackage(GlassPreload);
