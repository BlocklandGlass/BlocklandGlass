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

function GlassLive::init() {
  if(!isObject(GlassLive))
    new ScriptObject(GlassLive) {
      color_friend = "33cc44";
      color_default = "444444";
      color_self = "6688ff";
      color_admin = "ffaa00";
      color_mod = "ee6600";
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

  %settings = "RoomChatNotification RoomChatSound RoomMentionNotification RoomAutoJoin RoomShowAwake MessageNotification MessageSound MessageAnyone ShowTimestamps ShowJoinLeave StartupNotification StartupConnect ShowFriendStatus";
  for(%i = 0; %i < getWordCount(%settings); %i++) {
    %setting = getWord(%settings, %i);
    %box = "GlassModManagerGui_Prefs_" @ %setting;
    %box.setValue(GlassSettings.get("Live::" @ %setting));
  }
}

function GlassLive_keybind() {
  GlassLive::openOverlay();
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
  if(isObject(GlassSettingsWindow))
    GlassSettingsWindow.setVisible(!GlassSettingsWindow.visible);

  if(GlassSettingsWindow.visible)
    GlassOverlayGui.pushToBack(GlassSettingsWindow);
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
    // $Glass::ChatroomConnect = true; // must be a better way to do this?
    GlassLive::disconnect();
    GlassLive.schedule(0, connectToServer);
  }
}

function GlassLive::closeSettings() {
  GlassSettingsWindow.setVisible(false);
}

function GlassLive::closeOverlay() {
  canvas.popDialog(GlassOverlayGui);
}

function GlassLive::updateSetting(%setting) {
  %box = "GlassModManagerGui_Prefs_" @ %setting;
  GlassSettings.update("Live::" @ %setting, %box.getValue());
  %box.setValue(GlassSettings.get("Live::" @ %setting));
  
  if(strLen(%callback = GlassSettings.obj[%setting].callback))
    call(%callback);
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
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_friend @  ">Friend: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_self @  ">Self: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_admin @  ">Admin: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_default @  ">Pleb: <font:verdana:12><color:333333>rambling message", 0);
}

function GlassLive::disconnect(%reason) {
  GlassLive::cleanup();
  if(isObject(GlassLiveConnection))
    GlassLiveConnection.doDisconnect(%reason);
}

function GlassLive::cleanup() {
  GlassLiveUsers.deleteAll();
  GlassLive.friendList = "";
  GlassFriendGui_ScrollSwatch.deleteAll();
  GlassFriendGui_ScrollSwatch.setVisible(true);

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

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() !$= "GlassChatroomWindow")
      continue;

    %window.setDropMode(false);

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

  %newWindow.schedule(0, "resize", getWord(%newWindow.position, 0), getWord(%newWindow.position, 1), getWord(%newWindow.extent, 0), getWord(%newWindow.extent, 1));

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
  messageBoxYesNo("Join Group", "<font:verdana:13>Do you want to join the group chat?", "GlassLive::joinGroup(" @ %id @ ");");
}

function GlassLive::joinGroup(%id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "groupJoin");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();
}

function wordPos(%str, %word) {
  for(%i = 0; %i < getWordCount(%str); %i++) {
    if(getWord(%str, %i) $= %word)
      return %i;
  }
  return -1;
}

function GlassLive::addFriendToList(%user) {
  if((%i = wordPos(GlassLive.friendList, %user.blid)) > -1) {
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

function GlassLive::addfriendRequestToList(%user) {
  if((%i = wordPos(GlassLive.friendRequestList, %user.blid)) > -1) {
    return;
  }

  GlassLive.friendRequestList = setWord(GlassLive.friendRequestList, getWordCount(GlassLive.friendRequestList), %user.blid);
}

function GlassLive::removefriendRequestFromList(%blid) {
  if((%i = wordPos(GlassLive.friendRequestList, %blid)) == -1) {
    return;
  }

  GlassLive.friendRequestList = removeWord(GlassLive.friendRequestList, %i);
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

  GlassChatroomWindow.schedule(0, "openRoomBrowser");
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


  if(GlassLive.typing[%blid])
    GlassLive::messageTypeEnd(%blid);
}

function GlassLive::onMessage(%message, %username, %blid) {
  // TODO check friend, blocked, prefs, etc

  %gui = GlassLive::openDirectMessage(%blid, %username);

  GlassOverlayGui.pushToBack(%gui);

  for(%i = 0; %i < getWordCount(%message); %i++) {
    %word = getWord(%message, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0) {
      %raw = %word;
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %message = setWord(%message, %i, %word);

      %obj = getUrlMetadata(%word, "GlassLive::urlMetadata");
      %obj.context = "dm";
      %obj.blid = %blid;
      %obj.raw = %raw;
    }
    if(getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":") {
      %bitmap = stripChars(%word, ":");
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ">";
        %message = setWord(%message, %i, %word);
      }
    }
  }

  GlassLive::setMessageTyping(%blid, false);

  %val = %gui.chattext.getValue();
  %msg = "<color:333333><font:verdana bold:12>" @ %username @ ":<font:verdana:12><color:333333> " @ %message;
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %gui.chattext.setValue(%val);
  if(%gui.isAwake()) {
    %gui.chattext.forceReflow();
  }
  %gui.scrollSwatch.verticalMatchChildren(0, 3);
  %gui.scrollSwatch.setVisible(true);
  %gui.scroll.scrollToBottom();
}

function GlassLive::onMessageNotification(%message, %blid) {
  // TODO check friend, blocked, prefs, etc

  %user = GlassLiveUser::getFromBlid(%blid);
  %gui = %user.getMessageGui();
  if(%gui == false)
    return;

  %val = %gui.chattext.getValue();
  %msg = "<color:666666><font:verdana:12>" @ %message;
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %gui.chattext.setValue(%val);
  %gui.chattext.forceReflow();
  %gui.scrollSwatch.verticalMatchChildren(0, 3);
  %gui.scroll.scrollToBottom();
  %gui.scrollSwatch.setVisible(true);
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
  %gui.scroll.scrollToBottom();
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

  %obj = JettisonObject();
  %obj.set("type", "string", "friendRequest");
  %obj.set("target", "string", %blid);

  messageBoxOk("Friend Request Sent", "Friend request sent to BLID " @ %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
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
}

function GlassLive::friendDecline(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendDecline");
  %obj.set("blid", "string", %blid);

  for(%i = 0; %i < getWordCount(GlassLive.friendRequestList); %i++) {
    %blid2 = getWord(GlassLive.friendRequestList, %i);
    if(%blid2 != %blid) {
      %newRequests = trim(%newRequests @ %blid2);
    }
  }

  GlassLive.friendRequestList = %newRequests;

  GlassLive::createFriendList();

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::removeFriend(%blid, %silent) {
  if(%blid == getNumKeyId())
    return;

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

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
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
    GlassLive::disconnect();
  } else {
    GlassLive::connectToServer();
  }
}

function GlassLive::setPowerButton(%bool) {
  %btn = GlassFriendsGui_PowerButton;
  %btn.on = %bool;
  if(%btn.on) {
    %btn.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn_poweroff");
  } else {
    %btn.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn_poweron");
  }
}

function GlassLive::openAddDlg() {
  if(!GlassLiveConnection.connected)
    return;

  GlassFriendsGui_AddFriendBLID.getGroup().setVisible(true);
  GlassFriendsGui_ScrollOverlay.setVisible(true);
}

function GlassLive::addDlgSubmit() {
  if(GlassFriendsGui_AddFriendBLID.getValue()+0 !$= GlassFriendsGui_AddFriendBLID.getValue() || GlassFriendsGui_AddFriendBLID.getValue() < 0) {
    messageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  if(GlassFriendsGui_AddFriendBLID.getValue() == getNumKeyId()) {
    messageBoxOk("Invalid BLID", "You can't friend yourself");
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
  if(%val $= "")
    return;

  if(strPos(%val, "/") != 0) {
    GlassLive::sendRoomMessage(%val, %id);
  } else {
    GlassLive::sendRoomCommand(%val, %id);
  }

  %chatroom.input.setValue("");
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
  if(%val $= "")
    return;

  GlassLive::sendMessage(%id, %val);
  GlassLive::onMessage(%val, $Pref::Player::NetName, %id);
  %gui.input.setValue("");
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
  } else if(%this.online) {
    %this.getGroup().chaticon.setVisible(false);
  }
}

function GlassHighlightMouse::onMouseEnter(%this) {
  if(!%this.enabled)
    return;

  %this.getGroup().ocolor = %this.getGroup().color;
  %this.getGroup().color = %this.getGroup().hcolor;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(true);
    %this.getGroup().accept.setVisible(true);
  } else if(%this.online) {
    %this.getGroup().chaticon.setVisible(true);
  }
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
      messageBoxOk("Ignored", "Friend Request ignored");
      GlassLive::friendDecline(%this.blid);
    } else if(getWord(%pos, 0) > getWord(%this.extent, 0)-50) {
      messageBoxOk("Accepted", "Friend Request accepted");
      GlassLive::friendAccept(%this.blid);
    }
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
  if(%user) {
    messageBoxYesNo("Add Friend", "<font:verdana:13>Add <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> as a friend?", "GlassLive::sendFriendRequest(" @ %user.blid @ ");");
  }
}

function GlassLive::removeFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user) {
    messageBoxYesNo("Remove Friend", "<font:verdana:13>Remove <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> as a friend?", "GlassLive::removeFriend(" @ %user.blid @ ");");
  }
}

function GlassLive::openUserWindow(%blid) {
  %uo = GlassLiveUser::getFromBlid(%blid);
  if(%uo) {
    %window = GlassLive::createUserWindow(%uo);
    %text = "<font:verdana bold:13>" @ %uo.username @ "<br><font:verdana:12>" @ %uo.blid;
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
    GlassOverlayGui.pushToBack(%uo.window);
    return %uo.window;
  }

  %window = new GuiWindowCtrl() {
    profile = "GlassWindowProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "235 157";
    extent = "170 166";
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
    extent = "150 50";
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
    position = "10 125";
    extent = "150 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Unfriend";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "base/client/ui/button1";
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
    position = "10 90";
    extent = "150 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Message";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "base/client/ui/button1";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";
    command = "GlassLive::openDirectMessage(" @ %uo.blid @ ");";
  };

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
    position = "135 131";
    extent = "475 290";
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

  if(%this.isAwake())
    %this.schedule(0, "resize", getWord(%this.position, 0), getWord(%this.position, 1), getWord(%this.extent, 0), getWord(%this.extent, 1));

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

  if(%this.tabs > 1) {

  } else {
    %this.openTab(0);
  }
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
    }
    %this.renderTabs();
  }
}

function GlassChatroomWindow::exitTab(%this) {
  %tab = %this.activeTab;
  if(isObject(%tab)) {
    messageBoxYesNo("Leave Room?", "<font:verdana:13>Are you sure you want to leave <font:verdana bold:13>" @ %tab.title @ "<font:verdana:15>?", %tab.room.getId() @ ".leaveRoom();");
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
    %current.room.setAwake(false);
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
      %this.text = "Chatroom - " @ %tab.title;
      %this.setText(%this.text);

      %tab.room.setAwake(true);
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

  if(%this.isAwake())
    %this.schedule(0, "resize", getWord(%this.position, 0), getWord(%this.position, 1), getWord(%this.extent, 0), getWord(%this.extent, 1));
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

function GlassChatroomWindow::awakeCallback(%this, %callback) {
  if(isObject(%this.activeTab)) {
    %bool = GlassSettings.get(%callback) ? true : false;
    
    %this.activeTab.room.setAwake(%bool);
  }
}

function GlassChatroomWindow::onWake(%this) {
  if(isObject(%this.activeTab)) {
    %this.activeTab.room.setAwake(true);
  }
}

function GlassChatroomWindow::onSleep(%this) {
  if(isObject(%this.activeTab)) {
    %this.activeTab.room.setAwake(false);
  }
}

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
      bitmap = "base/client/ui/button1";
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
        %this.tabButton.text.setText(%this.title);
      cancel(%this.flashSchedule);
    }
  }
}

function GlassChatroomTab::flashTick(%this, %bool) {
  cancel(%this.flashSchedule);

  %button = %this.tabButton;
  if(isObject(%button)) {
    %button.text.setText(collapseEscape("\\c" @ %bool) @ %this.title);
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
  %chatText.extent = getWord(%scroll.extent, 0) SPC getWord(%chatText.extent, 1);
  %userScroll.extent = getWord(%userScroll.extent, 0) SPC getWord(%extent, 1) - 90;
  %userScroll.position = getWord(%scroll.extent, 0) + 15 SPC getWord(%userScroll.position, 1);
  %input.extent = getWord(%extent, 0) - 150 SPC getWord(%input.extent, 1);
  %input.position = getWord(%input.position, 0) SPC getWord(%scroll.extent, 1) + 5;

  if(isObject(%browserSwatch)) {
    %browserSwatch.extent = getWord(%extent, 0) - 20 SPC getWord(%extent, 1) - 70;
  }

  %scroll.scrollToBottom();

  %scrollSwatch.verticalMatchChildren(0, 2);
  %scrollSwatch.setVisible(true);

  %userSwatch.getGroup().scrollToTop();
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

  %chatroom.input = new GuiTextEditCtrl() {
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
    tabComplete = "0";
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

  %chatroom.input = new GuiTextEditCtrl() {
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
    tabComplete = "0";
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
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ (%online ? "user.png" : "user_gray.png");
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
  %window.scrollSwatch.extent = getWord(%extent, 0)-30 SPC getWord(%window.chattext.extent, 1);
  %window.chattext.extent = getWord(%extent, 0)-35 SPC getWord(%window.chattext.extent, 1);

  %window.input.extent = getWord(%extent, 0)-20 SPC getWord(%window.input.extent, 1);

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scroll.setVisible(true);
}

function GlassMessageTyping::startAnimation(%this) {
  %window = %this.window;
  %this.placeBelow(%window.chattext, 4);

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scrollSwatch.setVisible(true);
  %window.scroll.scrollToBottom();

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
  %window.scroll.scrollToBottom();
}

if(!isObject(GlassFriendsGui)) exec("Add-Ons/System_BlocklandGlass/client/gui/GlassFriendsGui.gui");

function GlassLive::createFriendHeader(%name) {
  %gui = new GuiSwatchCtrl() {
    extent = "190 26";
    position = "5 5";
    color = "120 160 225 255";
    hcolor = "190 200 225 255";
  };

  %gui.text = new GuiTextCtrl() {
    profile = "GlassFriendTextProfile";
    text = %name;
    extent = "45 18";
    position = "10 10";
  };

  %gui.add(%gui.text);

  //%gui.text.forceCenter();
  %gui.text.centerY();

  return %gui;
}


function GlassLive::createFriendSwatch(%name, %blid, %online) {
  if(%online) {
    %color = "210 220 255 255";
    %hcolor = "220 230 255 255";
  } else {
    %color = "210 210 210 255";
    %hcolor = "230 230 230 255";
  }

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
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ (%online ? "user.png" : "user_gray.png");
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
    bitmap = "base/client/ui/button1";

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
    position = "24 10";
  };

  %gui.icon = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = "5 5";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ "user_gray.png";
  };

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

    blid = %blid;
    type = "request";
  };

  %gui.add(%gui.text);
  %gui.add(%gui.icon);
  %gui.add(%gui.decline);
  %gui.add(%gui.accept);
  %gui.add(%gui.mouse);

  %gui.text.centerY();
  %gui.icon.centerY();

  return %gui;
}


function GlassLive::createFriendList(%friends) {
  GlassFriendGui_ScrollSwatch.deleteAll();
  %h = GlassLive::createFriendHeader("Friends");
  GlassFriendGui_ScrollSwatch.add(%h);

  %last = %h;

  for(%i = 0; %i < getWordCount(GlassLive.friendList); %i++) {
    %blid = getWord(GlassLive.friendList, %i);
    %uo = GlassLiveUser::getFromBlid(%blid);

    %gui = GlassLive::createFriendSwatch(%uo.username, %blid, %uo.online, %uo.isFriend());
    %gui.placeBelow(%last, 5);

    GlassFriendGui_ScrollSwatch.add(%gui);

    %last = %gui;
  }

  if(getWordCount(trim(GlassLive.friendRequestList)) > 0) {
    %h = GlassLive::createFriendHeader("Friend Requests");
    %h.placeBelow(%last, 10);
    GlassFriendGui_ScrollSwatch.add(%h);

    %last = %h;

    for(%i = 0; %i < getWordCount(GlassLive.friendRequestList); %i++) {
      %blid = getWord(GlassLive.friendRequestList, %i);
      %uo = GlassLiveUser::getFromBlid(%blid);

      %gui = GlassLive::createFriendRequest(%uo.username, %blid);
      %gui.placeBelow(%last, 5);

      GlassFriendGui_ScrollSwatch.add(%gui);

      %last = %gui;
    }
  }
  GlassFriendGui_ScrollSwatch.verticalMatchChildren(0, 5);
  GlassFriendGui_ScrollSwatch.setVisible(true);
  GlassFriendGui_ScrollSwatch.getGroup().setVisible(true);
}

package GlassLivePackage {
  function GlassOverlayGui::onWake(%this) {
    parent::onWake(%this);

    if(!GlassOverlayGui.isMember(GlassFriendsWindow)) {
      GlassFriendsWindow.position = (getWord(getRes(), 0) - getWord(GlassFriendsWindow.extent, 0) - 50) SPC 50;
      GlassOverlayGui.add(GlassFriendsWindow);
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
