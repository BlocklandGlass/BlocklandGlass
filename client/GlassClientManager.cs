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

package GlassClientManager {
  function GameConnection::onConnectionDropped(%this, %msg) {
    parent::onConnectionDropped(%this, %msg);
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
