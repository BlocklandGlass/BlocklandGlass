function GlassDownloadInterface::init() {
  if(!isObject(GlassDownloadInterface)) {
    new ScriptGroup(GlassDownloadInterface);
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
    messageBoxOk("Error", "Download GUI open without context. Please file a bug report");
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
  GlassDownloadGui_Text.setValue("<font:quicksand:16>" @ %ctx.text);

  GlassDownloadInterface.currentContext = %ctx;

	%currentY = 0;
	GlassDownloadGui_Scroll.clear();
	for(%i = 0; %i < %ctx.getCount(); %i++) {
		%obj = %ctx.getObject(%i);

		%swat = GlassDownloadGui::buildSwatch(%obj);
    %obj.swatch = %swat;
    %swat.position = 1 SPC %currentY;
    GlassDownloadGui_Scroll.add(%swat);

		%currentY += 31;
	}
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
    position = "0 0";
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
    position = %textX SPC 7;
    extent = "280 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "<font:quicksand-bold:16>" @ %obj.text;
  };

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

  %gui.text = %text;
  %gui.add(%text);

  return %gui;
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
}

function testcontext() {

  %rtb = GlassDownloadInterface::openContext("RTB", "We've found updated versions of some of your old RTB add-ons!");
  %rtb.addDownload("Admin Chat", "bricks", "");
  %rtb.registerCallback("GlassRTBSupport::doUpdates");

}
