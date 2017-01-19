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

function GlassLive::onJoinRoom(%data) {
  if(GlassSettings.get("Live::RoomNotification")) {
    new ScriptObject(GlassNotification) {
      title = "Entered Room";
      text = "You've entered " @ %data.title;
      image = "add";

      sticky = false;
      callback = "";
    };
  }

  %room = GlassLiveRooms::create(%data.id, %data.title);

  %room.icon = %data.icon;

  %clients = %data.clients;
  for(%i = 0; %i < %clients.length; %i++) {
    %cl = %clients.value[%i];

    %uo = GlassLiveUser::create(%cl.username, %cl.blid);
    %uo.setStatus(%cl.status);
    %uo.setIcon(%cl.icon);

    %uo.setAdmin(%cl.admin);
    %uo.setMod(%cl.mod);

    if(%cl.blid < 0)
      %uo.setBot(true);

    %room.addUser(%uo.blid);
  }

  %room.createView();

  %motd = %data.motd;
  %motd = strreplace(%motd, "\n", "<br> * ");
  %motd = strreplace(%motd, "[name]", $Pref::Player::NetName);
  %motd = strreplace(%motd, "[vers]", Glass.version);
  %motd = strreplace(%motd, "[date]", getWord(getDateTime(), 0));
  %motd = strreplace(%motd, "[time]", getWord(getDateTime(), 1));

  %motd = "<font:verdana bold:12><color:666666> * " @ %motd;

  %room.pushText(%motd);

  %room.view.userSwatch.getGroup().scrollToTop();
}

function GlassLiveRoom::leaveRoom(%this, %inhibitNotification) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomLeave");
  %obj.set("id", "string", %this.id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  if(GlassSettings.get("Live::RoomNotification") && !%inhibitNotification) {
    new ScriptObject(GlassNotification) {
      title = "Exited Room";
      text = "You've exited " @ %this.name;
      image = "delete";

      sticky = false;
      callback = "";
    };
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
      break;
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
    %text = "<font:verdana:12><color:666666>" @ %user.username @ " (" @ %blid @ ") entered the room.";
    %this.pushText(%text);
  }
  %this.addUser(%blid);
}

function GlassLiveRoom::onUserLeave(%this, %blid) {
  %chatroom = %this.view;

  %this.removeUser(%blid);

  %user = GlassLiveUser::getFromBlid(%blid);

  if(GlassSettings.get("Live::ShowJoinLeave")) {
    if(%user == false)
      %user = "BLID_" @ %blid; // todo local caching
    else
      %user = %user.username;

    %text = "<font:verdana:12><color:666666>" @ %user @ " (" @ %blid @ ") exited the room.";
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
  } else if(%sender.isBot()) {
    %color = GlassLive.color_bot;
  } else if(%sender.isAdmin()) {
    %color = GlassLive.color_admin;
  } else if(%sender.isMod()) {
    %color = GlassLive.color_mod;
  // } else if(%sender.isBlocked()) {
    // %color = GlassLive.color_blocked;
  } else if(%sender.isFriend()) {
    %color = GlassLive.color_friend;
  } else {
    %color = GlassLive.color_default;
  }

  %msg = stripMlControlChars(%msg);
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getASCIIString(getWord(%msg, %i));
    for(%o = 0; %o < %this.view.userSwatch.getCount(); %o++) {
      %user = %this.view.userSwatch.getObject(%o);
      %name = getASCIIString(strreplace(%user.text.rawtext, " ", "_"));
      %blid = %user.text.blid;
      if(%word $= ("@" @ %name) || %word $= ("@" @ %blid)) {
        %msg = setWord(%msg, %i, "<spush><font:verdana bold:12><color:" @ GlassLive.color_self @ ">" @ %word @ "<spop>");
        %uo = GlassLiveUser::getFromBlid(%blid);
        if(%senderblid == getNumKeyId()) {
          if(%uo.getStatus() $= "away") {
            glassMessageBoxOk("Away", "The user you just mentioned is currently away.");
          } else if(%uo.getStatus() $= "busy") {
            glassMessageBoxOk("Busy", "The user you just mentioned is currently busy.");
          }
        }
      }
    }

    %name = getASCIIString(strreplace($Pref::Player::NetName, " ", "_"));

    if(%word $= ("@" @ %name)) {
      %mentioned = true;
    } else if(%word $= ("@" @ getNumKeyId())) {
      %mentioned = true;
    }
  }
  %text = "<font:verdana bold:12><sPush><linkcolor:" @ %color @ "><a:gamelink_glass://user-" @ %sender.blid @ ">" @ %sender.username @ "</a><sPop>:<font:verdana:12><color:333333> " @ %msg;
  %this.pushText(%text);

  %this.view.setFlashing(true);

  GlassLive.curSound = !GlassLive.curSound;

  GlassAudio::play("chatroomMsg" @ GlassLive.curSound + 1, GlassSettings.get("Volume::RoomChat"));

  if(%senderblid != getNumKeyId()) {
    if(%mentioned && GlassSettings.get("Live::RoomMentionNotification") && %sender.canSendMessage()) {
      if(GlassLive.lastMentioned $= "" || $Sim::Time > GlassLive.lastMentioned) {
        if(!%this.view.isAwake()) {
          new ScriptObject(GlassNotification) {
            title = "Mentioned in " @ %this.name;
            text = "You were mentioned by <font:verdana bold:13>" @ %sender.username @ " (" @ %senderblid @ ")";
            image = "bell";

            sticky = false;
            callback = "";
          };
        }

        GlassAudio::play("bell");

        GlassLive.lastMentioned = $Sim::Time + 10;
      }
    } else if(GlassSettings.get("Live::RoomChatNotification")) {
      if(!%this.view.isAwake()) {
        %msg = %sender.username @ ": " @ %msg;

        if(strlen(%msg) > 100)
          %msg = getsubstr(%msg, 0, 100) @ "...";

        new ScriptObject(GlassNotification) {
          title = %this.name;
          text = %msg;
          image = %this.icon;

          sticky = false;
          callback = "";
        };
      }
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
        %msg = setWord(%msg, %i, strlwr(%word));
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
  } else {
    %chatroom.chattext.didUpdate = true;
  }

  %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
  %chatroom.scrollSwatch.setVisible(true);

  %lp = %chatroom.getLowestPoint() - %chatroom.scroll.getLowestPoint();

  if(%lp >= -50) {
    %chatroom.scroll.scrollToBottom();
  }
}

function GlassLiveRoom::getOrderedUserList(%this) {
  %mods = new GuiTextListCtrl();
  %admins = new GuiTextListCtrl();
  %bots = new GuiTextListCtrl();
  // %friends = new GuiTextListCtrl();
  %users = new GuiTextListCtrl();

  for(%i = 0; %i < %this.getCount(); %i++) {
    %user = %this.getUser(%i);
    if(%user.isBot()) {
      %bots.addRow(%i, %user.username);
    } else if(%user.isAdmin()) {
      %admins.addRow(%i, %user.username);
    } else if(%user.isMod()) {
      %mods.addRow(%i, %user.username);
    // } else if(%user.isFriend()) {
      // %friends.addRow(%i, %user.username);
    } else {
      %users.addRow(%i, %user.username);
    }
  }

  %bots.sort(0);
  %admins.sort(0);
  %mods.sort(0);
  // %friends.sort(0);
  %users.sort(0);

  %idList = "";

  for(%i = 0; %i < %admins.rowCount(); %i++) {
    %idList = %idList SPC %admins.getRowId(%i);
  }

  for(%i = 0; %i < %mods.rowCount(); %i++) {
    %idList = %idList SPC %mods.getRowId(%i);
  }

  for(%i = 0; %i < %bots.rowCount(); %i++) {
    %idList = %idList SPC %bots.getRowId(%i);
  }

  // for(%i = 0; %i < %friends.rowCount(); %i++) {
    // %idList = %idList SPC %friends.getRowId(%i);
  // }

  for(%i = 0; %i < %users.rowCount(); %i++) {
    %idList = %idList SPC %users.getRowId(%i);
  }

  %bots.delete();
  %admins.delete();
  %mods.delete();
  // %friends.delete();
  %users.delete();

  return trim(%idList);
}

function GlassLiveRoom::renderUserList(%this, %do) {
  cancel(%this.renderUserSch);
  if(!%do) {
    %this.renderUserSch = %this.schedule(100, renderUserList, true);
    return;
  }

  %userSwatch = %this.view.userswatch;

  %orderedList = %this.getOrderedUserList();

  for(%i = 0; %i < getWordCount(%orderedList); %i++) {
    %user = %this.getUser(getWord(%orderedList, %i));

    if(%user.isBot()) {
      %colorCode = 5;
    } else if(%user.isAdmin()) {
      %colorCode = 4;
    } else if(%user.isMod()) {
      %colorCode = 3;
    } else if(%user.blid == getNumKeyId()) {
      %colorCode = 1;
    // } else if(%user.isBlocked()) {
      // %colorCode = 6;
    } else if(%user.isFriend()) {
      %colorCode = 2;
    } else {
      %colorCode = 0;
    }

    %icon = %user.icon;
    if(%icon $= "")
      %icon = "ask_and_answer";

    if(!isObject(%userSwatch.blid[%user.blid])) {
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
        icon = %icon;
        mKeepCached = "1";
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
      %userSwatch.blid[%user.blid] = %swatch;
    } else {
      %swatch = %userSwatch.blid[%user.blid];
      if(%swatch.icon.icon !$= %icon) {
        %swatch.icon.icon = %icon;
        %swatch.icon.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon);
      }

      %text = collapseEscape("\\c" @ %colorCode) @ %user.username;
      if(%swatch.text.text !$= %text) {
        %swatch.text.setText(%text);
      }
    }

    %swatch.used = true;

    if(%last $= "") {
      %swatch.position = "3 3";
    } else {
      %swatch.placeBelow(%last, 0);
    }

    %last = %swatch;
    %userSwatch.add(%swatch);

    %this.userListSwatch[%user.blid] = %swatch;
  }

  for(%i = 0; %i < %userSwatch.getCount(); %i++) {
    %obj = %userSwatch.getObject(%i);
    if(!%obj.used) {
      %obj.deleteAll();
      %obj.delete();
      %i--;
      continue;
    } else {
      %obj.used = false;
    }
  }

  // %userSwatch.getGroup().scrollToTop();
  %userSwatch.verticalMatchChildren(0, 5);
  %userSwatch.setVisible(true);
}

function GlassLiveUserListSwatch::onMouseEnter(%this) {
  %this.swatch.color = "220 220 220 255";

  if(getWord(%this.swatch.text.extent, 0) > getWord(vectorSub(%this.swatch.extent, %this.swatch.pos), 0)-20)
    if(%this.swatch.scrollTick $= "")
      %this.swatch.scrollTick = %this.scrollLoop(%this.swatch.text, true);
}

function GlassLiveUserListSwatch::onMouseLeave(%this) {
  %this.swatch.color = "160 160 160 0";
  %this.down = false;

  %this.scrollEnd(%this.swatch.text);
}

function GlassLiveUserListSwatch::onMouseDown(%this) {
  %this.swatch.color = "150 150 255 255";
  %this.down = true;

  alxPlay(GlassClick1Audio);
}

function GlassLiveUserListSwatch::onMouseUp(%this) {
  %this.swatch.color = "220 220 220 255";
  if(%this.down) {
    %this.down = false;
    //if(%this.group)
    //  %this.group.displayUserOptions(%this.user);
    //else
	if(!isObject(%this.user)) {
	  if(isObject(%server = getServerFromIP(%this.currentServer)))
		  GlassServerPreviewGui.open(%server);
	  return;
	}

    if(%this.user.blid == getNumKeyId()) {
      if(GlassIconSelectorWindow.visible)
        GlassOverlay::closeIconSelector();
      else
        GlassOverlay::openIconSelector();
    } else if(%this.user.isBot()) {
      glassMessageBoxOk("Beep Boop", "That's a bot!");
    } else {
      if(isObject(%this.user.window))
        %this.user.window.delete();
      else
        GlassLive::openUserWindow(%this.user.blid);
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

function GlassLiveUserListSwatch::scrollLoop(%this, %text, %reset) {
  if(%reset) {
    %this.swatch._scrollOrigin = %this.swatch.text.position;
    %this.swatch._scrollOrigin_Icon = %this.swatch.icon.position;
    %this.swatch._scrollOffset = 0;
    %this.swatch._scrollRange = getWord(%this.swatch.text.extent, 0)-getWord(%this.swatch.extent, 0)+getWord(%this.swatch.text.position, 0)+5;
  }

  %this.swatch.text.position = vectorSub(%this.swatch._scrollOrigin, %this.swatch._scrollOffset);
  %this.swatch.icon.position = vectorSub(%this.swatch._scrollOrigin_Icon, %this.swatch._scrollOffset);

  if(%this.swatch._scrollOffset >= %this.swatch._scrollRange) {
    %this.swatch._scrollOffset = 0;
    // %this.swatch.scrollTick = %this.schedule(2000, scrollLoop, %text);
  } else {
    %this.swatch._scrollOffset++;
    %this.swatch.scrollTick = %this.schedule(25, scrollLoop, %text);
  }
}

function GlassLiveUserListSwatch::scrollEnd(%this, %text) {
  cancel(%this.swatch.scrollTick);
  %this.swatch.text.position = %this.swatch._scrollOrigin;
  %this.swatch.icon.position = %this.swatch._scrollOrigin_Icon;
  %this.swatch.scrollTick = "";
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
