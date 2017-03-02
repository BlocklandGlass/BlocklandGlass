function GlassGraphs::init() {
  new ScriptObject(GlassGraphs);
  GlassGraphs.populateTabs();
}

function GlassGraphs::populateTabs(%this) {
  GlassServerControlGui_GraphTabs.deleteAll();

  %swatch = %this.createTab("Bricks", "bricks");
  %swatch.position = "0 0";
  GlassServerControlGui_GraphTabs.add(%swatch);

  %swatch = %this.createTab("Players", "user");
  %swatch.position = "0 29";
  GlassServerControlGui_GraphTabs.add(%swatch);

  %swatch = %this.createTab("Memory", "server");
  %swatch.position = "0 58";
  GlassServerControlGui_GraphTabs.add(%swatch);
}

function GlassGraphs::createTab(%this, %name, %icon) {
  %swatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 39";
    extent = "80 24";
    minExtent = "1 1";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 100";
  };

  %swatch.bitmap = new GuiBitmapCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "4 4";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    bitmap = "~/System_BlocklandGlass/image/icon/" @ %icon;
    wrap = "0";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    keepCached = "0";
    mColor = "255 255 255 255";
    mMultiply = "0";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "24 4";
    extent = "49 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = %name;
    maxLength = "255";
  };

  %swatch.mouse = new GuiMouseEventCtrl(GlassGraphTabMouse) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = "135 24";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";
    swatch = %swatch;
  };

  %swatch.add(%swatch.bitmap);
  %swatch.add(%swatch.text);
  %swatch.add(%swatch.mouse);

  return %swatch;

}

function GlassGraphTabMouse::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%swatch.active)
    return;

  %swatch.extent = "85 24";
  %swatch.down = false;
}

function GlassGraphTabMouse::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%swatch.active)
    return;

  %swatch.extent = "80 24";
  %swatch.down = false;
  %swatch.color = "255 255 255 100";
}

function GlassGraphTabMouse::onMouseDown(%this) {
  %swatch = %this.swatch;
  if(%swatch.active)
    return;

  %swatch.down = true;
  %swatch.color = "131 195 243 100";
}

function GlassGraphTabMouse::onMouseUp(%this) {
  %swatch = %this.swatch;
  if(%swatch.active)
    return;

  if(%swatch.down) {
    %swatch.down = false;
    %swatch.color = "255 255 255 200";
    %swatch.active = true;

    if(isObject(GlassGraphs.active)) {
      %active = GlassGraphs.active;
      %active.color = "255 255 255 100";
      %active.extent = "80 24";
      %active.active = false;
    }

    GlassGraphs::populateRandom();

    GlassGraphs.active = %swatch;
  }
}

function GlassGraphs::populateRandom(%aniMode) {
  GlassServerControlGui_Graph.deleteAll();
  %extent = GlassServerControlGui_Graph.getExtent();
  %width = getWord(%extent, 0);
  %height = getWord(%extent, 1);

  if(%aniMode $= "")
    %aniMode = getRandom(0, 4);

  for(%i = 0; %i < %width; %i++) {

    %val = getRandom(5, %height-25);
    %bar = new GuiSwatchCtrl(GlassGraphBar) {
      extent = "1" SPC 0;
      position = %i SPC (%height-%val);
      color = "84 217 140 255";
      minExtent = "1 5";

      val = %val;
      maxHeight = %height;
    };

    switch(%aniMode) {
      case 0:
        %bar.animateTime = 5*(%i);
        %bar.elapsed = -5*(%width-%i);

      case 1:
        %bar.animateTime = 100+(%i);
        %bar.elapsed = 0;

      case 2:
        %bar.animateTime = 500;
        %bar.elapsed = 0;

      case 3:
        %bar.animateTime = 5*(%width-%i);
        %bar.elapsed = -5*(%i);

      case 4:
        %bar.animateTime = 500*mlog(%width/(%i+1));
        %bar.elapsed = (%width-%i)*-5;
    }

    GlassServerControlGui_Graph.add(%bar);
    %this.bar[%i] = %bar;
  }
}

function GlassGraphBar::onAdd(%this) {
  if(%this.sch $= "") {
    %this.sch = %this.schedule(33, tick);
  }
}

function GlassGraphBar::tick(%this) {
  cancel(%this.sch);

  %this.elapsed += 33;
  if(%this.elapsed >= %this.animateTime) {
    %this.elapsed = %this.animateTime;
  }

  if(%this.elapsed > 0) {
    %height = mFloor((%this.elapsed/%this.animateTime)*%this.val);
    %this.extent = 1 SPC %height;
    %this.position = getWord(%this.position, 0) SPC %this.maxHeight-%height;
  }

  if(%this.elapsed < %this.animateTime) {
    %this.sch = %this.schedule(33, tick);
  }
}
