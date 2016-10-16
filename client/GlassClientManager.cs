function GlassClientManager::init() {
  if(isObject(GlassClientManager))
    GlassClientManager.delete();

  new ScriptObject(GlassClientManager);

  GlassClientManager.scan();
}

function GlassClientManager::scan(%this) {
  discoverFile("Add-Ons/*/glass.json");
  %pattern = "Add-ons/*/glass.json";
	//echo("\c1Looking for Glass Add-Ons");
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %path = filePath(%file);
    %name = getsubstr(%path, 8, strlen(%path)-8);

    if(!isFile(%path @ "/client.cs"))
      continue;

    %error = jettisonReadFile(%file);
    if(%error) {
      echo("Jettison read error");
      continue;
    }

    %value = $JSON::Value;

    %error = jettisonReadFile(%path @ "/version.json");
    if(%error) {
      echo("Jettison read error");
      continue;
    }

    %version = $JSON::Value;

    %this.hasAddon[%value.id] = true;
    %this.addons = %this.addons SPC %value.id @ "|" @ %version.version;
  }
  %this.addons = trim(%this.addons);
}

function GlassClientManager::getClients(%this) {
  return %this.addons;
}

function GlassClientManager::hasClient(%this, %id) {
  return %this.hasAddon[%id];
}

function GlassClientManager::downloadFinished(%id) {
  GlassClientManager.downloads++;
  if(GlassClientManager.downloads >= GlassClientManager.mods) {
    //join server
    canvas.popDialog(GlassClientGui);
    GlassClientManager.downloads = 0;
    GlassClientManager.mods = 0;
    GlassClientManager.scan();
    JoinServerGui.join();
  }
}

function GlassClientManager::populateGui(%required) {
  %this = GlassClientManager;

  %container = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 1";
    extent = "370 165";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  for(%i = 0; %i < %this.mods; %i++) {
    %name = %this.name[%i];
    %id = %this.id[%i];

    %swatch = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "350 40";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "255 255 255 255";
      addonId = %id;
      addonName = %name;
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 6";
      extent = "330 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
      text = "<font:verdana bold:12>" @ %name;
    };

    %swatch.progress = new GuiProgressCtrl(GlassClientGuiProgress) {
      profile = "GuiProgressProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 22";
      extent = "330 15";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
    };

    %swatch.progresstext = new GuiMLTextCtrl() {
       profile = "GuiMLTextProfile";
       horizSizing = "center";
       vertSizing = "center";
       position = "0 1";
       extent = "330 14";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       lineSpacing = "2";
       allowColorChars = "0";
       maxChars = "-1";
       maxBitmapHeight = "-1";
       selectable = "1";
       autoResize = "1";
       text = "<shadow:1:1><just:center><font:verdana:12><color:999999>Waiting...";
    };
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.progress);
    %swatch.progress.add(%swatch.progresstext);
    %swatch.verticalMatchChildren(0, 6);

    %container.add(%swatch);
    if(%last !$= "")
      %swatch.placeBelow(%last, 10);

    %last = %swatch;
  }

  canvas.pushDialog(GlassClientGui);

  GlassClientGui_Scroll.deleteAll();
  GlassClientGui_Scroll.add(%container);
  %container.verticalMatchChildren(0, 10);
  %container.setVisible(true);

  GlassClientGui_Skip.setVisible(!%required);
}

function GlassClientManager::accept() {
  %container = GlassClientGui_Scroll.getObject(0);
  for(%i = 0; %i < %container.getCount(); %i++) {
    %swatch = %container.getObject(%i);
    echo(%swatch.addonId);
    %ret = GlassModManager::downloadAddonFromId(%swatch.addonId);
    %ret.rtbImportProgress = %swatch.progress;
  }
}

function GlassClientManager::skip() {
  canvas.popDialog(GlassClientGui);
  GlassClientManager.downloads = 0;
  GlassClientManager.mods = 0;
  GlassClientManager.bypass = true;
  JoinServerGui.join();
}

package GlassClientManager {
  function GameConnection::onConnectRequestRejected(%this, %reason) {
    GlassClientManager.mods = 0;
    if(getField(%reason, 0) $= "MISSING" || getField(%reason, 0) $= "MISSING_OPT") {
      //echo(%reason);
      if(getField(%reason, 0) $= "MISSING")
        %required = true;

      canvas.popDialog(connectingGui);
      %mods = trim(setField(%reason, 0, ""));
      %missing = "";

      for(%i = 0; %i < getFieldCount(%mods); %i++) {
        %args = strreplace(getField(%mods, %i), "^", "\t");
        %name = getField(%args, 0);
        %id = getField(%args, 1);

        %required[%id] = true;

        if(GlassClientManager.hasClient(%id)) {
          echo("Has required mod " @ %id @ " (" @ %name @ ")");
        } else {
          GlassClientManager.name[%i] = %name;
          GlassClientManager.id[%i] = %id;
          %missing = %missing TAB getField(%mods, %i);
          echo("Missing required mod " @ %id @ " (" @ %name @ ")");
        }
      }

      %missing = getsubstr(%missing, 1, strlen(%missing)-1);

      %count = getFieldCount(%missing);
      GlassClientManager.mods = %count;

      if(%count > 0) {
        GlassClientManager::populateGui(%missing);
      } else {
        %clients = GlassClientManager.getClients();
        %hasStr = "";
        for(%i = 0; %i < getWordCount(%clients); %i++) {
          %args = strreplace(getWord(%clients, %i), "|", "\t");

          if(%required[getField(%args, 0)]) {
            %hasStr = %hasStr SPC getWord(%clients, %i);
          }
        }
        GlassClientManager.requestedMods = getsubstr(%hasStr, 1, strlen(%hasStr));
        reconnectToServer();
      }
    } else {
      parent::onConnectRequestRejected(%this, %reason);
    }
  }

  function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
    parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Glass" TAB Glass.version TAB GlassClientManager.requestedMods TAB GlassClientManager.bypass NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
    GlassClientManager.bypass = false;
  }

  function GlassClientGuiProgress::setValue(%this, %val) {
    parent::setValue(%this, %val);
    if(%val == 1) {
      GlassClientManager::downloadFinished(%this.getGroup().addonId);
    }
  }
};
activatePackage(GlassClientManager);
