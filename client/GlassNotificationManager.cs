function GlassNotificationManager::init() {
  if(!isObject(GlassNotificationManager)) {
    GlassGroup.add(new ScriptGroup(GlassNotificationManager) {
      offset = 0;
      tickRate = 10;

      enterTime = 500;
    });
  }
}

function GlassNotificationManager::refocus(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %ob = %this.getObject(%i);

    canvas.getObject(canvas.getCount()-1).add(%ob.swatch);
  }
}

function GlassNotificationManager::newNotification(%title, %text, %image, %darkMode) {
  error("Deprecated Notification Creation");
}

function orderNumWords(%list) {
  %list = trim(%list);
  %ret = "";
  for(%i = 0; %i < getWordCount(%list); %i++) {
    %term = getWord(%list, %i);

    %added = false;
    for(%j = 0; %j < getWordCount(%ret); %j++) {
      %w = getWord(%ret, %j);
      if(%term < %w) {
        %ret = setWord(%ret, %j, %term SPC %w);
        %added = true;
        break;
      }
    }

    if(!%added) {
      %ret = %ret SPC %term;
    }

    %ret = trim(%ret);
  }

  return %ret;
}

function GlassNotificationManager::tick(%this) {
  cancel(%this.sch);
  %this.sch = "";

  if(!%this.getCount()) {
    %this.offset = 0;
    return;
  }

  for(%i = 0; %i < %this.getCount(); %i++) {
    %note = %this.getObject(%i);
    %swatch = %note.swatch;

    if(!isObject(%swatch)) {
      %note.delete();
      %i--;
      continue;
    }

    %action[%note.action]++;
    switch$(%note.action) {
      case "waiting":
        //nothing

      case "enter":
        %swatch.position = vectorSub(%swatch.position, "10 0");
        if(getWord(%swatch.position, 0) < getWord(getRes(), 0)-260) {
          %swatch.position = getWord(getRes(), 0)-260 SPC getWord(%swatch.position, 1);
          %note.action = "displaying";
          %note.ticksRemaining = mCeil(%note.time/%this.tickRate);
        }

      case "displaying":
        if(!%note.sticky)
          %note.ticksRemaining--;

      case "dismiss":
        if(getWord(%swatch.position, 0) > getWord(getRes(), 0)) {
          %swatch.deleteAll();
          %swatch.schedule(0, delete);
          %note.schedule(0, delete);
        } else {
          %swatch.position = vectorAdd(%swatch.position, "10 0");
        }

      case "condense":
        %note.ticksRemaining--;
        %note.condenseTicks++;
        %vector = vectorSub(%swatch.condensePos, %swatch.origin);
        %diff = vectorScale(%vector, %note.condenseTicks/20);
        %swatch.position = vectorAdd(%swatch.origin, %diff);

        if(%note.condenseTicks >= 20) {
          %swatch.position = %swatch.condensePos;
          %note.action = "displaying";
        }
    }
  }

  %moving = %action["enter"]+%action["dismiss"]+%action["condense"];
  if(%moving == 0) { //none are moving
    %ct = %this.condense();

    if(%ct == 0) {
      %offset = 0;
      for(%i = 0; %i < %this.getCount(); %i++) {
        %indexList = %indexList SPC %this.getObject(%i).index;
      }

      %indexList = orderNumWords(%indexList);

      for(%i = 0; %i < getWordCount(%indexList); %i++) {
        %note = %this.index[getWord(%indexList, %i)];
        %offset += getWord(%note.swatch.extent, 1)+10;

        if(%note.action $= "displaying" && !%note.isHovering) {
          if(%note.ticksRemaining <= 0) {
            %note.action = "dismiss";
          }
        } else if(%note.action $= "waiting") {
          if(GlassSettings.get("Notifications::Limit") > 0) {
            if(GlassSettings.get("Notifications::Limit") <= (%action["displaying"]+%newDisp)) {
              continue; //too many notifications!
            }
            %newDisp++;
          }

          if(GameModeGui.isAwake() || CustomGameGui.isAwake() || ServerSettingsGui.isAwake()) {
            continue;
          }

          %note.action = "enter";
          %note.swatch.position = getWord(getRes(), 0) SPC getWord(getRes(), 1)-%offset;
        }
      }
    }
  }

  %this.sch = %this.schedule(%this.tickRate, tick);
}

function GlassNotificationManager::condense(%this) {
  %offset = 10;
  %indexList = "";
  for(%i = 0; %i < %this.getCount(); %i++) {
    if(%this.getObject(%i).action $= "waiting")
      continue;

    %indexList = %indexList SPC %this.getObject(%i).index;
  }

  %indexList = orderNumWords(%indexList);

  for(%i = 0; %i < getWordCount(%indexList); %i++) {
    %note = %this.index[getWord(%indexList, %i)];
    %pos = getWord(%note.swatch.position, 0) SPC getWord(getRes(), 1)-%offset-getWord(%note.swatch.extent, 1);

    %offset += 10+getWord(%note.swatch.extent, 1);

    if(%pos !$= %note.swatch.position) {
      %ct++;
      %note.swatch.condensePos = %pos;
      %note.swatch.origin = %note.swatch.position;
      %note.action = "condense";

      %note.condenseTicks = 0;
    }
  }

  GlassNotificationManager.offset = %offset-10;
  return %ct;
}

function GlassNotification::dismiss(%this) {
  %this.action = "dismiss";
}

function GlassNotification::destroy(%this) {
  %this.swatch.delete();
  %this.schedule(1, delete);
}

function GlassNotificationManager::destroyAll(%this) {
  while(%this.getCount() > 0) {
    %obj = %this.getObject(0);
    %obj.destroy();
  }
}

function GlassNotificationManager::dismissAll(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %obj = %this.getObject(%i);
    %obj.dismiss();
  }
}

function GlassNotification::onAdd(%this, %a, %b) {
  if(!GlassNotificationManager.isMember(%this)) {
    GlassNotificationManager.add(%this);
    %this.index = GlassNotificationManager.index++;
  } else {
    return;
  }

  if(%this.swatch) {
    return;
  }

  if(%this.time $= "")
    %this.time = GlassSettings.get("Notifications::DisplayTime") * 1000;

  if(%this.darkMode $= "")
    %this.darkMode = GlassSettings.get("Notifications::DarkMode");


  if(%this.darkMode) {
    %color = "16 16 16 170";
  } else {
    %color = "255 255 255 170";
  }

  GlassNotificationManager.index[%this.index] = %this;

  if(GlassSettings.get("Notifications::ForceSticky"))
    %this.sticky = true;

  %swatch = new GuiSwatchCtrl(GlassNotificationSwatch) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 0";
    position = "0 0";
    extent = "250 2";

    notification = %this;
  };

  %swatch.head = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/notification_top.png";
    position = "0 0";
    extent = "250 10";
    minextent = "0 0";
    clipToParent = true;

    mColor = %color;
  };

  %swatch.body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = %color;
    position = "0 10";
    extent = "250 2";

    notification = %this;
  };

  %swatch.foot = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/notification_bottom.png";
    position = "0 0";
    extent = "250 10";
    minextent = "0 0";
    clipToParent = true;

    mColor = %color;
  };

  %swatch.image = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "center";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %this.image @ ".png";
    position = "6 4";
    extent = "16 16";
    minextent = "0 0";
    clipToParent = true;
  };

  %swatch.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = (%this.darkMode ? "<color:eeeeee>" : "") @ "<font:verdana bold:15><just:left>" @ %this.title @ "<br><font:verdana:13>" @ %this.text;
    position = "28 7";
    extent = "210 12";
    minextent = "210 12";
    autoResize = true;
    maxBitmapHeight = "12";
  };

  %swatch.mouse = new GuiMouseEventCtrl(GlassNotificationMouse) {
    swatch = %swatch;
    notification = %this;
    position = "0 0";
    extent = %swatch.extent;
  };

  %swatch.body.add(%swatch.image);

  %swatch.add(%swatch.head);
  %swatch.add(%swatch.body);
  %swatch.add(%swatch.foot);

  %swatch.add(%swatch.text);

  %swatch.add(%swatch.mouse);
  %swatch.position = getRes();

  Canvas.getObject(canvas.getCount()-1).add(%swatch);


  if(%swatch.text.isAwake()) {
    %swatch.text.forceReflow();
    %swatch.body.extent = 250 SPC getWord(%swatch.text.extent, 1)-6;
  } else {
    %swatch.body.extent = 250 SPC 10;
    %swatch.text.centerY();
  }
  %swatch.foot.placeBelow(%swatch.body);

  %swatch.verticalMatchChildren(20, 0);
  %swatch.image.centerY();
  %swatch.mouse.extent = %swatch.extent;

  %swatch.position = vectorAdd(getRes(), getWord(%swatch.extent, 0) SPC -getWord(%swatch.extent, 1)-10-GlassNotificationManager.offset);

  GlassNotificationManager.offset += getWord(%swatch.extent, 1)+10;

  %this.swatch = %swatch;
  %this.action = "waiting";

  if(!isEventPending(GlassNotificationManager.sch)) {
    GlassNotificationManager.schedule(1, tick);
  }
}

function GlassNotification::updateText(%this) {
  %this.swatch.text.setText((%this.darkMode ? "<color:eeeeee>" : "") @ "<font:verdana bold:15><just:left>" @ %this.title @ "<br><font:verdana:13>" @ %this.text);
  %this.ticksRemaining = mCeil(%this.time/GlassNotificationManager.tickRate);
}

function GlassNotificationMouse::onMouseEnter(%this) {
  %swatch = %this.swatch;
  %swatch.notification.isHovering = true;
  if(%this.notification.darkMode) {
    %color = "16 16 16 225";
  } else {
    %color = "255 255 255 225";
  }
  %swatch.head.mcolor = %color;
  %swatch.body.color = %color;
  %swatch.foot.mcolor = %color;
}

function GlassNotificationMouse::onMouseLeave(%this) {
  %swatch = %this.swatch;
  %swatch.notification.isHovering = false;
  if(%this.notification.darkMode) {
    %color = "16 16 16 150";
  } else {
    %color = "255 255 255 150";
  }
  %swatch.head.mcolor = %color;
  %swatch.body.color = %color;
  %swatch.foot.mcolor = %color;
}

function GlassNotificationMouse::onMouseUp(%this) {
  if(%this.notification.legacySource $= "RTB") {
    RTB_Overlay.fadeIn();
  } else if(%this.notification.callback !$= "") {
    if(strpos(%this.notification.callback, ";") == -1)
      call(%this.notification.callback, %this.notification);
    else
      eval(%this.notification.callback);
  }
  %this.notification.dismiss();
}

function GlassNotificationMouse::onRightMouseUp(%this) {
  %this.notification.dismiss();
}

package GlassNotifications {
  function RTBCC_NotificationManager::push(%this, %title, %message, %icon, %key, %holdTime) {
    if(%holdTime $= "") {
      %holdTime = 3000;
    }

    if(%holdTime < 0) {
      %sticky = true;
    }

    if(%icon $= "") {
      %icon = "note_pin";
    }

    %obj = new ScriptObject(GlassNotification) {
      title = %title;
      text = %message;
      image = %icon;

      sticky = %sticky;
    };

    %obj.time = %holdTime;
    %obj.legacySource = "RTB";
    %obj.legacyKey = %key;
  }

  function Canvas::setContent(%this,%content) {
    parent::setContent(%this,%content);

    GlassNotificationManager.refocus();

    if(%content.getName() $= "LoadingGui") {
      if(isObject(Glass.mmNotification)) {
        Glass.mmNotification.dismiss();
      }
    } else if(%content.getName() $= "MainMenuGui") {
      if(GlassSettings.get("Live::StartupNotification")) {
        if(!$Glass::StartupNotified) {
          Glass.mmNotification = new ScriptObject(GlassNotification) {
            title = "Blockland Glass";
            text = "Press <sPush><color:ff3333>" @ strupr(getField(GlassSettings.get("Live::Keybind"), 1)) @ "<sPop> to open Glass!";
            image = "glassLogo";

            sticky = true;
            callback = "GlassOverlay::open();";
          };
          $Glass::StartupNotified = true;
        }
      }
    }
  }

  function Canvas::pushDialog(%this,%dialog) {
    parent::pushDialog(%this,%dialog);

    if(%dialog.getName() $= "GameModeGui") {
      if(isObject(Glass.mmNotification)) {
        Glass.mmNotification.dismiss();
      }
    }

    GlassNotificationManager.refocus();
  }

  function Canvas::popDialog(%this,%dialog) {
    parent::popDialog(%this,%dialog);

    GlassNotificationManager.refocus();
  }

  function doScreenshot(%bool) {
    for(%i = 0; %i < GlassNotificationManager.getCount(); %i++) {
      GlassNotificationManager.getObject(%i).swatch.setVisible(!%bool);
    }

    parent::doScreenshot(%bool);
  }
};
activatePackage(GlassNotifications);
