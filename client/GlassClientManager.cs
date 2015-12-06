function GlassClientManager::init() {
  if(!isObject(GlassClientManager)) {
    new ScriptGroup(GlassClientManager);
  }
}

function GlassClientManager::clean() {
  GlassClientManager.deleteAll();
}

function GlassClientManager::addRequiredMod(%modid, %name) {
  %obj = new ScriptObject() {
    class = "GlassClientMod";
    id = %modid;
    name = %name;
  };
  GlassClientManager.add(%obj);
}

function GlassClientManager::openDownloadGui() {
  %ctx = GlassDownloadInterface::openContext("Required Clients", "This server requires some add-ons to be downloaded before joining!");

	for(%i = 0; %i < GlassClientManager.getCount(); %i++) {
		%mod = GlassClientManager.getObject(%i);
    echo(%mod);
		%handler = %ctx.addDownload("<font:quicksand-bold:16>" @ %mod.name @ " <font:quicksand:14>ID: " @ %mod.id);
	}

  %ctx.registerCallback("GlassClientManager::downloadCallback");
}

function GlassClientManager::downloadCallback(%code) {
  if(%code == 1) {
    messageBoxOk("Sorry..", "Automatic downloading is currently not available. Please visit the <a:blocklandglass.com>Blockland Glass Website</a> to download the required add-ons.");
  } else if(%code == -1) {
    canvas.pushDialog(JoinServerGui);
  } else if(%code == 2) {

  }
}

function GlassClientManager::getClients(%this) {
  %pref = GlassPrefGroup::findByVariable("$Pref::Glass::ClientAddons");
  %requiredMods = strreplace(%pref.value, ",", "\t");

  GlassServerControlGui_RequiredClientsPopUp.clear();
  %pattern = "Add-ons/*/glass.json";
	echo("\c1Looking for client Add-Ons");
  %files = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
		%json = loadJSON(%file);
    if(%json.get("formatVersion") == 1) {

      if(strpos(getsubstr(%file, 8, strlen(%file)-19), "/") != -1) {
        echo(getsubstr(%file, 8, strlen(%file)-19));
        continue;
      }

      if(isfile(getsubstr(%file, 0, strlen(%file)-10) @ "client.cs")) {
        %files = getsubstr(%file, 8, strlen(%file)-19) @ "," @ %files;
      }
    }
	}

  return %files;
}

package GlassClientManager {
  function GameConnection::onConnectionDropped(%this, %msg) {
    parent::onConnectionDropped(%this, %msg);
    %this.dump();
    GlassClientManager::clean();
    if(strpos(%msg, "Missing Blockland Glass Mods") == 0) {
      echo(" +- Glass Mods Missing!");
      %lines = strreplace(%msg, "<br>", "\n");
      for(%i = 2; %i < getLineCount(%lines); %i++) {
        %mod = getLine(%lines, %i);

        if(%mod !$= "") {
          %mid = getField(%mod, 0);
          %name = getField(%mod, 1);
          GlassClientManager::addRequiredMod(%mid, %name);
        }
      }
      canvas.popDialog(messageBoxOkDlg);
      GlassClientManager::openDownloadGui();
    }
  }
};
activatePackage(GlassClientManager);
