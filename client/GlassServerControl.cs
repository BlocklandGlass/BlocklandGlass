function GlassServerControlC::init() {
  new ScriptObject(GlassServerControlC) {
    requestedPrefs = false;
    receivedPrefs = false;
    allowedPrefs = false;
  };
}

$remapDivision[$remapCount] = "Blockland Glass";
   $remapName[$remapCount] = "Server Settings";
   $remapCmd[$remapCount] = "openGlassSettings";
   $remapCount++;

function openGlassSettings(%down) {
  if(!GlassServerControlC.receivedPrefs || !GlassServerControlC.requestedPrefs) {
    GlassPrefBridge::requestPreferences();
  }

  if(!%down) {
    if(GlassServerControlGui.isAwake()) {
      canvas.popDialog(GlassServerControlGui);
    } else if(GlassServerControlC.allowedPrefs) {
      canvas.pushDialog(GlassServerControlGui);
    }
  }
}

function clientCmdGlassNoUpdates() {
  GlassServerControlGui_UpdatesMsg.position = "0 107";
  GlassServerControlGui_UpdatesMsg.setText("<just:center><font:verdana bold:16>No Server Updates Available");

  %group = GlassServerControlGui_UpdatesMsg.getGroup();
  for(%i = 0; %i < %group.getCount(); %i++) {
    %group.getObject(%i).setVisible(false);
  }

  GlassServerControlGui_UpdatesMsg.setVisible(true);
}

function clientCmdGlassAddUpdate(%name, %version, %clear) {
  if(%clear) {
    %group = GlassServerControlGui_UpdatesMsg.getGroup();
    for(%i = 0; %i < %group.getCount(); %i++) {
      %group.getObject(%i).setVisible(true);
    }

    GlassServerControlGui_UpdateSwatch.clear();
    GlassServerControlGui_UpdateScroll.height = 1;
  }

  GlassServerControlGui_UpdatesMsg.position = "0 5";
  GlassServerControlGui_UpdatesMsg.setText("<just:center><font:verdana:16>Server Updates Available");

  %h = GlassServerControlGui_UpdateScroll.height;
  GlassServerControlGui_UpdateScroll.height += 35;

  %swatch = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = 0 SPC %h+0;
     extent = "306 34";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "255 255 255 255";

     new GuiMLTextCtrl() {
        profile = "GuiMLTextProfile";
        horizSizing = "center";
        vertSizing = "center";
        position = "13 8";
        extent = "280 17";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lineSpacing = "2";
        allowColorChars = "0";
        maxChars = "-1";
        text = "<font:verdana bold:18>" @ trim(%name) @ " <font:verdana:18>v" @ trim(%version);
        maxBitmapHeight = "-1";
        selectable = "1";
        autoResize = "1";
     };
  };

  GlassServerControlGui_UpdateSwatch.add(%swatch);
  GlassServerControlGui_UpdateSwatch.setVisible(true);

  if(GlassServerControlGui_UpdateScroll.height+35 > 178) {
    GlassServerControlGui_UpdateSwatch.extent = 306 SPC GlassServerControlGui_UpdateScroll.height+35;
  } else {
    GlassServerControlGui_UpdateSwatch.extent = 306 SPC 178;
  }
}

function GlassServerControlC::populateClientsPopUp(%this) {
  %pref = GlassPrefGroup::findByVariable("$Pref::Glass::ClientAddons");
  %requiredMods = strreplace(%pref.value, ",", "\t");

  GlassServerControlGui_RequiredClientsPopUp.clear();
  %pattern = "Add-ons/*/glass.json";
	echo("\c1Looking for client Add-Ons");
  %files = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
		%json = loadJSON(%file);
    if(%json.get("formatVersion") == 1) {

      if(strpos(getsubstr(%file, 8, strlen(%file)-19), "/") != -1) {
        continue;
      }

      if(isfile(getsubstr(%file, 0, strlen(%file)-10) @ "client.cs")) {

        GlassSettings.cachePut("AddonName_" @ %json.get("id"), %json.get("title"));

        if(containsField(%json.get("id"), %requiredMods)) {
          continue;
        }

        %fileId[%files] = %json.get("id");
        %fileTitle[%files] = %json.get("title");
        %file[%files] = %file;
        GlassServerControlGui_RequiredClientsPopUp.add(%fileTitle[%files], %fileId[%files]);

        %files++;
      }
    }
	}

  if(%files == 0) {
    GlassServerControlGui_RequiredClientsPopUp.enabled = false;
    GlassServerControlGui_RequiredClientsPopUp.setText("None Available");
  } else {
    GlassServerControlGui_RequiredClientsPopUp.enabled = true;
    GlassServerControlGui_RequiredClientsPopUp.setText("Select...");
  }

  GlassServerControlGui_RequiredClientsList.clear();
  for(%i = 0; %i < getFieldCount(%requiredMods); %i++) {
    %mid = getField(%requiredMods, %i);
    %name = GlassSettings.cacheFetch("AddonName_" @ %mid);
    if(%name $= "") {
      GlassServerControlGui_RequiredClientsList.addRow(%mid, "modID_" @ %mid);
    } else {
      GlassServerControlGui_RequiredClientsList.addRow(%mid, %name);
    }
  }
}

function containsField(%needle, %haystack) {
  for(%i = 0; %i < getFieldCount(%haystack); %i++) {
    if(getField(%haystack, %i) $= %needle) {
      return true;
    }
  }
  return false;
}

function GlassServerControlC::addRequiredClient() {
  %append = GlassServerControlGui_RequiredClientsPopUp.getSelected();

  if(%append) {
    %pref = GlassPrefGroup::findByVariable("$Pref::Glass::ClientAddons");
    commandToServer('glassNameCacheAdd', %append, GlassServerControlGui_RequiredClientsPopUp.getValue());
    commandToServer('updateBLPref', %pref.variable, %append @ "," @ %pref.value);
    %pref.value = %append @ "," @ %pref.value;
    %pref.actualvalue = %pref.value;

    GlassServerControlGui_RequiredClientsList.addRow(%append, GlassServerControlGui_RequiredClientsPopUp.getValue() TAB %append);
  }
  GlassServerControlC::populateClientsPopUp();
}

function GlassServerControlC::removeRequiredClient() {
  %id = GlassServerControlGui_RequiredClientsList.getSelectedId();
  if(%id == -1)
    return;
  %pref = GlassPrefGroup::findByVariable("$Pref::Glass::ClientAddons");

  %newValue = "";
  %value = strreplace(%pref.value, ",", "\t");
  for(%i = 0; %i < getFieldCount(%value); %i++) {
    %mid = trim(getField(%value, %i));
    if(%mid != %id) {
      %newValue = %mid @ "," @ %newValue;
    }
  }

  commandToServer('updateBLPref', %pref.variable, %newValue);
  %pref.actualvalue = %pref.value = %newValue;

  GlassServerControlGui_RequiredClientsList.removeRowById(%id);
  GlassServerControlC::populateClientsPopUp();
}


function GlassServerControlC::setTab(%tab) {
  for(%i = 0; %i < 3; %i++) {
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

  if(%type $= "num") {
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
  } else if(%type $= "string" || %type $= "textarea") {
    if(%parm !$= "") {
      if(strlen(%obj.ctrl.getValue()) > %parm) {
        %obj.ctrl.setValue(getsubstr(%obj.ctrl.getValue(), 0, %parm));
      }
    }
  }

  if(%type $= "dropdown" || %type $= "playercount") {
    %pref.localvalue = %obj.ctrl.getSelected();
  } else {
    %pref.localvalue = %obj.ctrl.getValue();
  }
}

function GlassServerControlC::renderPrefCategories() {
  GlassServerControlGui_Prefs_Categories.clear();
  %odd = false;
  %y = 0;
  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %cat = GlassPrefGroup.getObject(%i);

    if(%cat.name $= "Blockland Glass") {
      GlassServerControlC::renderGlassSettings(%cat);
      continue;
    }

    %odd = !%odd;
    if(%odd) {
      %color = "220 250 220 255";
    } else {
      %color = "220 230 220 255";
    }

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
        profile = "GuiTextVerdanaProfile";
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
      catid = %cat.id;
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

  if(%y > 203) {
    GlassServerControlGui_Prefs_Categories.extent = getWord(GlassServerControlGui_Prefs_Categories.extent, 0) SPC %y;
  } else {
    GlassServerControlGui_Prefs_Categories.extent = getWord(GlassServerControlGui_Prefs_Categories.extent, 0) SPC 203;
  }
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

  GlassServerControlC::renderPrefCategory(%this.catid);
}

function GlassServerControlC::renderPrefs() {
  GlassServerControlC::renderPrefCategory(GlassPrefGroup.getObject(0));
}

function GlassServerControlC::renderPrefCategory(%catid) {
  %category = GlassPrefGroup.cat[%catid];
  GlassServerControl_PrefScroll.clear();
  %currentY = 0;

  if(!isObject(%category))
    return;

  //create header
  %subheader = %category.getObject(0).subcategory;
  %header = GlassServerControlC::createHeader(%subheader);
  %header.position = 0 SPC %currentY;
  GlassServerControl_PrefScroll.add(%header);
  %currentY += 24;
  %light = false;
  for(%j = 0; %j < %category.getCount(); %j++) {
    %pref = %category.getObject(%j);
    if(%pref.subcategory !$= %subheader) {
      %subheader = %pref.subcategory;
      %header = GlassServerControlC::createHeader(%subheader);
      GlassServerControl_PrefScroll.add(%header);
      %header.position = 0 SPC %currentY;
      %currentY += 24;
    }
    %swatch = "";
    switch$(%pref.type) {
      case "bool":
        %swatch = GlassServerControlC::createCheckbox();
        %swatch.text.setText(%pref.title);
        %swatch.ctrl.setValue(%pref.value);

      case "num":
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

      case "dropdown":
        %swatch = GlassServerControlC::createList();
        %swatch.text.setText(%pref.title);
        %options = %pref.params;
        for(%k = 0; %k < getWordCount(%options); %k += 2) {
          %swatch.ctrl.add(strreplace(getWord(%options, %k), "_", " "), getWord(%options, %k+1));
        }
        %swatch.ctrl.setSelected(%pref.value);

      case "playercount":
        %swatch = GlassServerControlC::createList();
        %swatch.text.setText(%pref.title);
        %options = %pref.params;
        for(%k = 0; %k < 99; %k++) {
          %swatch.ctrl.add(%k+1, %k+1);
        }
        %swatch.ctrl.setSelected(%pref.value);

      case "wordlist":

      case "userlist":

      case "button":

      case "rgb":

      case "colorset":

      case "datablock":

      case "datablocklist":
    }

    if(%light) {
      %swatch.color = vectoradd(%swatch.color, "50 50 50") SPC 50;
    }
    %light = !%light;

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
      %currentY += 32;
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
     color = "170 170 170 255";

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
        text = "<just:center><font:verdana bold:14>" @ %text;
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
    profile = "GuiTextVerdanaProfile";
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

function GlassServerControlC::createList() {
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
    profile = "GuiTextVerdanaProfile";
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

  %swatch.ctrl = new GuiPopUpMenuCtrl() {
    profile = "GuiPopUpMenuProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "215 6";
    extent = "100 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    maxLength = "255";
    maxPopupHeight = "200";
  };

  %swatch.add(%swatch.text);
  %swatch.add(%swatch.ctrl);
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
    profile = "GuiTextVerdanaProfile";
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
    profile = "GuiTextVerdanaProfile";
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
    profile = "GuiTextVerdanaProfile";
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
    profile = "GuiTextVerdanaProfile";
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

function GlassServerControlC::populatePlayerList(%this) {
  %list = GlassServerControlGui_PlayerList;

  %list.addrow("Name\t9789\t-", 0);
}

function GlassServerControlC::onSelect(%list) {
  if(%list == 1) {
    %list = GlassServerControlGui_AdminList;
    %otherlist = GlassServerControlGui_PlayerList;
  } else if(%list == 2) {
    %list = GlassServerControlGui_PlayerList;
    %otherlist = GlassServerControlGui_AdminList;
  }
  %row = %list.getValue();
  %otherlist.setSelectedRow(-1);

  %status = getField(%row, 2);
  %blid = getField(%row, 1);
}

function GlassServerControlC::promoteSelected() {
  %list = GlassServerControlGui_PlayerList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  if(%status $= "S" || %status $= "H") {
    messageBoxOk("Can't promote!", "This user can't become any higher in rank!");
    return;
  }

  if(%status $= "A") {
    commandToServer('GlassSetAdmin', %blid, 2);
  } else {
    commandToServer('GlassSetAdmin', %blid, 1);
  }
}

function GlassServerControlC::demoteSelected() {
  %list = GlassServerControlGui_AdminList;
  %row = %list.getValue();

  %status = getField(%row, 2);
  %blid = getField(%row, 1);

  if(%status $= "H") {
    messageBoxOk("Can't demote!", "That's a host!");
    return;
  }

  if(%blid == getNumKeyId()) {
    messageBoxOk("Can't demote!", "That's you.");
    return;
  }

  if(%status $= "S") {
    commandToServer('GlassSetAdmin', %blid, 1);
  } else if(%status $= "A") {
    commandToServer('GlassSetAdmin', %blid, 0);
  }
}

function GlassServerControlGui::onWake(%this) {
  GlassServerControlGui_PlayerList.clear();
  for(%i = 0; %i < NPL_List.rowCount(); %i++) {
    %row = NPL_List.getRowText(%i);
    %admin = getField(%row, 0);
    %name = getField(%row, 1);
    %blid = getField(%row, 3);

    GlassServerControlGui_PlayerList.addRow(%i, %name TAB %blid TAB %admin);
  }
}

function clientCmdGlassServerControlEnable(%overall, %prefs) {
  GlassServerControlC.enabled = %overall;
  if(!%overall) {
    if(GlassServerControlGui.isAwake()) {
      canvas.popDialog(GlassServerControlGui);
    }
  }

  for(%i = 0; %i < GlassServerControlGui_Pane2.getCount(); %i++) {
    GlassServerControlGui_Pane2.getObject(%i).setVisible(%prefs);
  }
  GlassServerControlGui_AllowPrefs.setVisible(!%prefs);
}

function clientCmdBLPAllowedUse(%prefs) {
  for(%i = 0; %i < GlassServerControlGui_Pane2.getCount(); %i++) {
    GlassServerControlGui_Pane2.getObject(%i).setVisible(%prefs);
  }
  GlassServerControlGui_AllowPrefs.setVisible(!%prefs);
}

function clientCmdGlassAdminListing(%data, %append) {
  if(!%append) {
    GlassServerControlGui_AdminList.clear();
  }

  if(%data !$= "") {
    for(%i = 0; %i < getLineCount(%data); %i++) {
      GlassServerControlGui_AdminList.addRow(GlassServerControlGui_AdminList.getCount(), getLine(%data, %i));
    }

    GlassServerControlGui_AdminList.sort(1, true);
  }
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
  function NewPlayerListGui::update(%this, %a, %b, %c, %d, %e, %f) {
    parent::update(%this, %a, %b, %c, %d, %e, %f);
    GlassServerControlGui.onWake();
  }

  function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
		return parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Glass" TAB Glass.version TAB GlassClientManager.getClients() NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
	}

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
