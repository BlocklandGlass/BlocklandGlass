function GlassLiveRooms::create(%id, %name) {
  if(isObject(GlassLive.room[%id]))
    return GlassLive.room[%id];

  %room = new ScriptObject() {
    class = "GlassLiveRoom";

    id = %id;
    name = %name;

    users = "";
    view = "";

    listSize = 0;
  };


  if(!isObject(GlassLiveRoomGroup)) {
    new ScriptGroup(GlassLiveRoomGroup);
    GlassGroup.add(GlassLiveRoomGroup);

  }
  GlassLiveRoomGroup.add(%room);
  GlassLiveRooms::updatePersistence();

  GlassLive.room[%id] = %room;

  return %room;
}

function GlassLiveRooms::updatePersistence() {
  %roomStr = "";
  for(%i = 0; %i < GlassLiveRoomGroup.getCount(); %i++) {
    %obj = GlassLiveRoomGroup.getObject(%i);
    if(%obj.deleted) continue;

    %roomStr = %roomStr SPC %obj.id;
  }

  GlassSettings.update("Live::Rooms", trim(%roomStr));
}

function GlassLiveRooms::getFromId(%id) {
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

  if(GlassSettings.get("Live::MessageLogging")) {
    %room.writeLog("\n\nJoined \"" @ %data.title @ "\" at " @ getDateTime());
  }

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

  %motd = "<font:verdana bold:" @ GlassSettings.get("Live::FontSize") @ "><color:666666> * " @ %motd;

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
  %this.deleted = true;

  %this.writeLog("Left room at " @ getDateTime());

  GlassLiveRooms::updatePersistence();
}

function GlassLiveRoom::addUser(%this, %blid) {
  if(%this.user[%blid])
    return;

  %this.user[%blid] = true;
  %this.users = trim(%this.users TAB %blid);

  if(isObject(%this.view)) {
    %user = GlassLiveUser::getFromBlid(%blid);
    %this.userListAdd(%user);
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
  %this.userListBuild();

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

  %this.userListRemoveBLID(%blid);
}

function GlassLiveRoom::pushMessage(%this, %sender, %msg, %data) {
  %now = getRealTime();
  if(%now-%this.lastMessageTime > 1000 * 60 * 5) {
    %text = "<font:verdana bold:" @ GlassSettings.get("Live::FontSize") @ "><just:center><color:999999>[" @ formatTimeHourMin(%data.datetime) @ "]<just:left>";
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

    if(strPos(%word, "@") == 0 && strlen(%word) > 1) {
      if(%word $= "@here" || %word $= "@room") {

        if(%sender.isAdmin()) {
          %hlColor = GlassLive.color_admin;
        } else if(%sender.isMod()) {
          %hlColor = GlassLive.color_mod;
        } else if(%sender.isBot()) {
          %hlColor = GlassLive.color_bot;
        } else {
          continue;
        }

        %mentioned = true;
        %msg = setWord(%msg, %i, "<spush><font:verdana bold:" @ GlassSettings.get("Live::FontSize") @ "><color:" @ %hlColor @ ">" @ %word @ "<spop>");

      } else {

        for(%o = 0; %o < %this.view.userSwatch.getCount(); %o++) {
          %user = %this.view.userSwatch.getObject(%o);
          %name = getASCIIString(strreplace(%user.text.rawtext, " ", "_"));
          %blid = %user.text.blid;
          if(%word $= ("@" @ %name) || %word $= ("@" @ %blid)) {
            %msg = setWord(%msg, %i, "<spush><font:verdana bold:" @ GlassSettings.get("Live::FontSize") @ "><linkcolor:" @ GlassLive.color_self @ "><a:gamelink_glass://user-" @ %blid @ ">" @ %word @ "</a><spop>");
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

      }
    }

    %name = getASCIIString(strreplace($Pref::Player::NetName, " ", "_"));

    if(%word $= ("@" @ %name)) {
      %mentioned = true;
    } else if(%word $= ("@" @ getNumKeyId())) {
      %mentioned = true;
    }
  }
  %text = "<font:verdana bold:" @ GlassSettings.get("Live::FontSize") @ "><sPush><linkcolor:" @ %color @ "><a:gamelink_glass://user-" @ %sender.blid @ ">" @ %sender.username @ "</a><sPop>:<font:verdana:" @ GlassSettings.get("Live::FontSize") @ "><color:333333> " @ %msg;
  %this.pushText(%text);

  %this.view.setFlashing(true);

  GlassLive.curSound = !GlassLive.curSound;

  GlassAudio::play("chatroomMsg" @ GlassLive.curSound + 1, GlassSettings.get("Volume::RoomChat"));

  if(%senderblid != getNumKeyId()) {
    if(%mentioned && GlassSettings.get("Live::RoomMentionNotification") && %sender.canSendMessage()) {
      if(GlassLive.lastMentioned $= "" || $Sim::Time > GlassLive.lastMentioned) {
        if(!%this.view.isAwake()) {
          if(GlassSettings.get("Live::ReminderIcon"))
            GlassMessageReminder.setVisible(true);

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
          text = GlassLive::getEmotedMessage(%msg);
          image = %this.icon;

          sticky = false;
          callback = "";
        };
      }
    }
  }
}

function GlassLive::updateFontSize() {
  return;

  for(%i=0; %i < 4; %i++) {
    %room = GlassLive.room[%i];
    if(!isObject(%room))
      continue;

    %text = %room.view.chattext.getValue();
    %oldSize = getSubStr(%text, stripos(%text, "<font verdana bold:"), 1);

  }
}
function GlassLiveRoom::pushText(%this, %msg) {
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getWord(%msg, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0 || strpos(%word, "glass://") == 0) {
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %msg = setWord(%msg, %i, %word);
    }
    %validEmote = (getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":" && getsubstr(%word, 1, 1) !$= ":" && getsubstr(%word, strlen(%word) - 2, 1) !$= ":");
    if(%validEmote) {
      %bitmap = strlwr(stripChars(%word, "[]\\/{};:'\"<>,./?!@#$%^&*-=+`~;"));
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:" @ %bitmap @ ">";
        %msg = setWord(%msg, %i, strlwr(%word));
      }
    }
  }

  %fontSize = GlassSettings.get("Live::FontSize");

  %timestampedMsg = %msg = "<font:verdana:12><color:666666>[" @ getWord(getDateTime(), 1) @ "]" SPC %msg;
  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = %timestampedMsg;
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

  if(GlassSettings.get("Live::MessageLogging")) {
    %this.writeLog(%timestampedMsg);
  }
}

function GlassLiveRoom::writeLog(%this, %msg) {
  %msg = strReplace(%msg, "<br>", "\n");

  %file = "config/client/blg/chat_log/rooms/" @ %this.id @ "/" @ strReplace(getWord(getDateTime(), 0), "/", ".") @ ".txt";

  %fo = new FileObject();
  %fo.openForAppend(%file);

  for(%i = 0; %i < getLineCount(%msg); %i++) {
    %fo.writeLine(stripMlControlChars(getLine(%msg, %i)));
  }

  %fo.close();
  %fo.delete();
}

//
// User List
//

function GlassLiveRoom::userListHeader(%this, %colorCode, %headerText, %before) {
  %userSwatch = %this.view.userswatch;

  %text = new GuiTextCtrl() {
	 profile = "GlassFriendTextHeaderProfile";
	 text = collapseEscape("\\c" @ %colorCode) @ %headerText;
	 extent = "110 16";
	 position = "5 0";

	 used = true;
  };

  %userSwatch.add(%text);
  if(%before)
  	%text.placeBelow(%before, 5);

  return %text;
}

function GlassLiveRoom::userListAdd(%this, %user, %batched) {
  if(isObject(%this.listSwatchBlid[%user.blid])) {
    error("Attempted to add duplicate user (" @ %user.blid @ ") to glass room!");
    return;
  }
  %rank = 9;

  if(%user.isBot()) {
    %rank = 2;
    %this.userListAddHeader(%rank, 5, "Bots");
  } else if(%user.isAdmin()) {
    %rank = 0;
    %this.userListAddHeader(%rank, 4, "Administrators");
  } else if(%user.isMod()) {
    %rank = 1;
    %this.userListAddHeader(%rank, 3, "Moderators");
  } else if(%user.isFriend()) { // TODO setting
    %rank = 3;
    %this.userListAddHeader(%rank, 2, "Friends");
  } else {
    %this.userListAddHeader(%rank, 0, "Online");
  }

  %srt = %rank SPC %user.username;

  // find insert pos
  %insert = 0;
  for(%i = 0; %i < %this.listSize; %i++) {
    %cmp = %this.listStrCmp[%i];

    if(stricmp(%srt, %cmp) != 1) {
      break;
    }

    %insert++;
  }

  // create swatch
  %swatch = %this.userListCreateSwatch(%user);
  %swatch.index = %insert;
  %height = getWord(%swatch.extent, 1);

  // shift all others (both position and in list)
  for(%i = %this.listSize; %i > %insert; %i--) {
    %s = %this.listSwatch[%i-1];
    %s.position = getWord(%s.position, 0) SPC (getWord(%s.position, 1)+%height);
    %s.index    = %i;

    %this.listSwatch[%i] = %s;
    %this.listStrCmp[%i] = %this.listStrCmp[%i-1];
  }

  %this.listSize++;

  // insert
  %userSwatch = %this.view.userSwatch;

  if(%insert > 0) {
    // place below predecessor
    %swatch.placeBelow(%this.listSwatch[%insert-1], 0);
  }

  %userSwatch.add(%swatch);

  if(!%batched)
    %userSwatch.verticalMatchChildren(0, 5);

  %this.listSwatch[%insert] = %swatch;
  %this.listStrCmp[%insert] = %srt;

  // store data
  %this.listSwatchBlid[%user.blid] = %swatch;

  if(%this.view.window.isAwake()) {
    %this.view.window.resize.schedule(0, onResize);
  }
}

function GlassLiveRoom::userListRemove(%this, %user, %batched) {
  %this.userListRemoveBLID(%user.blid, %batched);
}

function GlassLiveRoom::userListRemoveBLID(%this, %blid, %batched) {
  if(!isObject(%this.listSwatchBlid[%blid])) {
    //error("Attempted to remove user not present! (" @ %blid @ ")");
    return;
  }

  // declaration
  %swatch = %this.listSwatchBlid[%blid];
  %index  = %swatch.index;
  %height = getWord(%swatch.extent, 1);

  // delete
  %swatch.deleteAll();
  %swatch.delete();

  // decrement list size
  %this.listSize--;

  // shift list
  for(%i = %index; %i < %this.listSize; %i++) {
    %s = %this.listSwatch[%i+1];
    %s.position = getWord(%s.position, 0) SPC (getWord(%s.position, 1)-%height);
    %s.index    = %i;

    %this.listSwatch[%i] = %s;
    %this.listStrCmp[%i] = %this.listStrCmp[%i+1];
  }

  // resize
  %userSwatch = %this.view.userSwatch;

  if(!%batched)
    %userSwatch.verticalMatchChildren(0, 5);

  // clear
  %this.listSwatchBlid[%user.blid] = "";



  // clean headers
  if(!%batched)
    %this.userListCleanHeaders();

  if(%this.view.window.isAwake()) {
    %this.view.window.resize.schedule(0, onResize);
  }
}

function GlassLiveRoom::userListCleanHeaders(%this) {
  %lastWasHeader = false;

  for(%j = 0; %j < %this.listSize; %j++) {
    %swatch = %this.listSwatch[%j];
    %header = %swatch.userListHeader;

    if(%header && (%j == %this.listSize-1 || %lastWasHeader)) {
      if(%lastWasHeader) {
        %index  = %j-1; //remove previous
      } else if(%j == %this.listSize-1) {
        %index = %j; //remove this
      }


      %swat = %this.listSwatch[%index];
      %height = getWord(%swat.extent, 1);

      %swat.deleteAll();
      %swat.delete();

      %this.listSize--;

      for(%i = %index; %i < %this.listSize; %i++) {
        %s = %this.listSwatch[%i+1];
        %s.position = getWord(%s.position, 0) SPC (getWord(%s.position, 1)-%height);
        %s.index    = %i;

        %this.listSwatch[%i] = %s;
        %this.listStrCmp[%i] = %this.listStrCmp[%i+1];
      }

      %j--;

    }

    %lastWasHeader = %header;
  }

  // resize
  %userSwatch = %this.view.userSwatch;
  %userSwatch.verticalMatchChildren(0, 5);
}

function GlassLiveRoom::userListAddHeader(%this, %rank, %colorCode, %text) {
  if(isObject(%this.listHeader[%rank]))
    return;

  if(!GlassSettings.get("Live::RoomHeaders"))
    return;

  %srt = %rank;

  %insert = 0;
  for(%i = 0; %i < %this.listSize; %i++) {
    %cmp = %this.listStrCmp[%i];

    if(stricmp(%srt, %cmp) != 1) {
      break;
    }

    %insert++;
  }

  // create swatch
  %swatch = %this.userListCreateHeader(%colorCode, %text);
  %swatch.index = %insert;
  %height = getWord(%swatch.extent, 1);

  // shift all others (both position and in list)
  for(%i = %this.listSize; %i > %insert; %i--) {
    %s = %this.listSwatch[%i-1];
    %s.position = getWord(%s.position, 0) SPC (getWord(%s.position, 1)+%height);
    %s.index    = %i;

    %this.listSwatch[%i] = %s;
    %this.listStrCmp[%i] = %this.listStrCmp[%i-1];
  }

  %this.listSize++;

  // insert
  %userSwatch = %this.view.userSwatch;

  if(%insert > 0) {
    // place below predecessor
    %swatch.placeBelow(%this.listSwatch[%insert-1], 0);
  }

  %userSwatch.add(%swatch);
  %userSwatch.verticalMatchChildren(0, 5);
  %this.listSwatch[%insert] = %swatch;
  %this.listStrCmp[%insert] = %srt;

  %this.listHeader[%rank] = %swatch;
}

function GlassLiveRoom::userListCreateHeader(%this, %colorCode, %headerText) {
  %userSwatch = %this.view.userswatch;

  %swatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "3 0";
    extent = "110 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
    userListHeader = true;
  };

  %swatch.text = new GuiTextCtrl() {
	 profile = "GlassFriendTextHeaderProfile";
	 text = collapseEscape("\\c" @ %colorCode) @ %headerText;
	 extent = "100 16";
	 position = "5 2";
  };

  %swatch.add(%swatch.text);

  return %swatch;
}

function GlassLiveRoom::userListCreateSwatch(%this, %user) {
  if(%user.isBot()) {
    %colorCode = 5;
  } else if(%user.isAdmin()) {
    %colorCode = 4;
  } else if(%user.isMod()) {
    %colorCode = 3;
  } else if(%user.blid == getNumKeyId()) {
    %colorCode = 1;
  } else if(%user.isFriend()) {
    %colorCode = 2;
  } else {
    %colorCode = 0;
  }

  %icon = %user.icon;
  if(%icon $= "")
    %icon = "ask_and_answer";

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
  %this.userListSwatch[%user.blid] = %swatch;

  return %swatch;
}

function GlassLiveRoom::userListUpdate(%this, %user) {
  if(!isObject(%this.userListSwatch[%user.blid]))
    return; //this is due to laziness, need better code

  // basically need to remove and add back
  %this.userListRemove(%user, true);
  %this.userListAdd(%user);
}

function GlassLiveRoom::userListBuild(%this, %do) {
  %startTime = getRealTime();

  cancel(%this.renderUserSch);
  if(!%do) {
    %this.renderUserSch = %this.schedule(100, userListBuild, true);
    return;
  }

  %userSwatch = %this.view.userswatch;

  for(%i = 0; %i < getWordCount(%this.users); %i++) {
    %user = %this.getUser(%i);

    if(!isObject(%this.listSwatchBlid[%user.blid])) {
      %this.userListAdd(%user, true);
    }
  }

  %userSwatch.verticalMatchChildren(0, 5);

// echo("\c2User list (room " @ %this.id @ ") built in " @ (getRealTime() - %startTime) @ "ms");
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

function GlassLiveUserListSwatch::onMouseUp(%this, %mod, %point, %count) {
  %this.swatch.color = "220 220 220 255";
  if(%this.down) {
    %this.down = false;

    if(%mod == 1 && GlassLiveUser::getFromBlid(getNumKeyId()).isMod()) {
      GlassOverlay::openModeration(true);
      GlassModeratorWindow_BLID.setValue(%this.user.blid);
      GlassModeratorGui.updateBLID();
      return;
    }

    //if(%this.group)
    //  %this.group.displayUserOptions(%this.user);
    //else
    if(!isObject(%this.user)) {
      if(isObject(%server = getServerFromIP(%this.currentServer)))
        GlassServerPreviewGui.open(%server);
      return;
    }


    if(%this.user.isBot()) {
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
  if(%this.user.blid != getNumKeyId()) {
    if(isObject(%input = GlassChatroomWindow.activeTab.input)) {
      %len = strlen(%input.getValue());
      %name = strreplace(%this.user.username, " ", "_");
      if(%len > 0 && getsubstr(%input.getValue(), %len - 1, %len) $= " ") {
        %input.setValue(%input.getValue() @ "@" @ %name @ " ");
      } else {
        %input.setValue(ltrim(%input.getValue() SPC "@" @ %name @ " "));
      }
    }
  } else {
    if(%this.user.blid == getNumKeyId()) {
      if(GlassIconSelectorWindow.visible) {
        GlassOverlay::closeIconSelector();
      } else {
        GlassOverlay::openIconSelector();
      }
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
  if(isObject(GlassEmoteSelMouse)) {
    if(getWordCount(%this.possEmoteList) == 0)
      GlassEmoteSelMouse.onMouseDown();

    return;
  }
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
