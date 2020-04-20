
//================================================================
//= Overlay Gui                                                  =
//================================================================

function GlassOverlayGui::onWake(%this) {
  Glass::setCrashable(true);

  %x = getWord(getRes(), 0);
  %y = getWord(getRes(), 1);
  GlassOverlay.resize(0, 0, %x, %y);

  GlassLiveBugReportIcon.position = (%x-24) SPC 8;
  GlassLiveBugReportButton.position = (%x-32) SPC 0;
  GlassOverlayGui.pushToBack(GlassLiveBugReportIcon);
  GlassOverlayGui.pushToBack(GlassLiveBugReportButton);

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      if(isObject(%window.activeTab)) {
        %tab = %window.activeTab;
        if(%tab.chattext.didUpdate) {
          %tab.chattext.forceReflow();
          %tab.chattext.didUpdate = false;
        }
        %tab.scrollSwatch.verticalMatchChildren(0, 2);
        %tab.scrollSwatch.setVisible(true);

        %tab.scroll.scrollToBottom();
      }
    } else if(%window.getName() $= "GlassMessageGui") {
      %window.chattext.forceReflow();
      %window.scrollSwatch.verticalMatchChildren(0, 3);
      %window.scrollSwatch.setVisible(true);
      %window.scroll.scrollToBottom();
    } else if(%window.getName() $= "GlassUserGui") {
      if(isObject(%window.Blockhead)) {
        %window.Blockhead.schedule(1, "setCamera");
        %window.Blockhead.schedule(1, "setOrbitDist", 6);
        %window.Blockhead.schedule(1, "setCameraRot", 0, 0, $pi * 1.1);
      }
    }
  }

  if(isObject(GlassFriendsGui_Blockhead)) {
    GlassFriendsGui_Blockhead.schedule(1, "setOrbitDist", 5.5);
    GlassFriendsGui_Blockhead.schedule(1, "setCameraRot", 0.22, 0.5, 2.8);
  }

  if(!isObject(GlassOverlayResponder)) {
    new GuiTextEditCtrl(GlassOverlayResponder) {
      profile = "GuiTextEditProfile";
      position = "-100 -100";
      extent = "10 10";
      visible = 1;
    };
    GlassOverlayGui.add(GlassOverlayResponder);
  }

  GlassOverlayResponder.schedule(1, makeFirstResponder, true);

  if(!GlassOverlayGui.isMember(GlassFriendsWindow)) {
    %pos = GlassSettings.get("Live::FriendsWindow_Pos");
    %ext = 260 SPC getWord(GlassSettings.get("Live::FriendsWindow_Ext"), 1);

    if(%pos > getWord(getRes(), 0))
      %pos = (getWord(getRes(), 0) - 280) SPC 50;

    if(%ext > getWord(getRes(), 1))
      %ext = "260 380";

    GlassFriendsWindow.position = %pos;
    GlassFriendsWindow.extent = %ext;

    GlassOverlayGui.add(GlassFriendsWindow);

    GlassFriendsResize.onResize(getWord(GlassFriendsWindow.position, 0), getWord(GlassFriendsWindow.position, 1), getWord(GlassFriendsWindow.extent, 0), getWord(GlassFriendsWindow.extent, 1));
  }

  if(isObject(GlassMessageReminder)) {
    GlassMessageReminder.setVisible(false);
  }
}

function GlassOverlayGui::onSleep(%this) {
  Glass::setCrashable(false);
}

function GlassLive_keybind(%down) {
  if(%down) {
    if(!GlassOverlayGui.isAwake()) {
      GlassOverlay::open();
    } else {
      GlassOverlay::close();
    }
  }
}

function GlassOverlay::open() {
  canvas.pushDialog(GlassOverlayGui);
  GlassNotificationManager.dismissAll();
}

function GlassOverlay::close() {
  canvas.popDialog(GlassOverlayGui);
}

function GlassOverlay::setVignette() {
  if(GlassSettings.get("Live::Vignette")) {
    GlassOverlay.mColor = "0 0 50 185";
    GlassOverlay.setBitmap("base/client/ui/vignette");
  } else {
    GlassOverlay.mColor = "0 0 0 185";
    GlassOverlay.setBitmap("base/client/ui/btnBlank_d");
  }
}

function GlassOverlay::updateButtonAlignment() {
  %x = getWord(getRes(), 0);

  for(%i = 0; %i < GlassOverlay.getCount(); %i++) {
    %obj = GlassOverlay.getObject(%i);

    if(%obj.profile $= "GlassRoundedButtonProfile") {
      if(GlassSettings.get("Glass::AlignOverlayButtons") $= "Right")
        %pos = (%x - getWord(%obj.extent, 0) - 6) SPC getWord(%obj.position, 1);
      else if(GlassSettings.get("Glass::AlignOverlayButtons") $= "Left")
        %pos = 6 SPC getWord(%obj.position, 1);

      %obj.position = %pos;
    }
  }
}

function GlassOverlay::setLogo() {
  if(GlassSettings.get("Live::OverlayLogo") && !GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(true);
  else if(!GlassSettings.get("Live::OverlayLogo") && GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(false);
}

function GlassOverlay::resetFocus() {
  %chatVisible = false;

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow" && %window.visible) {
      %chatVisible = true;
      break;
    }
  }

  if(%chatVisible) {
    if(isObject(%window.activeTab.input)) {
      %window.activeTab.input.makeFirstResponder(true);
      return;
    }
  }

  GlassOverlayResponder.schedule(1, makeFirstResponder, true);
}

//================================================================
//= Mod Manager                                                  =
//================================================================

function GlassOverlay::openModManager(%force) {
  GlassOverlay::open();
  if(GlassModManagerGui.getCount() > 0) {
    GlassOverlayGui.add(GlassModManagerGui_Window);
    GlassModManagerGui_Window.forceCenter();
    GlassModManagerGui_Window.visible = false;
  }
  GlassModManagerGui_Window.setVisible(%force $= "" ? !GlassModManagerGui_Window.visible : %force);

  if(!GlassModManagerGui_Window.visible) {
    GlassOverlay::resetFocus();
  }

  if(GlassModManagerGui.page $= "") {
    GlassModManagerGui.openPage(GMM_ActivityPage);
  }

  GlassOverlayGui.pushToBack(GlassModManagerGui_Window);
}

function GlassOverlay::closeModManager() {
  GlassModManagerGui_Window.setVisible(false);
  GlassOverlay::resetFocus();
}

//================================================================
//= Manual                                                       =
//================================================================

function GlassOverlay::openManual() {
  if(isObject(GlassManualWindow)) {
    GlassManualWindow.setVisible(!GlassManualWindow.visible);
  }

  if(GlassManualWindow.visible) {
    GlassOverlayGui.pushToBack(GlassManualWindow);
  }
}

function GlassOverlay::closeManual() {
  GlassManualWindow.setVisible(false);
}

//================================================================
//= Setings                                                      =
//================================================================

function GlassOverlay::openSettings() {
  if(isObject(GlassSettingsWindow)) {
    GlassSettingsWindow.setVisible(!GlassSettingsWindow.visible);
    GlassSettingsGui_ScrollOverlay.setVisible(true);
  }

  if(GlassSettingsWindow.visible) {
    GlassOverlayGui.pushToBack(GlassSettingsWindow);
  } else {
    GlassOverlay::resetFocus();
  }
}

function GlassOverlay::closeSettings() {
  GlassSettingsWindow.setVisible(false);
  GlassOverlay::resetFocus();
}

//================================================================
//= Icon Selector                                                =
//================================================================

function GlassOverlay::openIconSelector() {
  if(isObject(GlassIconSelectorWindow)) {
    GlassIconSelectorWindow.onWake();
    GlassIconSelectorWindow.setVisible(!GlassIconSelectorWindow.visible);
  }

  if(GlassIconSelectorWindow.visible) {
    GlassOverlayGui.pushToBack(GlassIconSelectorWindow);
  }
}

function GlassOverlay::closeIconSelector() {
  GlassIconSelectorWindow.setVisible(false);
}

//================================================================
//= Chatroom                                                     =
//================================================================

function GlassOverlay::openChatroom() {
  %chatFound = false;

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      %chatFound = true;

      %window.setVisible(!%window.visible);
    }
  }

  GlassOverlay::resetFocus();

  if(!%chatFound) {
    if(GlassSettings.get("Live::ConfirmConnectDisconnect")) {
      if(GlassLiveConnection.connected) {
        glassMessageBoxYesNo("Reconnect", "This will reconnect you to Glass Live, continue?", "GlassLive::disconnect(1); GlassLive.schedule(1000, connectToServer);");
      } else {
        glassMessageBoxYesNo("Connect", "This will connect you to Glass Live, continue?", "GlassLive.schedule(0, connectToServer);");
      }
    } else {
      if(GlassLiveConnection.connected) {
        GlassLive::disconnect($Glass::Disconnect["Manual"]);
        GlassLive.schedule(100, connectToServer);
      } else {
        GlassLive.schedule(0, connectToServer);
      }
    }
  }
}

//================================================================
//= Moderation                                                   =
//================================================================

function GlassOverlay::openModeration(%safe) {
  if(!GlassLiveUser::getFromBlid(getNumKeyId()).isMod())
	  return;

  if(!GlassOverlayGui.isMember(GlassModeratorWindow)) {
    GlassModeratorWindow_Selection.add("Ban", 0);
    GlassModeratorWindow_Selection.add("Bar", 1);
    GlassModeratorWindow_Selection.add("Kick", 2);
    GlassModeratorWindow_Selection.add("Mute", 3);
    GlassOverlayGui.add(GlassModeratorWindow);
    GlassModeratorWindow.forceCenter();

    GlassModeratorWindow_Reason.enabled = false;
    GlassModeratorWindow_Duration.enabled = false;
  } else {
    if(%safe)
      GlassModeratorWindow.setVisible(true);
    else
      GlassModeratorWindow.setVisible(!GlassModeratorWindow.visible);
  }

  if(!GlassModeratorWindow.visible) {
    GlassModeratorWindow_ReasonBlocker.setVisible(false);
    GlassModeratorWindow_DurationBlocker.setVisible(false);
    GlassModeratorWindow_Reason.enabled = false;
    GlassModeratorWindow_Duration.enabled = false;
    GlassOverlay::resetFocus();
    return;
  }

  GlassOverlayGui.pushToBack(GlassModeratorWindow);

  GlassModeratorWindow_Selection.onSelect();

  GlassModeratorGui.updateBLID();
}

function GlassOverlay::closeModeration() {
  GlassModeratorWindow_ReasonBlocker.setVisible(false);
  GlassModeratorWindow_DurationBlocker.setVisible(false);
  GlassModeratorWindow_Reason.enabled = false;
  GlassModeratorWindow_Duration.enabled = false;

  GlassModeratorWindow.setVisible(false);

  GlassOverlay::resetFocus();
}
