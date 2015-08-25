echo("Forcing Blockland Glass!");
exec("config/server/ADD_ON_LIST.cs");
$AddOn__System_BlocklandGlass = 1;
export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

echo("\nLoading Preferences");
exec("./server/GlassPreferences.cs");

//It appears that Blockland disables all packages before executing add-ons
//Clearly that is a problem when it comes to preloading

%fo = new FileObject();
%fo.openForAppend("config/server/ADD_ON_LIST.cs");
%fo.writeLine("if(isPackage(GlassPreferences)) { activatePackage(GlassPreferences); }");
%fo.close();
%fo.delete();


GlassPreferences::registerPref("System_BlocklandGlass", "Cool kid?", "bool", "", false);
GlassPreferences::registerPref("System_BlocklandGlass", "How cool?", "slider", "0 9000", 6);
GlassPreferences::registerPref("System_BlocklandGlass", "Multiplying factor", "int", "0 15", 1);
GlassPreferences::registerPref("System_BlocklandGlass", "Cool kid club?", "text", "100", "kkk");
