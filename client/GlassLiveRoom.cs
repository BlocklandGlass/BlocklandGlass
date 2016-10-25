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

  if(GlassSettings.get("Live::RoomNotification")) {
    GlassNotificationManager::newNotification("Left Room", "You've left " @ %this.name, "delete", 0);
  }

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
  if(GlassSettings.get("Live::ShowJoinLeave")) {
    %text = "<font:verdana:12><color:666666>" @ %user.username @ " entered the room.";
    %this.pushText(%text);
  }
  %this.addUser(%blid);
}

function GlassLiveRoom::onUserLeave(%this, %blid, %reason) {
  %chatroom = %this.view;

  %this.removeUser(%blid);

  if(GlassSettings.get("Live::ShowJoinLeave")) {
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
        %text = "Quit";

      case 4:
        %text = "Updates";

      default:
        %text = "Unhandled: " @ %reason;
    }

    %user = GlassLiveUser::getFromBlid(%blid);
    if(%user == false)
      %user = "BLID_" @ %blid; // todo local caching
    else
      %user = %user.username;

    %text = "<font:verdana:12><color:666666>" @ %user @ " left the room. [" @ %text @ "]";
    %this.pushText(%text);
  }

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
  %icon = %this.userListSwatch[%blid].icon;
  if(isObject(%icon)) {
    %icon.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ (%awake ? "user.png" : "user_yellow.png"));
  }
}

function GlassLiveRoom::setAwake(%this, %bool) {
  if(!GlassSettings.get("Live::RoomShowAwake"))
    %bool = false;

  %this.awake = %bool;

  %obj = JettisonObject();
  %obj.set("type", "string", "roomAwake");
  %obj.set("id", "string", %this.id);
  %obj.set("bool", "string", %bool);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
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
    for(%o = 0; %o < %this.view.userSwatch.getCount(); %o++) {
      %user = %this.view.userSwatch.getObject(%o);
      %name = strreplace(%user.text.rawtext, " ", "_");
      %blid = %user.text.blid;
      if(%word $= ("@" @ %name)) {
        %msg = setWord(%msg, %i, "<spush><font:verdana bold:12><color:" @ GlassLive.color_self @ ">" @ %word @ "<spop>");
      } else if(%word $= ("@" @ %blid)) {
        %msg = setWord(%msg, %i, "<spush><font:verdana bold:12><color:" @ GlassLive.color_self @ ">" @ %word @ "<spop>");
      }
    }

    %name = strreplace($Pref::Player::NetName, " ", "_");

    if(%word $= ("@" @ %name)) {
      %mentioned = true;
    } else if(%word $= ("@" @ getNumKeyId())) {
      %mentioned = true;
    }
  }

  %text = "<font:verdana bold:12><color:" @ %color @ ">" @ %sender.username @ ":<font:verdana:12><color:333333> " @ %msg;
  %this.pushText(%text);

  %this.view.setFlashing(true);

  if(GlassSettings.get("Live::RoomChatSound"))
    alxPlay(GlassChatroomMsgAudio);

  if(%senderblid != getNumKeyId()) {
    if(%mentioned && GlassSettings.get("Live::RoomMentionNotification")) {
      if($Glass::LastMentioned $= "" || $Sim::Time > $Glass::LastMentioned) {
        if(!%this.awake) {
          GlassNotificationManager::newNotification(%this.name, "You were mentioned by <font:verdana bold:13>" @ %sender.username @ " (" @ %senderblid @ ")", "bell", 0);
        }

        alxPlay(GlassUserMentionedAudio);

        $Glass::LastMentioned = $Sim::Time + 10;
      }
    } else if(GlassSettings.get("Live::RoomChatNotification")) {
      if(!%this.awake)
        GlassNotificationManager::newNotification(%this.name, %sender.username @ ": " @ %msg, "comment", 0);
    }
  }
}

function GlassLiveRoom::pushText(%this, %msg) {
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getWord(%msg, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0 || strpos(%word, "glass://") == 0) {
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %msg = setWord(%msg, %i, %word);
    }
    if(getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":") {
      %bitmap = strlwr(stripChars(%word, "[]\\/{};:'\"<>,./?!@#$%^&*-=+`~;"));
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:" @ %bitmap @ ">";
        %msg = setWord(%msg, %i, %word);
      } else {
        %word = " ";
        %msg = setWord(%msg, %i, %word);
      }
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
  if(GlassOverlayGui.isAwake()) {
    %chatroom.chattext.forceReflow();
  }

  %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
  %chatroom.scrollSwatch.setVisible(true);

  %lp = %chatroom.getLowestPoint() - %chatroom.scroll.getLowestPoint();

  if(%lp >= -50) {
    %chatroom.scroll.scrollToBottom();
  }
}

function GlassLiveRoom::getOrderedUserList(%this) {
  %admins = new GuiTextListCtrl();
  %mods = new GuiTextListCtrl();
  %friends = new GuiTextListCtrl();
  %users = new GuiTextListCtrl();

  for(%i = 0; %i < %this.getCount(); %i++) {
    %user = %this.getUser(%i);
    if(%user.isAdmin()) {
      %admins.addRow(%i, %user.username);
    } else if(%user.isMod()) {
      %mods.addRow(%i, %user.username);
    } else if(%user.isFriend()) {
      %friends.addRow(%i, %user.username);
    } else {
      %users.addRow(%i, %user.username);
    }
  }

  %admins.sort(0);
  %mods.sort(0);
  %friends.sort(0);
  %users.sort(0);

  %idList = "";

  for(%i = 0; %i < %admins.rowCount(); %i++) {
    %idList = %idList SPC %admins.getRowId(%i);
  }

  for(%i = 0; %i < %mods.rowCount(); %i++) {
    %idList = %idList SPC %mods.getRowId(%i);
  }

  for(%i = 0; %i < %friends.rowCount(); %i++) {
    %idList = %idList SPC %friends.getRowId(%i);
  }

  for(%i = 0; %i < %users.rowCount(); %i++) {
    %idList = %idList SPC %users.getRowId(%i);
  }

  %admins.delete();
  %mods.delete();
  %friends.delete();
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
      %colorCode = 3;
    } else if(%user.isMod()) {
      %colorCode = 2;
    } else if(%user.isFriend()) {
      %colorCode = 1;
    } else {
      %colorCode = 0;
    }

    %icon = %user.icon;

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
      extent = "16 16";
      position = "1 3";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon;
    };

    %swatch.text = new GuiTextCtrl() {
      profile = "GlassFriendTextProfile";
      text = collapseEscape("\\c" @ %colorCode) @ %user.username;
      rawtext = %user.username;
      blid = %user.blid;
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

  // %userSwatch.getGroup().scrollToTop();
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
    if(%this.user.blid != getNumKeyId()) {
      GlassLive::openUserWindow(%this.user.blid);
      //glassMessageBoxYesNo("Add Friend", "Add <font:verdana bold:13>" @ %this.user.username @ "<font:verdana:13> as a friend?", "GlassLive::sendFriendRequest(" @ %this.user.blid @ ");");
    } else {
      glassMessageBoxOk("Hey There", "That's you.");
    }
  }
}

function GlassLiveUserListSwatch::onRightMouseUp(%this) {
  if(isObject(%input = GlassChatroomWindow.activeTab.input) && %this.user.blid != getNumKeyId()) {
    %len = strlen(%input.getValue());
    %name = strreplace(%this.user.username, " ", "_");
    if(%len > 0 && getsubstr(%input.getValue(), %len - 1, %len) $= " ") {
      %input.setValue(%input.getValue() @ "@" @ %name @ " ");
    } else {
      %input.setValue(ltrim(%input.getValue() SPC "@" @ %name @ " "));
    }
  }
}

// From Crown's (2143) "Name Completion" Add-On
// Adapted for use with Glass

function GlassChatroomGui_Input::fixCasesByName(%this, %name)
{
	for(%i=0; %i < %this.getGroup().userSwatch.getCount(); %i++)
	{
		%compare = %this.getGroup().userSwatch.getObject(%i).text.rawtext;
		if(%name $= %compare)
			return %compare;
	}
	return -1;
}

function GlassChatroomGui_Input::findPartialName(%this, %partialName)
{
	%partialName = strLwr(%partialName);
	%bestName = -1;
	%bestPos = -1;
	for(%i=0; %i < %this.getGroup().userSwatch.getCount(); %i++)
	{
    %user = %this.getGroup().userSwatch.getObject(%i);
		%name = strLwr(%user.text.rawtext);

		%pos = strStr(%name, %partialName);
		if(%pos > %bestPos)
		{
			%bestPos = %pos;
			%bestName = %name;
		}
	}
	if(%bestName == -1)
		return -1;
	return %this.fixCasesByName(%bestName);
}

function GlassChatroomGui_Input::onTabComplete(%this) {
  %text = %this.getValue();

  %last = getWord(%text, getWordCount(%text) - 1);
  %closeName = %this.findPartialName(%last);

  if(strLen(%last) < 2 || %closeName == -1)
  {
    return;
  }

  if(%closeName != -1)
  {
    %text = removeWord(%text, getWordCount(%text) - 1);
    %closeName = "@" @ strreplace(%closeName, " ", "_") @ " ";
    if(getWordCount(%text) >= 1)
      %text = %text SPC %closeName;
    else
      %text = %text @ %closeName;
    %this.setValue(%text);
  }
}
