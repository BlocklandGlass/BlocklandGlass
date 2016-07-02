function GlassNotificationManager::init() {
  if(!isObject(GlassNotificationManager))
    new ScriptGroup(GlassNotificationManager) {
      offset = 0;
    };
}

function GlassNotificationManager::refocus(%this) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    %ob = %this.getObject(%i);

    canvas.getObject(canvas.getCount()-1).add(%ob.swatch);
  }
}

function GlassNotificationManager::newNotification(%title, %text, %image, %sticky, %callback) {
  %obj = new ScriptObject(GlassNotification) {
    title = %title;
    text = %text;
    image = %image;

    sticky = %sticky;
    callback = %callback;
    time = 5000;

    index = GlassNotificationManager.index++;
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

function GlassNotificationManager::condense(%this) {
  %offset = 10;
  %indexList = "";
  for(%i = 0; %i < %this.getCount(); %i++) {
    if(%this.getObject(%i).swatch.action !$= "hold") {
      return;
    } else {
      %indexList = %indexList SPC %this.getObject(%i).index;
    }
  }

  %indexList = orderNumWords(%indexList);

  for(%i = 0; %i < getWordCount(%indexList); %i++) {
    %note = %this.index[getWord(%indexList, %i)];
    %pos = getWord(%note.swatch.position, 0) SPC getWord(getRes(), 1)-%offset-getWord(%note.swatch.extent, 1);
    %offset += 10+getWord(%note.swatch.extent, 1);
    %note.swatch.conPos = %pos;
    %note.swatch.action = "condense";
    %note.swatch.animate();
  }

  GlassNotificationManager.offset = %offset-10;
}

function GlassNotification::dismiss(%this) {
  %this.swatch.action = "out";
  %this.swatch.sch = %this.swatch.schedule(0, animate);
}

function GlassNotification::onAdd(%this) {
  GlassNotificationManager.index[%this.index] = %this;
  %swatch = new GuiSwatchCtrl(GlassNotificationSwatch) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 128";
    position = "0 0";
    extent = "250 2";

    notification = %this;
  };

  %swatch.image = new GuiBitmapCtrl(GlassDownloadSprite) {
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
    text = "<font:verdana bold:15><just:left>" @ %this.title @ "<br><font:verdana:13>" @ %this.text;
    position = "28 5";
    extent = "172 12";
    minextent = "0 0";
    autoResize = true;
  };

  %swatch.mouse = new GuiMouseEventCtrl(GlassNotificationMouse) {
    swatch = %swatch;
    notification = %this;
    position = "0 0";
    extent = %swatch.extent;
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.image);
  %swatch.add(%swatch.mouse);
  %swatch.position = getRes();
  Canvas.getObject(canvas.getCount()-1).add(%swatch);

  %swatch.text.forceReflow();
  %swatch.verticalMatchChildren(10, 8);
  %swatch.mouse.extent = %swatch.extent;
  %swatch.image.centerY();

  %swatch.position = vectorAdd(getRes(), getWord(%swatch.extent, 0) SPC -getWord(%swatch.extent, 1)-10-GlassNotificationManager.offset);
  GlassNotificationManager.offset += getWord(%swatch.extent, 1)+10;
  %this.swatch = %swatch;

  %swatch.action = "in";
  %swatch.animate();
}

function GlassNotificationSwatch::animate(%this) {
  if(%this.sch)
    cancel(%this.sch);

  switch$(%this.action) {
    case "in":
      %this.position = vectorSub(%this.position, "5 0");
      if(getWord(%this.position, 0) < getWord(canvas.getExtent(), 0)-260) {
        %this.position = getWord(canvas.getExtent(), 0)-260 SPC getWord(%this.position, 1);
        %this.action = "hold";
        %this.sch = %this.schedule(%this.notification.time, animate);
        %this.arrived = getRealTime();
      } else {
        %this.sch = %this.schedule(10, animate);
      }

    case "hold":
      if(!%this.notification.sticky) {
        %this.action = "out";
        %this.sch = %this.schedule(0, animate);
      }

    case "out":
      %this.position = vectorAdd(%this.position, "5 0");
      if(getWord(%this.position, 0) > getWord(canvas.getExtent(), 0)) {
        %this.notification.delete();
        %this.delete();
        GlassNotificationManager.condense();
      } else {
        %this.sch = %this.schedule(10, animate);
      }

    case "condense":
      if(%this.conIter == 0) {
        %this.conVel = getWord(vectorSub(%this.position, %this.conPos), 1)/50;
      }
      %this.conIter++;
      %this.position = vectorSub(%this.position, 0 SPC %this.conVel);
      if(%this.conIter >= 50) {
        %this.position = %this.conPos;
        %this.conIter = 0;
        %this.action = "hold";

        if(%this.notification.sticky) {
          %time = %this.notification.time-(getRealTime()-%this.arrived);
          %this.sch = %this.schedule(%time, animate);
        } else {
          %this.sch = %this.schedule(10, animate);
        }
      } else {
        %this.sch = %this.schedule(10, animate);
      }
  }
}

function GlassNotificationMouse::onMouseEnter(%this) {
  %this.swatch.color = "255 255 255 225";
}

function GlassNotificationMouse::onMouseLeave(%this) {
  %this.swatch.color = "255 255 255 128";
}

function GlassNotificationMouse::onMouseDown(%this) {
  if(%this.notification.legacySource $= "oRBs") {
    ORBS_Overlay.fadeIn();
  } else if(%this.notification.legacySource $= "RTB") {
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
  function ORBSCC_NotificationManager::push(%this, %title, %message, %icon, %key, %holdTime) {
    //oRBs is cancer
    RTBCC_NotificationManager::push(%this, %title, %message, %icon, %key, %holdTime, true);
  }

  function RTBCC_NotificationManager::push(%this, %title, %message, %icon, %key, %holdTime, %orbs) {
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
    %obj.legacySource = %orbs ? "oRBs" : "RTB";
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
      if(!isObject(Glass.mmNotification)) {
        Glass.mmNotification = GlassNotificationManager::newNotification("Mod Manager", "Press <color:ff3333>" @ strupr(getField(GlassSettings.get("MM::Keybind"), 1)) @ "<color:000000> to open the mod manager!", "module", 1, "canvas.pushDialog(GlassModManagerGui);");
      }
    }
  }

  function Canvas::pushDialog(%this,%dialog) {
    parent::pushDialog(%this,%dialog);

    GlassNotificationManager.refocus();
  }

  function Canvas::popDialog(%this,%dialog) {
    parent::popDialog(%this,%dialog);

    GlassNotificationManager.refocus();
  }
};
activatePackage(GlassNotificationManager);
