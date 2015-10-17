echo("Forcing Blockland Glass!");
exec("config/server/ADD_ON_LIST.cs");
$AddOn__System_BlocklandGlass = 1;
export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

echo("\nLoading Preferences");
exec("./server/GlassPreferences.cs");
GlassPreferences::loadPrefs(false);

//It appears that Blockland disables all packages before executing add-ons
//Clearly that is a problem when it comes to preloading

GlassPreferences::registerPref("System_BlocklandGlass", "Cool kid?", "bool", "", false);
GlassPreferences::registerPref("System_BlocklandGlass", "How cool?", "slider", "0 9000", 6);
GlassPreferences::registerPref("System_BlocklandGlass", "Multiplying factor", "int", "0 15", 1);
GlassPreferences::registerPref("System_BlocklandGlass", "Cool kid club?", "text", "100", "kkk");
GlassPreferences::registerPref("System_BlocklandGlass", "Big space bruh", "textarea", "1000", "well look at that<br>nige");

package GlassPreload {
  function deactivateServerPackages() {
		parent::deactivateServerPackages();
		activatePackage(GlassPreferences);
	}
};
activatePackage(GlassPreload);
