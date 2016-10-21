function GlassResourceManager::execResource(%resource, %context) {
  if(!isObject(GlassResourceManager))
    new ScriptObject(GlassResourceManager);
  //first, check if we have an update-able local copy
  //if we do, allow it to load normally
  //if not, execute our local version

  //if there's a zip, check for a version.json
  //if it's just a folder, we're going to assume they know what they're doing
  if(getFileLength("Add-Ons/" @ %resource @ ".zip") > 0 && isFile("Add-Ons/" @ %resource @ ".zip") && (isFile("Add-Ons/" @ %resource @ "/version.json") || isFile("Add-Ons/" @ %resource @ "/version.txt"))) {
    echo("Local version of " @ %resource @ " found, proceeding as normal");
  } else if(getFileCount("Add-Ons/" @ %resource @ "/*.cs") > 0) {
    echo("Local zip of " @ %resource @ " is missing, but a folder is present");
  } else {
    echo("\n\c4Loading local resource " @ %resource @ "...");
    if(isFile("Add-Ons/System_BlocklandGlass/resources/" @ %resource @ "/" @ %context @ ".cs"))
      exec("Add-Ons/System_BlocklandGlass/resources/" @ %resource @ "/" @ %context @ ".cs");
    else
      echo("Missing context");
    echo("\n");

    GlassResourceManager::createFakeFile(%resource);

    if(!GlassResourceManager.faked[%resource]) {
      %fakes = GlassResourceManager.fakes + 0;
      GlassResourceManager.fake[%fakes] = %resource;
      GlassResourceManager.fakes++;
      GlassResourceManager.faked[%resource] = true;
    }
  }
}

function GlassResourceManager::createFakeFile(%resource) {
  %fo = new FileObject();
  %fo.openforwrite("Add-Ons/" @ %resource @ ".zip");
  %fo.writeLine("");
  %fo.close();
  %fo.delete();
}

package GlassResourceManager {
  function Updater::onAdd(%this) {
    parent::onAdd(%this);

    %version["Support_Updater"] = "0.12.1+release-20160619";
    %version["Support_Preferences"] = "1.2.0";
    %channel["Support_Updater"] = "release";
    %channel["Support_Preferences"] = "stable";
    %repourl["Support_Updater"] = "mods.greek2me.us";
    %repourl["Support_Preferences"] = "http://api.blocklandglass.com/api/2/repository.php";
    %format["Support_Updater"] = "TML";
    %format["Support_Preferences"] = "JSON";
    %id["Support_Updater"] = "";
    %id["Support_Preferences"] = "193";

    for(%i = 0; %i < GlassResourceManager.fakes; %i++) {
      %resource = GlassResourceManager.fake[%i];
      echo("\c5> GLASS FAKE: " @ %resource);

      //for debug purposes
      Glass.fakedResource[%resource] = true;

      %addonHandler = %this.addons;
      %addonHandler.storeFileInfo(%resource, %version[%resource], %channel[%resource], %repoURL[%resource], %format[%resource], %id[%resource], false, "");
    }
  }
};
activatePackage(GlassResourceManager);
