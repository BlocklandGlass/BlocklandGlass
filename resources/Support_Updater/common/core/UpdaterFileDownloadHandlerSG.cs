$Updater::DownloadLocation = "config/common/updater/download/";
$Updater::AutoDeleteTempFiles = true;

function UpdaterFileDownloadHandlerSG::onAdd(%this)
{
	%this.add(%this.queue = new SimSet());
}

//Adds an add-on to the update queue
//@param	UpdaterAddOnSO addon
function UpdaterFileDownloadHandlerSG::push(%this, %addon)
{
	if(!strLen(%addon.updateFile))
	{
		error("ERROR:" SPC %addon.name SPC "invalid download URL.");
		return;
	}
	%downloadPath = "Add-Ons/" @ %addon.name @ ".zip";
	if(!isWriteableFileName(%downloadPath))
	{
		error("ERROR:" SPC %addon.name SPC "invalid download path.");
		//return; //this will be caught later in the process - at least let the user know there's an update
	}

	%this.queue.add(%addon);

	if(!$Pref::Updater::SilentUpdates)
	{
		updaterInterfacePushItem(%addon);
	}

	echo(%addon.name SPC "added to download queue.");
}

//Removes the first object from the queue.
function UpdaterFileDownloadHandlerSG::pop(%this)
{
	%download = %this.queue.getObject(0);
	if(!$Pref::Updater::SilentUpdates)
		updaterInterfacePopItem(%download);
	%this.queue.remove(%download);
	if(%this.queue.getCount() > 1)
		%this.queue.pushToBack(%this.queue.getObject(0));
}

function UpdaterFileDownloadHandlerSG::removeAll(%this)
{
	while(%this.queue.getCount() > 0)
		%this.pop();
}

//Advances to the next file and begins downloading it.
function UpdaterFileDownloadHandlerSG::downloadNext(%this)
{
	if(%this.queue.getCount() < 1)
	{
		%this.onQueueEmpty();
		return;
	}
	%this.currentDownload = %this.queue.getObject(0);

	//view info
	if(!$Pref::Updater::SilentUpdates)
		updaterInterfaceSelectItem(%this.currentDownload);

	%downloadPath = $Updater::DownloadLocation @ %this.currentDownload.name @ ".zip";
	%this.downloadTCP = connectToURL(
		%this.currentDownload.updateFile,
		"GET",
		%downloadPath,
		UpdaterDownloadTCP
	);
}

//Downloads the change log associated with an object in the queue.
//@param	UpdaterAddOnSO queueObj	A download object which is currently in the queue.
function UpdaterFileDownloadHandlerSG::downloadChangeLog(%this, %queueObj)
{
	%queueObj.changeLogTCP = connectToURL(
		%queueObj.updateChangeLog,
		"GET",
		"",
		UpdaterChangeLogTCP
	);
	%queueObj.changeLogTCP.queueObj = %queueObj;
}

//Handles cleanup tasks after a download has finished.
//@param	int error
function UpdaterFileDownloadHandlerSG::onDownloadFinished(%this, %error)
{
	%addon = %this.currentDownload;
	%tempFile = $Updater::DownloadLocation @ %addon.name @ ".zip";
	discoverFile(%tempFile);
	if(%error)
	{
		echo("\c2ERROR: Unable to update" SPC %addon.name @ "! TCPClient error" SPC %error);
		updater.hasErrors = true;
	}
	else if(strLen(%addon.updateCRC) && %addon.updateCRC !$= getFileCRC(%tempFile))
	{
		echo("\c2ERROR: Unable to update" SPC %addon.name @ "! CRC mismatch.");
		updater.hasErrors = true;
	}
	else
	{
		%file = "Add-Ons/" @ %addon.name @ ".zip";
		fileCopy(%tempFile, %file);
		discoverFile(%file);
		$version__[%addon.name] = %addon.updateVersion;
		$versionOld__[%addon.name] = %addon.version;
		$versionRestartRequired__[%addon.name] = %addon.updateRestartRequired;
		%callbackFile = "Add-Ons/" @ %addon.name @ "/update.cs";
		if(isFile(%callbackFile))
		{
			echo(%addon.name SPC "running update scripts.");
			exec(%callbackFile);
		}
		if(%addon.updateRestartRequired)
		{
			warn(%addon.name SPC "requires a restart!");
			updater.restartRequired = true;
		}
		else
		{
			echo(%addon.name SPC "update completed!");
			%initFile = "Add-Ons/" @ %addon.name @ "/server.cs";
			if(isFile(%initFile) && $Game::Running && $AddOnLoaded__[%addon.name])
				exec(%initFile);
			%initFile = "Add-Ons/" @ %addon.name @ "/client.cs";
			if(isFile(%initFile) && !$Server::Dedicated)
				exec(%initFile);
		}
	}

	if($Updater::AutoDeleteTempFiles)
		fileDelete(%tempFile);
	%this.currentDownload = 0;

	%this.pop();
	%this.downloadNext();
}

//Called when all downloads have completed.
function UpdaterFileDownloadHandlerSG::onQueueEmpty(%this)
{
	if(!$Pref::Updater::SilentUpdates)
		updaterInterfaceOnQueueEmpty();
	updater.hasErrors = false;
	updater.addons.readLocalFiles();
}

//Handles cleanup after a file has been downloaded.
//@param	int error
function UpdaterDownloadTCP::onDone(%this, %error)
{
	updater.fileDownloader.onDownloadFinished(%error);
}

//Sets the progress bar.
//@param	float value	A floating point number from 0 to 1.
function UpdaterDownloadTCP::setProgressBar(%this, %value)
{
	%progressBar = updater.fileDownloader.currentDownload.guiSwatch.progress;
	if(isObject(%progressBar))
		%progressBar.setValue(%value);
}

//Displays the change log text.
//@param	int error
function UpdaterChangeLogTCP::onDone(%this, %error)
{
	if(updaterDlg.viewItem == %this.queueObj)
	{
		if(%error)
		{
			updaterDlgChangeLogText.setText("<color:ffffff><just:center>\n\n\n\nError occured. Change log unavailable.");
		}
		else
		{
			if(!%this.queueObj.updateChangeLogParsed)
			{
				%this.queueObj.updateChangeLogText = parseCustomTML(
					"<color:ffffff><linkcolor:cccccc>" @ %this.queueObj.updateChangeLogText,
					updaterDlgChangeLogText,
					"updaterChangeLog\tdefault");
				%this.queueObj.updateChangeLogParsed = true;
			}
			updaterDlgChangeLogText.setText(%this.queueObj.updateChangeLogText);
		}
	}
}

//Callback from the TCP library.
function UpdaterChangeLogTCP::handleText(%this, %text)
{
	%this.queueObj.updateChangeLogText = %this.queueObj.updateChangeLogText @ %text;
}

function customTMLParser_updaterChangeLog(%obj,%value0,%value1)
{
	switch$(%value[0])
	{
		case "version":
			return true TAB "<h3>Version" SPC %value[1] @ "</h3>";

		case "/version":
			return true TAB "<br><br>";
	}
	return false;
}