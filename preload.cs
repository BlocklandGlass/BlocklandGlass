exec("./common/GlassResourceManager.cs");
GlassResourceManager::execResource("Support_Updater", "preload");
GlassResourceManager::execResource("Support_Preferences", "preload");

if(!$Server::Dedicated && !$ClientLoaded) {
  if($AddOn__System_BlocklandGlass == 1) {
    $ServerSettingsGui::UseBLG = 1;
  } else if($AddOn__System_BlocklandGlass == -1) {
    $ServerSettingsGui::UseBLG = 0;
  }

  $ClientLoaded = true;
}

package GlassPreload {
  function deactivateServerPackages() {
		parent::deactivateServerPackages();

    if($ClientLoaded) {
      if(($ServerSettingsGui::UseBLG)) {
        $AddOn__System_BlocklandGlass = 1;

        export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

        if(isPackage(GlassServerControlS)) {
          activatePackage(GlassServerControlS);
        } else {
          exec("./server.cs");
        }
      } else {
        $AddOn__System_BlocklandGlass = -1;

        export("$AddOn__*", "config/server/ADD_ON_LIST.cs");
      }
    }
	}

  function GameConnection::BLP_isAllowedUse(%this) {
    if($AddOn__System_BlocklandGlass == -1) {
      return 0;
    }

    parent::BLP_isAllowedUse(%this);
  }
};
activatePackage(GlassPreload);
