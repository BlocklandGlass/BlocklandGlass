function UpdaterRepoHandlerSG::onAdd(%this)
{
	%this.queue = new SimSet();
}

function UpdaterRepoHandlerSG::onRemove(%this)
{
	if(isObject(%this.queue))
		%this.queue.delete();
}

//Adds a repository to the query queue.
//@param	UpdaterRepoSO repository
function UpdaterRepoHandlerSG::queuePush(%this, %repository)
{
	%this.queue.add(%repository);
}

//Removes the first repository from the query queue.
function UpdaterRepoHandlerSG::queuePop(%this)
{
	%repo = %this.queue.getObject(0);
	%this.queue.remove(%repo);
	if(%this.queue.getCount() < 1)
	{
		%this.queueOnEmpty();
		return;
	}
	else
	{
		%this.queue.pushToBack(%this.queue.getObject(0));
	}
}

//Removes the a specific repository from the query queue.
//@param	UpdaterRepoSO repo
function UpdaterRepoHandlerSG::queueRemove(%this, %repo)
{
	%this.queue.remove(%repo);
	if(%this.queue.getCount() < 1)
	{
		%this.queueOnEmpty();
		return;
	}
	else
	{
		%this.queue.pushToBack(%this.queue.getObject(0));
	}
}

//Queries the next repository in the queue.
function UpdaterRepoHandlerSG::queueQueryNext(%this)
{
	for(%i = 0; %i < %this.queue.getCount(); %i ++)
	{
		%repo = %this.queue.getObject(%i);
		if(!%repo.queried)
		{
			%repo.queryRemote();
			break;
		}
	}
}

//Called when a repository has finished downloading.
//@param	int error
function UpdaterRepoHandlerSG::queueOnDownloadFinished(%this, %repo, %error)
{
	if(%error)
	{
		//update failed
	}
	if(%this.queue.isMember(%repo))
		%this.queueRemove(%repo);
	%this.queueQueryNext();
}

//Called when the queue becomes empty.
function UpdaterRepoHandlerSG::queueOnEmpty(%this)
{
	%count = %this.getGroup().fileDownloader.queue.getCount();
	echo("Update check completed." SPC %count SPC "updates found.");
	if(%count > 0)
	{
		if($Pref::Updater::SilentUpdates)
		{
			echo("Updater performing silent updates.");
			updater.doUpdates();
		}
		else
		{
			updaterInterfaceDisplay();
		}
	}
	else if($Server::Dedicated && $Updater::dediServerPaused)
	{
		updaterDediContinue();
	}
}

//@param	string url
//@param	UpdaterRepoSO
function UpdaterRepoHandlerSG::getObjectByURL(%this, %url)
{
	%components = urlGetComponents(%url);
	for(%i = %this.getCount() - 1; %i >= 0; %i --)
	{
		%r = %this.getObject(%i);
		if(%components $= urlGetComponents(%r.url))
		{
			return %r;
		}
	}
	return 0;
}