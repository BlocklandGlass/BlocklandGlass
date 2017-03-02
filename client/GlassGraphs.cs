function GlassGraphs::init() {
  new ScriptObject(GlassGraphs);
  GlassGraphs.populateTabs();
  GlassGraphs.sets = new SimSet(GlassGraphSets);

  %extent = GlassServerControlGui_Graph.getExtent();
  %width = getWord(%extent, 0);

  GlassGraphs.width = %width;

  GlassGraphs.testSet = GlassGraphSet::create("Bricks", "bricks");
}


function GlassGraphSet::create(%name, %icon, %color) {
  %obj = new ScriptObject() {
    class = "GlassGraphSet";

    name = %name;
    icon = %icon;
    color = %color;

    interval = 15;

    dataCt = 0;
  };

  GlassGraphSets.add(%obj);


  %extent = GlassServerControlGui_Graph.getExtent();
  %width = getWord(%extent, 0);

  for(%i = 0; %i < %width; %i++) {
    %min = mfloor(%i/4);
    %hr = 15;
    %hr += mfloor(%min/60);
    %min = %min%60;

    if(strlen(%min) == 1) {
      %min = "0" @ %min;
    }

    %sec = ((%i%4)*15);
    if(strlen(%sec) == 1) {
      %sec = "0" @ %sec;
    }

    %time = "03/02/17 " @ %hr @ ":" @ %min @ ":" @ %sec;

    %val = mPow(%i, 0.5);
    %obj.addData(%time, %val);
  }

  return %obj;
}

function GlassGraphSet::addData(%this, %time, %val) {
  %this.key[%this.dataCt] = %time;
  %this.val[%this.dataCt] = %val;
  %this.time[%time] = %val;

  %this.dataCt++;
}

function GlassGraphSet::newData(%this, %time, %val) {

}

function GlassGraphSet::orderDisplayData(%this, %time) {
  //%metricTime = getRealTime();
  if(%time $= "")
    %time = getDateTime();

  %lastTime = %time;

  if(%time $= "")
    %time = getDateTime();

  %idx = 0;
  %yr = getSubStr(%time, 6, 2);
  %mo = getSubStr(%time, 0, 2);
  %da = getSubStr(%time, 3, 2);

  %hr = getSubStr(%time, 9, 2);
  %mn = getSubStr(%time, 12, 2);
  %sc = %this.interval*mfloor(getSubStr(%time, 16, 2)/%this.interval);

  while(%idx <= GlassGraphs.width) {
    %sc -= %this.interval;
    if(%sc < 0) {
      %mn -= mabs(mfloor(%sc/60));
      %sc += mabs(mfloor(%sc/60))*60;
    }

    if(%mn < 0) {
      %hr -= mabs(mfloor(%mn/60));
      %mn += mabs(mfloor(%mn/60))*60;
    }

    if(%hr < 0) {
      warn("negative hour");
      return -1;
    }

    %timeStr = constructTime(%mo, %da, %yr, %hr, %mn, %sc);
    if(strlen(%this.time[%timeStr]) > 0) {
      //echo("found data at " @ %timeStr);
      %v = %this.time[%timeStr];
      %this.dispVal[%idx]  = %v;
      %this.dispTime[%idx] = %timeStr;

      if(%v > %max) {
        %max = %v;
      }
    } else {
      //echo("no data at " @ %timeStr);
      %this.dispVal[%idx]  = "";
      %this.dispTime[%idx] = "";
    }

    %idx++;
  }

  %this.dispCt = %idx;
  %this.dispScale = %max;
  //echo(getRealTime()-%metricTime);
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

    //GlassGraphs::populateRandom();
    GlassGraphs.displayGraph(GlassGraphs.testSet);

    GlassGraphs.active = %swatch;
  }
}

function GlassGraphMouse::onMouseMove(%this, %a, %pos) {
  %pos = vectorSub(%pos, %this.getCanvasPosition());
  %x = getWord(%pos, 0);

  if(%this.hoverBar) {
    %this.hoverBar.color = "84 217 140 255";
  }

  %bar = GlassGraphs.bar[GlassGraphs.width-%x];
  %bar.color = "46 204 113 255";
  %this.hoverBar = %bar;
}

function GlassGraphs::displayGraph(%this, %set) {
  GlassServerControlGui_Graph.deleteAll();
  %extent = GlassServerControlGui_Graph.getExtent();

  %set.orderDisplayData();


  %width = getWord(%extent, 0);
  %height = getWord(%extent, 1);

  if(%aniMode $= "")
    %aniMode = getRandom(0, 4);

  for(%i = 0; %i < %set.dispCt; %i++) {
    %val = %set.dispVal[%i];
    if(%val $= "")
      continue;

    %h = (%val/%set.dispScale)*%height;

    %bar = new GuiSwatchCtrl(GlassGraphBar) {
      extent = "1" SPC 0;
      position = (%width-%i-1) SPC %height;
      color = "84 217 140 255";
      minExtent = "1 5";

      val = %h;
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
