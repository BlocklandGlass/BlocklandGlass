$Glass::Disconnect["Left"] = 0; // [Left]
$Glass::Disconnect["Manual"] = 1; // [Disconnected]
//$Glass::Disconnect["Kicked"] = 2; // [Kicked]
$Glass::Disconnect["Quit"] = 3; // [Quit]
$Glass::Disconnect["Update"] = 4; // [Updates]

function GlassLive::connectToServer() {
  cancel(GlassLive.reconnect);

  if(!GlassLive.ready) {
    glassMessageBoxOk("Wait", "You haven't fully authed yet!");
    return;
  }

  if(GlassLive.lastConnected !$= "" && getSimTime() < GlassLive.lastConnected + 2500) {
    glassMessageBoxOk("Wait", "You're trying to connect too fast!"); // **make sure to implement this server-side as well so we can get rid of this afterwards**
    return;
  }

  %server = Glass.liveAddress;
  %port = Glass.livePort;

  //warn("Connecting to notification server...");

  if(isObject(GlassLiveConnection)) {
    if(GlassLiveConnection.connected) {
      error("GlassLiveConnection exists!");
      return;
    }
  } else {
    new TCPObject(GlassLiveConnection) {
      debug = true;
    };
  }

  %this.connected = false;

  GlassLiveConnection.connect(%server @ ":" @ %port);
}

function GlassLiveConnection::onConnected(%this) {
  GlassLive::setPowerButton(1);

  GlassLive.noReconnect = false;
  GlassLive.lastConnected = getSimTime();
  GlassLive.hideFriendRequests = false;
  GlassLive.hideFriends = false;

  %this.connected = true;
  %obj = JettisonObject();
  %obj.set("type", "string", "auth");
  %obj.set("ident", "string", GlassAuth.ident);
  %obj.set("blid", "string", getNumKeyId());
  %obj.set("version", "string", Glass.version);

  %this.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14>" @ $Pref::Player::NetName @ "<br><font:verdana:12>" @ getNumKeyId());

  GlassLive.schedule(500, checkPendingFriendRequests);
}

function GlassLiveConnection::onDisconnect(%this) {
  GlassLive::setPowerButton(0);
  %this.connected = false;

  if(!GlassLive.noReconnect) {
    GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
  }

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  GlassLive::cleanUp();
}

function GlassLiveConnection::onDNSFailed(%this) {
  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  %this.connected = false;

  if(!GlassLive.noReconnect) {
    GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
  }
}

function GlassLiveConnection::onConnectFailed(%this) {
  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  %this.connected = false;

  if(!GlassLive.noReconnect) {
    GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
  }
}

function GlassLiveConnection::doDisconnect(%this, %reason) {
  %obj = JettisonObject();
  %obj.set("type", "string", "disconnect");
  %obj.set("reason", "string", %reason);
  %this.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();

  %this.disconnect();
  %this.connected = false;

  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");
}

function GlassLiveConnection::placeCall(%this, %call) {
  %obj = JettisonObject();
  %obj.set("type", "string", %call);
  %this.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();
}

function GlassLiveConnection::onLine(%this, %line) {
  Glass::debug(%line);
  %error = jettisonParse(%line);
  if(%error) {
    Glass::debug("Parse error - " @ $JSON::Error);
    return;
  }

  %data = $JSON::Value;

  switch$(%data.value["type"]) {
    case "auth":
      echo("Glass Live: " @ %data.status);
      // TODO handle failure

    case "notification":
      %title = %data.title;
      %text = %data.text;
      %image = %data.image;
      %sticky = (%data.duration == 0);

      GlassNotificationManager::newNotification(%title, %text, %image, %sticky, %callback);

    case "message":
      %sender = getASCIIString(%data.sender);

      // TODO create GlassLiveUser ?

      GlassLive::onMessage(%data.message, %sender, %data.sender_id);

      if(GlassSettings.get("Live::MessageNotification"))
        GlassNotificationManager::newNotification(%sender, %data.message, "comment", 0);

      if(GlassSettings.get("Live::MessageSound"))
        alxPlay(GlassUserMsgReceivedAudio);

    case "messageNotification":
      // TODO create GlassLiveUser ? data.chat_username is sent now
      GlassLive::onMessageNotification(%data.message, %data.chat_blid);

    case "roomJoinAuto":
      // TODO just mimic roomJoin for now
      if(GlassSettings.get("Live::RoomNotification")) {
        GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);
      }

      %room = GlassLiveRooms::create(%data.id, %data.title);

      %clients = %data.clients;
      for(%i = 0; %i < %clients.length; %i++) {
        %cl = %clients.value[%i];

        %user = GlassLiveUser::create(%cl.username, %cl.blid);
        %user.status = %cl.status;
        %user.icon = %cl.icon;

        %user.setAdmin(%cl.admin);
        %user.setMod(%cl.mod);
        
        if(%cl.blid < 0) {
          %user.setBot(true);
        }

        %room.addUser(%user.blid);
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

    case "roomJoin":
      if(GlassSettings.get("Live::RoomNotification")) {
        GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);
      }

      %room = GlassLiveRooms::create(%data.id, %data.title);

      %clients = %data.clients;
      for(%i = 0; %i < %clients.length; %i++) {
        %cl = %clients.value[%i];

        %user = GlassLiveUser::create(%cl.username, %cl.blid);
        %user.status = %cl.status;
        %user.icon = %cl.icon;

        %user.setAdmin(%cl.admin);
        %user.setMod(%cl.mod);
        
        if(%cl.blid < 0) {
          %user.setBot(true);
        }

        %room.addUser(%user.blid);
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

    case "messageTyping":
      GlassLive::setMessageTyping(%data.sender, %data.typing);

    case "roomMessage":
      %room = GlassLiveRoom::getFromId(%data.room);
      if(isObject(%room)) {
        %msg = %data.msg;
        %sender = %data.sender;
        %senderblid = %data.sender_id;

        %senderUser = GlassLiveUser::getFromBlid(%senderblid);

        %room.pushMessage(%senderUser, %msg, %data);
      }

    case "roomText":
      %room = GlassLiveRoom::getFromId(%data.id);

      if(isObject(%room)) {
        %data.text = strreplace(%data.text, "[name]", $Pref::Player::NetName);
        %data.text = strreplace(%data.text, "[vers]", Glass.version);
        %data.text = strreplace(%data.text, "[date]", getWord(getDateTime(), 0));
        %data.text = strreplace(%data.text, "[time]", getWord(getDateTime(), 1));

        %room.pushText(%data.text);
      }

    case "roomUserJoin":
      %user = GlassLiveUser::create(%data.username, %data.blid);
      %user.setAdmin(%data.admin);
      %user.setMod(%data.mod);
      if(%user.blid < 0) {
        %user.setBot(true);
      }
      %user.status = %data.status;
      %user.icon = %data.icon;

      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.onUserJoin(%user.blid);

    case "roomUserLeave": //other user got removed
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.onUserLeave(%data.blid, %data.reason);

    case "roomUserStatus":
      %user = GlassLiveUser::getFromBlid(%data.blid);
      %user.status = %data.status;
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.renderUserList();

    case "roomUserIcon":
      %user = GlassLiveUser::getFromBlid(%data.blid);
      %user.icon = %data.icon;
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.renderUserList();

    case "roomKicked": //we got removed from a room
      warn("TODO: roomKicked for reason " @ %data.reason);
      glassMessageBoxOk("Kicked", "You've been kicked from -room name-:<br><br>" @ %data.reason);

    case "roomBanned": //we got banned from a room
      warn("TODO: roomBanned for reason " @ %data.reason);
      glassMessageBoxOk("Banned", "You've been banned from -room name- for " @ %data.duration @ " seconds:<br><br>" @ %data.reason);

    // case "roomAwake":
      // %room = GlassLiveRoom::getFromId(%data.id);
      // if(isObject(%room))
        // %room.setUserAwake(%data.user, %data.awake);

    case "roomList":
      %window = GlassLive.pendingRoomList;
      %window.openRoomBrowser(%data.rooms);

    case "friendsList":
      for(%i = 0; %i < %data.friends.length; %i++) {
        %friend = %data.friends.value[%i];
        %user = GlassLiveUser::create(%friend.username, %friend.blid);
        %user.setFriend(true);
        %user.status = %friend.status;
        %user.icon = %friend.icon;

        GlassLive::addFriendToList(%user);
      }
      GlassLive::createFriendList();


    case "friendRequests":
      for(%i = 0; %i < %data.requests.length; %i++) {
        %friend = %data.requests.value[%i];
        %user = GlassLiveUser::create(%friend.username, %friend.blid);
        %user.setFriendRequest(true);

        GlassLive::addfriendRequestToList(%user);
      }

      if(%data.requests.length == 0)
        GlassLive.friendRequestList = "";

      GlassLive::createFriendList();

    case "friendRequest":
      if(strstr(GlassLive.friendRequestList, %blid = %data.sender_blid) == -1) {
        %username = %data.sender;
        %user = GlassLiveUser::create(%username, %blid);

        GlassLive::addfriendRequestToList(%user);

        GlassLive::createFriendList();

        GlassNotificationManager::newNotification("Friend Request", "You've been sent a friend request by <font:verdana bold:13>" @ %user.username @ " (" @ %blid @ ")", "email_add", 0);

        alxPlay(GlassFriendRequestAudio);
      }

    case "friendStatus":
      GlassLive.schedule(100, friendOnline, %data.blid, %data.status);

    case "friendIcon":
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      %uo.icon = %data.icon;

      GlassLive::createFriendList();

    case "friendAdd": // create all-encompassing ::addFriend function for this?
      %uo = GlassLiveUser::create(%data.username, %data.blid);
      %uo.isFriend = true;
      %uo.status = %data.status;
      %uo.icon = %data.icon;

      GlassLive::removeFriendRequestFromList(%uo.blid);
      GlassLive::addFriendToList(%uo);

      GlassLive::createFriendList();

      if(isObject(%room = GlassChatroomWindow.activeTab.room))
        %room.renderUserList();

      GlassNotificationManager::newNotification("Friend Added", "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has been added to your friends list.", "user_add", 0);

      alxPlay(GlassFriendAddedAudio);

    case "friendRemove":
      %uo = GlassLiveUser::getFromBlid(%data.blid);

      GlassLive::removeFriend(%data.blid, true);

      GlassNotificationManager::newNotification("Friend Removed", "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has been removed from your friends list.", "user_delete", 0);

      alxPlay(GlassFriendRemovedAudio);

    case "groupJoin":
      %group = GlassLiveGroup::create(%data.id, %data.clients);
      %group.createGui();

    case "groupInvite":
      %id = %data.id;
      %name = %data.inviterName;
      %blid = %data.inviterBlid;
      %users = %data.users;

      GlassNotificationManager::newNotification("Groupchat Invite", "You've been invited to group chat by <font:verdana bold:13>" @ %name, "group", 1, "GlassLive::joinGroupPrompt(" @ %id @ ");");

    case "groupMessage":
      %group = GlassLiveGroup::getFromId(%data.id);

      %name = %data.senderName;
      %blid = %data.senderBlid;

      %user = GlassLiveUser::getFromBlid(%blid);
      %group.pushMessage(%user, %data.msg);

    case "groupClientEnter":
      %client = GlassLiveUser::create(%data.username, %data.blid);
      %group = GlassLiveGroup::getFromId(%data.id);

      %group.addUser(%client.blid);
      %group.pushText("<font:verdana:12><color:666666>" @ %client.username @ " entered the group.");

    case "groupClientLeave":
      %group = GlassLiveGroup::getFromId(%data.id);
      %group.removeUser(%data.blid);

    case "location":
      GlassLive::displayLocation(%data);

    case "serverListUpdate":
      return;
      GlassServerList.doLiveUpdate(%data.ip, %data.port, %data.key, %data.value);

    case "serverListing":
      return;
      GlassServerList.doLiveUpdate(getWord(%data.addr, 0), getWord(%data.addr, 1), "hasGlass", %data.hasGlass);

    case "messageBox":
      glassMessageBoxOk(%data.title, %data.text);

    case "shutdown":
      %planned = %data.planned;
      %reason = %data.reason;
      %timeout = %data.timeout;

      if(%timeout < 5000) {
        %timeout = 5000;
      }

      GlassNotificationManager::newNotification("Glass Live" SPC (%planned ? "Planned" : "Unplanned") SPC "Shutdown", "Reason:" SPC %reason, "roadworks", 1);

      alxPlay(GlassBellAudio);

      %this.disconnect();
      %this.connected = false;
      GlassLive.reconnect = GlassLive.schedule(%timeout+getRandom(0, 2000), connectToServer);

    case "disconnected":
      // 0 - server shutdown
      // 1 - other sign-in
      // 2 - barred
      if(%data.reason == 1) {
        GlassLive.noReconnect = true;
        glassMessageBoxOk("Disconnected", "You logged in from somewhere else!");
        %this.disconnect();
      } else if(%data.reason == 2) {
        GlassLive.noReconnect = true;
        glassMessageBoxOk("Disconnected", "You are barred from using <font:verdana bold:13>Glass Live<font:verdana:13>!<br><br>Sorry for the inconvenience.");
        GlassSettings.update("Live::StartupConnect", false);
        %this.disconnect();
      }

    case "kicked": //we got kicked from all service
      GlassLive.noReconnect = true;
      glassMessageBoxOk("Kicked", "You've been kicked from <font:verdana bold:13>Glass Live<font:verdana:13>:<br><br>\"" @ %data.reason @ "\"<br><br>Sorry for the inconvenience.");
      %this.disconnect();

    case "banned":
      GlassLive.noReconnect = true;
      glassMessageBoxOk("Banned", "You've been banned from <font:verdana bold:13>Glass Live<font:verdana:13> for " @ %data.duration @ " seconds:<br><br>\"" @ %data.reason @ "\"<br><br>Sorry for the inconvenience.");
      GlassSettings.update("Live::StartupConnect", false);
      %this.disconnect();

    case "error":
      if(%data.showDialog) {
        glassMessageBoxOk("Glass Live Error", %data.message);
      } else {
        echo("Glass Live Error: " @ %data.message);
      }
  }
  //%data.delete();
}

function formatTimeHourMin(%datetime) {
  %t = getWord(%datetime, 0);
  %t = getSubStr(%t, 0, strpos(%t, ":", 4));
  return %t SPC getWord(%datetime, 1);
}

package GlassLiveConnectionPackage {
  function messageCallback(%this, %call) {
    if(updater.restartRequired) {
      if(%call $= "quit();") {
        GlassLive::disconnect($Glass::Disconnect["Update"]);
      }
    }
    parent::messageCallback(%this, %call);
  }

  function authTCPobj_Client::onDisconnect(%this) {
    parent::onDisconnect(%this);

    if(GlassLiveConnection.connected) {
      GlassLive::disconnect($Glass::Disconnect["Manual"]);
    }

    GlassAuth.schedule(0, init);
    GlassAuth.schedule(100, heartbeat);
  }
};
activatePackage(GlassLiveConnectionPackage);
