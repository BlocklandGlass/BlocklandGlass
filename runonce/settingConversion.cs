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
  if(GlassSettings.cacheFetch("SettingsReset") !$= "3.2.0") {
    warn("Resetting settings!");
    GlassSettings.resetToDefaults("client");
    GlassSettings.cachePut("SettingsReset", "3.2.0");
  }
}
