exec("./GlassLiveConnection.cs");
exec("./GlassLiveUser.cs");
exec("./GlassLiveRoom.cs");
exec("./GlassLiveGroup.cs");

//Settings:
//RoomChatNotification
//RoomChatSound
//RoomMentionNotification
//RoomAutoJoin
//MessageNotification
//MessageSound
//MessageAnyone

// instructions for adding a setting
// - add pref to %settings variable in glasslive::init() below.
// - register setting in glasssettings::init() in common/glassettings.cs
// - if setting is to be changed by the user at will via the glass settings gui, add corresponding .drawsetting() for pref in glasslive::init() below.

function GlassLive::init() {
  if(!isObject(GlassLive))
    new ScriptObject(GlassLive) {
      color_default = "222222";
      color_self = "55acee";
      color_friend = "2ecc71";
      color_mod = "e67e22";
      color_admin = "e74c3c";
      color_bot = "9b59b6";
    };

  if(!isObject(GlassLiveUsers))
    new ScriptGroup(GlassLiveUsers);

  if(!isObject(GlassLiveGroups))
    new ScriptGroup(GlassLiveGroups);

  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassOverlayGui.gui");
  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassSettingsGui.gui");


  //GlassOverlayGui.add(GlassFriendsGui.getObject(0));
  GlassSettingsWindow.setVisible(false);
  GlassOverlayGui.add(GlassSettingsWindow);

  // glass pref, description/name, category, type
  GlassSettings.drawSetting("Live::StartupConnect", "Auto-Connect During Startup", "Live", "checkbox");
  GlassSettings.drawSetting("Live::StartupNotification", "Startup Notification", "Live", "checkbox");
  GlassSettings.drawSetting("Live::PendingReminder", "Pending Friend Req. Reminder", "Live", "checkbox");
  GlassSettings.drawSetting("Live::ShowTimestamps", "Timestamping", "Live", "checkbox");
  GlassSettings.drawSetting("Live::ShowFriendStatus", "Friend On/Off-Line Notifications", "Live", "checkbox");
  GlassSettings.drawSetting("Live::ConfirmConnectDisconnect", "Confirm Connect/Disconnect", "Live", "checkbox");

  GlassSettings.drawSetting("MM::UseDefault", "Use Default Updater", "Mod Manager", "checkbox");
  GlassSettings.drawSetting("MM::LiveSearch", "Use Live Search", "Mod Manager", "checkbox");

  GlassSettings.drawSetting("Live::RoomShowAwake", "Share Awake Status", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::ShowJoinLeave", "User Connection Messages", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::RoomMentionNotification", "Mentioned Notification", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::RoomChatNotificationNew", "Chat Notifications", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::RoomChatSoundNew", "Chat Sounds", "Chatroom", "checkbox");
  // GlassSettings.drawSetting("Live::RoomAutoJoin", "Auto Join Room", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::RoomNotification", "Joined/Left Notifications", "Chatroom", "checkbox");

  GlassSettings.drawSetting("Live::MessageNotification", "Message Notifications", "Direct Messenging", "checkbox");
  GlassSettings.drawSetting("Live::MessageSound", "Message Sounds", "Direct Messenging", "checkbox");
  // GlassSettings.drawSetting("Live::MessageAnyone", "DM Anyone", "Direct Messenging", "checkbox");

  %settings = "RoomChatNotificationNew RoomChatSoundNew RoomMentionNotification RoomShowAwake MessageNotification MessageSound ShowTimestamps ShowJoinLeave StartupNotification StartupConnect ShowFriendStatus RoomNotification ConfirmConnectDisconnect PendingReminder";
  // removed: Live::RoomAutoJoin, Live::MessageAnyone

  for(%i = 0; %i < getWordCount(%settings); %i++) {
    %setting = getWord(%settings, %i);
    %box = "GlassModManagerGui_Prefs_" @ %setting;
    %box.setValue(GlassSettings.get("Live::" @ %setting));
  }
}

function GlassLive_keybind(%down) {
  if(%down) {
    if(!GlassOverlayGui.isAwake()) {
      GlassLive::openOverlay();
    } else {
      GlassLive::closeOverlay();
    }
  }
}

function GlassLive::openOverlay() {
  canvas.pushDialog(GlassOverlayGui);
  GlassNotificationManager.dismissAll();
}

function GlassLive::openModManager() {
  GlassLive::openOverlay();
  if(GlassModManagerGui.getCount() > 0) {
    GlassOverlayGui.add(GlassModManagerGui_Window);
    GlassModManagerGui_Window.forceCenter();
    GlassModManagerGui_Window.visible = false;
  }
  GlassModManagerGui_Window.setVisible(!GlassModManagerGui_Window.visible);

  GlassOverlayGui.pushToBack(GlassModManagerGui_Window);
}

function GlassLive::closeModManager() {
  GlassModManagerGui_Window.setVisible(false);
}

function GlassLive::openSettings() {
  if(isObject(GlassSettingsWindow)) {
    GlassSettingsWindow.setVisible(!GlassSettingsWindow.visible);
    GlassSettingsGui_ScrollOverlay.setVisible(true);
  }

  if(GlassSettingsWindow.visible) {
    GlassOverlayGui.pushToBack(GlassSettingsWindow);
  }
}

function GlassLive::openChatroom() {
  %chatFound = false;

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      %chatFound = true;

      %window.setVisible(!%window.visible);
    }
  }

  if(!%chatFound) {
    if(GlassSettings.get("Live::ConfirmConnectDisconnect")) {
      if(GlassLiveConnection.connected) {
      glassMessageBoxYesNo("Reconnect", "This will reconnect you to <font:verdana bold:13>Glass Live<font:verdana:13>, continue?", "GlassLive::disconnect(1); GlassLive.schedule(100, connectToServer);");
      } else {
      glassMessageBoxYesNo("Connect", "This will connect you to <font:verdana bold:13>Glass Live<font:verdana:13>, continue?", "GlassLive.schedule(0, connectToServer);");
      }
    } else {
      if(GlassLiveConnection.connected) {
        GlassLive::disconnect($Glass::Disconnect["Manual"]);
        GlassLive.schedule(100, connectToServer);
      } else {
        GlassLive.schedule(0, connectToServer);
      }
    }
  }
}

function GlassLive::closeSettings() {
  GlassSettingsWindow.setVisible(false);
}

function GlassLive::closeOverlay() {
  canvas.popDialog(GlassOverlayGui);
}

function GlassLive::updateSetting(%category, %setting) {
  %box = "GlassModManagerGui_Prefs_" @ %setting;
  GlassSettings.update(%category @ "::" @ %setting, %box.getValue());
  %box.setValue(GlassSettings.get(%category @ "::" @ %setting));

  if(strLen(%callback = GlassSettings.obj[%setting].callback)) {
    if(isFunction(%callback)) {
      call(%callback);
    }
  }
}

function GlassOverlayGui::onWake(%this) {
  %x = getWord(getRes(), 0);
	%y = getWord(getRes(), 1);
	GlassOverlay.resize(0, 0, %x, %y);

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      if(isObject(%window.activeTab)) {
        %tab = %window.activeTab;
        %tab.chattext.forceReflow();
        %tab.scrollSwatch.verticalMatchChildren(0, 2);
        %tab.scrollSwatch.setVisible(true);

        %tab.scroll.scrollToBottom();
      }
    }
    if(%window.getName() $= "GlassMessageGui") {
      %window.chattext.forceReflow();
      %window.scrollSwatch.verticalMatchChildren(0, 3);
      %window.scrollSwatch.setVisible(true);
      %window.scroll.scrollToBottom();
    }
  }

  if(!isObject(GlassOverlayResponder)) {
    new GuiTextEditCtrl(GlassOverlayResponder) {
      profile = "GuiTextEditProfile";
      position = "-100 -100";
      extent = "10 10";
      visible = 1;
    };
    GlassOverlayGui.add(GlassOverlayResponder);
  }

  GlassOverlayResponder.schedule(1, makeFirstResponder, true);



  //instantly close all notifications
}

function GlassLive::chatColorCheck(%this) {
  %room = GlassLiveRoom::getFromId(0);

  %room.pushText("<font:verdana bold:12><color:" @ %this.color_friend @  ">Friend: <font:verdana:12><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:12><color:" @ %this.color_self @  ">Self: <font:verdana:12><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:12><color:" @ %this.color_mod @  ">Moderator: <font:verdana:12><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:12><color:" @ %this.color_admin @  ">Admin: <font:verdana:12><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:12><color:" @ %this.color_bot @  ">Bot: <font:verdana:12><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:12><color:" @ %this.color_default @  ">User: <font:verdana:12><color:333333>rambling message", 0);
}

function GlassLive::disconnect(%reason) {
  GlassLive::cleanup();

  if(isObject(GlassLiveConnection)) {
    GlassLiveConnection.doDisconnect(%reason);
  }
}

function GlassLive::cleanup() {
  GlassLiveUsers.deleteAll();
  GlassLive.friendList = "";
  GlassFriendsGui_ScrollSwatch.deleteAll();
  GlassFriendsGui_ScrollSwatch.setVisible(true);

  if(isObject(GlassLiveRoomGroup)) {
    for(%i = 0; %i < GlassLiveRoomGroup.getcount(); %i++) {
      %room = GlassLiveRoomGroup.getObject(%i);
      if(isObject(%room.view)) {
        %room.view.deleteAll();
        %room.view.schedule(0, delete);
      }
    }

    GlassLiveRoomGroup.deleteAll();
  }

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      %window.deleteAll();
      %window.delete();
      %i--;
    } else if(%window.getName() $= "GlassGroupchatWindow" || %window.getName() $= "GlassMessageGui") {
      %window.deleteAll();
      %window.delete();
      %i--;
    }
  }

  GlassFriendsGui_ScrollSwatch.verticalMatchChildren(0, 5);
  GlassFriendsGui_ScrollSwatch.setVisible(true);
  GlassFriendsGui_ScrollSwatch.getGroup().setVisible(true);
}

function GlassLive::showUserStatus() {
  %str = "<font:verdana:15><color:333333><tab:110>";
  %val[%vals++] = "BLID\t9789";
  %val[%vals++] = "";
  %val[%vals++] = "Status\tOnline";
  %val[%vals++] = "Location\tCrown's Prison Escape";
  %val[%vals++] = "";
  %val[%vals++] = "Forum Account\t<a:forum.blockland.us>Scout31</a>";
  for(%i = 0; %i < %vals; %i++) {
    %line = %val[%i+1];
    if(%line $= "") {
      %str = %str @ "<br><br>";
    } else {
      %str = %str @ "<font:verdana bold:15>" @ getField(%line, 0) @ ":\t<font:verdana:15>" @ getField(%line, 1) @ "<br>";
    }
  }

  echo(%str);

  GlassUserStatus.setValue(%str);
}

function GlassLive::enterRoomDragMode(%obj, %pos) {
  if(isObject(GlassLiveDrag) || GlassLive.dragMode)
    return;

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      %window.setDropMode(true);
    }
  }

  new GuiMouseEventCtrl(GlassLiveDrag) {
    position = "0 0";
    extent = getRes();

    dragObj = %obj;
  };

  GlassOverlayGui.add(GlassLiveDrag);
  GlassOverlayGui.add(%obj);
  GlassLiveDrag.updatePosition(%pos);

  GlassLive.dragMode = true;
}

function GlassLiveDrag::onMouseDragged(%this, %a, %pos) {
  %this.updatePosition(%pos);
}

function GlassLiveDrag::onMouseUp(%this, %a, %pos) {
  %dropX = getWord(%pos, 0);
  %dropY = getWord(%pos, 1);

  //this loop is used to exit all windows from Drop Mode
  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() !$= "GlassChatroomWindow")
      continue;

    %window.setDropMode(false);
  }


  //this loop is used specifically to detect the whether to open a new window or add
  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() !$= "GlassChatroomWindow")
      continue;

    %posX = getWord(%window.position, 0);
    %posY = getWord(%window.position, 1);

    %extX = getWord(%window.extent, 0);
    %extY = getWord(%window.extent, 1);

    if(%dropX < %posX)
      continue;

    if(%dropX > %posX+%extX)
      continue;

    if(%dropY < %posY)
      continue;

    if(%dropY > %posY+%extY)
      continue;

    %newWindow = %window;
    break;
  }

  if(%newWindow $= "") {
    %newWindow = GlassLive::createChatroomWindow();
    %newWindow.position = %pos;

    %endPos = vectorAdd(%newWindow.position, %newWindow.extent);
    if(getWord(%endPos, 0) > getWord(getRes(), 0)) {
      %newWindow.position = setWord(%newWindow.position, 0, getWord(getRes(), 0)-getWord(%newWindow.extent, 0));
    }

    if(getWord(%endPos, 1) > getWord(getRes(), 1)) {
      %newWindow.position = setWord(%newWindow.position, 1, getWord(getRes(), 1)-getWord(%newWindow.extent, 1));
    }
  }

  %newWindow.addTab(%this.dragObj.tabObj);

  %newWindow.extent = %extX SPC %extY;

  %newWindow.schedule(0, resize, getWord(%newWindow.position, 0), getWord(%newWindow.position, 1), getWord(%newWindow.extent, 0), getWord(%newWindow.extent, 1));

  if(isObject(%this.dragObj))
    %this.dragObj.delete();

  %this.delete();

  GlassLive.dragMode = false;
}

function GlassLiveDrag::updatePosition(%this, %pos) {
  %xOffset = getWord(%this.dragObj.extent, 0)/2;
  %yOffset = getWord(%this.dragObj.extent, 1)/2;

  %this.dragObj.position = vectorSub(%pos, %xOffset SPC %yOffset);

  GlassOverlayGui.pushToBack(%this.dragObj);
  GlassOverlayGui.pushToBack(%this);
}

function GlassLive::joinGroupPrompt(%id) {
  glassMessageBoxYesNo("Join Group", "Do you want to join the group chat?", "GlassLive::joinGroup(" @ %id @ ");");
}

function GlassLive::joinGroup(%id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "groupJoin");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();
}

function wordPos(%str, %word) {
  for(%i = 0; %i < getWordCount(%str); %i++)
    if(getWord(%str, %i) $= %word)
      return %i;
  return -1;
}

function GlassLive::addFriendToList(%user) {
  if(wordPos(GlassLive.friendList, %user.blid) != -1) {
    return;
  }

  GlassLive.friendList = setWord(GlassLive.friendList, getWordCount(GlassLive.friendList), %user.blid);
}

function GlassLive::removeFriendFromList(%blid) {
  if((%i = wordPos(GlassLive.friendList, %blid)) == -1) {
    return;
  }

  GlassLive.friendList = removeWord(GlassLive.friendList, %i);
}

function GlassLive::addFriendRequestToList(%user) {
  if(wordPos(GlassLive.friendRequestList, %user.blid) != -1) {
    return;
  }

  GlassLive.friendRequestList = setWord(GlassLive.friendRequestList, getWordCount(GlassLive.friendRequestList), %user.blid);
}

function GlassLive::removeFriendRequestFromList(%blid) {
  if((%i = wordPos(GlassLive.friendRequestList, %blid)) == -1) {
    return;
  }

  GlassLive.friendRequestList = removeWord(GlassLive.friendRequestList, %i);
}

function GlassLive::checkPendingFriendRequests() {
  if(GlassSettings.get("Live::PendingReminder")) {
    if((%pending = getWordCount(GlassLive.friendRequestList)) > 0) {
      GlassNotificationManager::newNotification("Pending Friend Requests", "You have<font:verdana bold:13>" SPC %pending SPC "<font:verdana:13>pending friend request(s).", "new_email", 0, "GlassLive::openOverlay();");

      alxPlay(GlassBellAudio);
    }
  }
}

//================================================================
//= 3.2.0 things that we'll organize later                        =
//================================================================

function GlassLive::friendOnline(%this, %blid, %status) {
  %uo = GlassLiveUser::getFromBlid(%blid);

  GlassLive::createFriendList();

  %uo.setStatus(%status);

  if(GlassSettings.get("Live::ShowFriendStatus")) {
    if(%uo.getStatus() $= "online" || %uo.getStatus() $= "offline") {
      %online = (%uo.getStatus() $= "offline" ? false : true);
      %sound = (%online ? "GlassFriendOnlineAudio" : "GlassFriendOfflineAudio");

      alxPlay(%sound);
    }

    switch$(%uo.getStatus()) {
      case "online":
        %icon = "status_online";
      case "busy":
        %icon = "status_busy";
      case "away":
        %icon = "status_away";
      case "offline":
        %icon = "status_offline";
    }

    GlassNotificationManager::newNotification(%uo.username, "is now " @ %uo.getStatus() @ ".", %icon, 0);
  }
}

function GlassLive::setStatus(%status) {
  %status = strlwr(%status);

  if(%status $= "online" || %status $= "away" || %status $= "busy") {
    %obj = JettisonObject();
    %obj.set("type", "string", "setStatus");
    %obj.set("status", "string", %status);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

    %obj.delete();
  }
}

function GlassLive::setIcon(%icon) {
  if(!isFile("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ ".png"))
    return;

  %obj = JettisonObject();
  %obj.set("type", "string", "setIcon");
  %obj.set("icon", "string", %icon);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

//================================================================
//= Communication                                                =
//================================================================

function GlassLive::linkForumAccount(%url) {
  %obj = JettisonObject();
  %obj.set("type", "string", "linkForum");
  %obj.set("url", "string", %url);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::joinRoom(%id) {
  %room = GlassLiveRoom::getFromId(%id);

  if(isObject(GlassLive.room[%id]))
    return;

  if(isObject(%room.window)) {
    GlassOverlayGui.add(%room.window);
    GlassOverlayGui.pushToBack(%room.window);
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "roomJoin");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  // GlassChatroomWindow.schedule(0, openRoomBrowser);
}

function GlassLive::viewLocation(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "locationGet");
  %obj.set("target", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassFriendsGui_ScrollOverlay.setVisible(true);
  GlassFriendsGui_ScrollOverlay.deleteAll();
  %ml = new GuiMlTextCtrl() {
    profile = "GuiMlTextProfile";
    position = "20 20";
    extent = "170 210";
    text = "<font:verdana bold:15>" @ %blid @ "<font:verdana:12><br><br>Loading...";
  };
  GlassFriendsGui_ScrollOverlay.ml = %ml;
  GlassFriendsGui_ScrollOverlay.add(%ml);
}

function GlassLive::displayLocation(%data) {
 %ml = GlassFriendsGui_ScrollOverlay.ml;
 %user = GlassLiveUser::getFromBlid(%data.blid);

 %text = "<font:verdana bold:15>";
 %text = %text @ %user.username;
 %text = %text @ "<br><br>";
 %text = %text @ "<font:verdana bold:14>Activity: <font:verdana:14>";
 %text = %text @ %data.activity;
 %text = %text @ "<br><font:verdana bold:14>Location: <font:verdana:14>";
 %text = %text @ %data.location;

 %ml.setValue(%text);
}

function GlassLive::openDirectMessage(%blid, %username) {
  if(%blid < 0 || %blid $= "" || %blid == getNumKeyId()) {
    return false;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  if(%username $= "") {
    if(%user != false) { //this shouldn't happen
      %username = %user.username;
    } else {
      %username = "Blockhead" @ %blid;
    }
  }

  %gui = %user.getMessageGui();

  if(%gui == false) {
    %gui = GlassLive::createDirectMessageGui(%blid, %username);
    GlassLive.message[%blid] = %gui;
    %user.setMessageGui(%gui);

    GlassOverlayGui.add(%gui);
  } else {
    GlassOverlayGui.pushToBack(%gui);
  }

  return %gui;
}

function GlassLive::closeMessage(%blid) {
  %gui = GlassLive.message[%blid];
  GlassOverlayGui.remove(%gui);
  %gui.deleteAll();
  %gui.delete();

  %obj = JettisonObject();
  %obj.set("type", "string", "messageClose");
  %obj.set("target", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");


  if(GlassLive.typing[%blid]) {
    GlassLive::messageTypeEnd(%blid);
  }
}

function GlassLive::onMessage(%message, %username, %blid) {
  // TODO check friend, blocked, prefs, etc

  %timestamp = "[" @ getWord(getDateTime(), 1) @ "]";

  if(isFile(%file = "config/client/BLG/chat_log/DMs/" @ %blid @ "/" @ strReplace(getWord(getDateTime(), 0), "/", ".") @ ".txt")) {
    %fo = new FileObject();
    %fo.openForAppend(%file);
    %fo.writeLine(%timestamp @ " " @ %username @ ": " @ %message);
    %fo.close();
    %fo.delete();
  } else {
    %fo = new FileObject();
    %fo.openForWrite(%file);
    %fo.writeLine(%timestamp @ " Beginning chat log of " @ GlassLiveUser::getFromBlid(%blid).username @ " (" @ %blid @ ")");
    %fo.writeLine(%timestamp @ " " @ %username @ ": " @ %message);
    %fo.close();
    %fo.delete();
  }

  %gui = GlassLive::openDirectMessage(%blid, %username);

  GlassOverlayGui.pushToBack(%gui);

  for(%i = 0; %i < getWordCount(%message); %i++) {
    %word = getWord(%message, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0 || strpos(%word, "glass://") == 0) {
      %raw = %word;
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %message = setWord(%message, %i, %word);

      %obj = getUrlMetadata(%raw, "GlassLive::urlMetadata");
      %obj.context = "dm";
      %obj.blid = %blid;
      %obj.raw = %raw;
    }
    if(getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":") {
      %bitmap = strlwr(stripChars(%word, "[]\\/{};:'\"<>,./?!@#$%^&*-=+`~;"));
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:" @ %bitmap @ ">";
        %message = setWord(%message, %i, %word);
      } else {
        %word = " ";
        %message = setWord(%message, %i, %word);
      }
    }
  }

  GlassLive::setMessageTyping(%blid, false);

  %val = %gui.chattext.getValue();

  %msg = "<color:333333><font:verdana bold:12>" @ %username @ ":<font:verdana:12><color:333333> " @ %message;

  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = "<font:verdana:12><color:666666>" @ %timestamp SPC %msg;
  }

  if(%val !$= "") {
    %val = %val @ "<br>" @ %msg;
  } else {
    %val = %msg;
  }

  %gui.chattext.setValue(%val);
  if(%gui.isAwake()) {
    %gui.chattext.forceReflow();
  }
  %gui.scrollSwatch.verticalMatchChildren(0, 3);
  %gui.scrollSwatch.setVisible(true);

  %lp = %gui.getLowestPoint() - %gui.scroll.getLowestPoint();

  if(%lp >= -15) {
    %gui.scroll.scrollToBottom();
  }
}

function GlassLive::onMessageNotification(%message, %blid) {
  // TODO check friend, blocked, prefs, etc

  %timestamp = "[" @ getWord(getDateTime(), 1) @ "]";

  if(isFile(%file = "config/client/BLG/chat_log/DMs/" @ %blid @ "/" @ strReplace(getWord(getDateTime(), 0), "/", ".") @ ".txt")) {
    %fo = new FileObject();
    %fo.openForAppend(%file);
    %fo.writeLine(%timestamp SPC %message);
    %fo.close();
    %fo.delete();
  }

  %user = GlassLiveUser::getFromBlid(%blid);
  %gui = %user.getMessageGui();
  if(%gui == false) {
    return;
  }

  %val = %gui.chattext.getValue();

  %msg = "<color:666666><font:verdana:12>" @ %message;

  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = "<font:verdana:12><color:666666>" @ %timestamp SPC %msg;
  }

  if(%val !$= "") {
    %val = %val @ "<br>" @ %msg;
  } else {
    %val = %msg;
  }

  %gui.chattext.setValue(%val);
  if(%gui.isAwake()) {
    %gui.chattext.forceReflow();
  }
  %gui.scrollSwatch.verticalMatchChildren(0, 3);
  %gui.scrollSwatch.setVisible(true);

  %lp = %gui.getLowestPoint() - %gui.scroll.getLowestPoint();

  if(%lp >= -15) {
    %gui.scroll.scrollToBottom();
  }
}

function GlassLive::messageImagePreview(%blid, %url, %type) {
  %user = GlassLiveUser::getFromBlid(%blid);
  %gui = %user.getMessageGui();
  if(%gui == false)
    return;

  %val = %gui.chattext.getValue();
  %msg = "<br><br><br>";
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %offset = getWord(%gui.chattext.extent, 1);

  %gui.chattext.setValue(%val);
  %gui.chattext.forceReflow();
  %gui.scrollSwatch.verticalMatchChildren(0, 3);

  %swat = new GuiSwatchCtrl() {
    color = "200 220 255 255";
    extent = "100 42";
    position = 5 SPC (%offset+2);
  };
  %an = GlassModManagerGui::createLoadingAnimation();
  %swat.add(%an);
  %an.forceCenter();

  GlassLive::loadImagePreview(%swat, %url, %type);

  %gui.scrollSwatch.add(%swat);

  %gui.scrollSwatch.setVisible(true);
  // %gui.scroll.scrollToBottom();
}

function GlassLive::setMessageTyping(%blid, %typing) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(isObject(%user)) {
    %window = %user.getMessageGui();
    if(isObject(%window)) {
      if(%typing) {
        %window.typing.startAnimation();
        %window.typing.setVisible(true);
      } else {
        %window.typing.endAnimation();
        %window.typing.setVisible(false);
      }
    }
  }
}

function GlassLive::loadImagePreview(%swat, %url, %ext) {
  %method = "GET";
  %downloadPath = "config/client/cache/chat/" @ sha1(%url) @ "." @ %ext;
  %className = "GlassImagePreviewTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.swatch = %swat;
  %tcp.thumb = 1;
  %tcp.dlpath = %downloadPath;

  %swat.tcp = %tcp;
}

function GlassImagePreviewTCP::onDone(%this, %error) {
  %swatch = %this.swatch;
  %swatch.deleteAll();
  if(%error) {

  } else {
    %bit = %gui.flare = new GuiBitmapCtrl() {
      extent = vectorSub(%swatch.extent, "10 10");
      position = "5 5";
      bitmap = %this.dlpath;
    };
    %swatch.add(%bit);
  }
}

function GlassLive::sendRoomMessage(%msg, %id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomChat");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::sendRoomCommand(%msg, %id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomCommand");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::sendMessage(%blid, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "message");
  %obj.set("message", "string", %msg);
  %obj.set("target", "string", %blid);

  GlassLive.typing[%blid] = false;

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  if(GlassSettings.get("Live::MessageSound"))
    alxPlay(GlassUserMsgSentAudio);
}

function GlassLive::sendFriendRequest(%blid) {
  if(%blid == getNumKeyId())
    return;

  if((%blid+0 !$= %blid) || %blid < 0) {
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  if(wordPos(GlassLive.blockedList, %blid) != -1) {
    glassMessageBoxOk("Blocked", "You have blocked this user, unblock them before attempting to send a friend request.");
    return;
  }

  if(wordPos(GlassLive.friendRequestList, %blid) != -1) {
    GlassLive::friendAccept(%blid);
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "friendRequest");
  %obj.set("target", "string", %blid);

  glassMessageBoxOk("Friend Request Sent", "Friend request sent to BLID " @ %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::friendBlock(%blid) {
  return;

  %obj = JettisonObject();
  %obj.set("type", "string", "friendBlock");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

function GlassLive::friendUnblock(%blid) {
  return;

  %obj = JettisonObject();
  %obj.set("type", "string", "friendUnblock");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

function GlassLive::friendAccept(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendAccept");
  %obj.set("blid", "string", %blid);

  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user) {
    %user.isFriend = true;

    if(isObject(%room = GlassChatroomWindow.activeTab.room))
      %room.renderUserList();
  }

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

function GlassLive::friendDecline(%blid) {
  if((%i = wordPos(GlassLive.friendRequestList, %blid)) == -1) {
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "friendDecline");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  GlassLive.friendRequestList = removeWord(GlassLive.friendRequestList, %i);

  GlassLive::createFriendList();
}

function GlassLive::removeFriend(%blid, %silent) {
  if(%blid == getNumKeyId()) {
    return;
  }

  if(wordPos(GlassLive.friendList, %blid) == -1) {
    return;
  }

  if(!%silent) {
    %obj = JettisonObject();
    %obj.set("type", "string", "friendRemove");
    %obj.set("blid", "string", %blid);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

    %obj.delete();
  }

  GlassLive::removeFriendFromList(%blid);
  GlassLive::createFriendList();

  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user) {
    %user.isFriend = false;
    if(isObject(%user.window))
      %user.window.delete();
    if(isObject(%room = GlassChatroomWindow.activeTab.room))
      %room.renderUserList();
  }
}

function GlassLive::updateLocation(%inServer) {

  if(!%inServer) {
    %action = "idle";
  } else if(ServerConnection.isLocal()) {
    if($Server::LAN) {
      if($Server::ServerType $= "Singleplayer") {
        %action = "singleplayer";
      } else {
        %action = "lan_hosting";
      }
    } else {
      %action = "hosting";
    }
  } else {
    %ip = ServerConnection.getRawIP();
    if(strPos("192.168.", %ip) == 0 || strpos("10.", %ip) == 0) {
      %action = "playing_lan";
    } else {
      %action = "playing";
    }
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "locationUpdate");
  %obj.set("action", "string", %action);

  if(%action $= "playing") {
    %location = ServerConnection.getRawIP() SPC ServerConnection.getPort();
    %obj.set("location", "string", %location);
  }

  //echo(jettisonStringify("object", %obj));

  if(isObject(GlassLiveConnection)) {
    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }
}

function GlassLive::urlMetadata(%tcp, %error) {
  if(!%error) {
    %ctx = %tcp.context;
    if(%ctx $= "dm") {
      %blid = %tcp.blid;
      if(%tcp.header["Content-Type"] $= "image/png" || %tcp.header["Content-Type"] $= "image/jpg" || %tcp.header["Content-Type"] $= "image/jpeg") {
        %ext = getsubstr(%tcp.header["Content-Type"], strpos(%tcp.header["Content-Type"], "/")+1, strlen(%tcp.header["Content-Type"]));
        GlassLive::onMessageNotification(%tcp.raw, %blid);
        GlassLive::messageImagePreview(%blid, %tcp.raw, %ext);
      }
    }
  }
}

//================================================================
//= Gui Population                                               =
//================================================================

function GlassLive::powerButtonPress() {
  %btn = GlassFriendsGui_PowerButton;

  if(%btn.on) {
    if(GlassSettings.get("Live::ConfirmConnectDisconnect"))
      glassMessageBoxYesNo("Disconnect", "Are you sure you want to disconnect from <font:verdana bold:13>Glass Live<font:verdana:13>?", "GlassLive::disconnect(1);");
    else
      GlassLive::disconnect($Glass::Disconnect["Manual"]);
  } else {
    GlassLive::connectToServer();
  }
}

function GlassLive::setPowerButton(%bool) {
  %btn = GlassFriendsGui_PowerButton;
  %btn.on = %bool;
  if(%btn.on)
    %btn.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn_poweroff");
  else
    %btn.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn_poweron");
}

function GlassLive::openAddDlg() {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to <font:verdana bold:13>Glass Live<font:verdana:13> to add friends.");
    return;
  }

  %gui = GlassFriendsGui_AddFriendBLID.getGroup();

  if(!%gui.visible) {
    %gui.setVisible(true);
    GlassFriendsGui_ScrollOverlay.setVisible(true);
  } else {
    GlassLive::addDlgClose();
  }
}

function GlassLive::addDlgSubmit() {
  if(GlassFriendsGui_AddFriendBLID.getValue()+0 !$= GlassFriendsGui_AddFriendBLID.getValue() || GlassFriendsGui_AddFriendBLID.getValue() < 0) {
    GlassFriendsGui_AddFriendBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  if(GlassFriendsGui_AddFriendBLID.getValue() == getNumKeyId()) {
    GlassFriendsGui_AddFriendBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "You can't friend yourself.");
    return;
  }

  GlassLive::sendFriendRequest(GlassFriendsGui_AddFriendBLID.getValue());
  GlassFriendsGui_AddFriendBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_AddFriendBLID.setValue("");
}

function GlassLive::addDlgClose() {
  GlassFriendsGui_AddFriendBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_AddFriendBLID.setValue("");
}

function GlassLive::chatroomInputSend(%id) {
  %room = GlassLiveRoom::getFromId(%id);
  if(%room == false) {
    error("Trying to send input in inactive room");
    return;
  }

  %chatroom = %room.view;
  %val = trim(%chatroom.input.getValue());
  %val = stripMlControlChars(%val);
  if(%val $= "") {
    %chatroom.input.setValue("");
    return;
  }

  if(strPos(%val, "/") != 0) {
    GlassLive::sendRoomMessage(%val, %id);
  } else {
    GlassLive::sendRoomCommand(%val, %id);
  }

  %chatroom.input.setValue("");
  %chatroom.scroll.schedule(100, "scrollToBottom");
}

function GlassLive::messageType(%blid) {
  if(GlassLive.typing[%blid]) {
    cancel(GlassLive.typingSched);
  } else {
    %obj = JettisonObject();
    %obj.set("type", "string", "messageTyping");
    %obj.set("target", "string", %blid);
    %obj.set("typing", "string", "1");
    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
    GlassLive.typing[%blid] = 1;
  }

  GlassLive.typingSched = schedule(2500, 0, eval, "GlassLive::messageTypeEnd(" @ %blid @ ");");
}

function GlassLive::messageTypeEnd(%blid) {
  cancel(GlassLive.typingSched);

  %obj = JettisonObject();
  %obj.set("type", "string", "messageTyping");
  %obj.set("target", "string", %blid);
  %obj.set("typing", "string", "0");

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassLive.typing[%blid] = 0;
}

function GlassLive::messageInputSend(%id) {
  %gui = GlassLive.message[%id];
  %val = trim(%gui.input.getValue());
  %val = stripMlControlChars(%val);
  if(%val $= "") {
    %gui.input.setValue("");
    return;
  }

  GlassLive::sendMessage(%id, %val);
  GlassLive::onMessage(%val, $Pref::Player::NetName, %id);
  %gui.input.setValue("");
  %gui.scroll.schedule(100, "scrollToBottom");
}

function GlassHighlightMouse::onMouseMove(%this, %a, %pos) {
  if(%this.skip) {
    %this.skip = !%this.skip;
    return; // save those frames
  }

  %gui = %this.getGroup();
  %pos = vectorSub(%pos, %gui.getCanvasPosition());
  if(!isObject(%gui.flare)) {
    %gui.flare = new GuiBitmapCtrl() {
      extent = "256 256";
      bitmap = "Add-Ons/System_BlocklandGlass/image/gui/glare.png";
      mcolor = "255 255 255 90";
      //mMultiply = 1;
      overflowImage = 1;
    };
    %gui.add(%gui.flare);
    %gui.bringToFront(%gui.flare);
  }

  %gui.flare.setVisible(true);
  %gui.flare.extent = "256 256";

  %gui.flare.position = vectorSub(%pos, "128 128");
  %gui.pushToBack(%this);
}

function GlassHighlightMouse::onMouseLeave(%this) {
  if(!%this.enabled)
    return;

  if(isObject(%this.getGroup().flare))
    %this.getGroup().flare.setVisible(false);

  %this.getGroup().color = %this.getGroup().ocolor;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(false);
    %this.getGroup().accept.setVisible(false);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(false);
  } else if(%this.online) {
    %this.getGroup().chaticon.setVisible(false);
  }

  %this.scrollEnd(%this.getGroup().text);
}

function GlassHighlightMouse::onMouseEnter(%this) {
  if(!%this.enabled)
    return;

  %this.getGroup().ocolor = %this.getGroup().color;
  %this.getGroup().color = %this.getGroup().hcolor;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(true);
    %this.getGroup().accept.setVisible(true);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(true);
  } else if(%this.online) {
    %this.getGroup().chaticon.setVisible(true);
  }

  if(getWord(%this.getGroup().text.extent, 0) > getWord(vectorSub(%this.extent, %this.pos), 0)-20)
    if(%this.scrollTick $= "")
      %this.scrollTick = %this.scrollLoop(%this.getGroup().text, true);
}

function GlassHighlightMouse::scrollLoop(%this, %text, %reset) {
  %icon = %text.getGroup().icon;

  if(%reset) {
    %this._scrollOrigin = %text.position;
    if(isObject(%icon))
      %this._scrollOrigin_Icon = %icon.position;
    %this._scrollOffset = 0;
    %this._scrollRange = getWord(%text.extent, 0)-getWord(%this.extent, 0)+getWord(%text.position, 0)+50;
  }

  %text.position = vectorSub(%this._scrollOrigin, %this._scrollOffset);
  if(isObject(%icon))
    %icon.position = vectorSub(%this._scrollOrigin_Icon, %this._scrollOffset);
  
  if(%this._scrollOffset >= %this._scrollRange) {
    %this._scrollOffset = 0;
    // %this.scrollTick = %this.schedule(2000, scrollLoop, %text);
  } else {
    %this._scrollOffset++;
    %this.scrollTick = %this.schedule(25, scrollLoop, %text);
  }
}

function GlassHighlightMouse::scrollEnd(%this, %text) {
  cancel(%this.scrollTick);
  %text.position = %this._scrollOrigin;

  %icon = %text.getGroup().icon;
  if(isObject(%icon))
    %icon.position = %this._scrollOrigin_Icon;

  %this.scrollTick = "";
}

function GlassHighlightMouse::onMouseDown(%this) {
  if(%this.online) {
    %this.down = 1;
  }
}

function GlassHighlightMouse::onMouseUp(%this, %a, %pos) {
  %pos = vectorSub(%pos, %this.getCanvasPosition());
  if(%this.type $= "request") {
    if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
      glassMessageBoxOk("Declined", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been declined.");
      GlassLive::friendDecline(%this.blid);
    } else if(getWord(%pos, 0) > getWord(%this.extent, 0)-50) {
      glassMessageBoxOk("Added", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been added.");
      GlassLive::friendAccept(%this.blid);
    // } else {
      // GlassLive::openUserWindow(%this.blid);
    }
  } else if(%this.type $= "blocked") {
    if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
      glassMessageBoxOk("Unblocked", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been unblocked.");
      GlassLive::friendUnblock(%this.blid);
    }
  } else if(%this.type $= "toggle") {
    eval(%this.toggleVar @ " = !" @ %this.toggleVar @ ";");
    GlassLive::createFriendList();
  } else {
    if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
      if(%this.online)
        GlassLive::openDirectMessage(%this.blid);
    } else {
      GlassLive::openUserWindow(%this.blid);
    }
  }
}

//================================================================
//= Gui Creation                                                 =
//================================================================

function GlassLive::addFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user)
    glassMessageBoxYesNo("Add Friend", "Add <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> as a friend?", "GlassLive::sendFriendRequest(" @ %user.blid @ ");");
}

function GlassLive::removeFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user)
    glassMessageBoxYesNo("Remove Friend", "Remove <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> as a friend?", "GlassLive::removeFriend(" @ %user.blid @ ");");
}

function GlassLive::openUserWindow(%blid) {
  %uo = GlassLiveUser::getFromBlid(%blid);
  if(%uo) {
    %window = GlassLive::createUserWindow(%uo);

    if(!isObject(%window)) {
      return;
    }

    switch$(%uo.getStatus()) {
      case "online":
        %status = "<color:33CC33>Online";
      case "busy":
        %status = "<color:FF3300>Busy";
      case "away":
        %status = "<color:FF751A>Away";
      case "offline":
        %status = "<color:404040>Offline";
      default:
        %status = "<color:404040>Unknown";
    }

    %text = "<font:verdana bold:13>" @ %uo.username @ "<br><font:verdana:12>" @ %uo.blid @ "<br><br><font:verdana bold:12>" @ %status;
    %window.text.setValue(%text);
    %window.text.forceReflow();
    %window.text.centerY();

    if(%uo.isFriend()) {
      %window.friendButton.mcolor = "255 200 200 200";
      %window.friendButton.command = "GlassLive::removeFriendPrompt(" @ %uo.blid @ ");";
      %window.friendButton.text = "Unfriend";
    } else {
      %window.friendButton.mcolor = "200 255 200 200";
      %window.friendButton.command = "GlassLive::addFriendPrompt(" @ %uo.blid @ ");";
      %window.friendButton.text = "Add Friend";
    }

    %window.messageButton.enabled = true;

    %window.forceCenter();
  }
}

function GlassLive::createUserWindow(%uo) {
  if(isObject(%uo.window)) {
    // GlassOverlayGui.pushToBack(%uo.window);
    %uo.window.delete();
    return;
  }

  %window = new GuiWindowCtrl() {
    profile = "GlassWindowProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "235 157";
    extent = "170 176";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Glass User";
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
  };

  %window.textcontainer = new GuiSwatchCtrl(GlassUserTextContainer) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 35";
    extent = "150 60";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "220 220 220 255";
  };

  %window.text = new GuiMLTextCtrl(GlassUserText) {
   profile = "GuiMLTextProfile";
   horizSizing = "right";
   vertSizing = "bottom";
   position = "10 10";
   extent = "130 14";
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
  };

  %window.friendButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 135";
    extent = "150 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Unfriend";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 200 200 200";
  };

  %window.messageButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 100";
    extent = "150 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Message";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";
    command = "GlassLive::openDirectMessage(" @ %uo.blid @ "); if(isObject(" @ GlassLiveUser::getFromBlid(%uo.blid) @ ".getMessageGui())){" @ GlassLiveUser::getFromBlid(%uo.blid) @ ".getMessageGui().forceCenter();}";
  }; // move to a function ^^

  %window.add(%window.textcontainer);
  %window.textcontainer.add(%window.text);
  %window.add(%window.messageButton);
  %window.add(%window.friendButton);

  %window.closeCommand = %window.getId() @ ".delete();";

  GlassOverlayGui.add(%window);
  %uo.window = %window;
  return %window;
}

function GlassLive::createChatroomWindow() {
  %chatroom = new GuiWindowCtrl(GlassChatroomWindow) {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 130";
    extent = "568 460";
    minExtent = "568 460";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Phantom Chatroom Window";
    maxLength = "255";
    resizeWidth = "1";
    resizeHeight = "1";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
    closeCommand = "%this.exit();";

    tabs = 0;
  };

  %chatroom.closeCommand = %chatroom.getId() @ ".exitTab();";

  %chatroom.resize = new GuiMLTextCtrl(GlassChatroomResize) {
    profile = "GuiMLTextProfile";
    horizSizing = "relative";
    vertSizing = "relative";
    position = "0 0";
    extent = %chatroom.extent;
    visible = "0";
  };

  %chatroom.tabSwatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 35";
    extent = "445 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %chatroom.dropText = new GuiMLTextCtrl() {
     profile = "GuiMLTextProfile";
     horizSizing = "center";
     vertSizing = "center";
     position = "0 135";
     extent = "465 12";
     minExtent = "8 2";
     enabled = "1";
     visible = "0";
     clipToParent = "1";
     lineSpacing = "2";
     allowColorChars = "0";
     maxChars = "-1";
     maxBitmapHeight = "-1";
     selectable = "1";
     autoResize = "1";
     text = "<font:verdana:15><just:center>Add to Window";
  };

  %chatroom.add(%chatroom.resize);
  %chatroom.add(%chatroom.tabSwatch);
  %chatroom.add(%chatroom.dropText);
  GlassOverlayGui.add(%chatroom);
  return %chatroom;
}

function GlassChatroomWindow::addTab(%this, %tabObj) {
  if(%this.tabId[%tabObj] !$= "" && %tabObj.window.getId() == %this.getId()) {
    %this.renderTabs();
    %this.openTab(%this.tabId[%tabObj]);
    return;
  }

  if(%this.isAwake()) {
    %this.schedule(0, resize, getWord(%this.position, 0), getWord(%this.position, 1), getWord(%this.extent, 0), getWord(%this.extent, 1));
  }

  if(isObject(%tabObj.window)) {
    %tabObj.window.removeTab(%tabObj);
  }

  %this.tab[%this.tabs] = %tabObj;
  %this.tabId[%tabObj] = %this.tabs;
  %this.tabs++;

  %this.add(%tabObj);

  %tabObj.window = %this;

  %this.setTabsVisible(true);
  %this.renderTabs();

  //always open to the added tab
  %this.openTab(%this.tabId[%tabObj]);
}

function GlassChatroomWindow::removeTab(%this, %tabObj) {
  %id = %this.tabId[%tabObj];
  if(%id $= "") {
    return false;
  }

  %this.removeTabId(%id);
}

function GlassChatroomWindow::removeTabId(%this, %id) {
  %tabObj = %this.tabId[%id];
  %tabObj.window = "";

  %this.tab[%id] = "";
  %this.tabId[%tabObj] = "";
  %this.tabButton[%id].delete();

  for(%i = %id; %i < %this.tabs; %i++) {
    %o = %this.tab[%i+1];

    %this.tabId[%o] = %i;
    %this.tab[%i] = %o;

    if(isObject(%this.tabButton[%i]))
      %this.tabButton[%i].delete();
  }

  %this.tabs--;

  if(!%this.tabs) {
    %this.schedule(0, delete);
  } else {
    if(%this.activeTabId == %id) {
      if(%id >= %this.tabs)
        %this.openTab(%this.tabs-1);
      else
        %this.openTab(%id);
    } else if(%this.activeTabId >= %id) {
      %this.activeTabId--;
    }
    %this.renderTabs();
  }
}

function GlassChatroomWindow::exitTab(%this) {
  %tab = %this.activeTab;
  if(isObject(%tab)) {
    glassMessageBoxYesNo("Leave Room", "Are you sure you want to leave <font:verdana bold:13>" @ %tab.title @ "<font:verdana:15>?", %tab.room.getId() @ ".leaveRoom();");
  }
}

function GlassChatroomWindow::createTabButton(%this, %i, %width) {
  %tabObj = %this.tab[%i];

  %button = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = %width SPC 25;
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = %tabObj.title;
    groupNum = "-1";
    buttonType = "PushButton";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";

    tabObj = %tabObj;
  };

  %button.text = new GuiTextCtrl(GlassCRText) {
    horizSizing = "center";
    vertSizing = "center";
    profile = "GlassChatroomTabProfile";
    text = %tabObj.title;
    extent = %width SPC 18;
    minextent = %width SPC 18;
  };

  %button.mouseCtrl = new GuiMouseEventCtrl(GlassChatroomTabMouse) {
    image = %button;
    extent = %button.extent;
    position = "0 0";
    baseBitmap = (%i == %this.activeTabId ? "base/client/ui/tab1use" : "base/client/ui/tab1");
    extension = "n";

    command = %this.getId() @ ".openTab(" @ %i @ ");";

    tabObj = %tabObj;
  };

  %button.mouseCtrl.render();

  %button.add(%button.text);
  %button.add(%button.mouseCtrl);

  %button.text.forceCenter();

  return %button;
}

function GlassChatroomWindow::createAddTabButton(%this) {
  %button = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";

    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = 30 SPC 25;
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "+";
    groupNum = "-1";
    buttonType = "PushButton";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";

    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/tabAdd1";

    command = %this.getId() @ ".openRoomBrowser();";
  };
  return %button;
}

function GlassChatroomTabMouse::setUse(%this, %bool) {
  %this.baseBitmap = (%bool ? "base/client/ui/tab1use" : "base/client/ui/tab1");
  %this.render();
}

function GlassChatroomTabMouse::onMouseEnter(%this) {
  %this.extension = "h";
  %this.render();
}

function GlassChatroomTabMouse::onMouseLeave(%this, %a, %b, %c) {
  %this.extension = "n";
  %this.render();
}

function GlassChatroomTabMouse::onMouseDown(%this, %a, %pos) {
  %this.down = true;
  %this.downPoint = %pos;

  %this.extension = "d";
  %this.render();
}

function GlassChatroomTabMouse::onMouseDragged(%this, %a, %pos, %c) {
  %dist = vectorDist(%pos, %this.downPos);
  if(%dist > 20) {
    GlassLive::enterRoomDragMode(%this.image, %pos);
  }
}

function GlassChatroomTabMouse::onMouseUp(%this) {
  %this.extension = "n";
  %this.render();


  eval(%this.command);
}

function GlassChatroomTabMouse::render(%this) {
  %button = %this.image;
  %button.setBitmap(%this.baseBitmap @ "_" @ %this.extension);
}

function GlassChatroomWindow::renderTabs(%this) {
  %swatch = %this.tabSwatch;
  %swatch.deleteAll();

  %x = 0;
  %width = 140;
  for(%i = 0; %i < %this.tabs; %i++) {
    %tabObj = %this.tab[%i];
    if(isObject(%this.tabButton[%i])) {
      %this.tabButton[%i].delete();
    }

    %this.tabButton[%i] = %this.createTabButton(%i, %width);
    %this.tabButton[%i].position = %x SPC 0;
    %x += %width;

    %swatch.add(%this.tabButton[%i]);
    %tabObj.tabButton = %this.tabButton[%i];
  }

  %this.tabAddButton = %this.createAddTabButton();
  %this.tabAddButton.position = %x SPC 0;
  %swatch.add(%this.tabAddButton);
}

function GlassChatroomWindow::setTabsVisible(%this, %toggle) {
  if(%toggle) {
    %position = "0 60";
    %extent = "455 290";
  } else {
    %position = "0 35";
    %extent = "475 265";
  }

  %this.extent = %extent;

  for(%i = 0; %i < %this.tabs; %i++) {
    %tabObj = %this.tab[%i];

    %tabObj.position = %position;
  }

  %this.tabSwatch.setVisible(%toggle);
}

function GlassChatroomWindow::openTab(%this, %id) {
  %current = %this.activeTab;
  %currentId = %this.activeTabId;
  if(isObject(%current)) {
    %button = %this.tabButton[%currentId];
    if(isObject(%button))
      %button.mouseCtrl.setUse(false);

    %current.setVisible(false);
    // %current.room.setAwake(false);
  }

  if(isObject(%this.browserSwatch)) {
    %this.browserSwatch.delete();
  }

  %tab = %this.tab[%id];
  %this.activeTabId = %id;
  if(isObject(%tab)) {
    %this.activeTab = %tab;
    %tab.setVisible(true);

    %button = %this.tabButton[%id];
    if(isObject(%button))
      %button.mouseCtrl.setUse(true);

    if(%tab.getName() $= "GlassChatroomTab") {
      %this.text = "Chatroom - " @ %tab.title; // @ " - " @ %this.getId();
      %this.setText(%this.text);

      // %tab.room.setAwake(true);
      %tab.setFlashing(false);
    } else {
      %this.text = "Groupchat - " @ %tab.title;
      %this.setText(%this.text);
    }

    if(%this.isAwake()) {
      %tab.chattext.forceReflow();
      %tab.scrollSwatch.verticalMatchChildren(0, 2);
      %tab.scrollSwatch.setVisible(true);
      %tab.scroll.scrollToBottom();
      //%tab.renderUserList();
    }
  }

  if(%this.isAwake()) {
    %this.resize.schedule(0, onResize);
  }
}

function GlassChatroomWindow::setDropMode(%this, %bool) {
  if(%bool) {
    for(%i = 0; %i < %this.getCount(); %i++) {
      %this.getObject(%i).setVisible(false);
    }

    %this.dropText.setVisible(true);
  } else {
    %this.activeTab.setVisible(true);
    %this.tabSwatch.setVisible(true);
    %this.dropText.setVisible(false);
  }
}

function chatroomAwakeCallback(%callback) {
  return;

  if(isObject(GlassChatroomWindow) && isObject(GlassChatroomWindow.activeTab)) {
    %bool = GlassSettings.get(%callback) ? true : false;

    GlassChatroomWindow.activeTab.room.setAwake(%bool);
  }
}

// function GlassChatroomWindow::onWake(%this) {
  // if(isObject(%this.activeTab)) {
    // %this.activeTab.room.setAwake(true);
  // }
// }

// function GlassChatroomWindow::onSleep(%this) {
  // if(isObject(%this.activeTab)) {
    // %this.activeTab.room.setAwake(false);
  // }
// }

function GlassChatroomWindow::openRoomBrowser(%this, %rooms) {
  if(!isObject(%rooms)) {
    GlassLive.pendingRoomList = %this;
    GlassLiveConnection.placeCall("getRoomList");
    return;
  }

  %this.openTab(-1);

  %browserSwatch = new GuiSwatchCtrl(GlassRoomBrowser) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 60";
    extent = "455 220";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "230 230 230 255";
  };

  %swatch = new GuiSwatchCtrl(GlassRoomBrowser) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "435 26";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "100 100 100 255";

    id = %room.id;
  };

  %swatch.text = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    position = "25 6";
    text = "<tab:200><font:verdana bold:13><color:eeeeee>Name\tUsers";
    extent = "445 16";
  };

  %swatch.add(%swatch.text);
  %browserSwatch.add(%swatch);
  %last = %swatch;

  for(%i = 0; %i < %rooms.length; %i++) {
    %room = %rooms.value[%i];
    %swatch = new GuiSwatchCtrl(GlassRoomBrowser) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "435 26";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "210 210 210 255";

      id = %room.id;
    };

    %swatch.image = new GuiBitmapCtrl() {
      profile = "GuiDefaultProfile";
      extent = "16 16";
      position = "5 5";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %room.image;
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      position = "25 6";
      text = "<tab:200><font:verdana bold:13><color:333333>" @ %room.title TAB %room.users;
      extent = "405 16";
    };

    %swatch.button = new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonProfile";
      extent = "48 18";
      position = "365 4";
      bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
      text = "Join";
      visible = !isObject(GlassLive.room[%room.id]);
      command = "GlassLive::joinRoom(" @ %room.id @ ");";
    };

    %swatch.add(%swatch.image);
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.button);

    if(%i > 0)
      %swatch.placeBelow(%last, 5);
    else
      %swatch.placeBelow(%last, 10);

    %browserSwatch.add(%swatch);

    %last = %swatch;
  }

  if(isObject(%this.browserSwatch)) {
    %this.browserSwatch.deleteAll();
    %this.browserSwatch.delete();
  }

  %this.browserSwatch = %browserSwatch;
  %this.add(%this.browserSwatch);
}

function GlassChatroomTab::setFlashing(%this, %bool) {
  if(isObject(%this)) {
    if(%bool) {
      if(%this.visible)
        return;

      %this.flashSchedule = %this.schedule(0, flashTick, 1);
    } else {
      if(isObject(%this.tabButton))
        %this.tabButton.mColor = "255 255 255 200";
      cancel(%this.flashSchedule);
    }
  }
}

function GlassChatroomTab::flashTick(%this, %bool) {
  cancel(%this.flashSchedule);

  %button = %this.tabButton;
  if(isObject(%button)) {
    if(%bool) {
      %button.mColor = "255 60 60 200";
    } else {
      %button.mColor = "255 255 255 200";
    }
  }

  %this.flashSchedule = %this.schedule(500, flashTick, !%bool);
}

function GlassChatroomResize::onResize(%this, %x, %y, %h, %l) {
  %window = %this.getGroup();
  %extent = %window.extent;
  %position = %window.position;
  %activeTab = %window.activeTab;
  %tabSwatch = %window.tabSwatch;
  %browserSwatch = %window.browserSwatch;

  %scrollSwatch = %activeTab.scrollSwatch;
  %userSwatch = %activeTab.userswatch;
  %input = %activeTab.input;
  %scroll = %activeTab.scroll;
  %tabButton = %activeTab.tabButton;
  %userScroll = %activeTab.userscroll;
  %chatText = %activeTab.chattext;

  %activeTab.extent = getWord(%extent, 0) - 10 SPC getWord(%extent, 1) - 68;
  %tabSwatch.extent = getWord(%extent, 0) - 20 SPC getWord(%tabSwatch.extent, 1);
  %scroll.extent = getWord(%extent, 0) - 150 SPC getWord(%extent, 1) - 90;
  %scrollSwatch.extent = getWord(%scroll.extent, 0) SPC getWord(%scroll.extent, 1);
  %chatText.extent = getWord(%scroll.extent, 0) - 15 SPC getWord(%chatText.extent, 1);
  %userScroll.extent = getWord(%userScroll.extent, 0) SPC getWord(%extent, 1) - 90;
  %userScroll.position = getWord(%scroll.extent, 0) + 15 SPC getWord(%userScroll.position, 1);
  %input.extent = getWord(%extent, 0) - 150 SPC getWord(%input.extent, 1);
  %input.position = getWord(%input.position, 0) SPC getWord(%scroll.extent, 1) + 5;

  if(isObject(%browserSwatch)) {
    %browserSwatch.extent = getWord(%extent, 0) - 20 SPC getWord(%extent, 1) - 70;

    for(%i = 0; %i < %browserSwatch.getCount(); %i++) {
      %obj = %browserSwatch.getObject(%i);
      %obj.extent = getWord(%browserSwatch.extent, 0) - 20 SPC getWord(%obj.extent, 1);
      if(%obj.getCount() > 2) {
        %btn = %obj.getObject(2);
        if(%btn.buttonType $= "PushButton") {
          %btn.position = getWord(%obj.extent, 0) - 75 SPC getWord(%btn.position, 1);
        }
      }
    }
  }

  %scroll.scrollToBottom();

  %scrollSwatch.verticalMatchChildren(0, 2);
  %scrollSwatch.setVisible(true);

  // %userSwatch.getGroup().scrollToTop();
  %userSwatch.setVisible(true);
}

function GlassLive::createChatroomView(%id) {
  %chatroom = new GuiSwatchCtrl(GlassChatroomTab) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 35";
    extent = "450 215";
    minExtent = "8 2";
    enabled = "1";
    visible = "0";
    clipToParent = "1";
    color = "0 0 0 0";
    id = %id;
  };

  %chatroom.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 0";
    extent = "315 200";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
    id = %id;
  };

  %chatroom.scrollSwatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 -152";
    extent = "304 351";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %chatroom.chattext = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "3 0";
    extent = "300 351";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "12";
    selectable = "1";
    autoResize = "1";
  };

  %chatroom.userscroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "330 0";
    extent = "125 200";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
  };

  %chatroom.userswatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 -152";
    extent = "304 351";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %chatroom.input = new GuiTextEditCtrl(GlassChatroomGui_Input) {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 205";
    extent = "315 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    altCommand = "GlassLive::chatroomInputSend(" @ %id @ ");";
    accelerator = "enter";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "1";
    sinkAllKeyEvents = "0";
  };
  %chatroom.add(%chatroom.scroll);
  %chatroom.scroll.add(%chatroom.scrollSwatch);
  %chatroom.scrollSwatch.add(%chatroom.chattext);
  %chatroom.add(%chatroom.userscroll);
  %chatroom.userscroll.add(%chatroom.userswatch);
  %chatroom.add(%chatroom.input);

  return %chatroom;
}

function GlassLive::createGroupchatView(%id) {
  %chatroom = new GuiSwatchCtrl(GlassGroupchatTab) {
    class = "GlassChatroomTab";

    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 35";
    extent = "460 225";
    minExtent = "8 2";
    enabled = "1";
    visible = "0";
    clipToParent = "1";
    color = "0 0 0 0";
    id = %id;
  };

  %chatroom.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 0";
    extent = "295 180";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
    id = %id;
  };

  %chatroom.scrollSwatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 -152";
    extent = "284 351";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %chatroom.chattext = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "3 0";
    extent = "300 351";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "12";
    selectable = "1";
    autoResize = "1";
  };

  %chatroom.userscroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "310 0";
    extent = "125 205";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
  };

  %chatroom.userOverlay = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "311 1";
    extent = "123 178";
    minExtent = "8 2";
    enabled = "1";
    visible = "0";
    clipToParent = "1";
    color = "255 255 255 200";
  };

  %chatroom.userswatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 -152";
    extent = "304 351";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %chatroom.input = new GuiTextEditCtrl(GlassChatroomGui_Input) {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 185";
    extent = "295 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    altCommand = "GlassLiveGroup::inputSend(" @ %id @ ");";
    accelerator = "enter";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "1";
    sinkAllKeyEvents = "0";
  };

  %chatroom.addbtn = new GuiBitmapButtonCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "415 180";
     extent = "24 24";
     minExtent = "8 2";
     enabled = "1";
     visible = "0";
     clipToParent = "1";
     command = "GlassLive::openAddDlg();";
     groupNum = "-1";
     buttonType = "PushButton";
     bitmap = "~/System_BlocklandGlass/image/gui/btn_add";
     text = "";
     lockAspectRatio = "0";
     alignLeft = "0";
     alignTop = "0";
     overflowImage = "0";
     mKeepCached = "0";
     mColor = "255 255 255 255";
     on = "1";
  };

  %chatroom.add(%chatroom.scroll);
  %chatroom.scroll.add(%chatroom.scrollSwatch);
  %chatroom.scrollSwatch.add(%chatroom.chattext);
  %chatroom.add(%chatroom.userscroll);
  %chatroom.userscroll.add(%chatroom.userswatch);
  %chatroom.add(%chatroom.userOverlay);
  %chatroom.add(%chatroom.input);
  %chatroom.add(%chatroom.addbtn);

  return %chatroom;
}

function GlassLive::createDirectMessageGui(%blid, %username) {
  %dm = new GuiWindowCtrl(GlassMessageGui) {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 131";
    extent = "270 180";
    minExtent = "270 180";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Message - " @ %username @ " (" @ %blid @ ")";
    maxLength = "255";
    resizeWidth = "1";
    resizeHeight = "1";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    closeCommand = "GlassLive::closeMessage(" @ %blid @ ");";
  };

  %titleLen = strLen(%dm.text);

  if(%titleLen > 25) {
    %dm.extent = %titleLen * 10.75 SPC 180; // close enough
    // %dm.minExtent = %dm.extent;
  }

  %dm.resize = new GuiMLTextCtrl(GlassMessageResize) {
    profile = "GuiMLTextProfile";
    horizSizing = "relative";
    vertSizing = "relative";
    position = "0 0";
    extent = %dm.extent;
    visible = "0";
  };

  %dm.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 35";
    extent = "250 115";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
  };

  %dm.scrollSwatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 1";
    extent = "240 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %dm.chattext = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "3 0";
    extent = "240 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "12";
    selectable = "1";
    autoResize = "1";
  };

  %dm.typing = new GuiBitmapCtrl(GlassMessageTyping) {
    window = %dm;
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "24 6";
    position = "5 0";
    visible = 0;
    mcolor = "255 255 255 64";
  };

  %dm.input = new GuiTextEditCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "top";
    position = "10 155";
    extent = "250 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    command = "GlassLive::messageType(" @ %blid @ ");";
    altCommand = "GlassLive::messageInputSend(" @ %blid @ ");";
    accelerator = "enter";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "0";
    sinkAllKeyEvents = "0";
  };
  %dm.add(%dm.resize);
  %dm.add(%dm.scroll);
  %dm.scroll.add(%dm.scrollSwatch);
  %dm.scrollSwatch.add(%dm.chattext);
  %dm.scrollSwatch.add(%dm.typing);
  %dm.add(%dm.input);

  %dm.scrollSwatch.verticalMatchChildren(0, 3);

  %dm.resize.onResize(getWord(%dm.position, 0), getWord(%dm.position, 1), getWord(%dm.extent, 0), getWord(%dm.extent, 1));

  return %dm;
}

function GlassMessageResize::onResize(%this, %x, %y, %h, %l) {
  %window = %this.getGroup();
  %extent = %window.extent;
  %window.scroll.extent = vectorSub(%extent, "20 65");
  %window.scrollSwatch.extent = getWord(%extent, 0) - 30 SPC getWord(%window.chattext.extent, 1);
  %window.chattext.extent = getWord(%extent, 0) - 35 SPC getWord(%window.chattext.extent, 1);

  %window.input.extent = getWord(%extent, 0) - 20 SPC getWord(%window.input.extent, 1);

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scroll.setVisible(true);
}

function GlassMessageTyping::startAnimation(%this) {
  %window = %this.window;
  %this.placeBelow(%window.chattext, 4);

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scrollSwatch.setVisible(true);
  // %window.scroll.scrollToBottom();

  %this.tick();
}

function GlassMessageTyping::tick(%this) {
  cancel(%this.tick);

  %steps = 4;
  %this.step++;

  %this.setBitmap("Add-Ons/System_BlocklandGlass/image/loading_animation/" @ %this.step);

  if(%this.step >= %steps) {
    %this.step = 0;
  }

  %this.tick = %this.schedule(150, tick);
}

function GlassMessageTyping::endAnimation(%this) {
  %window = %this.window;
  %this.position = "5 0";
  cancel(%this.tick);
  %this.step = 0;

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scrollSwatch.setVisible(true);
  // %window.scroll.scrollToBottom();
}

if(!isObject(GlassFriendsGui)) exec("Add-Ons/System_BlocklandGlass/client/gui/GlassFriendsGui.gui");

function GlassLive::createFriendHeader(%name, %isOpen, %color) {
  %gui = new GuiSwatchCtrl() {
    extent = "190 26";
    position = "5 5";
    color = %color;
    hcolor = %color;
  };

  %gui.text = new GuiTextCtrl() {
    profile = "GlassFriendTextProfile";
    text = %name;
    extent = "45 18";
    position = "10 10";
  };

  %gui.bullet = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = "169 5";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ (%isOpen ? "bullet_arrow_down.png" : "bullet_arrow_right.png");
  };

  %gui.mouse = new GuiMouseEventCtrl(GlassHighlightMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    extent = %gui.extent;
    position = "0 0";
    //callback = "GlassLive::friendTabExtend(" @ %blid @ ");";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";

    type = "toggle";
  };

  %gui.add(%gui.text);
  %gui.add(%gui.bullet);
  %gui.add(%gui.mouse);

  //%gui.text.forceCenter();
  %gui.text.centerY();

  return %gui;
}


function GlassLive::createFriendSwatch(%name, %blid, %status) {
  if(%status $= "online") {
    %color = "210 220 255 255";
    %hcolor = "230 240 255 255";
  } else if(%status $= "away") {
    %color = "255 244 210 255";
    %hcolor = "255 255 230 255";
  } else if(%status $= "busy") {
    %color = "255 210 210 255";
    %hcolor = "255 230 230 255";
  } else {
    %color = "210 210 210 255";
    %hcolor = "230 230 230 255";
  }

  %online = (%status $= "offline" ? false : true);

  %icon = GlassLiveUser::getFromBlid(%blid).icon;
  if(%icon $= "")
    %icon = "help";

  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "180 26";
    position = "10 5";
    color = %color;
    hcolor = %hcolor;
  };

  %gui.text = new GuiTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassFriendTextProfile";
    text = %name;
    extent = "31 18";
    position = "24 10";
  };

  %gui.icon = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = "5 5";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ ".png";
  };

  %gui.chaticon = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = %gui.extent-22 SPC 5;
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/comment.png";
    visible = "0";
  };

  %gui.buttonChat = new GuiBitmapButtonCtrl() {
    profile = "GuiButtonProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    command = "GlassLive::openDirectMessage(" @ %blid @ ", \"" @ expandEscape(%name) @ "\");";
    text = "blocklandglass.com";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

    extent = "140 16";
    position = "31 28";
    text = "Message";
  };

  %gui.mouse = new GuiMouseEventCtrl(GlassHighlightMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    extent = %gui.extent;
    position = "0 0";
    //callback = "GlassLive::friendTabExtend(" @ %blid @ ");";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";

    username = %name;
    blid = %blid;
    online = %online;
  };

  %gui.add(%gui.text);
  %gui.add(%gui.icon);
  %gui.add(%gui.chaticon);
  %gui.add(%gui.mouse);

  %gui.add(%gui.buttonChat);

  %gui.text.centerY();
  %gui.icon.centerY();

  return %gui;
}

function GlassLive::createFriendRequest(%name, %blid) {
  %color = "210 210 210 255";
  %hcolor = "230 230 230 255";

  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "180 26";
    position = "10 5";
    color = %color;
    hcolor = %hcolor;
  };

  %gui.text = new GuiTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassFriendTextProfile";
    text = %name @ " (" @ %blid @ ")";
    extent = "31 18";
    position = "10 10";
  };

  // %gui.icon = new GuiBitmapCtrl() {
    // horizSizing = "right";
    // vertSizing = "bottom";
    // extent = "16 16";
    // position = "5 5";
    // bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ GlassLiveUser::getFromBlid(%blid).icon;
  // };

  %gui.decline = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = %gui.extent-22 SPC 5;
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/delete.png";
    visible = "0";
  };

  %gui.accept = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = %gui.extent-47 SPC 5;
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/accept_button.png";
    visible = "0";
  };

  %gui.mouse = new GuiMouseEventCtrl(GlassHighlightMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    extent = %gui.extent;
    position = "0 0";
    //callback = "GlassLive::friendTabExtend(" @ %blid @ ");";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";

    username = %name;
    blid = %blid;
    type = "request";
  };

  %gui.add(%gui.text);
  // %gui.add(%gui.icon);
  %gui.add(%gui.decline);
  %gui.add(%gui.accept);
  %gui.add(%gui.mouse);

  %gui.text.centerY();
  // %gui.icon.centerY();

  return %gui;
}

function GlassLive::createBlockedSwatch(%name, %blid) {
  %color = "210 210 210 255";
  %hcolor = "230 230 230 255";

  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "180 26";
    position = "10 5";
    color = %color;
    hcolor = %hcolor;
  };

  %gui.text = new GuiTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassFriendTextProfile";
    text = %name @ " (" @ %blid @ ")";
    extent = "31 18";
    position = "10 10";
  };

  %gui.unblock = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = %gui.extent-22 SPC 5;
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/delete.png";
    visible = "0";
  };

  %gui.mouse = new GuiMouseEventCtrl(GlassHighlightMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    extent = %gui.extent;
    position = "0 0";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";

    username = %name;
    blid = %blid;
    type = "blocked";
  };

  %gui.add(%gui.text);
  %gui.add(%gui.unblock);
  %gui.add(%gui.mouse);

  %gui.text.centerY();

  return %gui;
}

function GlassLive::sortFriendList(%list) {
  %friends = new GuiTextListCtrl();

  for(%i = 0; %i < getWordCount(%list); %i++) {
    %blid = getWord(%list, %i);
    %uo = GlassLiveUser::getFromBlid(%blid);

    switch$(%uo.getStatus()) {
      case "online":
        %priority = 4;

      case "away":
        %priority = 3;

      case "busy":
        %priority = 2;

      case "offline":
        %priority = 1;

      default:
        %priority = 0;
    }

    %friends.addRow(%blid, %uo.username TAB %priority);
  }

  %friends.sort(0, true);
  %friends.sortNumerical(1, false);

  %newList = "";
  for(%i = 0; %i < %friends.rowCount(); %i++) {
    %newList = %newList SPC %friends.getRowId(%i);
  }

  %newList = getSubStr(%newList, 1, strLen(%newList)-1);

  return %newList;
}

function GlassLive::createFriendList() {
  GlassFriendsGui_ScrollSwatch.deleteAll();

  if(getWordCount(trim(GlassLive.friendRequestList)) > 0) {
    %txt = "Friend Requests";
    if(GlassLive.hideFriendRequests) {
      %txt = %txt SPC "\c4(" @ getWordCount(GlassLive.friendRequestList) @ ")";
    }
    %h = GlassLive::createFriendHeader(%txt, !GlassLive.hideFriendRequests, "131 195 243 255");
    %h.mouse.toggleVar = "GlassLive.hideFriendRequests";
    GlassFriendsGui_ScrollSwatch.add(%h);
    %last = %h;

    if(!GlassLive.hideFriendRequests) {
      for(%i = 0; %i < getWordCount(GlassLive.friendRequestList); %i++) {
        %blid = getWord(GlassLive.friendRequestList, %i);
        %uo = GlassLiveUser::getFromBlid(%blid);

        %gui = GlassLive::createFriendRequest(%uo.username, %blid);
        %gui.placeBelow(%last, 5);

        GlassFriendsGui_ScrollSwatch.add(%gui);

        %last = %gui;
      }
    }
  }

  %h = GlassLive::createFriendHeader("Friends", !GlassLive.hideFriends, "84 217 140 255");
  %h.mouse.toggleVar = "GlassLive.hideFriends";

  if(getWordCount(trim(GlassLive.friendRequestList)) > 0) {
    %h.placeBelow(%last, 10);
  }

  %last = %h;

  GlassFriendsGui_ScrollSwatch.add(%h);

  if(!GlassLive.hideFriends) {

    %sorted = GlassLive::sortFriendList(GlassLive.friendList);

    for(%i = 0; %i < getWordCount(%sorted); %i++) {
      %blid = getWord(%sorted, %i);
      %uo = GlassLiveUser::getFromBlid(%blid);

      %gui = GlassLive::createFriendSwatch(%uo.username, %blid, %uo.status, %uo.isFriend());
      %gui.placeBelow(%last, 5);

      GlassFriendsGui_ScrollSwatch.add(%gui);

      %last = %gui;
    }
  }

  if(getWordCount(trim(GlassLive.blockedList)) > 0) {
    %h = GlassLive::createFriendHeader("Blocked", !GlassLive.hideBlocked, "237 118 105 255");
    %h.mouse.toggleVar = "GlassLive.hideBlocked";

    %h.placeBelow(%last, 10);

    %last = %h;

    GlassFriendsGui_ScrollSwatch.add(%h);

    if(!GlassLive.hideBlocked) {

      for(%i = 0; %i < getWordCount(GlassLive.blockedList); %i++) {
        %blid = getWord(GlassLive.blockedList, %i);
        %uo = GlassLiveUser::getFromBlid(%blid);

        %gui = GlassLive::createBlockedSwatch(%uo.username, %blid);
        %gui.placeBelow(%last, 5);

        GlassFriendsGui_ScrollSwatch.add(%gui);

        %last = %gui;
      }
    }
  }

  GlassFriendsGui_ScrollSwatch.verticalMatchChildren(0, 5);
  GlassFriendsGui_ScrollSwatch.setVisible(true);
  GlassFriendsGui_ScrollSwatch.getGroup().setVisible(true);
}

function GlassFriendsResize::onResize(%this, %x, %y, %h, %l) {
  GlassFriendsGui_Scroll.extent = vectorSub(GlassFriendsWindow.extent, "20 130");
  GlassFriendsGui_ScrollOverlay.extent = GlassFriendsGui_Scroll.extent;
  GlassFriendsGui_PowerButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-15 100");
  GlassFriendsGui_AddButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-65 100");

  GlassSettings.update("Live::FriendsWindow_Pos", GlassFriendsWindow.position);
  GlassSettings.update("Live::FriendsWindow_Ext", GlassFriendsWindow.extent);
}

function GlassSettingsResize::onResize(%this, %x, %y, %h, %l) {
  GlassSettingsGui_Scroll.extent = vectorSub(GlassSettingsWindow.extent, "20 45");

  GlassSettingsGui_ScrollOverlay.extent = getWord(GlassSettingsGui_ScrollOverlay.extent, 0) SPC (GlassSettingsGui_ScrollOverlay.settingsCount * 45) + 15;

  if(getWord(GlassSettingsGui_Scroll.extent, 1) > getWord(GlassSettingsGui_ScrollOverlay.extent, 1)) {
    GlassSettingsGui_ScrollOverlay.extent = GlassSettingsGui_Scroll.extent;
  }
}

function GlassSettingsGui_ScrollOverlay::onWake(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %o = %this.getObject(%i);

    if(isObject(%o.text)) {
      %o.text.forceCenter();
    }
  }
}

package GlassLivePackage {
  function GlassOverlayGui::onWake(%this) {
    parent::onWake(%this);

    if(!GlassOverlayGui.isMember(GlassFriendsWindow)) {
      GlassFriendsWindow.position = GlassSettings.get("Live::FriendsWindow_Pos");
      GlassFriendsWindow.extent = GlassSettings.get("Live::FriendsWindow_Ext");
      GlassOverlayGui.add(GlassFriendsWindow);

      GlassFriendsResize.onResize(getWord(GlassFriendsWindow.position, 0), getWord(GlassFriendsWindow.position, 1), getWord(GlassFriendsWindow.extent, 0), getWord(GlassFriendsWindow.extent, 1));
    }
  }

  function disconnectedCleanup(%doReconnect) {
    GlassLive::updateLocation(false);

    return parent::disconnectedCleanup(%doReconnect);
  }

  function GameConnection::onConnectionAccepted(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k) {
    parent::onConnectionAccepted(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k);

    GlassLive::updateLocation(true);
  }
};
activatePackage(GlassLivePackage);
