if(isFile("config/BLG/client/mm.cs")) {
  echo("Pre-1.1 settings found. Converting.");

  exec("config/BLG/client/mm.cs");
  GlassSettings.update("Live::Keybind", $BLG::Live::Keybind);
  GlassSettings.update("MM::UseDefault", $BLG::MM::UseUpdaterDefault);
  GlassSettings.update("MM::Colorset", $BLG::MM::Colorset);


  fileCopy("config/BLG/client/mm.cs", "config/BLG/client/mm.cs.old");
  fileDelete("config/BLG/client/mm.cs");
}

if(!$Server::Dedicated) {
  %reset = GlassSettings.cacheFetch("SettingsReset");

  if(semanticVersionCompare(%reset, "3.2.0") == 2) {
    warn("Resetting settings!");
    GlassSettings.resetToDefaults("client");
    GlassSettings.cachePut("SettingsReset", "3.2.0");
  }

  if(semanticVersionCompare(%reset, "4.0.1") == 2) {
    warn("Setting Live::ViewLocation to Anyone");
    GlassSettings.cachePut("SettingsReset", "4.0.1");
    GlassSettings.update("Live::ViewLocation", "Anyone");
  }
}
