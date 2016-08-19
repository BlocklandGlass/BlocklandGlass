$Glass::DisconnectManual = 1;
//$Glass::DisconnectKicked = 2;
//$Glass::DisconnectConnectionDropped = 3;
$Glass::DisconnectUpdate = 4;

function GlassLive::connectToServer() {
  cancel(GlassLive.reconnect);

  %server = Glass.address;
  %port = 27002;

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

  GlassLiveConnection.connect(%server @ ":" @ %port);
}

function GlassLiveConnection::onConnected(%this) {
  GlassLive::setPowerButton(1);

  %this.connected = true;
  %obj = JettisonObject();
  %obj.set("type", "string", "auth");
  %obj.set("ident", "string", GlassAuth.ident);
  %obj.set("version", "string", Glass.version);

  %this.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14>" @ $Pref::Player::NetName @ "<br><font:verdana:12>" @ getNumKeyId());
}

function GlassLiveConnection::onDisconnect(%this) {
  GlassLive::setPowerButton(0);
  %this.connected = false;
  GlassLive.reconnect = GlassLive.schedule(1000+getRandom(0, 1000), "connectToServer");

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  GlassLive::cleanUp();
}

function GlassLiveConnection::onDNSFailed(%this) {
  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  %this.connected = false;
  GlassLive.reconnect = GlassLive.schedule(1000+getRandom(0, 1000), "connectToServer");
}

function GlassLiveConnection::onConnectFailed(%this) {
  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  %this.connected = false;
  GlassLive.reconnect = GlassLive.schedule(1000+getRandom(0, 1000), "connectToServer");
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
    Glass::debug("error");
    return;
  }

  %data = $JSON::Value;

  switch$(%data.value["type"]) {
    case "auth":
      echo("Glass Live: " @ %data.status);

    case "notification":
      %title = %data.title;
      %text = %data.text;
      %image = %data.image;
      %sticky = (%data.duration == 0);

      GlassNotificationManager::newNotification(%title, %text, %image, %sticky, %callback);

    case "message":
      GlassLive::onMessage(%data.message, %data.sender, %data.sender_id);

      if(GlassSettings.get("Live::MessageNotification") )
        GlassNotificationManager::newNotification(%data.sender, %data.message, "comment", 0);

      if(GlassSettings.get("Live::MessageSound"))
        alxPlay(GlassNotificationAudio);

    case "messageNotification":
      GlassLive::onMessageNotification(%data.message, %data.chat_blid);

    case "roomJoin":
      GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);
      %room = GlassLiveRooms::create(%data.id, %data.title);

      %clients = %data.clients;
      for(%i = 0; %i < %clients.length; %i++) {
        %cl = %clients.value[%i];

        %user = GlassLiveUser::create(%cl.username, %cl.blid);


        %user.setAdmin(%cl.admin);
        %user.setMod(%cl.mod);

        %room.addUser(%user.blid);
      }

      %room.createView();

      %motd = %data.motd;
      %motd = strreplace(%motd, "\n", "<br> * ");
      %motd = "<font:verdana bold:12><color:666666> * " @ %motd;

      %room.pushText(%motd);


    case "messageTyping":
      GlassLive::setMessageTyping(%data.sender, %data.typing);

    case "roomMessage":
      %room = GlassLiveRoom::getFromId(%data.room);

      %msg = %data.msg;
      %sender = %data.sender;
      %senderblid = %data.sender_id;

      %senderUser = GlassLiveUser::getFromBlid(%senderblid);

      %room.pushMessage(%senderUser, %msg, %data);

    case "roomText":
      %room = GlassLiveRoom::getFromId(%data.id);
      %room.pushText(%data.text);

    case "roomUserJoin":
      %user = GlassLiveUser::create(%data.username, %data.blid);
      %user.setAdmin(%data.admin);
      %user.setMod(%data.mod);

      %room = GlassLiveRoom::getFromId(%data.id);
      %room.onUserJoin(%user.blid);

    case "roomUserLeave":
      %room = GlassLiveRoom::getFromId(%data.id);
      %room.onUserLeave(%data.blid, %data.reason);

    case "roomAwake":
      %room = GlassLiveRoom::getFromId(%data.id);
      %room.setUserAwake(%data.user, %data.awake);

    case "roomList":
      %window = GlassLive.pendingRoomList;
      %window.openRoomBrowser(%data.rooms);

    case "friendsList":
      for(%i = 0; %i < %data.friends.length; %i++) {
        %friend = %data.friends.value[%i];
        %user = GlassLiveUser::create(%friend.username, %friend.blid);
        %user.setFriend(true);
        %user.online = %friend.online;

        GlassLive::addFriendToList(%user);
      }
      GlassLive::createFriendList();


    case "friendRequests":
      for(%i = 0; %i < %data.requests.length; %i++) {
        %friend = %data.requests.value[%i];
        %user = GlassLiveUser::create(%friend.username, %friend.blid);
        %user.setFriendRequest(true);
      }
      GlassLive.friendRequests = %data.requests;
      GlassLive::createFriendList();

    case "friendRequest":
      %user = %data.sender;
      %blid = %data.sender_blid;

      %obj = JettisonObject();
      %obj.set("blid", "string", %blid);
      %obj.set("username", "string", %user);

      if(!isObject(GlassLive.friendRequests))
        GlassLive.friendRequests = JettisonArray();

      GlassLive.friendRequests.push("object", %obj);

      GlassLive::createFriendList();
      GlassNotificationManager::newNotification("Friend Request", "You've been sent a friend request by <font:verdana bold:13>" @ %user @ " (" @ %blid @ ")", "user_add", 0);

    case "friendStatus":
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      %uo.online = %data.online;
      GlassLive::createFriendList();

    case "friendAdd":
      %uo = GlassLiveUser::create(%data.username, %data.blid);
      %uo.online = %data.online;

      GlassLive::addFriendToList(%uo);

      %newRequests = JettisonArray();

      for(%i = 0; %i < GlassLive.friendRequests.length; %i++) {
        %o = GlassLive.friendRequests.value[%i];
        if(%o.blid != %data.blid) {
          %newRequests.push("object", %o);
        }
      }

      GlassLive.friendRequests.delete();
      GlassLive.friendRequests = %newRequests;

      GlassLive::createFriendList();

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
      messageBoxOk(%data.title, %data.text);

    case "shutdown":
      %planned = %data.planned;
      %reason = %data.reason;
      %timeout = %data.timeout;

      if(%timeout < 1000) {
        %timeout = 1000;
      }

      GlassNotificationManager::newNotification((%planned ? "Planned" : "Unplanned") SPC "Shutdown", %reason, (%planned ? "cog_go" : "cog_error"), 5000);

      %this.disconnect();
      %this.connected = false;
      GlassLive.reconnect = GlassLive.schedule(%timeout+getRandom(0, 2000), "connectToServer");

    case "disconnected":
    // 0 - server shutdown
    // 1 - other sign-in
    // 2 - barred
    if(%data.reason == 1) {
      messageBoxOk("Glass Live Disconnected", "You logged in from somewhere else!");
      %this.disconnect();
    } else if(%data.reason == 2) {
      messageBoxOk("Glass Live Disconnected", "You're banned!");
      %this.disconnect();
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
        GlassLive::disconnect($Glass::DisconnectUpdate);
      }
    }
    parent::messageCallback(%this, %call);
  }
};
activatePackage(GlassLiveConnectionPackage);
