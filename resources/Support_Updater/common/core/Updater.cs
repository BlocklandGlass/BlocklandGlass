//Handles startup tasks and initializes update check.
function Updater::onAdd(%this)
{
	//Startup
	%this.add(%this.repositories = new ScriptGroup()
	{
		class = UpdaterRepoHandlerSG;
	});
	%this.add(%this.addons = new ScriptGroup()
	{
		class = UpdaterAddOnHandlerSG;
		parent = %this;
	});
	%this.add(%this.fileDownloader = new ScriptGroup()
	{
		class = UpdaterFileDownloadHandlerSG;
	});
	%this.add(%this.repoQueue = new SimSet()
	{
		class = UpdaterRepoDownloadQueueSS;
	});
	
	//This repository is for "community updates".
	%repo = UpdaterRepoSO("http://mods.greek2me.us/third-party");
	%this.repositories.add(%repo);
	
	//Query
	%this.addons.readLocalFiles();
	if($Updater::ManualQuery || %this.canDoUpdateCheck())
	{
		%this.schedule(100, "checkForUpdates");
	}
	
	if($Server::Dedicated)
	{
		%this.checkDayTick(getWord(getDateTime(), 0));
	}
}

//Handles cleanup tasks.
function Updater::onRemove(%this)
{

}

//Connects to repositories and looks for updates.
function Updater::checkForUpdates(%this)
{
	//clear the queue
	if(!isObject(%this.fileDownloader.currentDownload))
		%this.fileDownloader.removeAll();

	$Pref::Updater::LastQueryDate = getDateTime();

	for(%i = %this.repositories.getCount() - 1; %i >= 0; %i --)
	{
		%repo = %this.repositories.getObject(%i);
		if(%repo.isFallback)
			continue;
		%repo.queried = false;
		%this.repositories.queuePush(%repo);
	}
	
	if(!strLen($Pref::Updater::SimultaneousQueries) ||
		$Pref::Updater::SimultaneousQueries < 1)
	{
		$Pref::Updater::SimultaneousQueries = 1;
	}
	for(%i = 0; %i < $Pref::Updater::SimultaneousQueries; %i ++)
	{
		%this.repositories.queueQueryNext();
	}
}

//Determines whether we can check for updates.
//Checks whether enough time has elapsed since previous check.
//@return	bool
function Updater::canDoUpdateCheck(%this)
{
	%datetime = getDateTime();
	%last = $Pref::Updater::LastQueryDate;
	%interval = $Pref::Updater::QueryInterval;
	return !strLen(%last) || DT_getDayDifference(%last, %datetime) >= %interval;
}

//Downloads and installs selected updates.
function Updater::doUpdates(%this)
{
	%this.fileDownloader.downloadNext();
}

//Checks for updates on a large interval.
//@param	string date
function Updater::checkDayTick(%this, %date)
{
	%datetime = getDateTime();
	%newDate = getWord(%datetime, 0);
	
	if(%newDate !$= %date && updater.canDoUpdateCheck())
	{
		%this.checkForUpdates();
	}
	
	if(isEventPending(%this.checkDaySched))
		cancel(%this.checkDaySched);
	%this.checkDaySched = %this.scheduleNoQuota(30000, checkDayTick, %newDate);
}