exec("./submodules/live/GlassLiveConnection.cs");
exec("./submodules/live/GlassLiveUser.cs");
exec("./submodules/live/GlassLiveRoom.cs");
// exec("./GlassLiveGroup.cs");

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
  if(!isObject(GlassLive)) {
    new ScriptObject(GlassLive) {
      // color_blocked = "969696";
      color_default = "222222";
      color_self = "55acee";
      color_friend = "2ecc71";
      color_mod = "e67e22";
      color_admin = "e74c3c";
      color_bot = "9b59b6";
    };

    GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  }

  if(!isObject(GlassLiveUsers))
    new ScriptGroup(GlassLiveUsers);

  if(!isObject(GlassLiveGroups))
    new ScriptGroup(GlassLiveGroups);

  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassOverlayGui.gui");
  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassSettingsGui.gui");

  //GlassOverlayGui.add(GlassFriendsGui.getObject(0));
  GlassSettingsWindow.setVisible(false);
  GlassOverlayGui.add(GlassSettingsWindow);

  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassIconSelectorGui.gui");

  GlassIconSelectorWindow.setVisible(false);
  GlassOverlayGui.add(GlassIconSelectorWindow);
  GlassIconSelectorWindow.updateIcons();

  // glass pref, description/name, category, type, properties (for dropdowns), information
  GlassSettings.drawSetting("Live::StartupConnect", "Auto-Connect During Startup", "Live", "checkbox", "", "Automatically connect to Glass Live on start-up.");
  GlassSettings.drawSetting("Live::StartupNotification", "Startup Notification", "Live", "checkbox", "", "Show a start-up notification which includes your current keybind.");
  GlassSettings.drawSetting("Live::PendingReminder", "Pending Friend Req. Reminder", "Live", "checkbox", "", "Show notification if you have any pending friend requests when you connect to Glass Live.");
  GlassSettings.drawSetting("Live::ShowTimestamps", "Timestamping", "Live", "checkbox", "", "Show the time next to all chat messages in the chatroom and DMs.");
  GlassSettings.drawSetting("Live::ShowFriendStatus", "Friend Status Notifications", "Live", "checkbox", "", "Show notifications when your friends change their status.");
  GlassSettings.drawSetting("Live::ConfirmConnectDisconnect", "Confirm Connect/Disconnect", "Live", "checkbox", "", "Show a dialog box asking for confirmation when connecting and disconnecting to and from Glass Live.");
  GlassSettings.drawSetting("Live::AutoJoinRoom", "Automatically Join Rooms", "Live", "checkbox", "", "Automatically join the chatroom when you connect to Glass Live.");
  GlassSettings.drawSetting("Live::OverlayLogo", "Display Overlay Logo", "Live", "checkbox", "", "Show the Glass logo in the overlay in the top left.");
  GlassSettings.drawSetting("Live::TalkingAnimation", "Avatar Talking Animation", "Live", "checkbox", "", "Play avatar talking animation whenever you send a message on Glass Live.");

  GlassSettings.drawSetting("MM::UseDefault", "Use Default Updater", "Mod Manager", "checkbox", "", "Use Support_Updater's interface when updating add-ons.");
  GlassSettings.drawSetting("MM::LiveSearch", "Use Live Search", "Mod Manager", "checkbox");

  GlassSettings.drawSetting("Live::ShowJoinLeave", "User Connection Messages", "Chatroom", "checkbox", "", "Show all users entering and exiting the chatroom.");
  GlassSettings.drawSetting("Live::RoomMentionNotification", "Mentioned Notification", "Chatroom", "checkbox", "", "Display a notification and play a sound when you're @mentioned in the chatroom.");
  GlassSettings.drawSetting("Live::RoomChatNotification", "Chat Notifications", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::RoomChatSound", "Chat Sounds", "Chatroom", "checkbox");
  GlassSettings.drawSetting("Live::RoomNotification", "Entered/Exited Notifications", "Chatroom", "checkbox", "", "Show notifications when you enter and exit a room in Glass Live.");
  GlassSettings.drawSetting("Live::RoomShowBlocked", "Show Blocked Users", "Chatroom", "checkbox", "", "Show blocked users' messages in the chatroom.");

  GlassSettings.drawSetting("Live::MessageNotification", "Message Notifications", "Direct Messaging", "checkbox");
  GlassSettings.drawSetting("Live::MessageSound", "Message Sounds", "Direct Messaging", "checkbox");
  GlassSettings.drawSetting("Live::MessageLogging", "Message Logging", "Direct Messaging", "checkbox", "", "Log DMs to config/client/BLG/chat_log/DMs");
  GlassSettings.drawSetting("Live::MessageAnyone", "Messages From Strangers", "Direct Messaging", "checkbox", "", "Receive DMs from people not on your friends list.");

  GlassSettings.drawSetting("Servers::EnableFavorites", "Favorite Servers *", "Servers", "checkbox", "", "Display Favorite Servers menu GUI.<br><br><font:verdana bold:13>Requires restart."); // add restart msgbox callback
  GlassSettings.drawSetting("Servers::LoadingGUI", "Glass Loading GUI *", "Servers", "checkbox", "", "Use the Glass Loading GUI when connecting to a server.<br><br><font:verdana bold:13>Requires restart.");
  GlassSettings.drawSetting("Servers::LoadingImages", "Custom Loading Images", "Servers", "checkbox", "", "Display a custom loading image if the server has set one.");

  GlassSettings.drawSetting("Live::FakeSetting", "A Fake Setting", "Test", "dropdown", "One Two Three Four Five", "This does nothing practical.");

  %settings["Live"] = "TalkingAnimation RoomChatNotification RoomChatSound RoomMentionNotification RoomShowBlocked MessageNotification MessageSound MessageLogging MessageAnyone ShowTimestamps ShowJoinLeave StartupNotification StartupConnect ShowFriendStatus RoomNotification ConfirmConnectDisconnect PendingReminder MessageLogging AutoJoinRoom OverlayLogo FakeSetting";
  %settings["MM"] = "UseDefault LiveSearch";
  %settings["Servers"] = "LoadingGUI LoadingImages EnableFavorites";

  %settings = "Live MM Servers";

  for(%i = 0; %i < getWordCount(%settings); %i++) {
    %prefix = getWord(%settings, %i);
    %group = %settings[%prefix];
    for(%o = 0; %o < getWordCount(%group); %o++) {
      %setting = getWord(%group, %o);
      %box = "GlassSettingsGui_Prefs_" @ %setting;

      switch$(%box.profile) {
        case "GlassCheckBoxProfile":
          %box.setValue(GlassSettings.get(%prefix @ "::" @ %setting));
        case "GuiPopUpMenuProfile":
          %box.setText(GlassSettings.get(%prefix @ "::" @ %setting));
      }
    }
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

function GlassLive::onAuthSuccess() {
  GlassLive::createBlockhead();

  GlassLive_StatusSwatch.setVisible(true);
  GlassFriendsGui_StatusSelect::selectStatus("Online");

  GlassFriendsGui_Blockhead.setVisible(true);
  GlassFriendsGui_Blockhead.setOrbitDist(5.5);
  GlassFriendsGui_Blockhead.setCameraRot(0.22, 0.5, 2.8);

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14>" @ $Pref::Player::NetName @ "<br><font:verdana:12>" @ getNumKeyId());

  GlassFriendsGui_HeaderText.position = "10 5";

  GlassLive::sendAvatarData();
  GlassLive::updateLocation();
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

  if(GlassModManagerGui.page $= "") {
    GlassModManagerGui.openPage(GMM_ActivityPage);
  }

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

function GlassLive::closeSettings() {
  GlassSettingsWindow.setVisible(false);
}

function GlassLive::openIconSelector() {
  if(isObject(GlassIconSelectorWindow)) {
    GlassIconSelectorWindow.onWake();
    GlassIconSelectorWindow.setVisible(!GlassIconSelectorWindow.visible);
  }

  if(GlassIconSelectorWindow.visible) {
    GlassOverlayGui.pushToBack(GlassIconSelectorWindow);
  }
}

function GlassLive::closeIconSelector() {
  GlassIconSelectorWindow.setVisible(false);
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
      glassMessageBoxYesNo("Reconnect", "This will reconnect you to Glass Live, continue?", "GlassLive::disconnect(1); GlassLive.schedule(100, connectToServer);");
      } else {
      glassMessageBoxYesNo("Connect", "This will connect you to Glass Live, continue?", "GlassLive.schedule(0, connectToServer);");
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

function GlassLive::closeOverlay() {
  canvas.popDialog(GlassOverlayGui);
}

function GlassLive::updateSetting(%category, %setting) {
  %box = "GlassSettingsGui_Prefs_" @ %setting;
  GlassSettings.update(%category @ "::" @ %setting, %box.getValue());
  %box.setValue(GlassSettings.get(%category @ "::" @ %setting));

  if(strlen(%callback = GlassSettings.obj[%setting].callback)) {
    if(isFunction(%callback)) {
      call(%callback);
    }
  }
}

function GlassOverlayGui::onWake(%this) {
  %x = getWord(getRes(), 0);
  %y = getWord(getRes(), 1);
  GlassOverlay.resize(0, 0, %x, %y);

  if(GlassSettings.get("Live::OverlayLogo") && !GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(true);
  else if(!GlassSettings.get("Live::OverlayLogo") && GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(false);

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      if(isObject(%window.activeTab)) {
        %tab = %window.activeTab;
        if(%tab.chattext.didUpdate) {
          %tab.chattext.forceReflow();
          %tab.chattext.didUpdate = false;
        }
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

  if(isObject(GlassFriendsGui_Blockhead)) {
    GlassFriendsGui_Blockhead.schedule(1, "setOrbitDist", 5.5);
    GlassFriendsGui_Blockhead.schedule(1, "setCameraRot", 0.22, 0.5, 2.8);
  }
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
  // %room.pushText("<font:verdana bold:12><color:" @ %this.color_blocked @  ">Blocked: <font:verdana:12><color:333333>rambling message", 0);
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
    if(%window.getName() $= "GlassChatroomWindow" || %window.getName() $= "GlassGroupchatWindow" || %window.getName() $= "GlassMessageGui" || %window.getName() $= "GlassUserGui") {
      %window.deleteAll();
      %window.delete();
      %i--;
    } else if(%window.getName() $= "GlassIconSelectorWindow") {
      %window.setVisible(false);
    }
  }

  GlassFriendsGui_ScrollSwatch.verticalMatchChildren(0, 5);
  GlassFriendsGui_ScrollSwatch.setVisible(true);
  GlassFriendsGui_ScrollSwatch.getGroup().setVisible(true);
  GlassFriendsGui_Blockhead.setVisible(false);
}

// function GlassLive::showUserStatus() {
  // %str = "<font:verdana:15><color:333333><tab:110>";
  // %val[%vals++] = "BLID\t9789";
  // %val[%vals++] = "";
  // %val[%vals++] = "Status\tOnline";
  // %val[%vals++] = "Location\tCrown's Prison Escape";
  // %val[%vals++] = "";
  // %val[%vals++] = "Forum Account\t<a:forum.blockland.us>Scout31</a>";
  // for(%i = 0; %i < %vals; %i++) {
    // %line = %val[%i+1];
    // if(%line $= "") {
      // %str = %str @ "<br><br>";
    // } else {
      // %str = %str @ "<font:verdana bold:15>" @ getField(%line, 0) @ ":\t<font:verdana:15>" @ getField(%line, 1) @ "<br>";
    // }
  // }

  // echo(%str);

  // GlassUserStatus.setValue(%str);
// }

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

function secondsToTimeString(%total) { // Crown
  %days = mFloor(%total / 86400);
  %remander = %total % 86400;

  %hours = mFloor(%remander / 3600);
  %remander = %remander % 3600;

  %minutes = mFloor(%remander / 60);

  %seconds = mFloor(%remander % 60);

  if(%days != 1)
    %s = "s";
  if(%hours != 1)
    %hs = "s";
  if(%minutes != 1)
    %ms = "s";
  if(%seconds != 1)
    %ss = "s";

  return %days SPC "day" @ %s @ "," SPC %hours SPC "hour" @ %hs @ "," SPC %minutes SPC "minute" @ %ms SPC "and" SPC %seconds SPC "second" @ %ss;
}

function GlassFriendsGui_StatusSelect::updateStatus() {
  %status = trim(stripMlControlChars(GlassLive_Status.getValue()));
  %selector = GlassFriendsGui_StatusSelect;

  switch$ (%status) {
	case "Online":
	  %selector.position = "14 65";
	case "Away":
	  %selector.position = "14 40";
	case "Busy":
	  %selector.position = "14 15";
	default:
		return;
  }
  %selector.setVisible(true);
}

function GlassFriendsGui_StatusSelect::selectStatus(%status) {
  GlassFriendsGui_StatusSelect.setVisible(false);
  switch$ (%status) {
	case "Online":
	  %color = "84 217 140 100";
	  %value = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_online><font:verdana bold:13> Online";
	case "Away":
	  %color = "241 196 15 100";
	  %value = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_away><font:verdana bold:13> Away";
	case "Busy":
	  %color = "231 76 60 100";
	  %value = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_busy><font:verdana bold:13> Busy";
	default:
		return;
  }
  GlassLive_Status.setValue(%value);
  GlassFriendsGui_InfoSwatch.color = %color;
  GlassLive::setStatus(%status);
}

function GlassLive::setFriendStatus(%blid, %status) {
  %uo = GlassLiveUser::getFromBlid(%blid);
  // echo("Set Status: " @ %status);
  %uo.setStatus(%status);
  // echo("Status: " @ %uo.getStatus());

  GlassLive::createFriendList();

  if(GlassSettings.get("Live::ShowFriendStatus") && !%uo.isBlocked()) {
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
      default:
        %icon = "user";
    }

    GlassNotificationManager::newNotification(%uo.username, "is now " @ %uo.getStatus() @ ".", %icon, 0);
  }
}

function GlassLive::setStatus(%status) {
  %status = strlwr(%status);

  if(%status $= GlassLiveUser::getFromBlid(getNumKeyId()).status)
    return;

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

  GlassIconSelectorWindow_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon);
}

function GlassIconSelectorWindow::updateIcons(%this) {
  %allowed = "Add-Ons/System_BlocklandGlass/resources/icons_allowed.txt";
  if(!isFile(%allowed)) {
    error(%allowed SPC "not found, unable to create icon list.");
    return;
  }
  %swatch = GlassIconSelectorWindow_Swatch;

  if(!isObject(%swatch)) {
    error("Could not find icon list swatch.");
    return;
  }

  %path = "Add-Ons/System_BlocklandGlass/image/icon/";
  %iconCount = -1;

  %file = new FileObject();
  %file.openForRead("Add-Ons/System_BlocklandGlass/resources/icons_allowed.txt");
  while(!%file.isEOF()) {
    %line = %file.readLine();
    %icon = %path @ %line;

    if(!isFile(%icon @ ".png"))
      continue;

    %iconCount++;

    if(%iconCount % 14 == 0)
      %column = 0;

    %row = mFloor(%iconCount / 14);
    %position = (20 * %column) + 3 SPC (%row * 20) + 3;
    %column++;

    %bitmap = new GuiBitmapCtrl() {
      bitmap = %icon;
      position = %position;
      extent = "16 16";
    };
    %button = new GuiButtonBaseCtrl() {
      position = %position;
      extent = "16 16";
      command = "GlassIconSelectorWindow_Preview.setBitmap(\"Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ "\");";
    };
    GlassIconSelectorWindow_Swatch.add(%bitmap);
    GlassIconSelectorWindow_Swatch.add(%button);
  }
  GlassIconSelectorWindow_Swatch.extent = getWord(GlassIconSelectorWindow_Swatch.extent, 0) SPC (%row * 16 + %row * 7);
  GlassIconSelectorWindow_Swatch.setVisible(true);
}

function GlassIconSelectorWindow::selectIcon(%this) {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to change your icon.");
    return;
  }

  GlassLive::setIcon(strreplace(GlassIconSelectorWindow_Preview.bitmap, "Add-Ons/System_BlocklandGlass/image/icon/", ""));
  GlassLive::closeIconSelector();
}

function GlassIconSelectorWindow::onWake(%this) {
  %this.forceCenter();

  %icon = GlassLiveUser::getFromBlid(getNumKeyId()).icon;

  if(%icon $= "")
    %icon = "ask_and_answer";

  GlassIconSelectorWindow_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon);
}

function GlassLive::userBlock(%blid) {
  if(%blid+0 !$= %blid || %blid < 0 || mfloor(%blid) !$= %blid)
    return;

  if(%blid == getNumKeyId())
    return;

  if(wordPos(GlassLive.friendRequestList, %blid) != -1) {
    GlassLive::friendDecline(%blid);
    return;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  %blockedIcon = "wall";

  if(isObject(%user)) {
    if(%user.isBlocked())
      return;

    %user.setBlocked(true);

    if(isObject(%user.window))
      GlassLive::openUserWindow(%blid);

    %user.setIcon(%blockedIcon);

    if(isObject(%user.getMessageGui())) {
      %user.getMessageGui().input.setValue("");
      %user.getMessageGui().input.enabled = false;
    }
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "block");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  for(%i = 0; %i < getWordCount(GlassLive.blockedList); %i++) {
    if(getWord(GlassLive.blockedList, %i) == %blid) {
      return;
    }
  }

  GlassLive.blockedList = trim(GlassLive.blockedList SPC %blid);

  %username = %user.username;

  if(%username $= "")
    %username = "Blockhead" @ %blid;

  GlassLive::createFriendList();
  GlassLive::onMessageNotification("You have blocked " @ %username @ ".", %blid);
  if(isObject(%room = GlassChatroomWindow.activeTab.room))
    %room.pushText("You have blocked " @ %username @ ".");
}

function GlassLive::userUnblock(%blid) {
  if(%blid+0 !$= %blid || %blid < 0 || mfloor(%blid) !$= %blid)
    return;

  if(%blid == getNumKeyId())
    return;

  %user = GlassLiveUser::getFromBlid(%blid);

  if(isObject(%user)) {
    if(!%user.isBlocked())
      return;

    %user.setBlocked(false);

    if(isObject(%user.window))
      GlassLive::openUserWindow(%blid);

    %user.setIcon(%user.realIcon);

    if(isObject(%user.getMessageGui()))
      %user.getMessageGui().input.enabled = true;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "unblock");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  for(%i = 0; %i < getWordCount(GlassLive.blockedList); %i++) {
    if(getWord(GlassLive.blockedList, %i) == %blid) {
      GlassLive.blockedList = removeWord(GlassLive.blockedList, %i);
      break;
    }
  }

  %username = %user.username;

  if(%username $= "")
    %username = "Blockhead" @ %blid;

  GlassLive::createFriendList();
  GlassLive::onMessageNotification("You have unblocked " @ %username @ ".", %blid);
  if(isObject(%room = GlassChatroomWindow.activeTab.room))
    %room.pushText("You have unblocked " @ %username @ ".");
}

function GlassLive::createBlockhead() {
  AvatarGui.OnWake();

  if(isObject(GlassFriendsGui_Blockhead))
	  GlassFriendsGui_Blockhead.delete();

  new GuiObjectView(GlassFriendsGui_Blockhead) {
    forceFOV = "18";
    lightDirection = "0 0.2 0.2";
    extent = "74 176";
  };

  if(isObject(GlassFriendsGui_BlockheadAnim))
    GlassFriendsGui_BlockheadAnim.delete();

  new GuiButtonBaseCtrl(GlassFriendsGui_BlockheadAnim) {
    extent = "74 176";
    command = "GlassLive::BlockheadRandomAnim();";
  };

  GlassFriendsGui_Blockhead.add(GlassFriendsGui_BlockheadAnim);
  GlassFriendsGui_Blockhead.setVisible(false);
  GlassFriendsGui_Blockhead.position = "120 -5";
  GlassFriendsGui_Blockhead.lightDirection = "0 0.2 0.2";
  GlassFriendsGui_Blockhead.createBlockhead();
  GlassFriendsGui_InfoSwatch.add(GlassFriendsGui_Blockhead);

  GlassFriendsGui_Blockhead.schedule(500, "setVisible", true);
  GlassFriendsGui_Blockhead.schedule(200, "setOrbitDist", 5.5);
  GlassFriendsGui_Blockhead.schedule(200, "setCameraRot", 0.22, 0.5, 2.8);
}

function GlassLive::BlockheadRandomAnim() {
  %rand = getRandom(0, 7);

  switch (%rand) {
	  case 0:
		%thread = "talk";
	  case 1:
		%thread = "standjump";
	  case 2:
		%thread = "side";
	  case 3:
		%thread = "spearReady";
		%time = 800;
		return;
	  case 4:
		%thread = "undo";
	  case 5:
		%thread = "back";
	  case 6:
		%thread = "headside";
	  case 7:
		%thread = "armReadyBoth";
  }

  if(%time $= "")
    %time = 400;

  GlassLive::BlockheadAnim(%thread, %time);
}

function GlassLive::BlockheadAnim(%thread, %time, %type) {
  %blockhead = GlassFriendsGui_Blockhead;
  if(!isObject(%blockhead))
    return;

   if(GlassSettings.get("Live::TalkingAnimation") == 0 && %thread $= "talk")
	  return;

  cancel(%blockhead.rootSchedule);

  if(%type $= "")
	  %type = 1;

  GlassFriendsGui_Blockhead.setSequence("", %type, %thread, 1);

  %blockhead.rootSchedule = %blockhead.schedule(%time, "setSequence", "", 1, "root", 1);
}

function GuiObjectView::createBlockhead(%this, %json) {
  %this.forceFOV = 18;

  %this.setObject("", "base/data/shapes/player/m.dts", "", 100);
  if(!%json) {
    %FaceName = $Pref::Avatar::FaceName;
    %DecalName = $Pref::Avatar::DecalName;
    %HeadColor = $Pref::Avatar::HeadColor;

    %accent = $Pref::Avatar::accent;
    %accentColor = $Pref::Avatar::accentColor;

    %hat = $Pref::Avatar::hat;
    %hatColor = $Pref::Avatar::hatColor;

    %chest = $Pref::Avatar::chest;
    %chestColor = $Pref::Avatar::chestColor;
    %TorsoColor = $Pref::Avatar::TorsoColor;

    %pack = $Pref::Avatar::pack;
    %packColor = $Pref::Avatar::packColor;

    %secondPack = $Pref::Avatar::secondPack;
    %secondPackColor = $Pref::Avatar::secondPackColor;

    %larm = $Pref::Avatar::larm;
    %larmColor = $Pref::Avatar::larmColor;

    %rarm = $Pref::Avatar::rarm;
    %rarmColor = $Pref::Avatar::rarmColor;

    %lhand = $Pref::Avatar::lhand;
    %lhandColor = $Pref::Avatar::lhandColor;

    %rhand = $Pref::Avatar::rhand;
    %rhandColor = $Pref::Avatar::rhandColor;

    %hip = $Pref::Avatar::hip;
    %hipColor = $Pref::Avatar::hipColor;

    %lleg = $Pref::Avatar::lleg;
    %llegColor = $Pref::Avatar::llegColor;

    %rleg = $Pref::Avatar::rleg;
    %rlegColor = $Pref::Avatar::rlegColor;
  } else {
    %FaceName = %json.faceName;
    %DecalName = %json.decalName;
    %HeadColor = %json.headColor;

    %accent = %json.accent;
    %accentColor = %json.accentColor;

    %hat = %json.hat;
    %hatColor = %json.hatColor;

    %chest = %json.chest;
    %chestColor = %json.chestColor;
    %TorsoColor = %json.torsoColor;

    %pack = %json.pack;
    %packColor = %json.packColor;

    %secondPack = %json.secondPack;
    %secondPackColor = %json.secondPackColor;

    %larm = %json.larm;
    %larmColor = %json.larmColor;

    %rarm = %json.rarm;
    %rarmColor = %json.rarmColor;

    %lhand = %json.lhand;
    %lhandColor = %json.lhandColor;

    %rhand = %json.rhand;
    %rhandColor = %json.rhandColor;

    %hip = %json.hip;
    %hipColor = %json.hipColor;

    %lleg = %json.lleg;
    %llegColor = %json.llegColor;

    %rleg = %json.rleg;
    %rlegColor = %json.rlegColor;
  }


  for(%i = 0; %i < $numFace; %i++)
	if(strStr($Face[%i], %FaceName) >= 0)
	  %faceDecal = %i;

  for(%i = 0; %i < $numDecal; %i++)
    if(strStr($Decal[%i], %DecalName) >= 0)
	  %shirtDecal = %i;

  %this.setIFLFrame("", "face", %faceDecal);
  %this.setIFLFrame("", "decal",%shirtDecal);
  %this.setNodeColor("", "ALL", %HeadColor);

  %bodyParts = "accent hat chest pack secondpack larm rarm lhand rhand hip lleg rleg";
  for(%i = 0; %i <= 11; %i++) {
    %currPart = getWord(%bodyParts, %i);
    %numPart = $num[%currPart];

	for(%j = 0; %j <= %numPart; %j++) {
	  eval("%equipCurrPart = %" @ %currPart @ ";");
	  eval("%equipCurrPartColor = %" @ %currPart @ "Color;");
	  eval("%currCheck = $" @ %currPart @ "[" @ %j @ "];");

	  if(%currCheck $= "" || %currCheck $= "None")
		  continue;

	  if(%j $= %equipCurrPart) {
      %this.unHideNode("", %currCheck);
      %this.setNodeColor("", %currCheck, %equipCurrPartColor);
	  }
	  else
      %this.hideNode("", %currCheck);
    }
  }
  %accent = getWord($accentsAllowed[$hat[%hat]], %accent);
  if(%accent !$= "" && %accent !$= "none") {
    %this.unhidenode("", %accent);
    %this.hideNode("", "plume");
    %this.setnodeColor("", %accent, %accentColor);
  }

  if($Hip[%hip] !$= "skirtHip") {
	%this.hideNode("", "SkirtTrimLeft");
	%this.hideNode("", "SkirtTrimRight");
  } else {
	%this.hideNode("", "rShoe");
	%this.hideNode("", "lShoe");
  }

  %this.hideNode("", "rski");
  %this.hideNode("", "lski");

  %this.setNodeColor("", $chest[%chest], %torsoColor);
  %this.setMouse(1, 1);
}

function GlassLive::sendAvatarData() {
  %obj = JettisonObject();
  %obj.set("type", "string", "avatar");

  %partArray = JettisonArray();

  %FaceName = $Pref::Avatar::FaceName;
  %DecalName = $Pref::Avatar::DecalName;
  %HeadColor = $Pref::Avatar::HeadColor;
  %obj.set("faceName", "string", %faceName);
  %obj.set("decalName", "string", %decalName);
  %obj.set("headColor", "string", %headColor);

  %accent = $Pref::Avatar::accent;
  %accentColor = $Pref::Avatar::accentColor;
  %obj.set("accent", "string", %accent);
  %obj.set("accentColor", "string", %accentColor);

  %hat = $Pref::Avatar::hat;
  %hatColor = $Pref::Avatar::hatColor;
  %obj.set("hat", "string", %hat);
  %obj.set("hatColor", "string", %hatColor);

  %chest = $Pref::Avatar::chest;
  %chestColor = $Pref::Avatar::chestColor;
  %TorsoColor = $Pref::Avatar::TorsoColor;
  %obj.set("chest", "string", %chest);
  %obj.set("chestColor", "string", %chestColor);
  %obj.set("torsoColor", "string", %torsoColor);

  %pack = $Pref::Avatar::pack;
  %packColor = $Pref::Avatar::packColor;
  %obj.set("pack", "string", %pack);
  %obj.set("packColor", "string", %packColor);

  %secondPack = $Pref::Avatar::secondPack;
  %secondPackColor = $Pref::Avatar::secondPackColor;
  %obj.set("secondPack", "string", %secondPack);
  %obj.set("secondPackColor", "string", %secondPackColor);

  %larm = $Pref::Avatar::larm;
  %larmColor = $Pref::Avatar::larmColor;
  %obj.set("larm", "string", %larm);
  %obj.set("larmColor", "string", %larmColor);

  %rarm = $Pref::Avatar::rarm;
  %rarmColor = $Pref::Avatar::rarmColor;
  %obj.set("rarm", "string", %rarm);
  %obj.set("rarmColor", "string", %rarmColor);

  %lhand = $Pref::Avatar::lhand;
  %lhandColor = $Pref::Avatar::lhandColor;
  %obj.set("lhand", "string", %lhand);
  %obj.set("lhandColor", "string", %lhandColor);

  %rhand = $Pref::Avatar::rhand;
  %rhandColor = $Pref::Avatar::rhandColor;
  %obj.set("rhand", "string", %rhand);
  %obj.set("rhandColor", "string", %rhandColor);

  %hip = $Pref::Avatar::hip;
  %hipColor = $Pref::Avatar::hipColor;
  %obj.set("hip", "string", %hip);
  %obj.set("hipColor", "string", %hipColor);

  %lleg = $Pref::Avatar::lleg;
  %llegColor = $Pref::Avatar::llegColor;
  %obj.set("lleg", "string", %lleg);
  %obj.set("llegColor", "string", %llegColor);

  %rleg = $Pref::Avatar::rleg;
  %rlegColor = $Pref::Avatar::rlegColor;
  %obj.set("rleg", "string", %rleg);
  %obj.set("rlegColor", "string", %rlegColor);

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

function GlassLive::joinFriendServer(%blid) {
	%user = GlassLiveUser::getFromBlid(%blid);
	%server = getServerFromIP(%user.getServerAddress());

	if(!isObject(%server)) {
		glassMessageBoxOk("Uh oh", %user.username SPC "is not currently playing a joinable server.");
		return;
	}

	GlassServerPreviewGui.open(%server);
}

function GlassLive::openDirectMessage(%blid, %username) {
  if(%blid < 0 || %blid $= "" || %blid == getNumKeyId()) {
    return false;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  if(!%user.canSendMessage()) {
    glassMessageBoxOk("Blocked", "You have blocked this user, unblock them before attempting to send them a message.");
    return;
  }

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
  %timestamp = "[" @ getWord(getDateTime(), 1) @ "]";

  if(GlassSettings.get("Live::MessageLogging")) {
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

  %user = GlassLiveUser::getFromBlid(%blid);

  if(!isObject(%user))
    return;

  %gui = %user.getMessageGui();

  if(%gui == false)
    return;

  %timestamp = "[" @ getWord(getDateTime(), 1) @ "]";

  if(GlassSettings.get("Live::MessageLogging")) {
    if(isFile(%file = "config/client/BLG/chat_log/DMs/" @ %blid @ "/" @ strReplace(getWord(getDateTime(), 0), "/", ".") @ ".txt")) {
      %fo = new FileObject();
      %fo.openForAppend(%file);
      %fo.writeLine(%timestamp SPC %message);
      %fo.close();
      %fo.delete();
    }
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
  %obj.delete();

  GlassLive::BlockheadAnim("talk", strLen(%msg) * 50);

  if(GlassLiveUser::getFromBlid(getNumKeyId()).getStatus() !$= "away") {
    for(%i = 0; %i < getWordCount(%msg); %i++) {
      %word = getWord(%msg, %i);
      if(%word $= "brb" || %word $= "afk") {
        glassMessageBoxYesNo("Away", "Would you like to set your status to \"Away\"?", "GlassFriendsGui_StatusSelect::selectStatus(\"away\");");
      }
    }
  }
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

  %obj.delete();
}

function GlassLive::sendFriendRequest(%blid) {
  if(%blid == getNumKeyId())
    return;

  if((%blid+0 !$= %blid) || %blid < 0) {
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  if(%user.isBlocked()) {
    glassMessageBoxOk("Blocked", "You have blocked this user, unblock them before attempting to send a friend request.");
    return;
  }

  if(wordPos(GlassLive.friendRequestList, %blid) != -1) { // insert has friend request here if exists in glassliveuser as func?
    GlassLive::friendAccept(%blid);
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "friendRequest");
  %obj.set("target", "string", %blid);

  glassMessageBoxOk("Friend Request Sent", "Friend request sent to BLID <font:verdana bold:13>" @ %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::friendAccept(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendAccept");
  %obj.set("blid", "string", %blid);

  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user) {
    %user.setFriend(true);

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
    %user.setFriend(false);
    if(isObject(%user.window))
      GlassLive::openUserWindow(%blid);
    if(isObject(%room = GlassChatroomWindow.activeTab.room))
      %room.renderUserList();
  }
}

function GlassLive::updateLocation(%inServer) {

  if(!%inServer) {
    %action = "menus";
  } else if(ServerConnection.isLocal()) {
    if($Server::LAN) {
      if($Server::ServerType $= "Singleplayer") {
        %action = "singleplayer";
      } else {
        %action = "hosting_lan";
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
  %obj.set("type", "string", "updateLocation");
  %obj.set("location", "string", %action);

  if(%action $= "playing") {
    %location = ServerConnection.getRawIP() @ ":" @ ServerConnection.getPort();
    %name = NPL_Window.getValue();
    %name = getSubStr(%name, strpos(%name, "-")+2, strlen(%name));

    %obj.set("address", "string", %location);
  }

  //echo(jettisonStringify("object", %obj));

  if(isObject(GlassLiveConnection) && GlassLiveConnection.connected) {
    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }

  %obj.delete();
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
    if(GlassSettings.get("Live::ConfirmConnectDisconnect")) {
      glassMessageBoxYesNo("Disconnect", "Are you sure you want to disconnect from Glass Live?", "GlassLive.noReconnect = true; GlassLive::disconnect(" @ $Glass::Disconnect["Manual"] @ ");");
    } else {
      GlassLive.noReconnect = true;
      GlassLive::disconnect($Glass::Disconnect["Manual"]);
    }
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
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to add friends.");
    return;
  }

  if(GlassFriendsGui_BlockUserBLID.getGroup().visible)
    return;

  %gui = GlassFriendsGui_AddFriendBLID.getGroup();

  if(!%gui.visible) {
    %gui.setVisible(true);
    GlassFriendsGui_ScrollOverlay.setVisible(true);
  } else {
    GlassLive::addDlgClose();
  }
}

function GlassLive::openBlockDlg() {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to block users.");
    return;
  }

  if(GlassFriendsGui_AddFriendBLID.getGroup().visible)
    return;

  %gui = GlassFriendsGui_BlockUserBLID.getGroup();

  if(!%gui.visible) {
    %gui.setVisible(true);
    GlassFriendsGui_ScrollOverlay.setVisible(true);
  } else {
    GlassLive::blockDlgClose();
  }
}

function GlassLive::blockDlgSubmit() {
  if(GlassFriendsGui_BlockUserBLID.getValue()+0 !$= GlassFriendsGui_BlockUserBLID.getValue() || GlassFriendsGui_BlockUserBLID.getValue() < 0) {
    GlassFriendsGui_BlockUserBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  if(GlassFriendsGui_BlockUserBLID.getValue() == getNumKeyId()) {
    GlassFriendsGui_BlockUserBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "You can't block yourself.");
    return;
  }

  GlassLive::userBlock(GlassFriendsGui_BlockUserBLID.getValue());
  GlassFriendsGui_BlockUserBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_BlockUserBLID.setValue("");
}

function GlassLive::blockDlgClose() {
  GlassFriendsGui_BlockUserBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_BlockUserBLID.setValue("");
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
  GlassLive::BlockheadAnim("talk", strLen(%val) * 50);
  GlassLive::onMessage(%val, $Pref::Player::NetName, %id);
  %gui.input.setValue("");
  %gui.scroll.schedule(100, "scrollToBottom");
}

function GlassLive::friendListHover(%swatch) {
  %this = %swatch.glassHighlight;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(true);
    %this.getGroup().accept.setVisible(true);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(true);
  } else if(%this.online && %this.status !$= "busy") {
    %this.getGroup().chaticon.setVisible(true);
  }

  if(getWord(%this.getGroup().text.extent, 0) > getWord(vectorSub(%this.extent, %this.pos), 0)-20)
    if(%this.scrollTick $= "")
      %this.scrollTick = %this.scrollLoop(%this.getGroup().text, true);
}

function GlassLive::friendListExit(%swatch) {
  %this = %swatch.glassHighlight;

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

function GlassLive::friendListClick(%swatch, %pos) {
  %this = %swatch.glassHighlight;

  switch$(%this.type) {
    case "request":
      echo(%pos);
      if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
        glassMessageBoxOk("Friend Declined", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been declined.");
        GlassLive::friendDecline(%this.blid);
      } else if(getWord(%pos, 0) > getWord(%this.extent, 0)-50) {
        glassMessageBoxOk("Friend Added", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been added.");
        GlassLive::friendAccept(%this.blid);
      } else {
        if(isObject(%window = GlassLiveUser::getFromBlid(%this.blid).window))
          %window.delete();
        else
          GlassLive::openUserWindow(%this.blid);
      }

    case "blocked":
      if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
        glassMessageBoxOk("Unblocked", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been unblocked.");
        GlassLive::userUnblock(%this.blid);
      }

    case "toggle":
        eval(%this.toggleVar @ " = !" @ %this.toggleVar @ ";");
        GlassLive::createFriendList();

    default:
      if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
        if(%this.online) {
          GlassLive::openDirectMessage(%this.blid);
        }
      } else {
        if(isObject(%window = GlassLiveUser::getFromBlid(%this.blid).window))
          %window.delete();
        else
          GlassLive::openUserWindow(%this.blid);
      }
  }
}

function GlassHighlightMouse::scrollLoop(%this, %text, %reset) {
  %icon = %text.getGroup().icon;
  %buttonChat = %text.getGroup().buttonChat;
  %unblock = %text.getGroup().unblock;

  if(%reset) {
    %this._scrollOrigin = %text.position;
    if(isObject(%icon))
      %this._scrollOrigin_Icon = %icon.position;
    %this._scrollOffset = 0;
    if(isObject(%unblock) || isObject(%buttonChat))
      %this._scrollRange = getWord(%text.extent, 0)-getWord(%this.extent, 0)+getWord(%text.position, 0)+25;
    else
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

//================================================================
//= Gui Creation                                                 =
//================================================================

function GlassLive::addFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);

  if(%user.isBlocked()) {
    glassMessageBoxOk("Blocked", "You have blocked this user, unblock them before attempting to send a friend request.");
    return;
  }

  if(%user)
    glassMessageBoxYesNo("Add Friend", "Add <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> as a friend?", "GlassLive::sendFriendRequest(" @ %user.blid @ ");");
}

function GlassLive::removeFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user)
    glassMessageBoxYesNo("Remove Friend", "Remove <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> as a friend?", "GlassLive::removeFriend(" @ %user.blid @ ");");
}

function GlassLive::createUserWindow(%uo) {
   if(isObject(%uo.window)) {
    GlassOverlayGui.pushToBack(%uo.window);
    //%uo.window.delete();
    return %uo.window;
  }

  %window = new GuiWindowCtrl() {
    profile = "GlassWindowProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "265 157";
    extent = "320 255";
    text = "Glass User";
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMinimize = "0";
    canMaximize = "0";
  };

  %window.infoSwatch = new GuiSwatchCtrl() {
	color = "235 235 235 255";
	position = "10 35";
	extent = "100 210";
  };

  %window.blockhead = new GuiObjectView() {
    position = "0 40";
  	extent = "100 170";
  	forceFov = 18;
  	lightDirection = "-1 -1 -1";
  };

  %window.headerText = new GuiMLTextCtrl() {
  	position = "120 35";
  	extent = "300 35";
  	text = "";
  };

  %window.statusSwatch = new GuiBitmapButtonCtrl() {
  	profile = "GlassBlockButtonProfile";
  	bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
  	text = "";
  	mColor = "220 220 220 255";
  	position = "22 10";
  	extent = "70 22";
  };

  %window.statusText = new GuiMLTextCtrl() {
  	position = "2 3";
  	extent = "65 16";
  	minExtent = "8 2";
  	text = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_online><font:verdana bold:13> Online";
  };

  %window.messageButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 108";
    extent = "190 30";
    text = "Message";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "255 255 255 200";
    command = "GlassLive::openDirectMessage(" @ %uo.blid @ "); if(isObject(" @ GlassLiveUser::getFromBlid(%uo.blid) @ ".getMessageGui())){" @ GlassLiveUser::getFromBlid(%uo.blid) @ ".getMessageGui().forceCenter();}";
  };

  %window.friendButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 143";
    extent = "190 30";
    text = "Unfriend";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "255 200 200 200";
  };

  %window.blockButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 178";
    extent = "190 30";
    text = "Block";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "237 118 105 200";
  };

  %window.inviteButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 213";
    extent = "93 30";
    text = "Invite";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "85 172 238 200";
    command = ""; // Invite friend to server function
  };

  %window.joinButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "217 213";
    extent = "93 30";
    text = "Join";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "46 204 113 200";
    command = "GlassLive::joinFriendServer(" @ %uo.blid @ ");";
  };

  %window.locationSwatch = new GuiBitmapButtonCtrl() {
  	profile = "GlassBlockButtonProfile";
  	mColor = "220 220 220 120";
  	position = "120 58";
  	extent = "145 22";
  	bitmap = "Add-Ons/System_BlocklandGlass/image/gui/Tab1";
  	text = "Unknown";
  	buttonType = "";
  };

  %window.serverText = new GuiTextCtrl() {
  	profile = "GlassFriendTextProfile";
  	position = "2 3";
  	extent = "146 30";
  	text = "Crown's Jailbreak - Desert";
  };

   %window.serverSwatch = new GuiSwatchCtrl() {
  	color = "220 220 220 0";
  	position = "115 80";
  	extent = "148 20";
  	text = %window.serverText;
  	visible = 0;
  };

  %window.mouse = new GuiMouseEventCtrl(GlassLiveUserListSwatch) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = %window.serverSwatch.extent;

    swatch = %window.serverSwatch;
  };

  %window.add(%window.infoSwatch);
  %window.infoSwatch.add(%window.blockhead);

  %window.infoSwatch.add(%window.statusSwatch);
  %window.statusSwatch.add(%window.statusText);
  %window.statusSwatch.centerX();

  //%window.add(%window.locationSwatch);

  //%window.add(%window.serverSwatch);
  //%window.serverSwatch.add(%window.serverText);
  //%window.serverSwatch.add(%window.mouse);

  %window.add(%window.headerText);
  %window.add(%window.messageButton);
  %window.add(%window.friendButton);
  %window.add(%window.blockButton);
  %window.add(%window.inviteButton);
  %window.add(%window.joinButton);


  %window.closeCommand = %window.getId() @ ".delete();";

  GlassOverlayGui.add(%window);

   %window.setName("GlassUserGui");
   %uo.window = %window;
   return %window;
}


function GlassLive::openUserWindow(%blid) {
  %uo = GlassLiveUser::getFromBlid(%blid);
  if(%uo) {
    %window = GlassLive::createUserWindow(%uo);

    switch$(%uo.getStatus()) {
      case "online":
        %status = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_online><font:verdana bold:13> Online";
      case "busy":
        %status = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_busy><font:verdana bold:13> Busy";
      case "away":
        %status = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_away><font:verdana bold:13> Away";
      default:
        %status = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_offline><font:verdana bold:13> Offline";
    }

    switch$ (%location = %uo.getLocation()) {
      case "menus":
        %locationColor = "220 220 220 220";
        %locationDisplay = "In Menus";

      case "playing":
        %locationColor = "46 204 113 220";
        %locationDisplay = "Playing";

      case "playing_lan":
        %locationColor = "46 204 113 220";
        %locationDisplay = "Playing LAN";

      case "hosting":
        %locationColor = "85 172 238";
        %locationDisplay = "Hosting";

      case "hosting_lan":
        %locationColor = "85 172 238";
        %locationDisplay = "Hosting LAN";

      case "singleplayer":
        %locationColor = "231 76 60 220";
        %locationDisplay = "Single Player";

      default:
        %locationColor = "220 220 220 120";
        %locationDisplay = "Unknown";
    }

  	if(%locationDisplay $= "Unknown")
  	  %window.locationSwatch.setVisible(false);

  	%window.locationSwatch.mColor = %locationColor;
  	%window.locationSwatch.setText(%locationDisplay);

  	%serverName = %uo.getServerTitle();
  	if(%serverName !$= "" && %serverName !$= "1") {
  	  %window.serverText.setText(%serverName);
  	  %window.serverSwatch.setVisible(true);
  	  %window.mouse.currentServer = %uo.getServerAddress();
  	}

  	%window.statusText.setText(%status);
  	%window.headerText.setText("<font:verdana bold:14>" @ %uo.username NL "<font:verdana:12>" @ %uo.blid @ "<br><br>" @ strCap(%uo.getLocation()) @ "<br><font:verdana bold:12>" @ %uo.getServerTitle());

    %uo.getAvatar(%window.blockhead);

    if(%uo.isFriend()) {
      %window.friendButton.mcolor = "255 200 200 200";
      %window.friendButton.command = "GlassLive::removeFriendPrompt(" @ %uo.blid @ ");";
      %window.friendButton.text = "Unfriend";
    } else {
      %window.friendButton.mcolor = "200 255 200 200";
      %window.friendButton.command = "GlassLive::addFriendPrompt(" @ %uo.blid @ ");";
      %window.friendButton.text = "Add Friend";
    }

    if(%uo.isBlocked()) {
      %window.blockButton.mcolor = "237 184 105 200";
      %window.blockButton.command = "GlassLive::userUnblock(" @ %uo.blid @ ");";
      %window.blockButton.text = "Unblock";
    } else {
      %window.blockButton.mcolor = "237 118 105 200";
      %window.blockButton.command = "GlassLive::userBlock(" @ %uo.blid @ ");";
      %window.blockButton.text = "Block";
    }

    %window.messageButton.enabled = true;

    if(!%window.centered) {
      %window.forceCenter();
      %window.centered = true;
    }
  }
}

function GlassLive::createBanWindow(%blid, %name, %type) {
  if(!GlassOverlayGui.isMember(GlassBanWindowGui))
	GlassoverlayGui.add(GlassBanWindowGui);

  if(%type $= "Mute")
	GlassBanWindowReasonBlocker.setVisible(true);
  else
	GlassBanWindowReasonBlocker.setVisible(false);
  GlassBanWindowGui.punishType = %type;
  GlassBanWindowGui.blid = %blid;
  GlassBanWindowLabel.setText("<just:center><font:verdana bold:14>" @ %type @ %name SPC "(" @ %blid @ ")");
  GlassBanWindowGui.forceCenter();
  GlassOverlayGui.pushToBack(GlassBanWindowGui);
  GlassBanWindowGui.setVisible(true);
}

function GlassLive::submitBanWindow() {
	%reason = GlassBanWindowReason.getValue();
	if(%reason !$= "")
		%reason = " " @ %reason;
  GlassLive::sendRoomCommand("/" @ GlassBanWindowGui.punishType @ "id" SPC GlassBanWindowDuration.getValue() SPC GlassBanWindowGui.blid @ %reason, GlassChatroomWindow.activeTab.id);
  GlassBanWindowGui.setVisible(false);
}

function GlassLive::createChatroomWindow() {
  %chatroom = new GuiWindowCtrl(GlassChatroomWindow) {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 130";
    extent = "475 290";
    // minExtent = "475 290";
    // extent = "604 476";
    minExtent = "475 290";
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
	if(%this.activeTabId == -1)
		%browserOpen = 1;
    else if(%this.activeTabId == %id) {
      if(%id >= %this.tabs)
        %this.openTab(%this.tabs-1);
      else
        %this.openTab(%id);
    } else if(%this.activeTabId >= %id) {
      %this.activeTabId--;
    }
	if(%browserOpen)
		%this.openTab(0);
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
    baseBitmap = (%i == %this.activeTabId ? "Add-Ons/System_BlocklandGlass/image/gui/tab1use" : "Add-Ons/System_BlocklandGlass/image/gui/tab1");
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
  %this.baseBitmap = (%bool ? "Add-Ons/System_BlocklandGlass/image/gui/tab1use" : "Add-Ons/System_BlocklandGlass/image/gui/tab1");
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

  %swatch.extent = (%x+getWord(%this.tabAddButton.extent, 0)) SPC getWord(%swatch.extent, 1);

  %pad = 10;
  %minWidth = (%pad*2)+getWord(%swatch.extent, 0);

  %this.minExtent = (%minWidth > 475 ? %minWidth : 475) SPC 290;
  if(getWord(%this.extent, 0) < %minWidth) {
    if(%this.isAwake()) {
      %this.schedule(0, resize, getWord(%this.position, 0), getWord(%this.position, 1), %minWidth, getWord(%this.extent, 1));
    } else {
      %this.extent = %minWidth SPC getWord(%this.extent, 1);
    }
  }
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

// function chatroomAwakeCallback(%callback) {
  // if(isObject(GlassChatroomWindow) && isObject(GlassChatroomWindow.activeTab)) {
    // %bool = GlassSettings.get(%callback) ? true : false;

    // GlassChatroomWindow.activeTab.room.setAwake(%bool);
  // }
// }

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

  if(%this.isAwake()) {
    %chatText.forceReflow();
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
    position = "0 60";
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

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

  %gui.add(%gui.text);
  %gui.add(%gui.bullet);
  %gui.add(%gui.mouse);

  //%gui.text.forceCenter();
  %gui.text.centerY();

  return %gui;
}

function GlassLive::createFriendSwatch(%name, %blid, %status) {
  if(%name $= "")
    %name = "Blockhead" @ %blid;

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
    %icon = "ask_and_answer";

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
    status = %status;

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

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

  if(%name $= "")
    %name = "Blockhead" @ %blid;

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

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

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

  if(%name $= "")
    %name = "Blockhead" @ %blid;

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

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

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
  %friends.delete();
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

    if(getWordCount(trim(GlassLive.friendList)) > 0) {
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
  GlassFriendsGui_Scroll.extent = vectorSub(GlassFriendsWindow.extent, "20 135");
  GlassFriendsGui_ScrollOverlay.extent = GlassFriendsGui_Scroll.extent;

  GlassFriendsGui_PowerButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-15 105");
  GlassFriendsGui_AddButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-65 105");
  GlassFriendsGui_BlockButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-95 105");

  GlassSettings.update("Live::FriendsWindow_Pos", GlassFriendsWindow.position);
  GlassSettings.update("Live::FriendsWindow_Ext", GlassFriendsWindow.extent);
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

package GlassLivePackage {
  function GlassOverlayGui::onWake(%this) {
    parent::onWake(%this);

    if(!GlassOverlayGui.isMember(GlassFriendsWindow)) {
      %pos = GlassSettings.get("Live::FriendsWindow_Pos");
      %ext = GlassSettings.get("Live::FriendsWindow_Ext");

      if(%pos > getWord(getRes(), 0))
        %pos = (getWord(getRes(), 0) - 280) SPC 50;

      if(%ext > getWord(getRes(), 1))
        %ext = "230 380";

      GlassFriendsWindow.position = %pos;
      GlassFriendsWindow.extent = %ext;

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

  function Avatar_Done() {
	parent::Avatar_Done();
    GlassFriendsGui_Blockhead.createBlockhead();
  }
};
activatePackage(GlassLivePackage);
