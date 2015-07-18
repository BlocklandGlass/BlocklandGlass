//I figured that this will be substantial enough to be in it's own file

function GlassModManager_AddonPage::init() {
  new ScriptObject(GlassModManager_AddonPage) {

  };
}

function GlassModManager_AddonPage::loadAddon(%this, %id) {
  GlassModManager::setLoading(true);
  %url = "http://" @ BLG.address @ "/api/mm.php?request=addon&id=" @ %id;
  %method = "GET";
  %downloadPath = "";
  %className = "GlassModManager_AddonPageTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassModManager_AddonPage::render(%this) {
  GlassModManager_Boards.clear();
  %board = GlassModManagerBoards.board[%this.boardid];

  %text = "<font:arial:12>";
  %text = %text @ "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/" @ %board.image @ ".png><br>" @ %board.title @ "<br><br>";
  %text = %text @ "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/page_white_zip.png><br><font:arial:12>" @ %this.filename @ "<br>";
  %gui = new GuiSwatchCtrl() {
     profile = "GuiDefaultProfile";
     horizSizing = "right";
     vertSizing = "bottom";
     position = "10 10";
     extent = "485 309";
     minExtent = "8 2";
     enabled = "1";
     visible = "1";
     clipToParent = "1";
     color = "240 240 240 255";

     new GuiMLTextCtrl() {
        profile = "GuiMLTextProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "10 10";
        extent = "455 24";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lineSpacing = "2";
        allowColorChars = "0";
        maxChars = "-1";
        text = "<font:arial bold:25>" @ %this.name @ "<font:arial:16> by <font:arial bold:16>" @ %this.authorName;
        maxBitmapHeight = "-1";
        selectable = "1";
        autoResize = "1";
     };
     new GuiSwatchCtrl() {
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "150 45";
        extent = "1 252";
        minExtent = "1 1";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        color = "0 0 0 255";
     };
     new GuiMLTextCtrl() {
        profile = "GuiMLTextProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "10 45";
        extent = "133 32";
        minExtent = "133 32";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lineSpacing = "2";
        allowColorChars = "0";
        maxChars = "-1";
        //text = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/delete.png> <font:arial:16>Board<br>";
        text = %text;
        maxBitmapHeight = "-1";
        selectable = "1";
        autoResize = "1";
     };
     new GuiMLTextCtrl() {
        profile = "GuiMLTextProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "160 45";
        extent = "315 255";
        minExtent = "315 255";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lineSpacing = "2";
        allowColorChars = "0";
        maxChars = "-1";
        text = %this.desc;
        //text = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/delete.png> <font:arial:16>Board<br>";
        maxBitmapHeight = "-1";
        selectable = "1";
        autoResize = "1";
     };
  };
  GlassModManager_Boards.add(%gui);

  %screenshotBox = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 325";
    extent = "485 105";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "240 240 240 255";
  };

  %currentX = 10;
  for(%i = 0; %i < %this.ss; %i++) {
    %ssObj = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = %currentX SPC 10;
      extent = "85 85";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "220 220 220 255";

      new GuiBitmapCtrl() {
         profile = "GuiDefaultProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "0 0";
         extent = "85 85";
         minExtent = "8 2";
         enabled = "1";
         visible = "1";
         clipToParent = "1";
         bitmap = "config/BLG/tmp/screenshots/" @ %i @ "_thumb.png";
         wrap = "0";
         lockAspectRatio = "1";
         alignLeft = "0";
         alignTop = "0";
         overflowImage = "0";
         keepCached = "0";
         mColor = "255 255 255 255";
         mMultiply = "0";
      };
    };
    %currentX += 95;
    %screenshotBox.add(%ssObj);
  }
  if(%this.ss) {
    GlassModManager_Boards.add(%screenshotBox);
  }
  if(isObject(%this.branch[1])) {
    %dlButton = new GuiBitmapButtonCtrl() {
       profile = "BlockButtonProfile";
       horizSizing = "center";
       vertSizing = "bottom";
       position = "202 440";
       extent = "100 30";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       text = "Download";
       groupNum = "-1";
       buttonType = "PushButton";
       bitmap = "base/client/ui/button1";
       lockAspectRatio = "0";
       alignLeft = "0";
       alignTop = "0";
       overflowImage = "0";
       mKeepCached = "0";
       mColor = "170 255 170 255";
       command = "GlassDownloadManager.fetchAddon(" @ %this.branch[1] @ ");";
    };
    GlassModManager_Boards.add(%dlButton);
  }
}

function GlassModManager_AddonPage::downloadThumbnail(%this, %id) {
  if(%this.thumbnailUrl[%id] !$= "") {
    %url = %this.thumbnailUrl[%id];
    %method = "GET";
    %downloadPath = "config/BLG/tmp/screenshots/" @ %id @ "_thumb.png";
    %className = "GlassModManager_AddonPageTCP";

    %tcp = connectToURL(%url, %method, %downloadPath, %className);
    %tcp.thumbnailDl = true;
    %tcp.screenshot = %id;
  }
}

function GlassModManager_AddonPage::downloadScreenshot(%this, %id) {
  if(%this.screenshotUrl[%id] !$= "") {
    %url = %this.screenshotUrl[%id];
    %method = "GET";
    %downloadPath = "config/BLG/tmp/screenshots/" @ %id @ ".png";
    %className = "GlassModManager_AddonPageTCP";

    %tcp = connectToURL(%url, %method, %downloadPath, %className);
    %tcp.screenshotDl = true;
    %tcp.screenshot = %id;
  }
}

function GlassModManager_AddonPageTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

//{
//    "aid": "11",
//    "filename": "System_BlocklandGlass.zip",
//    "board": "9",
//    "screenshotcount": "3",
//    "name": "Blockland Glass",
//    "description": "Blockland Glass is aimed at extending the functionality of Blockland, and making it an easier game to play. The primary feature of Blockland Glass is add-on hosting, with an upcoming in-game downloader.\r\n\r\nFeatures of v0.1:\r\n- Verify your account",
//    "author": {
//        "blid": "9789",
//        "name": "Jincux"
//    },
//    "screenshots": [
//        {
//            "id": 0,
//            "url": "http:\/\/api.blocklandglass.com\/files\/screenshots\/11\/0.png",
//            "thumbnail": "http:\/\/api.blocklandglass.com\/files\/screenshots\/11\/0_thumbnail.png"
//        },
//        {
//            "id": 1,
//            "url": "http:\/\/api.blocklandglass.com\/files\/screenshots\/11\/1.png",
//            "thumbnail": "http:\/\/api.blocklandglass.com\/files\/screenshots\/11\/1_thumbnail.png"
//        },
//        {
//            "id": 2,
//            "url": "http:\/\/api.blocklandglass.com\/files\/screenshots\/11\/2.png",
//            "thumbnail": "http:\/\/api.blocklandglass.com\/files\/screenshots\/11\/2_thumbnail.png"
//        }
//    ],
//    "branches": [
//        {
//            "id": 1,
//            "file": "1553",
//            "version": "0.2.0-alpha.1",
//            "mal": "2"
//        }
//    ]
//}

function GlassModManager_AddonPageTCP::onDone(%this, %error) {
  if(%this.screenshotDl || %this.thumbnailDl) {
    echo("Downloaded image");
    //something about rendering it
  } else if(!%error) {
		%main = parseJSON(%this.buffer);
		if(getJSONType(%main) $= "hash") {
      %ap = GlassModManager_AddonPage;

      %ap.aid = %main.get("aid");
      %ap.name = %main.get("name");
      %ap.boardid = %main.get("board");
      %ap.desc = %main.get("description");
      %ap.filename = %main.get("filename");

      %ap.authorName = %main.get("author").get("name");
      %ap.authorBlid = %main.get("author").get("blid");

      %ap.ss = %main.get("screenshotcount");

      %ssArray = %main.get("screenshots");
      %ap.ss = %ssArray.length;
      for(%i = 0; %i < %ssArray.length; %i++) {
        %screenObj = %ssArray.item[%i];
        %ap.screenshotUrl[%screenObj.id] = %screenObj.get("url");
        %ap.thumbnailUrl[%screenObj.id] = %screenObj.get("thumbnail");
        GlassModManager_AddonPage.downloadThumbnail(%screenObj.id);
      }

      %branches = %main.get("branches");
      for(%i = 0; %i < %branches.length; %i++) {
        %branchObj = %branches.item[%i];
        %ap.branch[%branchObj.get("id")] = GlassFileData::create(%ap.name, %ap.aid, %branchObj.get("id"), %ap.filename);
      }

      GlassModManager_AddonPage.render();
      GlassModManager::setLoading(false);
		} else {

    }
	} else {

  }
}
