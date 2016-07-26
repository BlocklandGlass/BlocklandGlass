$Updater::CacheLocation = "config/common/updater/cache/";
$Updater::MaxGETRequest = 3500;

if(!isObject($Updater::RepoQueryOptions))
{
	$Updater::RepoQueryOptions = new ScriptObject(:TCPClientDefaults)
	{
		connectionTimeout = 5000;
		connectionRetryCount = 0;
		printErrors = true;
	};
}

//Creates a new UpdaterRepoSO object.
//@param	string url	The URL of the repository.
//@param	string format	The format of the repository. Can be either TML (default) or JSON.
//@param	bool isFallback	If true, this repository is only a fallback, not primary, repo.
//@return	UpdaterRepo The newly created object.
function UpdaterRepoSO(%url, %format, %isFallback)
{
	%hash = sha1(%url);
	if(!strLen(%format))
		%format = "TML";
	if(%format $= "JSON")
		%cacheFile = $Updater::CacheLocation @ %hash @ ".json";
	else if(%format $= "TML")
		%cacheFile = $Updater::CacheLocation @ %hash @ ".txt";
	echo("Updater add repository" SPC %url);
	return new ScriptObject()
	{
		class = UpdaterRepoSO;
		hash = %hash;
		cacheFile = %cacheFile;
		url = %url;
		format = %format;
		isFallback = !!%isFallback;
		fallbackCount = 0;
		queried = false;
	};
}

//Adds another repository as a fallback in case this one fails.
//@param	UpdaterRepoSO fallback
function UpdaterRepoSO::addFallback(%this, %fallback)
{
	%this.fallback[%this.fallbackCount] = %fallback;
	%this.fallbackCount ++;
}

//Queries the remote server for the latest version of the repository.
//@return TCPObject	The TCPObject used to connect.
function UpdaterRepoSO::queryRemote(%this)
{
	%this.queried = true;

	%components = urlGetComponents(%this.url);
	%method = "GET";
	%server = getField(%components, 1);
	%port = getField(%components, 2);
	%path = getField(%components, 3);
	%query = getField(%components, 4);
	%savePath = %this.cacheFile;
	%class = UpdaterRepoTCP;
	%options = $Updater::RepoQueryOptions;
	
	//send the username with the query
	%uname = "user=" @ urlEnc($Pref::Player::NetName);
	%query = (strLen(%query) ? %query @ "&" @ %uname : %uname);
	
	//send a list of mod IDs
	for(%i = updater.addons.getCount() - 1; %i >= 0; %i --)
	{
		%obj = updater.addons.getObject(%i);
		if(%obj.hasRepository(%this) && strLen(%id = %obj.getModID(%this)))
		{
			%modList = %modList @ "-" @ %id;
		}
	}
	
	%modListLen = strLen(%modList);
	if(%modListLen)
		%modList = getSubStr(%modList, 1, %modListLen - 1);
	
	//Impose a length restriction.
	if(%modListLen > $Updater::MaxGETRequest)
	{
		error("ERROR: Updater mod list is too large to send.");
		%modList = "";
	}
	%modList = "mods=" @ urlEnc(%modList);
	%query = (strLen(%query) ? %query @ "&" @ %modList : %modList);
	
	%this.tcp = TCPClient(%method, %server, %port, %path, %query, %savePath, %class, %options);
	%this.tcp.repo = %this;
	
	echo("Updater query repository" SPC %this.url);
	return %this.tcp;
}

//Parses the local repository file that was downloaded from the server.
//Any updates are placed in the UpdaterFileDownloadHandlerSG.
//@param	FileObject fo	A FileObject used for reading the file.
//@return	bool	Whether parsing was successful.
function UpdaterRepoSO::parseLocal(%this, %fo)
{
	if(!isFile(%this.cacheFile))
		return 0;
	if(%this.format $= "TML")
	{
		%parserObj = new ScriptObject()
		{
			repo = %this;
		};
		echo("Updater parse TML for repository" SPC %this.url);
		parseCustomTMLFile(%this.cacheFile, %parserObj, "updater", %fo);
		%parserObj.delete();
	}
	else if(%this.format $= "JSON")
	{
		%json = loadJSON(%this.cacheFile, "", %fo);
		if(isObject(%json))
		{
			echo("Updater parse JSON for repository" SPC %this.url);
			%this._parseJSON(%json);
			%json.delete();
		}
		else
		{
			error("ERROR: Invalid JSON repository" SPC %this.url);
		}
	}
}

function UpdaterRepoSO::_parseJSON(%this, %json)
{
	if(%json.isKey("name"))
		%this.name = %json.get("name");
	%addons = %json.get("add-ons");
	for(%i = 0; %i < %addons.length; %i ++)
	{
		%update = %addons.item[%i];
		%name = %update.get("name");
		%addon = updater.addons.getObjectByName(%name);
		
		if(isObject(%addon))
		{
			if(!%addon.hasRepository(%this))
			{
				warn("WARN:" SPC %this.url SPC "does not own" SPC %addon.name @ "!");
				continue;
			}
			
			if(%update.isKey("desc"))
				%addon.updateDescription = %update.get("desc");
			else
				%addon.updateDescription = %update.get("description");
			
			%channels = %update.get("channels");
			for(%e = 0; %e < %channels.length; %e ++)
			{
				%channel = %channels.item[%e];
				if(%channel.name $= %addon.channel || %channel.name $= "*")
				{
					%addon.updateVersion = %channel.get("version");
					%addon.updateFile = %channel.get("file");
					%addon.updateCRC = %channel.get("crc");
					%addon.updateChangeLog = %channel.get("changeLog");
					%restartRequired = %channel.get("restartRequired");
					if(strLen(%restartRequired))
						%addon.updateRestartRequired = semanticVersionCompare(%addon.version, %restartRequired) == 2;
					else
						%addon.updateRestartRequired = false;
					if(semanticVersionCompare(%addon.updateVersion, %addon.version) == 1)
					{
						updater.fileDownloader.push(%addon);
					}
					break;
				}
			}
		}
	}
}

//Cleans up after the connection has completed.
function UpdaterRepoTCP::onDone(%this, %error)
{
	%repo = %this.repo;
	if(%error)
	{
		warn("WARN: Updater failed to query repository" SPC %repo.url);
		for(%i = 0; %i < %repo.fallbackCount; %i ++)
		{
			%r = %repo.fallback[%i];
			if(!%r.queried)
			{
				%r.queryRemote();
			}
		}
		echo("Updater checking for cached version of repository.");
	}
	%repo.parseLocal();
	%repo.getGroup().queueOnDownloadFinished(%repo, %error);
}

//Legacy TML parser
function customTMLParser_updater(%obj,%val0,%val1,%val2,%val3,%val4,%val5,%val6,%val7,%val8,%val9,%val10,%val11,%val12,%val13,%val14,%val15)
{
	switch$(%val[0])
	{
		case "repository":
			if(strLen(%val[1]))
				%obj.repoName = %val[1];
			else
				%obj.repoName = %obj.repo.url;

		case "/repository":
			if(%obj.addedToQueue > 0)
			{

			}
			%obj.repoName = "";

		case "addon":
			%obj.addon = updater.addons.getObjectByName(%val[1]);
			if(!isObject(%obj.addon) && isFile("Add-Ons/" @ %val[1] @ ".zip"))
			{
				if(%obj.repo.tcp.server $= "mods.greek2me.us")
				{
					//This is a special exception for RTB.
					//I have to resort to awful hacks like this
					// because people like their special RTB
					// versions.
					if(%val[1] $= "System_ReturnToBlockland")
					{
						%crc = getFileCRC("Add-Ons/System_ReturnToBlockland.zip");
						if(%crc $= "-1")
							%channel = "Not_a_real_channel";
						else if(%crc $= "-343036381")
							%channel = "DAProgs";
						else if(%crc $= "-642662817")
							%channel = "Port";
						else
							%channel = "NO_INET";
					}
					else
					{
						%channel = "release";
					}
					
					%obj.addon = UpdaterAddOnSO(%val[1], 0, %channel, %obj.repo);
					updater.addons.add(%obj.addon);
				}
			}
			if(isObject(%obj.addon) && !$Pref::Updater::Ignore[%val[1]])
			{
				if(!%obj.addon.hasRepository(%obj.repo))
				{
					warn("WARN:" SPC %obj.repoName SPC "does not own" SPC %obj.addon.name @ "!");
					%obj.skipAddon = true;
				}
			}
			else
			{
				%obj.skipAddon = true;
			}

		case "/addon":
			if(!%obj.skipAddon)
			{
				if(semanticVersionCompare(%obj.addon.updateVersion, %obj.addon.version) == 1)
				{
					%obj.addedToQueue ++;
					updater.fileDownloader.push(%obj.addon);
				}
			}
			%obj.skipAddon = false;
			%obj.addon = "";

		case "channel":
			if(%val[1] $= %obj.addon.channel || %val[1] $= "*")
			{
				%obj.inUpdateChannel = true;
			}

		case "/channel":
			%obj.inUpdateChannel = "";

		case "desc" or "description":
			%obj.addon.updateDescription = %v;

		default:
			if(!%obj.skipAddon && %obj.inUpdateChannel)
			{
				%v = %val[1];
				//URL handling
				if((%v $= "http" || %v $= "https") &&
					strPos(%val[2], "//") == 0)
				{
					%v = getSubStr(%val[2], 2, strLen(%val[2]));
				}
				switch$(%val[0])
				{
					case "version": %obj.addon.updateVersion = %v;
					case "file": %obj.addon.updateFile = %v;
					case "crc": %obj.addon.updateCRC = %v;
					case "changelog": %obj.addon.updateChangeLog = %v;
					case "restartRequired":
						if(strLen(%v))
							%obj.addon.updateRestartRequired = semanticVersionCompare(%obj.addon.version, %v) == 2;
						else
							%obj.addon.updateRestartRequired = false;
				}
			}
	}
}