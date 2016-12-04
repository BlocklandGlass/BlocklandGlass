function GMM_MyAddonsPage::init() {
  new ScriptObject(GMM_MyAddonsPage);
}

function GMM_MyAddonsPage::open(%this) {
  if(isObject(%this.container) && isObject(%this.container.body)) {
    %body = %this.container.body;
    %body.scroll.scrollToTop();
    %this.populateAddonList(%body.scroll.addonList);
    %body.scroll.addonList.verticalMatchChildren(456, 10);
    %body.scroll.addonList.setVisible(true);
    return %this.container;
  }

  %container = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    extent = "645 498";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 0";
  };

  %body = new GuiSwatchCtrl() {
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

  %body.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "370 458";
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

  %body.scroll.addonList = new GuiSwatchCtrl(GMM_MyAddonsPage_List) {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "1 1";
    extent = "359 456";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 255";
  };

  %this.populateAddonList(%body.scroll.addonList);
  %body.scroll.addonList.verticalMatchChildren(456, 10);
  %body.scroll.addonList.setVisible(true);

  %body.settings = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "430 10";
    extent = "145 500";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "240 240 240 0";

    new GuiBitmapButtonCtrl() {
      profile = "BlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 10";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GlassModManager_MyAddons::defaults();";
      text = "Defaults";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "85 172 238 128";
    };
    new GuiBitmapButtonCtrl() {
      profile = "BlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 45";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GlassModManager_MyAddons::enableAll();";
      text = "Enable All";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "255 255 255 128";
    };
    new GuiBitmapButtonCtrl() {
      profile = "BlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 80";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GlassModManager_MyAddons::disableAll();";
      text = "Disable All";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "255 255 255 128";
    };
    new GuiBitmapButtonCtrl() {
      profile = "BlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 430";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GlassModManager_MyAddons::apply();";
      text = "Apply";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "46 204 113 128";
    };
  };

  %body.scroll.add(%body.scroll.addonList);
  %body.add(%body.scroll);
  %body.add(%body.settings);
  %container.add(%body);

  %container.body = %body;
  %this.container = %container;

  GlassModManagerGui_MainDisplayScroll.vScrollBar = "alwaysOff";

  return %this.container;
}

function GMM_MyAddonsPage::close(%this) {
  if(isObject(%this.container)) {
    echo("is container!");
    %this.container.getGroup().remove(%this.container);
  } else {
    echo("No container!");
  }
  GlassModManagerGui_MainDisplayScroll.vScrollBar = "alwaysOn";
}


function GMM_MyAddonsPage::populateAddonList(%this, %swatch) {
  %swatch.deleteAll();

  for(%i = 0; %i < GlassModManager_MyAddons.getCount(); %i++) {
    %addon = GlassModManager_MyAddons.getObject(GlassModManager_MyAddons.getCount()-%i-1);

  	%enabled = ($AddOn["__" @ %addon.name] == 1);

    if(%enabled) {
      %color = "46 204 113 200";
    } else {
      %color = "237 118 105 200";
    }

    %text = "<font:Verdana Bold:15>" @ %addon.name;

    %gui = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "340 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = %color;
    };

    %gui.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "center";
      position = "30 7";
      extent = "281 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      text = %text;
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
    };
    %gui.add(%gui.text);

    %gui.checkbox = new GuiCheckBoxCtrl(GlassTempCheck) {
      addon = %addon.name;
      profile = "GuiCheckBoxProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "7 0";
      extent = "297 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      groupNum = "-1";
      buttonType = "ToggleButton";
      text = "";
    };
    %gui.add(%gui.checkbox);

    //blLogo
    //bricks
    //glassLogo

    %icon = "";
    if(isDefaultAddon(%addon)) {
      %icon = "blLogo";
    } else if(%addon.isBLG) {
      %icon = "glassLogo";
    } else if(%addon.isRTB) {
      %icon = "bricks";
    }

    if(%icon !$= "") {
      %gui.icon = new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "291 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = isDefaultAddon(%addon.name);
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
      %gui.add(%gui.icon);
    }

    if(%icon $= "glassLogo") {
      %gui.icon.redirect = new GuiMouseEventCtrl("GlassModManagerGui_AddonRedirect") {
        addon = %addon;
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "0 0";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lockMouse = "0";
      };
      %gui.icon.add(%gui.icon.redirect);
    }

    %gui.delete = new GuiBitmapCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "center";
      position = "312 7";
      extent = "16 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/cross.png";
      wrap = "0";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      keepCached = "0";
      mColor = "255 255 255 255";
      mMultiply = "0";

      new GuiMouseEventCtrl("GlassModManagerGui_AddonDelete") {
        addon = %addon;
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "0 0";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lockMouse = "0";
      };
    };
    %gui.add(%gui.delete);

    GlassTempCheck.setValue(%enabled);

    %swatch.add(%gui);

    if(%last)
      %gui.placeBelow(%last, 2);

    %last = %gui;
  }
}
