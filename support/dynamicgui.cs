//================================================
// Dynamic Gui Extension
//
// Jincux (9789)
//
// Just some functions to make the dynamic creation
// of GUI elements a little easier
//
//================================================

if($DynamicGui::Version > 1)
  return;

$DynamicGui::Version = 1;

// .setMargin (horizontal/[left], vertical/[right], [top], [bottom])
function GuiControl::setMargin(%this, %a, %b, %c, %d) {
  if(%c !$= "" && %d !$= "") {
    %left = %a;
    %right = %b;
    %top = %c;
    %bottom = %d;
  } else {
    %left = %a;
    %right = %a;
    %top = %b;
    %bottom = %b;
  }

  %width  = getWord(%this.extent, 0);
  %height = getWord(%this.extent, 1);
  %this.resize(%top, %left, %width, %height);
  //%this.position = %top SPC %left;
}

//resizes the object to be maximum size within the margin
function GuiControl::setMarginResize(%this, %a, %b, %c, %d) {
  if(%c !$= "" && %d !$= "") {
    %left = %a;
    %right = %b;
    %top = %c;
    %bottom = %d;
  } else {
    %left = %a;
    %right = %a;
    %top = %b;
    %bottom = %b;
  }

  %parent = %this.getGroup();

  if(%a !$= "") { //only resize if we have a margin
    %x = getWord(%parent.extent, 0)-%left-%right;
  } else {
    %x = getWord(%this.extent, 0);
  }

  if(%b !$= "") { //only resize if we have a margin
    %y = getWord(%parent.extent, 1)-%top-%bottom;
  } else {
    %y = getWord(%this.extent, 1);
  }

  //%this.position = %top SPC %left;
  //%this.extent = %x SPC %y;
  %this.resize(%top, %left, %x, %y);
}

//resizes the parent to be the size of the object+margins
function GuiControl::setMarginResizeParent(%this, %a, %b, %c, %d) {
  if(%c !$= "" && %d !$= "") {
    %left = %a;
    %right = %b;
    %top = %c;
    %bottom = %d;
  } else {
    %left = %a;
    %right = %a;
    %top = %b;
    %bottom = %b;
  }

  if(%a !$= "") { //only resize the parent if we have a margin
    %x = getWord(%this.extent, 0)+%left+%right;
  } else {
    %x = getWord(%this.getGroup().extent, 0);
  }

  if(%b !$= "") { //only resize the parent if we have a margin
    %y = getWord(%this.extent, 1)+%top+%bottom;
  } else {
    %y = getWord(%this.getGroup().extent, 1);
  }

  //%this.position = %top SPC %left;
  %width  = getWord(%this.extent, 0);
  %height = getWord(%this.extent, 1);
  %this.resize(%top, %left, %width, %height);

  %groupLeft = getWord(%this.getGroup().position, 0);
  %groupTop  = getWord(%this.getGroup().position, 1);
  //%this.getGroup().extent = %x SPC %y;
  %this.getGroup().resize(%groupLeft, %groupTop, %x, %y);
}

function GuiControl::forceCenter(%this) {
  %parent = %this.getGroup();

  %x = mFloor((getWord(%parent.extent, 0)-getWord(%this.extent, 0))/2);
  %y = mFloor((getWord(%parent.extent, 1)-getWord(%this.extent, 1))/2);

  %this.position = %x SPC %y;
}

function GuiControl::centerY(%this) {
  %parent = %this.getGroup();

  %x = getWord(%this.position, 0);
  %y = mFloor((getWord(%parent.extent, 1)-getWord(%this.extent, 1))/2);

  %this.position = %x SPC %y;
}

function GuiControl::centerX(%this) {
  %parent = %this.getGroup();

  %y = getWord(%this.position, 1);
  %x = mFloor((getWord(%parent.extent, 0)-getWord(%this.extent, 0))/2);

  %this.position = %x SPC %y;
}

function GuiControl::getCenter(%this) {
  %x = mFloor(getWord(%this.extent, 0)/2);
  %y = mFloor(getWord(%this.extent, 1)/2);
  return %x SPC %y;
}

function GuiControl::verticalMatchChildren(%this, %min, %pad) {
  for(%i = 0; %i < %this.getCount(); %i++) {
    if(!%this.getObject(%i).visible)
      continue;

    %low = getWord(vectorAdd(%this.getObject(%i).position, %this.getObject(%i).extent), 1);
    if(%low > %lowest) {
      %lowest = %low;
    }
  }

  if(%lowest+%pad > %min) {
    %this.extent = getWord(%this.extent, 0) SPC %lowest+%pad;
  } else {
    %this.extent = getWord(%this.extent, 0) SPC %min;
  }
}

function GuiControl::placeBelow(%this, %other, %margin) {
  %y = getWord(%other.position, 1)+getWord(%other.extent, 1)+%margin;
  %this.position = getWord(%this.position, 0) SPC %y;
}

function GuiControl::getCanvasPosition(%this) {
  %pos = %this.position;
  %parent = %this;
  while(isObject(%parent = %parent.getGroup())) {
    %pos = vectorAdd(%pos, %parent.position);
  }
  return %pos;
}

// **Courtesy of RTB 4.0**
//- GuiControl::getLowestPoint (finds the lowest point within a gui)

function GuiControl::getLowestPoint(%this) {
  %lowest = 0;
  for(%i=0;%i<%this.getCount();%i++) {
    %obj = %this.getObject(%i);
    %low = getWord(%obj.position,1) + getWord(%obj.extent,1);
    if(%low > %lowest)
      %lowest = %low;
  }
  return %lowest;
}
