//@param	string name
//@return	UpdaterAddOnSO
function UpdaterAddOnHandlerSG::getObjectByName(%this, %name)
{
	for(%i = %this.getCount() - 1; %i >= 0; %i --)
	{
		%obj = %this.getObject(%i);
		if(%obj.name $= %name)
			return %obj;
	}
	return 0;
}

//Reads version information from all version.txt files in Blockland/Add-Ons.
function UpdaterAddOnHandlerSG::readLocalFiles(%this)
{
	%this.deleteAll();
	echo("Updater reading files.");

	%readFO = new FileObject();
	
	//Read version.txt files.
	%mask = "Add-Ons/*/version.txt";
	for(%file = findFirstFile(%mask); %file !$= ""; %file = findNextFile(%mask))
	{
		%path = filePath(%file);
		if(isFile(%path @ "/version.json"))
			warn("WARN: Add-on" SPC fileBase(%path) SPC "contains both version.txt and version.json files!");
		else
			%this._readFileInfoTXT(%file, %readFO);
	}
	
	//Read version.json files.
	%mask = "Add-Ons/*/version.json";
	for(%file = findFirstFile(%mask); %file !$= ""; %file = findNextFile(%mask))
		%this._readFileInfoJSON(%file, %readFO);

	%readFO.delete();
}

//Reads file information from a version.txt file.
//@param	string file
//@param	FileObject readFO
function UpdaterAddOnHandlerSG::_readFileInfoTXT(%this, %file, %readFO)
{
	%filePath = filePath(%file);
	%zip = %filePath @ ".zip";
	if($Pref::Updater::ZippedFilesOnly && !isFile(%zip))
		return;
	%zipPath = filePath(%zip);
	if(%zipPath !$= "Add-Ons")
		return;
	%zipBase = fileBase(%zip);

	//Read the file info.
	%readFO.openForRead(%file);
	while(!%readFO.isEOF())
	{
		%line = %readFO.readLine();
		%tag = "";
		%val = "";
		if(getFieldCount(%line) == 2)
		{
			%tag = getField(%line, 0);
			%val = getField(%line, 1);
		}
		else if(getWordCount(%line) == 2)
		{
			%tag = getWord(%line, 0);
			%val = getWord(%line, 1);
		}
		switch$(%tag)
		{
			case "version" or "version:" or "vers":			%version = %val;
			case "channel" or "channel:" or "chan":			%channel = %val;
			case "repository" or "repository:" or "repo":	%repository = %val;
			case "format" or "format:" or "form":			%format = %val;
			case "id" or "id:":
				if(strPos(%val, " ") >= 0 || strPos(%val, "\t") >= 0)
					echo("Invalid ID for add-on" SPC %zipBase);
				else
					%id = %val;
		}
	}
	%readFO.close();
	
	if(strLen(%version) && strLen(%channel) && strLen(%repository))
	{
		%this.storeFileInfo(%zipBase, %version, %channel, %repository, %format, %id);
	}
	else
	{
		warn("WARN: Invalid version file:" SPC %file);
	}
}

//Reads file information from a version.json file.
//JSON allows for more complicated data structures than the old TXT system.
//@param	string file
//@param	FileObject readFO
function UpdaterAddOnHandlerSG::_readFileInfoJSON(%this, %file, %readFO)
{
	%filePath = filePath(%file);
	%zip = %filePath @ ".zip";
	if($Pref::Updater::ZippedFilesOnly && !isFile(%zip))
		return;
	%zipPath = filePath(%zip);
	if(%zipPath !$= "Add-Ons")
		return;
	%zipBase = fileBase(%zip);
	
	%json = loadJSON(%file, "", %readFO);
	if(isObject(%json))
	{
		%version = %json.get("version");
		%channel = %json.get("channel");
		%repositories = %json.get("repositories");
		if(isObject(%repositories))
		{
			%fallback = 0;
			for(%i = %repositories.length - 1; %i >= 0; %i --)
			{
				%repo = %repositories.item[%i];
				%url = %repo.get("url");
				%format = %repo.get("format");
				%id = %repo.get("id");
				%isFallback = (%i > 0);
				%this.storeFileInfo(%zipBase, %version, %channel, %url, %format, %id, %isFallback, %fallback);
				%fallback = %this.parent.repositories.getObjectByURL(%url);
			}
		}
		%json.delete();
	}
	else
	{
		warn("WARN: Invalid version file:" SPC %file);
	}
}

//Stores version information for an add-on.
//@param	string zipBase	The name of the add-on's *.zip file.
//@param	string version	Version number.
//@param	string channel	Update channel to use.
//@param	string repoURL	URL of the repository.
//@param	string format	Format of the repository.
//@param	string id	Mod ID for the repository.
//@param	bool isFallback	Whether this is a fallback repository.
//@param	UpdaterRepoSO fallback	The fallback repository if this one fails.
//@return	UpdaterAddOnSO
function UpdaterAddOnHandlerSG::storeFileInfo(%this, %zipBase, %version, %channel, %repoURL, %format, %id, %isFallback, %fallback)
{
	//Find the correct repo object, if it exists
	%repoSO = %this.parent.repositories.getObjectByURL(%repoURL);
	if(isObject(%repoSO))
	{
		//Update the format tag if it was never set.
		if(!strLen(%repoSO.format) && strLen(%format))
			%repoSO.format = %format;
		if(%repoSO.isFallback && !%isFallback)
			%repoSO.isFallback = false;
	}
	else
	{
		%repoSO = UpdaterRepoSO(%repoURL, %format, %isFallback);
		%this.parent.repositories.add(%repoSO);
	}
	
	//Add fallback
	if(isObject(%fallback))
	{
		%repoSO.addFallback(%fallback);
	}
	
	//Add the mod to our list of add-ons.
	%addonSO = %this.getObjectByName(%zipBase);
	if(!isObject(%addonSO))
	{
		%addonSO = UpdaterAddOnSO(%zipBase, %version, %channel, "", "");
		%this.add(%addonSO);
	}
	if(!%addonSO.hasRepository(%repoSO))
	{
		%addonSO.addRepository(%repoSO, %id);
	}
	return %addonSO;
}