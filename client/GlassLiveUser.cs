
function GlassLiveUser::create(%username, %blid) {
  if(isObject(GlassLiveUsers.user[%blid]))
    return GlassLiveUsers.user[%blid];

  %user = new ScriptObject() {
    class = "GlassLiveUser";
    username = %username;
    blid = %blid;
  };
  GlassLiveUsers.add(%user);
  GlassLiveUsers.user[%blid] = %user;
  return %user;
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

function GlassLiveUser::disconnected(%this) {
  GlassLiveUsers.user[%this.blid] = "";
  GlassLiveUsers.remove(%this);
  %this.delete();
}
