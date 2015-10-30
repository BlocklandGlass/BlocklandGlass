function GlassResourceManager::addResource(%name, %filename, %url, %restart) {
  if(!isObject(GlassResourceManager)) {
    %this = new ScriptObject(GlassResourceManager);
    %this.resources = new ScriptGroup(GlassResourceGroup);
  } else {
    %this = GlassResourceManager;
  }

  %obj = new ScriptObject() {
    class = "GlassResource";

    name = %name;
    filename = %filename;
    url = %url;

    restart = %restart;

    downloaded = false;
  };
  %this.resources.add(%obj);

  if(isfile("Add-Ons/" @ %filename)) {
    echo("Resource \"" @ %name @ "\" found!");
    %obj.downloaded = true;
  } else {
    echo("Resource \"" @ %name @ "\" not found. Attempting to download.");
    warn("If you're developing, I'd recommend putting an empty zip in your add-ons folder");
  }

  return %obj;
}

function GlassResourceManager::prompt(%this) {
  for(%i = 0; %i < %this.resources.getCount(); %i++) {
    if(!%this.resources.getObject(%i).downloaded) {
      %download = true;
    }
  }

  if(%download) {
    //if dedicated, skip prompt and go straight to download

    //open a gui
  } else {
    echo("All resources found");
  }
}

function GlassResourceManager::acceptPrompt(%this) { //the user doesn't quite have a choice with this one, as it's required
  for(%i = 0; %i < %this.resources.getCount(); %i++) {
    %res = %this.resources.getObject(%i);
    if(!%res.downloaded) {
      echo("Downloading " @ %this.name);
      %url = %res.url;
    	%method = "GET";
    	%downloadPath = "Add-Ons/" @ %res.filename;
    	%className = "GlassResourceTCP";

    	%tcp = connectToURL(%url, %method, %downloadPath, %className);
      %tcp.resource = %res;

      %this.downloads++;
    }
  }
}

function GlassResourceManager::check(%this) {
  for(%i = 0; %i < %this.resources.getcount(); %i++) {
    %resource = %this.resources.getObject(%i);
  }
}

function GlassResourceTCP::setProgressBar(%this, %completed) {
  if(isObject(%this.resource.progressBar)) {
    //create progressbar
  } else {
    %this.resource.progressBar.setValue(%completed);
  }
}

function GlassResourceTCP::onDone(%this, %error) {
  echo("Downloaded " @ %this.resource.name @ " as " @ %this.resource.filename);

  if(isFunction(%this.resource.getClassName(), "onDone")) {
    %this.resource.onDone();
  }

  if(%this.resource.restart) {
    GlassResourceManager.restart = true;
  } else {
    //check client/server
    exec("Add-Ons/" @ getsubstr(%this.resource.filename, 0, strlen(%this.resource.filename)-3) @ "/client.cs");
  }
}

GlassResourceManager::addResource("Updater", "Support_Updater.zip", "http://mods.greek2me.us/storage/Support_Updater.zip", true);
GlassResourceManager::addResource("Preferences", "Support_Preferences.zip", "http://mods.greek2me.us/storage/Support_Updater.zip", false);
