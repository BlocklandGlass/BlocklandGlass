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
  return;
  for(%i = 0; %i < %this.resources.getCount(); %i++) {
    if(!%this.resources.getObject(%i).downloaded) {
      %download = true;
    }
  }

  if(%download) {
    if($Server::Dedicated) {
      echo("Downloading Glass Dependencies");
      GlassResourceManager::acceptPrompt();
    } else {
      %ctx = GlassDownloadInterface::openContext("Required Add-Ons", "Blockland Glass relies on a few standardized add-ons to work. They'll automatically install, all you have to do is press download!");
      %ctx.registerCallback("GlassResourceManager::downloadGui");
      %ctx.inhibitClose(true);
      for(%i = 0; %i < %this.resources.getCount(); %i++) {
        %res = %this.resources.getObject(%i);
        if(!%res.downloaded)
          %res.dlHandler = %ctx.addDownload("<font:arial bold:16>" @ %res.name @ " <font:arial:14>" @ %res.filename);
      }
    }
  } else {
    echo("All dependencies found");
  }
}

function GlassResourceManager::downloadGui(%call) {
  if(%call == 1) {
    GlassResourceManager::acceptPrompt();
  } else if(%code == 2) {
    if(GlassDownloadInterface.getCount() == 1) {
      messageBoxOk("Please Restart", "Please restart Blockland for these changes to take effect. Pressing OK will close Blockland.", "quit();");
    }
  }
}

function GlassResourceManager::acceptPrompt() { //the user doesn't quite have a choice with this one, as it's required
  %this = GlassResourceManager;
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
  if(!$Server::Dedicated) {
    %this.resource.dlHandler.setProgress(%completed);
  }
}

function GlassResourceTCP::onDone(%this, %error) {
  if(!%error) {
    echo("Downloaded " @ %this.resource.name @ " as " @ %this.resource.filename);

    if(isFunction(%this.resource.getClassName(), "onDone")) {
      %this.resource.onDone();
    }

    if(%this.resource.restart) {
      GlassResourceManager.restart = true;
    } else {
      if($Server::Dedicated) {
        %f = "server.cs";
      } else {
        %f = "client.cs";
      }
      exec("Add-Ons/" @ getsubstr(%this.resource.filename, 0, strlen(%this.resource.filename)-4) @ "/" @ %f);
    }
  } else {
    echo("Error: " @ %error);
  }
}

GlassResourceManager::addResource("Updater", "Support_Updater.zip", "http://mods.greek2me.us/storage/Support_Updater.zip", true);
GlassResourceManager::addResource("Preferences", "Support_Preferences.zip", "http://api.blocklandglass.com/download.php?branch=3&aid=193", false);
