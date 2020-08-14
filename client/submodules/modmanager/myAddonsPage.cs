function GMM_MyAddonsPage::init() {
  new ScriptObject(GMM_MyAddonsPage);
}

function GMM_MyAddonsPage::open(%this) {
  if(!isObject(GMM_MyAddonsPage_AddonGroup)) {
    GMM_MyAddonsPage.populateAddons();
  }

  GlassModManagerGui_MainDisplayScroll.vScrollBar = "alwaysOff";

  if(isObject(%this.container) && isObject(%this.container.body)) {
    %body = %this.container.body;
    %body.scroll.scrollToTop();
    %this.populateAddonList(%body.scroll.addonList);
    %body.scroll.addonList.verticalMatchChildren(456, 10);
    %body.scroll.addonList.setVisible(true);
    GlassModManagerGui.schedule(0, pageDidLoad, %this);
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
      profile = "GlassBlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 10";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "glassMessageBoxYesNo(\"Reset\", \"Would you like to reset the enabled/disabled add-ons to Blockland default?\", \"GMM_MyAddonsPage::defaults();\");";
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
      profile = "GlassBlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 45";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GMM_MyAddonsPage::enableAll();";
      text = "Enable All";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "220 220 220 128";
    };
    new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 80";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GMM_MyAddonsPage::disableAll();";
      text = "Disable All";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "220 220 220 128";
    };
    new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 120";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GMM_MyAddonsPage.createGroup();";
      text = "New Group";
      groupNum = "-1";
      buttonType = "PushButton";
      bitmap = "~/System_BlocklandGlass/image/gui/btn";
      lockAspectRatio = "0";
      alignLeft = "0";
      alignTop = "0";
      overflowImage = "0";
      mKeepCached = "0";
      mColor = "84 217 140 128";
    };
    new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonProfile";
      horizSizing = "center";
      vertSizing = "bottom";
      position = "22 430";
      extent = "100 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      command = "GMM_MyAddonsPage::apply();";
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

  GlassModManagerGui.schedule(0, pageDidLoad, %this);
  return %this.container;
}

function GMM_MyAddonsPage::close(%this) {
  if(isObject(%this.container)) {
    %this.container.getGroup().remove(%this.container);
  } else {
  }
  GlassModManagerGui_MainDisplayScroll.vScrollBar = "alwaysOn";
}


function GMM_MyAddonsPage::populateAddonList(%this, %swatch) {
  %swatch.deleteAll();

  // list All Add-Ons add-ons

  %gui = %this.createHeader("All Add-Ons");
  %gui.defaultGroup = true;
  %swatch.add(%gui);

  %last = %gui;

  for(%i = 0; %i < GMM_MyAddonsPage_AddonGroup.getCount(); %i++) {
    %addon = GMM_MyAddonsPage_AddonGroup.getObject(GMM_MyAddonsPage_AddonGroup.getCount()-%i-1);

	 %gui = %this.createAddonToggle("All Add-Ons", %addon);
    %swatch.add(%gui);

    if(%last)
      %gui.placeBelow(%last, 2);

    %last = %gui;
  }

	%fo = new FileObject();

  %path = "config/client/BLG/addon_groups/*.txt";
  %file = findFirstFile(%path);
  while(%file !$= "") {
	  %trim = strLen("config/client/BLG/addon_groups/");
	  %group = getSubStr(%file, %trim, strlen(%file)-%trim-4);
	  %group = strReplace(%group, "_", " ");

	  %gui = %this.createHeader(%group);
	  %swatch.add(%gui);

	  %gui.placeBelow(%last, 10);
	  %last = %gui;

	  %this.groupExists[%group] = true;

	  %fo.openForRead(%file);
	  while(!%fo.isEOF()) {
		  %line = %fo.readLine();
		  if(isObject(GMM_MyAddonsPage_AddonGroup.addon[%line])) {
			  %addon = GMM_MyAddonsPage_AddonGroup.addon[%line];
			  %gui = %this.createAddonToggle(%group, %addon);
		     %swatch.add(%gui);

		     if(%last)
		       %gui.placeBelow(%last, 2);

		     %last = %gui;
		  }
	  }
	  %fo.close();

	  %file = findNextFile(%path);
  }
  %fo.delete();
}

function GMM_MyAddonsPage::createHeader(%this, %group) {
   %color = "46 204 113 200";

	%text = "<font:Verdana Bold:15>" @ %group;

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
	  group = %group;
	  open = true;
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

	%gui.checkbox = new GuiCheckBoxCtrl() {
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
	%gui.checkbox.setValue(true);

  %gui.icon = new GuiBitmapCtrl() {
	 profile = "GuiDefaultProfile";
	 horizSizing = "right";
	 vertSizing = "center";
	 position = "320 7";
	 extent = "16 16";
	 minExtent = "8 2";
	 enabled = "1";
	 visible = true;
	 clipToParent = "1";
	 bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bullet_arrow_down.png";
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

  	if(%this.toggled[%group] $= "")
  		%this.toggled[%group] = true;

	%this.groupCt[%group] = 0;
	%this.group[%this.groupCt+0] = %group;
	%this.groupSwatch[%this.groupCt+0] = %gui;
	%this.groupSwatch[%group] = %gui;
	%this.groupCt++;

	GlassHighlightSwatch::addToSwatch(%gui, "20 20 20", "GMM_MyAddonsPage.toggleGroupDisplay", "GMM_MyAddonsPage.deleteGroupPrompt");

	return %gui;
}

function GMM_MyAddonsPage::toggleGroupDisplay(%this, %swatch, %pos) {
	%x = getWord(%pos, 0);
	%group = %swatch.group;

	if(%x < 32) {
		%swatch.checkbox.setValue(!%swatch.checkbox.getValue());
		%tog = %swatch.checkbox.getValue();
		for(%i = 0; %i < %this.groupCt[%group]; %i++) {
			%addon = %this.addonSwatch[%group, %i].addon;

			for(%j = 0; %j < %addon.swatchSet.getCount(); %j++) {
				%s = %addon.swatchSet.getObject(%j);
				%s.checkbox.setValue(%tog);
			}
		}

		%this.toggled[%swatch.group] = !%this.toggled[%swatch.group];

	} else {
		%swatch.open = !%swatch.open;
		if(%swatch.open) {
			%icon = "bullet_arrow_down";
		} else {
			%icon = "bullet_arrow_right";
		}

		for(%i = 0; %i < %this.groupCt[%group]; %i++) {
			%this.addonSwatch[%group, %i].setVisible(%swatch.open);
		}

		for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
			%obj = GMM_MyAddonsPage_List.getObject(%i);
			if(!%obj.visible) continue;

			if(%last)
				%obj.placeBelow(%last, (%obj.group $= "" ? 2 : 10));

			%last = %obj;
		}

		%swatch.icon.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %icon @ ".png");

	}
}

function GMM_MyAddonsPage::createGroup(%this, %conf) {
	%addons = 0;
	for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
     %guiObj = GMM_MyAddonsPage_List.getObject(%i);

     %check = %guiObj.checkbox;
	  if(%check) {
		  %addon[%addons] = %guiObj.addon;
		  %addons++;
	  }
   }

	if(!%conf) {
		canvas.pushDialog(GlassModManagerGroupGui);
		GlassModManagerGroupGui_Text.setValue("<font:verdana:12>Please select a name for your group containing <font:verdana bold:12>" @ (%addons+0) @ "<font:verdana:12> add-ons");
		GlassModManagerGroupGui_Input.setValue("");
	} else {
		%name = GlassModManagerGroupGui_Input.getValue();

		if(%this.groupExists[%name]) {
			glassMessageBoxOk("Already Exists!", "A group by that name already exists!");
			return;
		}

		%name = strReplace(%name, " ", "_");
		%allowed = "abcdefghijklmnopqrstuvwxyz0123456789-()[]_";
		for(%i = 0; %i < strlen(%name); %i++) {
			%char = getSubStr(%name, %i, 1);
			if(stripos(%allowed, %char) < 0) {
				echo("Bad char " @ %char);
				glassMessageBoxOk("Invalid Name", "Group names must be alphanumeric!");
				return;
			}
		}

		if(strlen(%name) == 0) {
			glassMessageBoxOk("Invalid Name", "Please insert a name!");
			return;
		}

	 	%file = "config/client/BLG/addon_groups/" @ %name @ ".txt";
		%fo = new FileObject();
		%fo.openForWrite(%file);
		for(%i = 0; %i < %addons; %i++) {
			%fo.writeLine(%addon[%i].name);
		}
		%fo.close();
		%fo.delete();

		canvas.popDialog(GlassModManagerGroupGui);
		GMM_MyAddonsPage.populateAddonList(GMM_MyAddonsPage_List);
	}
}

function GMM_MyAddonsPage::deleteGroupPrompt(%this, %swatch) {
	%group = %swatch.group;
	if(%swatch.defaultGroup) return;

	glassMessageBoxYesNo("Confirm", "Are you sure you want to delete <font:verdana bold:13>" @ %group @ "<font:verdana:13>?", "GMM_MyAddonsPage.deleteGroup(\"" @ expandEscape(%group) @ "\");");
}

function GMM_MyAddonsPage::deleteGroup(%this, %name) {
	%name = strReplace(%name, " ", "_");
	%file = "config/client/BLG/addon_groups/" @ %name @ ".txt";

	if(isFile(%file))
		fileDelete(%file);

	GMM_MyAddonsPage.populateAddonList(GMM_MyAddonsPage_List);
}

function GMM_MyAddonsPage::createAddonToggle(%this, %group, %addon) {

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
	  position = "20 10";
	  extent = "320 30";
	  minExtent = "8 2";
	  enabled = "1";
	  visible = "1";
	  clipToParent = "1";
	  color = %color;

	  addon = %addon;
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

	%gui.checkbox = new GuiCheckBoxCtrl() {
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
	%gui.checkbox.setValue(%enabled);

	//blLogo
	//bricks
	//glassLogo

	%icon = "";
	if(isDefaultAddon(%addon.name)) {
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
		 position = "273 7";
		 extent = "16 16";
		 minExtent = "8 2";
		 enabled = "1";
		 visible = true;
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
	  position = "294 7";
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


	%this.addonSwatch[%group, %this.groupCt[%group]+0] = %gui;
	%this.groupCt[%group]++;

	if(!%enabled) {
		%this.groupSwatch[%group].color = "237 118 105 200";
		%this.groupSwatch[%group].checkbox.setValue(false);
	}

	if(!isObject(%addon.swatchSet))
		%addon.swatchSet = new SimSet();

	%addon.swatchSet.add(%gui);

	return %gui;
}

function GMM_MyAddonsPage::defaults() {
  GlassLog::debug("Setting add-ons to default");
  for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
    %guiObj = GMM_MyAddonsPage_List.getObject(%i);

    %check = %guiObj.checkbox;
    $AddOn__[%check.addon] = false;

    %check.setValue(false);
  }

  $AddOn__Bot_Blockhead = 1;
  $AddOn__Bot_Hole = 1;
  $AddOn__Bot_Horse = 1;
  $AddOn__Bot_Shark = 1;
  $AddOn__Bot_Zombie = 1;
  $AddOn__Brick_Arch = 1;
  $AddOn__Brick_Checkpoint = 1;
  $AddOn__Brick_Christmas_Tree = 1;
  $AddOn__Brick_Doors = 1;
  $AddOn__Brick_Halloween = 1;
  $AddOn__Brick_Large_Cubes = 1;
  $AddOn__Brick_Teledoor = 1;
  $AddOn__Brick_Treasure_Chest = 1;
  $AddOn__Brick_V15 = 1;
  $AddOn__Emote_Alarm = 1;
  $AddOn__Emote_Confusion = 1;
  $AddOn__Emote_Hate = 1;
  $AddOn__Emote_Love = 1;
  $AddOn__Item_Key = 1;
  $AddOn__Item_Skis = 1;
  $AddOn__Item_Sports = 1;
  $AddOn__Light_Animated = 1;
  $AddOn__Light_Basic = 1;
  $AddOn__Particle_Basic = 1;
  $AddOn__Particle_FX_Cans = 1;
  $AddOn__Particle_Grass = 1;
  $AddOn__Particle_Player = 1;
  $AddOn__Particle_Tools = 1;
  $AddOn__Player_Fuel_Jet = 1;
  $AddOn__Player_Jump_Jet = 1;
  $AddOn__Player_Leap_Jet = 1;
  $AddOn__Player_No_Jet = 1;
  $AddOn__Player_Quake = 1;
  $AddOn__Print_1x2f_BLPRemote = 1;
  $AddOn__Print_1x2f_Default = 1;
  $AddOn__Print_2x2f_Default = 1;
  $AddOn__Print_2x2r_Default = 1;
  $AddOn__Print_Letters_Default = 1;
  $AddOn__Projectile_GravityRocket = 1;
  $AddOn__Projectile_Pinball = 1;
  $AddOn__Projectile_Pong = 1;
  $AddOn__Projectile_Radio_Wave = 1;
  $AddOn__Sound_Beeps = 1;
  $AddOn__Sound_Phone = 1;
  $AddOn__Sound_Synth4 = 1;
  $AddOn__Support_Doors = 1;
  $AddOn__Vehicle_Ball = 1;
  $AddOn__Vehicle_Flying_Wheeled_Jeep = 1;
  $AddOn__Vehicle_Horse = 1;
  $AddOn__Vehicle_Jeep = 1;
  $AddOn__Vehicle_Magic_Carpet = 1;
  $AddOn__Vehicle_Pirate_Cannon = 1;
  $AddOn__Vehicle_Rowboat = 1;
  $AddOn__Vehicle_Tank = 1;
  $AddOn__Weapon_Bow = 1;
  $AddOn__Weapon_Gun = 1;
  $AddOn__Weapon_Guns_Akimbo = 1;
  $AddOn__Weapon_Horse_Ray = 1;
  $AddOn__Weapon_Push_Broom = 1;
  $AddOn__Weapon_Rocket_Launcher = 1;
  $AddOn__Weapon_Spear = 1;
  $AddOn__Weapon_Sword = 1;

  for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
    %guiObj = GMM_MyAddonsPage_List.getObject(%i);

    %check = %guiObj.checkbox;
    %check.setValue($AddOn__[%check.addon]);
  }

  GMM_MyAddonsPage.populateAddonList(GMM_MyAddonsPage_List);
}

function GMM_MyAddonsPage::enableAll() {
  for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
    %guiObj = GMM_MyAddonsPage_List.getObject(%i);

    %check = %guiObj.checkbox;
    %check.setValue(true);
  }

  // $AddOn__System_BlocklandGlass = 1;
}

function GMM_MyAddonsPage::disableAll() {
  for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
    %guiObj = GMM_MyAddonsPage_List.getObject(%i);

    %check = %guiObj.checkbox;
    %check.setValue(false);
  }

  // $AddOn__System_BlocklandGlass = 1;
}

function GMM_MyAddonsPage::apply() {
  for(%i = 0; %i < GMM_MyAddonsPage_List.getCount(); %i++) {
    %guiObj = GMM_MyAddonsPage_List.getObject(%i);

    %check = %guiObj.checkbox;
    $AddOn__[%check.addon] = %check.getValue();
  }

  export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

  GMM_MyAddonsPage.populateAddonList(GMM_MyAddonsPage_List);
}

function GMM_MyAddonsPage::populateAddons(%this) {
  discoverFile("Add-Ons/*.zip");
  if(isObject(GMM_MyAddonsPage_AddonGroup)) {
    GMM_MyAddonsPage_AddonGroup.delete();
  }

  new ScriptGroup(GMM_MyAddonsPage_AddonGroup);
  GlassGroup.add(GMM_MyAddonsPage_AddonGroup);

  //rtbInfo.txt
  //server.cs
  %pattern = "Add-Ons/*/server.cs";
	%idArrayLen = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-18);

    if(!clientIsValidAddon(%name, 0)) {
      continue;
    }

    if(strpos(%name, "/") >= 0) { //removes sub-directories
      continue;
    }

    if(%name $= "System_BlocklandGlass" || %name $= "Support_Preferences" || %name $= "Support_Updater") {
      continue;
    }

    %so = new ScriptObject() {
      class = "GlassModManager_Addon";
      name = %name;
      isRTB = isfile("Add-Ons/" @ %name @ "/rtbInfo.txt");
      isBLG = isfile("Add-Ons/" @ %name @ "/glass.json");
    };

    if(%so.isBLG) {
      %buffer = "";
      %fo = new FileObject();
      %fo.openforread("Add-Ons/" @ %name @ "/glass.json");
      while(!%fo.isEOF()) {
        if(%buffer !$= "") {
          %buffer = %buffer NL getASCIIString(%fo.readLine());
        } else {
          %buffer = getASCIIString(%fo.readLine());
        }
      }
      %fo.close();
      %fo.delete();
      jettisonParse(collapseEscape(%buffer));
      %so.glassdata = $JSON::Value;
    }
    GMM_MyAddonsPage_AddonGroup.add(%so);
	 GMM_MyAddonsPage_AddonGroup.addon[%name] = %so;
	}
}
