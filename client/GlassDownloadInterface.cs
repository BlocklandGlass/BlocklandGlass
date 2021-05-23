function GlassDownloadInterface::init() {
  if(!isObject(GlassDownloadInterface)) {
    GlassGroup.add(new ScriptGroup(GlassDownloadInterface));
  }
}

//================================================================
// interaction
//================================================================

function GlassDownloadGui::onAccept(%this) {
  %ctx = GlassDownloadInterface.currentContext;
  if(isObject(%ctx)) {
    for(%i = 0; %i < %ctx.callbacks; %i++) {
      eval(%ctx.callback[%i] @ "(1);");
    }
  } else {
    glassMessageBoxOk("Error", "Download GUI open without context. Please file a bug report.");
  }
}

function GlassDownloadGui::onDecline(%this) {
  %ctx = GlassDownloadInterface.currentContext;
  if(isObject(%ctx)) {
    for(%i = 0; %i < %ctx.callbacks; %i++) {
      eval(%ctx.callback[%i] @ "(-1);");
    }

    if(!%ctx.inhibitClose) {
      if(GlassDownloadInterface.getCount() > 1) {
        %ctx.delete();
        GlassDownloadGui.loadContext(GlassDownloadInterface.getObject(0));
      } else {
        %ctx.delete();
        canvas.popDialog(GlassDownloadGui);
      }
    }
  } else {
    canvas.popDialog(GlassDownloadGui);
  }
}

function GlassDownloadGui::onDone(%this) {
  %ctx = GlassDownloadInterface.currentContext;
  for(%i = 0; %i < %ctx.callbacks; %i++) {
    eval(%ctx.callback[%i] @ "(2);");
  }

  if(GlassDownloadInterface.getCount() > 1) {
    %ctx.delete();
    GlassDownloadGui.loadContext(GlassDownloadInterface.getObject(0));
  } else {
    canvas.popDialog(GlassDownloadGui);
  }
}

function GlassDownloadGui::loadContext(%this, %ctx) {
  %this.getObject(0).setText(%ctx.title);
  GlassDownloadGui_Text.setValue("<font:verdana:15>" @ %ctx.text);

  GlassDownloadInterface.currentContext = %ctx;

	%currentY = 1;
  if(isObject(GlassDownloadGui_ScrollSwatch))
    GlassDownloadGui_ScrollSwatch.deleteAll();
  else
    new GuiSwatchCtrl(GlassDownloadGui_ScrollSwatch) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "1 1";
      extent = GlassDownloadGui_Scroll.extent;
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "0 0 0 0";
    };

	for(%i = 0; %i < %ctx.getCount(); %i++) {
		%obj = %ctx.getObject(%i);

		%swat = GlassDownloadGui::buildSwatch(%obj);
    %obj.swatch = %swat;
    %swat.position = 1 SPC %currentY;
    GlassDownloadGui_ScrollSwatch.add(%swat);

		%currentY += 31;
	}

  GlassDownloadGui_ScrollSwatch.extent = getWord(GlassDownloadGui_ScrollSwatch.extent, 0) SPC %currentY;
  GlassDownloadGui_Scroll.clear();
  GlassDownloadGui_Scroll.add(GlassDownloadGui_ScrollSwatch);
}

function GlassDownloadGui::buildSwatch(%obj) {
  if(%obj.image !$= "") {
    %textX = 28;
  } else {
    %textX = 7;
  }

  %gui = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 1";
    extent = "280 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "200 220 200 255";
  };
  %text = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = %textX SPC 8;
    extent = "280 14";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "<font:verdana bold:15>" @ %obj.text;
  };

  %gui.text = %text;
  %gui.add(%text);

  if(%obj.image !$= "") {
    %img = new GuiBitmapCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "center";
      position = "7 7";
      extent = "16 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %obj.image @ ".png";
      wrap = "0";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      keepCached = "0";
      mColor = "255 255 255 255";
      mMultiply = "0";
    };
    %gui.img = %img;
    %gui.add(%img);
  }

  if(%obj.callback !$= "") {
    %mouse = new GuiMouseEventCtrl(GlassDownloadGui_Mouse) {
      downloadObj = %obj;
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "0 0";
      extent = "280 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lockMouse = "0";
    };
    %gui.mouse = %mouse;
    %gui.add(%mouse);
  }

  return %gui;
}

function GlassDownloadGui_Mouse::onMouseEnter(%this) {
  %gs = %this.getGroup();
  %gs.color = vectorAdd(%gs.color, "50 30 50") SPC 255;
}

function GlassDownloadGui_Mouse::onMouseLeave(%this) {
  %gs = %this.getGroup();
  %gs.color = vectorAdd(%gs.color, "-50 -30 -50") SPC 255;
}

function GlassDownloadGui_Mouse::onMouseDown(%this, %a, %b, %c) {
  eval(%this.downloadObj.callback @ "(%this.downloadObj,0);");

}

function GlassDownloadGui_Mouse::onRightMouseUp(%this) {
  eval(%this.downloadObj.callback @ "(%this.downloadObj,1);");
}

//================================================================
// api
//================================================================

function GlassDownloadInterface::openContext(%title, %text, %configurable) {
  %ctx = new ScriptGroup() {
    title = %title;
    text = %text;

    canDelete = %configurable;

    class = "GlassDownloadInterfaceContext";
  };

  GlassDownloadInterface.add(%ctx);

  if(GlassDownloadInterface.getCount() == 1) {
    GlassDownloadGui.schedule(1, loadContext, %ctx);
    canvas.pushDialog(GlassDownloadGui);
  }

  return %ctx;
}

function GlassDownloadInterfaceContext::inhibitClose(%this, %bool) {
  %this.inhibitClose = %bool;
}

function GlassDownloadInterfaceContext::addDownload(%this, %text, %image, %callback) {
  %obj = new ScriptObject() {
    class = "GlassDownloadInterfaceObject";
    text = %text;
    image = %image;
    callback = %callback;
    ctx = %this;
  };
  %this.add(%obj);
  return %obj;
}

function GlassDownloadInterfaceContext::registerCallback(%this, %callback) {
  %this.callback[%this.callbacks+0] = %callback;
  %this.callbacks++;
}

function GlassDownloadInterfaceObject::setProgress(%this, %value) {
  %swatch = %this.swatch;
  if(!isObject(%swatch.progress)) {
    %swatch.progress = new GuiProgressCtrl() {
      profile = "GuiProgressProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "5 5";
      extent = "265 20";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
    };
    %swatch.deleteAll();
    %swatch.add(%swatch.progress);
  }

  %swatch.progress.setValue(%value);

  if(%value == 1) {
    %this.done = true;
    for(%i = 0; %i < %this.ctx.getCount(); %i++) {
      if(!%this.ctx.getObject(%i).done)
        return;
    }

    GlassDownloadGui.onDone();
  }
}

function GlassDownloadInterfaceObject::cancelDownload(%this) {
  if(%this.ctx == GlassDownloadInterface.currentContext) {
    GlassDownloadGui.schedule(0, loadContext, %this.ctx);
  }
  %this.delete();
}

function testcontext() {

  %rtb = GlassDownloadInterface::openContext("RTB", "We've found updated versions of some of your old RTB add-ons!");
  %rtb.addDownload("Admin Chat", "bricks", "");
  %rtb.registerCallback("GlassRTBSupport::doUpdates");

}
