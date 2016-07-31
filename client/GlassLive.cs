exec("./GlassLiveConnection.cs");
exec("./GlassLiveUser.cs");
exec("./GlassLiveRoom.cs");


function GlassLive::init() {
  if(!isObject(GlassLive))
    new ScriptObject(GlassLive) {
      color_friend = "66dd88";
      color_default = "666666";
      color_self = "6688ff";
      color_admin = "ffaa00";
      color_mod = "ee6600";
    };

  if(!isObject(GlassLiveUsers))
    new ScriptGroup(GlassLiveUsers);

  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassOverlayGui.gui");

  GlassOverlayGui.add(GlassFriendsGui.getObject(0));
}

function GlassLive_keybind() {
  GlassLive::openOverlay();
}

function GlassLive::openOverlay() {
  canvas.pushDialog(GlassOverlayGui);
  GlassNotificationManager.dismissAll();
}

function GlassLive::openModManager() {
  canvas.pushDialog(GlassModManagerGui);
}

function GlassLive::openSettings() {
  canvas.pushDialog(GlassModManagerGui);
  GlassModManagerGui::setPane(5);
}

function GlassLive::closeOverlay() {
  canvas.popDialog(GlassOverlayGui);
}

function GlassOverlayGui::onWake(%this) {
  %x = getWord(getRes(), 0);
	%y = getWord(getRes(), 1);
	GlassOverlay.resize(0, 0, %x, %y);

  for(%i = 0; %i < %this.getCount(); %i++) {
    %obj = %this.getObject(%i);
    if(%obj.getName() $= "GlassChatroomWindow") {
      %chatroom = %obj;
      %chatroom.chattext.forceReflow();

      %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
      %chatroom.scrollSwatch.setVisible(true);
      %chatroom.scroll.scrollToBottom();


    }
  }
  //instantly close all notifications
}

function GlassLive::chatColorCheck(%this) {
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_friend @  ">Friend: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_self @  ">Self: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_admin @  ">Admin: <font:verdana:12><color:333333>rambling message", 0);
  GlassLive::pushMessage("<font:verdana bold:12><color:" @ %this.color_default @  ">Pleb: <font:verdana:12><color:333333>rambling message", 0);
}

function GlassLive::disconnect() {
  if(isObject(GlassLiveConnection))
    GlassLiveConnection.doDisconnect();

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow" || %window.getName() $= "GlassMessageGui") {
      %window.deleteAll();
      %window.delete();
    }
  }
}

//================================================================
//= Communication                                                =
//================================================================

function GlassLive::joinRoom(%id) {
  %room = GlassLiveRoom::getFromId(%id);

  if(isObject(%oom.window)) {
    GlassOverlayGui.add(%room.window);
    GlassOverlayGui.pushToBack(%room.window);
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "roomJoin");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
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
  }

  GlassLive::setMessageTyping(%blid, false);

  %val = %gui.chattext.getValue();
  %msg = "<color:333333><font:verdana bold:12>" @ %username @ ":<font:verdana:12><color:333333> " @ %message;
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  %gui.chattext.setValue(%val);
  %gui.chattext.forceReflow();
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
  echo("typing: " @ %typing);
  %user = GlassLiveUser::getFromBlid(%blid);
  if(isObject(%user)) {
    echo("usr");
    %window = %user.getMessageGui();
    if(isObject(%window)) {
      echo("wind");
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

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::friendDecline(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendDecline");
  %obj.set("blid", "string", %blid);

  %newRequests = JettisonArray();

  for(%i = 0; %i < GlassLive.friendRequests.length; %i++) {
    %o = GlassLive.friendRequests.value[%i];
    if(%o.blid != %blid) {
      %newRequests.push("object", %o);
    }
  }

  GlassLive.friendRequests.delete();
  GlassLive.friendRequests = %newRequests;

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
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
  %btn.text = "";
  %btn.on = %bool;
  if(%btn.on) {
    %btn.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn_poweroff");
  } else {
    %btn.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn_poweron");
  }
}

function GlassLive::openAddDlg() {
  GlassFriendsGui_AddFriendBLID.getGroup().setVisible(true);
  GlassFriendsGui_ScrollOverlay.setVisible(true);
}

function GlassLive::addDlgSubmit() {
  if(GlassFriendsGui_AddFriendBLID.getValue()+0 !$= GlassFriendsGui_AddFriendBLID.getValue()) {
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

  %chatroom = %room.window;
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
  } else {
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
  } else {
    %this.getGroup().chaticon.setVisible(true);
  }
}

function GlassHighlightMouse::onMouseDown(%this) {
  %this.down = 1;
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
  } else if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
    GlassLive::openDirectMessage(%this.blid);
  } else {
    eval(%this.callback);
  }
}

//================================================================
//= Gui Creation                                                 =
//================================================================

function GlassLive::createChatroomGui(%id) {
  %chatroom = new GuiWindowCtrl(GlassChatroomWindow) {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 131";
    extent = "465 267";
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
    closeCommand = "GlassLiveRoom::leave(" @ %id @ ");";
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
    maxBitmapHeight = "-1";
    selectable = "1";
    autoResize = "1";
  };

  %chatroom.userscroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "330 35";
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

  %chatroom.userswatch = new GuiSwatchCtrl(GlassChatroomGui_TextSwatch) {
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
  %chatroom.userscroll.add(%chatroom.userswatch);
  %chatroom.add(%chatroom.input);

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
    text = "Message - " @ %username;
    maxLength = "255";
    resizeWidth = "1";
    resizeHeight = "1";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    closeCommand = "GlassLive::closeMessage(" @ %blid @ ");";
  };

  %dm.resize = new GuiMLTextCtrl(GlassMessageResize) {
    profile = "GuiMLTextProfile";
    horizSizing = "relative";
    vertSizing = "relative";
    position = "0 0";
    extent = %dm.extent;
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
    maxBitmapHeight = "-1";
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
  if(%friends $= "")
    %friends = GlassLive.friends;

  GlassLive.friends = %friends;
  GlassFriendGui_ScrollSwatch.deleteAll();
  %h = GlassLive::createFriendHeader("Friends");
  GlassFriendGui_ScrollSwatch.add(%h);

  %last = %h;

  for(%i = 0; %i < %friends.length; %i++) {
    %friend = %friends.value[%i];
    %gui = GlassLive::createFriendSwatch(%friend.username, %friend.blid, %friend.online);
    %gui.placeBelow(%last, 5);

    GlassLive.isFriend[%friend.blid] = 1;
    GlassLive.friendTab[%friend.blid] = %gui;
    GlassFriendGui_ScrollSwatch.add(%gui);

    %last = %gui;
  }

  %requests = GlassLive.friendRequests;
  if(isObject(%requests) && %requests.length > 0) {
    %h = GlassLive::createFriendHeader("Friend Requests");
    %h.placeBelow(%last, 10);
    GlassFriendGui_ScrollSwatch.add(%h);

    %last = %h;

    for(%i = 0; %i < %requests.length; %i++) {
      %friend = %requests.value[%i];
      %gui = GlassLive::createFriendRequest(%friend.username, %friend.blid);
      %gui.placeBelow(%last, 5);
      GlassFriendGui_ScrollSwatch.add(%gui);

      %last = %gui;
    }
  }
}

function GlassChatroomWindow::onWake(%chatroom) {
  %chatroom.chattext.forceReflow();
  %chatroom.scrollSwatch.verticalMatchChildren(0, 2);
  %chatroom.scrollSwatch.setVisible(true);
  %chatroom.scroll.scrollToBottom();

  %obj = JettisonObject();
  %obj.set("type", "string", "roomAwake");
  %obj.set("id", "string", %chatroom.id);
  %obj.set("bool", "string", "1");

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassChatroomWindow::onSleep(%chatroom) {
  echo("sleep");
  %obj = JettisonObject();
  %obj.set("type", "string", "roomAwake");
  %obj.set("id", "string", %chatroom.id);
  %obj.set("bool", "string", "0");

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

package GlassLivePackage {
  function GlassOverlayGui::onWake(%this) {
    parent::onWake(%this);
    GlassOverlayGui.add(GlassFriendsWindow);

    GlassFriendsWindow.position = (getWord(getRes(), 0) - getWord(GlassFriendsWindow.extent, 0) - 50) SPC 50;
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
