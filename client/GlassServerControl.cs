function GlassServerControlC::init() {
  new ScriptObject(GlassServerControlC) {

  };
}

$remapDivision[$remapCount] = "Blockland Glass";
   $remapName[$remapCount] = "Server Settings";
   $remapCmd[$remapCount] = "openGlassSettings";
   $remapCount++;

function openGlassSettings() {
  if(GlassServerControlC.enabled) {
    canvas.pushDialog(GlassServerControlGui);
  }
}


function GlassServerControlC::setTab(%tab) {
  for(%i = 0; %i < 2; %i++) {
    %ctrl = "GlassServerControlGui_Pane" @ %i+1;
    if(%i+1 == %tab) {
      %ctrl.setVisible(true);
    } else {
      %ctrl.setVisible(false);
    }
  }
}

function GlassServerControlC::valueUpdate(%obj) {
  %pref = %obj.pref;
  %type = %pref.type;
  %parm = %pref.params;

  if(%type $= "number") {
    if(%parm !$= "") {
      %val = %obj.ctrl.getValue();

      %min = getWord(%parm, 0);
      %max = getWord(%parm, 1);
      %decimal = getWord(%parm, 2);

      if(%val !$= "") {
        if(%val < %min) {
          %obj.ctrl.setValue(%min);
        } else if(%val > %max) {
          %obj.ctrl.setValue(%max);
        }

        if(%decimal !$= "") {
          if(strpos(%val, ".") != -1) {
            if(strlen(%val) - strpos(%val, ".") > %decimal) {
              %newval = getSubStr(%val, 0, strpos(%val, ".")+%decimal+1);
              %obj.ctrl.setValue(%newval);
            }

            if(%decimal == 0) {
              %newval = getSubStr(%val, 0, strpos(%val, "."));
              %obj.ctrl.setValue(%newval);
            }
          }
        }
      }
    }
  } else if(%type $= "text" || %type $= "textarea") {
    if(%parm !$= "") {
      if(strlen(%obj.ctrl.getValue()) > %parm) {
        %obj.ctrl.setValue(getsubstr(%obj.ctrl.getValue(), 0, %parm));
      }
    }
  } else {

  }

  %pref.value = %obj.ctrl.getValue();
}

function GlassServerControlC::renderPrefCategories() {
  GlassServerControlGui_Prefs_Categories.clear();
  %odd = false;
  %y = 0;
  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %odd = !%odd;
    if(%odd) {
      %color = "220 250 220 255";
    } else {
      %color = "220 230 220 255";
    }
    %cat = GlassPrefGroup.getObject(%i);

    %swat = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = 0 SPC %y;
      extent = "125 24";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = %color;
      ocolor = %color;
      rcolor = %color;

      new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "4 4";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %cat.icon;
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";
      };
      new GuiTextCtrl() {
        profile = "GuiTextProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "24 3";
        extent = "38 18";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        text = %cat.name;
        maxLength = "255";
      };
    };
    %swat.mouse = new GuiMouseEventCtrl(GlassServerControlGui_CatMouseCtrl) {
      category = %cat;
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "0 0";
      extent = "125 24";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lockMouse = "0";
    };

    %cat.swatch = %swat;
    %swat.add(%swat.mouse);

    GlassServerControlGui_Prefs_Categories.add(%swat);

    %y += 24;
  }

  if(%y > 233) {
    echo(%y);
    GlassServerControlGui_Prefs_Categories.extent = getWord(GlassServerControlGui_Prefs_Categories.extent, 0) SPC %y;
  } else {
    GlassServerControlGui_Prefs_Categories.extent = getWord(GlassServerControlGui_Prefs_Categories.extent, 0) SPC 233;
  }
  echo(GlassServerControlGui_Prefs_Categories.extent);
  GlassServerControlGui_Prefs_Categories.getGroup().scrollToTop();
  GlassServerControlGui_Prefs_Categories.setVisible(true);
}

function GlassServerControlGui_CatMouseCtrl::onMouseEnter(%this) {
  %swatch = %this.getGroup();
  %swatch.color = "255 255 255 255";
}

function GlassServerControlGui_CatMouseCtrl::onMouseLeave(%this) {
  %swatch = %this.getGroup();
  %swatch.color = %swatch.ocolor;
}

function GlassServerControlGui_CatMouseCtrl::onMouseDown(%this, %down) {
  for(%i = 0; %i < GlassServerControlGui_Prefs_Categories.getCount(); %i++) {
    %s = GlassServerControlGui_Prefs_Categories.getObject(%i);
    %s.color = %s.ocolor = %s.rcolor;
  }

  %swatch = %this.getGroup();
  %swatch.color = %swatch.ocolor = "170 200 255 255";

  GlassServerControlC::renderPrefCategory(%this.category);
}

function GlassServerControlC::renderPrefs() {
  GlassServerControlC::renderPrefCategory(GlassPrefGroup.getObject(0));
}

function GlassServerControlC::renderPrefCategory(%category) {
  GlassServerControl_PrefScroll.clear();
  %currentY = 0;

  //create header
  %header = GlassServerControlC::createHeader(%category.name);
  %header.position = 0 SPC %currentY;
  GlassServerControl_PrefScroll.add(%header);
  %currentY += 25;

  for(%j = 0; %j < %category.getCount(); %j++) {
    %pref = %category.getObject(%j);
    %swatch = "";
    switch$(%pref.type) {
      case "boolean":
        %swatch = GlassServerControlC::createCheckbox();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);

      case "number":
        %swatch = GlassServerControlC::createInt();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);

      case "slider":
        %swatch = GlassServerControlC::createSlider();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);
        %swatch.ctrl.range = %pref.parm;

      case "string":
        %swatch = GlassServerControlC::createText();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(expandEscape(%pref.value));

      case "textarea":
        %swatch = GlassServerControlC::createTextArea();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);
    }

    if(!isObject(%swatch)) {
      warn("Failed to make pref of type \"" @ %pref.type @ "\"");
      continue;
    }

    %swatch.ctrl.command = "GlassServerControlC::valueUpdate(" @ %swatch.getId() @ ");";
    %swatch.position = 0 SPC %currentY;
    GlassServerControl_PrefScroll.add(%swatch);

    %pref.swatch = %swatch;
    %swatch.pref = %pref;

    if(%pref.type !$= "textarea") {
      %currentY += 33;
    } else {
      %currentY += 129;
    }
  }

  GlassServerControl_PrefScroll.extent = getWord(GlassServerControl_PrefScroll.extent, 0) SPC %currentY;
  GlassServerControl_PrefScroll.getGroup().setVisible(true);
  GlassServerControl_PrefScroll.setVisible(true);
}

function GlassServerControlC::createHeader(%text) {
  return new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 1";
     extent = "325 24";
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
        extent = "325 16";
        minExtent = "325 2";
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

function GlassServerControlC::createTextArea() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "325 128";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "100 100 100 50";
  };

  %swatch.text = new GuiTextCtrl() {
    profile = "GuiTextProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 4";
    extent = "77 18";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Some checkbox";
    maxLength = "255";
  };

  %swat2 = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "5 25";
    extent = "270 98";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "255 255 255 200";
  };

  %swatch.ctrl = new GuiMLTextEditCtrl() {
    profile = "GuiMLTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "5 5";
    extent = "260 93";
    minExtent = "260 93";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "-1";
    selectable = "1";
    autoResize = "1";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swat2);
  %swat2.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createCheckbox() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "325 32";
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
    text = "";
    maxLength = "255";
  };

  %swatch.ctrl = new GuiCheckBoxCtrl() {
    profile = "GuiCheckBoxProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "303 8";
    extent = "16 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    groupNum = "-1";
    buttonType = "ToggleButton";
    text = " ";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
  return %swatch;
}

function GlassServerControlC::createInt() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "325 32";
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
    extent = "70 18";
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

function GlassServerControlC::createSlider() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "325 32";
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
    extent = "140 20";
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

function GlassServerControlC::createText() {
  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "1 25";
     extent = "325 32";
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
    extent = "140 18";
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

function GlassServerControlC::onSelect(%this) {
  %list = GlassServerControlGui_AdminList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  if(%status $= "S") {
    GlassServerControlGui_PromoteBtn.mcolor = "220 170 170 255";
    GlassServerControlGui_PromoteBtn.setText("Demote");
  } else if(%status $= "A") {
    GlassServerControlGui_PromoteBtn.mcolor = "170 220 170 255";
    GlassServerControlGui_PromoteBtn.setText("Promote");
  } else {
    GlassServerControlGui_PromoteBtn.mcolor = "255 200 200 255";
    GlassServerControlGui_PromoteBtn.setText("fuck");
  }
}

function GlassServerControlC::promoteSelected() {
  %list = GlassServerControlGui_AdminList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  %action = GlassServerControlGui_PromoteBtn.text;

  if(%action $= "fuck") {
    messageBoxOk("You Win!", "I don't know who you are.<br><br>I don't know what you did.<br><br>But you found me.");
  } else if(%action $= "Promote") {
    //commandToServer('GlassPromote', %blid);
  } else if(%action $= "Demote") {
    //commandToServer('GlassDemote', %blid);
  }
}

function clientCmdGlassServerControlEnable(%tog) {
  GlassServerControlC.enabled = %tog;
  if(!%tog) {
    if(GlassServerControlGui.isAwake()) {
      canvas.popDialog(GlassServerControlGui);
    }
  }
}

function clientCmdGlassAdminListing(%data, %append) {
  if(!%append) {
    GlassServerControlGui_AdminList.clear();
  }

  for(%i = 0; %i < getLineCount(%data); %i++) {
    GlassServerControlGui_AdminList.addRow(GlassServerControlGui_AdminList.getCount(), getLine(%data, %i));
  }

  GlassServerControlGui_AdminList.sort(1, true);
}

function GlassPrefGroup::cleanup() {
  GlassServerControlC.enabled = false;
  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %cat = GlassPrefGroup.getObject(%i);
    %cat.deleteAll();
  }
  GlassPrefGroup.deleteAll();
}

package GlassServerControlC {
  function disconnect(%a) {
    GlassPrefGroup::cleanup();
    parent::disconnect(%a);
  }

  function disconnectCleanup(%a) {
    GlassPrefGroup::cleanup();
    return parent::disconnectCleanup(%a);
  }
};
activatePackage(GlassServerControlC);
