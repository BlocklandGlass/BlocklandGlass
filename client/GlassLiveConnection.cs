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
  
  %this.connected = false;

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
      %sender = getASCIIString(%data.sender);
      
      GlassLive::onMessage(%data.message, %sender, %data.sender_id);

      if(GlassSettings.get("Live::MessageNotification"))
        GlassNotificationManager::newNotification(%sender, %data.message, "comment", 0);

      if(GlassSettings.get("Live::MessageSound"))
        alxPlay(GlassUserMsgReceivedAudio);

    case "messageNotification":
      GlassLive::onMessageNotification(%data.message, %data.chat_blid);

    case "roomJoin":
      if(GlassSettings.get("Live::RoomNotification")) {
        GlassNotificationManager::newNotification("Joined Room", "You've joined " @ %data.title, "add", 0);
      }
      
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
      if(isObject(%room))
        %room.pushText(%data.text);

    case "roomUserJoin":
      %user = GlassLiveUser::create(%data.username, %data.blid);
      %user.setAdmin(%data.admin);
      %user.setMod(%data.mod);

      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.onUserJoin(%user.blid);

    case "roomUserLeave":
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
        %room.onUserLeave(%data.blid, %data.reason);

    case "roomAwake":
      %room = GlassLiveRoom::getFromId(%data.id);
      if(isObject(%room))
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

        GlassLive::addfriendRequestToList(%user);
      }
      GlassLive::createFriendList();

    case "friendRequest":
      if(strStr(GlassLive.friendRequestList, %blid = %data.sender_blid) == -1) {
        %username = %data.sender;
        %user = GlassLiveUser::create(%username, %blid);

        GlassLive::addfriendRequestToList(%user);

        GlassLive::createFriendList();
        
        GlassNotificationManager::newNotification("Friend Request", "You've been sent a friend request by <font:verdana bold:13>" @ %user.username @ " (" @ %blid @ ")", "email_add", 0);
        
        alxPlay(GlassFriendRequestAudio);
      }
      
    case "friendStatus":
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      %uo.online = %data.online;
      
      GlassLive::createFriendList();
      
      if(GlassSettings.get("Live::ShowFriendStatus")) {
        %sound = %uo.online ? "GlassFriendOnlineAudio" : "GlassFriendOfflineAudio";
        alxPlay(%sound);
        GlassNotificationManager::newNotification(%uo.username, "is now " @ (%uo.online ? "online" : "offline") @ ".", (%uo.online ? "world_add" : "world_delete"), 0);
      }

    case "friendAdd": // create all-encompassing ::addFriend function for this?
      %uo = GlassLiveUser::create(%data.username, %data.blid);
      %uo.online = %data.online;
      %uo.isFriend = true;

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
      
    case "friendDecline": // nothing responding here - serverside issue?
      %uo = GlassLiveUser::getFromBlid(%data.blid);
      
      GlassNotificationManager::newNotification("Friend Declined", "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has declined your friend request.", "cross", 0);
      
      alxPlay(GlassFriendDeclinedAudio);
      
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

      if(%timeout < 1000) {
        %timeout = 1000;
      }

      GlassNotificationManager::newNotification("Glass Live" SPC (%planned ? "Planned" : "Unplanned") SPC "Shutdown", "Reason:" SPC %reason, "roadworks", 1);
      
      alxPlay(GlassUserMentionedAudio);
      
      %this.disconnect();
      %this.connected = false;
      GlassLive.reconnect = GlassLive.schedule(%timeout+getRandom(0, 2000), "connectToServer");

    case "disconnected":
      // 0 - server shutdown
      // 1 - other sign-in
      // 2 - barred
      if(%data.reason == 1) {
        glassMessageBoxOk("Disconnected", "You logged in from somewhere else!");
        %this.disconnect();
      } else if(%data.reason == 2) {
        glassMessageBoxOk("Disconnected", "You are barred from using the <font:verdana bold:13>Glass Live<font:verdana:13> service!<br><br>Sorry for the inconvenience.");
        GlassSettings.update("Live::StartupConnect", false);
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
