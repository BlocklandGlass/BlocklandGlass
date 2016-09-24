function glassResetAllGuis() {
  GlassServerControlGui_Prefs_Categories.clear();
  GlassServerControl_PrefScroll.clear();
  GlassServerControlGui_PlayerList.clear();
  GlassServerControlGui_AdminList.clear();
  GlassServerControlGui_RequiredClientsPopUp.clear();
  GlassServerControlGui_RequiredClientsList.clear();

  if(isObject(GlassModManagerActivityList)) {
    GlassModManagerActivityList.clear();
  }

  GlassModManager_ActivityFeed.clear();
  GlassModManager_Boards.clear();
  GlassModManagerBoardListings.clear();
  GlassModManagerGui_MyAddons.clear();


  GlassModManager::setPane(1);
  GlassServerControlC::setTab(1);
}