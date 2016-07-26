exec("./common/support/Support_DateTime.cs");
exec("./common/support/Support_LibStr.cs");
exec("./common/support/Support_PreLoad.cs");
exec("./common/support/Support_SemVer.cs");
exec("./common/support/Support_TCPClient.cs");
exec("./common/support/Support_TMLParser.cs");
exec("./common/support/jettison.cs");

exec("./common/core/Updater.cs");
exec("./common/core/UpdaterAddOnSO.cs");
exec("./common/core/UpdaterAddOnHandlerSG.cs");
exec("./common/core/UpdaterRepoSO.cs");
exec("./common/core/UpdaterRepoHandlerSG.cs");
exec("./common/core/UpdaterFileDownloadHandlerSG.cs");

exec("./common/modules/Module_Statistics.cs");

if($Pref::Updater::DefaultPrefVersion $= "0.5.0")
{
	$Pref::Updater::DefaultPrefVersion = "0.12.0";
	$Pref::Updater::SimultaneousQueries = 3;
	echo("Updater set default preferences.");
}
else if($Pref::Updater::DefaultPrefVersion !$= "0.12.0")
{
	$Pref::Updater::DefaultPrefVersion = "0.12.0";
	//default preferences:
	$Pref::Updater::DedicatedServerUpdates = true;
	$Pref::Updater::QueryInterval = 0;
	$Pref::Updater::SilentUpdates = false;
	$Pref::Updater::SimultaneousQueries = 3;
	echo("Updater set default preferences.");
}

if(!isObject(Updater))
{
	new ScriptGroup(Updater);
}