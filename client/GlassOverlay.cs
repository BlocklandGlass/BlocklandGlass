function GlassOverlayGui::onWake(%this) {
  %x = getWord(getRes(), 0);
  %y = getWord(getRes(), 1);
  GlassOverlay.resize(0, 0, %x, %y);

  if(GlassSettings.get("Live::OverlayLogo") && !GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(true);
  else if(!GlassSettings.get("Live::OverlayLogo") && GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(false);

 if(GlassSettings.get("Live::Vignette") && GlassOverlay.bitmap !$= "")
	GlassOverlay.setBitmap("base/client/ui/vignette");
  else if(!GlassSettings.get("Live::Vignette") && GlassOverlay.bitmap $= "base/client/ui/vignette")
    GlassOverlay.setBitmap("base/client/ui/btnBlank_d");

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
		%window.Blockhead.createBlockhead(%window.uo.avatar);
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
    %ext = GlassSettings.get("Live::FriendsWindow_Ext");

    if(%pos > getWord(getRes(), 0))
      %pos = (getWord(getRes(), 0) - 280) SPC 50;

    if(%ext > getWord(getRes(), 1))
      %ext = "230 380";

    GlassFriendsWindow.position = %pos;
    GlassFriendsWindow.extent = %ext;

    GlassOverlayGui.add(GlassFriendsWindow);

    GlassFriendsResize.onResize(getWord(GlassFriendsWindow.position, 0), getWord(GlassFriendsWindow.position, 1), getWord(GlassFriendsWindow.extent, 0), getWord(GlassFriendsWindow.extent, 1));
  }
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

function GlassOverlay::openModManager() {
  GlassOverlay::open();
  if(GlassModManagerGui.getCount() > 0) {
    GlassOverlayGui.add(GlassModManagerGui_Window);
    GlassModManagerGui_Window.forceCenter();
    GlassModManagerGui_Window.visible = false;
  }
  GlassModManagerGui_Window.setVisible(!GlassModManagerGui_Window.visible);

  if(GlassModManagerGui.page $= "") {
    GlassModManagerGui.openPage(GMM_ActivityPage);
  }

  GlassOverlayGui.pushToBack(GlassModManagerGui_Window);
}

function GlassOverlay::closeModManager() {
  GlassModManagerGui_Window.setVisible(false);
}

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

function GlassOverlay::openSettings() {
  if(isObject(GlassSettingsWindow)) {
    GlassSettingsWindow.setVisible(!GlassSettingsWindow.visible);
    GlassSettingsGui_ScrollOverlay.setVisible(true);
  }

  if(GlassSettingsWindow.visible) {
    GlassOverlayGui.pushToBack(GlassSettingsWindow);
  }
}

function GlassOverlay::closeSettings() {
  GlassSettingsWindow.setVisible(false);
}

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

function GlassOverlay::openChatroom() {
  %chatFound = false;

  for(%i = 0; %i < GlassOverlayGui.getCount(); %i++) {
    %window = GlassOverlayGui.getObject(%i);
    if(%window.getName() $= "GlassChatroomWindow") {
      %chatFound = true;

      %window.setVisible(!%window.visible);
    }
  }

  if(!%chatFound) {
    if(GlassSettings.get("Live::ConfirmConnectDisconnect")) {
      if(GlassLiveConnection.connected) {
      glassMessageBoxYesNo("Reconnect", "This will reconnect you to Glass Live, continue?", "GlassLive::disconnect(1); GlassLive.schedule(100, connectToServer);");
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