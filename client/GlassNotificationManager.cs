function GlassNotificationManager::init() {
  if(!isObject(GlassNotificationManager)) {
    new ScriptGroup(GlassNotificationManager) {
      offset = 0;
      tickRate = 50;

      enterTime = 500;
    };
  }
}

function GlassNotificationManager::refocus(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %ob = %this.getObject(%i);

    canvas.getObject(canvas.getCount()-1).add(%ob.swatch);
  }
}

function GlassNotificationManager::newNotification(%title, %text, %image, %darkMode) {
  %obj = new ScriptObject(GlassNotification) {
    title = %title;
    text = %text;
    image = %image;

    sticky = true;
    callback = %callback;
    time = 5000;

    index = GlassNotificationManager.index++;

    darkMode = %darkMode;
  };

  GlassNotificationManager.add(%obj);

  return %obj;
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

    %action[%note.action]++;
    switch$(%note.action) {
      case "waiting":

      case "enter":
        %swatch.position = vectorSub(%swatch.position, "10 0");
        if(getWord(%swatch.position, 0) < getWord(getRes(), 0)-260) {
          %swatch.position = getWord(getRes(), 0)-260 SPC getWord(%swatch.position, 1);
          %note.action = "displaying";
          %note.ticksRemaining = mCeil(%note.time/%this.tickRate);
        }

      case "displaying":
        %note.ticksRemaining--;

      case "dismiss":
        if(getWord(%swatch.position, 0) > getWord(getRes(), 0)) {
          %swatch.deleteAll();
          %swatch.delete();
          %note.delete();
          %i--;
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

        if(%note.action $= "displaying") {
          if(%note.ticksRemaining <= 0) {
            %note.action = "dismiss";
          }
        } else if(%note.action $= "waiting") {
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

function GlassNotificationManager::dismissAll(%this) {
  while(%this.getCount() > 0) {
    %obj = %this.getObject(0);
    %obj.swatch.delete();
    %obj.delete();
  }
  %this.condense();
}

function GlassNotificationSwatch::animate() {}

function GlassNotification::dismiss(%this) {
  %this.swatch.action = "out";
  %this.swatch.sch = %this.swatch.schedule(0, animate);
}

function GlassNotification::instantDismiss(%this) {
  %this.swatch.delete();
  %this.delete();
  GlassNotificationManager.schedule(0, condense);
}

function GlassNotification::onAdd(%this) {
  if(%this.swatch)
    return;

  if(%this.darkMode) {
    %color = "0 0 0 192";
  } else {
    %color = "255 255 255 128";
  }

  GlassNotificationManager.index[%this.index] = %this;
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
    vertSizing = "center";
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
    vertSizing = "center";
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
    text = (%this.darkMode ? "<color:ffffff>" : "") @ "<font:verdana bold:15><just:left>" @ %this.title @ "<br><font:verdana:13>" @ %this.text;
    position = "28 0";
    extent = "172 12";
    minextent = "0 0";
    autoResize = true;
    maxBitmapHeight = "12";
  };

  %swatch.mouse = new GuiMouseEventCtrl(GlassNotificationMouse) {
    swatch = %swatch;
    notification = %this;
    position = "0 0";
    extent = %swatch.extent;
  };

  %swatch.body.add(%swatch.text);
  %swatch.body.add(%swatch.image);

  %swatch.add(%swatch.head);
  %swatch.add(%swatch.body);
  %swatch.add(%swatch.foot);

  %swatch.add(%swatch.mouse);
  %swatch.position = getRes();
  Canvas.getObject(canvas.getCount()-1).add(%swatch);

  %swatch.text.forceReflow();
  %swatch.body.verticalMatchChildren(10, 0);
  %swatch.foot.placeBelow(%swatch.body);

  %swatch.verticalMatchChildren(20, 0);
  %swatch.mouse.extent = %swatch.extent;
  %swatch.image.centerY();

  %swatch.position = vectorAdd(getRes(), getWord(%swatch.extent, 0) SPC -getWord(%swatch.extent, 1)-10-GlassNotificationManager.offset);

  GlassNotificationManager.offset += getWord(%swatch.extent, 1)+10;

  %this.swatch = %swatch;
  %this.action = "waiting";

  if(!isEventPending(GlassNotificationManager.sch)) {
    echo("Calling tick");
    GlassNotificationManager.schedule(1, tick);
  }
}

function GlassNotificationMouse::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%this.notification.darkMode) {
    %swatch.head.mcolor = "32 32 32 225";
    %swatch.body.color = "32 32 32 225";
    %swatch.foot.mcolor = "32 32 32 225";
  } else {
    %swatch.head.mcolor = "255 255 255 225";
    %swatch.body.color = "255 255 255 225";
    %swatch.foot.mcolor = "255 255 255 225";
  }
}

function GlassNotificationMouse::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%this.notification.darkMode) {
    %swatch.head.mcolor = "0 0 0 192";
    %swatch.body.color = "0 0 0 192";
    %swatch.foot.mcolor = "0 0 0 192";
  } else {
    %swatch.head.mcolor = "255 255 255 128";
    %swatch.body.color = "255 255 255 128";
    %swatch.foot.mcolor = "255 255 255 128";
  }
}

function GlassNotificationMouse::onMouseDown(%this) {
  if(%this.notification.legacySource $= "RTB") {
    RTB_Overlay.fadeIn();
  } else if(%this.notification.callback !$= "") {
    if(strpos(%this.notification.callback, ";") == -1)
      call(%this.notification.callback, %this.notification);
    else
      eval(%this.notification.callback);
  }
  %this.swatch.action = "out";
  %this.swatch.animate();
}

function GlassNotificationMouse::onRightMouseDown(%this) {
  %this.swatch.action = "out";
  %this.swatch.animate();
}

package GlassNotificationManager {
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

    %obj = GlassNotificationManager::newNotification(%title, %message, %icon, %sticky);

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
          Glass.mmNotification = GlassNotificationManager::newNotification("Glass Live", "Press <color:ff3333>" @ strupr(getField(GlassSettings.get("Live::Keybind"), 1)) @ "<color:000000> to open Glass!", "glassLogo", 1, "GlassOverlay::open();");
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
};
activatePackage(GlassNotificationManager);
