function GlassLiveUser::create(%username, %blid) {
  if(isObject(GlassLiveUsers.user[%blid])) {
    GlassLiveUsers.user[%blid].username = getASCIIString(%username);
    return GlassLiveUsers.user[%blid];
  }

  //echo("[" @ GlassLiveUsers.user[%blid] @ "] exists: " @ isObject(GlassLiveUsers.user[%blid]));
  //echo("creating glu: [" @ %username @ "] [" @ %blid @ "]");

  %user = new ScriptObject() {
    class = "GlassLiveUser";
    username = getASCIIString(%username);
    blid = %blid;
    online = true;
  };

  GlassLiveUsers.add(%user);
  GlassLiveUsers.user[%blid] = %user;
  return %user;
}

function GlassLiveUser::getFromBlid(%blid) {
  if(isObject(GlassLiveUsers.user[%blid]))
    return GlassLiveUsers.user[%blid];
  else
    return false;
}

function GlassLiveUser::getFromName(%name) {
  for(%i = 0; %i < GlassLiveUsers.getCount(); %i++) {
    %user = GlassLiveUsers.getObject(%i);
    if(strpos(strlwr(%user.username), strlwr(%name)) != -1)
      return %user;
  }
  return false;
}

function GlassLiveUser::addRoom(%this, %id) {
  if(%this.inRoom[%id])
    return;

  %this.inRoom[%id] = 1;
  %this.rooms++;
}

function GlassLiveUser::removeRoom(%this, %id) {
  if(!%this.inRoom[%id])
    return;

  %this.inRoom[%id] = 0;
  %this.rooms--;
}

function GlassLiveUser::setAdmin(%this, %bool) {
  %this.isAdmin = %bool == 1;
}

function GlassLiveUser::isAdmin(%this) {
  return %this.isAdmin;
}

function GlassLiveUser::setMod(%this, %bool) {
  %this.isMod = %bool == 1;
}

function GlassLiveUser::isMod(%this) {
  return %this.isMod;
}

function GlassLiveUser::setFriend(%this, %bool) {
  %this.isFriend = %bool == 1;
}

function GlassLiveUser::isFriend(%this) {
  return %this.isFriend;
}

function GlassLiveUser::setBot(%this, %bool) {
  %this.isBot = %bool == 1;
}

function GlassLiveUser::isBot(%this) {
  return %this.isBot;
}

function GlassLiveUser::setFriendRequest(%this, %bool) {
  %this.isFriendRequest = %bool == 1;
}

function GlassLiveUser::isFriendRequest(%this) {
  return %this.isFriendRequest;
}

function GlassLiveUser::setMessageGui(%this, %obj) {
  %this.messageGui = %obj;
}

function GlassLiveUser::getMessageGui(%this) {
  if(isObject(%this.messageGui))
    return %this.messageGui;
  else
    return false;
}

function GlassLiveUser::setBlocked(%this, %bool) {
  %this.isBlocked = %bool == 1;
}

function GlassLiveUser::isBlocked(%this) {
  return %this.isBlocked;
}

function GlassLiveUser::block(%this) {
  GlassLive::userBlock(%this.blid);
}

function GlassLiveUser::unblock(%this) {
  GlassLive::userUnblock(%this.blid);
}

function GlassLiveUser::canSendMessage(%this) {
  if(%this.isAdmin() || %this.isMod() || %this.isBot())
    return true;

  //%me = GlassLiveUser::getFromBlid(getNumKeyId()); //admins must receive all
  //if(%me.isAdmin() || %me.isMod())
  //  return true;

  if(%this.isBlocked())
    return false;

  if(%this.isFriend())
    return true;

  //random user, default to setting
  if(GlassSettings.get("Live::MessageAnyone") && GlassLiveUser::getFromBlid(getNumKeyId()).status !$= "busy") {
    return true;
  } else {
    //infrom user that this user is private
    %obj = JettisonObject();
    %obj.set("type", "string", "messagePrivate");
    %obj.set("target", "string", %this.blid);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
    %obj.delete();
    return false;
  }
}

function GlassLiveUser::setIcon(%this, %icon, %roomid) {
  %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ ".png";
  %blockedIcon = "wall";

  if(isFile(%bitmap)) {
    if(%icon !$= %blockedIcon)
      %this.realIcon = %icon;

    if(%this.isBlocked()) {
      %this.icon = %blockedIcon;
    } else {
      %this.icon = %icon;
    }

    if(%roomid !$= "") {
      %room = GlassLiveRoom::getFromId(%roomid);
      if(isObject(%room))
        %room.renderUserList();
    } else {
      for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
        %window = GlassOverlayGui.getObject(%i);
        if(%window.getName() $= "GlassChatroomWindow") {
          %window.activeTab.room.renderUserList();
        }
      }
    }

    if(%this.isFriend())
      GlassLive::createFriendList();
  }
}

function GlassLiveUser::setStatus(%this, %status) {
  if(%status $= GlassLiveUser::getFromBlid(%this.blid).status)
    return;

  if(%status $= "online" || %status $= "away" || %status $= "busy" || %status $= "offline") {
    %this.status = %status;

    if(isObject(%this.window))
      GlassLive::openUserWindow(%this.blid);

    GlassLive::onMessageNotification(%this.username @ " is now " @ %this.status @ ".", %this.blid);
  }
}

function GlassLiveUser::getStatus(%this) {
  if(%this.status $= "")
    return "offline";
    
  return %this.status;
}

function GlassLiveUser::disconnected(%this) {
  GlassLiveUsers.user[%this.blid] = "";
  GlassLiveUsers.remove(%this);
  %this.delete();
}

function GlassLiveUser::getAvatar(%this, %gui) {
  %this.avatarGui = %gui;
  if(%this.status !$= "offline" && %this.status !$= "") {
    %obj = JettisonObject();
    %obj.set("type", "string", "getAvatar");
    %obj.set("blid", "string", %this.blid);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
    %obj.delete();
  } else {
    %this.gotAvatar();
  }
}

function GlassLiveUser::gotAvatar(%this, %jsonObj, %private) {
  if(%private) echo("Avatar is private");

  if(!isObject(%jsonObj) || %jsonObj.keyCount == 0 || %private) {
    %this.avatarFiller = true;
    %this.avatarGui.createBlockhead(false);
  } else {
    %this.avatarFiller = false;
    %this.avatar = %jsonObj;
    %this.avatarGui.createBlockhead(%this.avatar);
  }

  //%this.avatarGui.setSequence("", 0, "crouch", 1);
  //%this.avatarGui.setSequence("test", 1, "headside", 1);
  %this.avatarGui.setOrbitDist(6);
  %this.avatarGui.setCameraRot(0, 0, $pi * 1.1);
  %this.avatarGui.lightDirection = "0 0.2 0.3";
}

function GlassLiveUser::updateLocation(%this, %location, %serverTitle, %serverAddress, %serverPassworded) {
  %this.lastLocation = %this.location;
  %this.location = %location;
  %this.serverTitle = %serverTitle;
  %this.serverAddress = %serverAddress;
  %this.serverPassworded = %serverPassworded;

  if(isObject(%this.window))
    GlassLive::openUserWindow(%this.blid);
}

function GlassLiveUser::getLastLocation(%this) {
  if(%this.status $= "offline")
    return "";

  return %this.lastLocation;
}

function GlassLiveUser::getLocation(%this) {
  if(%this.status $= "offline")
    return "";

  return %this.location;
}

function GlassLiveUser::getServerTitle(%this) {
  if(%this.status $= "offline")
    return "";

  return %this.serverTitle;
}

function GlassLiveUser::getServerAddress(%this) {
  if(%this.status $= "offline")
    return "";

  return %this.serverAddress;
}

function GlassLiveUser::isServerPassworded(%this) {
  if(%this.status $= "offline")
    return false;

  if(%this.location $= "playing" || %this.location $= "hosting") {
    return %this.serverPassworded;
  } else {
    return false;
  }
}
