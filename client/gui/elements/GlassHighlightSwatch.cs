function GlassHighlightSwatch::addToSwatch(%swatch, %command) {
  %swatch.glassHighlight = new GuiMouseEventCtrl(GlassHighlightMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    extent = %swatch.extent;
    position = "0 0";
    //callback = "GlassLive::friendTabExtend(" @ %blid @ ");";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";

    command = %command;
  };
}

function GlassHighlightMouse::onMouseMove(%this, %a, %pos) {
  if(%this.skip) {
    %this.skip = !%this.skip;
    return; // save those frames
  }

  %gui = %this.getGroup();
  %pos = vectorSub(%pos, %gui.getCanvasPosition());
  if(!isObject(%gui.flare)) {
    %gui.flare = new GuiBitmapCtrl() {
      extent = "256 256";
      bitmap = "Add-Ons/System_BlocklandGlass/image/gui/glare.png";
      mcolor = "255 255 255 90";
      //mMultiply = 1;
      overflowImage = 1;
    };
    %gui.add(%gui.flare);
    %gui.bringToFront(%gui.flare);
  }

  %gui.flare.setVisible(true);
  %gui.flare.extent = "256 256";

  %gui.flare.position = vectorSub(%pos, "128 128");
  %gui.pushToBack(%this);
}

function GlassHighlightMouse::onMouseLeave(%this) {
  if(!%this.enabled)
    return;

  if(isObject(%this.getGroup().flare))
    %this.getGroup().flare.setVisible(false);

  %this.getGroup().color = %this.getGroup().ocolor;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(false);
    %this.getGroup().accept.setVisible(false);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(false);
  } else if(%this.online) {
    %this.getGroup().chaticon.setVisible(false);
  }

  %this.scrollEnd(%this.getGroup().text);
}

function GlassHighlightMouse::onMouseEnter(%this) {
  if(!%this.enabled)
    return;

  %this.getGroup().ocolor = %this.getGroup().color;
  %this.getGroup().color = %this.getGroup().hcolor;

  if(%this.type $= "request") {
    %this.getGroup().decline.setVisible(true);
    %this.getGroup().accept.setVisible(true);
  } else if(%this.type $= "blocked") {
    %this.getGroup().unblock.setVisible(true);
  } else if(%this.online && %this.status !$= "busy") {
    %this.getGroup().chaticon.setVisible(true);
  }

  if(getWord(%this.getGroup().text.extent, 0) > getWord(vectorSub(%this.extent, %this.pos), 0)-20)
    if(%this.scrollTick $= "")
      %this.scrollTick = %this.scrollLoop(%this.getGroup().text, true);
}

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

function GlassHighlightMouse::onMouseDown(%this) {
  if(%this.online) {
    %this.down = 1;
  }
}

function GlassHighlightMouse::onMouseUp(%this, %a, %pos) {
  %pos = vectorSub(%pos, %this.getCanvasPosition());
  if(%this.type $= "request") {
    if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
      glassMessageBoxOk("Friend Declined", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been declined.");
      GlassLive::friendDecline(%this.blid);
    } else if(getWord(%pos, 0) > getWord(%this.extent, 0)-50) {
      glassMessageBoxOk("Friend Added", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been added.");
      GlassLive::friendAccept(%this.blid);
    } else {
      if(isObject(%window = GlassLiveUser::getFromBlid(%this.blid).window))
        %window.delete();
      else
        GlassLive::openUserWindow(%this.blid);
    }
  } else if(%this.type $= "blocked") {
    if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
      glassMessageBoxOk("Unblocked", "<font:verdana bold:13>" @ %this.username SPC "<font:verdana:13>has been unblocked.");
      GlassLive::userUnblock(%this.blid);
    }
  } else if(%this.type $= "toggle") {
    if(!GlassLive_StatusPopUp.open) {
      eval(%this.toggleVar @ " = !" @ %this.toggleVar @ ";");
      GlassLive::createFriendList();
    }
  } else {
    if(getWord(%pos, 0) > getWord(%this.extent, 0)-25) {
      if(%this.online) {
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
