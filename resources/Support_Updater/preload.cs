if(!$Server::Dedicated)
	return;

function checkForUpdates()
{
	if(getMax(300, $Sim::Time) - $Updater::LastManualQuery < 300)
	{
		echo("You can only check for updates once every 5 minutes.");
		return;
	}
	$Updater::LastManualQuery = $Sim::Time;

	$Updater::ManualQuery = true;
	if(isObject(updater))
		updater.checkForUpdates();
	else
		exec("./dedicated.cs");
	$Updater::ManualQuery = false;
}

if(!$Pref::Updater::DedicatedServerUpdates && $Pref::Updater::DedicatedServerUpdates !$= "")
	return;

$Updater::DediAutoCheckTimeout = 5000;

package Support_Updater_DedicatedAuto
{
	function initDedicated()
	{
		if(updater.canDoUpdateCheck())
		{
			$Updater::dediServerPaused = true;
			echo("\n--------- Checking for Add-On Updates ---------");
		}
		else
		{
			return parent::initDedicated();
		}
	}
	
	function initDedicatedLAN()
	{
		if(updater.canDoUpdateCheck())
		{
			$Updater::dediServerPaused = true;
			echo("\n--------- Checking for Add-On Updates ---------");
		}
		else
		{
			return parent::initDedicatedLAN();
		}
	}

	function updaterInterfacePushItem(%item)
	{
		parent::updaterInterfacePushItem(%item);
		cancel($Updater::dediContinueSched);
	}
};

function updaterDediContinue()
{
	if(!$Updater::dediServerPaused)
		return;
	$Updater::dediServerPaused = false;
	deActivatePackage(Support_Updater_DedicatedAuto);
	if($Server::LAN)
		initDedicatedLAN();
	else
		initDedicated();
}

exec("./dedicated.cs");
activatePackage(Support_Updater_DedicatedAuto);