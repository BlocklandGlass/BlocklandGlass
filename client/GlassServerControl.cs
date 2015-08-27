function GlassServerControl::updatePrefs() {
  for(%i = 0; %i < GlassPrefs.getCount(); %i++) {
    %pref = GlassPrefs.getObject(%i);

    %pref.swatch.ctrl.setValue(%pref.value);
  }
}

function GlassServerControl::savePrefs() {
  commandToServer('GlassUpdateSend');
  for(%i = 0; %i < GlassPrefs.getCount(); %i++) {
    %pref = GlassPrefs.getObject(%i);

    if(%pref.swatch.ctrl.getValue() !$= %pref.value) {
      echo(%pref.title @ " was updated to " @ %pref.swatch.ctrl.getValue());

      commandToServer('GlassUpdatePref', %pref.idx, %pref.swatch.ctrl.getValue());
    }
  }
}

function GlassServerControl::renderPrefs() {
  GlassServerControl_PrefScroll.clear();
  %currentY = 1;
  for(%i = 0; %i < getWordCount(GlassPrefs.addons); %i++) {
    %addon = getWord(GlassPrefs.addons, %i);

    //create header
    %header = GlassServerControl::createHeader(%addon);
    %header.position = 0 SPC %currentY;
    GlassServerControl_PrefScroll.add(%header);
    %currentY += 25;

    for(%j = 0; %j < GlassPrefs.addonCount[%addon]; %j++) {
      %pref = GlassPrefs.addonItem[%addon SPC %j];

      switch$(%pref.type) {
        case "bool":
          %swatch = GlassServerControl::createCheckbox();
          %swatch.text.setText(%pref.title);
          %swatch.ctrl.setValue(%pref.value);

        case "int":
          %swatch = GlassServerControl::createInt();
          %swatch.text.setText(%pref.title);
          %swatch.ctrl.setValue(%pref.value);

        case "slider":
          %swatch = GlassServerControl::createSlider();
          %swatch.text.setText(%pref.title);
          %swatch.ctrl.setValue(%pref.value);

        case "text":
          %swatch = GlassServerControl::createText();
          %swatch.text.setText(%pref.title);
          %swatch.ctrl.setValue(%pref.value);
      }

      %swatch.position = 0 SPC %currentY;
      GlassServerControl_PrefScroll.add(%swatch);
      %pref.swatch = %swatch;
      %currentY += 33;
    }
  }

  GlassServerControl_PrefScroll.extent = getWord(GlassServerControl_PrefScroll.extent, 0) SPC %currentY;
  GlassServerControl_PrefScroll.getGroup().setVisible(true);
}

function GlassServerControl::createHeader(%text) {
  return new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 1";
     extent = "280 24";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 255";

     new GuiMLTextCtrl() {
        profile = "GuiMLTextProfile";
        horizSizing = "center";
        vertSizing = "center";
        position = "0 4";
        extent = "280 16";
        minExtent = "280 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lineSpacing = "2";
        allowColorChars = "0";
        maxChars = "-1";
        text = "<just:center><font:arial bold:16>" @ %text;
        maxBitmapHeight = "-1";
        selectable = "1";
        autoResize = "1";
     };
  };
}

function GlassServerControl::createCheckbox() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "280 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiCheckBoxCtrl() {
    profile = "GuiCheckBoxProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "257 8";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    groupNum = "-1";
    buttonType = "ToggleButton";
    text = "";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControl::createInt() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "280 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiTextEditCtrl() {
    profile = "GuiTextEditProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "245 7";
    extent = "30 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "0";
    sinkAllKeyEvents = "0";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControl::createSlider() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "280 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiSliderCtrl() {
    profile = "GuiSliderProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "175 6";
    extent = "100 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    range = "0.000000 1.000000";
    ticks = "10";
    value = "0.433333";
    snap = "0";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControl::createText() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "280 32";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "10 7";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiTextEditCtrl() {
    profile = "GuiTextEditProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "175 7";
    extent = "100 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    maxLength = "255";
    historySize = "0";
    password = "0";
    tabComplete = "0";
    sinkAllKeyEvents = "0";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}
