function GlassLiveGroups::create(%id, %name) {
  if(isObject(GlassLive.group[%id]))
    return GlassLive.group[%id];

  %group = new ScriptObject() {
    class = "GlassLiveGroup";

    id = %id;
    name = %name;

    users = "";
    view = "";
  };

  if(!isObject(GlassLiveGroupGroup)) {
    new ScriptGroup(GlassLiveGroupGroup);
  }

  GlassLiveGroupGroup.add(%group);
  GlassLive.group[%id] = %group;

  return %group;
}

function GlassLiveGroups::startNew(%inviteList) {
  //%inviteList space delimited
  %invites = JettisonArray();
  for(%i = 0; %i < getWordCount(%inviteList); %i++) {
    %invites.push("string", getWord(%inviteList, %i)+0);
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "groupCreate");
  %obj.set("invites", "object", %invites);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %invites.delete();
  %obj.delete();
}

function GlassLiveGroup::openWindow(%this) {
  %window = GlassLive::createGroupchat(%this);
  if(isObject(%this.window))
    %this.window.delete();

  %this.window = %window;
  GlassOverlayGui.add(%window);
}

function GlassLiveGroup::closeWindow(%this) {
  if(isObject(%this.window))
    %this.window.delete();
}

function GlassLiveGroup::onInvite(%this) {
  if(%this.inviter !$= "") {
    %inviter = %this.inviter.username;
  } else {
    %inviter = "You've been";
  }

  %userStr = "";
  %clients = %data.clients;

  %this.inviteNotification = new ScriptObject(GlassNotification) {
    title = "Groupchat Invitiation";
    text = %inviter @ "invited to a Groupchat with " @ %userStr;
    image = "bell";

    sticky = true;
    callback = "GlassOverlay::open();";
  };
}

function GlassLive::createGroupchat(%obj) {
  %window = new GuiWindowCtrl(GlassGroupchatWindow) {
    profile = "GlassWindowProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "20 25";
    extent = "465 265";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    closeCommand = %obj @ ".closeWindow();";
    accelerator = "escape";
    text = "Groupchat - " @ %obj.name;
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
    closeCommand = %obj @ ".closeWindow();";
  };

  %container = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 35";
    extent = "455 225";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
    id = %obj.id;
    obj = %obj;
  };

  %container.scroll = new GuiScrollCtrl() {
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

  %container.scrollSwatch = new GuiSwatchCtrl() {
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

  %container.chattext = new GuiMLTextCtrl() {
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

  %container.userscroll = new GuiScrollCtrl() {
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

  %container.userswatch = new GuiSwatchCtrl() {
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

  %container.input = new GuiTextEditCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 205";
    extent = "315 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    altCommand = %window @ ".sendInput();";
    accelerator = "enter";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "1";
    sinkAllKeyEvents = "0";
  };
  %container.add(%container.scroll);
  %container.scroll.add(%container.scrollSwatch);
  %container.scrollSwatch.add(%container.chattext);
  %container.add(%container.userscroll);
  %container.userscroll.add(%container.userswatch);
  %container.add(%container.input);

  %window.add(%container);

  %window.container = %container;
  %window.group = %obj;

  return %window;
}

function GlassGroupchatWindow::sendInput(%this) {
  %text = %this.container.input.getValue();
  if(strlen(trim(%text)) == 0)
    return;

  %this.container.input.setValue("");

  %this.group.pushText(%text);
}

function GlassLiveGroup::pushText(%this, %msg) {
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

  %chatroom = %this.window.container;
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
          text = %msg;
          image = %this.icon;

          sticky = false;
          callback = "";
        };
      }
    }
  }
}
