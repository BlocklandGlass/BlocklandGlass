function GlassModManager::init() {
  exec("./submodules/GlassModManager.cs");
  if(isObject(GlassModManager)) {
    GlassModManager.delete();
  }

  new ScriptObject(GlassModManager) {
    
  };

  GlassModManager::catalogAddons();
  GlassModManager::scanForRTB();

  GlassModManagerGui::setPane(1);

  GlassModManager_MyColorsets::init();

  if(Glass.dev) {
    GlassModManagerGui_HostButton.setVisible(true);
    GlassModManagerGui_HostButton.setText(Glass.address);
  } else {
    GlassModManagerGui_HostButton.setVisible(false);
  }
}

function GlassModManager::toggleHost() {
  if(Glass.alt_address $= "") {
    Glass.alt_address = "blocklandglass.com";
  }

  if(Glass.address $= "localhost") {
    Glass.address = Glass.alt_address;
  } else {
    Glass.alt_address = Glass.address;
    Glass.address = "localhost";
  }

  GlassAuth.ident = "";
  GlassAuth.heartbeat();
  GlassModManagerGui_HostButton.setText(Glass.address);
}

function GlassModManagerImageMouse::onMouseDown(%this) {
  canvas.popDialog(GlassModManagerImage);
}

// function GlassModManager::updateLiveSearch() {
  // %val = GlassModManagerGui_Prefs_LiveSearch.getValue();

  // GlassModManager.liveSearch = %val;
  // // GlassSettings.update("MM::LiveSearch", %val);
// }

function GlassModManager::setAddonStatus(%aid, %status) {
  // status:
  // - installed
  // - downloading
  // - queued
  // - outdated
  GlassModManager.addonStatus[%aid] = %status;
}

function GlassModManager::getAddonStatus(%aid) {
  return GlassModManager.addonStatus[%aid];
}

function GlassModManager::catalogAddons() {
  %pattern = "Add-ons/*/glass.json";
	%idArrayLen = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-19);
    if(strpos(%name, "/") >= 0) { //removes sub-directories
      continue;
    }

    jettisonReadFile("Add-Ons/" @ %name @ "/glass.json");
    %json = $JSON::Value;
    GlassModManager::setAddonStatus(%json.get("id"), "installed");
  }
}

function GlassModManagerGui_Window::onWake(%this) {
  if(!GlassModManagerGui.firstWake) {
    GlassModManagerGui::loadContext("home");
    GlassModManagerGui.firstWake = true;
  }
}

function GlassModManager::changeKeybind(%this) {
  GlassModManagerGui_KeybindText.setText("<font:verdana:16><just:center><color:111111>Press any key ...");
  GlassModManagerGui_KeybindOverlay.setVisible(true);
  %remapper = new GuiInputCtrl(GlassModManager_Remapper);
  GlassOverlayGui.add(%remapper);
  %remapper.makeFirstResponder(1);

  %bind = GlassSettings.get("Live::Keybind");
  GlobalActionMap.unbind(getField(%bind, 0), getField(%bind, 1));
  //swatch
}

function GlassModManager_Remapper::onInputEvent(%this, %device, %key) {
  if(%device $= "mouse0") {
    return;
  }

  if(strlen(%key) == 1) {
    %badChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789[]\\/{};:'\"<>,./?!@#$%^&*-_=+`~";
    if(strpos(%badChar, strupr(%key)) >= 0) {
      GlassModManagerGui_KeybindText.setText("<font:Verdana Bold:15><just:center><color:111111>Invalid Character <font:verdana:16><br>Please try again");
      return;
    }
  } else {
    if(%key $= "ESCAPE") {
      GlassModManagerGui_KeybindOverlay.setVisible(false);

      %bind = GlassSettings.get("Live::Keybind");
      GlobalActionMap.bind(getField(%bind, 0), getField(%bind, 1), "GlassLive_keybind");
      GlassModManager_Remapper.delete();
      return;
    }
  }

  GlassModManagerGui_KeybindOverlay.setVisible(false);

  %bind = GlassSettings.get("Live::Keybind");

  GlobalActionMap.unbind(getField(%bind, 0), getField(%bind, 1));
  GlobalActionMap.bind(%device, %key, "GlassLive_keybind");
  GlassModManager_Remapper.delete();
  GlassModManagerGui_Prefs_Keybind.setText("\c4" @ strupr(%key));
  GlassSettings.update("Live::Keybind", %device TAB %key);
}

//====================================
// Communications
//====================================

function GlassModManager::placeCall(%call, %params, %uniqueReturn) {
  if(GlassAuth.ident !$= "") {
    if(%params !$= "")
      for(%i = 0; %i < getLineCount(%params); %i++) {
        %line = getLine(%params, %i);
        %paramText = %paramText @ "&" @ urlenc(getField(%line, 0)) @ "=" @ urlenc(getField(%line, 1));
      }

      %paramText = %paramText @ "&ident=" @ GlassAuth.ident;

    %url = "http://" @ Glass.address @ "/api/2/mm.php?call=" @ %call @ "&ident=" @ GlassAuth.ident @ %paramText;

    Glass::debug("Calling url: " @ %url);

    %method = "GET";
    %downloadPath = "";
    %className = "GlassModManagerTCP";

    %tcp = connectToURL(%url, %method, %downloadPath, %className);
    %tcp.glass_call = %call;
    %tcp.glass_params = %params;
    %tcp.glass_uniqueReturn = %uniqueReturn;
    return %tcp;
  } else {
    GlassAuth.heartbeat();
    return false;
  }
}

function GlassModManagerTCP::handleText(%this, %line) {
  %this.buffer = %this.buffer NL %line;
}

function GlassModManagerTCP::onDone(%this, %error) {
  if(!%error) {
    %error = jettisonParse(%this.buffer);
    if(%error) {
      Glass::debug(%this.buffer);
      GlassModManagerGui::loadErrorPage("jettisonError", $JSON::Error @ " : " @ $JSON::Index);
      return;
    }
    %ret = $JSON::Value;

    Glass::debug("glass server res");

    if(%ret.action $= "auth") {
      GlassAuth.ident = "";
      GlassAuth.heartbeat();
    }

    if(%ret.status $= "success") {
      GlassModManagerGui::setLoading(false);
      switch$(%this.glass_call) {
        case "home":
          Glass::debug(%this.buffer);
          GlassModManagerGui::renderHome(%ret.data);

        case "addon":
          Glass::debug(%this.buffer);

          if(%ret.authors.length == 1) {
            %author = "<font:verdana bold:14>" @ %ret.authors.value[0].name;
          }

          if(%ret.authors.length == 2) {
            %author = %ret.authors.value[0].name @ " and " @ %ret.authors.value[1].name;
          }

          %obj = new ScriptObject() {
            class = "GlassAddonData";

            id = %ret.aid;
            name = GetASCIIString(%ret.name);
            filename = %ret.filename;
            board = %ret.board;
            description = %ret.description;

            date = %ret.date;
            downloads = %ret.downloads;

            rating = %ret.rating;

            screenshots = %ret.screenshots;

            author = GetASCIIString(%author);

            buffer = %this.buffer;
          };

          for(%i = 0; %i < %ret.branches.length; %i++) {
            %branch = %ret.branches.value[%i];
            %obj.branches = trim(%obj.branches SPC %branch.id);
            %obj.branchVersion[%branch.id] = %branch.version;
            %obj.branchName[%branch.id] = %branch.name;
          }

          if(%this.action $= "render") {
            GlassModManagerGui::renderAddon(%obj);
          } else if(%this.action $= "download") {
            // echo("Action: download"); // is this used?
            %ret = GlassModManager::downloadAddon(%ret.aid, false, %this.rtbImportProgress);
          }

        case "boards":
          %str = "";
          for(%i = 0; %i < %ret.boards.length; %i++) {
            %branch = %ret.boards.value[%i];
            %name = %branch.name;
            %id = %branch.id;
            %bg = %branch.video;
            %desc = %branch.description;
            %str = %str @ %name TAB %id TAB %desc TAB %bg NL "";
          }
          GlassModManagerGui::renderBoards(trim(%str));

        case "board":
          Glass::debug(%this.buffer);
          for(%i = 0; %i < %ret.addons.length; %i++) {
            %addon = %ret.addons.value[%i];
            %name = %addon.name;
            %id = %addon.id;
            %rating = %addon.rating;
            %author = %addon.author;
            %downloads = %addon.downloads;

            %listing = %listing @ %id TAB %name TAB %author TAB %rating TAB %downloads NL "";
          }
          GlassModManagerGui::renderBoardPage(%ret.board_id, %ret.board_name, trim(%listing), %ret.page, %ret.pages, %ret.rtb);

        case "comments":
          Glass::debug(%this.buffer);
          GlassModManagerGui::renderAddonComments(%ret.comments);

        case "search":
          if(GlassModManagerGui_SearchBar.lastTCP == %this)
            GlassModManagerGui::searchResults(%ret.results);

        case "rating":
          Glass::debug(%this.buffer);
          GlassModManagerGui::displayAddonRating(%ret.rating);

        case "rtbaddon":
          Glass::debug(%this.buffer);
          glassMessageBoxOk("Open In Browser", "<a:http://blocklandglass.com/addons/rtb/view.php?id=" @ %ret.addon.id @ ">Link</a>");

        case "rtb":
          %newArray = JettisonArray();
          %addons = %ret.addons;
          for(%i = 0; %i < %addons.length; %i++) {
            %addon = %addons.value[%i];
            if(GlassModManager.rtbAddon[%addon.id] !$= "") {
              %newArray.push("object", %addon);
              %addon.set("filename", "string", GlassModManager.rtbAddon[%addon.id]);
            }
          }

          if(%newArray.length > 0) {
            GlassModManagerGui.firstWake = true;
            GlassLive::openModManager();
            GlassModManagerGui::openRTBImport(%newArray);

          }
      }

    } else {
      GlassModManagerGui::loadErrorPage("status_" @ %ret.status, %this.buffer);
    }
	} else {
    GlassModManagerGui::loadErrorPage("tcpclient_" @ %error);
  }
}

//====================================
// RTB
//====================================

function GlassModManager::scanForRTB() {
  %pattern = "Add-ons/*/rtbInfo.txt";
	//echo("\c1Looking for Glass Add-Ons");
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %path = filePath(%file);
    %name = getsubstr(%path, 8, strlen(%path)-8);

    if(strpos(%name, "_") == -1 || strpos(%name, "/") > -1) {
      warn("Skipping " @ %name @ ", invalid add-on name");
      continue;
    }

    if(isFile(%path @ "/glass.json")) {
      continue;
    }

    //echo("Found " @ %name);
    %fo = new FileObject();
    %fo.openForRead(%file);
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      if(getWord(%line, 0) $= "id:") {
        %id = getWord(%line, 1);
        GlassModManager.rtbAddon[%id] = %name;
        GlassModManager.isRTBAddon[%name] = true;
      }
    }
    %fo.close();
    %fo.delete();
  }
}

//====================================
// Home
//====================================

function GlassModManager::loadHome() {
  GlassModManager::placeCall("home");
}

//====================================
// Boards
//====================================

function GlassModManager::loadBoards(%this) {
  GlassModManager::placeCall("boards");
}

//====================================
// My Add-Ons
//====================================

function GlassModManager_MyAddons::defaults() {
  // echo("Loading defaults");
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    $AddOn["__" @ %check.addon] = false;
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

  // $AddOn__System_BlocklandGlass = 1;

  GlassModManager.renderMyAddons();
  export("$AddOn__*", "config/server/ADD_ON_LIST.cs");
}

function GlassModManager_MyAddons::enableAll() {
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    %check.setValue(true);
  }

  // $AddOn__System_BlocklandGlass = 1;
}

function GlassModManager_MyAddons::disableAll() {
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    %check.setValue(false);
  }

  // $AddOn__System_BlocklandGlass = 1;
}

function GlassModManager_MyAddons::apply() {
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    $AddOn["__" @ %check.addon] = %check.getValue();
  }

  GlassModManager.renderMyAddons();
  // $AddOn__System_BlocklandGlass = 1;
  export("$AddOn__*", "config/server/ADD_ON_LIST.cs");
}

function GlassModManager::populateMyAddons(%this) {
  discoverFile("Add-Ons/*.zip");
  if(isObject(GlassModManager_MyAddons)) {
    GlassModManager_MyAddons.delete();
  }

  new ScriptGroup(GlassModManager_MyAddons);

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
      class = "GlassModManager_MyAddon";
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
    GlassModManager_MyAddons.add(%so);
	}
  %this.renderMyAddons();
}

function GlassModManager::renderMyAddons(%this) {
  //GlassModManager::setLoading(false);
  GlassModManagerGui_MyAddons.clear();
  %currentY = 10;
  for(%i = 0; %i < GlassModManager_MyAddons.getCount(); %i++) {
    //I guess they load in reverse order. lets fix that
    %addon = GlassModManager_MyAddons.getObject(GlassModManager_MyAddons.getCount()-%i-1);
	if($AddOn["__" @ %addon.name] == 1)
		%enabled = true;
	else
		%enabled = false;
    if(%enabled) {
      %color = "153 204 119 255";
    } else {
      %color = "204 119 119 255";
    }

    %text = "<font:Verdana Bold:15>" @ %addon.name;

    // if(%addon.isBLG) {
      // %text = "<font:Verdana Bold:15>" @ %addon.glassdata.get("title") @ " <font:verdana:14>" @ %addon.name;
    // }

    %gui = new GuiSwatchCtrl("GlassModManager_AddonListing_" @ %i) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = 10 SPC %currentY;
      extent = "340 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = %color;
       //color = "172 216 230 255";

      new GuiMLTextCtrl() {
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
      new GuiCheckBoxCtrl(GlassTempCheck) {
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
     new GuiMouseEventCtrl("GlassModManagerGui_AddonHighlight") {
        addonId = %i;
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "0 0";
        extent = "340 30";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lockMouse = "0";
     };
     new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "291 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = isDefaultAddon(%addon.name);
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/blLogo.png";
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";
     };
     new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "291 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = %addon.isRTB && !%addon.isBLG && !isDefaultAddon(%addon.name);
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bricks.png";
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";
     };
     new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "291 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = %addon.isBLG && !isDefaultAddon(%addon.name);
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/glassLogo.png";
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";

        new GuiMouseEventCtrl("GlassModManagerGui_AddonRedirect") {
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
     new GuiBitmapCtrl() {
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
    };
    GlassTempCheck.setValue(%enabled);
    GlassTempCheck.setName("GlassModManagerGui_MyAddonCheckbox");
    %currentY += 32;
    GlassModManagerGui_MyAddons.add(%gui);
  }

  if(%currentY > 500) {
    GlassModManagerGui_MyAddons.extent = 500 SPC %currentY;
    GlassModManagerGui_MyAddons.setVisible(true);
  }
  GlassModManagerGui_MyAddons.getGroup().scrollToTop();
}

function isDefaultAddOn(%file) {
  %list = "Add-Ons/System_BlocklandGlass/resources/default_addons.txt";

  if(!isFile(%list)) {
    error(%list SPC "not found.");
    return;
  }

  if(%file $= "")
    return;

  %fo = new FileObject();
  %fo.openForRead(%list);
  while(!%fo.isEOF()) {
    if(strlwr(%fo.readLine()) $= strlwr(%file)) {
      %fo.close();
      %fo.delete();
      return true;
    }
  }
  %fo.close();
  %fo.delete();
  return false;
}

function GlassModManager::deleteAddOn(%this, %addon) {
  %name = %addon.name;

  if(isDefaultAddon(%name)) {
    error("Will not delete default add-ons.");
    return;
  }

  if((%name $= "System_BlocklandGlass") || (%name $= "Support_Preferences") || (%name $= "Support_Updater")) {
    error("Will not delete essential add-ons.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs"))
      error("Will not delete folders.");
    else
      error(%dir @ %name @ ".zip not found.");
    return;
  }

  fileDelete(%dir @ %name @ ".zip");
  GlassModManager::setAddonStatus(%addon.glassdata.id, "");

  glassMessageBoxOk("Add-On Deleted", "<font:verdana bold:13>" @ %name @ "<font:verdana:13> has been deleted.");

  GlassModManager.schedule(1, populateMyAddons);
}

function GlassModManagerGui_AddonDelete::onMouseUp(%this) {
  %name = %this.addon.name;

  if(isDefaultAddon(%name)) {
    glassMessageBoxOk("Delete Add-On", "Sorry, you may not delete default add-ons from the Glass Mod Manager.");
    return;
  }

  if((%name $= "System_BlocklandGlass") || (%name $= "Support_Preferences") || (%name $= "Support_Updater")) {
    glassMessageBoxOk("Delete Add-On", "Sorry, you may not delete essential add-ons required for Blockland Glass' operation from the Glass Mod Manager.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs")) {
      glassMessageBoxOk("Delete Add-On", "Sorry, you may not delete add-ons packaged as folders from the Glass Mod Manager.");
    }
    return;
  }

  glassMessageBoxYesNo("Delete Add-On", "Are you sure you want to delete <font:verdana bold:13>" @ %name, "GlassModManager.deleteAddon(\"" @ %this.addon @ "\");");
}

function GlassModManagerGui_AddonRedirect::onMouseUp(%this) {
  $Glass::MM_PreviousPage = -1;
  $Glass::MM_PreviousBoard = -1;
  GlassModManagerGui::setPane(1);
  GlassModManagerGui::fetchAndRenderAddon(%this.addon.glassdata.id).action = "render";
}

function GlassModManagerGui_AddonHighlight::onMouseEnter(%this) {
  %swatch = "GlassModManager_AddonListing_" @ %this.addonId;
  %swatch.color = vectorAdd(%swatch.color, "50 50 50") SPC 255;
}

function GlassModManagerGui_AddonHighlight::onMouseLeave(%this) {
  %swatch = "GlassModManager_AddonListing_" @ %this.addonId;
  %swatch.color = vectorAdd(%swatch.color, "-50 -50 -50") SPC 255;
}

function GlassModManagerGui_AddonHighlight::onMouseUp(%this) {
  %swatch = "GlassModManager_AddonListing_" @ %this.addonId;
  %button = %swatch.getObject(1);
  %button.setValue(!%button.getValue());
}

//====================================
// Colorsets
//====================================

function GlassModManager::deleteColorset(%this, %colorset) {
  %name = %colorset.name;

  if(isDefaultAddon(%name)) {
    error("Will not delete default colorsets.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs"))
      error("Will not delete folders.");
    else
      error(%dir @ %name @ ".zip not found.");
    return;
  }

  if(GlassSettings.get("MM::Colorset") $= ("Add-Ons/" @ %name @ "/colorSet.txt"))
    GlassModManager_MyColorsets::def();

  fileDelete(%dir @ %name @ ".zip");
  GlassModManager::setAddonStatus(%colorset.glassdata.id, "");

  glassMessageBoxOk("Colorset Deleted", "<font:verdana bold:13>" @ %name @ "<font:verdana:13> has been deleted.");

  GlassModManager.schedule(1, populateColorsets);
}

function GlassModManager::populateColorsets() {
  %this = GlassModManager_MyColorsets;

  %this.colorsets = 0;
  %pattern = "Add-Ons/*/colorSet.txt";
	%idArrayLen = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-21);
    if(strpos(%name, "/") >= 0) { //removes sub-directories
      continue;
    }

    if(%name !$= "Colorset_Default" && getFileCRC(%file) == -1147879122) continue; //default colorset

    %so = new ScriptObject() {
      class = "GlassModManager_MyColorset";
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

    %this.colorsetFile[%this.colorsets] = %file;
    %this.colorsetName[%this.colorsets] = %name;
    %this.colorsetData[%this.colorsets] = %so;
    %this.colorsets++;
	}

  GlassModManagerGui_MyColorsets.clear();
  %currentY = 10;
  for(%i = 0; %i < %this.colorsets; %i++) {
    if(GlassSettings.get("MM::Colorset") $= %this.colorsetFile[%i]) {
      %color = "153 204 119 255";
    } else {
      %color = "204 119 119 255";
    }
    %cs = new GuiSwatchCtrl("GlassModManager_ColorsetListing_" @ %i) {
       profile = "GuiDefaultProfile";
       horizSizing = "right";
       vertSizing = "bottom";
       position = 10 SPC %currentY;
       extent = "230 30";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       dcolor = %color;
       color = %color;

       new GuiMLTextCtrl() {
          profile = "GuiMLTextProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "10 7";
          extent = "429 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lineSpacing = "2";
          allowColorChars = "0";
          maxChars = "-1";
          text = "<font:Verdana Bold:15>" @ %this.colorsetName[%i];
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
       };
       new GuiMouseEventCtrl("GlassModManager_ColorsetButton") {
          colorsetId = %i;
          profile = "GuiDefaultProfile";
          horizSizing = "right";
          vertSizing = "bottom";
          position = "0 0";
          extent = "465 30";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lockMouse = "0";
       };
       new GuiBitmapCtrl() {
          profile = "GuiDefaultProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "200 7";
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

          new GuiMouseEventCtrl("GlassModManager_ColorsetDelete") {
            colorset = %this.colorsetData[%i];
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
       new GuiBitmapCtrl() {
          profile = "GuiDefaultProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "180 7";
          extent = "16 16";
          minExtent = "8 2";
          enabled = "1";
          visible = isDefaultAddon(%this.colorsetName[%i]);
          clipToParent = "1";
          bitmap = "Add-Ons/System_BlocklandGlass/image/icon/blLogo.png";
          wrap = "0";
          lockAspectRatio = "0";
          alignLeft = "0";
          alignTop = "0";
          overflowImage = "0";
          keepCached = "0";
          mColor = "255 255 255 255";
          mMultiply = "0";
       };
     new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "180 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = %this.colorsetData[%i].isRTB && !%this.colorsetData[%i].isBLG && !isDefaultAddon(%this.colorsetName[%i]);
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bricks.png";
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";
     };
     new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "180 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = %this.colorsetData[%i].isBLG && !isDefaultAddon(%this.colorsetName[%i]);
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/glassLogo.png";
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";

        new GuiMouseEventCtrl("GlassModManager_ColorsetRedirect") {
          colorset = %this.colorsetData[%i];
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
    };
    %currentY += 35;
    GlassModManagerGui_MyColorsets.add(%cs);
    GlassModManagerGui_MyColorsets.verticalMatchChildren(498, 10);
    GlassModManagerGui_MyColorsets.setVisible(true);
    GlassModManagerGui_MyColorsets.getGroup().scrollToTop();
  }
  
  GlassModManager_MyColorsets.renderColorset(GlassSettings.get("MM::Colorset"));
}

function GlassModManager_ColorsetDelete::onMouseUp(%this) {
  %name = %this.colorset.name;

  if(isDefaultAddon(%name)) {
    glassMessageBoxOk("Delete Colorset", "Sorry, you may not delete the default colorset from the Glass Mod Manager.");
    return;
  }

  %dir = "Add-Ons/";

  if(!isFile(%dir @ %name @ ".zip")) {
    if(getFileCount(%dir @ %name @ "/*.cs")) {
      glassMessageBoxOk("Delete Colorset", "Sorry, you may not delete colorsets packaged as folders from the Glass Mod Manager.");
    }
    return;
  }

  glassMessageBoxYesNo("Delete Colorset", "Are you sure you want to delete <font:verdana bold:13>" @ %name, "GlassModManager.deleteColorset(\"" @ %this.colorset @ "\");");
}

function GlassModManager_ColorsetRedirect::onMouseUp(%this) {
  $Glass::MM_PreviousPage = -1;
  $Glass::MM_PreviousBoard = -1;
  GlassModManagerGui::setPane(1);
  GlassModManagerGui::fetchAndRenderAddon(%this.colorset.glassdata.id).action = "render";
}

function GlassModManager_ColorsetButton::onMouseDown(%this) {
  if(GlassModManager_MyColorsets.selected !$= "") {
    %swatch = "GlassModManager_ColorsetListing_" @ GlassModManager_MyColorsets.selected;
    %swatch.color = %swatch.dcolor;
  }

  GlassModManager_MyColorsets.renderColorset(GlassModManager_MyColorsets.colorsetFile[%this.colorsetId]);
  GlassModManager_MyColorsets.selected = %this.colorsetId;
  %swatch = "GlassModManager_ColorsetListing_" @ GlassModManager_MyColorsets.selected;
  %swatch.color = "119 119 204 255";
  %swatch.color = vectorAdd(%swatch.color, "50 50 50") SPC 255;
}

function GlassModManager_ColorsetButton::onMouseEnter(%this) {
  %swatch = "GlassModManager_ColorsetListing_" @ %this.colorsetId;
  %swatch.color = vectorAdd(%swatch.color, "50 50 50") SPC 255;
}

function GlassModManager_ColorsetButton::onMouseLeave(%this) {
  %swatch = "GlassModManager_ColorsetListing_" @ %this.colorsetId;
  %swatch.color = vectorAdd(%swatch.color, "-50 -50 -50") SPC 255;
}

function GlassModManager_MyColorsets::init() {
  if(!isObject(GlassModManager_MyColorsets)) {
    new ScriptObject(GlassModManager_MyColorsets);
  }
}

function GlassModManager_MyColorsets::def() {
  GlassSettings.update("MM::Colorset", "Add-Ons/Colorset_Default/colorSet.txt");

  if(!isFile(GlassSettings.get("MM::Colorset"))) {
    error(%default SPC "not found.");
    return;
  }

  GlassModManager_MyColorsets.renderColorset(GlassSettings.get("MM::Colorset"));
  GlassModManager::populateColorsets();
  filecopy("config/server/colorset.txt", "config/server/colorset.old");
  filecopy_hack(GlassSettings.get("MM::Colorset"), "config/server/colorset.txt");
}

function GlassModManager_MyColorsets::apply() {
  if(GlassModManager_MyColorsets.selected $= "") {
    return;
  }

  GlassSettings.update("MM::Colorset", GlassModManager_MyColorsets.colorsetFile[GlassModManager_MyColorsets.selected]);
  GlassModManager::populateColorsets();
  GlassModManager_MyColorsets.selected = "";
  //do file stuff
  filecopy("config/server/colorset.txt", "config/server/colorset.old");
  filecopy_hack(GlassSettings.get("MM::Colorset"), "config/server/colorset.txt");
}

//filecopy doesnt like zips
function filecopy_hack(%source, %destination) {
  %fo_source = new FileObject();
  %fo_dest = new FileObject();
  %fo_source.openForRead(%source);
  %fo_dest.openForWrite(%destination);
  while(!%fo_source.isEOF()) {
    %fo_dest.writeLine(%fo_source.readLine());
  }
  %fo_source.close();
  %fo_dest.close();
  %fo_source.delete();
  %fo_dest.delete();
}

function GlassModManager_MyColorsets::renderColorset(%this, %file) {
  %fo = new FileObject();
  %fo.openforread(%file);
  %this.divs = 0;
  %this.divPointer = 0;
  while(!%fo.isEOF()) {
    %line = %fo.readLine();
    if(%line $= "") {
      continue;
    }

    if(strpos(%line, "DIV:") == 0) {
      %this.divCount[%this.divs] = %this.divPointer;
      %this.divs++;
      %this.divPointer = 0;
      continue;
    }

    if(strpos(getWord(%line, 0), ".") > 0) { //float color
      %r = mFloor(getword(%line, 0)*255);
      %g = mFloor(getword(%line, 1)*255);
      %b = mFloor(getword(%line, 2)*255);
      %a = mFloor(getword(%line, 3)*255);
      %this.color[%this.divs @ "_" @ %this.divPointer] = %r SPC %g SPC %b SPC %a;
      %this.divPointer++;
    } else {
      %this.color[%this.divs @ "_" @ %this.divPointer] = %line;
      %this.divPointer++;
    }
  }
  %fo.close();
  %fo.delete();

  GlassModManagerGui_ColorsetPreview.clear();
  GlassModManager_MyColorsets.selected = "";
  %maxY = 8;
  %currentX = 8;
  %currentY = 8;
  for(%a = 0; %a < %this.divs; %a++) {
    for(%b = 0; %b < %this.divCount[%a]; %b++) {
      %swatch = new GuiSwatchCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = %currentX SPC %currentY;
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        color = %this.color[%a @ "_" @ %b];
      };
      GlassModManagerGui_ColorsetPreview.add(%swatch);
      %currentY += 16;
      if(%currentY > %maxY) {
        %maxY = %currentY;
      }
    }
    %currentX += 16;
    %currentY = 8;
  }
  GlassModManagerGui_ColorsetPreview.extent = %currentX+8 SPC %maxY+8;
  //center
  %parent = GlassModManagerGui_ColorsetPreview.getGroup();
  %x = (getWord(%parent.extent, 0)/2) - (getWord(GlassModManagerGui_ColorsetPreview.extent, 0)/2);
  %y = (getWord(%parent.extent, 1)/2) - (getWord(GlassModManagerGui_ColorsetPreview.extent, 1)/2);
  GlassModManagerGui_ColorsetPreview.position = mFloor(%x) SPC mFloor(%y);
}

//====================================
// Downloading
//====================================

// id - addon id
// beta - bool
// progressBar - additional progress bar to update other than mod manager
function GlassModManager::downloadAddon(%id, %beta, %progressBar) {
  if(!isObject(GlassModManagerQueue)) {
    new ScriptGroup(GlassModManagerQueue);
  }

  %dl = GlassDownloadManager::newDownload(%id, %beta ? 2 : 1);
  %dl.progressBar = %progressBar;
  %dl.addHandle("done", "GlassModManagerQueue_Done");
  %dl.addHandle("progress", "GlassModManagerQueue_Progress");
  %dl.addHandle("failed", "GlassModManagerQueue_Failed");

  GlassModManagerQueue.add(%dl);
  GlassModManagerQueue.next();
}

function GlassModManagerQueue::next(%this) {
  if(isObject(%this.current) && %this.isMember(%this.current))
    return; //downloading

  if(%this.getCount() == 0) {
    GlassModManagerGui::setProgress(1, "All Downloads Finished");
  	GlassModManagerGui.progressSch = GlassModManagerGui.schedule(2000, setProgress);
    CustomGameGui.populateAddOnList();
    return;
  }

  %this.current = %this.getObject(0);
  %this.current.startDownload();

  GlassModManagerGui::setProgress(0, "Connecting..." @ " (" @ GlassModManagerQueue.getCount() @ " remaining)");
}

function GlassModManagerQueue_Done(%this) {
  echo("Downloaded " @ %this.filename);

  GlassModManager::setAddonStatus(%this.addonId, "installed");

  %name = "GlassModManagerGui_DlButton_" @ %this.addonId @ "_" @ %this.branchId;
  if(isObject(%name)) {
    %name.setValue("<font:Verdana Bold:15><just:center>Downloaded<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));
    GlassModManagerGui::fetchAndRenderAddon(%this.addonId).action = "render";
  }

  %file = getsubstr(%this.filename, 0, strlen(%this.filename) - 4);

  setModPaths(getModPaths());

  if(getsubstr(strlwr(%file), 0, 7) $= "client_")
    exec("Add-Ons/" @ %file @ "/client.cs");

  GlassModManagerQueue.remove(%this);
  GlassModManagerQueue.next();
}

function GlassModManagerQueue_Progress(%this, %float) {
  cancel(GlassModManagerGui.progressSch);

  GlassModManager::setAddonStatus(%this.addonId, "downloading");
  GlassModManagerGui::setProgress(%float, "Downloading " @ %this.filename @ " (" @ GlassModManagerQueue.getCount() @ " remaining)");
}

function GlassModManagerQueue_Failed(%this, %error) {
  error("Failed to download add-on " @ %this.addonId @ " (branch " @ %this.branchId @ ")");
  GlassModManagerQueue.remove(%this);
  GlassModManagerQueue.next();
}

package GlassModManager {
  function GuiMLTextCtrl::onURL(%this, %url) {
    if(strpos(%url, "glass://") != -1) {
      %url = stripChars(%url, "[]\\{};'\"<>,.@#%^*+`~");
      %link = getsubstr(%url, 8, strlen(%url)-8);

      if(strpos(%link, "board=") != -1 && strpos(%link, "&page=") != -1) {
        %board = getsubstr(%link, 6, strpos(%link, "&")-6);
        %page = getsubstr(%link, 12+strlen(%board), strlen(%link)-12-strlen(%board));
      } else if(strpos(%link, "aid-") != -1) {
        $Glass::MM_PreviousPage = -1;
        $Glass::MM_PreviousBoard = -1;

        %id = getsubstr(%link, 4, strlen(%link)-4);
      }
    } else if(strpos(%url, "blocklandglass.com/addons/addon.php?id=") != -1) {
      $Glass::MM_PreviousPage = -1;
      $Glass::MM_PreviousBoard = -1;

      %id = getsubstr(%url, strpos(%url, "=") + 1, strlen(%url));
    } else {
      return parent::onURL(%this, %url);
    }

    if(GlassModManagerGui.getCount() > 0) {
      GlassModManagerGui.firstWake = true;
      GlassOverlayGui.add(GlassModManagerGui_Window);
      GlassModManagerGui_Window.forceCenter();
      GlassModManagerGui_Window.visible = true;
    } else {
      GlassModManagerGui_Window.setVisible(true);
    }

    GlassOverlayGui.pushToBack(GlassModManagerGui_Window);

    if(%id+0 $= %id || %id > 0) {
      GlassModManagerGui::fetchAndRenderAddon(%id).action = "render";
    }

    if((%board+0 $= %board || %board > 0) || (%page+0 $= %page || %page > 0)) {
      GlassModManagerGui::fetchBoard(%board, %page);
    }

    if(%link $= "home") {
      GlassModManagerGui::loadContext("home");
    } else if(%link $= "boards") {
      GlassModManagerGui::loadContext("addons");
    }
  }
};
activatePackage(GlassModManager);
