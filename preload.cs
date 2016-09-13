echo("Forcing Blockland Glass!");
exec("config/server/ADD_ON_LIST.cs");
$AddOn__System_BlocklandGlass = 1;
export("$AddOn__*", "config/server/ADD_ON_LIST.cs");


exec("./common/GlassResourceManager.cs");
GlassResourceManager::execResource("Support_Updater", "preload");
GlassResourceManager::execResource("Support_Preferences", "preload");
//It appears that Blockland disables all packages before executing add-ons
//Clearly that is a problem when it comes to preloading


package GlassPreload {
  function deactivateServerPackages() {
		parent::deactivateServerPackages();
		activatePackage(GlassServerControlS);
	}
};

if(!$Server::Dedicated && !$ClientLoaded) {
  $ClientLoaded = true;
}

if($ClientLoaded || $Server::Dedicated) {
  activatePackage(GlassPreload);
}