exec("./submodules/live/GlassLiveConnection.cs");
exec("./submodules/live/GlassLiveUser.cs");
exec("./submodules/live/GlassLiveRoom.cs");
//exec("./submodules/live/GlassLiveGroup.cs");

if(!isObject(GlassFriendsGui)) exec("Add-Ons/System_BlocklandGlass/client/gui/GlassFriendsGui.gui");


// instructions for adding a setting
// - add pref to %settings variable in glasslive::init() below.
// - register setting in glasssettings::init() in common/glassettings.cs
// - if setting is to be changed by the user at will via the glass settings gui, add corresponding .drawsetting() for pref in glasslive::init() below.

//================================================================
//= Table of Contents (subhect to change)
//=
//= 0.   Homeless methods
//= 1.   System-Level methods
//= 2.   Data Management
//= 3.   Chatroom Tabs
//= 4.   Communcations
//= 5.   Blockheads
//= 6.   Friend Gui
//= 7.   AFK Checks
//= 8.   Direct Messages
//= 9.   Direct Messages GUI
//= 10.  Scroll?
//= 11.  User Window
//= 12.  Moderation Gui
//= 13.  Chatroom Gui
//= 14.  Tab Buttons
//= 15.  Icon Selector
//= 16.  Emote Selection
//= 17.  Packages
//================================================================

function GlassLive::init() {
  if(!isObject(GlassLive)) {
    new ScriptObject(GlassLive) {
      // color_blocked = "969696";
      color_default = "222222";
      color_self = "55acee";
      color_friend = "2ecc71";
      color_mod = "e67e22";
      color_admin = "e74c3c";
      color_bot = "9b59b6";
    };

    GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  }

  if(!isObject(GlassLiveUsers))
    new ScriptGroup(GlassLiveUsers);

  if(!isObject(GlassLiveGroups))
    new ScriptGroup(GlassLiveGroups);

  GlassSettingsWindow.setVisible(false);
  GlassOverlayGui.add(GlassSettingsWindow);

  GlassManualWindow.setVisible(false);
  GlassOverlayGui.add(GlassManualWindow);

  GlassIconSelectorWindow.setVisible(false);
  GlassOverlayGui.add(GlassIconSelectorWindow);
  GlassIconSelectorWindow.updateIcons();

  GlassOverlay::setVignette();
  GlassLive::createMessageReminder();
  GlassEmoteSelector::CacheEmotes();

  if(GlassSettings.get("Live::OverlayLogo") && !GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(true);

  GlassOverlay::updateButtonAlignment();
}

//================================================================
//= Homeless methods                                             =
//=                                                              =
//= I can't find anywhere to put these (yet)                     =
//================================================================

function GlassLive::chatColorCheck(%this) {
  %room = GlassLiveRooms::getFromId(0);

  %room.pushText("<font:verdana bold:16><color:" @ %this.color_friend @  ">Friend: <font:verdana:16><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:16><color:" @ %this.color_self @  ">Self: <font:verdana:16><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:16><color:" @ %this.color_mod @  ">Moderator: <font:verdana:16><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:16><color:" @ %this.color_admin @  ">Admin: <font:verdana:16><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:16><color:" @ %this.color_bot @  ">Bot: <font:verdana:16><color:333333>rambling message", 0);
  %room.pushText("<font:verdana bold:16><color:" @ %this.color_default @  ">User: <font:verdana:16><color:333333>rambling message", 0);
  // %room.pushText("<font:verdana bold:12><color:" @ %this.color_blocked @  ">Blocked: <font:verdana:12><color:333333>rambling message", 0);
}

function GlassLive::checkPendingFriendRequests() {
  if(GlassSettings.get("Live::PendingReminder")) {
    if((%pending = getWordCount(GlassLive.friendRequestList)) > 0) {

      new ScriptObject(GlassNotification) {
        title = "Pending Friend Requests";
        text = "You have<font:verdana bold:13>" SPC %pending SPC "<font:verdana:13>pending friend request(s).";
        image = "new_email";

        sticky = false;
        callback = "GlassOverlay::open();";
      };

      GlassAudio::play("bell");
    }
  }
}

function GlassLive::inviteClick(%addr, %blid, %isPassworded) {
  if(%blid !$= "") {
    %uo = GlassLiveUser::getFromBlid(%blid);
    if(%uo == false) {
      glassMessageBoxOk("Invitation Expired", "That invitation has expired!");
      return;
    }

    if(%uo.getLocation() !$= "playing" && %uo.getLocation() !$= "hosting") {
      glassMessageBoxOk("Invitation Expired", %uo.username @ " is no longer there!");
      return;
    }

    if(%uo.getServerAddress() !$= %addr) {
      glassMessageBoxOk("Invitation Expired", %uo.username @ " is no longer there!");
      return;
    }
  }

  if(isObject(ServerConnection)) {
    if(ServerConnection.getAddress() $= %addr) {
      glassMessageBoxOk("Already There", "That's the server you're in right now!");
      return;
    }

    if(ServerConnection.isLocal()) {
      glassMessageBoxYesNo("Stop Hosting", "Would you like to stop hosting and join the server?", "GlassLive::inviteAcceptBusy(\"" @ expandEscape(%addr) @ "\", \"" @ expandEscape(%isPassworded) @ "\");");
    } else {
      glassMessageBoxYesNo("Disconnect", "Would you like to leave this server?", "GlassLive::inviteAcceptBusy(\"" @ expandEscape(%addr) @ "\", \"" @ expandEscape(%isPassworded) @ "\");");
    }
  } else {
    if(%isPassworded) {
      $ServerInfo::Address = %addr;
      canvas.pushDialog(JoinServerPassGui);
    } else {
	  if(isObject(serverConnection))
		  disconnectedCleanup();
      connectToServer(%addr, "", 1, 1);
      canvas.pushDialog(connectingGui);
    }
  }
}

function GlassLive::inviteAcceptBusy(%addr, %isPass) {
  //user is in server but has accepted the invite
  disconnect();
  GlassLive.isInviteAccepted = true;
  GlassLive.inviteAddress = %addr;
  GlassLive.invitePass = %isPass;
}

//================================================================
//= System-level methods                                         =
//================================================================

function GlassLive::onAuthSuccess() {
  GlassLive::createBlockhead();

  GlassLive_StatusSwatch.setVisible(true);
  GlassFriendsGui_StatusSelect::selectStatus("Online");

  GlassFriendsGui_Blockhead.setVisible(true);
  GlassFriendsGui_Blockhead.setOrbitDist(5.5);
  GlassFriendsGui_Blockhead.setCameraRot(0.22, 0.5, 2.8);

  GlassFriendsGui_HeaderText.setText("<font:verdana bold:14>" @ $Pref::Player::NetName @ "<br><font:verdana:12>" @ getNumKeyId());

  GlassFriendsGui_HeaderText.position = "10 5";

  GlassLive.disableAvatarTelemetry = (GlassSettings.get("Live::ViewAvatar") $= "Myself");
  GlassLive.disableLocationTelemetry = (GlassSettings.get("Live::ViewLocation") $= "Myself");

  if(!GlassLive.disableAvatarTelemetry)
    GlassLive::sendAvatarData();

  if(!GlassLive.disableLocationTelemetry)
    GlassLive::updateLocation();

  if(trim(GlassSettings.get("Live::Rooms")) $= "" || !GlassSettings.get("Live::AutoJoinRooms")) {
    if(!isObject($Glass::defaultRoomWindow))
      $Glass::defaultRoomWindow = GlassLive::createChatroomWindow();

    %window = $Glass::defaultRoomWindow;
    %window.setText("Glass Chatroom List");
    %window.renderTabs();
    %window.openRoomBrowser();
  } else {
    if(GlassSettings.get("Live::AutoJoinRooms")) {
      %roomStr = GlassSettings.get("Live::Rooms");

      for(%i = 0; %i < getWordCount(%roomStr); %i++) {
        GlassLive::joinRoom(getWord(%roomStr, %i));
      }
    }
  }

  new ScriptObject(GlassNotification) {
    title = "Signed In";
    text = "You have been signed in to Glass Live.";
    image = "networking_green";

    sticky = false;
  };

  GlassLive.schedule(1000, "checkPendingFriendREquests");
  GlassLive.waitingForAuth = false;
}

function GlassLive::disconnect() {
  GlassLive::cleanup();

  if(isObject(GlassLiveConnection)) {
    GlassLiveConnection.doDisconnect();
  }
}

function GlassLive::cleanup() {
  GlassLiveUsers.deleteAll();
  GlassLive.friendList = "";
  GlassFriendsGui_ScrollSwatch.deleteAll();
  GlassFriendsGui_ScrollSwatch.setVisible(true);

  if(isObject(GlassLiveRoomGroup)) {
    for(%i = 0; %i < GlassLiveRoomGroup.getcount(); %i++) {
      %room = GlassLiveRoomGroup.getObject(%i);
      if(isObject(%room.view)) {
        %room.view.deleteAll();
        %room.view.schedule(0, delete);
      }
    }

    GlassLiveRoomGroup.deleteAll();
  }

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow" || %window.getName() $= "GlassMessageGui" || %window.getName() $= "GlassUserGui") {
      %window.deleteAll();
      %window.delete();
      %i--;
    } else if(%window.getName() $= "GlassIconSelectorWindow") {
      %window.setVisible(false);
    }
  }

  GlassFriendsGui_ScrollSwatch.verticalMatchChildren(0, 5);
  GlassFriendsGui_ScrollSwatch.setVisible(true);
  GlassFriendsGui_ScrollSwatch.getGroup().setVisible(true);
  GlassModeratorWindow.setVisible(false);
  GlassLiveModerationButton.setVisible(false);
  if(isObject(GlassFriendsGui_Blockhead))
    GlassFriendsGui_Blockhead.setVisible(false);
  GlassFriendsGui_StatusSelect.setVisible(false);

  GlassLive.afkCheck(false);
}


//================================================================
//= Data Management                                              =
//================================================================

function GlassLive::addFriendToList(%user) {
  if(wordPos(GlassLive.friendList, %user.blid) != -1) {
    return;
  }

  GlassLive.friendList = setWord(GlassLive.friendList, getWordCount(GlassLive.friendList), %user.blid);
}

function GlassLive::removeFriendFromList(%blid) {
  if((%i = wordPos(GlassLive.friendList, %blid)) == -1) {
    return;
  }

  GlassLive.friendList = removeWord(GlassLive.friendList, %i);
}

function GlassLive::addFriendRequestToList(%user) {
  if(wordPos(GlassLive.friendRequestList, %user.blid) != -1) {
    return;
  }

  GlassLive.friendRequestList = setWord(GlassLive.friendRequestList, getWordCount(GlassLive.friendRequestList), %user.blid);
}

function GlassLive::removeFriendRequestFromList(%blid) {
  if((%i = wordPos(GlassLive.friendRequestList, %blid)) == -1) {
    return;
  }

  GlassLive.friendRequestList = removeWord(GlassLive.friendRequestList, %i);
}

function GlassLive::updateSetting(%category, %setting) {
  %box = "GlassSettingsGui_Prefs_" @ %setting;
  GlassSettings.update(%category @ "::" @ %setting, %box.getValue());
  %box.setValue(GlassSettings.get(%category @ "::" @ %setting));

  if(strlen(%callback = GlassSettings.obj[%setting].callback)) {
    if(isFunction(%callback)) {
      call(%callback);
    }
  }
}

function wordPos(%str, %word) {
  for(%i = 0; %i < getWordCount(%str); %i++)
    if(getWord(%str, %i) $= %word)
      return %i;
  return -1;
}

function secondsToTimeString(%total) { // Crown, amended by Shock 2/24/18
  if(%total == -1)
    return "infinity";

  %years = mFloor(%total / 31536000);
  %remainder = %total % 31536000;

  %days = mFloor(%remainder / 86400);
  %remainder = %remainder % 86400;

  %hours = mFloor(%remainder / 3600);
  %remainder = %remainder % 3600;

  %minutes = mFloor(%remainder / 60);

  %seconds = mFloor(%remainder % 60);

  %ys = (%years != 1 ? "s" : "");
  %ds = (%days != 1 ? "s" : "");
  %hs = (%hours != 1 ? "s" : "");
  %ms = (%minutes != 1 ? "s" : "");
  %ss = (%seconds != 1 ? "s" : "");

  if(%years > 0)
    %str = %str SPC %years SPC "year" @ %ys;
  if(%days > 0)
    %str = %str SPC %days SPC "day" @ %ds;
  if(%hours > 0)
    %str = %str SPC %hours SPC "hour" @ %hs;
  if(%minutes > 0)
    %str = %str SPC %minutes SPC "minute" @ %ms;
  if(%seconds > 0)
    %str = %str SPC %seconds SPC "second" @ %ss;

  return trim(%str);
}

//================================================================
//= Chatroom Tabs                                                =
//================================================================

function GlassChatroomWindow::addTab(%this, %tabObj) {
  if(%this.tabId[%tabObj] !$= "" && %tabObj.window.getId() == %this.getId()) {
    //move to last position
    for(%i = %this.tabId[%tabObj]; %i < %this.tabs; %i++) {
      %this.tab[%i] = %this.tab[%i+1];
      %this.tabId[%this.tab[%i]] = %i;
    }
    %this.tab[%this.tabs-1] = %tabObj;
    %this.tabId[%tabObj] = %this.tabs-1;

    %this.renderTabs();
    %this.openTab(%this.tabId[%tabObj]);
    return;
  }

  if(%this.isAwake()) {
    %this.schedule(0, resize, getWord(%this.position, 0), getWord(%this.position, 1), getWord(%this.extent, 0), getWord(%this.extent, 1));
  }

  if(isObject(%tabObj.window)) {
    %tabObj.window.removeTab(%tabObj);
  }

  %this.tab[%this.tabs] = %tabObj;
  %this.tabId[%tabObj] = %this.tabs;
  %this.tabs++;

  %this.add(%tabObj);

  %tabObj.window = %this;

  %this.renderTabs();

  //always open to the added tab
  %this.openTab(%this.tabId[%tabObj]);
}

function GlassChatroomWindow::removeTab(%this, %tabObj) {
  %id = %this.tabId[%tabObj];
  if(%id $= "") {
    return false;
  }

  %this.removeTabId(%id);
}

function GlassChatroomWindow::removeTabId(%this, %id) {
  %tabObj = %this.tabId[%id];

  if(isObject(GlassLive_EmoteSelection)) {
    if(GlassOverlay.activeInput == %this.tab[%id].input) {
      GlassLive_EmoteSelection.delete();
    }
  }

  %tabObj.window = "";

  %this.tab[%id] = "";
  %this.tabId[%tabObj] = "";
  %this.tabButton[%id].delete();

  for(%i = %id; %i < %this.tabs; %i++) {
    %o = %this.tab[%i+1];

    %this.tabId[%o] = %i;
    %this.tab[%i] = %o;

    if(isObject(%this.tabButton[%i]))
      %this.tabButton[%i].delete();
  }

  %this.tabs--;

  if(!%this.tabs) {

    %ct = 0;
    for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
      if(GlassOverlayGui.getObject(%i).getName() $= "GlassChatroomWindow")
        %ct++;
    }

    if(%ct > 1) {
      %this.schedule(0, delete);
    } else {
      %this.openRoomBrowser();
      %this.renderTabs();
      %this.setText("Glass Chatroom List");
    }

  } else {
	if(%this.activeTabId == -1)
		%browserOpen = 1;
    else if(%this.activeTabId == %id) {
      if(%id >= %this.tabs)
        %this.openTab(%this.tabs-1);
      else
        %this.openTab(%id);
    } else if(%this.activeTabId >= %id) {
      %this.activeTabId--;
    }
	if(%browserOpen)
		%this.openTab(0);
    %this.renderTabs();
  }
}

function GlassChatroomWindow::openTab(%this, %id) {
  %current = %this.activeTab;
  %currentId = %this.activeTabId;
  if(isObject(%current)) {
    %button = %this.tabButton[%currentId];
    if(isObject(%button))
      %button.mouseCtrl.setUse(false);

    %current.setVisible(false);
    // %current.room.setAwake(false);
  }

  if(isObject(%this.browserSwatch)) {
    %this.browserSwatch.delete();
  }

  %tab = %this.tab[%id];
  %this.activeTabId = %id;
  if(isObject(%tab)) {
    %this.activeTab = %tab;
    %tab.setVisible(true);

    %button = %this.tabButton[%id];
    if(isObject(%button))
      %button.mouseCtrl.setUse(true);

    if(%tab.getName() $= "GlassChatroomTab") {
      %this.text = "Glass Chatroom - " @ %tab.title; // @ " - " @ %this.getId();
      %this.setText(%this.text);

      // %tab.room.setAwake(true);
      %tab.setFlashing(false);
    }

    if(%this.isAwake()) {
      %tab.chattext.forceReflow();
      %tab.scrollSwatch.verticalMatchChildren(0, 2);
      %tab.scrollSwatch.setVisible(true);
      %tab.scroll.scrollToBottom();
    }
  }

  %this.resize.schedule(0, onResize);

  GlassOverlay::resetFocus();
}

function GlassLive::enterRoomDragMode(%obj, %pos) {
  if(isObject(GlassLiveDrag) || GlassLive.dragMode)
    return;

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      %window.setDropMode(true);
    }
  }

  new GuiMouseEventCtrl(GlassLiveDrag) {
    position = "0 0";
    extent = getRes();

    dragObj = %obj;
  };

  GlassOverlayGui.add(GlassLiveDrag);
  GlassOverlayGui.add(%obj);
  GlassLiveDrag.updatePosition(%pos);

  GlassLive.dragMode = true;
}

function GlassLiveDrag::onMouseDragged(%this, %a, %pos) {
  %this.updatePosition(%pos);
}

function GlassLiveDrag::onMouseUp(%this, %a, %pos) {
  %dropX = getWord(%pos, 0);
  %dropY = getWord(%pos, 1);

  //this loop is used to exit all windows from Drop Mode
  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() !$= "GlassChatroomWindow")
      continue;

    %window.setDropMode(false);
  }


  //this loop is used specifically to detect the whether to open a new window or add
  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() !$= "GlassChatroomWindow")
      continue;

    %posX = getWord(%window.position, 0);
    %posY = getWord(%window.position, 1);

    %extX = getWord(%window.extent, 0);
    %extY = getWord(%window.extent, 1);

    if(%dropX < %posX)
      continue;

    if(%dropX > %posX+%extX)
      continue;

    if(%dropY < %posY)
      continue;

    if(%dropY > %posY+%extY)
      continue;

    %newWindow = %window;
    break;
  }

  if(%newWindow $= "") {
    %newWindow = GlassLive::createChatroomWindow();
    %newWindow.position = %pos;

    %endPos = vectorAdd(%newWindow.position, %newWindow.extent);
    if(getWord(%endPos, 0) > getWord(getRes(), 0)) {
      %newWindow.position = setWord(%newWindow.position, 0, getWord(getRes(), 0)-getWord(%newWindow.extent, 0));
    }

    if(getWord(%endPos, 1) > getWord(getRes(), 1)) {
      %newWindow.position = setWord(%newWindow.position, 1, getWord(getRes(), 1)-getWord(%newWindow.extent, 1));
    }
  }

  %newWindow.addTab(%this.dragObj.tabObj);

  %newWindow.extent = %extX SPC %extY;

  %newWindow.schedule(0, resize, getWord(%newWindow.position, 0), getWord(%newWindow.position, 1), getWord(%newWindow.extent, 0), getWord(%newWindow.extent, 1));

  if(isObject(%this.dragObj))
    %this.dragObj.delete();

  %this.delete();

  GlassLive.dragMode = false;
}

function GlassLiveDrag::updatePosition(%this, %pos) {
  %xOffset = getWord(%this.dragObj.extent, 0)/2;
  %yOffset = getWord(%this.dragObj.extent, 1)/2;

  %this.dragObj.position = vectorSub(%pos, %xOffset SPC %yOffset);

  GlassOverlayGui.pushToBack(%this.dragObj);
  GlassOverlayGui.pushToBack(%this);
}


//================================================================
//= Communcations                                                =
//================================================================

//====
// User data
//====

function GlassLive::setStatus(%status) {
  %status = strlwr(%status);


  // this is not valid. there is no promise that a glassliveuser has been created
  // for the local user

  if(%status $= GlassLive.status)
    return;

  if(%status $= "online" || %status $= "away" || %status $= "busy") {
    %obj = JettisonObject();
    %obj.set("type", "string", "setStatus");
    %obj.set("status", "string", %status);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

    %obj.delete();

    GlassLive.status = %status;
  }
}

function GlassLive::setIcon(%icon) {
  if(!isFile("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ ".png"))
    return;

  %obj = JettisonObject();
  %obj.set("type", "string", "setIcon");
  %obj.set("icon", "string", %icon);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  GlassIconSelectorWindow_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon);
}

function GlassLive::sendAvatarData() {
  if(!GlassLiveConnection.connected)
    return;

  if(GlassLive.disableAvatarTelemetry)
    return;

  %obj = JettisonObject();
  %obj.set("type", "string", "avatar");

  %partArray = JettisonArray();

  %FaceName = $Pref::Avatar::FaceName;
  %DecalName = $Pref::Avatar::DecalName;
  %HeadColor = $Pref::Avatar::HeadColor;
  %obj.set("faceName", "string", %faceName);
  %obj.set("decalName", "string", %decalName);
  %obj.set("headColor", "string", %headColor);

  %accent = $Pref::Avatar::accent;
  %accentColor = $Pref::Avatar::accentColor;
  %obj.set("accent", "string", %accent);
  %obj.set("accentColor", "string", %accentColor);

  %hat = $Pref::Avatar::hat;
  %hatColor = $Pref::Avatar::hatColor;
  %obj.set("hat", "string", %hat);
  %obj.set("hatColor", "string", %hatColor);

  %chest = $Pref::Avatar::chest;
  %chestColor = $Pref::Avatar::chestColor;
  %TorsoColor = $Pref::Avatar::TorsoColor;
  %obj.set("chest", "string", %chest);
  %obj.set("chestColor", "string", %chestColor);
  %obj.set("torsoColor", "string", %torsoColor);

  %pack = $Pref::Avatar::pack;
  %packColor = $Pref::Avatar::packColor;
  %obj.set("pack", "string", %pack);
  %obj.set("packColor", "string", %packColor);

  %secondPack = $Pref::Avatar::secondPack;
  %secondPackColor = $Pref::Avatar::secondPackColor;
  %obj.set("secondPack", "string", %secondPack);
  %obj.set("secondPackColor", "string", %secondPackColor);

  %larm = $Pref::Avatar::larm;
  %larmColor = $Pref::Avatar::larmColor;
  %obj.set("larm", "string", %larm);
  %obj.set("larmColor", "string", %larmColor);

  %rarm = $Pref::Avatar::rarm;
  %rarmColor = $Pref::Avatar::rarmColor;
  %obj.set("rarm", "string", %rarm);
  %obj.set("rarmColor", "string", %rarmColor);

  %lhand = $Pref::Avatar::lhand;
  %lhandColor = $Pref::Avatar::lhandColor;
  %obj.set("lhand", "string", %lhand);
  %obj.set("lhandColor", "string", %lhandColor);

  %rhand = $Pref::Avatar::rhand;
  %rhandColor = $Pref::Avatar::rhandColor;
  %obj.set("rhand", "string", %rhand);
  %obj.set("rhandColor", "string", %rhandColor);

  %hip = $Pref::Avatar::hip;
  %hipColor = $Pref::Avatar::hipColor;
  %obj.set("hip", "string", %hip);
  %obj.set("hipColor", "string", %hipColor);

  %lleg = $Pref::Avatar::lleg;
  %llegColor = $Pref::Avatar::llegColor;
  %obj.set("lleg", "string", %lleg);
  %obj.set("llegColor", "string", %llegColor);

  %rleg = $Pref::Avatar::rleg;
  %rlegColor = $Pref::Avatar::rlegColor;
  %obj.set("rleg", "string", %rleg);
  %obj.set("rlegColor", "string", %rlegColor);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

function GlassLive::updateLocation(%inServer) {
  if(!GlassLiveConnection.connected)
    return;

  if(GlassLive.disableLocationTelemetry)
    return;

  if(%inServer $= "") {
    %inServer = GlassLive.inServer;
  } else {
    GlassLive.inServer = %inServer;
  }

  if(!%inServer) {
    %action = "menus";
  } else if(ServerConnection.isLocal()) {
    if($Server::LAN) {
      if($Server::ServerType $= "Singleplayer") {
        %action = "singleplayer";
      } else {
        %action = "hosting_lan";
      }
    } else {
      %action = "hosting";
    }
  } else {
    %ip = ServerConnection.getRawIP();
    if(strPos("192.168.", %ip) == 0 || strpos("10.", %ip) == 0 || strpos("127.0.0.", %ip) == 0) {
      %action = "playing_lan";
    } else {
      %action = "playing";
    }
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "updateLocation");
  %obj.set("location", "string", %action);

  if(%action $= "playing") {
    %location = ServerConnection.getRawIP() @ ":" @ ServerConnection.getPort();
    %name = NPL_Window.getValue();
    %name = getSubStr(%name, strpos(%name, "-")+2, strlen(%name));

    %obj.set("address", "string", %location);
    %obj.set("passworded", "string", $ServerInfo::Password);
  }

  if(%action $= "hosting") {
    %obj.set("port", "string", $Server::Port);
    %obj.set("serverName", "string", $Pref::Server::Name);
    %obj.set("passworded", "string", $ServerInfo::Password);
  } else if(%inServer) {
    %obj.set("serverName", "string", $ServerInfo::Name);
  }

  if(isObject(GlassLiveConnection) && GlassLiveConnection.connected) {
    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }

  %obj.delete();
}

function GlassLive::getEmotedMessage(%message) {
  for(%i = 0; %i < getWordCount(%message); %i++) {
    %word = getWord(%message, %i);
    %validEmote = (getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":" && getsubstr(%word, 1, 1) !$= ":" && getsubstr(%word, strlen(%word) - 2, 1) !$= ":");
    if(%validEmote) {
      %bitmap = strlwr(stripChars(%word, "[]\\/{};:'\"<>,./?!@#$%^&*-=+`~;"));
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:" @ %bitmap @ ">";
        %message = setWord(%message, %i, strlwr(%word));
      }
    }
  }
  return %message;
}

//====
// Rooms
//====

function GlassLive::joinRoom(%id) {
  %room = GlassLiveRooms::getFromId(%id);

  if(isObject(GlassLive.room[%id]))
    return;

  if(isObject(%room.window)) {
    GlassOverlayGui.add(%room.window);
    GlassOverlayGui.pushToBack(%room.window);
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "roomJoin");
  %obj.set("id", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  schedule(1000, "GlassOverlayGui", "GlassCheckModeratorButton");

  // GlassChatroomWindow.schedule(0, openRoomBrowser);
}

function GlassLive::sendRoomMessage(%msg, %id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomChat");
  %obj.set("message", "string", getUTF8String(%msg));
  %obj.set("room", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  %obj.delete();

  if(GlassSettings.get("Live::TalkingAnimation"))
    GlassLive::BlockheadAnim("talk", strLen(%msg) * 50);
}

function GlassLive::sendRoomCommand(%msg, %id) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomCommand");
  %obj.set("message", "string", %msg);
  %obj.set("room", "string", %id);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

//====
// Direct Messages
//====

function GlassLive::sendMessage(%blid, %msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "message");
  %obj.set("message", "string", %msg);
  %obj.set("target", "string", %blid);

  GlassLive.typing[%blid] = false;

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
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

function GlassLive::createMessageReminder() {
  if(isObject(GlassMessageReminder))
    GlassMessageReminder.delete();

	if(!GlassSettings.get("Live::ReminderIcon"))
	  return;

  new GuiBitmapCtrl(GlassMessageReminder) {
    extent = "16 16";
	  bitmap = "Add-Ons/System_BlocklandGlass/image/icon/glassLogo";
    mColor = "255 255 255 220";
    mMultiply = "0";
  };
  NewChatHUD.add(GlassMessageReminder);
  GlassMessageReminder.setVisible(false);
  GlassLive::positionMessageReminder();
}

function GlassLive::positionMessageReminder() {
	if(!GlassSettings.get("Live::ReminderIcon"))
	  return;

  if(!isObject(GlassMessageReminder))
    GlassLive::createMessageReminder();

  GlassMessageReminder.resize(getWord(getRes(), 0) - 26, getWord(getRes(), 1) - 26, 16, 16);
}

//====
// Friends
//====

function GlassLive::sendFriendRequest(%blid) {
  if(%blid == getNumKeyId()) {
    glassMessageBoxOk("Invalid Request", "Blockland Glass is a social platform and as such you are not able to friend yourself at this present time.<br><br>We apologize for the inconvenience, please find some real friends instead.");
    return;
  }

  if((%blid+0 !$= %blid) || %blid < 0) {
    glassMessageBoxOk("Invalid Request", "That is not a valid Blockland ID!");
    return;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  if(%user.isBlocked()) {
    glassMessageBoxOk("Blocked", "You have blocked this user, unblock them before attempting to send a friend request.");
    return;
  }

  if(wordPos(GlassLive.friendRequestList, %blid) != -1) { // insert has friend request here if exists in glassliveuser as func?
    GlassLive::friendAccept(%blid);
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "friendRequest");
  %obj.set("target", "string", %blid);

  glassMessageBoxOk("Friend Request Sent", "Friend request sent to BLID <font:verdana bold:13>" @ %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::friendAccept(%blid) {
  %obj = JettisonObject();
  %obj.set("type", "string", "friendAccept");
  %obj.set("blid", "string", %blid);

  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user) {
    %user.setFriend(true);

    if(isObject(%room = GlassChatroomWindow.activeTab.room))
      %room.userListUpdate(%user);
  }

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

function GlassLive::friendDecline(%blid) {
  if((%i = wordPos(GlassLive.friendRequestList, %blid)) == -1) {
    return;
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "friendDecline");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  GlassLive.friendRequestList = removeWord(GlassLive.friendRequestList, %i);

  GlassLive::createFriendList();
}

function GlassLive::removeFriend(%blid, %silent) {
  if(%blid == getNumKeyId()) {
    return;
  }

  if(wordPos(GlassLive.friendList, %blid) == -1) {
    return;
  }

  if(!%silent) {
    %obj = JettisonObject();
    %obj.set("type", "string", "friendRemove");
    %obj.set("blid", "string", %blid);

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

    %obj.delete();
  }

  GlassLive::removeFriendFromList(%blid);
  GlassLive::createFriendList();

  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user) {
    %user.setFriend(false);
    if(isObject(%user.window))
      GlassLive::openUserWindow(%blid);
    if(isObject(%room = GlassChatroomWindow.activeTab.room))
      %room.userListUpdate(%blid);
  }
}

function GlassLive::inviteFriend(%blid) {
  %friend = GlassLiveUser::getFromBlid(%blid);

  //validation is on server side

  %obj = JettisonObject();
  %obj.set("type", "string", "friendInvite");
  %obj.set("blid", "string", %blid);
  %obj.set("passworded", "string", $ServerInfo::Password);
  %obj.set("name", "string", $ServerInfo::Name);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();
}

//====
// Users
//====

function GlassLive::userBlock(%blid) {
  if(%blid+0 !$= %blid || %blid < 0 || mfloor(%blid) !$= %blid)
    return;

  if(%blid == getNumKeyId())
    return;

  if(wordPos(GlassLive.friendRequestList, %blid) != -1) {
    GlassLive::friendDecline(%blid);
    return;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  %blockedIcon = "wall";

  if(isObject(%user)) {
    if(%user.isBlocked())
      return;

    %user.setBlocked(true);

    if(isObject(%user.window))
      GlassLive::openUserWindow(%blid);

    %user.setIcon(%blockedIcon);

    if(isObject(%dm = %user.getMessageGui())) {
      %dm.input.setValue("");
      %dm.input.enabled = false;
      %dm.blockButton.mColor = "237 184 105 200";
      %dm.blockButton.command = "GlassLive::userUnblock(" @ %blid @ ");";
      %dm.blockButton.text = "U";
    }
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "block");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  for(%i = 0; %i < getWordCount(GlassLive.blockedList); %i++) {
    if(getWord(GlassLive.blockedList, %i) == %blid) {
      return;
    }
  }

  GlassLive.blockedList = trim(GlassLive.blockedList SPC %blid);

  %username = %user.username;

  if(%username $= "")
    %username = "Blockhead" @ %blid;

  GlassLive::createFriendList();
  GlassLive::onMessageNotification("You have blocked " @ %username @ " (" @ %blid @ ").", %blid);
  if(isObject(%room = GlassChatroomWindow.activeTab.room))
    %room.pushText("You have blocked " @ %username @ " (" @ %blid @ ").");
}

function GlassLive::userUnblock(%blid) {
  if(%blid+0 !$= %blid || %blid < 0 || mfloor(%blid) !$= %blid)
    return;

  if(%blid == getNumKeyId())
    return;

  %user = GlassLiveUser::getFromBlid(%blid);

  if(isObject(%user)) {
    if(!%user.isBlocked())
      return;

    %user.setBlocked(false);

    if(isObject(%user.window))
      GlassLive::openUserWindow(%blid);

    %user.setIcon(%user.realIcon);

    if(isObject(%dm = %user.getMessageGui())) {
      %dm.input.enabled = true;
      %dm.blockButton.mColor = "237 118 105 200";
      %dm.blockButton.command = "GlassLive::userBlock(" @ %blid @ ");";
      %dm.blockButton.text = "B";
    }
  }

  %obj = JettisonObject();
  %obj.set("type", "string", "unblock");
  %obj.set("blid", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");

  %obj.delete();

  for(%i = 0; %i < getWordCount(GlassLive.blockedList); %i++) {
    if(getWord(GlassLive.blockedList, %i) == %blid) {
      GlassLive.blockedList = removeWord(GlassLive.blockedList, %i);
    }
  }

  %username = %user.username;

  if(%username $= "")
    %username = "Blockhead" @ %blid;

  GlassLive::createFriendList();
  GlassLive::onMessageNotification("You have unblocked " @ %username @ " (" @ %blid @ ").", %blid);
  if(isObject(%room = GlassChatroomWindow.activeTab.room))
    %room.pushText("You have unblocked " @ %username @ " (" @ %blid @ ").");
}


//================================================================
//= Blockheads                                                   =
//================================================================

function GlassLive::createBlockhead() {
  AvatarGui.OnWake();

  if(isObject(GlassFriendsGui_Blockhead))
	  GlassFriendsGui_Blockhead.delete();

  new GuiObjectView(GlassFriendsGui_Blockhead) {
    forceFOV = "18";
    lightDirection = "0 0.2 0.2";
    extent = "74 176";
  };

  if(isObject(GlassFriendsGui_BlockheadAnim))
    GlassFriendsGui_BlockheadAnim.delete();

  new GuiButtonBaseCtrl(GlassFriendsGui_BlockheadAnim) {
    extent = "74 176";
    command = "GlassLive::BlockheadRandomAnim();";
  };

  GlassFriendsGui_Blockhead.add(GlassFriendsGui_BlockheadAnim);
  GlassFriendsGui_Blockhead.setVisible(false);
  GlassFriendsGui_Blockhead.position = "150 -5";
  GlassFriendsGui_Blockhead.lightDirection = "0 0.2 0.2";
  GlassFriendsGui_Blockhead.createBlockhead(0, true);
  GlassFriendsGui_InfoSwatch.add(GlassFriendsGui_Blockhead);

  GlassFriendsGui_Blockhead.schedule(500, "setVisible", true);
  GlassFriendsGui_Blockhead.schedule(200, "setOrbitDist", 5.5);
  GlassFriendsGui_Blockhead.schedule(200, "setCameraRot", 0.22, 0.5, 2.8);
}

function GlassLive::BlockheadRandomAnim() {
  %rand = getRandom(0, 3);

  switch (%rand) {
	  case 0:
		%thread = "talk";
	  case 1:
		%thread = "undo";
	  case 2:
		%thread = "back";
	  case 3:
		%thread = "headside";
  }

  if(%time $= "")
    %time = 400;

  GlassLive::BlockheadAnim(%thread, %time);
}

function GlassLive::BlockheadAnim(%thread, %time, %type) {
  %blockhead = GlassFriendsGui_Blockhead;
  if(!isObject(%blockhead))
    return;

  cancel(%blockhead.rootSchedule);

  if(%type $= "")
	  %type = 1;

  GlassFriendsGui_Blockhead.setSequence("", %type, %thread, 1);

  %blockhead.rootSchedule = %blockhead.schedule(%time, "setSequence", "", 1, "root", 1);

  alxPlay(GlassPop1Audio);
}

function GuiObjectView::createBlockhead(%this, %json, %usePlayerAvatar) {
  %this.forceFOV = 18;

  %this.setObject("", "base/data/shapes/player/m.dts", "", 100);
  if(%usePlayerAvatar) {
    %FaceName = $Pref::Avatar::FaceName;
    %DecalName = $Pref::Avatar::DecalName;
    %HeadColor = $Pref::Avatar::HeadColor;

    if(%FaceName $= "smiley")
      %FaceName = "default";

    %accent = $Pref::Avatar::accent;
    %accentColor = $Pref::Avatar::accentColor;

    %hat = $Pref::Avatar::hat;
     %hatColor = $Pref::Avatar::hatColor;

    %chest = $Pref::Avatar::chest;
    %chestColor = $Pref::Avatar::chestColor;
    %TorsoColor = $Pref::Avatar::TorsoColor;

    %pack = $Pref::Avatar::pack;
    %packColor = $Pref::Avatar::packColor;

    %secondPack = $Pref::Avatar::secondPack;
    %secondPackColor = $Pref::Avatar::secondPackColor;

    %larm = $Pref::Avatar::larm;
    %larmColor = $Pref::Avatar::larmColor;

    %rarm = $Pref::Avatar::rarm;
    %rarmColor = $Pref::Avatar::rarmColor;

    %lhand = $Pref::Avatar::lhand;
    %lhandColor = $Pref::Avatar::lhandColor;

    %rhand = $Pref::Avatar::rhand;
    %rhandColor = $Pref::Avatar::rhandColor;

    %hip = $Pref::Avatar::hip;
    %hipColor = $Pref::Avatar::hipColor;

    %lleg = $Pref::Avatar::lleg;
    %llegColor = $Pref::Avatar::llegColor;

    %rleg = $Pref::Avatar::rleg;
    %rlegColor = $Pref::Avatar::rlegColor;
  } else if(!isObject(%json)) {
    %color = "0 0 0 1";

    %FaceName = "default";
    %DecalName = "AAA-None";
    %HeadColor = %color;

    %accent = 0;
    %accentColor = %color;

    %hat = 0;
    %hatColor = %color;

    %chest = 0;
    %chestColor = %color;
    %TorsoColor = %color;

    %pack = 0;
    %packColor = %color;

    %secondPack = 0;
    %secondPackColor = %color;

    %larm = 0;
    %larmColor = %color;

    %rarm = 0;
    %rarmColor = %color;

    %lhand = 0;
    %lhandColor = %color;

    %rhand = 0;
    %rhandColor = %color;

    %hip = 0;
    %hipColor = %color;

    %lleg = 0;
    %llegColor = %color;

    %rleg = 0;
    %rlegColor = %color;
  } else {
    %FaceName = %json.faceName;
    %DecalName = %json.decalName;
    %HeadColor = %json.headColor;

    if(%FaceName $= "smiley")
      %FaceName = "default";

    %accent = %json.accent;
    %accentColor = %json.accentColor;

    %hat = %json.hat;
    %hatColor = %json.hatColor;

    %chest = %json.chest;
    %chestColor = %json.chestColor;
    %TorsoColor = %json.torsoColor;

    %pack = %json.pack;
    %packColor = %json.packColor;

    %secondPack = %json.secondPack;
    %secondPackColor = %json.secondPackColor;

    %larm = %json.larm;
    %larmColor = %json.larmColor;

    %rarm = %json.rarm;
    %rarmColor = %json.rarmColor;

    %lhand = %json.lhand;
    %lhandColor = %json.lhandColor;

    %rhand = %json.rhand;
    %rhandColor = %json.rhandColor;

    %hip = %json.hip;
    %hipColor = %json.hipColor;

    %lleg = %json.lleg;
    %llegColor = %json.llegColor;

    %rleg = %json.rleg;
    %rlegColor = %json.rlegColor;
  }

  if(%hat == 0 || %hat == 2 || %hat == 3 || %hat == 5)
    %accent = 0;

  if(%pack > 0 || %secondPack > 0)
    %this.setSequence("", 1, headup, 1);
  else
    %this.setSequence("", 1, headup, 0);

  for(%i = 0; %i < $numFace; %i++)
	if(strStr($Face[%i], %FaceName) >= 0)
	  %faceDecal = %i;

  for(%i = 0; %i < $numDecal; %i++)
    if(strStr($Decal[%i], %DecalName) >= 0)
	   %shirtDecal = %i;

  %this.setIFLFrame("", "face", %faceDecal);
  %this.setIFLFrame("", "decal",%shirtDecal);
  %this.setNodeColor("", "ALL", %HeadColor);

  %bodyParts = "accent hat chest pack secondpack larm rarm lhand rhand hip lleg rleg";
  for(%i = 0; %i <= 11; %i++) {
    %currPart = getWord(%bodyParts, %i);
    %numPart = $num[%currPart];

	for(%j = 0; %j <= %numPart; %j++) {
	  eval("%equipCurrPart = %" @ %currPart @ ";");
	  eval("%equipCurrPartColor = %" @ %currPart @ "Color;");
	  eval("%currCheck = $" @ %currPart @ "[" @ %j @ "];");

	  if(%currCheck $= "" || %currCheck $= "None")
		  continue;

	  if(%j $= %equipCurrPart) {
      %this.unHideNode("", %currCheck);
      %this.setNodeColor("", %currCheck, %equipCurrPartColor);
	  }
	  else
      %this.hideNode("", %currCheck);
    }
  }
  %accent = getWord($accentsAllowed[$hat[%hat]], %accent);
  if(%accent !$= "" && %accent !$= "none") {
    %this.unhidenode("", %accent);
    %this.hideNode("", "plume");
    %this.setnodeColor("", %accent, %accentColor);
  }

  if($Hip[%hip] !$= "skirtHip") {
    %this.hideNode("", "SkirtTrimLeft");
    %this.hideNode("", "SkirtTrimRight");
  } else {
    %this.hideNode("", "rShoe");
    %this.hideNode("", "lShoe");
  }

  %this.hideNode("", "rski");
  %this.hideNode("", "lski");

  %this.setNodeColor("", $chest[%chest], %torsoColor);
  %this.setMouse(1, 1);
}

function GlassUserGui_Blockhead::onSleep(%this) {
  %this.position = "0 -20";
  %this.firstOpen = false;
}

function GlassUserGui_Blockhead::onWake(%this) {
  if(!%this.firstOpen) {
    //%this.position = "0 0";
  } else {
    %this.position = "0 -20";
  }
}

//================================================================
//= Friend Gui                                                   =
//================================================================

function GlassFriendsResize::onResize(%this, %x, %y, %h, %l) {
  GlassFriendsGui_Scroll.extent = vectorSub(GlassFriendsWindow.extent, "20 135");
  GlassFriendsGui_ScrollOverlay.extent = GlassFriendsGui_Scroll.extent;

  GlassFriendsGui_AddButton.position   = vectorAdd(GlassFriendsGui_Scroll.extent, "-230 104");
  GlassFriendsGui_BlockButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-95 104");
  GlassFriendsGui_PowerButton.position = vectorAdd(GlassFriendsGui_Scroll.extent, "-122 103");

  GlassSettings.update("Live::FriendsWindow_Pos", GlassFriendsWindow.position);
  GlassSettings.update("Live::FriendsWindow_Ext", GlassFriendsWindow.extent);

  GlassFriendsWindow.makeFirstResponder(1);
}

//====
// List
//====

function GlassLive::createFriendList() {
  GlassFriendsGui_ScrollSwatch.deleteAll();

  if(getWordCount(trim(GlassLive.friendRequestList)) > 0) {
    %txt = "Friend Requests";
    if(GlassLive.hideFriendRequests) {
      %txt = %txt SPC "\c4(" @ getWordCount(GlassLive.friendRequestList) @ ")";
    }
    %h = GlassLive::createFriendHeader(%txt, !GlassLive.hideFriendRequests, "131 195 243 255");
    %h.mouse.toggleVar = "GlassLive.hideFriendRequests";
    GlassFriendsGui_ScrollSwatch.add(%h);
    %last = %h;

    if(!GlassLive.hideFriendRequests) {
      for(%i = 0; %i < getWordCount(GlassLive.friendRequestList); %i++) {
        %blid = getWord(GlassLive.friendRequestList, %i);
        %uo = GlassLiveUser::getFromBlid(%blid);

        %gui = GlassLive::createFriendRequest(%uo.username, %blid);
        %gui.placeBelow(%last, 5);

        GlassFriendsGui_ScrollSwatch.add(%gui);

        %last = %gui;
      }
    }
  }

  // friends

  // get counts

  %onlineCt = 0;
  if(GlassSettings.get("Live::ShowFriendOnlineCount")) {
    %friendCt = 0;
    if(getWordCount(trim(GlassLive.friendList)) > 0) {
      %sorted = GlassLive::sortFriendList(GlassLive.friendList);

      for(%i = 0; %i < getWordCount(%sorted); %i++) {
        %blid = getWord(%sorted, %i);
        %uo = GlassLiveUser::getFromBlid(%blid);
        %friendCt++;
        if(%uo.getStatus() !$= "offline")
          %onlineCt++;
      }
    }
    %headerText = "Friends (" @ %onlineCt @ "/" @ %friendCt @")";
  } else {
    %headerText = "Friends";
  }

  %h = GlassLive::createFriendHeader(%headerText, !GlassLive.hideFriends, "84 217 140 255");
  %h.mouse.toggleVar = "GlassLive.hideFriends";

  if(getWordCount(trim(GlassLive.friendRequestList)) > 0) {
    %h.placeBelow(%last, 10);
  }

  %last = %h;

  GlassFriendsGui_ScrollSwatch.add(%h);

  if(!GlassLive.hideFriends) {

    if(getWordCount(trim(GlassLive.friendList)) > 0) {
      %sorted = GlassLive::sortFriendList(GlassLive.friendList);

      for(%i = 0; %i < getWordCount(%sorted); %i++) {
        %blid = getWord(%sorted, %i);
        %uo = GlassLiveUser::getFromBlid(%blid);

        %gui = GlassLive::createFriendSwatch(%uo);
        %gui.placeBelow(%last, 5);

        GlassFriendsGui_ScrollSwatch.add(%gui);

        %last = %gui;
      }
    }
  }

  if(getWordCount(trim(GlassLive.blockedList)) > 0) {
    %h = GlassLive::createFriendHeader("Blocked", !GlassLive.hideBlocked, "237 118 105 255");
    %h.mouse.toggleVar = "GlassLive.hideBlocked";

    %h.placeBelow(%last, 10);

    %last = %h;

    GlassFriendsGui_ScrollSwatch.add(%h);

    if(!GlassLive.hideBlocked) {
      for(%i = 0; %i < getWordCount(GlassLive.blockedList); %i++) {
        %blid = getWord(GlassLive.blockedList, %i);
        %uo = GlassLiveUser::getFromBlid(%blid);

        %gui = GlassLive::createBlockedSwatch(%uo.username, %blid);
        %gui.placeBelow(%last, 5);

        GlassFriendsGui_ScrollSwatch.add(%gui);

        %last = %gui;
      }
    }
  }

  GlassFriendsGui_ScrollSwatch.verticalMatchChildren(0, 5);
  GlassFriendsGui_ScrollSwatch.setVisible(true);
  GlassFriendsGui_ScrollSwatch.getGroup().setVisible(true);
}

function GlassLive::sortFriendList(%list) {
  %online = new GuiTextListCtrl();
  %offline = new GuiTextListCtrl();

  for(%i = 0; %i < getWordCount(%list); %i++) {
    %blid = getWord(%list, %i);
    %uo = GlassLiveUser::getFromBlid(%blid);

	%status = %uo.getStatus();
	if(%status $= "Offline")
		%offline.addRow(%blid, %uo.username);
	else
		%online.addRow(%blid, %uo.username);

  }
  %online.sort(0, true);
  %offline.sort(0, true);

  %newList = "";
  for(%i = 0; %i < %online.rowCount(); %i++) {
    %newList = %newList SPC %online.getRowId(%i);
  }
  %online.delete();

  for(%i = 0; %i < %offline.rowCount(); %i++) {
    %newList = %newList SPC %offline.getRowId(%i);
  }
  %offline.delete();

  %newList = getSubStr(%newList, 1, strLen(%newList)-1);
  return %newList;
}

function GlassLive::createFriendHeader(%name, %isOpen, %color) {
  %gui = new GuiSwatchCtrl() {
    extent = "220 26";
    position = "5 5";
    color = %color;
    hcolor = %color;
  };

  %gui.text = new GuiTextCtrl() {
    profile = "GlassFriendTextProfile";
    text = %name;
    extent = "190 18";
    position = "10 10";
  };

  %gui.bullet = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = "199 5";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ (%isOpen ? "bullet_arrow_down.png" : "bullet_arrow_right.png");
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

    type = "toggle";

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

  %gui.add(%gui.text);
  %gui.add(%gui.bullet);
  %gui.add(%gui.mouse);

  //%gui.text.forceCenter();
  %gui.text.centerY();

  return %gui;
}

function GlassLive::createFriendSwatch(%uo, %name, %blid, %status) {
  %name = %uo.username;
  %blid = %uo.blid;
  %status = %uo.status;

  if(%name $= "")
    %name = "Blockhead" @ %blid;

  %height = GlassSettings.get("Live::ShowFriendLocationList") ? 38 : 26;

  if(%status $= "online") {
    %color = "210 220 255 255";
    %hcolor = "230 240 255 255";
  } else if(%status $= "away") {
    %color = "255 244 210 255";
    %hcolor = "255 255 230 255";
  } else if(%status $= "busy") {
    %color = "255 210 210 255";
    %hcolor = "255 230 230 255";
  } else {
    %color = "210 210 210 200";
    %hcolor = "230 230 230 255";

    %height = 26;
  }

  %online = (%status $= "offline" ? false : true);

  %icon = GlassLiveUser::getFromBlid(%blid).icon;
  if(%icon $= "")
    %icon = "ask_and_answer";

  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "210" SPC %height;
    position = "10 5";
    color = %color;
    hcolor = %hcolor;
  };

  %displayName = %name;
  if(GlassSettings.get("Live::DisplayFriendIDs"))
    %displayName = %displayName SPC "(" @ %blid @ ")";

  %gui.text = new GuiTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = (%status !$= "offline") ? "GlassFriendTextProfile" : "GlassFriendTextOfflineProfile";
    text = %displayName;
    extent = "31 18";
    position = "25 4";
  };

  if(%status !$= "offline" && GlassSettings.get("Live::ShowFriendLocationList")) {
    %loc = %uo.getLocation();
    switch$(%loc) {
      case "menus":
        %text = "Main Menu";

      case "playing":
        %text = getASCIIString(%uo.getServerTitle());

      case "playing_lan":
        %text = "LAN Server";

      case "hosting":
        %text = getASCIIString(%uo.getServerTitle());

      case "hosting_lan":
        %text = "LAN Server";

      case "singleplayer":
        %text = "Singleplayer";

      case "private":
        %text = "Location Private";

      default:
        if(strlen(trim(%loc)) > 0)
          %text = "(Unknown: " @ trim(%loc) @ ")";
        else
          %text = "(Unknown)";
    }
    %gui.details = new GuiTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      profile = "GuiTextVerdanaProfile";
      text = "\c2" @ %text;
      extent = "31 18";
      position = "25 18";
    };
  }

  %gui.icon = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = "5 5";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ ".png";
    mcolor = (%status !$= "offline") ? "255 255 255 255" : "150 150 150 70";
    mMultiply = 0;
  };

  %gui.chaticon = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = %gui.extent-22 SPC 5;
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/comment.png";
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

    username = %name;
    blid = %blid;
    online = %online;
    status = %status;

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

  %gui.add(%gui.text);
  if(isObject(%gui.details))
    %gui.add(%gui.details);
  %gui.add(%gui.icon);
  %gui.add(%gui.chaticon);
  %gui.add(%gui.mouse);

  return %gui;
}

function GlassLive::createFriendRequest(%name, %blid) {
  %color = "210 210 210 255";
  %hcolor = "230 230 230 255";

  if(%name $= "")
    %name = "Blockhead" @ %blid;

  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "210 26";
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
    position = "10 10";
  };

  // %gui.icon = new GuiBitmapCtrl() {
    // horizSizing = "right";
    // vertSizing = "bottom";
    // extent = "16 16";
    // position = "5 5";
    // bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ GlassLiveUser::getFromBlid(%blid).icon;
  // };

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

    username = %name;
    blid = %blid;
    type = "request";

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

  %gui.add(%gui.text);
  // %gui.add(%gui.icon);
  %gui.add(%gui.decline);
  %gui.add(%gui.accept);
  %gui.add(%gui.mouse);

  %gui.text.centerY();
  // %gui.icon.centerY();

  return %gui;
}

function GlassLive::createBlockedSwatch(%name, %blid) {
  %color = "210 210 210 255";
  %hcolor = "230 230 230 255";

  if(%name $= "")
    %name = "Blockhead" @ %blid;

  %gui = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "210 26";
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
    position = "10 10";
  };

  %gui.unblock = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    extent = "16 16";
    position = %gui.extent-22 SPC 5;
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/delete.png";
    visible = "0";
  };

  %gui.mouse = new GuiMouseEventCtrl(GlassHighlightMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    extent = %gui.extent;
    position = "0 0";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";

    username = %name;
    blid = %blid;
    type = "blocked";

    hoverCommand = "GlassLive::friendListHover";
    exitCommand = "GlassLive::friendListExit";
    command = "GlassLive::friendListClick";
  };
  %gui.glassHighlight = %gui.mouse;

  %gui.add(%gui.text);
  %gui.add(%gui.unblock);
  %gui.add(%gui.mouse);

  %gui.text.centerY();

  return %gui;
}

function GlassLive::friendListHover(%swatch) {
  %this = %swatch.glassHighlight;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(true);
    %this.getGroup().accept.setVisible(true);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(true);
  } else if(%this.online) {
    if(%this.status $= "busy") {
      %this.getGroup().chaticon.setVisible(false);
    } else {
      %this.getGroup().chaticon.setVisible(true);
    }
  }

  if(getWord(%this.getGroup().text.extent, 0) > getWord(vectorSub(%this.extent, %this.pos), 0)-20)
    if(%this.scrollTick $= "")
      %this.scrollTick = %this.scrollLoop(%this.getGroup().text, true);
}

function GlassLive::friendListExit(%swatch) {
  %this = %swatch.glassHighlight;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(false);
    %this.getGroup().accept.setVisible(false);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(false);
  } else if(%this.online && %this.status !$= "busy") {
    %this.getGroup().chaticon.setVisible(false);
  }

  %this.scrollEnd(%this.getGroup().text);
}

function GlassLive::friendListClick(%swatch, %pos) {
  %this = %swatch.glassHighlight;

  switch$(%this.type) {
    case "request":
      if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
        glassMessageBoxOk("Friend Declined", "<font:verdana bold:13>" @ %this.username SPC " (" @ %this.blid @ ") <font:verdana:13>has been declined.");
        GlassLive::friendDecline(%this.blid);
      } else if(getWord(%pos, 0) > getWord(%this.extent, 0)-50) {
        glassMessageBoxOk("Friend Added", "<font:verdana bold:13>" @ %this.username SPC " (" @ %this.blid @ ") <font:verdana:13>has been added.");
        GlassLive::friendAccept(%this.blid);
      } else {
        if(isObject(%window = GlassLiveUser::getFromBlid(%this.blid).window))
          %window.delete();
        else
          GlassLive::openUserWindow(%this.blid);
      }

    case "blocked":
      if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
        glassMessageBoxOk("Unblocked", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>(" @ %this.blid @ ") has been unblocked.");
        GlassLive::userUnblock(%this.blid);
      } else {
        if(isObject(%window = GlassLiveUser::getFromBlid(%this.blid).window))
          %window.delete();
        else
          GlassLive::openUserWindow(%this.blid);
      }

    case "toggle":
        eval(%this.toggleVar @ " = !" @ %this.toggleVar @ ";");
        GlassLive::createFriendList();

    default:
      if(getWord(%pos, 0) > getWord(%this.extent, 0)-25 && %this.online) {
        if(%this.status !$= "busy") {
          GlassLive::openDirectMessage(%this.blid);
        }
      } else {
        if(isObject(%window = GlassLiveUser::getFromBlid(%this.blid).window))
          %window.delete();
        else
          GlassLive::openUserWindow(%this.blid);
      }
  }
}

function GlassFriendsGui_StatusSelect::updateStatus() {
  %status = trim(stripMlControlChars(GlassLive_Status.getValue()));
  %selector = GlassFriendsGui_StatusSelect;

  switch$(%status) {
    case "Online":
      %selector.position = "14 65";
    case "Away":
      %selector.position = "14 40";
    case "Busy":
      %selector.position = "14 14";
    default:
      return;
  }
  %selector.setVisible(true);
}

function GlassFriendsGui_StatusSelect::selectStatus(%status) {
  GlassFriendsGui_StatusSelect.setVisible(false);
  switch$(%status) {
    case "Online":
      %color = "84 217 140 100";
      %value = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_online><font:verdana bold:13> Online";
    case "Away":
      %color = "241 196 15 100";
      %value = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_away><font:verdana bold:13> Away";
    case "Busy":
      %color = "231 76 60 100";
      %value = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_busy><font:verdana bold:13> Busy";
    default:
      return;
  }
  GlassLive_Status.setValue(%value);
  GlassFriendsGui_InfoSwatch.color = %color;
  GlassLive::setStatus(%status);
}

//====
//
//====

function GlassLive::setFriendStatus(%blid, %status, %force) {
  %uo = GlassLiveUser::getFromBlid(%blid);

  if(!isObject(%uo))
    return;

  if(%status $= %uo.getStatus() && !%force)
    return;

  %uo.setStatus(%status);

  GlassLive::createFriendList();

  if(GlassSettings.get("Live::ShowFriendStatus") && !%uo.isBlocked()) {
    if(%uo.getStatus() $= "online" || %uo.getStatus() $= "offline") {
      %online = (%uo.getStatus() $= "offline" ? false : true);
      %sound = (%online ? "friendOnline" : "friendOffline");

      GlassAudio::play(%sound, GlassSettings.get("Volume::FriendStatus"));
    }

    switch$(%uo.getStatus()) {
      case "online":
        %icon = "status_online";
      case "busy":
        %icon = "status_busy";
      case "away":
        %icon = "status_away";
      case "offline":
        %icon = "status_offline";
      default:
        %icon = "user";
    }

    new ScriptObject(GlassNotification) {
      title = %uo.username;
      text = "is now " @ %uo.getStatus() @ ".";
      image = %icon;

      sticky = false;
      callback = "";
    };

  }
}

function GlassLive::joinFriendServer(%blid) {
	%user = GlassLiveUser::getFromBlid(%blid);
	%server = %user.getServerAddress();

	if(%server $= "") {
		glassMessageBoxOk("Error", "<font:verdana bold:13>" @ %user.username SPC "<font:verdana:13>is not currently playing on a joinable server.");
		return;
	}

  if(%user.isServerPassworded()) {
    $ServerInfo::Address = %server;
    canvas.pushDialog(JoinServerPassGui);
	} else {
	if(isObject(serverConnection))
	  disconnectedCleanup();
    connectToServer(%server, "", "1", "1");
    canvas.pushDialog(connectingGui);
    Connecting_Text.setValue("Connecting to " @ %server @ "...<br>");
  }
}

//====
// Prompts
//====

function GlassLive::addFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);

  if(%user.isBlocked()) {
    glassMessageBoxOk("Blocked", "You have blocked this user, unblock them before attempting to send a friend request.");
    return;
  }

  if(%user)
    if(getNumKeyId() == %blid)
      glassMessageBoxYesNo("Seriously", "Add yourself as a friend?", "GlassLive::sendFriendRequest(" @ %user.blid @ ");");
    else
      glassMessageBoxYesNo("Add Friend", "Add <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> (" @ %user.blid @ ") as a friend?", "GlassLive::sendFriendRequest(" @ %user.blid @ ");");
}

function GlassLive::removeFriendPrompt(%blid) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(%user)
    glassMessageBoxYesNo("Remove Friend", "Remove <font:verdana bold:13>" @ %user.username @ "<font:verdana:13> (" @ %user.blid @ ") as a friend?", "GlassLive::removeFriend(" @ %user.blid @ ");");
}

//====
// Buttons
//====

function GlassLive::powerButtonPress() {
  %btn = GlassFriendsGui_PowerButton;

  if(%btn.on) {
    if(GlassSettings.get("Live::ConfirmConnectDisconnect")) {
      glassMessageBoxYesNo("Disconnect", "Are you sure you want to disconnect from Glass Live?", "GlassLive.noReconnect = true; GlassLive::disconnect(" @ $Glass::Disconnect["Manual"] @ ");");
    } else {
      GlassLive.noReconnect = true;
      GlassLive::disconnect($Glass::Disconnect["Manual"]);
    }
  } else {
    GlassLive.schedule(0, connectToServer);
  }
}

function GlassLive::setPowerButton(%bool) {
  %btn = GlassFriendsGui_PowerButton;
  %btn.on = %bool;
  if(%btn.on)
    %btn.mColor = "84 217 140 150";
  else
    %btn.mColor = "237 118 105 150";
}

function GlassLive::openAddDlg() {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to add friends.");
    return;
  }

  if(GlassFriendsGui_BlockUserBLID.getGroup().visible)
    return;

  %gui = GlassFriendsGui_AddFriendBLID.getGroup();

  if(!%gui.visible) {
    %gui.setVisible(true);
    GlassFriendsGui_ScrollOverlay.setVisible(true);
    GlassFriendsGui_AddFriendBLID.schedule(50, makeFirstResponder, true);
  } else {
    GlassLive::addDlgClose();
  }
}

function GlassLive::openBlockDlg() {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to block users.");
    return;
  }

  if(GlassFriendsGui_AddFriendBLID.getGroup().visible)
    return;

  %gui = GlassFriendsGui_BlockUserBLID.getGroup();

  if(!%gui.visible) {
    %gui.setVisible(true);
    GlassFriendsGui_ScrollOverlay.setVisible(true);
    GlassFriendsGui_BlockUserBLID.schedule(50, makeFirstResponder, true);
  } else {
    GlassLive::blockDlgClose();
  }
}

function GlassLive::blockDlgSubmit() {
  if(!GlassFriendsGui_ScrollOverlay.isVisible())
    return;

  if(GlassFriendsGui_BlockUserBLID.getValue()+0 !$= GlassFriendsGui_BlockUserBLID.getValue() || GlassFriendsGui_BlockUserBLID.getValue() < 0) {
    GlassFriendsGui_BlockUserBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  if(GlassFriendsGui_BlockUserBLID.getValue() == getNumKeyId()) {
    GlassFriendsGui_BlockUserBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "You can't block yourself.");
    return;
  }

  GlassLive::userBlock(GlassFriendsGui_BlockUserBLID.getValue());
  GlassFriendsGui_BlockUserBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_BlockUserBLID.setValue("");
}

function GlassLive::blockDlgClose() {
  GlassFriendsGui_BlockUserBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_BlockUserBLID.setValue("");
}

function GlassLive::addDlgSubmit() {
  if(!GlassFriendsGui_ScrollOverlay.isVisible())
    return;

  if(GlassFriendsGui_AddFriendBLID.getValue()+0 !$= GlassFriendsGui_AddFriendBLID.getValue() || GlassFriendsGui_AddFriendBLID.getValue() < 0) {
    GlassFriendsGui_AddFriendBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "That is not a valid Blockland ID!");
    return;
  }

  if(GlassFriendsGui_AddFriendBLID.getValue() == getNumKeyId()) {
    GlassFriendsGui_AddFriendBLID.setValue("");
    glassMessageBoxOk("Invalid BLID", "Blockland Glass is a social platform and as such you are not able to friend yourself at this present time.<br><br>We apologize for the inconvenience, please find some real friends instead.");
    return;
  }

  GlassLive::sendFriendRequest(GlassFriendsGui_AddFriendBLID.getValue());
  GlassFriendsGui_AddFriendBLID.getGroup().setVisible(false);
  GlassFriendsGui_ScrollOverlay.setVisible(false);
  GlassFriendsGui_AddFriendBLID.setValue("");
}

function GlassLive::addDlgClose() {

  if(!GlassFriendsGui_ScrollOverlay.isVisible())
    GlassOverlay::close();
  else
  {
    GlassFriendsGui_AddFriendBLID.getGroup().setVisible(false);
    GlassFriendsGui_ScrollOverlay.setVisible(false);
    GlassFriendsGui_AddFriendBLID.setValue("");
  }
}


//================================================================
//= AFK Checks                                                   =
//================================================================

function GlassLive::afkCheck(%this, %on) {
  if(%on) {
    %this.afkMouseLoop();
    %this.afkPackage = true;
    %this.afkAction();
  } else {
    cancel(%this.afkMouseLoop);
    %this.afkPackage = false;
    cancel(%this.afkTriggerSchedule);
  }
}

function GlassLive::afkAction(%this) {
  cancel(%this.afkTriggerSchedule);

  if(%this.isAFK) {
    GlassFriendsGui_StatusSelect::selectStatus(%this.lastStatus);

    %this.isAFK = false;
  } else {
    if(GlassSettings.get("Live::AFKTime") < 5)
      GlassSettings.update("Live::AFKTime", 5);

    %this.afkTriggerSchedule = %this.schedule((GlassSettings.get("Live::AFKTime") * 60000) | 0, "afkTrigger");
  }
}

function GlassLive::afkTrigger(%this) {
  if(%this.status !$= "") {
    %status = %this.status;

    if(%status !$= "away") {
      %this.lastStatus = %status;

      GlassFriendsGui_StatusSelect::selectStatus("away");

      %this.isAFK = true;
      %this.afkMousePos = Canvas.getCursorPos();
      %this.afkMouseLoop();
    }
  }
}

function GlassLive::afkMouseLoop(%this) {
  cancel(%this.afkMouseLoop);

  if(%this.isAFK && %this.afkMousePos !$= canvas.getCursorPos()) {
    %this.afkAction();
  } else {
    if(%this.isAFK)
      %this.afkMouseLoop = %this.schedule(250, afkMouseLoop);
    else
      %this.afkMouseLoop = %this.schedule(10000, afkMouseLoop);
  }
}

//================================================================
//= Direct Messages                                              =
//================================================================

function GlassLive::openDirectMessage(%blid, %username) {
  if(%blid $= "" || %blid == getNumKeyId())
    return false;

  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to use direct messaging.");
    return;
  }

  %user = GlassLiveUser::getFromBlid(%blid);

  if(!isObject(%user)) {
    glassMessageBoxOk("Error", "Unable to find the requested user on Glass Live.");
    return;
  }

  if(!(%error = %user.canSendMessage())) {
    %error = getField(%error, 1);
    glassMessageBoxOk("Error", %error);
    return;
  }

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
  } else {
    GlassOverlayGui.pushToBack(%gui);
  }

  return %gui;
}

function GlassLive::closeMessage(%blid) {
  %gui = GlassLive.message[%blid];

  if(isObject(GlassLive_EmoteSelection)) {
    if(GlassOverlay.activeInput == %gui.input) {
      GlassLive_EmoteSelection.delete();
    }
  }

  GlassOverlayGui.remove(%gui);
  %gui.deleteAll();
  %gui.delete();

  %obj = JettisonObject();
  %obj.set("type", "string", "messageClose");
  %obj.set("target", "string", %blid);

  GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");


  if(GlassLive.typing[%blid]) {
    GlassLive::messageTypeEnd(%blid);
  }
}

function GlassLive::onMessage(%message, %username, %blid) {
  %timestamp = "[" @ getWord(getDateTime(), 1) @ "]";

  if(GlassSettings.get("Live::MessageLogging")) {
    if(isFile(%file = "config/client/BLG/chat_log/DMs/" @ %blid @ "/" @ strReplace(getWord(getDateTime(), 0), "/", ".") @ ".txt")) {
      %fo = new FileObject();
      %fo.openForAppend(%file);
      %fo.writeLine(%timestamp @ " " @ %username @ ": " @ %message);
      %fo.close();
      %fo.delete();
    } else {
      %fo = new FileObject();
      %fo.openForWrite(%file);
      %fo.writeLine(%timestamp @ " Beginning chat log of " @ GlassLiveUser::getFromBlid(%blid).username @ " (" @ %blid @ ")");
      %fo.writeLine(%timestamp @ " " @ %username @ ": " @ %message);
      %fo.close();
      %fo.delete();
    }
  }

  %gui = GlassLive::openDirectMessage(%blid, %username);

  if(!%gui)
    return;

  GlassOverlayGui.pushToBack(%gui);

  for(%i = 0; %i < getWordCount(%message); %i++) {
    %word = getWord(%message, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0 || strpos(%word, "glass://") == 0) {
      %raw = %word;
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %message = setWord(%message, %i, %word);

      //%obj = getUrlMetadata(%raw, "GlassLive::urlMetadata");
      //%obj.context = "dm";
      //%obj.blid = %blid;
      //%obj.raw = %raw;
    }
    %validEmote = (getsubstr(%word, 0, 1) $= ":" && getsubstr(%word, strlen(%word) - 1, strlen(%word)) $= ":" && getsubstr(%word, 1, 1) !$= ":" && getsubstr(%word, strlen(%word) - 2, 1) !$= ":");
    if(%validEmote) {
      %bitmap = strlwr(stripChars(%word, "[]\\/{};:'\"<>,./?!@#$%^&*-=+`~;"));
      %bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap @ ".png";
      if(isFile(%bitmap)) {
        %word = "<bitmap:" @ %bitmap @ ">";
        %message = setWord(%message, %i, strlwr(%word));
      }
    }
  }

  GlassLive::setMessageTyping(%blid, false);

  %val = %gui.chattext.getValue();

  %msg = "<color:333333><font:verdana bold:12><color:" @ (%username $= $Pref::Player::NetName ? "fc0000" : "0000ff") @ ">" @ %username @ ":<font:verdana:12><color:333333> " @ %message;

  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = "<font:verdana:12><color:666666>" @ %timestamp SPC %msg;
  }

  if(%val !$= "") {
    %val = %val @ "<br>" @ %msg;
  } else {
    %val = %msg;
  }

  %gui.chattext.setValue(%val);

  if(%gui.isAwake()) {
    %gui.chattext.forceReflow();
  }

  %gui.scrollSwatch.verticalMatchChildren(0, 3);
  %gui.scrollSwatch.setVisible(true);

  %lp = %gui.getLowestPoint() - %gui.scroll.getLowestPoint();

  if(%lp >= -15) {
    %gui.scroll.scrollToBottom();
  }

  GlassAudio::play((%username $= $Pref::Player::NetName ? "userMsgSent" : "userMsgReceived"), GlassSettings.get("Volume::DirectMessage"));
}

function GlassLive::onMessageNotification(%message, %blid, %create) {
  // TODO check friend, blocked, prefs, etc

  %user = GlassLiveUser::getFromBlid(%blid);

  if(!isObject(%user))
    return;

  if(!%create) {
    %gui = %user.getMessageGui();
  } else {
    %gui = GlassLive::openDirectMessage(%blid, %user.username);
  }

  if(%gui == false)
    return;

  %timestamp = "[" @ getWord(getDateTime(), 1) @ "]";

  if(GlassSettings.get("Live::MessageLogging")) {
    if(isFile(%file = "config/client/BLG/chat_log/DMs/" @ %blid @ "/" @ strReplace(getWord(getDateTime(), 0), "/", ".") @ ".txt")) {
      %fo = new FileObject();
      %fo.openForAppend(%file);
      %fo.writeLine(%timestamp SPC %message);
      %fo.close();
      %fo.delete();
    }
  }

  %val = %gui.chattext.getValue();

  %msg = "<color:666666><font:verdana:12>" @ %message;

  if(GlassSettings.get("Live::ShowTimestamps")) {
    %msg = "<font:verdana:12><color:666666>" @ %timestamp SPC %msg;
  }

  if(%val !$= "") {
    %val = %val @ "<br>" @ %msg;
  } else {
    %val = %msg;
  }

  %gui.chattext.setValue(%val);
  if(%gui.isAwake()) {
    %gui.chattext.forceReflow();
  }
  %gui.scrollSwatch.verticalMatchChildren(0, 3);
  %gui.scrollSwatch.setVisible(true);

  %lp = %gui.getLowestPoint() - %gui.scroll.getLowestPoint();

  if(%lp >= -15) {
    %gui.scroll.scrollToBottom();
  }
}

function GlassLive::setMessageTyping(%blid, %typing) {
  %user = GlassLiveUser::getFromBlid(%blid);
  if(isObject(%user)) {
    %window = %user.getMessageGui();
    if(isObject(%window)) {
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

//================================================================
//= Direct Messages GUI                                          =
//================================================================

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
    text = %username @ " (" @ %blid @ ")";
    maxLength = "255";
    resizeWidth = "1";
    resizeHeight = "1";
    canMove = "1";
    canClose = "1";
    canMinimize = "0";
    canMaximize = "0";
    closeCommand = "GlassLive::closeMessage(" @ %blid @ ");";
  };

  %titleLen = strLen(%dm.text);

  if(%titleLen > 25) {
    %dm.extent = mFloor(%titleLen * 11.25 SPC 180); // close enough
    %dm.minExtent = %dm.extent;
  }

  %dm.resize = new GuiMLTextCtrl(GlassMessageResize) {
    profile = "GuiMLTextProfile";
    horizSizing = "relative";
    vertSizing = "relative";
    position = "0 0";
    extent = %dm.extent;
    visible = "0";
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
    maxBitmapHeight = "12";
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
    mcolor = "255 255 255 64";
  };

  %dm.input = new GuiTextEditCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "top";
    position = "10 155";
    extent = "220 16";
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
  %dm.input.command = %dm.input.command @ "GlassEmoteSelector::ListEmotes(" @ %dm.input.getID() @ ");";

  %dm.userButton = new GuiBitmapCtrl() {
    position = getWord(%dm.extent, 0) - 24 SPC "155";
    extent = "16 16";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/user";
    new GuiButtonBaseCtrl()
    {
      extent = "16 16";
      command = "if(isObject(" @ GlassLiveUser::getFromBlid(%blid) @ ".window)){" @ GlassLiveUser::getFromBlid(%blid) @ ".window.forceCenter();}GlassLive::openUserWindow(" @ %blid @ ");alxPlay(GlassClick1Audio);";
    };
  };

  %dm.add(%dm.resize);
  %dm.add(%dm.scroll);
  %dm.scroll.add(%dm.scrollSwatch);
  %dm.scrollSwatch.add(%dm.chattext);
  %dm.scrollSwatch.add(%dm.typing);
  %dm.add(%dm.input);
  %dm.add(%dm.userButton);

  %dm.scrollSwatch.verticalMatchChildren(0, 3);

  %dm.resize.onResize(getWord(%dm.position, 0), getWord(%dm.position, 1), getWord(%dm.extent, 0), getWord(%dm.extent, 1));

  return %dm;
}

function GlassLive::messageInputSend(%id) {
  %gui = GlassLive.message[%id];
  %val = trim(%gui.input.getValue());
  %val = stripMlControlChars(%val);
  if(%val $= "") {
    %gui.input.setValue("");
    return;
  }

  GlassLive::sendMessage(%id, %val);
  if(GlassSettings.get("Live::TalkingAnimation"))
    GlassLive::BlockheadAnim("talk", strLen(%msg) * 50);
  GlassLive::onMessage(%val, $Pref::Player::NetName, %id);
  %gui.input.setValue("");
  %gui.input.makeFirstResponder(1);
  %gui.scroll.schedule(100, "scrollToBottom");
}

function GlassMessageResize::onResize(%this, %x, %y, %h, %l) {
  %window = %this.getGroup();
  %extent = %window.extent;
  %window.scroll.extent = vectorSub(%extent, "20 65");
  %window.scrollSwatch.extent = getWord(%extent, 0) - 30 SPC getWord(%window.chattext.extent, 1);
  %window.chattext.extent = getWord(%extent, 0) - 35 SPC getWord(%window.chattext.extent, 1);

  %window.input.extent = getWord(%extent, 0) - 38 SPC getWord(%window.input.extent, 1);
  %window.userButton.position = getWord(%window.extent, 0) - 24 SPC getWord(%window.input.position, 1);

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scroll.setVisible(true);

  if(isObject(GlassLive_EmoteSelection)) {
    if(GlassOverlay.activeInput == %window.input) {
      GlassLive_EmoteSelection.position = vectorAdd(%window.position, 2 SPC getWord(%window.extent, 1));
    }
  }
}

function GlassMessageTyping::startAnimation(%this) {
  %window = %this.window;
  %this.placeBelow(%window.chattext, 4);

  %window.scrollSwatch.verticalMatchChildren(0, 3);
  %window.scrollSwatch.setVisible(true);
  // %window.scroll.scrollToBottom();

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
  // %window.scroll.scrollToBottom();
}


//================================================================
//= Scroll?                                                      =
//================================================================

function GlassHighlightMouse::scrollLoop(%this, %text, %reset) {
  %icon = %text.getGroup().icon;
  %buttonChat = %text.getGroup().buttonChat;
  %unblock = %text.getGroup().unblock;

  if(%reset) {
    %this._scrollOrigin = %text.position;
    if(isObject(%icon))
      %this._scrollOrigin_Icon = %icon.position;
    %this._scrollOffset = 0;
    if(isObject(%unblock) || isObject(%buttonChat))
      %this._scrollRange = getWord(%text.extent, 0)-getWord(%this.extent, 0)+getWord(%text.position, 0)+25;
    else
      %this._scrollRange = getWord(%text.extent, 0)-getWord(%this.extent, 0)+getWord(%text.position, 0)+50;
  }

  %text.position = vectorSub(%this._scrollOrigin, %this._scrollOffset);
  if(isObject(%icon))
    %icon.position = vectorSub(%this._scrollOrigin_Icon, %this._scrollOffset);

  if(%this._scrollOffset >= %this._scrollRange) {
    %this._scrollOffset = 0;
    // %this.scrollTick = %this.schedule(2000, scrollLoop, %text);
  } else {
    %this._scrollOffset++;
    %this.scrollTick = %this.schedule(25, scrollLoop, %text);
  }
}

function GlassHighlightMouse::scrollEnd(%this, %text) {
  cancel(%this.scrollTick);
  %text.position = %this._scrollOrigin;

  %icon = %text.getGroup().icon;
  if(isObject(%icon))
    %icon.position = %this._scrollOrigin_Icon;

  %this.scrollTick = "";
}

//================================================================
//= User Window                                                  =
//================================================================

function GlassLive::createUserWindow(%uo) {
   if(isObject(%uo.window)) {
    GlassOverlayGui.pushToBack(%uo.window);
    //%uo.window.delete();
    return %uo.window;
  }

  if(%uo.isAdmin())
    %verified = "Glass Administrator";
  else if(%uo.isMod())
    %verified = "Glass Moderator";
  else
    %verified = "Glass User";

  %window = new GuiWindowCtrl() {
    profile = "GlassWindowProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "265 157";
    extent = "340 260";
    text = %verified;
    maxLength = "255";
    resizeWidth = "0";
    resizeHeight = "0";
    canMinimize = "0";
    canMaximize = "0";
  };
  %window.uo = %uo;

  %window.infoSwatch = new GuiSwatchCtrl() {
  	color = "235 235 235 255";
  	position = "10 35";
  	extent = "100 215";
  };

  %window.blockheadSwatch = new GuiSwatchCtrl() {
  	color = "235 0 0 0";
    position = "0 40";
    extent = "100 210";
  };

  %window.blockhead = new GuiObjectView(GlassUserGui_Blockhead) {
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
  	extent = "100 210";
  	forceFov = 18;
  	lightDirection = "-1 -1 -1";
    firstOpen = true;
  };

  %window.privateAvatar = new GuiBitmapCtrl(GlassUserGui_PrivateAvatar) {
    position = "6 35";
    extent = "84 84";
	  bitmap = "Add-Ons/Print_Letters_Default/prints/-qmark.png";
    mColor = "255 255 255 255";
    mMultiply = "0";
    visible = "0";
  };

  %window.headerSwatch = new GuiSwatchCtrl() {
    color = "235 235 235 255";
  	position = "120 35";
  	extent = "210 72";
  };

  %window.headerText = new GuiMLTextCtrl() {
  	position = "10 5";
  	extent = "190 62";
  	text = "";
    autoResize = true;
  };

  %window.statusSwatch = new GuiBitmapButtonCtrl() {
  	profile = "GlassBlockButtonProfile";
  	bitmap = "Add-Ons/System_BlocklandGlass/image/gui/tab1";
  	text = "";
  	mColor = "255 255 255 255";
  	position = "22 10";
  	extent = "70 22";
    enabled = "0";
  };

  %window.statusText = new GuiMLTextCtrl() {
  	position = "2 3";
  	extent = "65 16";
  	minExtent = "8 2";
  	text = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_online><font:verdana bold:13> Online";
  };

  %window.messageButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 116";
    extent = "210 30";
    text = "Message";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "255 255 255 200";
    command = "GlassLive::openDirectMessage(" @ %uo.blid @ "); if(isObject(" @ GlassLiveUser::getFromBlid(%uo.blid) @ ".getMessageGui())){" @ GlassLiveUser::getFromBlid(%uo.blid) @ ".getMessageGui().forceCenter();}"; // ech
  };

  %window.friendButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 151";
    extent = "210 30";
    text = "Unfriend";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "255 200 200 200";
  };

  %window.blockButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 186";
    extent = "210 30";
    text = "Block";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = "237 118 105 200";
  };

  %invitable = %uo.getStatus !$= "offline" && %uo.blid != getNumKeyId() && %uo.isFriend() && isObject(ServerConnection);

  %window.inviteButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "120 221";
    extent = "102 30";
    text = "Invite";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = %invitable ? "85 172 238 200" : "200 200 200 200";
    enabled = %invitable;
    command = "GlassLive::inviteFriend(" @ (%uo.blid+0) @ ");";
  };

  %joinable = %uo.getStatus !$= "offline" && %uo.blid != getNumKeyId() && %uo.isFriend() && (%uo.getLocation() $= "playing" || %uo.getLocation $= "hosting");

  %window.joinButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    position = "228 221";
    extent = "102 30";
    text = "Join";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    mColor = %joinable ? "46 204 113 200" : "200 200 200 200";
    enabled = %joinable;
    command = "glassMessageBoxYesNo(\"Join\", \"Would you like to join the server <font:verdana bold:13>" @ %uo.username @ "<font:verdana:13> is on?\", \"GlassLive::joinFriendServer(" @ %uo.blid @ ");\");";
  };

  %window.add(%window.infoSwatch);
  %window.infoSwatch.add(%window.blockheadSwatch);
  %window.blockheadSwatch.add(%window.blockhead);
  %window.blockheadSwatch.add(%window.privateAvatar);

  %window.infoSwatch.add(%window.statusSwatch);
  %window.statusSwatch.add(%window.statusText);
  %window.statusSwatch.centerX();

  %window.add(%window.headerSwatch);
  %window.headerSwatch.add(%window.headerText);

  %window.add(%window.messageButton);
  %window.add(%window.friendButton);
  %window.add(%window.blockButton);
  %window.add(%window.inviteButton);
  %window.add(%window.joinButton);

  %window.closeCommand = %window.getId() @ ".delete();";

  GlassOverlayGui.add(%window);

  %window.setName("GlassUserGui");
  %uo.window = %window;
  return %window;
}

function GlassLive::openUserWindow(%blid, %didUpdate) {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to open user windows.");
    return;
  }

  if(%blid < 0) {
    glassMessageBoxOk("Beep Boop", "That's a bot!");
    return;
  }

  %uo = GlassLiveUser::getFromBlid(%blid);
  if(%uo) {
    if(!%didUpdate) {
      %uo.requestLocation();
    }

    %window = GlassLive::createUserWindow(%uo);

    %status = %uo.status;

    if(%status $= "")
      %status = "offline";

    %statusText = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/status_" @ %status @ "><font:verdana bold:13> " @ strCap(%status);

    switch$(%status) {
      case "online":
        %window.statusText.position = "2 4";
      case "away":
        %window.statusText.position = "6 4";
      case "busy":
        %window.statusText.position = "8 4";
      default:
        %window.statusText.position = "2 4";
    }

    %location = %uo.getLocation();
    switch$(%location) {
      case "menus":
        //%locationColor = "AFAFAF";
        %locationRGB = "235 235 235 255";
        %locationDisplay = "<br>At the Main Menu";

      case "playing":
        //%locationColor = "2ECC71";
        %locationRGB = "46 204 113 100";
        %locationDisplay = "Playing";

      case "playing_lan":
        //%locationColor = "2ECC71";
        %locationRGB = "241 196 15 100";
        %locationDisplay = "<br>Playing a LAN server";

      case "hosting":
        //%locationColor = "55ACEE";
        %locationRGB = "85 172 238 100";
        %locationDisplay = "Hosting";

      case "hosting_lan":
        //%locationColor = "55ACEE";
        %locationRGB = "241 196 15 100";
        %locationDisplay = "<br>Hosting a LAN server";

      case "singleplayer":
        //%locationColor = "E74C3C";
        %locationRGB = "241 196 15 100";
        %locationDisplay = "<br>Playing Singleplayer";

      case "private":
        //%locationColor = "AFAFAF";
        %locationRGB = "235 235 235 255";
        %locationDisplay = "<br>Location Private";

      default:
        //%locationColor = "AFAFAF";
        %locationRGB = "235 235 235 255";
        %locationDisplay = "<br>Unknown";
    }
    %locationColor = "333333";

    if(%uo.blid == getNumKeyId()) {
      // ea.st.er eg.gs shouldn't remove, hide or overwrite information
      // add them elsewhere
      // %locationDisplay = "<br>Chillin' like a Villain";
      %serverTitle = "";
    }

  	%window.statusText.setText(%statusText);

    %serverTitle = %uo.getServerTitle();
    // if(strlen(%serverTitle = %uo.getServerTitle()) > 32)
      // %serverTitle = getsubstr(%serverTitle, 0, 28) @ "...";

    %br = "<br>";
    if(strLen(%serverTitle) > 32)
      %br = " - ";

    %serverInfo = "<br><br><color:" @ %locationColor @ ">" @ %locationDisplay @ %br @ "<font:verdana bold:16>" @ getASCIIString(%serverTitle);

    if(%uo.online && %uo.country !$= "") {
      if(%uo.country $= "United States") {
        %country = "usa";
      } else if(%uo.country $= "United Kingdom") {
        %country = "great_britain";
      } else if(%uo.country $= "Russian Federation") {
        %country = "russia";
      } else {
        %country = strReplace(%uo.country, " ", "_");
      }

      %file = "Add-Ons/System_BlocklandGlass/image/icon/flag_" @ %country @ ".png";
      if(isFile(%file)) {
        %countryFlag = "<just:right><bitmap:" @ %file @ ">";
      } else {
        %countryFlag = "<just:right><font:verdana:12><sPush><color:444444>" @ %uo.country @ "<sPop>";
      }
    } else {

    }

    %window.headerText.setText("<font:verdana bold:14>" @ %uo.username @ %countryFlag @ "<br><just:left><font:verdana:12>" @ %uo.blid @ %serverInfo);
    %window.headerSwatch.color = %locationRGB;

    %uo.getAvatar(%window.blockhead);

    if(%uo.isFriend()) {
      %window.friendButton.mcolor = "255 200 200 200";
      %window.friendButton.command = "GlassLive::removeFriendPrompt(" @ %uo.blid @ ");";
      %window.friendButton.text = "Unfriend";
    } else {
      %window.friendButton.mcolor = "200 255 200 200";
      %window.friendButton.command = "GlassLive::addFriendPrompt(" @ %uo.blid @ ");";
      %window.friendButton.text = "Add Friend";
    }

    if(%uo.isBlocked()) {
      %window.blockButton.mcolor = "237 184 105 200";
      %window.blockButton.command = "GlassLive::userUnblock(" @ %uo.blid @ ");";
      %window.blockButton.text = "Unblock";
    } else {
      %window.blockButton.mcolor = "237 118 105 200";
      %window.blockButton.command = "GlassLive::userBlock(" @ %uo.blid @ ");";
      %window.blockButton.text = "Block";
    }

    %window.messageButton.enabled = true;

    if(!%window.centered) {
      %window.forceCenter();
      %window.centered = true;
    }
  } else {
    glassMessageBoxOk("Error", "Unable to find the requested user on Glass Live.");
  }
}

//================================================================
//= Moderation Gui                                               =
//================================================================
// GlassLiveUsers
function GlassCheckModeratorButton() {
  if(%uo = GlassLiveUser::getFromBlid(getNumKeyId())) {
    if(%uo.isMod()) {
  	  GlassLiveModerationButton.setVisible(true);
    }
  }
}

function GlassOverlay::closeModeration() {
  GlassModeratorWindow.setVisible(false);
}

function GlassModeratorGui::searchPlayers(%search) {
  %this = GlassModeratorGui;
  %text = GlassModeratorWindow_Search;

  if(%this.searchFiller) {
    if(strlen(%search) < 6) {
      %this.searchFiller = true;
      %text.setValue("Search");
      %search = "";
    } else {
      %this.searchFiller = false;
      %text.setValue(getSubStr(%search, %text.getCursorPos()-1, 1));
      %search = %text.getValue();
    }
  } else if(strlen(%search) == 0) {
    %this.searchFiller = true;
    %text.setValue("Search");
    %search = "";
  }

  GlassModeratorWindow_Playerlist.clear();

  for(%i = 0; %i < GlassLiveUsers.getCount(); %i++) {
  	%user = GlassLiveUsers.getObject(%i);
  	if(strStr(strLwr(%user.username), strLwr(%search)) >= 0 || strStr(%user.blid, %search) >= 0)
      if(%user.blid > 0)
        GlassModeratorWindow_Playerlist.addRow(%i, %user.username TAB %user.blid);
  }
  GlassModeratorWindow_Playerlist.sort(0, 1);
}


function GlassModeratorGui::refreshPlayerlist() {
  GlassModeratorWindow_Playerlist.clear();

  %room = GlassLiveRooms::getFromId(0);
  for(%i = 0; %i < %room.getCount(); %i++) {
    %user = %room.getUser(%i);
    if(%user.blid < 0)
      continue;
    GlassModeratorWindow_Playerlist.addRow(%i, %user.username TAB %user.blid);
  }

}

function GlassModeratorWindow_Selection::onSelect(%this) {
  %selection = %this.getValue();
  GlassModeratorWindow.selection = %selection;
  GlassModeratorWindow_Reason.setText("");
  GlassModeratorWindow_Duration.setText("");
  if(%selection $= "Ban" || %selection $= "Bar") {
  	GlassModeratorWindow_ReasonBlocker.setVisible(false);
  	GlassModeratorWindow_DurationBlocker.setVisible(false);
    GlassModeratorWindow_Reason.enabled = true;
    GlassModeratorWindow_Duration.enabled = true;
  } else if(%selection $= "Kick") {
  	GlassModeratorWindow_ReasonBlocker.setVisible(true);
  	GlassModeratorWindow_DurationBlocker.setVisible(true);
    GlassModeratorWindow_Reason.enabled = false;
    GlassModeratorWindow_Duration.enabled = false;
  } else if(%selection $= "Mute") {
  	GlassModeratorWindow_ReasonBlocker.setVisible(true);
  	GlassModeratorWindow_DurationBlocker.setVisible(false);
    GlassModeratorWindow_Reason.enabled = false;
    GlassModeratorWindow_Duration.enabled = true;
  }
  GlassModeratorWindow.selection = %selection;
  GlassModeratorGui.updateDuration();
}

function GlassModeratorWindow_Playerlist::onSelect(%this, %rowID, %rowText) {
  %blid = getField(%rowText, 1);
  %name = getField(%rowText, 0);

  GlassModeratorWindow_BLID.setValue(%blid);
}

function GlassModeratorGui::submit(%this, %confirm) {
  %blid = GlassModeratorWindow_BLID.getValue();
  %name = GlassLiveUsers.user[%blid].username;
  %type = GlassModeratorWindow_Selection.getValue();
  %duration = trim(GlassModeratorWindow_Duration.getValue());
  %reason = trim(GlassModeratorWindow_Reason.getValue());

  if(stripChars(%blid, "-0123456789") !$= "" || %blid < 1 || %blid > 999999) {
    glassMessageBoxOk("Error", "You must enter a valid BL_ID.");
    return;
  }

  if(%type $= "Bar" || %type $= "Ban") {
    if(%reason $= "") {
      glassMessageBoxOk("Error", "You must enter a reason.");
      return;
    }
  }

  if(%type !$= "Kick" && stripChars(%duration, "-0123456789") !$= "" && %duration >= -1 && %duration != 0) {
    glassMessageBoxOk("Error", "You must enter a valid duration.");
    return;
  }

  if(GlassChatroomWindow.activeTab.id $= "") {
    glassMessageBoxOk("Error", "An active Glass chatroom window was not found.<br><br>Please connect to a chatroom before using the moderation GUI.");
    return;
  }

  if(!%confirm) {
    %txt = "<font:verdana bold:20>Confirm " @ %type @ "<br><br>";
    %txt = %txt SPC "<font:verdana:18>You are about to";
    %txt = %txt SPC strlwr(%type);
    if(%name !$= "") {
      %txt = %txt SPC "<font:verdana bold:18>" @ %name @ "<font:verdana:18>";
      %txt = %txt SPC "(" @ %blid @ ")";
    } else {
      %txt = %txt SPC "<font:verdana bold:18>BL_ID: " @ %blid @ "<font:verdana:18>";
    }
    if(%type $= "Ban" || %type $= "Bar")
      %txt = %txt SPC "with reason \"" @ trim(%reason) @ "\"";
    if(%type !$= "Kick") {
      if(%duration == -1)
        %durTime = secondsToTimeString(%duration);
      else if(%type !$= "Mute")
        %durTime = secondsToTimeString(%duration * 60);
      else
        %durTime = secondsToTimeString(%duration);

      %txt = %txt SPC "for" SPC %durTime @ ".";
    }
    %txt = %txt SPC "<br><br><font:verdana:14>";
    if(%type $= "Bar")
      %txt = %txt SPC "The user will not be able to use any online Glass services for the duration specified.<br><br><color:FF0000><font:verdana bold:14>Consider this action carefully.";
    else if(%type $= "Ban")
      %txt = %txt SPC "The user will not be able to connect to any Glass chatrooms for the duration specified.<br><br>Access to their friends list and DMing will remain available.";
    else if(%type $= "Kick")
      %txt = %txt SPC "The user will be kicked from the active Glass chatroom you are in.";
    else if(%type $= "Mute")
      %txt = %txt SPC "The user will be muted in all Glass chatrooms for the duration specified.";

    glassMessageBoxOkCancel("Wait", %txt, "GlassModeratorGui.submit(true);");
    return;
  }

  if(%type $= "Kick")
    GlassLive::sendRoomCommand(%cmd = "/kickid" SPC %blid, GlassChatroomWindow.activeTab.id);
  else
    GlassLive::sendRoomCommand(%cmd = "/" @ strlwr(%type) @ "id" SPC %duration SPC %blid SPC %reason, GlassChatroomWindow.activeTab.id);

  Glass::debug("Sent room command:" SPC %cmd);
}

function GlassModeratorGui::addDuration(%this, %seconds) {
  if(GlassModeratorWindow_Selection.getValue() $= "Kick" || GlassModeratorWindow_Selection.getValue() $= "Select")
    return;

  if(GlassModeratorWindow_Selection.getValue() !$= "Mute")
    %seconds /= 60;

  %current = trim(GlassModeratorWindow_Duration.getValue());

  if(%current == -1)
    %current = 0;
  GlassModeratorWindow_Duration.setValue((%current + %seconds) | 0);

  GlassModeratorGui.updateDuration();
}

function GlassModeratorGui::updateDuration(%this) {
  if(GlassModeratorWindow_Selection.getValue() $= "Kick" || GlassModeratorWindow_Selection.getValue() $= "Select") {
    GlassModeratorWindow_CalcDuration.setValue("<font:verdana:13><just:center>Duration Not Available");
    return;
  }

  %duration = GlassModeratorWindow_Duration.getValue();

  if(%duration $= "") {
    GlassModeratorWindow_CalcDuration.setValue("<font:verdana:13><just:center>No Duration Set");
    return;
  }

  if(%duration == -1) {
    GlassModeratorWindow_CalcDuration.setValue("<font:verdana:13><just:center><color:ff0000>Permanent");
    return;
  }

  if((%duration == 0 || %duration $= "") || %duration < -1) {
    GlassModeratorWindow_CalcDuration.setValue("<font:verdana:13><just:center>Error");
    return;
  }

  if(GlassModeratorWindow_Selection.getValue() !$= "Mute")
    %duration *= 60;

  GlassModeratorWindow_CalcDuration.setValue("<font:verdana:13><just:center>" @ secondsToTimeString(%duration));
}

function GlassModeratorGui::updateBLID(%this) {
  %this.refreshPlayerlist();

  for(%i = 0; %i < GlassModeratorWindow_Playerlist.rowCount(); %i++) {
    %rowText = GlassModeratorWindow_Playerlist.getRowText(%i);
    %blid = getField(%rowText, 1);
    %name = getField(%rowText, 0);

    if(GlassModeratorWindow_BLID.getValue() $= %blid) {
      GlassModeratorWindow_Playerlist.setSelectedRow(%i);
      return;
    }
  }
}

function GlassModeratorWindow_Duration::onUpdate(%this) {
  GlassModeratorGui.updateDuration();
}

//================================================================
//= Chatroom Gui                                                 =
//================================================================

function GlassLive::createChatroomWindow() {
  %chatroom = new GuiWindowCtrl(GlassChatroomWindow) {
    profile = "GlassWindowProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "135 130";
    minExtent = "475 290";
    extent = "699 421";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Phantom Chatroom Window";
    maxLength = "255";
    resizeWidth = "1";
    resizeHeight = "1";
    canMove = "1";
    canClose = "0";
    canMinimize = "0";
    canMaximize = "0";
    minSize = "50 50";
    closeCommand = "%this.exit();";

    tabs = 0;
  };

  %chatroom.resize = new GuiMLTextCtrl(GlassChatroomResize) {
    profile = "GuiMLTextProfile";
    horizSizing = "relative";
    vertSizing = "relative";
    position = "0 0";
    extent = %chatroom.extent;
    visible = "0";
  };

  %chatroom.tabSwatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 35";
    extent = "445 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "0 0 0 0";
  };

  %chatroom.dropText = new GuiMLTextCtrl() {
     profile = "GuiMLTextProfile";
     horizSizing = "center";
     vertSizing = "center";
     position = "0 135";
     extent = "465 12";
     minExtent = "8 2";
     enabled = "1";
     visible = "0";
     clipToParent = "1";
     lineSpacing = "2";
     allowColorChars = "0";
     maxChars = "-1";
     maxBitmapHeight = "-1";
     selectable = "1";
     autoResize = "1";
     text = "<font:verdana:15><just:center>Add to Window";
  };

  %chatroom.add(%chatroom.resize);
  %chatroom.add(%chatroom.tabSwatch);
  %chatroom.add(%chatroom.dropText);
  GlassOverlayGui.add(%chatroom);
  return %chatroom;
}

function GlassLive::createChatroomView(%id) {
  %chatroom = new GuiSwatchCtrl(GlassChatroomTab) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 60";
    extent = "450 215";
    minExtent = "8 2";
    enabled = "1";
    visible = "0";
    clipToParent = "1";
    color = "0 0 0 0";
    id = %id;
  };

  %chatroom.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 0";
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
    id = %id;
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
    maxBitmapHeight = "12";
    selectable = "1";
    autoResize = "1";
  };

  %chatroom.userscroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "330 0";
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

  %chatroom.userswatch = new GuiSwatchCtrl() {
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
    position = "10 205";
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
    tabComplete = "1";
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

function GlassLive::chatroomInputSend(%id) {
  %room = GlassLiveRooms::getFromId(%id);
  if(%room == false) {
    error("Trying to send input in inactive room");
    return;
  }

  %chatroom = %room.view;
  %val = trim(%chatroom.input.getValue());
  %val = stripMlControlChars(%val);
  if(%val $= "") {
    %chatroom.input.setValue("");
    return;
  }

  if(strPos(%val, "/") != 0) {
    GlassLive::sendRoomMessage(%val, %id);
  } else {
    GlassLive::sendRoomCommand(%val, %id);
  }

  %chatroom.input.setValue("");
  %chatroom.scroll.schedule(100, "scrollToBottom");
}

function GlassChatroomWindow::setDropMode(%this, %bool) {
  if(%bool) {
    for(%i = 0; %i < %this.getCount(); %i++) {
      %this.getObject(%i).setVisible(false);
    }

    %this.dropText.setVisible(true);
  } else {
    %this.activeTab.setVisible(true);
    %this.tabSwatch.setVisible(true);
    %this.dropText.setVisible(false);
  }
}

function GlassChatroomResize::onResize(%this, %x, %y, %h, %l) {
  %window = %this.getGroup();
  %extent = %window.extent;
  %position = %window.position;
  %activeTab = %window.activeTab;
  %tabSwatch = %window.tabSwatch;
  %browserSwatch = %window.browserSwatch;

  %scrollSwatch = %activeTab.scrollSwatch;
  %userSwatch = %activeTab.userswatch;
  %input = %activeTab.input;
  %scroll = %activeTab.scroll;
  %tabButton = %activeTab.tabButton;
  %userScroll = %activeTab.userscroll;
  %chatText = %activeTab.chattext;

  %activeTab.extent = getWord(%extent, 0) - 10 SPC getWord(%extent, 1) - 68;
  %tabSwatch.extent = getWord(%extent, 0) - 20 SPC getWord(%tabSwatch.extent, 1);
  %scroll.extent = getWord(%extent, 0) - 150 SPC getWord(%extent, 1) - 90;
  %scrollSwatch.extent = getWord(%scroll.extent, 0) SPC getWord(%scroll.extent, 1);
  %chatText.extent = getWord(%scroll.extent, 0) - 15 SPC getWord(%chatText.extent, 1);
  %userScroll.extent = getWord(%userScroll.extent, 0) SPC getWord(%extent, 1) - 90;
  %userScroll.position = getWord(%scroll.extent, 0) + 15 SPC getWord(%userScroll.position, 1);
  %input.extent = getWord(%extent, 0) - 150 SPC getWord(%input.extent, 1);
  %input.position = getWord(%input.position, 0) SPC getWord(%scroll.extent, 1) + 5;

  if(isObject(%browserSwatch)) {
    %browserSwatch.extent = getWord(%extent, 0) - 20 SPC getWord(%extent, 1) - 70;

    for(%i = 0; %i < %browserSwatch.getCount(); %i++) {
      %obj = %browserSwatch.getObject(%i);
      %obj.extent = getWord(%browserSwatch.extent, 0) - 20 SPC getWord(%obj.extent, 1);
      if(%obj.getCount() > 2) {
        %btn = %obj.getObject(2);
        if(%btn.buttonType $= "PushButton") {
          %btn.position = getWord(%obj.extent, 0) - 75 SPC getWord(%btn.position, 1);
        }
      }
    }
  }

  if(!isObject(%activeTab))
    return;

  if(isObject(GlassLive_EmoteSelection)) {
    if(GlassOverlay.activeInput == %input) {
      GlassLive_EmoteSelection.position = vectorAdd(%window.position, 2 SPC getWord(%window.extent, 1));
    }
  }

  // %input.makeFirstResponder(1);

  if(%this.isAwake()) {
    %chatText.forceReflow();
  }

  // %scroll.scrollToBottom();

  %scrollSwatch.verticalMatchChildren(0, 2);
  %scrollSwatch.setVisible(true);

  // %userSwatch.getGroup().scrollToTop();
  %userSwatch.setVisible(true);
}

//================================================================
//= Tab Buttons                                                  =
//================================================================

function GlassChatroomWindow::createTabButton(%this, %i, %width) {
  %tabObj = %this.tab[%i];

  %button = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = %width SPC 25;
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = %tabObj.title;
    groupNum = "-1";
    buttonType = "PushButton";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";

    tabObj = %tabObj;
  };

  %button.text = new GuiTextCtrl(GlassCRText) {
    horizSizing = "center";
    vertSizing = "center";
    profile = "GlassChatroomTabProfile";
    text = %tabObj.title;
    extent = %width SPC 18;
    minextent = %width SPC 18;
  };

  if(GlassSettings.get("Glass::UseDefaultWindows")) {
    %inUse = "base/client/ui/tab1use";
    %notInUse = "base/client/ui/tab1";
  } else {
    %inUse = "Add-Ons/System_BlocklandGlass/image/gui/tab1use";
    %notInUse = "Add-Ons/System_BlocklandGlass/image/gui/tab1";
  }

  %button.mouseCtrl = new GuiMouseEventCtrl(GlassChatroomTabMouse) {
    image = %button;
    extent = %button.extent;
    position = "0 0";
    baseBitmap = (%i == %this.activeTabId ? %inUse : %notInUse);
    extension = "n";

    command = %this.getId() @ ".openTab(" @ %i @ ");";

    tabObj = %tabObj;
  };

  %button.mouseCtrl.render();

  %button.add(%button.text);
  %button.add(%button.mouseCtrl);

  %button.text.forceCenter();

  return %button;
}

function GlassChatroomWindow::createAddTabButton(%this) {
  %button = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";

    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = 35 SPC 25;
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "+/-";
    groupNum = "-1";
    buttonType = "PushButton";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 200";
    bitmap = (GlassSettings.get("Glass::UseDefaultWindows") ? "base/client/ui/tab1" : "Add-Ons/System_BlocklandGlass/image/gui/tabAdd1");

    command = %this.getId() @ ".openRoomBrowser();";
  };
  return %button;
}

function GlassChatroomTabMouse::setUse(%this, %bool) {
  if(GlassSettings.get("Glass::UseDefaultWindows"))
    %this.baseBitmap = (%bool ? "base/client/ui/tab1use" : "base/client/ui/tab1");
  else
    %this.baseBitmap = (%bool ? "Add-Ons/System_BlocklandGlass/image/gui/tab1use" : "Add-Ons/System_BlocklandGlass/image/gui/tab1");
  %this.render();
}

function GlassChatroomTabMouse::onMouseEnter(%this) {
  %this.extension = "h";
  %this.render();
}

function GlassChatroomTabMouse::onMouseLeave(%this, %a, %b, %c) {
  %this.extension = "n";
  %this.render();
}

function GlassChatroomTabMouse::onMouseDown(%this, %a, %pos) {
  %this.down = true;
  %this.downPoint = %pos;

  %this.extension = "d";
  %this.render();

  alxPlay(GlassPop1Audio);
}

function GlassChatroomTabMouse::onMouseDragged(%this, %a, %pos, %c) {
  %dist = vectorDist(%pos, %this.downPos);
  if(%dist > 20) {
    GlassLive::enterRoomDragMode(%this.image, %pos);
  }
}

function GlassChatroomTabMouse::onMouseUp(%this) {
  %this.extension = "n";
  %this.render();


  eval(%this.command);
}

function GlassChatroomTabMouse::render(%this) {
  %button = %this.image;
  %button.setBitmap(%this.baseBitmap @ "_" @ %this.extension);
}

function GlassChatroomWindow::renderTabs(%this) {
  %swatch = %this.tabSwatch;
  %swatch.deleteAll();
  %x = 0;
  %width = 140;
  for(%i = 0; %i < %this.tabs; %i++) {
    %tabObj = %this.tab[%i];
    if(isObject(%this.tabButton[%i])) {
      %this.tabButton[%i].delete();
    }

    %this.tabButton[%i] = %this.createTabButton(%i, %width);
    %this.tabButton[%i].position = %x SPC 0;
    %x += %width;

    %swatch.add(%this.tabButton[%i]);
    %tabObj.tabButton = %this.tabButton[%i];
  }

  %this.tabAddButton = %this.createAddTabButton();
  %this.tabAddButton.position = %x SPC 0;
  %swatch.add(%this.tabAddButton);

  %swatch.extent = (%x+getWord(%this.tabAddButton.extent, 0)) SPC getWord(%swatch.extent, 1);

  %pad = 10;
  %minWidth = (%pad*2)+getWord(%swatch.extent, 0);

  %this.minExtent = (%minWidth > 475 ? %minWidth : 475) SPC 290;
  if(getWord(%this.extent, 0) < %minWidth) {
    if(%this.isAwake()) {
      %this.schedule(0, resize, getWord(%this.position, 0), getWord(%this.position, 1), %minWidth, getWord(%this.extent, 1));
    } else {
      %this.extent = %minWidth SPC getWord(%this.extent, 1);
    }
  }
}

function GlassChatroomWindow::openRoomBrowser(%this, %rooms) {
  if(!isObject(%rooms)) {
    GlassLive.pendingRoomList = %this;
    GlassLiveConnection.placeCall("getRoomList");
    return;
  }

  %this.openTab(-1);

  %browserSwatch = new GuiSwatchCtrl(GlassRoomBrowser) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 60";
    extent = "455 220";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "230 230 230 255";
  };

  %swatch = new GuiSwatchCtrl(GlassRoomBrowser) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "435 26";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "100 100 100 255";

    id = %room.id;
  };

  %swatch.text = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    position = "25 6";
    text = "<tab:200><font:verdana bold:13><color:eeeeee>Name\tUsers";
    extent = "445 16";
  };

  %swatch.add(%swatch.text);
  %browserSwatch.add(%swatch);
  %last = %swatch;

  for(%i = 0; %i < %rooms.length; %i++) {
    %room = %rooms.value[%i];
    %swatch = new GuiSwatchCtrl(GlassRoomBrowser) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "435 26";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "210 210 210 255";

      id = %room.id;
    };

    %swatch.image = new GuiBitmapCtrl() {
      profile = "GuiDefaultProfile";
      extent = "16 16";
      position = "5 5";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %room.image;
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      position = "25 6";
      text = "<tab:200><font:verdana bold:13><color:333333>" @ %room.title TAB %room.users;
      extent = "405 16";
    };

    %swatch.button = new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonProfile";
      extent = "48 18";
      position = "365 4";
      bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
      text = (isObject(GlassLive.room[%room.id]) ? "Leave" : "Join");
      visible = "1";
      command = (!isObject(GlassLive.room[%room.id]) ? "GlassLive::joinRoom(" @ %room.id @ ");" : GlassLive.room[%room.id] @ ".leaveRoom();");
    };

    %swatch.add(%swatch.image);
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.button);

    if(%i > 0)
      %swatch.placeBelow(%last, 5);
    else
      %swatch.placeBelow(%last, 10);

    %browserSwatch.add(%swatch);

    %last = %swatch;
  }

  if(isObject(%this.browserSwatch)) {
    %this.browserSwatch.deleteAll();
    %this.browserSwatch.delete();
  }

  %this.browserSwatch = %browserSwatch;
  %this.add(%this.browserSwatch);
}

function GlassChatroomTab::setFlashing(%this, %bool) {
  if(isObject(%this)) {
    if(%bool) {
      if(%this.visible || %this.isFlashing)
        return;

      %this.isFlashing = true;

      %this.flashSchedule = %this.schedule(0, flashTick, 1);
    } else {
      if(isObject(%this.tabButton))
        %this.tabButton.mColor = "255 255 255 200";

      %this.isFlashing = false;

      cancel(%this.flashSchedule);
    }
  }
}

function GlassChatroomTab::flashTick(%this, %bool) {
  cancel(%this.flashSchedule);

  %button = %this.tabButton;
  if(isObject(%button)) {
    if(%bool) {
      %button.mColor = "255 60 60 200";
    } else {
      %button.mColor = "255 255 255 200";
    }
  }

  %this.flashSchedule = %this.schedule(500, flashTick, !%bool);
}

//================================================================
//= Icon Selector (Crown)                                        =
//================================================================

function GlassIconSelectorWindow::updateIcons(%this) {
  %allowed = "Add-Ons/System_BlocklandGlass/resources/icons_allowed.txt";
  if(!isFile(%allowed)) {
    error(%allowed SPC "not found, unable to create icon list.");
    return;
  }
  %swatch = GlassIconSelectorWindow_Swatch;

  if(!isObject(%swatch)) {
    error("Could not find icon list swatch.");
    return;
  }

  %path = "Add-Ons/System_BlocklandGlass/image/icon/";
  %iconCount = -1;

  %file = new FileObject();
  %file.openForRead("Add-Ons/System_BlocklandGlass/resources/icons_allowed.txt");
  while(!%file.isEOF()) {
    %line = %file.readLine();
    %icon = %path @ %line;

    if(!isFile(%icon @ ".png"))
      continue;

    %iconCount++;

    if(%iconCount % 14 == 0)
      %column = 0;

    %row = mFloor(%iconCount / 14);
    %position = (20 * %column) + 3 SPC (%row * 20) + 3;
    %column++;

    %bitmap = new GuiBitmapCtrl() {
      bitmap = %icon;
      position = %position;
      extent = "16 16";
    };
    %button = new GuiButtonBaseCtrl() {
      position = %position;
      extent = "16 16";
      command = "GlassIconSelectorWindow_Preview.setBitmap(\"Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ "\");";
    };
    GlassIconSelectorWindow_Swatch.add(%bitmap);
    GlassIconSelectorWindow_Swatch.add(%button);
  }
  GlassIconSelectorWindow_Swatch.extent = getWord(GlassIconSelectorWindow_Swatch.extent, 0) SPC (%row * 16 + %row * 7);
  GlassIconSelectorWindow_Swatch.setVisible(true);
}

function GlassIconSelectorWindow::selectIcon(%this) {
  if(!GlassLiveConnection.connected) {
    glassMessageBoxOk("No Connection", "You must be connected to Glass Live to change your icon.");
    return;
  }

  GlassLive::setIcon(strreplace(GlassIconSelectorWindow_Preview.bitmap, "Add-Ons/System_BlocklandGlass/image/icon/", ""));
  GlassOverlay::closeIconSelector();
}

function GlassIconSelectorWindow::onWake(%this) {
  %this.forceCenter();

  %icon = GlassLiveUser::getFromBlid(getNumKeyId()).icon;

  if(%icon $= "")
    %icon = "ask_and_answer";

  GlassIconSelectorWindow_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon);
}

//================================================================
//= Emote Selection                                     =
//================================================================

function GlassEmoteSelector::CacheEmotes() {
  $GlassEmoteCount = -1;
  %file = findFirstFile("Add-Ons/System_BlocklandGlass/image/icon/*");

  while(%file !$= "") {
    $GlassEmote[$GlassEmoteCount++] = fileBase(%file);
    %file = findNextFile("Add-Ons/System_BlocklandGlass/image/icon/*");
 }
}

function GlassEmoteSelector::ListEmotes(%msgBox) {
  GlassOverlay.activeInput = %msgBox;

  %parent = %msgBox.getGroup();
  if(%parent.getName() !$= "GlassMessageGui")
    %parent = %msgBox.getGroup().getGroup();

  if(isObject(GlassLive_EmoteSelection))
    GlassLive_EmoteSelection.delete(); // only allow one emote selection window at a time (not actually necessary)

  if(!GlassSettings.get("Live::EmotePredict"))
    return;

  %text = %msgBox.getValue();
  %currWord = getWord(%text, getWordCount(%text) - 1);
  %checkEmote = strReplace(%currWord, ":" , "");

  if(getSubStr(%currWord, 0, 1) !$= ":" || strLen(%currWord) < 3 || strReplace(%currWord, " ", "") !$= %currWord)
    return;

  if(getSubStr(%currWord, 1, 1) $= ":")
    return;

  if(getSubStr(%currWord, strLen(%currWord) - 1, 1) $= ":")
    return;

  for(%i=0; %i < $GlassEmoteCount; %i++) {
    %currEmote = $GlassEmote[%i];

    if(%possEmoteCount >= 75)
      return;

    if(striPos(%currEmote, %checkEmote) >= 0)
    {
      %possEmoteCount++;
      %possEmoteList = %possEmoteList SPC %currEmote;
    }
  }
  %possEmoteList = trim(%possEmoteList);
  %msgBox.emoteList = %possEmoteList;
  %possEmoteCount = getWordCount(%possEmoteList);
  if(%possEmoteCount == 0)
    return;

  %pos = vectorAdd(%parent.position, 2 SPC getWord(%parent.extent, 1));

  if(%possEmoteCount >= 10)
  {
    %vScroll = "alwaysOn";
    %scrollExtent = "150 400";
  }
  else
  {
    %vScroll = "alwaysOff";
    %scrollExtent = %possEmoteCount * 20;
  }

  %scroll = new GuiScrollCtrl(GlassLive_EmoteSelection) {
    profile = "GlassScrollProfile";

    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = %vScroll;
    extent = "150" SPC %scrollExtent;
    position = vectorAdd(%pos, "0 0");
  };

  %sel = new GuiSwatchCtrl() {
    position = %scroll.extent;
    extent = "150" SPC %possEmoteCount * 20;
    color = "0 0 0 0";
    position = "0 -2";
  };
  %scroll.add(%sel);


  for(%i=0; %i < %possEmoteCount; %i++) {
    %currEmote = getWord(%possEmoteList, %i);

    %swatch = new GuiSwatchCtrl() {
      extent = "150 20";
      position = "2" SPC %i * 20 + 2;
      color = "0 0 0 0";
    };

    %swatch.add(new GuiBitmapCtrl() {
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %currEmote @ ".png";
      extent = "16 16";
      position = "1 1";
    });

     %swatch.add(new GuiMLTextCtrl() {
      text = "<color:424242>" @ %currEmote;
      extent = "500 20";
      position = "22 2";
    });

    %swatch.add(new GuiMouseEventCtrl(GlassEmoteSelMouse) {
      extent = %swatch.extent;
      swatch = %swatch;
      emote = %currEmote;
      textBox = %msgBox;
      master = %scroll;
    });

    %sel.add(%swatch);
  }

  GlassOverlayGui.add(%scroll);
  %parent.emoteSelector = %scroll;
}


function GlassEmoteSelMouse::onMouseEnter(%this) {
  %this.swatch.color = "200 200 200 255";
}

function GlassEmoteSelMouse::onMouseLeave(%this) {
  %this.swatch.color = "0 0 0 0";
}

function GlassEmoteSelMouse::onMouseDown(%this, %a, %b, %c) {
  %currText = %this.textBox.getValue();

  %this.textBox.setValue(setWord(%currText, getWordCount(%currText) - 1, ":" @ %this.emote @ ": "));
  %this.master.delete();
}

function GlassChatroomGui_Input::onAdd(%this) {
  %this.command = "GlassEmoteSelector::ListEmotes(" @ %this.getID() @ ");";
}

//================================================================
//= Packages                                                     =
//================================================================

package GlassLivePackage {
  function NPL_List::onSelect(%this, %rowID, %rowText) {
    %row = %this.getRowTextById(%rowID);

    %blid = getField(%rowText, 3);

    if(%blid !$= "") {
      if(isEventPending(%this.glassDoubleClick) && %this.glassLastClicked $= %blid) {
        if(!GlassLiveConnection.connected) {
          glassMessageBoxOk("No Connection", "You must be connected to Glass Live to open user windows.");
          cancel(%this.glassDoubleClick);
          return;
        }

        %user = GlassLiveUser::getFromBlid(%blid);

        if(!isObject(%user)) {
          glassMessageBoxOk("Error", "Unable to find the requested user on Glass Live.");
          return;
        }

        GlassOverlay::open();
        GlassLive::openUserWindow(%blid);

        cancel(%this.glassDoubleClick);
      }

      %this.glassDoubleClick = %this.schedule(200, "");
      %this.glassLastClicked = %blid;
    }
  }

  function onExit() {
    GlassLive::disconnect();
    parent::onExit();
  }

  function disconnectedCleanup(%doReconnect) {
    GlassLive::updateLocation(false);

    return parent::disconnectedCleanup(%doReconnect);
  }

  function GameConnection::onConnectionAccepted(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k) {
    parent::onConnectionAccepted(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k);

    GlassLive::updateLocation(true);
  }

  function Avatar_Done() {
    parent::Avatar_Done();

    if(GlassLiveConnection.connected) {
      GlassFriendsGui_Blockhead.createBlockhead(0, true);
      GlassLive::sendAvatarData();
    }
  }

  function MainMenuGui::onRender(%this) {
    if(GlassLive.isInviteAccepted) {
      %addr = GlassLive.inviteAddress;

      if(!GlassLive.invitePass) {
        canvas.pushDialog(connectingGui);
        Connecting_Text.setValue("Connecting to " @ %addr @ "<br>");
      if(isObject(serverConnection))
        disconnectedCleanup();
        connectToServer(%addr, "", 1, 1);
      } else {
        $ServerInfo::Address = %addr;
        canvas.pushDialog(JoinServerPassGui);
      }

      GlassLive.isInviteAccepted = false;
      GlassLive.inviteAddress = "";
      GlassLive.invitePass = "";
    }
    parent::onRender(%this);
  }

  function Crouch(%bool) {
    if(GlassOverlayGui.isAwake())
      %bool = 0;

    return parent::Crouch(%bool);
  }

  function resetCanvas() {
    parent::resetCanvas();
    GlassLive::positionMessageReminder();
  }
};
activatePackage(GlassLivePackage);

package GlassAFKCheckPackage {
  function NMH_Type::Send(%this) {
    parent::Send(%this);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function GlassLive::chatroomInputSend(%id) {
    parent::chatroomInputSend(%id);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function GlassLive::messageInputSend(%id) {
    parent::messageInputSend(%id);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function Canvas::popDialog(%gui, %dlg) {
    parent::popDialog(%gui, %dlg);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function Canvas::pushDialog(%gui, %dlg) {
    parent::pushDialog(%gui, %dlg);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function mouseFire(%on) {
    parent::mouseFire(%on);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function Jump(%on) {
    parent::Jump(%on);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }

  function Jet(%on) {
    parent::Jet(%on);

    if(GlassLive.afkPackage)
      GlassLive.afkAction();
  }
};
activatePackage(GlassAFKCheckPackage);
