function GlassNotificationManager::connectToNotificationServer() {
  %server = "localhost";
  %port = 27000;

  if(isObject(GlassNotificationTCP)) {
    error("GlassNotificationTCP exists!");
    return;
  }

  new TCPObject(GlassNotificationTCP) {
    debug = true;
  };

  GlassNotificationTCP.connect(%server @ ":" @ %port);
}

function GlassNotificationTCP::onConnected(%this) {
  %this.send("auth\t" @ GlassAuth.ident @ "\r\n");
}

function GlassNotificationTCP::onLine(%this, %line) {
  if(%this.debug) {
    echo("\c4>\c5" @ %line);
  }
}

function GlassNotificationManager::newNotification(%title, %text, %image, %sticky, %callback) {
  new ScriptObject(GlassNotification) {
    title = %title;
    text = %text;
    image = %image;

    sticky = %sticky;
    callback = %callback;
  };
}

function GlassNotification::onAdd(%this) {
  %swatch = new GuiSwatchCtrl(GlassNotificationSwatch) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 128";
    position = "0 0";
    extent = "200 50";

    notification = %this;
  };

  %swatch.image = new GuiBitmapCtrl(GlassDownloadSprite) {
    horizSizing = "center";
    vertSizing = "center";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %this.image @ ".png";
    position = "4 4";
    extent = "16 16";
    minextent = "0 0";
    clipToParent = true;
  };

  %swatch.text = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand-bold:15><just:left>" @ %this.title @ "<br><font:quicksand:13>" @ %this.text;
    position = "24 4";
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
  %swatch.verticalMatchChildren(5, 4);
  %swatch.mouse.extent = %swatch.extent;
  %swatch.image.centerY();

  %swatch.position = vectorAdd(getRes(), getWord(%swatch.extent, 0) SPC -getWord(%swatch.extent, 1)-10);

  %swatch.action = "in";
  %swatch.animate();
}

function GlassNotificationSwatch::animate(%this) {
  if(%this.sch)
    cancel(%this.sch);

  switch$(%this.action) {
    case "in":
      %this.position = vectorSub(%this.position, "5 0");
      if(getWord(%this.position, 0) < getWord(canvas.getExtent(), 0)-210) {
        %this.position = getWord(canvas.getExtent(), 0)-210 SPC getWord(%this.position, 1);
        %this.action = "hold";
        %this.sch = %this.schedule(5000, animate);
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
        %this.delete();
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
  if(%this.notification.callback !$= "") {
    if(strpos(%this.notification.callback, ";") == -1)
      call(%this.notification.callback, %this.notification);
    else
      eval(%this.notification.callback);
  }
  %this.swatch.action = "out";
  %this.swatch.animate();
}
