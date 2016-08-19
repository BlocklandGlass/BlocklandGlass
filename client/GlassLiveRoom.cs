function GlassLiveRooms::create(%id, %name) {
  if(isObject(GlassLive.room[%id]))
    return GlassLive.room[%id];

  %room = new ScriptObject() {
    class = "GlassLiveRoom";

    id = %id;
    name = %name;

    users = "";
    view = "";
  };

  if(!isObject(GlassLiveRoomGroup)) {
    new ScriptGroup(GlassLiveRoomGroup);
  }

  GlassLiveRoomGroup.add(%room);

  GlassLive.room[%id] = %room;

  return %room;
}

function GlassLiveRoom::getFromId(%id) {
  if(isObject(GlassLive.room[%id]))
    return GlassLive.room[%id];
  else
    return false;
}

function GlassLiveRoom::leaveRoom(%this) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomLeave");
  %obj.set("id", "string", %this.id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %this.view.window.removeTab(%this.view);
  %this.view.deleteAll();
  %this.view.delete();

  %this.schedule(0, delete);
}

function GlassLiveRoom::addUser(%this, %blid) {
  if(%this.user[%blid])
    return;

  %this.user[%blid] = true;
  %this.users = trim(%this.users TAB %blid);

  if(isObject(%this.view)) {
    %this.renderUserList();
  }
}

function GlassLiveRoom::removeUser(%this, %blid) {
  if(!%this.user[%blid])
    return;

  %this.user[%blid] = false;
  for(%i = 0; %i < getFieldCount(%this.users); %i++) {
    if(getField(%this.users, %i) == %blid) {
      %this.users = removeField(%this.users, %i);
      return;
    }
  }
}

function GlassLiveRoom::getCount(%this) {
  return getFieldCount(%this.users);
}

function GlassLiveRoom::getBl_id(%this, %idx) {
  return getField(%this.users, %idx);
}

function GlassLiveRoom::getUser(%this, %idx) {
  return GlassLiveUser::getFromBlid(%this.getBl_id(%idx));
}

function GlassLiveRoom::createView(%this, %window) {
  if(isObject(%this.view)) {
    %this.view.deleteAll();
    %this.view.delete();
  }

  %gui = GlassLive::createChatroomView(%this.id);

  %gui.title = %this.name;
  %gui.id = %this.id;

  %gui.room = %this;

  if(!isObject(%window)) {
    if(!isObject($Glass::defaultRoomWindow)) {
      $Glass::defaultRoomWindow = GlassLive::createChatroomWindow();
    }

    %window = $Glass::defaultRoomWindow;
  }

  %window.addTab(%gui);

  %this.view = %gui;
  %this.renderUserList();

  return %gui;
}

function GlassLiveRoom::onUserJoin(%this, %blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  %text = "<font:verdana:12><color:666666>" @ %user.username @ " entered the room.";
  %this.pushText(%text);
  %this.addUser(%blid);
}

function GlassLiveRoom::onUserLeave(%this, %blid, %reason) {
  %chatroom = %this.view;

  %this.removeUser(%blid);

  switch(%reason) {
    case -1:
      %text = "No Reason";

    case 0:
      %text = "Left";

    case 1:
      %text = "Disconnected";

    case 2:
      %text = "Kicked";

    case 3:
      %text = "Connection Dropped";

    case 4:
      %text = "Updates";

    default:
      %text = "unhandled: " @ %reason;
  }

  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user == false)
    %user = "BLID_" @ %blid; // todo local caching
  else
    %user = %user.username;

  %text = "<font:verdana:12><color:666666>" @ %user @ " left the room. [" @ %text @ "]";
  %this.pushText(%text);

  %this.renderUserList();
}

function GlassLiveRoom::sendMessage(%this, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomChat");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %this.id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLiveRoom::sendCommand(%this, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomCommand");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %this.id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLiveRoom::setUserAwake(%this, %blid, %awake) {
  %this.awake[%blid] = %awake;
  %text = %this.userListSwatch[%blid].text;
  if(isObject(%text)) {
    %colorCode = %awake ? 0 : 2;
    %text.setValue(collapseEscape("\\c" @ %colorCode) @ %text.rawtext);
  }
}

function GlassLiveRoom::setAwake(%this, %bool) {
  %this.awake = %bool;
  if(GlassSettings.get("Live::RoomShowAwake")) {
    %obj = JettisonObject();
    %obj.set("type", "string", "roomAwake");
    %obj.set("id", "string", %this.id);
    %obj.set("bool", "string", %bool);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }
}

function GlassLiveRoom::pushMessage(%this, %sender, %msg, %data) {
  %now = getRealTime();
  if(%now-%this.lastMessageTime > 1000 * 60 * 5) {
    %text = "<font:verdana bold:12><just:center><color:999999>[" @ formatTimeHourMin(%data.datetime) @ "]<just:left>";
    %this.pushText(%text);
  }
  %this.lastMessageTime = %now;

  %senderblid = %sender.blid;

  if(%senderblid == getNumKeyId()) {
    %color = GlassLive.color_self;
  } else if(%sender.isAdmin()) {
    %color = GlassLive.color_admin;
  } else if(%sender.isMod()) {
    %color = GlassLive.color_mod;
  } else if(GlassLive.isFriend[%senderblid]) {
    %color = GlassLive.color_friend;
  } else {
    %color = GlassLive.color_default;
  }

  %msg = stripMlControlChars(%msg);
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getWord(%msg, %i);
    if(%word $= ("@" @ $Pref::Player::NetName)) {
      %mentioned = true;
      %msg = setWord(%msg, %i, " <spush><font:verdana bold:12><color:" @ GlassLive.color_self @ ">" @ %word @ "<spop>");
    }
  }


  %text = "<font:verdana bold:12><color:" @ %color @ ">" @ %sender.username @ ":<font:verdana:12><color:333333> " @ %msg;
  %this.pushText(%text);

  %this.view.setFlashing(true);

  if(GlassSettings.get("Live::RoomChatSound"))
    alxPlay(GlassChatAudio);

  if(%senderblid != getNumKeyId() && !%this.awake)
    if(%mentioned && GlassSettings.get("Live::RoomMentionNotification")) {
      GlassNotificationManager::newNotification(%this.name, "You were mentioned by " @ %sender.username, 0);
    } else if(GlassSettings.get("Live::RoomChatNotification"))
      GlassNotificationManager::newNotification(%this.name, %sender.username@": "@%msg, "comment", 0);
}

function GlassLiveRoom::pushText(%this, %msg) {
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getWord(%msg, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0) {
      %raw = %word;
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %msg = setWord(%msg, %i, %word);
    }
  }

  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = "<font:verdana:12><color:666666>[" @ getWord(getDateTime(), 1) @ "]" SPC %msg;
  }

  %chatroom = %this.view;
  %val = %chatroom.chattext.getValue();
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %chatroom.chattext.setValue(%val);
  if(GlassOverlayGui.isAwake())
    %chatroom.chattext.forceReflow();

  %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
  %chatroom.scrollSwatch.setVisible(true);
  %chatroom.scroll.scrollToBottom();
}

function GlassLiveRoom::getOrderedUserList(%this) {
  %users = new GuiTextListCtrl();
  %admins = new GuiTextListCtrl();
  %mods = new GuiTextListCtrl();

  for(%i = 0; %i < %this.getCount(); %i++) {
    %user = %this.getUser(%i);
    if(%user.isAdmin()) {
      %admins.addRow(%i, %user.username);
    } else if(%user.isMod()) {
      %mods.addRow(%i, %user.username);
    } else {
      %users.addRow(%i, %user.username);
    }
  }

  %users.sort(0);
  %admins.sort(0);
  %mods.sort(0);

  %idList = "";

  for(%i = 0; %i < %admins.rowCount(); %i++) {
    %idList = %idList SPC %admins.getRowId(%i);
  }

  for(%i = 0; %i < %mods.rowCount(); %i++) {
    %idList = %idList SPC %mods.getRowId(%i);
  }

  for(%i = 0; %i < %users.rowCount(); %i++) {
    %idList = %idList SPC %users.getRowId(%i);
  }

  %admins.delete();
  %mods.delete();
  %users.delete();

  return trim(%idList);
}

function GlassLiveRoom::renderUserList(%this) {
  %userSwatch = %this.view.userswatch;
  %userSwatch.deleteAll();

  %orderedList = %this.getOrderedUserList();

  for(%i = 0; %i < getWordCount(%orderedList); %i++) {
    %user = %this.getUser(getWord(%orderedList, %i));

    if(%user.isAdmin()) {
      %icon = "crown_gold";
      %pos = "2 4";
      %ext = "14 14";
    } else if(%user.isMod()) {
      %icon = "crown_silver";
      %pos = "2 4";
      %ext = "14 14";
    } else if(%user.isFriend()) {
      %icon = "user_green";
      %pos = "1 3";
      %ext = "16 16";
    } else {
      %icon = "user";
      %pos = "1 3";
      %ext = "16 16";
    }

    if(%this.awake[%user.blid])
      %colorCode = 0;
    else
      %colorCode = 2;

    // TODO GuiBitmapButtonCtrl
    %swatch = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "3 3";
      extent = "110 22";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "0 0 0 0";
    };

    %swatch.icon = new GuiBitmapCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      extent = %ext;
      position = %pos;
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon;
    };

    %swatch.text = new GuiTextCtrl() {
      profile = "GlassFriendTextProfile";
      text = collapseEscape("\\c" @ %colorCode) @ %user.username;
      rawtext = %user.username;
      extent = "45 18";
      position = "22 12";
    };

    %swatch.mouse = new GuiMouseEventCtrl(GlassLiveUserListSwatch) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "0 0";
      extent = %swatch.extent;

      swatch = %swatch;
      user = %user;
    };

    %swatch.add(%swatch.icon);
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.mouse);
    %swatch.text.centerY();
    if(%last !$= "") {
      %swatch.placeBelow(%last, 0);
    }
    %last = %swatch;
    %userSwatch.add(%swatch);

    %this.userListSwatch[%user.blid] = %swatch;
  }

  %userSwatch.getGroup().scrollToTop();
  %userSwatch.verticalMatchChildren(0, 5);
  %userSwatch.setVisible(true);
}

function GlassLiveUserListSwatch::onMouseEnter(%this) {
  %this.swatch.color = "220 220 220 255";
}

function GlassLiveUserListSwatch::onMouseLeave(%this) {
  %this.swatch.color = "160 160 160 0";
  %this.down = false;
}

function GlassLiveUserListSwatch::onMouseDown(%this) {
  %this.swatch.color = "150 150 255 255";
  %this.down = true;
}

function GlassLiveUserListSwatch::onMouseUp(%this) {
  %this.swatch.color = "220 220 220 255";
  if(%this.down) {
    %this.down = false;
    //if(%this.group)
    //  %this.group.displayUserOptions(%this.user);
    //else
    if(%this.user.blid != getNumKeyId())
      messageBoxYesNo("Add Friend", "<font:verdana:13>Add <font:verdana bold:13>" @ %this.user.username @ "<font:verdana:13> as a friend?", "GlassLive::sendFriendRequest(" @ %this.user.blid @ ");");
    else
      messageBoxOk("Hey There!", "<font:verdana:13>That's you!");
  }
}
