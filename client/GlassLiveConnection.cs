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
  //echo(jettisonStringify("object", %obj));
  %this.send(jettisonStringify("object", %obj) @ "\r\n");

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14>" @ $Pref::Player::NetName @ "<br><font:verdana:12>" @ getNumKeyId());
}

function GlassLiveConnection::onDisconnect(%this) {
  GlassLive::setPowerButton(0);
  %this.connected = false;
  GlassLive.reconnect = GlassLive.schedule(1000+getRandom(0, 1000), "connectToServer");

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");

  %text = "<br><font:verdana:12><color:666666>[ Disconnected ]<br>";

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

function GlassLiveConnection::doDisconnect(%this) {
  %this.disconnect();
  %this.connected = false;

  GlassLive::setPowerButton(0);
  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14><color:cc0000>Disconnected");
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
      echo("Auth status: " @ %data.status);

    case "notification":
      %title = %data.title;
      %text = %data.text;
      %image = %data.image;
      %sticky = (%data.duration == 0);
      GlassNotificationManager::newNotification(%title, %text, %image, %sticky, %callback);

    case "message":
      GlassLive::onMessage(%data.message, %data.sender, %data.sender_id);
      GlassNotificationManager::newNotification(%data.sender, %data.message, "comment", 0);

    case "messageNotification":
      GlassLive::onMessageNotification(%data.message, %data.chat_blid);

    case "roomJoin":
      GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);
      %room = GlassLiveRoom::create(%data.id, %data.title);

      %clients = %data.clients;
      for(%i = 0; %i < %clients.length; %i++) {
        %cl = %clients.value[%i];

        %user = GlassLiveUser::create(%cl.username, %cl.blid);

        //echo("user (" @ %user.blid @ "): " @ %user);

        %user.setAdmin(%cl.admin);
        %user.setMod(%cl.mod);

        %room.addUser(%user.blid);
      }

      %room.createWindow();

      %motd = %data.motd;
      %motd = strreplace(%motd, "\n", "<br> * ");
      %motd = "<font:verdana bold:12><color:666666> * " @ %motd;

      %room.pushText(%motd);


    case "messageTyping":
      GlassLive::setMessageTyping(%data.sender, %data.typing);

    case "roomMessage":
      %room = GlassLiveRoom::getFromId(%data.room);

      %now = getRealTime();
      if(%now-GlassLive.lastMessageTime > 1000 * 60 * 5) {
        %text = "<font:verdana bold:12><just:center><color:999999>[" @ formatTimeHourMin(%data.datetime) @ "]<just:left>";
        %room.pushText(%text);
      }
      GlassLive.lastMessageTime = %now;

      %msg = %data.msg;
      %sender = %data.sender;
      %senderblid = %data.sender_id;

      if(%senderblid == getNumKeyId()) {
        %color = GlassLive.color_self;
      } else if(GlassLive.isAdmin[%senderblid]) {
        %color = GlassLive.color_admin;
      } else if(GlassLive.isModerator[%senderblid]) {
        %color = GlassLive.color_moderator;
      } else if(GlassLive.isFriend[%senderblid]) {
        %color = GlassLive.color_friend;
      } else {
        %color = GlassLive.color_default;
      }

      %text = "<font:verdana bold:12><color:" @ %color @ ">" @ %sender @ ": <font:verdana:12><color:333333>" @ stripMlControlChars(%msg);
      %room.pushText(%text);
      alxPlay(GlassChatAudio);

    case "roomUserJoin":
      %user = GlassLiveUser::create(%data.username, %data.blid);
      %user.setAdmin(%data.admin);
      %user.setMod(%data.mod);

      %room = GlassLiveRoom::getFromId(%data.id);
      %room.onUserJoin(%user.blid);

    case "roomUserLeave":
      %room = GlassLiveRoom::getFromId(%data.id);
      %room.onUserLeave(%data.blid, %data.reason);

    case "friendsList":
      for(%i = 0; %i < %data.friends.length; %i++) {
        %friend = %data.friends.value[%i];
        %user = GlassLiveUser::create(%friend.username, %friend.blid);
        %user.setFriend(true);
      }
      GlassLive::createFriendList(%data.friends);


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
      GlassLive.friendRequests.push("object", %obj);

      GlassLive::createFriendList();
      GlassNotificationManager::newNotification("Friend Request", "You've been sent a friend request by <font:verdana bold:13>" @ %user @ " (" @ %blid @ ")", "user_add", 0);

    case "friendStatus":
      for(%i = 0; %i < GlassLive.friends.length; %i++) {
        %friend = GlassLive.friends.value[%i];
        if(%friend.blid == %data.blid)
          %friend.set("online", "string", %data.online);
      }
      GlassLive::createFriendList(GlassLive.friends);

    case "friendAdd":
      %obj = JettisonObject();
      %obj.set("blid", "string", %data.blid);
      %obj.set("username", "string", %data.username);
      GlassLive.friends.push("object", %obj);

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

    case "location":
      GlassLive::displayLocation(%data);

    case "messageBox":
      messageBoxOk(%data.title, %data.text);
  }
}

function formatTimeHourMin(%datetime) {
  %t = getWord(%datetime, 0);
  %t = getSubStr(%t, 0, strpos(%t, ":", 4));
  return %t SPC getWord(%datetime, 1);
}
