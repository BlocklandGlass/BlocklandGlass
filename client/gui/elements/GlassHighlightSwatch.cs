function GlassHighlightSwatch::addToSwatch(%swatch, %highlight, %command) {
  %swatch.hcolor = vectorAdd(%swatch.color, %highlight) SPC "255";

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
  %swatch.add(%swatch.glassHighlight);
  return %swatch.glassHighlight;
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
    %gui.pushToBack(%gui.flare);
  }

  %gui.flare.setVisible(true);
  %gui.flare.extent = "256 256";

  %gui.flare.position = vectorSub(%pos, "128 128");
  %gui.pushToBack(%this);
}

function GlassHighlightMouse::onMouseLeave(%this) {
  %this.down = false;
  if(!%this.enabled)
    return;

  if(isObject(%this.getGroup().flare))
    %this.getGroup().flare.setVisible(false);

  %this.getGroup().color = %this.getGroup().ocolor;

  if(%this.exitCommand !$= "") {
    eval(%this.exitCommand @ "(%this.getGroup().getId());");
  }
}

function GlassHighlightMouse::onMouseEnter(%this) {
  %this.down = false;
  if(!%this.enabled)
    return;

  %this.getGroup().ocolor = %this.getGroup().color;
  %this.getGroup().color = %this.getGroup().hcolor;

  if(%this.hoverCommand !$= "") {
    eval(%this.hoverCommand @ "(%this.getGroup().getId());");
  }
}


function GlassHighlightMouse::onMouseDown(%this) {
  %this.down = true;
  
  alxPlay(GlassClick1Audio);
}

function GlassHighlightMouse::onMouseUp(%this, %a, %pos) {
  %pos = vectorSub(%pos, %this.getCanvasPosition());

  if(%this.command !$= "" && %this.down) {
    eval(%this.command @ "(%this.getGroup().getId(), %pos);");
  }
  %this.down = false;
}
