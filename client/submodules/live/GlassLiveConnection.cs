function GlassLive::connectToServer() {
	if(isObject(GlassLiveConnection) && GlassLiveConnection.connected)
		return;

	cancel(GlassLive.reconnect);

	GlassLive::setConnectionStatus("Disconnected", 1);

	if(!GlassAuth.isAuthed) {
		return;
	}

	if(GlassLive.connectionTries > 4) {
		%minutes = 2;
		GlassLive.reconnect = GlassLive.schedule((%minutes * 60 * 1000) | 0, connectToServer);
		GlassLive.connectionTries = 0;
		return;
	}

	%server = Glass.liveAddress;
	%port = Glass.livePort;

	if(isObject(GlassLiveConnection)) {
		if(GlassLiveConnection.connected) {
			GlassLog::debug("Tried to connect to Glass Live, already connected!");
			return;
		}
	} else {
		new TCPObject(GlassLiveConnection) {
			debug = true;

			timeoutThreshold = 5; //5 seconds
			timeoutFrequency = 60; //once a minute
		};
	}

	%this.connected = false;
	%this.waitingForAuth = false;

	GlassLive::setPowerButton(0);

	GlassLive::setConnectionStatus("Connecting...", 2);
	GlassLiveConnection.connect(%server @ ":" @ %port);
}

function GlassLiveConnection::onConnected(%this) {
	if(!GlassAuth.isAuthed) {
		%this.disconnect();
		return;
	}

	GlassLive::setPowerButton(1);

	GlassLive.noReconnect = false;
	GlassLive.connectionTries = 0;
	GlassLive.hideFriendRequests = GlassSettings.get("Live::HideRequests");
	GlassLive.hideFriends = GlassSettings.get("Live::HideFriends");
	GlassLive.hideBlocked = GlassSettings.get("Live::HideBlocked");

	%this.connected = true;
	GlassLive.waitingForAuth = true;
	// "authenticating" status will be reset in GlassLive::onAuthSuccess

	%obj = JettisonObject();
	%obj.set("type", "string", "auth");

	if(GlassSettings.get("Live::AutoJoinRooms")) {
		%obj.set("autoJoinRooms", "string", true);
		%autoJoin = true;
	} else {
		%autoJoin = false;
	}

	if(GlassAuth.usingDAA) {
		%data = JettisonObject();

		%data.set("blid", "string", getNumKeyId());

		%data.set("viewAvatar", "string", GlassSettings.get("Live::ViewAvatar"));
		%data.set("viewLocation", "string", GlassSettings.get("Live::ViewLocation"));
		%data.set("autoJoinRooms", "string", %autoJoin);

		%digest = GlassAuth.daa.digest(%data);

		%obj.set("authType", "string", "daa");
		%obj.set("version", "string", Glass.version);

	  %obj.set("ident", "string", GlassAuth.ident);
		%obj.set("digest", "object", %digest);
	} else {

		%obj.set("authType", "string", "default");

		%obj.set("ident", "string", GlassAuth.ident);
		%obj.set("blid", "string", getNumKeyId());
		%obj.set("version", "string", Glass.version);

		%obj.set("viewAvatar", "string", GlassSettings.get("Live::ViewAvatar"));
		%obj.set("viewLocation", "string", GlassSettings.get("Live::ViewLocation"));
		%obj.set("autoJoinRooms", "string", %autoJoin);
	}

	%this.send(jettisonStringify("object", %obj) @ "\r\n");
	%obj.delete();

	GlassLive::setConnectionStatus("Authenticating...", 0);

	GlassLive.afkCheck(true);
}

function GlassLiveConnection::onDisconnect(%this) {
  GlassSettings.update("Live::hideRequests", GlassLive.hideFriendRequests);
  GlassSettings.update("Live::hideFriends", GlassLive.hideFriends);
  GlassSettings.update("Live::hideBlocked", GlassLive.hideBlocked);

	GlassLive::setPowerButton(0);
	GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
	GlassLive_StatusSwatch.setVisible(false);

	%this.connected = false;

	// Don't try to reconnect if we never got past the initial auth.
	if(!GlassLive.waitingForAuth && !GlassLive.noReconnect) {
		GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
	}

	cancel(%this.keepaliveTimeout);
	cancel(%this.keepaliveSchedule);

	GlassLive::setConnectionStatus("Disconnected", 1);
	GlassLive::cleanUp();

  new ScriptObject(GlassNotification) {
    title = "Signed Out";
    text = "You have been signed out of Glass Live.";
    image = "networking_red";

    sticky = false;
  };
	GlassLive.waitingForAuth = false;
}

function GlassLiveConnection::onDNSFailed(%this) {
	GlassLive::setPowerButton(0);

	GlassLive::setConnectionStatus("DNS Failed", 1);

	%this.connected = false;
	GlassLive.connectionTries++;

	if(!GlassLive.noReconnect)
		GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
}

function GlassLiveConnection::onConnectFailed(%this) {
	GlassLive::setPowerButton(0);

	GlassLive::setConnectionStatus("Connect Failed", 1);

	%this.connected = false;
	GlassLive.waitingForAuth = false;
	GlassLive.connectionTries++;

	if(!GlassLive.noReconnect)
		GlassLive.reconnect = GlassLive.schedule(5000+getRandom(0, 1000), connectToServer);
}


function GlassLiveConnection::doDisconnect(%this) {
	%obj = JettisonObject();
	%obj.set("type", "string", "disconnect");
	%this.send(jettisonStringify("object", %obj) @ "\r\n");
	%obj.delete();

	%this.disconnect();
	%this.onDisconnect();
	%this.connected = false;
	GlassLive.waitingForAuth = false;

	GlassLive::setPowerButton(0);
	GlassLive::setConnectionStatus("Disconnected", 1);
}

function GlassLive::setConnectionStatus(%text, %color) {
	switch(%color) {
		case 0:
			%color = "2ecc71";

		case 1:
			%color = "e74c3c";

		case 2:
			%color = "e67e22";

		default:
			%color = "666666";
	}

	GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:" @ %color @ ">" @ %text);
	if(GlassFriendsGui_HeaderText.isAwake()) {
		GlassFriendsGui_HeaderText.forceReflow();
		GlassFriendsGui_HeaderText.forceCenter();
	}

	GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
	GlassLive_StatusSwatch.setVisible(false);
}

function GlassLiveConnection::sendKeepalivePing(%this) {
	cancel(%this.keepaliveSchedule);

  %obj = JettisonObject();
  %obj.set("type", "string", "ping");
  %obj.set("key", "string", "keepalive");
  %this.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();

  %this.keepaliveTimeout = %this.schedule(%this.timeoutThreshold * 1000, keepaliveFailed);
}

function GlassLiveConnection::gotKeepalivePong(%this) {
	if(isEventPending(%this.keepaliveTimeout)) {
		cancel(%this.keepaliveTimeout);
	}

	%this.keepaliveSchedule = %this.schedule(%this.timeoutFrequency * 1000, sendKeepalivePing);
}

function GlassLiveConnection::keepaliveFailed(%this) {
	%this.doDisconnect();
	GlassLog::error("Glass Live ping timed out!");
	GlassLive::connectToServer();
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
	GlassLog::debug("Glass Live got \c1" @ %data.value["type"]);

	switch$(%data.value["type"]) {
		case "auth":
			switch$(%data.status) {
				case "failed":
					%this.doDisconnect();
					GlassLog::log("Glass Live Authentication: FAILED \c3" @ getSubStr(getWord(getDateTime(), 1), 0, 5));
					if(%data.action $= "reident") {
						GlassAuth.reident();
					}

				case "success":
					GlassLog::log("Glass Live Authentication: SUCCESS \c3" @ getSubStr(getWord(getDateTime(), 1), 0, 5));
					GlassLive.onAuthSuccess();
					GlassLiveConnection.sendKeepalivePing();

				default:
					GlassLog::error("\c2Glass Live received an unknown auth response: " @ %data.status);
					if(%data.action $= "reident") {
						GlassAuth.reident();
					}
			}
			// TODO handle failure

		case "notification":
			%title = %data.title;
			%text = %data.text;
			%image = %data.image;
			%sticky = (%data.duration == 0);

			new ScriptObject(GlassNotification) {
				title = %title;
				text = %text;
				image = %image;

				sticky = %sticky;
				callback = %callback;
			};

		case "message":
			%user = GlassLiveUser::getFromBlid(%data.sender_id);

			if(!%user.canSendMessage()) {
				%data.schedule(0, delete);
				return;
			}

			%sender = getASCIIString(%data.sender);

			GlassLive::onMessage(%data.message, %sender, %data.sender_id);

			if(GlassSettings.get("Live::MessageNotification") && !GlassOverlayGui.isAwake()) {

				if(GlassSettings.get("Live::ReminderIcon"))
					GlassMessageReminder.setVisible(true);

					if(strlen(%data.message) > 100)
						%data.message = getsubstr(%data.message, 0, 100) @ "...";

					if(isObject(%user.dmNotification) && %user.dmNotification.action !$= "dismiss") {
						%user.dmCount++;
						%no = %user.dmNotification;
						%no.text = "<font:verdana bold:13>" @ %user.dmCount @ "<font:verdana:13> new messages.";
						%no.updateText();
					} else {
						%user.dmCount = 1;
						%user.dmNotification = new ScriptObject(GlassNotification) {
							title = %sender;
							text = "\"" @ GlassLive::getEmotedMessage(%data.message) @ "\"";
							image = %user.icon;

							sticky = false;
							callback = "GlassOverlay::open();";
						};
					}
				}

		case "messageNotification":
			// TODO create GlassLiveUser ? data.chat_username is sent now
			GlassLive::onMessageNotification(%data.message, %data.chat_blid);

		case "roomJoinAuto":
			//The server takes in our auto join room setting and handles it now
			// this is mostly just backwards compat
			GlassLive::onJoinRoom(%data);

		case "roomJoin":
			GlassLive::onJoinRoom(%data);

		case "messageTyping":
			%user = GlassLiveUser::getFromBlid(%data.sender);

			if(!%user.canSendMessage()) {
				%data.schedule(0, delete);
				return;
			}

			GlassLive::setMessageTyping(%data.sender, %data.typing);

		case "roomMessage":
			%room = GlassLiveRooms::getFromId(%data.room);
			if(isObject(%room)) {
				%msg = getASCIIString(%data.msg);
				%sender = %data.sender;
				%senderblid = %data.sender_id;

				%senderUser = GlassLiveUser::getFromBlid(%senderblid);

				if(!GlassSettings.get("Live::RoomShowBlocked") && %senderUser.isBlocked()) {
					%data.schedule(0, delete);
					return;
				}

				%room.pushMessage(%senderUser, %msg, %data);
			}

		case "roomText":
			%room = GlassLiveRooms::getFromId(%data.id);

			if(isObject(%room)) {
				%data.text = strreplace(%data.text, "[name]", $Pref::Player::NetName);
				%data.text = strreplace(%data.text, "[vers]", Glass.version);
				%data.text = strreplace(%data.text, "[date]", getWord(getDateTime(), 0));
				%data.text = strreplace(%data.text, "[time]", getWord(getDateTime(), 1));

				// getASCIIString(); can't go over 1021 characters at one time

				if(strlen(%data.text) <= 1021) // to-do: split string if over 1021 and apply getASCIIString separately
					%room.pushText("<spush><color:666666>" @ getASCIIString(%data.text) @ "<spop>");
				else
					%room.pushText("<spush><color:666666>" @ %data.text @ "<spop>");
			}

		case "roomUserJoin":
			%uo = GlassLiveUser::create(%data.username, %data.blid);
			%uo.setAdmin(%data.admin);
			%uo.setMod(%data.mod);
			if(%uo.blid < 0) {
				%uo.setBot(true);
			}
			%uo.setStatus(%data.status);
			%uo.setIcon(%data.icon);

			%room = GlassLiveRooms::getFromId(%data.id);
			if(isObject(%room))
				%room.onUserJoin(%uo.blid);

			if(isObject(GlassRoomBrowser))
				GlassChatroomWindow.openRoomBrowser();

		case "roomUserLeave": //other user got removed
			%room = GlassLiveRooms::getFromId(%data.id);
			if(isObject(%room))
				%room.onUserLeave(%data.blid);

			if(isObject(GlassRoomBrowser))
				GlassChatroomWindow.openRoomBrowser();

		case "roomUserStatus":
			%uo = GlassLiveUser::getFromBlid(%data.blid);
			%uo.setStatus(%data.status);
			%room = GlassLiveRooms::getFromId(%data.id);

		case "roomUserIcon":
			%uo = GlassLiveUser::getFromBlid(%data.blid);
			%uo.setIcon(%data.icon, %data.id);

		case "roomKicked": //we got removed from a room
			glassMessageBoxOk("Kicked", "You've been kicked:<br><br>" @ %data.reason);
			GlassLive.noReconnect = true;
			%this.doDisconnect();

		case "roomBanned": //we got banned from a room
			GlassLive.noReconnect = true;
			if(%data.all) {
				//we got banned from all rooms
				glassMessageBoxOk("Banned", "You've been banned from all chatrooms for <font:verdana bold:13>" @ secondsToTimeString(%data.duration) @ "<font:verdana:13>:<br><br><font:verdana bold:13>" @ %data.reason);
			} else {
				%room = GlassLiveRooms::getFromId(%data.id);
				glassMessageBoxOk("Banned", "You've been banned from <font:verdana bold:13>" @ %room.name @ "<font:verdana:13> for <font:verdana bold:13>" @ secondsToTimeString(%data.duration) @ "<font:verdana:13>:<br><br><font:verdana bold:13>" @ %data.reason);
			}


		// case "roomAwake":
			// %room = GlassLiveRooms::getFromId(%data.id);
			// if(isObject(%room))
				// %room.setUserAwake(%data.user, %data.awake);

		case "roomList":
			%window = GlassLive.pendingRoomList;
			%window.openRoomBrowser(%data.rooms);

		case "friendsList":
			for(%i = 0; %i < %data.friends.length; %i++) {
				%friend = %data.friends.value[%i];
				%uo = GlassLiveUser::create(%friend.username, %friend.blid);
				%uo.setFriend(true);
				%uo.setStatus(%friend.status);
				%uo.setIcon(%friend.icon);

				if(isObject(%friend.locationData)) {
					%uo.updateLocation(%friend.locationData.location, %friend.locationData.serverTitle, %friend.locationData.address, %friend.locationData.passworded);
				}

				GlassLive::addFriendToList(%uo);
			}
			GlassLive::createFriendList();

		case "friendRequests":
			for(%i = 0; %i < %data.requests.length; %i++) {
				%friend = %data.requests.value[%i];
				%uo = GlassLiveUser::create(%friend.username, %friend.blid);

				%uo.setFriendRequest(true);

				GlassLive::addfriendRequestToList(%uo);
			}

			if(%data.requests.length == 0)
				GlassLive.friendRequestList = "";

			GlassLive::createFriendList();

		case "friendRequest":
			if(strstr(GlassLive.friendRequestList, %blid = %data.sender_blid) == -1) {
				%username = %data.sender;
				%uo = GlassLiveUser::create(%username, %blid);

				if(%uo.isBlocked()) {
					%obj = JettisonObject();
					%obj.set("type", "string", "friendDecline");
					%obj.set("blid", "string", %blid);

					GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
					return;
				}

				GlassLive::addFriendRequestToList(%uo);
				GlassLive::createFriendList();

				new ScriptObject(GlassNotification) {
					title = "Friend Request";
					text = "You've been sent a friend request by <font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ")";
					image = "email_add";

					sticky = false;
					callback = "";
				};

				GlassAudio::play("friendRequest");
			}

		case "friendStatus":
			%user = GlassLiveUser::create(%data.username, %data.blid); //update username
			GlassLive::setFriendStatus(%data.blid, %data.status, false);

		case "friendIcon":
			%uo = GlassLiveUser::getFromBlid(%data.blid);
			%uo.setIcon(%data.icon);

		case "friendAdd": // create all-encompassing ::addFriend function for this?
			%uo = GlassLiveUser::create(%data.username, %data.blid);
			%uo.setFriend(true);
			%uo.setStatus(%data.status);
			%uo.setIcon(%data.icon);

			GlassLive::removeFriendRequestFromList(%uo.blid);
			GlassLive::addFriendToList(%uo);

			GlassLive::createFriendList();

			if(isObject(%room = GlassChatroomWindow.activeTab.room))
				%room.userListUpdate(%uo);

			new ScriptObject(GlassNotification) {
				title = "Friend Added";
				text = "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has been added to your friends list.";
				image = "user_add";

				sticky = false;
				callback = "";
			};

		case "friendRemove":
			%uo = GlassLiveUser::getFromBlid(%data.blid);

			GlassLive::removeFriend(%data.blid, true);

			new ScriptObject(GlassNotification) {
				title = "Friend Removed";
				text = "<font:verdana bold:13>" @ %uo.username @ " (" @ %uo.blid @ ") <font:verdana:13>has been removed from your friends list.";
				image = "user_delete";

				sticky = false;
				callback = "";
			};

			GlassAudio::play("friendRemoved");

		case "friendLocation":
			%uo = GlassLiveUser::create(%data.username, %data.blid);

			%uo.updateLocation(%data.location, %data.serverTitle, %data.address, %data.passworded);

			if(GlassSettings.get("Live::ShowFriendLocation") && !%uo.isBlocked()) {
				if(%uo.getLastLocation() !$= "") {
					switch$(%data.location) {
						case "menus":
							%message = "is now at the main menu";

						case "hosting":
							%message = "is now hosting <font:verdana bold:13>" @ getASCIIString(%data.serverTitle);

						case "hosting_lan":
							%message = "is now hosting a LAN server";

						case "singleplayer":
							%message = "is now playing in singleplayer";

						case "playing":
							%message = "is now playing in <font:verdana bold:13>" @ getASCIIString(%data.serverTitle);

						case "playing_lan":
							%message = "is now playing in a LAN server";
					}

					new ScriptObject(GlassNotification) {
						title = %uo.username;
						text = %message;
						image = %uo.icon;

						sticky = false;
						callback = "GlassLive::openUserWindow(" @ %uo.blid @ ");GlassOverlay::open();";
					};
				}
			}

		case "friendInvite":
			%sender = GlassLiveUser::getFromBlid(%data.sender);
			%address = %data.address;
			%title = %data.serverTitle;
			%isHost = (%data.location $= "hosting");

			%message = "<font:verdana bold:13>" @ %sender.username @ "<font:verdana:13> has invited you to ";

			if(%isHost) {
				%message = %message @ "their server ";
			} else {
				%message = %message @ "play ";
			}

			%message = %message @ "<font:verdana bold:13>" @ %title;

			new ScriptObject(GlassNotification) {
				title = "Server Invitation";
				text = %message;
				image = "world_go";

				sticky = true;
				callback = "GlassLive::inviteClick(\"" @ expandEscape(%address) @ "\", \"" @ expandEscape(%sender.blid) @ "\", \"" @ %data.passworded @ "\");";
			};

			GlassAudio::play("friendInvite");

			GlassLive::onMessageNotification("<color:eb9950>" @ %sender.username @ " has invited you to a server. <a:glass://invite=" @ %address @ ">Click here to join!</a>", %sender.blid, true);

		case "location":
			GlassLive::displayLocation(%data);

		case "serverListUpdate":
			%data.schedule(0, delete);
			return;
			GlassServerList.doLiveUpdate(%data.ip, %data.port, %data.key, %data.value);

		case "serverListing":
			%data.schedule(0, delete);
			return;
			GlassServerList.doLiveUpdate(getWord(%data.addr, 0), getWord(%data.addr, 1), "hasGlass", %data.hasGlass);

		case "userAvatar":
			%user = GlassLiveUser::getFromBlid(%data.blid);
			%blid = %data.blid;
			%avatarData = %data.avatar;

			%user.gotAvatar(%avatarData, %data.private);

		case "userLocation":
			//this is only in response to getLocation
			%uo = GlassLiveUser::getFromBlid(%data.blid);
			%uo.countryCode = %data.countryCode;
			%uo.country = %data.country;

			%uo.updateLocation(%data.location, %data.serverTitle, %data.address, %data.passworded, true);

		case "blockedList":
			%list = "";
			for(%i = 0; %i < %data.blocked.length; %i++) {
				%userData = %data.blocked.value[%i];
				%list = %list SPC %userData.blid;

				%user = GlassLiveUser::create(%userData.username, %userData.blid);
				%user.setBlocked(true);
			}

			if(strlen(%list) > 0)
				%list = getSubStr(%list, 1, strlen(%list)-1);

			GlassLive.blockedList = %list;
			GlassLive::createFriendList();

		case "messageBox":
			glassMessageBoxOk(%data.title, %data.text);

		case "glassUpdate":
			if(semanticVersionCompare(Glass.version, %data.version) == 2 && !Glass.updateNotified) {
				new ScriptObject(GlassNotification) {
					title = "Glass Update";
					text = "Version <font:verdana bold:13>" @ %data.version @ "<font:verdana:13> of Blockland Glass is now available!";
					image = "glassLogo";

					sticky = true;
					callback = "updater.checkForUpdates();";
				};
				GlassAudio::play("bell");

				Glass.updateNotified = true;
			}

		case "shutdown":
			%planned = %data.planned;
			%reason = %data.reason;
			%timeout = %data.timeout;

			if(%timeout < 5000) {
				%timeout = 5000;
			}

			if(%reason $= "") {
				%text = "Glass Live has gone offline!";
			} else {
				%text = "Glass Live has gone offline: " @ %reason;
			}

			new ScriptObject(GlassNotification) {
				title = (%planned ? "Planned" : "Unplanned") SPC "Shutdown";
				text = %text;
				image = "roadworks";

				sticky = false;
				callback = "";
			};

			GlassAudio::play("bell");

			%this.doDisconnect();
			GlassLive.noReconnect = false;
			GlassLive.reconnect = GlassLive.schedule(%timeout+getRandom(0, 2000), connectToServer);

		case "disconnected":
			// 0 - server shutdown
			// 1 - other sign-in
			// 2 - barred
			if(%data.reason == 1) {
				GlassLive.noReconnect = true;
				glassMessageBoxOk("Disconnected", "You logged in from somewhere else!");

				%this.doDisconnect();
			} else if(%data.reason == 2) {
				GlassLive.noReconnect = true;
				glassMessageBoxOk("Disconnected", "You've been barred from the Glass Live service.");
				GlassSettings.update("Live::StartupConnect", false);

				%this.doDisconnect();
			}

		case "connectTimeout":
			GlassLive.noReconnect = true;
			%message = %data.message;
			%time = %data.timeout;
			glassMessageBoxOk("Disconnected", %message @ "<br><br>Please wait " @ mCeil(%time/1000) @ " seconds.");

			%this.doDisconnect();

		case "kicked": //we got kicked from all service
			GlassLive.noReconnect = true;
			glassMessageBoxOk("Kicked", "You've been kicked from Glass Live:<font:verdana bold:13><br><br>" @ %data.reason);

			%this.doDisconnect();

		case "barred": //we're not allowed to use glass live
			GlassLive.noReconnect = true;
			if(%data.duration == -1) {
				glassMessageBoxOk("Barred", "You've been permanently barred from the Glass Live service:<br><br>" @ %data.reason);
			} else {
				glassMessageBoxOk("Barred", "You've been barred from the Glass Live service for <font:verdana bold:13>" @ secondsToTimeString(%data.duration) @ ":<br><br>" @ %data.reason);
			}
			GlassSettings.update("Live::StartupConnect", false);

			%this.doDisconnect();

		case "error":
			if(%data.showDialog) {
				glassMessageBoxOk("Glass Live Error", %data.message);
			} else {
				warn("Glass Live Error: " @ %data.message);
			}

		case "groupInvite":
			for(%i = 0; %i < %data.clients.length; %i++) {
				%udata = %data.clients.value[%i];

				%uo = GlassLiveUser::create(%udata.username, %udata.blid);
				%uo.setAdmin(%udata.admin);
				%uo.setMod(%udata.mod);
				if(%uo.blid < 0) {
					%uo.setBot(true);
				}
				%uo.setStatus(%udata.status);
				%uo.setIcon(%udata.icon);
			}

			//%inviter = GlassLive::getFromBlid(%data.blid);
			%inviter = GlassLiveUser::getFromBlid(%data.owner);
			%group = GlassLiveGroups::create(%data.id, %data.name);
			%group.setUserList(%data.clients);

			%group.onInvite(%inviter);

		case "ping":
			%obj = JettisonObject();
			%obj.set("type", "string", "pong");
			%obj.set("key", "string", %data.key);
			%this.send(jettisonStringify("object", %obj) @ "\r\n");
			%obj.delete();

		case "pong":
			if(%data.key $= "keepalive")
				%this.gotKeepalivePong();

		default:
			if(Glass.debug) {
				GlassLog::error("Unhandled Live Call: " @ %data.value["type"]);
			}
	}

	%data.schedule(0, delete);
}

function formatTimeHourMin(%datetime) {
	%t = getWord(%datetime, 0);
	%t = getSubStr(%t, 0, strpos(%t, ":", 4));
	return %t SPC getWord(%datetime, 1);
}
