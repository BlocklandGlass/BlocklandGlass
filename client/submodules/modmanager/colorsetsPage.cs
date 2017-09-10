function GMM_ColorsetsPage::init() {
  new ScriptObject(GMM_ColorsetsPage);
}

function GMM_ColorsetsPage::open(%this) {
  GlassModManagerGui_MainDisplayScroll.vScrollBar = "alwaysOff";

  if(!isObject(%this.colorsets)) {
    GMM_ColorsetsPage.populateColorsets();
  }

  if(isObject(%this.container)) {
    GlassModManagerGui.schedule(0, pageDidLoad, %this);
    return %this.container;
  }

  %container = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "625 478";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 255";
  };

  %container.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "260 458";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
  };

  %container.scroll.list = new GuiSwatchCtrl(GMM_ColorsetsPage_List) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 1";
    extent = "249 456";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 255";
  };

  GMM_ColorsetsPage.renderColorsetList();

  %container.scroll.list.verticalMatchChildren(456, 10);

  %container.scroll.add(%container.scroll.list);
  %container.add(%container.scroll);

  %container.options = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "320 10";
    extent = "256 456";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "240 0 0 0";
  };

  %container.options.defaultButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    horizSizing = "center";
    vertSizing = "bottom";
    position = "77 10";
    extent = "100 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    command = "GMM_ColorsetsPage.defaultColorset();";
    text = "Default";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "255 255 255 128";
  };

  %container.options.applyButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonProfile";
    horizSizing = "center";
    vertSizing = "bottom";
    position = "77 425";
    extent = "100 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    command = "GMM_ColorsetsPage.applyColorset();";
    text = "Apply";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "120 220 120 128";
  };

  %container.options.preview = new GuiBitmapCtrl(GMM_ColorsetsPage_Preview) {
    profile = "GuiDefaultProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "0 100";
    extent = "256 256";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    bitmap = "Add-Ons/System_BlocklandGlass/image/trans.png";
    wrap = "1";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "1";
    keepCached = "1";
    mColor = "64 64 64 255";
    mMultiply = "1";
  };

  %container.options.add(%container.options.defaultButton);
  %container.options.add(%container.options.applyButton);
  %container.options.add(%container.options.preview);
  %container.add(%container.options);

  %this.container = %container;

  %this.renderPreview(GlassSettings.get("MM::Colorset"));

  GlassModManagerGui.schedule(0, pageDidLoad, %this);
  return %this.container;
}

function GMM_ColorsetsPage::close(%this) {
  GlassModManagerGui_MainDisplayScroll.vScrollBar = "alwaysOn";
  if(isObject(%this.container)) {
    %this.container.getGroup().remove(%this.container);
  }
}

function GMM_ColorsetsPage::renderColorsetList(%this, %swatch) {
  if(%swatch $= "") %swatch = GMM_ColorsetsPage_List;

  %swatch.deleteAll();

  for(%i = 0; %i < %this.colorsets.getCount(); %i++) {
    %data = %this.colorsets.getObject(%i);

    if(GlassSettings.get("MM::Colorset") $= ("Add-Ons/" @ %data.name @ "/colorSet.txt")) {
      %color = "84 217 140 255";
    } else {
      %color = "237 118 105 255";
    }

    %colorset = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "10 10";
      extent = "230 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = %color;
      ocolor = %color;
      index = %i;

      aid = %data.aid;
    };

    if(strlen(%data.name) > 18) {
      %name = getsubstr(%data.name, 0, 18) @ "...";
    } else {
      %name = %data.name;
    }

    %colorset.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "center";
      position = "10 8";
      extent = "429 14";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      text = "<font:Verdana Bold:15>" @ %name;
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
    };

    if(%data.isBLG) {
      %icon = "glassLogo";
    } else if(%data.isRTB) {
      %icon = "bricks";
    } else {
      %icon = "blLogo";
    }

    %colorset.icon = new GuiBitmapCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "left";
      vertSizing = "center";
      position = "207 7";
      extent = "16 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %icon;
      wrap = "1";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "1";
      keepCached = "1";
      mColor = "255 255 255 255";
      mMultiply = "0";
    };

    %colorset.add(%colorset.text);
    %colorset.add(%colorset.icon);

    GlassHighlightSwatch::addToSwatch(%colorset, "10 10 10", "GMM_ColorsetsPage.interact");

    %swatch.add(%colorset);
    if(%last)
      %colorset.placeBelow(%last, 2);

    %last = %colorset;
  }
}

function GMM_ColorsetsPage::interact(%this, %swatch, %pos) {
  if(getWord(%pos, 0) > getWord(%swatch.extent, 0)-20) {
    if(%swatch.aid !$= "") {
      GMM_Navigation.clear();
      GlassModManagerGui.openPage(GMM_AddonPage, %swatch.aid);
      return;
    }
  }

  %this.renderPreview(%this.colorsets.getObject(%swatch.index).file);

  if(%this.selectedColorset) {
    %this.selectedColorset.color = %this.selectedColorset.originalColor;
    %this.selectedColorset.hcolor = vectorAdd(%this.selectedColorset.originalColor, "10 10 10") SPC 255;
  }

  %swatch.originalColor = %swatch.ocolor;

  %swatch.color = vectorAdd("131 195 243", "10 10 10") SPC 255;
  %swatch.hcolor = vectorAdd("131 195 243", "10 10 10") SPC 255;
  %swatch.ocolor = "131 195 243 255";


  %this.selectedColorset = %swatch;
}

function GMM_ColorsetsPage::renderPreview(%this, %file) {
  %swatch = GMM_ColorsetsPage_Preview;
  %swatch.deleteAll();

  %fo = new FileObject();
  %fo.openforread(%file);

  %swatch.divs = 0;
  %swatch.divPointer = 0;

  while(!%fo.isEOF()) {
    %line = %fo.readLine();
    if(%line $= "") {
      continue;
    }

    if(strpos(%line, "DIV:") == 0) {
      %swatch.divCount[%swatch.divs] = %swatch.divPointer;
      %swatch.divs++;
      %swatch.divPointer = 0;
      continue;
    }

    if(strpos(getWord(%line, 0), ".") > 0) { //float color
      %r = mFloor(getword(%line, 0)*255);
      %g = mFloor(getword(%line, 1)*255);
      %b = mFloor(getword(%line, 2)*255);
      %a = mFloor(getword(%line, 3)*255);
      %swatch.color[%swatch.divs @ "_" @ %swatch.divPointer] = %r SPC %g SPC %b SPC %a;
      %swatch.divPointer++;
    } else {
      %swatch.color[%swatch.divs @ "_" @ %swatch.divPointer] = %line;
      %swatch.divPointer++;
    }
  }
  %fo.close();
  %fo.delete();

  %maxY = 8;
  %currentX = 8;
  %currentY = 8;
  for(%a = 0; %a < %swatch.divs; %a++) {
    for(%b = 0; %b < %swatch.divCount[%a]; %b++) {
      %tile = new GuiSwatchCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = %currentX SPC %currentY;
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        color = %swatch.color[%a @ "_" @ %b];
      };
      %swatch.add(%tile);
      %currentY += 16;
      if(%currentY > %maxY) {
        %maxY = %currentY;
      }
    }
    %currentX += 16;
    %currentY = 8;
  }

  %swatch.extent = %currentX+8 SPC %maxY+8;
  %swatch.forceCenter();
}

function GMM_ColorsetsPage::populateColorsets(%this) {
  if(isObject(%this.colorsets)) {
    %this.colorsets.deleteAll();
    %this.colorsets.delete();
  }

  %this.colorsets = new ScriptGroup();

  %pattern = "Add-Ons/*/colorSet.txt";
  while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-21);
    if(strpos(%name, "/") >= 0) { //removes sub-directories
      continue;
    }

    if(%name !$= "Colorset_Default" && getFileCRC(%file) == -1147879122) continue; //default colorset

    %so = new ScriptObject() {
      class = "GlassModManager_Colorset";
      name = %name;
      isRTB = isfile("Add-Ons/" @ %name @ "/rtbInfo.txt");
      isBLG = isfile("Add-Ons/" @ %name @ "/glass.json");

      file = %file;
    };

    if(%so.isBLG) {
      if(!jettisonReadFile("Add-Ons/" @ %name @ "/glass.json")) {
        %so.aid = ($JSON::Value).id;
        $JSON::Value.delete();
      } else {
        GlassLog::error("Error reading glass.json for " @ %name @ ": " @ $JSON::Error);
      }
    }

    %this.colorsets.add(%so);
  }
}

function GMM_ColorsetsPage::applyColorset(%this) {
  if(!isObject(%this.selectedColorset))
    return;

  %colorset = %this.colorsets.getObject(%this.selectedColorset.index);

  GlassSettings.update("MM::Colorset", %colorset.file);

  filecopy("config/server/colorset.txt", "config/server/colorset.old");
  filecopy_hack(GlassSettings.get("MM::Colorset"), "config/server/colorset.txt");

  %this.selectedColorset = "";
  %this.renderColorsetList();
  %this.renderPreview(GlassSettings.get("MM::Colorset"));
}

function GMM_ColorsetsPage::defaultColorset(%this) {
  if(!isObject(%this.selectedColorset))
    return;

  GlassSettings.update("MM::Colorset", "Add-Ons/Colorset_Default/colorSet.txt");

  filecopy("config/server/colorset.txt", "config/server/colorset.old");
  filecopy_hack(GlassSettings.get("MM::Colorset"), "config/server/colorset.txt");

  %this.selectedColorset = "";
  %this.renderColorsetList();
  %this.renderPreview(GlassSettings.get("MM::Colorset"));
}
