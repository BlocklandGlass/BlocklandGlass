function GlassSnowflakes::doSnow(%parent, %amount) {
  if(!isObject(GlassSnowflakes)) {
    %x = getWord(getRes(), 0);
    %y = getWord(getRes(), 1);

    new ScriptObject(GlassSnowflakes) {
      container = %parent;
      count = %amount;
      width = %x;
      height = %y;
    };
  }
}

function GlassSnowflakes::newFlake(%this) {
  %big = getRandom(1, 10) == 1;
  %img = new GuiBitmapCtrl() {
    snowFlake = true;
    profile = "GuiDefaultProfile";
    extent = (%big ? "32 32" : "16 16");

    speed = getRandom(7, 20);
    position = getRandom(0, %this.width-32) SPC -32;

    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/" @ (%big ? "snow_big.png" : "snow.png");

    mcolor = "255 255 255 0";

    opacityRange = getRandom(50, 200);
  };
  %this.container.add(%img);
}

function GlassSnowflakes::tick(%this) {
  cancel(%this.sch);

  for(%i = 0; %i < %this.container.getCount(); %i++) {
    %obj = %this.container.getObject(%i);
    if(%obj.snowFlake) {
      %obj.position = vectorAdd(%obj.position, 0 SPC %obj.speed);
      if(getWord(%obj.position, 1) > %this.height+16) {
        %obj.delete();
        %i--;
      } else {
        %ct++;
        %dist = getWord(%obj.position, 1);
        %range = %obj.opacityRange;
        if(%dist < 0)
          %dist = 0;

        if(%dist < %range)
          %obj.mcolor = "255 255 255" SPC mfloor((255*%dist)/%range);
        else
          %obj.mcolor = "255 255 255" SPC 255;
      }
    }
  }

  if(%ct < %this.count) {
    if(getRandom(0, 1) == 1) {
      %this.newFlake();
    }
  }
  %this.sch = %this.schedule(50, tick);
}

package GlassSnow {
  function GlassOverlay::onWake(%this) {
    if(isObject(GlassSnowflakes)) {
      GlassSnowflakes.tick();
    }
    if(isFunction(%this, onWake))
      parent::onWake(%this);
  }

  function GlassOverlay::onSleep(%this) {
    if(isObject(GlassSnowflakes)) {
      cancel(GlassSnowflakes.sch);
    }
    if(isFunction(%this, onSleep))
      parent::onSleep(%this);
  }
};
activatePackage(GlassSnow);
