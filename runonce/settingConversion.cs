if(isFile("config/BLG/client/mm.cs")) {
  GlassLog::log("Pre-1.1 settings found. Converting.");

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
    GlassLog::log("Resetting settings!");
    GlassSettings.resetToDefaults("client");
    GlassSettings.cachePut("SettingsReset", "3.2.0");
  }

  if(semanticVersionCompare(%reset, "4.3.1") == 2) {
    GlassLog::log("Setting Volume::RoomChat to 0");
    GlassSettings.cachePut("SettingsReset", "4.3.1");
    GlassSettings.update("Volume::RoomChat", 0);
    GlassSettingsGui_Prefs_RoomChat.setValue(GlassSettings.get("Volume::RoomChat"));
  }
}
