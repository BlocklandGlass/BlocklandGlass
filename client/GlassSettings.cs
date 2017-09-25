if(!isObject(GlassSettings)) {
  new ScriptGroup(GlassSettings);
}

function GlassSettings::init() {
  if($Server::Dedicated)
    return;

  Glass.settingsLoaded = true;

  GlassLog::log("Loading Glass settings...");
  // registerSetting(%this, %name, %value, %callback, %displayName, %category, %type, %properties, %desc)
  // Hidden Settings
  GlassSettings.registerSetting("MM::Colorset", "Add-Ons/Colorset_Default/colorSet.txt");
  GlassSettings.registerSetting("Live::AFKTime", 15);
  GlassSettings.registerSetting("Live::HideRequests", false);
  GlassSettings.registerSetting("Live::HideFriends", false);
  GlassSettings.registerSetting("Live::HideBlocked", false);
  GlassSettings.registerSetting("Live::oRBsNotified", false); // do not change
  GlassSettings.registerSetting("Live::Keybind", "keyboard\tctrl space");
  GlassSettings.registerSetting("Live::FriendsWindow_Pos", (getWord(getRes(), 0) - 280) SPC 50);
  GlassSettings.registerSetting("Live::FriendsWindow_Ext", "230 380");
  GlassSettings.registerSetting("Live::Rooms", "0");
  GlassSettings.registerSetting("Servers::Favorites", "");
  GlassSettings.registerSetting("Server::UseBLG", true);

  // Auth
  GlassSettings.registerSetting("Auth::useDAA", "Default", "GlassAuth::updateDAASetting", "Digest Access Authentication", "Authentication", "dropdown", "Default Always Never", "Uses Digest Access Authentication, which requires an account at blocklandglass.com and password input.");

  // Volume
  GlassSettings.registerSetting("Volume::RoomChat", 0.8, "GlassAudio::updateVolume", "Room Chat", "Volume", "slider", "0 1 1 4");
  GlassSettings.registerSetting("Volume::FriendStatus", 0.8, "GlassAudio::updateVolume", "Friend Status", "Volume", "slider", "0 1 1 4");
  GlassSettings.registerSetting("Volume::DirectMessage", 0.8, "GlassAudio::updateVolume", "Direct Messages", "Volume", "slider", "0 1 1 4");
  // Mod Manager
  GlassSettings.registerSetting("MM::UseDefault", false, "GlassUpdaterSupport::updateSetting", "Use Default Updater", "Mod Manager", "checkbox", "", "Use Support_Updater's interface when updating add-ons.");
  GlassSettings.registerSetting("MM::LiveSearch", true, "", "Use Live Search", "Mod Manager", "checkbox");
  // Live
  GlassSettings.registerSetting("Live::StartupConnect", true, "", "Auto-Connect During Startup", "Live", "checkbox", "", "Automatically connect to Glass Live on start-up.");
  GlassSettings.registerSetting("Live::AutoJoinRoom", true, "", "Automatically Join Rooms", "Live", "checkbox", "", "Automatically join the chatroom when you connect to Glass Live.");
  GlassSettings.registerSetting("Live::TalkingAnimation", true, "", "Avatar Talking Animation", "Live", "checkbox", "", "Play avatar talking animation whenever you send a message on Glass Live.");
  GlassSettings.registerSetting("Live::ConfirmConnectDisconnect", false, "", "Confirm Connect/Disconnect", "Live", "checkbox", "", "Show a dialog box asking for confirmation when connecting and disconnecting to and from Glass Live.");
  GlassSettings.registerSetting("Live::DisplayFriendIDs", false, "GlassLive::createFriendList", "Display Friend BL_IDs", "Live", "checkbox", "", "Display friend's BL_IDs in the friends list.");
  GlassSettings.registerSetting("Live::OverlayLogo", true, "GlassOverlay::setLogo", "Display Overlay Logo", "Live", "checkbox", "", "Show the Glass logo in the overlay in the top left.");
  GlassSettings.registerSetting("Live::Vignette", true, "GlassOverlay::setVignette", "Display Vignette", "Live", "checkbox", "", "Show the vignette on the Glass overlay.");
  GlassSettings.registerSetting("Live::ShowFriendLocation", true, "", "Friend Location Notifications", "Live", "checkbox", "", "Show notifications when your friends join or leave a server.");
  GlassSettings.registerSetting("Live::ShowFriendStatus", true, "", "Friend Status Notifications", "Live", "checkbox", "", "Show notifications when your friends change their status.");
  GlassSettings.registerSetting("Live::PendingReminder", true, "", "Pending Friend Req. Reminder", "Live", "checkbox", "", "Show notification if you have any pending friend requests when you connect to Glass Live.");
  GlassSettings.registerSetting("Live::ReminderIcon", true, "GlassLive::createMessageReminder", "Reminder Icon", "Live", "checkbox", "", "Display a Glass icon in the corner of your screen while playing if you have an unread message or mention.");
  GlassSettings.registerSetting("Live::StartupNotification", true, "", "Startup Notification", "Live", "checkbox", "", "Show a start-up notification which includes your current keybind.");
  GlassSettings.registerSetting("Live::ShowTimestamps", true, "",  "Timestamping", "Live", "checkbox", "", "Show the time next to all chat messages in the chatroom and DMs.");
  GlassSettings.registerSetting("Live::MessageLogging", true, "", "Message Logging", "Live", "checkbox", "", "Log messages to config/client/BLG/chat_log/");
  // Chatroom
  GlassSettings.registerSetting("Live::RoomChatNotification", true, "", "Chat Notifications", "Chatroom", "checkbox");
  GlassSettings.registerSetting("Live::RoomNotification", false, "", "Entered/Exited Notifications", "Chatroom", "checkbox", "", "Show notifications when you enter and exit a room in Glass Live."); // joined room / left room notifications
  GlassSettings.registerSetting("Live::EmotePredict", true, "", "Emote Predictions", "Chatroom", "checkbox", "", "Display emote predictions while typing in the chat."); // joined room / left room notifications
  GlassSettings.registerSetting("Live::RoomMentionNotification", true, "", "Mentioned Notification", "Chatroom", "checkbox", "", "Display a notification and play a sound when you're @mentioned in a chatroom.");
  GlassSettings.registerSetting("Live::RoomShowBlocked", false, "", "Show Blocked Users", "Chatroom", "checkbox", "", "Show blocked users' messages in the chatroom.");
  GlassSettings.registerSetting("Live::ShowJoinLeave", true, "", "User Connection Messages", "Chatroom", "checkbox", "", "Show all users entering and exiting the chatroom."); // user connection messages in chatroom
  // Direct Messaging
  GlassSettings.registerSetting("Live::MessageNotification", true, "", "Message Notifications", "Direct Messaging", "checkbox");
  GlassSettings.registerSetting("Live::MessageAnyone", true, "", "Messages From Strangers", "Direct Messaging", "checkbox", "", "Receive DMs from people not on your friends list.");
  // Servers
  GlassSettings.registerSetting("Servers::LoadingImages", true, "", "Custom Loading Images", "Servers", "checkbox", "", "Display a custom loading image if the server has set one.");
  GlassSettings.registerSetting("Servers::DisplayPasswordedFavorites", true, "GlassServers::init", "Display Passworded Favorites", "Servers", "checkbox", "", "Display Favorite Servers that are passworded.");
  GlassSettings.registerSetting("Servers::EnableFavorites", true, "GlassServers::init", "Favorite Servers", "Servers", "checkbox", "", "Display Favorite Servers menu GUI.");
  GlassSettings.registerSetting("Servers::LoadingGUI", true, "", "Glass Loading GUI *", "Servers", "checkbox", "", "Use the Glass Loading GUI when connecting to a server.<br><br><font:verdana bold:13>Requires restart.");
  // Privacy
  GlassSettings.registerSetting("Live::ViewAvatar", "Anyone", "", "can view my avatar *", "Privacy", "dropdown", "Anyone Friends Myself", "Set who is able to view your avatar.<br><br><font:verdana bold:13>Requires reconnect.");
  GlassSettings.registerSetting("Live::ViewLocation", "Anyone", "", "can view my server *", "Privacy", "dropdown", "Anyone Friends Myself", "Set who is able to view the server you are currently playing on.<br><br><font:verdana bold:13>Requires reconnect.");
  // Notifications
  GlassSettings.registerSetting("Notifications::DarkMode", false, "", "Dark Notifications", "Notifications", "checkbox", "", "Enabled dark mode notifications.");
  GlassSettings.registerSetting("Notifications::ForceSticky", false, "", "Sticky Notifications", "Notifications", "checkbox", "", "Notifications stay on-screen until interacted with.");
  GlassSettings.registerSetting("Notifications::Limit", "0", "", "Notification Limit (0 = none)", "Notifications", "shorttext", "", "Maximum number of visible notifications.");
  // Experimental
  GlassSettings.registerSetting("Glass::UseDefaultWindows", false, "Glass::updateWindowSetting", "Default Windows", "Experimental", "checkbox", "", "EXPERIMENTAL: Uses default window themes. Functionality and quality not guaranteed.");


  %settings = trim(GlassSettings.settingsList);

  GlassSettings.loadData();

  for(%i = 0; %i < getWordCount(%settings); %i++) {
    %prefix = strReplace(getWord(%settings, %i), "_", " ");
    %group = GlassSettings.settingsList[%prefix];
    for(%o = 0; %o < getWordCount(%group); %o++) {
      %setting = getWord(%group, %o);
      %box = "GlassSettingsGui_Prefs_" @ %setting;

      switch$(%box.profile) {
        case "GlassCheckBoxProfile":
          %box.setValue(GlassSettings.get(%prefix @ "::" @ %setting));
        case "GuiPopUpMenuProfile":
          %box.setText(GlassSettings.get(%prefix @ "::" @ %setting));
        case "GuiSliderProfile":
          %box.setValue(GlassSettings.get(%prefix @ "::" @ %setting));
        case "GlassTextEditProfile":
          %box.setText(GlassSettings.get(%prefix @ "::" @ %setting));
      }
    }
  }
}

function GlassSettings::registerSetting(%this, %name, %value, %callback, %displayName, %category, %type, %properties, %desc) {
   %obj = new ScriptObject() {
    class = "GlassSetting";

    name = %name;
    value = %value;
    defaultValue = %value;
    callback = %callback;
  };

  %this.obj[%name] = %obj;
  %this.add(%obj);
  %this.schedule(0, "add", %obj);

  if(%displayName $= "")
	  return %obj;

  %categoryName = getWord(strReplace(%name, "::", " "), 0);
  if(!hasItemOnList(GlassSettings.settingsList, %categoryName))
	GlassSettings.settingsList = addItemToList(GlassSettings.settingsList, %categoryName);

  %this.settingsList[%categoryName] = addItemToList(%this.settingsList[%categoryName], getWord(strReplace(%name, "::", " "), 1));

  if(%name $= "") {
    error("No name specified.");
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

  %prefix = getSubStr(%name, 0, strpos(%name, ":"));
  %suffix = strchr(%name, ":");
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
        text = %displayName;
        groupNum = "-1";
        buttonType = "ToggleButton";
      };
    case "dropdown":
      %setting.extent = "250 30";
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

      %label = new GuiTextCtrl() {
        profile = "GuiTextVerdanaProfile";
        vertSizing = "center";
        text = %displayName;
        position = "95 3";
      };

      for(%i = 0; %i < getWordCount(%properties); %i++) {
        %ctrl.add(getWord(%properties, %i), %i + 1);
      }
    case "slider":
      %ctrl = new GuiSliderCtrl("GlassSettingsGui_Prefs_" @ %suffix) {
        profile = "GuiSliderProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "28 3";
        extent = "110 20";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        command = %command;
        range = getWord(%properties, 0) SPC getWord(%properties, 1);
        ticks = getWord(%properties, 3);
        snap = getWord(%properties, 2);
      };

      %label = new GuiTextCtrl() {
        profile = "GuiTextVerdanaProfile";
        vertSizing = "center";
        text = %displayName;
        position = "150 3";
      };
    case "button": // reset button?
      // to-do
    case "keybind":
      // to-do

    case "shorttext":
      %ctrl = new GuiTextEditCtrl("GlassSettingsGui_Prefs_" @ %suffix) {
        profile = "GlassTextEditProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "28 3";
        extent = "30 20";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        command = %command;
        groupNum = "-1";
      };

      %label = new GuiTextCtrl() {
        profile = "GuiTextVerdanaProfile";
        vertSizing = "center";
        text = %displayName;
        position = "70 3";
      };

    default:
      error("Non-existent setting type.");
      return;
  }

  %setting.add(%ctrl);

  if(isObject(%label))
	  %setting.add(%label);

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
      setting = %displayName;
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

  if(%type $= "checkbox")
    %setting.verticalMatchChildren(20, 4);
  %info.centerY();

  GlassSettingsGui_ScrollOverlay.settingsCount++;
  GlassSettingsGui_ScrollOverlay.add(%setting);

  GlassSettingsGui_ScrollOverlay.verticalMatchChildren(40, 10);
  GlassSettingsGui_ScrollOverlay.setVisible(true);
}

function GlassSettings::resetToDefaults(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %setting = %this.getObject(%i);

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

function GlassSettingsResize::onResize(%this, %x, %y, %h, %l) {
  GlassSettingsGui_Scroll.extent = vectorSub(GlassSettingsWindow.extent, "20 45");
  GlassSettingsGui_ScrollOverlay.verticalMatchChildren(getWord(GlassSettingsGui_Scroll.extent, 1), 10);
}

function GlassSettingsGui_ScrollOverlay::onWake(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %o = %this.getObject(%i);

    if(isObject(%o.text)) {
      %o.text.forceCenter();
    }
  }
}

function GlassSettingsGui_Info::onMouseUp(%this) {
  %desc = %this.description;

  if(%desc $= "")
    %desc = "No information available.";

  glassMessageBoxOk(%this.setting, %desc);
}

function GlassSettings::loadData(%this) {
  %fo = new FileObject();
  if(isFile("config/client/glass.conf")) {
    %fo.openForRead("config/client/glass.conf");
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
}

function GlassSettings::saveData(%this) {
  if(!Glass.settingsLoaded)
    return;

  %fo = new FileObject();
  %fo.openForWrite("config/client/glass.conf");
  %fo2 = new FileObject();
  %fo2.openForWrite("config/cache/glass.dat");

  for(%i = 0; %i < %this.getCount(); %i++) {
    %setting = %this.getObject(%i);
    if(%setting.class $= "GlassCache") {
      %fo2.writeLine(%setting.name TAB %setting.created TAB %setting.ttl TAB expandEscape(%setting.value));
    } else {
      %fo.writeLine(%setting.name TAB expandEscape(%setting.value));
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
    GlassLog::debug(" + Loaded setting " @ %name @ " with value " @ %value);
    %obj.value = %value; //only do that if loading!
  } else {
    warn("Data found for non-existant setting \"" @ %name @ "\"");
  }
}

function GlassSettings::update(%this, %name, %value) {
  %obj = %this.obj[%name];
  if(!isObject(%obj)) {
    error("Tried to update non-existant setting \"" @ %name @ "\"");
    return;
  }
  %obj.value = %value;
  if(%obj.callback !$= "") {
    eval(%obj.callback @ "(\"" @ expandEscape(%name) @ "\",\"" @ %value @ "\");");
  }
}

function GlassSettings::get(%this, %name) {
  return %this.obj[%name].value;
}

function GlassSettings::cacheCreate(%this, %name, %value, %ttl, %time) {
  if(isObject(%this.cache[%name])) {
    %obj = %this.cache[%name];
    %obj.value = %value;
    %obj.ttl = %ttl;
    %obj.created = %time;
  } else {
    %obj = new ScriptObject() {
      class = "GlassCache";
      value = %value;

      name = %name;

      created = %time;
      ttl = %ttl; // %ttl -- 0 = infinite
    };
  }

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
    GlassSettings.saveData();
    parent::onExit();
  }
};
activatePackage(GlassSettingsPackage);

function hasItemOnList(%string,%item) {
  for(%i=0;%i<getWordCount(%string);%i++) {
  %word = getWord(%string,%i);
	if(%word $= %item)
	  return true;
  }
  return false;
}

function addItemToList(%string,%item) {
  if(!hasItemOnList(%string, %item))
  	%string = %string SPC %item;
  return %string;
}

function removeItemFromList(%string,%item) {
  for(%i=0;%i<getWordCount(%string);%i++) {
  	%word = getWord(%string,%i);
  	if(%word $= %item)
  		continue;
  	%fString = %fString SPC %word;
  }
  return trim(%fString);
}
