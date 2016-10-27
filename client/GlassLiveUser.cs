
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
  if(isObject(%this.messageGui)) {
    return %this.messageGui;
  } else {
    return false;
  }
}

function GlassLiveUser::canSendMessage(%this) {
  //can we show a message from them?
  //privacy settings
  //blocked users

  %friendsOnly = GlassLive::getSetting("GL::FriendOnly");
  %blocked = %this.blocked;

  if(%this.isAdmin() || %this.isMod())
    return true;

  if(%blocked)
    return false;

  if(%friendsOnly && !%this.isFriend())
    return false;

  return true;
}

function GlassLiveUser::setStatus(%this, %status) {
  if(%status $= "online" || %status $= "away" || %status $= "busy") {
    %this.status = %status;
  }
}

function GlassLiveUser::getStatus(%this) {
  if(%this.status $= "") {
    return (%this.online ? "online" : "offline");
  }

  return %this.status;
}

function GlassLiveUser::disconnected(%this) {
  GlassLiveUsers.user[%this.blid] = "";
  GlassLiveUsers.remove(%this);
  %this.delete();
}