if(!$Server::Dedicated)
	return;

exec("./common.cs");

function update()
{
	cancel($Updater::dediContinueSched);
	if(!isObject(updater.fileDownloader) || updater.fileDownloader.queue.getCount() < 1)
		echo("There are no updates to be performed.");
	else if(isObject(updater.fileDownloader.currentDownload))
		echo("An update is in progress.");
	else
	{
		echo("Performing updates...");
		updater.doUpdates();
		return true;
	}
	return false;
}

function skip(%addon, %ignore)
{
	cancel($Updater::dediContinueSched);
	%obj = updater.addons.getObjectByName(%addon);
	if(isObject(%obj))
	{
		if(%ignore)
		{
			echo("Ignoring" SPC %obj.name);
			$Pref::Updater::Ignore[%obj.name] = true;
		}
		else
			echo("Skipping" SPC %obj.name);
		%obj.delete();
		if(updater.fileDownloader.queue.getCount() < 1)
			updater.fileDownloader.onQueueEmpty();
		else
			updaterInterfaceDisplay();
		return true;
	}
	else if(%addon $= "")
	{
		echo("Please specify an add-on to skip. SYNTAX: skip(\"AddOn_Name\");");
	}
	else
	{
		echo(%addon SPC "is not in the update queue.");
	}
	return false;
}

function skipAll()
{
	cancel($Updater::dediContinueSched);
	updater.skipAll = true;
	updaterInterfaceOnQueueEmpty();
	updater.skipAll = false;
}

function ignore(%addon)
{
	skip(%addon, true);
}

function updaterInterfaceDisplay(%refreshing)
{
	%width = 75;
	%div = repeatChar("-", %width);
	%col0 = 35;
	%col1 = 32;
	%col2 = 8;
	%head0 = "Add-On";
	%head1 = "New Version";
	%head2 = "Restart?";
	%numCols = 3;
	echo("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
	echo(" \nADD-ON UPDATES AVAILABLE:\n");
	%line = "  ";
	for(%e = 0; %e < %numCols; %e ++)
		%line = %line @ %head[%e] @ repeatChar(" ", %col[%e] - strLen(%head[%e]));
	echo(%line);
	echo("  " @ %div);
	%count = updater.fileDownloader.queue.getCount();
	for(%i = 0; %i < %count; %i ++)
	{
		%item = updater.fileDownloader.queue.getObject(%i);
		%val0 = %item.name;
		%val1 = %item.updateVersion;
		%val2 = (%item.updateRestartRequired ? "Yes" : "No");
		%line = "   ";
		for(%e = 0; %e < %numCols; %e ++)
			%line = %line @ %val[%e] @ repeatChar(" ", %col[%e] - strLen(%val[%e]));
		echo(%line);
	}
	echo("  " @ %div @ "\n");
	echo("  update(); - Perform all updates in the queue."
		NL "  ignore(\"AddOn_Name\"); - Skip and ignore all future updates."
		NL "  skip(\"AddOn_Name\"); - Skip an update once."
		NL "  skipAll(); - Skip all updates in the queue.\n");
	if($Updater::dediServerPaused)
	{
		if($Updater::dediInterfaceDisplayed)
		{
			echo(" ");
		}
		else
		{
			echo("  If no action is taken, updates will be automatically skipped in 30 seconds.");
			cancel($Updater::dediContinueSched);
			$Updater::dediContinueSched = schedule(30000, 0, updaterDediContinue);
		}
	}
	
	//Display message to host!
	if(!$Updater::dediInterfaceDisplayed)
	{
		%blid = getNumKeyID();
		for(%i = clientGroup.getCount() - 1; %i >= 0; %i --)
		{
			%cl = clientGroup.getObject(%i);
			if(%cl.getBLID() $= %blid)
			{
				//FOR MessageBoxYesNo, MAX OF 279 CHARACTERS
				%maxChars = 279;
				%title = "Server Updates Available";
				%end = "\n\n<spush><font:arial bold:14>Update now?";
				%callback = 'DoUpdates';
				%message = "<spush><font:arial bold:14>Updates for the following are available:<spop>\n";
				for(%i = updater.fileDownloader.queue.getCount() - 1; %i >= 0; %i --)
					%message = %message NL updater.fileDownloader.queue.getObject(%i).name;
				%messageLen = strLen(%message);
				%titleLen = strLen(%title);
				%endLen = strLen(%end);
				%callbackLen = strLen(%callback);
				%totalLen = %titleLen + %messageLen + %endLen + %callbackLen;
				if(%totalLen > %maxChars)
					%message = getSubStr(%message, 0, %maxChars - %totalLen + %messageLen - 4) @ "\n...";
				%message = %message @ %end;
				commandToClient(%cl, 'MessageBoxYesNo', %title, %message, %callback);
				break;
			}
		}
	}
	$Updater::dediInterfaceDisplayed = true;
}

function updaterInterfacePushItem(%item)
{

}

function updaterInterfacePopItem(%item)
{
	if(updater.echoToChat)
		talk("Updated" SPC %item.name @ ".");
}

function updaterInterfaceSelectItem(%item)
{

}

function updaterInterfaceOnQueueEmpty()
{
	if(updater.skipAll)
		updaterEcho("All updates skipped.");
	else if(updater.restartRequired)
	{
		if(updater.hasErrors)
			updaterWarn("Updater completed with errors.");
		updaterEcho("Some updates require a restart. Type \"quit();\" to close Blockland now.");
	}
	else if(updater.hasErrors)
		updaterWarn("Updater completed with errors.");
	else
		updaterEcho("All updates have been completed.");

	if($Updater::dediServerPaused)
	{
		cancel($Updater::dediContinueSched);
		$Updater::dediContinueSched = schedule(3000, 0, updaterDediContinue);
	}
	
	updater.echoToChat = false;
	$Updater::dediInterfaceDisplayed = false;
}

function updaterEcho(%msg)
{
	if(updater.echoToChat)
		talk(%msg);
	echo(%msg);
}

function updaterWarn(%msg)
{
	if(updater.echoToChat)
		talk("\c0" @ %msg);
	warn(%msg);
}

function serverCmdDoUpdates(%client)
{
	if(%client.getBLID() !$= getNumKeyID())
		return;
	if(updater.fileDownloader.queue.getCount() < 1 || isObject(updater.fileDownloader.currentDownload))
		return;
	talk("Performing server add-on updates.");
	updater.echoToChat = true;
	updater.doUpdates();
}

function repeatChar(%char, %count)
{
	for(%i = 0; %i < %count; %i ++)
		%string = %string @ %char;
	return %string;
}

package Support_Updater_DediServer
{
	function GameConnection::onClientEnterGame(%this)
	{
		if(%this.getBLID() $= getNumKeyID() &&
			!isObject(updater.fileDownloader.currentDownload) &&
			updater.fileDownloader.queue.getCount() > 0)
		{
			schedule(5000, 0, updaterInterfaceDisplay);
		}
		return parent::onClientEnterGame(%this);
	}
};
activatePackage(Support_Updater_DediServer);