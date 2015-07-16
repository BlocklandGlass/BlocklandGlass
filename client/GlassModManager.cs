function GlassModManager::init() {
  if(isObject(GlassModManager)) {
    GlassModManager.delete();
  }

  new ScriptObject(GlassModManager) {

  };

  GlassModManager.pullActivityFeed();
  GlassModManager::setPane(1);
}

function GlassModManager::pullActivityFeed(%this) {
  %url = "http://" @ BLG.address @ "/api/activity.php";
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
          text = "<font:Arial:16>" @ %event.text;
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
    }
    canvas.pushDialog(GlassModManagerGui);
  }
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

function GlassModManager::setPane(%pane) {
  for(%a = 0; %a < 4; %a++) {
    %obj = "GlassModManager_Pane" @ %a+1;
    %obj.setVisible(false);
  }

  if(%pane == 2) {
    GlassModManager.loadBoards();
    GlassModManager::setLoading(true);
  }

  if(%pane == 3) {
    GlassModManager.populateMyAddons();
    GlassModManager::setLoading(true);
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
//====================================
// Boards
//====================================

function GlassModManager::loadBoards() {
  %url = "http://" @ BLG.address @ "/api/mm.php?request=boards";
  %method = "GET";
  %downloadPath = "";
  %className = "GlassModManagerBoardsTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassModManager::addBoard(%this, %id, %image, %title, %fileCount) {
  if(!isObject(GlassModManagerBoards)) {
    new ScriptGroup(GlassModManagerBoards);
  }

  %so = new ScriptObject() {
    class = GlassModManagerBoard;
    id = %id;
    image = %image;
    title = %title;
    filecount = %fileCount;
  };

  GlassModManagerBoards.add(%so);
}

function GlassModManager::renderBoards() {
  GlassModManager::setLoading(false);
  GlassModManager.location = "boards";
  GlassModManager_Boards.clear();
  %currentY = 10;
  for(%i = 0; %i < GlassModManagerBoards.getCount(); %i++) {
    %bo = GlassModManagerBoards.getObject(%i);

    %board = new GuiSwatchCtrl("GlassModManager_Board_" @ %bo.id) {
       profile = "GuiDefaultProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = 10 SPC %currentY;
       extent = "475 30";
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
		%array = parseJSON(%this.buffer);
    GlassModManagerBoards.deleteAll();
		if(getJSONType(%array) $= "array") {
			for(%i = 0; %i < %array.length; %i++) {
				%obj = %array.item[%i];
        %id = %obj.get("id");
        %image = %obj.get("image");
        %name = %obj.get("name");
        %files = %obj.get("files");
        GlassModManager.addBoard(%id, %image, %name, %files);
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
  %url = "http://" @ BLG.address @ "/api/mm.php?request=board&board=" @ %id;
  %method = "GET";
  %downloadPath = "";
  %className = "GlassModManagerBoardTCP";

  GlassModManager::setLoading(true);

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
  GlassModManager.loadBoard(%this.boardId);
  GlassDownloadManager.fetchAddon(%this.fileObject);
}

function GlassModManager::renderCurrentBoard() {
  GlassModManager::setLoading(false);
  GlassModManager_Boards.clear();
  %currentY = 10;
  for(%i = 0; %i < GlassModManagerBoardListings.getCount(); %i++) {
    %bo = GlassModManagerBoardListings.getObject(%i);

    %board = new GuiSwatchCtrl("GlassModManager_AddonListing_" @ %bo.aid) {
       profile = "GuiDefaultProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = 10 SPC %currentY;
       extent = "475 30";
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
}

function GlassModManagerBoardTCP::onDone(%this) {
  if(!%error) {
		%array = parseJSON(%this.buffer);
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

        %fileData = GlassFileData::create(%name, %id, %branch, %filename);

        GlassModManager.addBoardListing(%id, %name, %author, %client, %server, 0, %fileData);
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




function GlassModManagerTCP::onDone(%this, %error) {
	if(!%error) {
		%array = parseJSON(%this.buffer);
		if(getJSONType(%array) $= "array") {
      echo(%array.length);
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

function GlassModManager::populateMyAddons(%this) {
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
    %so = new ScriptObject() {
      class = "GlassModManager_MyAddon";
      name = %name;
      isRTB = isfile("Add-Ons/" @ %name @ "/rtbInfo.txt");
    };
    GlassModManager_MyAddons.add(%so);
	}
  %this.renderMyAddons();
}

function GlassModManager::renderMyAddons(%this) {
  %this.setLoading(false);
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

    %gui = new GuiSwatchCtrl() {
       profile = "GuiDefaultProfile";
       horizSizing = "right";
       vertSizing = "bottom";
       position = 10 SPC %currentY;
       extent = "335 30";
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
          //text = "<font:arial bold:16>Slap <font:arial:14>Server_Slap";
          text = "<font:arial bold:16>" @ %addon.name;
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
}
