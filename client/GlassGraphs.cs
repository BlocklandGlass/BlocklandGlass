function GlassGraphs::populateRandom() {
  GlassServerControlGui_Graph.deleteAll();
  %extent = GlassServerControlGui_Graph.getExtent();
  %width = getWord(%extent, 0);
  %height = getWord(%extent, 1);
  for(%i = 0; %i < %width; %i++) {
    
    %val = getRandom(5, %height/2);
    %bar = new GuiSwatchCtrl() {
      extent = "1" SPC %val;
      position = %i SPC (%height-%val);
      color = "84 217 140 255";
      minExtent = "1 5";
    };

    GlassServerControlGui_Graph.add(%bar);
  }
}
