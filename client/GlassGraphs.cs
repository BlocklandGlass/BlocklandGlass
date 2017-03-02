function GlassGraphs::populateRandom(%aniMode) {
  GlassServerControlGui_Graph.deleteAll();
  %extent = GlassServerControlGui_Graph.getExtent();
  %width = getWord(%extent, 0);
  %height = getWord(%extent, 1);

  if(%aniMode $= "")
    %aniMode = getRandom(0, 4);

  for(%i = 0; %i < %width; %i++) {

    %val = getRandom(5, %height/2);
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
