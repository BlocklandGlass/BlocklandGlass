function GlassModManager::init() {
  if(isObject(GlassModManager)) {
    GlassModManager.delete();
  }

  new ScriptObject(GlassModManager) {
    historyPos = -1;
  };

  GlassModManager::setPane(1);
  GlassModManager_AddonPage::init();
  GlassModManager_MyColorsets::init();


    GlassModManagerGui_ForwardButton.setVisible(false);
    GlassModManagerGui_BackButton.setVisible(false);
}

function GlassModManager::setPaneRaw(%pane) {
  for(%a = 0; %a < 5; %a++) {
    %obj = "GlassModManager_Pane" @ %a+1;
    %obj.setVisible(false);
  }

  %obj = "GlassModManager_Pane" @ %pane;
  %obj.setVisible(true);
}

function GlassModManager::setPane(%pane) {
  for(%a = 0; %a < 5; %a++) {
    %obj = "GlassModManager_Pane" @ %a+1;
    %obj.setVisible(false);
  }

  if(%pane == 1) {
    if(isObject(GlassModManagerActivityList)) {
      GlassModManagerActivityList.clear();
    }
    GlassModManager.pullActivityFeed();
  }

  if(%pane == 2) {
    GlassModManager::setLoading(true);
    GlassModManager.loadBoards();
  }

  if(%pane == 3) {
    GlassModManager::setLoading(true);
    GlassModManager.populateMyAddons();
  }

  if(%pane == 4) {
    GlassModManager::populateColorsets();
  }

  %obj = "GlassModManager_Pane" @ %pane;
  %obj.setVisible(true);
}

function GlassModManager::setLoading(%bool) {
  if(%bool) {
    //%parent = GlassModManagerGui_LoadingAnimation.getGroup();
    //%parent.bringToFront(GlassModManagerGui_LoadingAnimation);

    GlassModManagerGui_LoadingAnimation.setVisible(true);
    GlassModManagerGui_LoadingAnimation.frame = 1;

    GlassModManager.animationTick();
  } else {
    GlassModManagerGui_LoadingAnimation.setVisible(false);
    cancel(GlassModManagerGui_LoadingAnimation.schedule);
  }
}

function GlassModManager::animationTick(%this) {
  %obj = GlassModManagerGui_LoadingAnimation;
  cancel(%obj.schedule);

  %obj.frame++;
  if(%obj.frame > 22) {
    %obj.frame = 1;
  }
  GlassModManagerGui_LoadingAnimation.setBitmap("Add-Ons/System_BlocklandGlass/image/loading_animation/" @ %obj.frame @ ".png");
  %obj.schedule = %this.schedule(100, "animationTick");
}

function GlassModManager::historyAdd(%this, %page, %parameter) {
  if(%this.historyWriteIgnore) {
    %this.historyWriteIgnore = false;
    return;
  }

  %this.historyLen = %this.historyPos+1;
  %this.history[%this.historyLen] = %page TAB %parameter;

  %this.historyLen++;
  %this.historyPos++;

  GlassModManagerGui_ForwardButton.setVisible(false);
  GlassModManagerGui_BackButton.setVisible(true);
}

function GlassModManager::historyBack(%this) {
  GlassModManagerGui_ForwardButton.setVisible(true);
  if(%this.historyPos <= 1) {
    GlassModManagerGui_BackButton.setVisible(false);
  }

  if(%this.historyPos <= 0) {
    return;
  }

  %this.historyPos--;
  %this.historyWriteIgnore = true;
  %hdat = %this.history[%this.historyPos];
  %page = getField(%hdat, 0);
  %parm = getField(%hdat, 1);

  //main, board, addon
  if(%page $= "main") {
    GlassModManager.loadBoards();
    GlassModManager::setLoading(true);
  } else if(%page $= "board") {
    GlassModManager.loadBoard(%parm);
  } else if(%page $= "addon") {
    GlassModManager_AddonPage.loadAddon(%parm);
  }

  GlassModManager::setPaneRaw(2);
}

function GlassModManager::historyForward(%this) {
  GlassModManagerGui_BackButton.setVisible(true);
  if(%this.historyPos < %this.historyLen-1) {
    %this.historyPos++;
    %hdat = %this.history[%this.historyPos];
    %page = getField(%hdat, 0);
    %parm = getField(%hdat, 1);

    %this.historyWriteIgnore = true;
    //main, board, addon
    if(%page $= "main") {
      GlassModManager.loadBoards();
      GlassModManager::setLoading(true);
    } else if(%page $= "board") {
      GlassModManager.loadBoard(%parm);
      GlassModManager::setLoading(true);
    } else if(%page $= "addon") {
      GlassModManager_AddonPage.loadAddon(%parm);
      GlassModManager::setLoading(true);
    }

    if(%this.historyPos == %this.historyLen-1) {
      GlassModManagerGui_ForwardButton.setVisible(false);
    }
  } else {
    GlassModManagerGui_ForwardButton.setVisible(false);
  }

  GlassModManager::setPaneRaw(2);
}

function GlassModManager_keybind(%down) {
  if(%down) {
    return;
  }

  if(GlassModManagerGui.isAwake()) {
    canvas.popDialog(GlassModManagerGui);
  } else {
    canvas.pushDialog(GlassModManagerGui);
  }
}

function GlassModManager::changeKeybind(%this) {
  GlassModManagerGui_KeybindText.setText("<font:Quicksand:16><just:center><color:ffffff>Press any key ...");
  GlassModManagerGui_KeybindOverlay.setVisible(true);
  %remapper = new GuiInputCtrl(GlassModManager_Remapper);
  GlassModManagerGui.add(%remapper);
  %remapper.makeFirstResponder(1);

  %bind = GlassSettings.get("MM::Keybind");
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
      GlassModManagerGui_KeybindText.setText("<font:Arial Bold:16><just:center><color:ffffff>Invalid Character. <font:arial:16>Please try again");
      return;
    }
  }

  //clearSwatch
  GlassModManagerGui_KeybindOverlay.setVisible(false);

  %bind = GlassSettings.get("MM::Keybind");

  GlobalActionMap.unbind(getField(%bind, 0), getField(%bind, 1));
  GlobalActionMap.bind(%device, %key, "GlassModManager_keybind");
  GlassModManager_Remapper.delete();
  GlassModManagerGui_Prefs_Keybind.setText("\c4" @ strupr(%key));
  GlassSettings.update("MM::Keybind", %device TAB %key);
}

//====================================
// Activity
//====================================

function GlassModManager::pullActivityFeed(%this) {
  %url = "http://" @ Glass.address @ "/api/activity.php";
  %method = "GET";
  %downloadPath = "";
  %className = "GlassModManagerTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassModManager::addActivityEvent(%this, %id, %image, %text, %action, %date) {
  if(!isObject(GlassModManagerActivityList)) {
    new ScriptGroup(GlassModManagerActivityList) {

    };
  }

  %activity = new ScriptObject() {
    class = "GlassEvent";

    id = %id; //unused
    image = %image;
    text = %text;
    action = %action; //unused
    datestring = %date;
  };

  GlassModManagerActivityList.add(%activity);
}

function GlassModManager::renderActivity() {
  GlassModManager_ActivityFeed.clear();
  %currentY = 0;
  %lastDate = "";
  for(%i = 0; %i < GlassModManagerActivityList.getCount(); %i++) {
    %event = GlassModManagerActivityList.getObject(%i);
    if(%lastDate !$= getWord(%event.datestring, 0)) {
      %currentY += 5;
      %title = new GuiTextCtrl() {
         profile = "BlockButtonProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = 0 SPC %currentY;
         extent = "500 25";
         minExtent = "8 2";
         enabled = "1";
         visible = "1";
         clipToParent = "1";
         text = getWord(%event.datestring, 0);
         maxLength = "255";
      };
      %currentY += 30;
      GlassModManager_ActivityFeed.add(%title);
    }

    %note = new GuiSwatchCtrl() {
       profile = "GuiDefaultProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = 17 SPC %currentY;
       extent = "465 30";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       color = "172 216 230 255";

       new GuiBitmapCtrl() {
          profile = "GuiDefaultProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "7 7";
          extent = "16 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %event.image @ ".png";
          wrap = "0";
          lockAspectRatio = "0";
          alignLeft = "0";
          alignTop = "0";
          overflowImage = "0";
          keepCached = "0";
          mColor = "255 255 255 255";
          mMultiply = "0";
       };
       new GuiMLTextCtrl() {
          profile = "GuiMLTextProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "30 7";
          extent = "429 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lineSpacing = "2";
          allowColorChars = "0";
          maxChars = "-1";
          text = "<font:Quicksand:16>" @ %event.text;
          //text = "<font:impact:16>Slap<font:arial:16> was given moderator approval by <font:impact:16>Greek2me";
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
       };
    };
    %currentY += 35;
    GlassModManager_ActivityFeed.add(%note);
    %lastDate = getWord(%event.datestring, 0);

    if(%currentY > 500) {
      GlassModManager_ActivityFeed.extent = getWord(GlassModManager_ActivityFeed.extent, 0) SPC %currentY;
      GlassModManager_ActivityFeed.setVisible(true);
    }
    GlassModManager_ActivityFeed.getGroup().scrollToTop();
  }
}

function GlassModManagerTCP::onDone(%this, %error) {
	if(!%error) {
		%array = parseJSON(collapseEscape(%this.buffer));
		if(getJSONType(%array) $= "array") {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.item[%i];
        %id = %obj.get("id");
        %image = %obj.get("image");
        %text = %obj.get("text");
        %action = %obj.get("action");
        %date = %obj.get("datestring");
        GlassModManager.addActivityEvent(%id, %image, %text, %action, %date);
			}
      GlassModManager.renderActivity();
		} else {

    }
	} else {

  }

}

function GlassModManagerTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}


//  [
//    {
//      "id": 12,
//      "image": "accept",
//      "text": "Jincux pulled some shit",
//      "datestring": "07-11-2014 4:27 PM",
//      "click": "addon\t123"
//    }
//  ]

//====================================
// Boards
//====================================

function GlassModManager::loadBoards() {
  %url = "http://" @ Glass.address @ "/api/mm.php?request=boards";
  %method = "GET";
  %downloadPath = "";
  %className = "GlassModManagerBoardsTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassModManager::addBoard(%this, %id, %image, %title, %fileCount, %sub) {
  if(%id >= 0) {
    $BLG::MM::BoardCache::Image[%id] = %image;
  }
  if(!isObject(GlassModManagerBoards)) {
    new ScriptGroup(GlassModManagerBoards);
  }

  %so = new ScriptObject() {
    class = GlassModManagerBoard;
    id = %id;
    image = %image;
    title = %title;
    filecount = %fileCount;
    subcategory = %sub;
  };

  GlassModManagerBoards.board[%id] = %so;
  GlassModManagerBoards.add(%so);
}

function GlassModManager::renderBoards() {
  GlassModManager::setLoading(false);
  GlassModManager.historyAdd("main");
  GlassModManager_Boards.clear();
  %currentY = 0;
  %lastSub = "";
  %boardcolor = "172 216 230 255";
  for(%i = 0; %i < GlassModManagerBoards.getCount(); %i++) {
    %bo = GlassModManagerBoards.getObject(%i);

    if(%bo.subcategory !$= %lastSub) {
      %divcolor = "122 170 200 255";
      if(%bo.subcategory $= "Special") {
        %currentY += 30;
        %divcolor = "232 118 0 255";
        //%boardcolor = "200 200 70 255";
      }

      %currentY += 10;
      %div = new GuiSwatchCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "center";
        vertSizing = "bottom";
        position = 10 SPC %currentY;
        extent = "485 30";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        color = %divcolor;

        new GuiMLTextCtrl() {
          profile = "GuiMLTextProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "0 7";
          extent = "485 30";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lineSpacing = "2";
          allowColorChars = "0";
          maxChars = "-1";
          text = "<just:center><font:arial bold:18>" @ %bo.subcategory;
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
        };
      };
      GlassModManager_Boards.add(%div);
      %currentY += 32;
    }
    %lastSub = %bo.subcategory;

    %board = new GuiSwatchCtrl("GlassModManager_Board_" @ %bo.id) {
       profile = "GuiDefaultProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = 10 SPC %currentY;
       extent = "485 30";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       color = %boardColor;

       new GuiBitmapCtrl() {
          profile = "GuiDefaultProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "7 7";
          extent = "16 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bo.image @ ".png";
          wrap = "0";
          lockAspectRatio = "0";
          alignLeft = "0";
          alignTop = "0";
          overflowImage = "0";
          keepCached = "0";
          mColor = "255 255 255 255";
          mMultiply = "0";
       };
       new GuiMLTextCtrl() {
          profile = "GuiMLTextProfile";
          horizSizing = "right";
          vertSizing = "center";
          position = "30 7";
          extent = "429 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lineSpacing = "2";
          allowColorChars = "0";
          maxChars = "-1";
          text = "<font:arial bold:16>" @ %bo.title;
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
       };
       new GuiTextCtrl() {
          profile = "GuiDefaultProfile";
          horizSizing = "left";
          vertSizing = "center";
          position = "450 7";
          extent = "20 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lineSpacing = "2";
          allowColorChars = "0";
          maxChars = "-1";
          text = %bo.filecount;
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
       };
       new GuiMouseEventCtrl("GlassModManager_BoardButton") {
          boardId = %bo.id;
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
    };
    GlassModManager_Boards.add(%board);
    %currentY += 32;
  }

  %currentY += 8;

  if(%currentY > 499) {
    GlassModManager_Boards.extent = getWord(GlassModManager_Boards.extent, 0) SPC %currentY;
  } else {
    GlassModManager_Boards.extent = getWord(GlassModManager_Boards.extent, 0) SPC 499;
  }
  GlassModManager_Boards.getGroup().scrollToTop();
  GlassModManager_Boards.setVisible(true);
}

function GlassModManager_BoardButton::onMouseEnter(%this) {
  %board = "GlassModManager_Board_" @ %this.boardId;
  %board.color = "150 200 255 255";
}

function GlassModManager_BoardButton::onMouseLeave(%this) {
  %board = "GlassModManager_Board_" @ %this.boardId;
  %board.color = "172 216 230 255";
}

function GlassModManager_BoardButton::onMouseDown(%this) {
  GlassModManager.loadBoard(%this.boardId);
}

function GlassModManagerBoardsTCP::onDone(%this) {
  if(!%error) {
		%array = parseJSON(collapseEscape(%this.buffer));
    GlassModManagerBoards.deleteAll();
		if(getJSONType(%array) $= "array") {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.item[%i];
        %id = %obj.get("id");
        %image = %obj.get("image");
        %name = %obj.get("name");
        %files = %obj.get("files");
        %sub = %obj.get("sub");
        GlassModManager.addBoard(%id, %image, %name, %files, %sub);
			}
      GlassModManager.renderBoards();
		} else {
    }
	} else {
  }
}

function GlassModManagerBoardsTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}


//====================================
// Board Loading
//====================================

function GlassModManager::loadBoard(%this, %id) {
  if(%id == -2) {
    gotoWebPage("http://blocklandglass.com/rtb/");
    return;
  }
  %url = "http://" @ Glass.address @ "/api/mm.php?request=board&board=" @ %id;
  %method = "GET";
  %downloadPath = "";
  %className = "GlassModManagerBoardTCP";

  GlassModManager::setLoading(true);
  GlassModManager.historyAdd("board", %id);
  %this.currentBoard = %id;

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassModManager::addBoardListing(%this, %aid, %name, %author, %client, %server, %downloads, %fd) {
  if(!isObject(GlassModManagerBoardListings)) {
    new ScriptGroup(GlassModManagerBoardListings);
  }

  %listing = new ScriptObject() {
    class = "GlassModManagerBoardListing";

    aid = %aid;
    name = %name;
    author = %author;
    client = %client;
    server = %server;
    downloads = %downloads;

    temp_filedata = %fd;
  };

  GlassModManagerBoardListings.add(%listing);
}

function GlassModManager_AddonButton::onMouseEnter(%this) {
  %board = "GlassModManager_AddonListing_" @ %this.addonId;
  %board.color = "150 200 255 255";
}

function GlassModManager_AddonButton::onMouseLeave(%this) {
  %board = "GlassModManager_AddonListing_" @ %this.addonId;
  %board.color = "172 216 230 255";
}

function GlassModManager_AddonButton::onMouseDown(%this) {
  //GlassModManager.loadBoard(%this.boardId);
  //GlassDownloadManager.fetchAddon(%this.fileObject);
  GlassModManager_AddonPage.loadAddon(%this.addonId);
}

function GlassModManager::renderCurrentBoard() {
  GlassModManager::setLoading(false);
  GlassModManager_Boards.clear();
  %currentY = 10;
  %div = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "center";
    vertSizing = "bottom";
    position = 10 SPC %currentY;
    extent = "485 30";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "122 170 200 255";

    new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "center";
      position = "0 7";
      extent = "485 30";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      text = "<just:center><font:arial bold:18>" @ GlassModManagerBoards.board[GlassModManager.currentBoard].title;
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
    };
  };
  GlassModManager_Boards.add(%div);
  %currentY += 32;

  for(%i = 0; %i < GlassModManagerBoardListings.getCount(); %i++) {
    %bo = GlassModManagerBoardListings.getObject(%i);

    %board = new GuiSwatchCtrl("GlassModManager_AddonListing_" @ %bo.aid) {
       profile = "GuiDefaultProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = 10 SPC %currentY;
       extent = "485 30";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       color = "172 216 230 255";

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
          text = "<font:arial bold:16>" @ %bo.name @ " <font:arial:14>by " @ %bo.author;
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
       };
       new GuiTextCtrl() {
          profile = "GuiDefaultProfile";
          horizSizing = "left";
          vertSizing = "center";
          position = "450 7";
          extent = "20 16";
          minExtent = "8 2";
          enabled = "1";
          visible = "1";
          clipToParent = "1";
          lineSpacing = "2";
          allowColorChars = "0";
          maxChars = "-1";
          text = %bo.downloads;
          maxBitmapHeight = "-1";
          selectable = "1";
          autoResize = "1";
       };
       new GuiMouseEventCtrl("GlassModManager_AddonButton") {
          addonId = %bo.aid;
          fileObject = %bo.temp_filedata;
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
    };
    GlassModManager_Boards.add(%board);
    %currentY += 32;
  }
  %currentY += 8;
  if(%currentY > 499) {
    GlassModManager_Boards.extent = getWord(GlassModManager_Boards.extent, 0) SPC %currentY;
  } else {
    GlassModManager_Boards.extent = getWord(GlassModManager_Boards.extent, 0) SPC 499;
  }
  GlassModManager_Boards.getGroup().scrollToTop();
  GlassModManager_Boards.setVisible(true);
}

function GlassModManagerBoardTCP::onDone(%this) {
  if(!%error) {
		%array = parseJSON(collapseEscape(%this.buffer));
    GlassModManagerBoardListings.clear();
		if(getJSONType(%array) $= "array") {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.item[%i];

        //$ro->title = $addon->getName();
        //$ro->rating = $ratingData['average'];
        //$ro->author = $addon->getAuthor()->getName();
        //$ro->server = $addon->isServer();
        //$ro->client
        %id = %obj.get("id");
        %author = %obj.get("author");
        %server = %obj.get("server");
        %client = %obj.get("client");
        %name = %obj.get("title");

        %filename = %obj.get("temp_filename");
        %branch = %obj.get("temp_branch");

        %dl = %obj.get("downloads");

        %fileData = GlassFileData::create(%name, %id, %branch, %filename);

        GlassModManager.addBoardListing(%id, %name, %author, %client, %server, %dl, %fileData);
			}
      GlassModManager.renderCurrentBoard();
		} else {
    }
	} else {
  }
}

function GlassModManagerBoardTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

//====================================
// My Add-Ons
//====================================

function GlassModManager_MyAddons::defaults() {
  echo("Loading defaults");
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

  $AddOn__System_BlocklandGlass = 1;

  GlassModManager.renderMyAddons();
  export("$AddOn__*", "config/server/ADD_ON_LIST.cs");
}

function GlassModManager_MyAddons::enableAll() {
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    %check.setValue(true);
  }

  $AddOn__System_BlocklandGlass = 1;
}

function GlassModManager_MyAddons::disableAll() {
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    %check.setValue(false);
  }

  $AddOn__System_BlocklandGlass = 1;
}

function GlassModManager_MyAddons::apply() {
  for(%i = 0; %i < GlassModManagerGui_MyAddons.getCount(); %i++) {
    %guiObj = GlassModManagerGui_MyAddons.getObject(%i);
    %check = %guiObj.getObject(1);
    $AddOn["__" @ %check.addon] = %check.getValue();
  }

  GlassModManager.renderMyAddons();
  $AddOn__System_BlocklandGlass = 1;
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
  %pattern = "Add-ons/*/server.cs";
	%idArrayLen = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-18);
    if(strpos(%name, "/") >= 0) { //removes sub-directories
      continue;
    }

    if(%name $= "System_BlocklandGlass") {
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
          %buffer = %buffer NL %fo.readLine();
        } else {
          %buffer = %fo.readLine();
        }
      }
      %fo.close();
      %fo.delete();
      %so.glassdata = parseJSON(collapseEscape(%buffer));
    }
    GlassModManager_MyAddons.add(%so);
	}
  %this.renderMyAddons();
}

function GlassModManager::renderMyAddons(%this) {
  GlassModManager::setLoading(false);
  GlassModManagerGui_MyAddons.clear();
  %currentY = 10;
  for(%i = 0; %i < GlassModManager_MyAddons.getCount(); %i++) {
    //I guess they load in reverse order. lets fix that
    %addon = GlassModManager_MyAddons.getObject(GlassModManager_MyAddons.getCount()-%i-1);
    %enabled = $AddOn["__" @ %addon.name];
    if(%enabled) {
      %color = "153 204 119 255";
    } else {
      %color = "204 119 119 255";
    }

    %text = "<font:arial bold:16>" @ %addon.name;

    if(%addon.isBLG) {
      %text = "<font:arial bold:16>" @ %addon.glassdata.get("title") @ " <font:arial:14>" @ %addon.name;
    }

    %gui = new GuiSwatchCtrl() {
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
     new GuiBitmapCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "center";
        position = "291 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = %addon.isRTB;
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
        position = "312 7";
        extent = "16 16";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/gear_in.png";
        wrap = "0";
        lockAspectRatio = "0";
        alignLeft = "0";
        alignTop = "0";
        overflowImage = "0";
        keepCached = "0";
        mColor = "255 255 255 255";
        mMultiply = "0";

        new GuiMouseEventCtrl("GlassModManagerGui_AddonSettings") {
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
    GlassTempCheck.setValue($AddOn["__" @ %addon.name]);
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

function GlassModManagerGui_AddonSettings::onMouseDown(%this) {
  if(!%this.addon.isBLG) {
    messageBoxOk("Add-On", %this.addon.name);
  } else {
    %versionData = loadJSON("Add-Ons/" @ %this.addon.name @ "/version.json");

    //GlassModManagerGui_AddonSettings_Branch.clear();
    //GlassModManagerGui_AddonSettings_Branch.add("Stable", 1);
    //GlassModManagerGui_AddonSettings_Branch.add("Unstable", 2);
    //GlassModManagerGui_AddonSettings_Branch.add("Development", 3);

    //GlassModManagerGui_AddonSettings_Window.setText(%this.addon.glassdata.get("title") @ " - " @ %versionData.get("version"));
    //GlassModManagerGui_AddonSettings_Window.setVisible(true);
    messageBoxOk(%this.addon.glassdata.get("title"), "<font:arial bold:14>Version:<font:arial:14> " @ %versionData.get("version"));
  }
}

//====================================
// Colorsets
//====================================

function GlassModManager::populateColorsets() {
  GlassModManager::setLoading(false);
  %this = GlassModManager_MyColorsets;

  %this.colorsets = 0;
  %pattern = "Add-ons/*/colorset.txt";
	%idArrayLen = 0;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-21);
    if(strpos(%name, "/") >= 0) { //removes sub-directories
      continue;
    }

    %this.colorsetFile[%this.colorsets] = %file;
    %this.colorsetName[%this.colorsets] = %name;
    %this.colorsets++;
	}

  GlassModManagerGui_MyColorsets.clear();
  %currentY = 5;
  for(%i = 0; %i < %this.colorsets; %i++) {
    if($BLG::MM::Colorset $= %this.colorsetFile[%i]) {
      %color = "153 204 119 255";
    } else {
      %color = "204 119 119 255";
    }
    %cs = new GuiSwatchCtrl("GlassModManager_ColorsetListing_" @ %i) {
       profile = "GuiDefaultProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = 5 SPC %currentY;
       extent = "240 30";
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
          text = "<font:arial bold:16>" @ %this.colorsetName[%i];
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
    };
    %currentY += 35;
    GlassModManagerGui_MyColorsets.add(%cs);
    if(%currentY > 498) {
      GlassModManagerGui_MyColorsets.extent = getWord(GlassModManagerGui_MyColorsets.extent, 0) SPC %currentY;
    } else {
      GlassModManagerGui_MyColorsets.extent = getWord(GlassModManagerGui_MyColorsets.extent, 0) SPC 498;
    }
    GlassModManagerGui_MyColorsets.setVisible(true);
    GlassModManagerGui_MyColorsets.getGroup().scrollToTop();
  }
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
  GlassModManager_MyColorsets.renderColorset($BLG::MM::Colorset = "Add-Ons/System_BlocklandGlass/colorset_default.txt");
  filecopy("config/server/colorset.txt", "config/server/colorset.old");
  filecopy_hack($BLG::MM::Colorset, "config/server/colorset.txt");
  export("$BLG::MM::*", "config/BLG/client/mm.cs");
  GlassModManager::populateColorsets();
}

function GlassModManager_MyColorsets::apply() {
  if(GlassModManager_MyColorsets.selected $= "") {
    return;
  }

  $BLG::MM::Colorset = GlassModManager_MyColorsets.colorsetFile[GlassModManager_MyColorsets.selected];
  GlassModManager::populateColorsets();
  GlassModManager_MyColorsets.selected = "";
  //do file stuff
  filecopy("config/server/colorset.txt", "config/server/colorset.old");
  filecopy_hack($BLG::MM::Colorset, "config/server/colorset.txt");
  export("$BLG::MM::*", "config/BLG/client/mm.cs");
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

exec("./GlassModManager_AddonPage.cs");
