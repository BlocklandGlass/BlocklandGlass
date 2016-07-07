function registerServerSettingPrefs() {
	%cat = "Blockland";
	%icon = "blLogo";

	registerPref(%cat, "General", "Server Name", "string", "$Pref::Server::Name", $Pref::Server::Name, "64 1", "updateServerSetting", 0);
	registerPref(%cat, "General", "Welcome Message", "string", "$Pref::Server::WelcomeMessage", $Pref::Server::WelcomeMessage, "512 0", "updateServerSetting", 0);
	registerPref(%cat, "General", "Maximum Players", "playercount", "$Pref::Server::MaxPlayers", $Pref::Server::MaxPlayers, "1 99", "updateServerSetting", 0);
	registerPref(%cat, "Security", "Server Password", "string", "$Pref::Server::Password", $Pref::Server::Password, "512 0", "updateServerSetting", 0, 1);
	registerPref(%cat, "Security", "Admin Password", "string", "$Pref::Server::AdminPassword", $Pref::Server::AdminPassword, "512 0", "updateServerSetting", 0, 1);
	registerPref(%cat, "Security", "Super Admin Password", "string", "$Pref::Server::SuperAdminPassword", $Pref::Server::SuperAdminPassword, "512 0", "updateServerSetting", 0, 1, 1);
	registerPref(%cat, "Security", "Enable E-Tard Filter", "boolean", "$Pref::Server::ETardFilter", $Pref::Server::ETardFilter, "", "updateServerSetting", 0);
	registerPref(%cat, "Security", "E-Tard List", "wordlist", "$Pref::Server::ETardList", $Pref::Server::ETardList, ", 51", "updateServerSetting", 0);
	registerPref(%cat, "Security", "Who can change preferences?", "list", "$Pref::BLPrefs::AllowedRank", $Pref::BLPrefs::AllowedRank, "Host 3 Super_Admin 2 Admin 1", "updateBLPrefPermission", 0, 0, 1);
	registerPref(%cat, "Security", "Auto Admin IDs", "userlist", "$Pref::Server::AutoAdminList", $Pref::Server::AutoAdminList, "_ -1", "updateBLPrefPermission", 0, 0, 0);
	registerPref(%cat, "Security", "Auto Super Admin IDs", "userlist", "$Pref::Server::AutoSuperAdminList", $Pref::Server::AutoSuperAdminList, "_ -1", "updateBLPrefPermission", 0, 0, 1);
	registerPref(%cat, "Gameplay", "Enable Falling Damage", "boolean", "$Pref::Server::FallingDamage", $Pref::Server::FallingDamage, "", "updateServerSetting", 0);
	registerPref(%cat, "Gameplay", "Maximum Bricks/second", "number", "$Pref::Server::MaxBricksPerSecond", $Pref::Server::MaxBricksPerSecond, "1 9999 0", "updateServerSetting", 0);
	registerPref(%cat, "Gameplay", "Randomly color bricks?", "boolean", "$Pref::Server::RandomBrickColor", $Pref::Server::RandomBrickColor, "", "updateServerSetting", 0);
	registerPref(%cat, "Gameplay", "Too Far Distance", "number", "$Pref::Server::TooFarDistance", $Pref::Server::TooFarDistance, "1 999999 0", "updateServerSetting", 0);
	registerPref(%cat, "Gameplay", "Wrench events are admin only?", "boolean", "$Pref::Server::WrenchEventsAdminOnly", $Pref::Server::WrenchEventsAdminOnly, "", "updateServerSetting", 0);

	registerPrefGroupIcon(%cat, %icon);
	
	// as an example later on with colors, allow all player shapeNameColors to be set

	$BLPrefs::AddedServerSettings = true;
}
registerServerSettingPrefs();

function updateServerSetting(%value, %client, %prefSO) {
	%title = %prefSO.title;

	if(%title $= "Server Name" || %title $= "Maximum Players" || %title $= "Server Password") {
		webcom_postserver();
		
		for(%i=0; %i< ClientGroup.getCount(); %i++)
		{
			%cl = ClientGroup.getObject(%i);
			%cl.sendPlayerListUpdate();
		}
	}
	
	// headachey ones:
	$Server::Name = $Pref::Server::Name;
	$Server::WelcomeMessage = $Pref::Server::WelcomeMessage;
	$Server::MaxBricksPerSecond = $Pref::Server::MaxBricksPerSecond;
	$Server::WrenchEventsAdminOnly = $Pref::Server::WrenchEventsAdminOnly;
}

function updateBLPrefPermission(%level, %client, %pso) { // let client mods know if they're allowed or not
	for(%i = 0; %i < ClientGroup.getCount(); %i++) {
		%cl = ClientGroup.getObject(%i);
		commandToClient(%cl, 'BLPAllowedUse', %cl.BLP_isAllowedUse());
	}
}

function autoAdminsChanged(%value, %client, %prefSO) {
	// update each player's admin status
	for(%i=0;%i<ClientGroup.getCount();%i++)
	{
		%cl = ClientGroup.getObject(%i);
		
		%status = %cl.checkAdminStatus();

		if(%status $= 2)
		{
		   if(%cl.isSuperAdmin)
			  continue;
		   
		   %cl.isAdmin = 1;
		   %cl.isSuperAdmin = 1;
		   %cl.sendPlayerListUpdate();
		   commandtoclient(%cl,'setAdminLevel',2);
		   messageAll('MsgAdminForce','\c2%1 has become Super Admin (Auto)',%cl.name);
		
		   RTBSC_SendPrefList(%client);
		}
		else if(%status == 1)
		{
		   if(%cl.isAdmin)
			  continue;
		   
		   %cl.isAdmin = 1;
		   %cl.isSuperAdmin = 0;
		   %cl.sendPlayerListUpdate();
		   commandtoclient(%cl,'setAdminLevel',1);
		   messageAll('MsgAdminForce','\c2%1 has become Admin (Auto)',%cl.name);
		}
		else if(%status == 0)
		{
		   if(!%cl.isAdmin)
			  continue;
		   
		   %cl.isAdmin = 0;
		   %cl.isSuperAdmin = 0;
		   %cl.sendPlayerListUpdate();
		   commandtoclient(%cl,'setAdminLevel',0);
		   messageAll('MsgAdminForce','\c2%1 is no longer Admin.',%cl.name);
		}
	}
}