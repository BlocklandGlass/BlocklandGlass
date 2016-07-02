function GlassLive::init() {
  new ScriptObject(GlassLive) {
    color_friend = "66dd88";
    color_default = "666666";
    color_self = "6688ff";
    color_admin = "ffaa00";
  };
  GlassOverlayGui.add(GlassFriendsGui.getObject(0));
}

function GlassLive::chatColorCheck(%this) {
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_friend @  ">Friend: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_self @  ">Self: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_admin @  ">Admin: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_default @  ">Pleb: <font:verdana:12><color:333333>rambling message", 0);
}



//================================================================
//= Communication                                                =
//================================================================

function GlassLive::leaveRoom(%id, %conf) {
  if(!%conf) {
    messageBoxYesNo("Are you sure?", "Are you sure you want to leave this room?", "GlassLive::leaveRoom(" @ %id @ ", 1);");
  } else {
    GlassLive.chatroom[%id].deleteAll();
    GlassLive.chatroom[%id].delete();

    %obj = JettisonObject();
    %obj.set("type", "string", "roomLeave");
    %obj.set("id", "string", %id);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }
}

function GlassLive::joinRoom(%id) {
  if(isObject(GlassLive.chatroom[%id]))
    return;

  %obj = JettisonObject();
  %obj.set("type", "string", "roomJoin");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::openDirectMessage(%blid, %username) {
  if(!isObject(GlassLive.message[%blid])) {
    %gui = GlassLive::createDirectMessageGui(%blid, %username);
    GlassLive.message[%blid] = %gui;
    GlassOverlayGui.add(%gui);
  } else {
    %gui = GlassLive.message[%blid];
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

function GlassLive::onChatroomJoin(%id, %title) {
  if(isObject(GlassLive.chatroom[%id])) {
    GlassOverlayGui.add(GlassLive.chatroom[%id]);
    GlassLive.chatroom[%id].userlist.clear();
    return;
  }

  %gui = GlassLive::createChatroomGui(%id);
  %gui.title = %title;
  %gui.text = %title;
  %gui.id = %id;
  GlassLive.chatroom[%id] = %gui;

  %gui.position = "100 100";

  GlassOverlayGui.add(%gui);
}

function GlassLive::onMessage(%message, %username, %blid) {
  // TODO check friend, blocked, prefs, etc

  %gui = GlassLive::openDirectMessage(%blid, %username);

  GlassOverlayGui.pushToBack(%gui);

  %val = %gui.chattext.getValue();
  %msg = "<color:333333><font:verdana bold:12>" @ %username @ ":<font:verdana:12><color:333333> " @ %message;
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

function GlassLive::sendRoomMessage(%msg, %id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomChat");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::sendMessage(%blid, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "message");
  %obj.set("message", "string", %msg);
  %obj.set("target", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::sendFriendRequest(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendRequest");
  %obj.set("target", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::friendAccept(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendAccept");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::chatroomUserJoin(%id, %name, %blid) {
  %chatroom = GlassLive.chatroom[%id];
  %chatroom.userlist.addRow(%blid, %name);
  %chatroom.userlist.sort(0);
}

function GlassLive::chatroomUserLeave(%id, %blid, %reason) {
  %chatroom = GlassLive.chatroom[%id];
  %chatroom.userlist.removeRowById(%blid);

  switch(%reason) {
    case 0:
      %text = "Left";

    case 1:
      %text = "Disconnected";

    case 2:
      %text = "Connection Dropped";

    default:
      %text = "no reason";
  }

  %user = "BLID_" @ %blid; // todo local caching
  %text = "<font:verdana:12><color:666666>" @ %user @ " left the room. [" @ %text @ "]";
  GlassLive::pushMessage(%text, %id);
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

  echo(jettisonStringify("object", %obj));

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}


//================================================================
//= Gui Population                                               =
//================================================================

function GlassLive::chatroomInputSend(%id) {
  %chatroom = GlassLive.chatroom[%id];
  %val = trim(%chatroom.input.getValue());
  %val = stripMlControlChars(%val);
  if(%val $= "")
    return;

  GlassLive::sendRoomMessage(%val, %id);
  %chatroom.input.setValue("");
}

function GlassLive::messageType(%blid) {
  if(GlassLive.typing[%blid]) {
    cancel(GlassLive.typingSched);
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "messageTyping");
  %obj.set("target", "string", %blid);
  %obj.set("typing", "string", "1");

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassLive.typing[%blid] = 1;
  GlassLive.typingSched = schedule(5000, 0, eval, "GlassLive::messageTypeEnd(" @ %blid @ ");");
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

function GlassLive::pushMessage(%msg, %id) {
  %chatroom = GlassLive.chatroom[%id];
  %val = %chatroom.chattext.getValue();
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %chatroom.chattext.setValue(%val);
  %chatroom.chattext.forceReflow();
  %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
  %chatroom.scrollSwatch.setVisible(true);
  %chatroom.scroll.scrollToBottom();
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

  %this.getGroup().flare.setVisible(false);
  %this.getGroup().color = %this.getGroup().ocolor;
}

function GlassHighlightMouse::onMouseEnter(%this) {
  if(!%this.enabled)
    return;

  %this.getGroup().ocolor = %this.getGroup().color;
  %this.getGroup().color = %this.getGroup().hcolor;
}

function GlassHighlightMouse::onMouseDown(%this) {
  %this.down = 1;
}

function GlassHighlightMouse::onMouseUp(%this) {
  eval(%this.callback);
}

//================================================================
//= Gui Creation                                                 =
//================================================================

function GlassLive::createChatroomGui(%id) {
  %chatroom = new GuiWindowCtrl() {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 131";
    extent = "436 267";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Chatroom - General Discussion";
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
    closeCommand = "GlassLive::leaveRoom(" @ %id @ ");";
  };

  %chatroom.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 35";
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
  };

  %chatroom.scrollSwatch = new GuiSwatchCtrl(GlassChatroomGui_TextSwatch) {
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

  %chatroom.chattext = new GuiMLTextCtrl(GlassChatroomGui_Text) {
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
    maxBitmapHeight = "-1";
    selectable = "1";
    autoResize = "1";
  };

  %chatroom.userscroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "330 35";
    extent = "95 200";
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

  %chatroom.userlist = new GuiTextListCtrl() {
    profile = "GuiTextListProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "330 35";
    extent = "95 200";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    fitParentWidth = 1;
    resizeCell = 1;
  };

  %chatroom.input = new GuiTextEditCtrl(GlassChatroomGui_Input) {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 240";
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
  %chatroom.userscroll.add(%chatroom.userlist);
  %chatroom.add(%chatroom.input);

  return %chatroom;
}

function GlassLive::createDirectMessageGui(%blid, %username) {
  %dm = new GuiWindowCtrl() {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 131";
    extent = "270 180";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Message - " @ %username;
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
    closeCommand = "GlassLive::closeMessage(" @ %blid @ ");";
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
    extent = "240 115";
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
    extent = "240 115";
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

  %dm.input = new GuiTextEditCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
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
  %dm.add(%dm.scroll);
  %dm.scroll.add(%dm.scrollSwatch);
  %dm.scrollSwatch.add(%dm.chattext);
  %dm.add(%dm.input);

  return %dm;
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
    position = "24 10";
  };

  %gui.add(%gui.text);

  %gui.text.forceCenter();

  return %gui;
}


function GlassLive::createFriendSwatch(%name, %blid, %status) {
  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "180 26";
    position = "10 5";
    color = "210 220 255 255";
    hcolor = "220 230 255 255";
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
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/user.png";
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
    callback = "GlassLive::friendTabExtend(" @ %blid @ ");";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";
  };

  %gui.add(%gui.text);
  %gui.add(%gui.icon);
  %gui.add(%gui.mouse);

  %gui.add(%gui.buttonChat);

  %gui.text.centerY();
  %gui.icon.centerY();

  return %gui;
}

function GlassLive::createFriendList(%friends) {
  GlassLive.friends = %friends;
  GlassFriendGui_ScrollSwatch.deleteAll();
  %h = GlassLive::createFriendHeader("Friends");
  GlassFriendGui_ScrollSwatch.add(%h);

  %last = %h;

  for(%i = 0; %i < %friends.length; %i++) {
    %friend = %friends.value[%i];
    %gui = GlassLive::createFriendSwatch(%friend.username, %friend.blid);
    %gui.placeBelow(%last, 5);

    GlassLive.isFriend[%friend.blid] = 1;
    GlassLive.friendTab[%friend.blid] = %gui;
    GlassFriendGui_ScrollSwatch.add(%gui);

    %last = %gui;
  }
}

function GlassLive::friendTabExtend(%blid) {
  %tab = GlassLive.friendTab[%blid];
  %tab.flare.extent = "0 0";
  %tab.verticalMatchChildren(26, 5);
  GlassFriendGui_ScrollSwatch.pushToBack(%tab);

  %tab.mouse.callback = "GlassLive::friendTabShrink(" @ %blid @");";
  %tab.color = "100 100 220 255";

  %this.expanded = true;
}

function GlassLive::friendTabShrink(%blid) {
  %tab = GlassLive.friendTab[%blid];

  %tab.extent = "180 26";
  %tab.mouse.callback = "GlassLive::friendTabExtend(" @ %blid @");";
  %tab.color = "210 220 255 255";

  %this.expanded = false;
}

package GlassLivePackage {
  function GlassOverlayGui::onWake(%this) {
    parent::onWake(%this);
    GlassOverlayGui.add(GlassFriendsWindow);
    //GlassLive::createFriendList();
    GlassLive.chatroom0.chattext.forceReflow();
  }

  function disconnectedCleanup() {
    GlassLive::updateLocation(false);

    parent::disconnectedCleanup();
  }

  function GameConnection::onConnectionAccepted(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k) {
    parent::onConnectionAccepted(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k);

    GlassLive::updateLocation(true);
  }
};
activatePackage(GlassLivePackage);
