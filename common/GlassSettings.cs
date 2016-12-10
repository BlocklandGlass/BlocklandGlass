if(!isObject(GlassSettings)) {
  new ScriptGroup(GlassSettings);
}

function GlassSettings::init(%context) {
  echo("Loading " @ %context @ " prefs");

  if(%context $= "client") {
    GlassSettings.registerSetting("client", "MM::UseDefault", false, "GlassUpdaterSupport::updateSetting");
    GlassSettings.registerSetting("client", "MM::Colorset", "Add-Ons/Colorset_Default/colorSet.txt");
    GlassSettings.registerSetting("client", "MM::LiveSearch", true);

    GlassSettings.registerSetting("client", "Live::oRBsNotified", false); // do not change

    GlassSettings.registerSetting("client", "Live::TalkingAnimation", true);
    GlassSettings.registerSetting("client", "Live::OverlayLogo", true);
    GlassSettings.registerSetting("client", "Live::Vignette", true);
    GlassSettings.registerSetting("client", "Live::Keybind", "keyboard\tctrl space");
    GlassSettings.registerSetting("client", "Live::ShowTimestamps", true);
    GlassSettings.registerSetting("client", "Live::ConfirmConnectDisconnect", false);
    GlassSettings.registerSetting("client", "Live::PendingReminder", true);

    GlassSettings.registerSetting("client", "Live::RoomChatNotification", true);
    GlassSettings.registerSetting("client", "Live::RoomChatSound", true);
    GlassSettings.registerSetting("client", "Live::RoomMentionNotification", true);
    GlassSettings.registerSetting("client", "Live::RoomShowBlocked", false);
    GlassSettings.registerSetting("client", "Live::AutoJoinRoom", true);
    GlassSettings.registerSetting("client", "Live::RoomNotification", false); // joined room / left room notifications

    GlassSettings.registerSetting("client", "Live::FriendsWindow_Pos", (getWord(getRes(), 0) - 280) SPC 50);
    GlassSettings.registerSetting("client", "Live::FriendsWindow_Ext", "230 380");

    GlassSettings.registerSetting("client", "Live::MessageNotification", true);
    GlassSettings.registerSetting("client", "Live::MessageSound", true);
    GlassSettings.registerSetting("client", "Live::MessageLogging", true);
    GlassSettings.registerSetting("client", "Live::MessageAnyone", true);

    GlassSettings.registerSetting("client", "Live::ShowJoinLeave", true); // user connection messages in chatroom
    GlassSettings.registerSetting("client", "Live::StartupNotification", true);
    GlassSettings.registerSetting("client", "Live::StartupConnect", true);
    GlassSettings.registerSetting("client", "Live::ShowFriendStatus", true);

    GlassSettings.registerSetting("client", "Servers::EnableFavorites", true);
    GlassSettings.registerSetting("client", "Servers::LoadingImages", true);
    GlassSettings.registerSetting("client", "Servers::LoadingGUI", true);

    GlassSettings.registerSetting("client", "Live::FakeSetting", "One");

  // } else if(%context $= "server") {
    // GlassSettings.registerSetting("server", "SC::SAEditRank", 3);
    // GlassSettings.registerSetting("server", "SC::AEditRank", 2);
    // GlassSettings.registerSetting("server", "SC::RequiredClients", "");

    GlassSettings.registerSetting("client", "Servers::Favorites", "");
  }

  GlassSettings.loadData(%context);
}

function GlassSettings::registerSetting(%this, %context, %name, %value, %callback) {
  %obj = new ScriptObject() {
    class = "GlassSetting";

    name = %name;
    value = %value;
    defaultValue = %value;
    callback = %callback;

    context = %context;
  };
  %this.obj[%name] = %obj;
  %this.add(%obj);
  %this.schedule(0, "add", %obj);

  return %obj;
}

function GlassSettings::resetToDefaults(%this, %context) {
  if(%context $= "") {
    error("Specify \"client\" or \"server\" settings to reset.");
    return;
  }

  if(%context $= "server" || %context $= "client") {
    for(%i = 0; %i < %this.getCount(); %i++) {
      %setting = %this.getObject(%i);

      if(%setting.context !$= %context)
        continue;

      if(%setting.name $= "Live::Keybind")
        continue;

      %this.update(%setting.name, %setting.defaultValue);

      %name = getsubstr(%setting.name, strpos(%setting.name, "::") + 2, strlen(%setting.name));
      %box = "GlassSettingsGui_Prefs_" @ %name;

      if(isObject(%box)) {
        %box.setValue(%setting.defaultValue);
      }
    }

    warn("All Glass Settings have been reset!");
  } else {
    error("Invalid context.");
  }
}

function GlassSettings::createSettingHeader(%name) {
  %header = "GlassModManagerGui_Header_" @ strreplace(%name, " ", "_");

  if(isObject(%header)) {
    return %header;
  }

  %gui = new GuiSwatchCtrl(%header) {
    position = "10 50";
    extent = "250 25";
    minExtent = "8 2";
    color = "100 100 100 255";
  };

  %gui.text = new GuiTextCtrl() {
    profile = "GlassSearchResultProfile";
    position = "5 2";
    vertSizing = "center";
    horizSizing = "center";
    extent = "12 4";
    text = "\c3" @ %name;
  };

  %gui.add(%gui.text);
  %gui.text.centerX();

  return %gui;
}

function GlassSettings::drawSetting(%this, %pref, %name, %category, %type, %properties, %desc) {
  if(GlassSettings.get(%pref) $= "") {
    error("Non-existent setting.");
    return;
  }

  if(%category $= "") {
    error("No category specified.");
    return;
  }

  if(!isObject("GlassModManagerGui_Header_" @ strreplace(%category, " ", "_"))) {
    %header = GlassSettings::createSettingHeader(%category);

    if(isObject(%this.last) && %this.last != %header) {
      %header.placeBelow(%this.last, 15);
    }

    GlassSettingsGui_ScrollOverlay.add(%header);

    %this.last = %header;
  }

  %setting = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 0";
    extent = "250 25";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "230 230 230 255";
  };

  %setting.placeBelow(%this.last, 5);

  %this.last = %setting;

  %prefix = getSubStr(%pref, 0, strpos(%pref, ":"));
  %suffix = strchr(%pref, ":");
  %suffix = getSubStr(%suffix, 2, strlen(%suffix));
  %command = "GlassLive::updateSetting(\"" @ %prefix @ "\", \"" @ %suffix @ "\");";

  if(isObject("GlassSettingsGui_Prefs_" @ %suffix)) {
    error("Setting '" @ %suffix @ "' already exists in GUI.");
    return;
  }

  switch$(%type) {
    case "checkbox":
      %ctrl = new GuiCheckBoxCtrl("GlassSettingsGui_Prefs_" @ %suffix) {
        profile = "GlassCheckBoxProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "28 4";
        extent = "180 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        command = %command;
        text = %name;
        groupNum = "-1";
        buttonType = "ToggleButton";
      };
    case "dropdown": // unfinished - need to add GUI text (using %name) explaining what the dropdown text is for; also need to tweak extent/placement of dropdown ctrl.
      %setting.extent = "250 50";
      %ctrl = new GuiPopUpMenuCtrl("GlassSettingsGui_Prefs_" @ %suffix) {
        profile = "GuiPopUpMenuProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "28 3";
        extent = "60 20";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        command = %command;
        maxLength = "255";
        maxPopupHeight = "200";
      };

      for(%i = 0; %i < getWordCount(%properties); %i++) {
        %ctrl.add(getWord(%properties, %i), %i + 1);
      }
    case "slider": // for audio stuff
      // to-do
    case "button": // reset button
      // to-do
    case "keybind":
      // to-do
    default:
      error("Non-existent setting type.");
      return;
  }

  %setting.add(%ctrl);

  if(%desc !$= "")
    %infoColor = "255 255 255 255";
  else
    %infoColor = "180 180 180 255";

  %info = new GuiBitmapCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "7 4";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/help.png";
    wrap = "0";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    keepCached = "0";
    mColor = %infoColor;
    mMultiply = "0";

    new GuiMouseEventCtrl("GlassSettingsGui_Info") {
      setting = %name;
      description = %desc;
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "0 0";
      extent = "16 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lockMouse = "0";
    };
  };
  %setting.add(%info);

  %setting.verticalMatchChildren(20, 4);
  %info.centerY();

  GlassSettingsGui_ScrollOverlay.settingsCount++;
  GlassSettingsGui_ScrollOverlay.add(%setting);

  GlassSettingsGui_ScrollOverlay.verticalMatchChildren(40, 10);
  GlassSettingsGui_ScrollOverlay.setVisible(true);
}

function GlassSettingsGui_Info::onMouseUp(%this) {
  %desc = %this.description;

  if(%desc $= "")
    %desc = "No information available.";

  glassMessageBoxOk(%this.setting, %desc);
}

function GlassSettings::loadData(%this, %context) {
  %fo = new FileObject();
  if(isFile("config/" @ %context @ "/glass.conf")) {
    %fo.openForRead("config/" @ %context @ "/glass.conf");
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      %this.loadSetting(getField(%line, 0), collapseEscape(getField(%line, 1)));
    }
  }

  %fo.close();

  if(!%this.cacheLoaded && isFile("config/cache/glass.dat")) {
    %fo.openForRead("config/cache/glass.dat");
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      %name = getField(%line, 0);
      %created = getField(%line, 1);
      %ttl = getField(%line, 2);
      %value = collapseEscape(getField(%line, 3));

      if(%created+%ttl < getRealTime() && %ttl != 0) {
        if(Glass.dev)
          warn("Cached value [" @ %name @ "] has expired! [ " @ %created @ " | " @ %ttl @ " ]");
      } else {
        %this.cacheCreate(%name, %value, %ttl, %created);
      }
    }

    %this.cacheLoaded = true;
  }

  %fo.delete();

  %this.loaded[%context] = true;
}

function GlassSettings::saveData(%this, %context) {
  %fo = new FileObject();
  %fo.openForWrite("config/" @ %context @ "/glass.conf");
  %fo2 = new FileObject();
  %fo2.openForWrite("config/cache/glass.dat");

  for(%i = 0; %i < %this.getCount(); %i++) {
    %setting = %this.getObject(%i);
    if(%setting.context $= %context) {
      %fo.writeLine(%setting.name TAB expandEscape(%setting.value));
    }

    if(%setting.class $= "GlassCache") {
      %fo2.writeLine(%setting.name TAB %setting.created TAB %setting.ttl TAB expandEscape(%setting.value));
    }
  }

  %fo.close();
  %fo2.close();
  %fo.delete();
  %fo2.delete();
}

function GlassSettings::loadSetting(%this, %name, %value) {
  %obj = GlassSettings.obj[%name];
  if(isObject(%obj)) {
    if(Glass.dev) {
      echo(" + Loaded pref " @ getField(%line, 0));
    }
    %obj.value = %value; //only do that if loading!
  } else {
    warn("Data found for non-existant pref \"" @ %name @ "\"");
  }
}

function GlassSettings::update(%this, %name, %value) {
  %obj = GlassSettings.obj[%name];
  %obj.value = %value;
  if(%obj.callback !$= "") {
    eval(%obj.callback @ "(\"" @ expandEscape(%name) @ "\",\"" @ %value @ "\");");
  }
}

function GlassSettings::get(%this, %name) {
  return %this.obj[%name].value;
}

function GlassSettings::cacheCreate(%this, %name, %value, %ttl, %time) {
  %obj = new ScriptObject() {
    class = "GlassCache";
    value = %value;

    name = %name;

    created = %time;
    ttl = %ttl; // %ttl -- 0 = infinite
  };

  %this.cache[%name] = %obj;
  %this.add(%obj);
}

function GlassSettings::cachePut(%this, %name, %value, %ttl) {
  if(!isObject(%this.cache[%name])) {
    %this.cacheCreate(%name, %value, %ttl+0, getRealTime());
  } else {
    %this.cache[%name].value = %value;
    %this.cache[%name].created = getRealTime();
  }
}

function GlassSettings::cacheFetch(%this, %name) {
  if(isObject(%this.cache[%name])) {
    return %this.cache[%name].value;
  } else {
    return "";
  }
}

package GlassSettingsPackage {
  function onExit() {
    if(GlassSettings.loaded["client"]) {
      GlassSettings.saveData("client");
    }

    if(GlassSettings.loaded["server"]) {
      GlassSettings.saveData("server");
    }
    parent::onExit();
  }
};
activatePackage(GlassSettingsPackage);
