function GlassLiveGroup::startNew(%user1, %user2, %user3, %user4, %user5) {
  %users = JettisonArray();
  for(%i = 0; %i < 5; %i++) {
    if(%user[%i+1] !$= "") {
      %users.push("string", %user[%i+1]);
    }
  }
  %obj = JettisonObject();
  %obj.set("type", "string", "groupCreate");
  %obj.set("invite", "object", %users);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %users.delete();
  %obj.delete();
}

function GlassLiveGroup::create(%id, %users) {
  if(isObject(GlassLiveGroups.group[%id])) {
    return GlassLiveGroups.group[%id];
  }

  %group = new ScriptObject(GlassLiveGroup) {
    id = %id;
  };

  if(isObject(%users)) {
    for(%i = 0; %i < %users.length; %i++) {
      %ud = %users.value[%i];
      GlassLiveUser::create(%ud.username, %ud.blid);
      %group.addUser(%ud.blid);
    }
  }

  GlassLiveGroups.add(%group);
  GlassLiveGroups.group[%id] = %group;
}

function GlassLiveGroup::getFromId(%id) {
  if(isObject(GlassLiveGroups.group[%id])) {
    return GlassLiveGroups.group[%id];
  } else {
    return false;
  }
}

function GlassLiveGroup::leaveId(%id) {
  %group = GlassLiveGroup::getFromId(%id);

  %obj = JettisonObject();
  %obj.set("type", "string", "groupLeave");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();

  %group.tab.getGroup().delete();
  %group.tab.deleteAll();
  %group.tab.delete();
  %group.delete();
}

function GlassLiveGroup::inputSend(%id) {
  if(isObject(%id)) {
    %this = %id;
  } else {
    %this = GlassLiveGroup::getFromId(%id);
  }

  if(!%this)
    return;

  %val = trim(%this.tab.input.getValue());
  %val = stripMlControlChars(%val);
  if(%val $= "")
    return;

  if(strPos(%val, "/") != 0) {
    %obj = JettisonObject();
    %obj.set("type", "string", "groupMessage");
    %obj.set("msg", "string", %val);
    %obj.set("id", "string", %id);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
    %obj.delete();
  }

  %this.tab.input.setValue("");
}

function GlassLiveGroup::displayUserOptions(%this, %user) {
  %overlay = %this.tab.userOverlay;
  %overlay.setVisible(true);
  %overlay.deleteAll();

  %overlay.text = new GuiTextCtrl() {
    profile = "GlassFriendTextProfile";
    text = %user.username;
    extent = "45 18";
    position = "10 10";
  };

  %button = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";

    horizSizing = "right";
    vertSizing = "bottom";
    position = "36 40";
    extent = 50 SPC 20;
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Friend";
    groupNum = "-1";
    buttonType = "PushButton";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";

    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
  };

  %overlay.add(%overlay.text);
  %overlay.add(%button);

  %overlay.text.centerX();
  %button.centerX();
}

function GlassLiveGroup::pushMessage(%this, %sender, %msg) {
  %senderblid = %sender.blid;

  if(%senderblid == getNumKeyId()) {
    %color = GlassLive.color_self;
  } else if(GlassLive.isFriend[%senderblid]) {
    %color = GlassLive.color_friend;
  } else {
    %color = GlassLive.color_default;
  }

  %msg = stripMlControlChars(%msg);
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getWord(%msg, %i);
    if(%word $= $Pref::Player::NetName) {
      %mentioned = true;
      %msg = setWord(%msg, %i, " <spush><font:verdana bold:12><color:" @ GlassLive.color_self @ ">" @ %word @ "<spop>");
    }
  }


  %text = "<font:verdana bold:12><color:" @ %color @ ">" @ %sender.username @ ":<font:verdana:12><color:333333> " @ %msg;
  %this.pushText(%text);

  if(GlassSettings.get("Live::RoomChatSound"))
    alxPlay(GlassChatroomMsgAudio);

  if(%senderblid != getNumKeyId()) {
    if(%mentioned && GlassSettings.get("Live::RoomMentionNotification")) {
      if(!%this.awake) {
        GlassNotificationManager::newNotification(%this.name, "You were mentioned by <font:verdana bold:13>" @ %sender.username @ " (" @ %senderblid @ ")", "bell", 0);
      }
      
      alxPlay(GlassUserMentionedAudio);
    } else if(GlassSettings.get("Live::RoomChatNotification")) {
      if(!%this.awake) {
        GlassNotificationManager::newNotification(%this.name, %sender.username @ ": " @ %msg, "comment", 0);
      }
    }
  }
}

function GlassLiveGroup::pushText(%this, %msg) {
  for(%i = 0; %i < getWordCount(%msg); %i++) {
    %word = getWord(%msg, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0) {
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %msg = setWord(%msg, %i, %word);
    }
    if(getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":") {
      %bitmap = stripChars(%word, "[]\\/{};:'\"<>,./?!@#$%^&*-=+`~\";");
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:" @ %bitmap @ ">";
        %msg = setWord(%msg, %i, %word);
      }
    }
  }

  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = "<font:verdana:12><color:666666>[" @ getWord(getDateTime(), 1) @ "]" SPC %msg;
  }

  %chatroom = %this.tab;
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

function GlassLiveGroup::createGui(%this) {
  %tab = GlassLive::createGroupchatView(%this.id);
  %tab.group = %this;

  %this.tab = %tab;

  %window = new GuiWindowCtrl(GlassGroupchatWindow) {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 131";
    extent = "445 245";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Groupchat";
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
    closeCommand = "GlassLiveGroup::leaveId(" @ %this.id @ ");";

    tabs = 0;
  };

  %tab.visible = true;
  %window.add(%tab);
  GlassOverlayGui.add(%window);

  //%tab.title = "GroupChat";

  //if(!isObject(%window)) {
  //  if(!isObject($Glass::defaultRoomWindow)) {
  //    $Glass::defaultRoomWindow = GlassLive::createChatroomWindow();
  //  }

  //  %window = $Glass::defaultRoomWindow;
  //}

  //%window.addTab(%tab);
}

function GlassGroupchatWindow::onWake(%this) {
  %tab = %this.getObject(0);

  %tab.chattext.forceReflow();
  %tab.scrollSwatch.verticalMatchChildren(0, 2);
  %tab.scrollSwatch.setVisible(true);
  %tab.scroll.scrollToBottom();
}

function GlassLiveGroup::addUser(%this, %blid) {
  if(%this.user[%blid])
    return;

  %this.user[%blid] = true;
  %this.users = trim(%this.users TAB %blid);

  if(isObject(%this.tab)) {
    %this.renderUserList();
  }
}

function GlassLiveGroup::removeUser(%this, %blid) {
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

function GlassLiveGroup::getCount(%this) {
  return getFieldCount(%this.users);
}

function GlassLiveGroup::getBl_id(%this, %idx) {
  return getField(%this.users, %idx);
}

function GlassLiveGroup::getUser(%this, %idx) {
  return GlassLiveUser::getFromBlid(%this.getBl_id(%idx));
}

function GlassLiveGroup::getOrderedUserList(%this) {
  %users = new GuiTextListCtrl();
  %admins = new GuiTextListCtrl();
  %mods = new GuiTextListCtrl();

  for(%i = 0; %i < %this.getCount(); %i++) {
    %user = %this.getUser(%i);
    %users.addRow(%i, %user.username);
  }

  %users.sort(0);

  %idList = "";
  for(%i = 0; %i < %users.rowCount(); %i++) {
    %idList = %idList SPC %users.getRowId(%i);
  }

  %users.delete();

  return trim(%idList);
}

function GlassLiveGroup::renderUserList(%this) {
  %userSwatch = %this.tab.userswatch;
  %userSwatch.deleteAll();

  %orderedList = %this.getOrderedUserList();

  for(%i = 0; %i < getWordCount(%orderedList); %i++) {
    %user = %this.getUser(getWord(%orderedList, %i));

    %icon = "user";
    %pos = "1 3";
    %ext = "16 16";

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

      type = "group";
      group = %this;
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
  %userSwatch.verticalMatchChildren(0, 5);
  %userSwatch.setVisible(true);
}
