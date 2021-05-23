function GMM_RTBBoardPage::init() {
  GlassGroup.add(new ScriptObject(GMM_RTBBoardPage));
}

function GMM_RTBBoardPage::open(%this, %boardName, %page) {
  if(%page < 1) %page = 1;
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "635 498";
  };

  %call = GlassModManager::placeCall("rtbBoard", "name" TAB %boardName NL "page" TAB %page, "GMM_RTBBoardPage.handleResults");

  GlassModManagerGui.setLoading(true);

  %this.container = %container;
  %this.call = %call;

  return %container;
}

function GMM_RTBBoardPage::close(%this) {
  //nothing
  %this.open = false;
}

function GMM_RTBBoardPage::handleResults(%this, %res) {
  %status = %res.status;

  %name = %res.board_name;
  %page = %res.page;
  %pages = %res.pages;

  %addons = %res.addons;

  %container = %this.container;

  if(%this.open) {
    GMM_Navigation.steps--;
  }

  GMM_Navigation.addStep(%name, "GlassModManagerGui.openPage(GMM_RTBBoardPage, \"" @ expandEscape(%name) @ "\", " @ expandEscape(%page) @ ");");

  GlassModManagerGui.pageDidLoad(%this);
  GlassModManagerGui.setLoading(false);

  %this.open = true;

  %container.nav = GMM_Navigation.createSwatch();
  %container.add(%container.nav);

  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 10";
  };
  %container.add(%body);

  %body.placeBelow(%container.nav, 10);

  %nav = GMM_RTBBoardPage::createBoardNav(%name, %page, %pages);
  %header = GMM_RTBBoardPage::createBoardHeader(%name);

  %body.add(%nav);
  %body.add(%header);
  %header.placeBelow(%nav);

  %last = %header;

  if(%addons.length == 0) {
    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = (%odd = !%odd) ? "235 235 235 255" : "230 230 230 255";
      position = "10 10";
      extent = "595 40";

      aid = %aid;
    };

    %swatch.title = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<color:666666><font:Verdana Bold:12><just:center>No Add-Ons Found";
      position = "0 5";
      extent = "595 15";
    };

    %swatch.add(%swatch.title);
    %swatch.title.centerY();

    %body.add(%swatch);
    %swatch.placeBelow(%last, 0);
  }

  for(%i = 0; %i < %addons.length; %i++) {
    %addon = %addons.value[%i];

    %aid = %addon.id;
    %addonName = getASCIIString(%addon.title);
    %author = getASCIIString(%addon.author);
    %summary = getASCIIString(%addon.description);

    if(%summary $= "")
      %summary = "< Missing Summary >";

    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = (%odd = !%odd) ? "235 235 235 255" : "230 230 230 255";
      position = "10 10";
      extent = "595 40";

      aid = %aid;
    };

    %swatch.title = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<color:444444><font:Verdana Bold:15>" @ %addonName @ "<br><font:verdana:12>" @ trim(%summary);
      position = "10 5";
      extent = "385 10";
    };

    %swatch.author = new GuiMLTextCtrl() {
      horizSizing = "left";
      vertSizing = "bottom";
      text = "<color:333333><just:center><font:verdana:13>By " @ %author;
      position = "415 12";
      extent = "150 16";
    };


    %swatch.add(%swatch.title);
    %swatch.add(%swatch.author);

    %body.add(%swatch);
    %swatch.placeBelow(%last, 0);

    %swatch.author.forceReflow();
    %swatch.title.forceReflow();

    %swatch.verticalMatchChildren(10, 5);

    %swatch.author.centerY();

    GlassHighlightSwatch::addToSwatch(%swatch, "10 10 10", "GMM_RTBBoardPage.swatchClick");
    %swatch.hColor = "240 240 240 255";

    %last = %swatch;
  }

  %body.verticalMatchChildren(10, 10);
  %container.verticalMatchChildren(498, 0);

  GlassModManagerGui.resizePage();
}

function GMM_RTBBoardPage::swatchClick(%this, %swatch) {
  %obj = GlassModManagerGui.openPage(GMM_RTBAddonPage, %swatch.aid);
}

function _glassRTBPageNav(%board, %id) {
  return "<a:glass://rtbBoard=" @ strReplace(%board, " ", "_") @ "&page=" @ %id @ ">" @ %id @ "</a>";
}

function GMM_RTBBoardPage::createBoardNav(%bid, %page, %pages) {
  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "100 100 100 0";
    position = "10 10";
    extent = "595 25";
  };

  if(%pages <= 1) {
    %pageText = "[1]";
  } else {
    if(%page > 3) {
      %pageText = _glassRTBPageNav(%bid, "1") SPC "..." SPC "";
    } else if(%page > 2) {
      %pageText = _glassRTBPageNav(%bid, "1") SPC "";
    }

    if(%page < 2) {
      %pageText = %pageText @ "[" @ %page @ "]" SPC _glassRTBPageNav(%bid, %page+1);
    } else if(%page+1 > %pages) {
      %pageText = %pageText @ _glassRTBPageNav(%bid, %page-1) SPC "[" @ %page @ "]";
    } else {
      %pageText = %pageText @ _glassRTBPageNav(%bid, %page-1) SPC "[" @ %page @ "]" SPC _glassRTBPageNav(%bid, %page+1);
    }

    if(%page+2 == %pages) {
      %pageText = %pageText SPC _glassRTBPageNav(%bid, %pages);
    } else if(%page+2 < %pages) {
      %pageText = %pageText SPC "..." SPC _glassRTBPageNav(%bid, %pages);
    }
  }

  %swatch.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassModManagerMLProfile";
    text = "<color:333333><font:verdana:15><just:right>" @ %pageText;
    position = "0 0";
    extent = "595 45";
  };

  %swatch.add(%swatch.text);
  return %swatch;
}

function GMM_RTBBoardPage::createBoardHeader(%title) {
  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "46 204 113 255";
    position = "10 10";
    extent = "595 30";
  };

  %swatch.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<color:ffffff><font:verdana bold:15><just:center>" @ %title;
    position = "0 5";
    extent = "595 15";
  };

  %swatch.add(%swatch.text);
  %swatch.verticalMatchChildren(0, 5);
  return %swatch;
}

function GMM_RTBBoardPage::createStars(%stars) {
  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "10 10";
    extent = "100 16";
  };

  %fullStars = mfloor(%stars);
  %fracStar = mfloor((%stars - %fullStars + 0.125)*4);
  %emptyStars = 4-%fullStars;
  %x = 0;
  for(%i = 0; %i < %fullStars; %i++) {
    %swatch.star[%i] = new GuiBitmapCtrl() {
      horizSizing = "left";
      vertSizing = "bottom";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star.png";
      position = %x SPC 0;
      extent = "16 16";
      minextent = "0 0";
      clipToParent = true;
    };
    %swatch.add(%swatch.star[%i]);
    %x += 20;
  }

  if(%fracStar != 0) {
    if(%fracStar > 3)
      %fracStar = 3;

    %swatch.fracstar = new GuiBitmapCtrl() {
      horizSizing = "left";
      vertSizing = "bottom";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star_frac_" @ %fracStar @ ".png";
      position = %x SPC 0;
      extent = "16 16";
      minextent = "0 0";
      clipToParent = true;
    };
    %swatch.add(%swatch.fracstar);
    %x += 20;
  } else {
    %emptyStars++;
  }

  for(%i = 0; %i < %emptyStars; %i++) {
    %swatch.emptystar[%i] = new GuiBitmapCtrl() {
      horizSizing = "left";
      vertSizing = "bottom";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star_empty.png";
      position = %x SPC 0;
      extent = "16 16";
      minextent = "0 0";
      clipToParent = true;
    };
    %swatch.add(%swatch.emptystar[%i]);
    %x += 20;
  }

  return %swatch;
}
