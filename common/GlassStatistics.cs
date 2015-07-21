function GlassStatistics::reportMods() {
  %glassRepo = updater.repositories.getObjectByURL("http://blocklandglass.com/api/support_updater/repo.php");
  for(%i = 0; %i < Updater.addons.getCount(); %i++) {
    %addon = Updater.addons.getObject(%i);
    if(%addon.hasRepository(%glassRepo)) {
      %index = %addon.repositoryIdx[%glassRepo];
      echo(%addon);
    	if(strLen(%addon.id[%index])) {
        %id = %addon.id[%index];
        %channel = %addon.channel;
        %version = %addon.version;
    	}
    }
  }
  echo("Glass Mods: " @ %modList);
}
