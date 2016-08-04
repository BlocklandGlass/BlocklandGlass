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
  //echo(jettisonStringify("object", %obj));
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

      %senderUser = GlassLiveUser::getFromBlid(%senderblid);

      if(%senderblid == getNumKeyId()) {
        %color = GlassLive.color_self;
      } else if(%senderUser.isAdmin()) {
        %color = GlassLive.color_admin;
      } else if(%senderUser.isMod()) {
        %color = GlassLive.color_mod;
      } else if(GlassLive.isFriend[%senderblid]) {
        %color = GlassLive.color_friend;
      } else {
        %color = GlassLive.color_default;
      }

      %msg = stripMlControlChars(%msg);
      for(%i = 0; %i < getWordCount(%msg); %i++) {
        %word = getWord(%msg, %i);
        if(%word $= ("@" @ $Pref::Player::NetName)) {
          %mentioned = true;
          %msg = setWord(%msg, %i, " <spush><font:verdana bold:12><color:" @ GlassLive.color_self @ ">" @ %word @ "<spop>");
        }
      }

      %text = "<font:verdana bold:12><color:" @ %color @ ">" @ %sender @ ":<font:verdana:12><color:333333> " @ %msg;
      %room.pushText(%text);
      if(GlassSettings.get("Live::RoomChatSound"))
        alxPlay(GlassChatAudio);

      if(%senderblid != getNumKeyId())
        if(%mentioned && GlassSettings.get("Live::RoomMentionNotification")) {
          GlassNotificationManager::newNotification(%room.name, "You were mentioned by " @ %sender, 0);
        } else if(GlassSettings.get("Live::RoomChatNotification"))
          GlassNotificationManager::newNotification(%room.name, %sender@": "@%msg, "comment", 0);

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

      if(!isObject(GlassLive.friendRequests))
        GlassLive.friendRequests = JettisonArray();

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
      %obj.set("online", "string", %data.online);
      if(!isObject(GlassLive.friends))
        GlassLive.friends = JettisonArray();

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

    case "serverListUpdate":
      GlassServerList.doLiveUpdate(%data.ip, %data.port, %data.key, %data.value);

    case "serverListing":
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
        GlassLiveConnection.doDisconnect($Glass::DisconnectUpdate);
      }
    }
    echo(callback);
    parent::messageCallback(%this, %call);
  }
};
activatePackage(GlassLiveConnectionPackage);
