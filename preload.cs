exec("./common/GlassResourceManager.cs");
GlassResourceManager::execResource("Support_Updater", "preload");
//GlassResourceManager::execResource("Support_Preferences", "preload");
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

    //glass executes specially, disable default execution
    $AddOn__System_BlocklandGlass = -1;
    export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

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
