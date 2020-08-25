function GlassResourceManager::execResource(%resource, %context) {
  if(!isObject(GlassResourceManager)) {
    new ScriptObject(GlassResourceManager);
    GlassGroup.add(GlassResourceManager);
  }
  //first, check if we have an update-able local copy
  //if we do, allow it to load normally
  //if not, execute our local version

  //if there's a zip, check for a version.json
  //if it's just a folder, we're going to assume they know what they're doing
  if(getFileLength("Add-Ons/" @ %resource @ ".zip") > 0 && isFile("Add-Ons/" @ %resource @ ".zip") && (isFile("Add-Ons/" @ %resource @ "/version.json") || isFile("Add-Ons/" @ %resource @ "/version.txt"))) {
    GlassLog::log("Local version of " @ %resource @ " found, proceeding as normal");
  } else if(getFileCount("Add-Ons/" @ %resource @ "/*.cs") > 0) {
    GlassLog::log("Local zip of " @ %resource @ " is missing, but a folder is present");
  } else {
    GlassLog::log("\n\c4Loading local resource " @ %resource @ "...");
    if(isFile("Add-Ons/System_BlocklandGlass/resources/" @ %resource @ "/" @ %context @ ".cs"))
      exec("Add-Ons/System_BlocklandGlass/resources/" @ %resource @ "/" @ %context @ ".cs");

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

function GlassResourceManager::loadPreferences(%context) {
  //delete zip
  fileDelete("Add-Ons/Support_Preferences.zip");
  //delete local installation
  folderDeleteAll("Add-Ons/Support_Preferences/");

  %resource = "Support_Preferences";
  GlassLog::log("\n\c4Loading local resource " @ %resource @ "...");
  if(isFile("Add-Ons/System_BlocklandGlass/resources/" @ %resource @ "/" @ %context @ ".cs"))
    exec("Add-Ons/System_BlocklandGlass/resources/" @ %resource @ "/" @ %context @ ".cs");
}

function folderDeleteAll(%folder) {
  if(getSubStr(%folder, strlen(%folder)-2, 1) !$= "/") {
    %folder = %folder @ "/";
  }

  for(%file = findFirstFile(%folder @ "*"); %file !$= ""; %file = findNextFile(%folder @ "*")) {
    fileDelete(%file);
  }
}

package GlassResourceManager {
  function Updater::onAdd(%this) {
    parent::onAdd(%this);

    %version["Support_Updater"] = "0.12.1+release-20160619";
    //%version["Support_Preferences"] = "1.2.1";
    %channel["Support_Updater"] = "release";
    //%channel["Support_Preferences"] = "stable";
    %repourl["Support_Updater"] = "mods.greek2me.us";
    //%repourl["Support_Preferences"] = "http://api.blocklandglass.com/api/3/repository.php";
    %format["Support_Updater"] = "TML";
    //%format["Support_Preferences"] = "JSON";
    %id["Support_Updater"] = "";
    //%id["Support_Preferences"] = "193";

    for(%i = 0; %i < GlassResourceManager.fakes; %i++) {
      %resource = GlassResourceManager.fake[%i];
      GlassLog::debug("\c5> GLASS FAKE: " @ %resource);

      //for debug purposes
      Glass.fakedResource[%resource] = true;

      %addonHandler = %this.addons;
      %addonHandler.storeFileInfo(%resource, %version[%resource], %channel[%resource], %repoURL[%resource], %format[%resource], %id[%resource], false, "");
    }
  }

  function MM_AuthBar::blinkSuccess(%this) {
    parent::blinkSuccess(%this);


    // Git check
    %countUp = getFileCount("Add-Ons/System_BlocklandGlass/resources/Support_Updater/*");
    %countPr = getFileCount("Add-Ons/System_BlocklandGlass/resources/Support_Preferences/*");

    if(%countUp == 0 || %countPr == 0) {
      schedule(50, 0, glassMessageBoxOk, "Missing Submodules", "Hey there,<br><br>It looks like you may have installed Glass from git. We use 'submodules' to manage Support_Updater and Support_Preferences, so they're not installed automatically. Assuming you have git installed, open the command line and navigate to the System_BlocklandGlass folder and type the following:<br><br><color:eb9950><just:left><lmargin:20><font:Lucida Console:10>git submodule init<br>git submodule update<br><br><lmargin:0><just:center><color:000000><font:verdana:13>Thanks, and enjoy!");
    }


    // Version check
    %version = updater.addons.getObjectByName("Support_Preferences").version;
    if(%version !$= "" && semanticVersionCompare(%version, "2.0.0") == 2) {
      //outdated
      schedule(50, 0, glassMessageBoxOk, "Bad Preferences", "It appears that you have a (very) old version of Support_Preferences installed. Please accept any updaters given by the updater or manually re-download it!<br><br><font:verdana bold:13>Failure to update Support_Preferences will lead to Glass Server Preferences problems.", "updater.checkForUpdates();");
    }
	}
};
activatePackage(GlassResourceManager);
