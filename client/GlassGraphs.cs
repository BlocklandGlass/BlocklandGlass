function GlassGraphs::init() {
  new ScriptObject(GlassGraphs) {
    barWidth = 1;
    barSpacing = 0;
  };

  GlassGraphs.populateTabs();
  GlassGraphs.sets = new SimSet(GlassGraphSets);

  %extent = GlassServerControlGui_Graph.getExtent();
  %width = getWord(%extent, 0);
  %height = getWord(%extent, 1);

  GlassGraphs.width = %width;
  GlassGraphs.height = %height;
}

function GlassGraphs::newGraph(%this, %id, %name, %icon, %color) {
  %this.graphName[%id] = %name;
  %this.graphIcon[%id] = %icon;
  %this.graphColor[%id] = %color;
  %this.graphs++;
}

function GlassGraphs::populateTabs(%this) {
  GlassServerControlGui_GraphTabs.deleteAll();

  %y = 0;
  for(%i = 0; %i < %this.graphs; %i++) {
    %swatch = %this.createTab(%i, %this.graphName[%i], %this.graphIcon[%i]);
    %swatch.position = 0 SPC %y;
    GlassServerControlGui_GraphTabs.add(%swatch);
    %y += 29;
  }
}

function GlassGraphs::createTab(%this, %id, %name, %icon) {
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
    graphId = %id;
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
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon;
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

    GlassGraphs.active = %swatch;
    GlassGraphs.activeId = %swatch.graphId;

    GlassGraphTitle.setText(GlassGraphs.graphName[%swatch.graphId]);

    GlassGraphs.displayGraph(%swatch.graphId);
  }
}

function GlassGraphMouse::onMouseLeave(%this) {
  if(isObject(%this.label))
    %this.label.visible = false;

  if(%this.hoverBar) {
    %this.hoverBar.color = "84 217 140 255";
  }
}

function GlassGraphMouse::onMouseMove(%this, %a, %pos) {
  %pos = vectorSub(%pos, %this.getCanvasPosition());
  %x = getWord(%pos, 0);

  if(%this.hoverBar) {
    %this.hoverBar.color = "84 217 140 255";
  }

  %bar = GlassGraphs.bar[%x - (%x % GlassGraphs.barWidth)];
  %bar.color = "46 204 113 255";
  %this.hoverBar = %bar;

  %labelPos = vectorAdd(%pos, %this.position);

  if(!isObject(%this.label)) {
    %this.label = GlassGraphs.createLabel();
    GlassServerControlGui_Graph.getGroup().add(%this.label);
  } else {
    %this.label.visible = true;
  }

  %this.label.position = %labelPos;

  %this.label.text.setValue("<font:verdana bold:12>" @ getWord(%bar.time, 1) @ "<br><font:verdana:12>" @ %bar.val);
  %this.label.text.forceReflow();
  %this.label.text.setMarginResizeParent(5, 5);

  %this.label.position = vectorSub(%this.label.position, %this.label.extent);

  if(getWord(%this.label.position, 0) < 0) {
    %this.label.position = setWord(%this.label.position, 0, 0);
  }


  if(getWord(%this.label.position, 1) < 0) {
    %this.label.position = setWord(%this.label.position, 1, 0);
  }

  %this.getGroup().pushToBack(%this);
}

function GlassGraphs::createLabel() {
  %label = new GuiSwatchCtrl() {
    extent = 110 SPC 25;
    position = %xPos SPC %height;
    color = "255 255 255 200";
    minExtent = "1 1";
  };

  %label.text = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "5 5";
    extent = "50 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "12";
    selectable = "1";
    autoResize = "1";
  };
  %label.add(%label.text);
  return %label;
}

function GlassGraphs::pushData(%this, %time, %val, %fresh, %pos) {
  %swatch = GlassServerControlGui_Graph;

  %width = getWord(%swatch.extent, 0);
  %height = getWord(%swatch.extent, 1);

  if(%pos < 0)
    return;

  if(%fresh) {
    for(%i = 0; %i < %swatch.getCount(); %i++) {
      %bar = %swatch.getObject(%i);

      %bar.position = vectorSub(%bar.position, %this.barWidth SPC "0");

      %idx = getWord(%bar.position, 0);
      if((%idx+%this.barWidth) < 0) {
        %bar.delete();
        %i--;
      }

      %this.bar[%idx] = %bar;
      if(%bar.val > %max) {
        %max = %bar.val;
      }
    }

    %this.scale = %max;

    %xPos = %width-(%this.barWidth);
  } else {
    %xPos = %width-(%this.barWidth*(%pos));
  }

  if(%val > %this.scale) {
    %this.scale = %val;
  }
  %scale = %this.scale;

  for(%i = 0; %i < %swatch.getCount(); %i++) {
    %bar = %swatch.getObject(%i);
    %h = mFloor((%bar.val/%scale)*%height) + (%scale == 0 ? 5 : 0);
    if(%h != %bar.height) {
      %bar.height = %h;
      %bar.startNewAnim(%bar.height, 1000);
    }
  }

  %bar = new GuiSwatchCtrl(GlassGraphBar) {
    extent = %this.barWidth SPC 0;
    position = %xPos SPC %height;
    color = "84 217 140 255";
    //color = "255 0 0 255";
    minExtent = "1 1";

    time = %time;
    val = %val;
    height = (%val/%scale)*%height;

    animTime = 1000;
    elapsed = 0;

    pause = !%fresh;
  };
  %this.bar[%xPos] = %bar;

  %swatch.add(%bar);
  %bar.startNewAnim(%bar.height, 1000);
}

function GlassGraphs::displayGraph(%this, %id) {
  commandToServer('GlassGraphRequest', %id, mceil(%this.width/%this.barWidth)+1);
}

function GlassGraphBar::startNewAnim(%this, %height, %time) {
  %this.animStartHeight = getWord(%this.extent, 1);
  %this.animBottomPos   = getWord(%this.position, 1)+getWord(%this.extent, 1);

  %this.animEndHeight   = %height;
  %this.animTime        = %time;

  %this.elapsed         = 0;

  cancel(%this.sch);
  %this.sch = %this.schedule(33, tick);
}

function GlassGraphBar::tick(%this) {
  cancel(%this.sch);

  if(!%this.pause) {
    %this.elapsed += 33;
  }

  if(%this.elapsed >= %this.animTime) {
    %this.elapsed = %this.animTime;
    %this.color = "84 217 140 255";
  }

  if(%this.elapsed > 0) {
    if(%this.animTime > %this.elapsed) {
      //logistic growth woo!
      //guess my math minor IS useful
      %error = 0.01/%this.animTime;
      %k = mlog((1/%error) - 1) / (%this.animTime/2);
      %e = 2.71828;
      %ratio = 1 / (1 + mpow(%e, -%k*(%this.elapsed-(%this.animTime/2))));
    } else {
      %ratio = 1;
    }

    %height = round(%ratio * (%this.animEndHeight - %this.animStartHeight)) + %this.animStartHeight;

    %this.extent = GlassGraphs.barWidth SPC %height;
    %this.position = getWord(%this.position, 0) SPC (GlassGraphs.height-%height);
  }

  if(%this.elapsed < %this.animTime) {
    %this.sch = %this.schedule(33, tick);
  }
}


function constructTime(%mo, %da, %yr, %hr, %mn, %sc) {

  if(strlen(%mo) == 1) {
    %mo = "0" @ %mo;
  }
  if(strlen(%da) == 1) {
    %da = "0" @ %da;
  }
  if(strlen(%yr) == 1) {
    %yr = "0" @ %yr;
  }

  if(strlen(%hr) == 1) {
    %hr = "0" @ %hr;
  }
  if(strlen(%mn) == 1) {
    %mn = "0" @ %mn;
  }
  if(strlen(%sc) == 1) {
    %sc = "0" @ %sc;
  }

  return %mo @ "/" @ %da @ "/" @ %yr SPC %hr @ ":" @ %mn @ ":" @ %sc;
}

function strTimeCompare(%datetime1, %datetime2) {
  %month[%a = 1] = 31; //jan
  %month[%a++]   = 28; //feb
  %month[%a++]   = 31; //march
  %month[%a++]   = 30; //april
  %month[%a++]   = 31; //may
  %month[%a++]   = 30; //june
  %month[%a++]   = 31; //july
  %month[%a++]   = 30; //august
  %month[%a++]   = 31; //sept
  %month[%a++]   = 30; //nov
  %month[%a++]   = 31; //dec

  %date1 = getWord(%datetime1, 0);
  %date2 = getWord(%datetime2, 0);

  %time1 = getWord(%datetime1, 1);
  %time2 = getWord(%datetime2, 1);

  %diff = 0;

  //seconds
  %s1 = getSubStr(%time1, 6, 2);
  %s2 = getSubStr(%time2, 6, 2);
  %diff += (%s1-%s2);

  //minutes
  %m1 = getSubStr(%time1, 3, 2);
  %m2 = getSubStr(%time2, 3, 2);
  %diff += (%m1-%m2)*60;

  //hours
  %h1 = getSubStr(%time1, 0, 2);
  %h2 = getSubStr(%time2, 0, 2);
  %diff += (%h1-%h2)*3600;

  //we'll adjust days to be from the beginning of the year
  //days
  %mo1 = getSubStr(%date1, 0, 2);
  %mo2 = getSubStr(%date2, 0, 2);
  %d1 = getSubStr(%date1, 3, 2);
  %d2 = getSubStr(%date2, 3, 2);
  %y1 = getSubStr(%date1, 6, 2);
  %y2 = getSubStr(%date2, 6, 2);

  if(mabs(%y1-%y2) > 1) {
    error("strTimeCompare given difference of > 1 year!");
    return "";
  }

  if(%y1 > %y2) {
    %d2 -= (%y2 % 4 == 0) ? 366 : 365;
  } else if(%y1 < %y2) {
    %d1 -= (%y1 % 4 == 0) ? 366 : 365;
  }

  for(%i = 1; %i < %mo1; %i++) {
    %d1 += %month[%i];

    if(%i == 2 && (%y1 % 4) == 0) {
      //leap year
      %d1 += 1;
    }
  }

  for(%i = 1; %i < %mo2; %i++) {
    %d2 += %month[%i];

    if(%i == 2 && %y2 % 4 == 0) {
      //leap year
      %d2 += 1;
    }
  }

  %diff += (%d1-%d2)*(24*60*60);


  return %diff;
}


//cannot add more than one month
function datetimeAdd(%time, %diff) {
  %month[%a = 1] = 31; //jan
  %month[%a++]   = 28; //feb
  %month[%a++]   = 31; //march
  %month[%a++]   = 30; //april
  %month[%a++]   = 31; //may
  %month[%a++]   = 30; //june
  %month[%a++]   = 31; //july
  %month[%a++]   = 30; //august
  %month[%a++]   = 31; //sept
  %month[%a++]   = 30; //nov
  %month[%a++]   = 31; //dec

  %yr = getSubStr(%time, 6, 2);
  %mo = getSubStr(%time, 0, 2);
  %da = getSubStr(%time, 3, 2);

  %hr = getSubStr(%time, 9, 2);
  %mn = getSubStr(%time, 12, 2);
  %sc = getSubStr(%time, 16, 2);

  %sc += %diff % 60;
  %mn += mfloor(%diff/60);
  %hr += mfloor(%diff/3600);
  %da += mfloor(%diff/(3600*24));

  if(%sc >= 60) {
    %mn--;
    %sc -= 60;
  }

  if(%mn >= 60) {
    %hr++;
    %mn -= 60;
  }

  if(%hr >= 24) {
    %da++;
    %hr -= 60;
  }

  %daysInMonth = %month[%mo+0];
  if(%yr % 4 == 0 && %mo == 2) {
    %daysInMonth++;
  }

  if(%da > %daysInMonth) {
    %da = %da % %daysInMonth;
    %mo++;
  }

  if(%mo > 12) {
    %mo = %mo % 12;
    %yr++;
  }

  return constructTime(%mo, %da, %yr, %hr, %mn, %sc);
}


//================================================================
//= Communications                                               =
//================================================================

function clientCmdGlassGraphsClear() {
  GlassGraphs.graphs = 0;
}

function clientCmdGlassGraphAdd(%id, %name, %icon) {
  GlassGraphs.newGraph(%id, %name, %icon);
}

function clientCmdGlassGraphAddDone() {
  GlassGraphs.populateTabs();
}

function clientCmdGlassGraphClearData() {
  GlassServerControlGui_Graph.deleteAll();
  GlassGraphs.scale = 0;
}

function clientCmdGlassGraphData(%id, %time, %value, %fresh, %offset) {
  if(GlassGraphs.activeId != %id)
    return;

  GlassGraphs.pushData(%time, %value, %fresh, %offset);
}

function clientCmdGlassGraphDataDone() {
  for(%i = 0; %i < GlassServerControlGui_Graph.getCount(); %i++) {
    GlassServerControlGui_Graph.getObject(%i).pause = false;
    GlassServerControlGui_Graph.getObject(%i).tick();
  }
}
