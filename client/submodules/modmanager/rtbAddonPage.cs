function GMM_RTBAddonPage::init() {
  new ScriptObject(GMM_RTBAddonPage);
}

function GMM_RTBAddonPage::open(%this, %modId) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 0 255 0";
    position = "0 0";
    extent = "635 498";
  };

  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 498";
  };

  %this.container = %container;
  %this.body = %body;
  %container.add(%body);

  GlassModManager::placeCall("rtbaddon", "id" TAB %modId, "GMM_RTBAddonPage.handleResults");

  return %container;
}

function GMM_RTBAddonPage::close(%this) {
  %this.container.deleteAll();
}

function GMM_RTBAddonPage::handleResults(%this, %obj) {
  GlassModManagerGui.pageDidLoad(%this);

  %obj = %obj.addon;
  //obj:
  // id
  // glass_id
  // icon
  // type
  // title
  // filename

  %container = %this.container;

  GMM_Navigation.addStep("RTB: " @ %obj.title, "GlassModManagerGui.openPage(GMM_RTBAddonPage, " @ expandEscape(%obj.id) @ ");");

  %container.nav = GMM_Navigation.createSwatch();
  %container.add(%container.nav);

  %body = %this.body;

  %body.placeBelow(%container.nav, 10);

  %body.rtb = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 12";
    extent = "32 32";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bricks_large.png";
  };
  %body.add(%body.rtb);

  %body.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:20><just:left>" @ %obj.title;
    position = "52 10";
    extent = "553 20";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.title);

  %body.author = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana:12><just:left>Uploaded by " @ %obj.author @ "<just:right><color:444444>" @ %obj.date;
    position = "52 20";
    extent = "553 12";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.author);
  %body.author.placeBelow(%body.title, 0);
  %body.verticalMatchChildren(20, 10);
  %body.rtb.centerY();

  %downloads = %obj.downloads;

  %info = 4;
  %border = 5;
  %width = mFloor((615+%border)/%info);
  for(%i = 0; %i < %info; %i++) {
    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "255 255 255 255";
      position = (%width)*%i+10 SPC 0;
      extent = %width-%border SPC 36;
    };

    %swatch.image = new GuiBitmapCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "16 16";
      bitmap = "";
    };

    %swatch.text = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "";
      position = "0 11";
      extent = (%width-%border) SPC "13";
      minextent = (%width-20) SPC "13";
    };

    switch(%i) {
      case 0:
        %swatch.text.setText("<font:verdana:13><just:center>RTB Archive");
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/bricks.png");

      case 1:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.type);
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/category.png");

      case 2:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.filename);
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/folder_vertical_zipper.png");

      case 3:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.downloads+0 @ " downloads");
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/inbox_download.png");
    }

    %swatch.add(%swatch.text);
    %swatch.add(%swatch.image);
    %container.add(%swatch);

    %swatch.placeBelow(%body, %border);
    %container.info[%i] = %swatch;
  }

  for(%i = 0; %i < getWordCount(%obj.description); %i++) {
    %word = getWord(%obj.description, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0 || strpos(%word, "glass://") == 0) {
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %obj.description = setWord(%obj.description, %i, %word);
    }
  }

  %description = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 0";
    extent = "615 30";
  };

  %description.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:13>Description<br><br><color:444444><font:verdana:13>" @ trim(%obj.description);
    position = "10 10";
    extent = "595 16";
    minextent = "0 0";
    autoResize = true;
  };

  %description.add(%description.text);

  %container.description = %description;
  %container.add(%container.description);
  %container.description.placeBelow(%container.info0, 5);

  %container.description.text.forceReflow();
  %container.description.verticalMatchChildren(10, 10);

  %download = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 0";
    extent = "615 30";
  };

  %download.dlButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonWhiteProfile";
    position = mfloor((595/2)-60) SPC 10;
    extent = "120 35";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

    text = "Download";

    command = "GMM_RTBAddonPage.downloadClick(" @ %obj.aid @ ");";

    mColor = "231 76 60 255";
  };

  %download.add(%download.dlButton);
  %download.verticalMatchChildren(30, 10);

  %container.download = %download;
  %container.add(%download);
  if(%obj.screenshots.length)
    %container.download.placeBelow(%container.screenshots, 10);
  else
    %container.download.placeBelow(%container.description, 10);

  %container.verticalMatchChildren(0, 10);
  GlassModManagerGui.resizePage();
}
