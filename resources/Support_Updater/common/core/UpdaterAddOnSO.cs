//Creates a new UpdaterAddOnSO object.
//@param	string name
//@param	string version
//@param	string channel
//@param	UpdaterRepoSO repository
//@param	string id	An identifier to be used by add-on hosting services.
//@return	UpdaterAddOnSO The newly created object.
function UpdaterAddOnSO(%name, %version, %channel, %repository, %id)
{
	%this = new ScriptObject()
	{
		class = UpdaterAddOnSO;
		name = %name;
		version = %version;
		channel = %channel;
		repositoryCount = 0;
	};
	if(isObject(%repository))
	{
		%this.addRepository(%repository, %id);
	}
	return %this;
}

//Associates the add-on with a repository.
//@param	UpdaterRepoSO repository
//@param	string id
function UpdaterAddOnSO::addRepository(%this, %repository, %id)
{
	%this.repositoryIdx[%repository] = %this.repositoryCount;
	%this.repository[%this.repositoryCount] = %repository;
	%this.id[%this.repositoryCount] = %id;
	%this.repositoryCount ++;
}

//Checks whether this add-on already has the repository.
//@param	UpdaterRepoSO repository
//@return	bool
function UpdaterAddOnSO::hasRepository(%this, %repository)
{
	return strLen(%this.repositoryIdx[%repository]) > 0;
}

//@param	UpdaterRepoSO repository
//@return	string
function UpdaterAddOnSO::getModID(%this, %repository)
{
	return %this.id[%this.repositoryIdx[%repository]];
}