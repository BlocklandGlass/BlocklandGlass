function GMM_BoardPage::init() {
  GlassGroup.add(new ScriptObject(GMM_BoardPage));
}

function GMM_BoardPage::open(%this, %id, %page) {
  if(%page < 1) %page = 1;
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "635 498";
  };

  %call = GlassModManager::placeCall("board", "id" TAB %id NL "page" TAB %page, "GMM_BoardPage.handleResults");

  GlassModManagerGui.setLoading(true);

  %this.container = %container;
  %this.call = %call;

  return %container;
}

function GMM_BoardPage::close(%this) {
  %this.container.deleteAll();
  %this.open = false;
}

function GMM_BoardPage::handleResults(%this, %res) {
  if(%this.open) {
    GMM_Navigation.steps--;
  }
  GlassModManagerGui.pageDidLoad(%this);

  GlassModManagerGui.setLoading(false);
  %status = %res.status;

  %id = %res.board_id;
  %name = %res.board_name;
  %page = %res.page;
  %pages = %res.pages;

  %addons = %res.addons;

  %container = %this.container;
  GMM_Navigation.addStep(%name, "GlassModManagerGui.openPage(GMM_BoardPage, \"" @ expandEscape(%id) @ "\", " @ expandEscape(%page) @ ");");

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

  %nav = GMM_BoardPage::createBoardNav(%id, %page, %pages);
  %header = GMM_BoardPage::createBoardHeader(%name);

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
    %addonName = getASCIIString(%addon.name);

    if(strlen(%addonName) > 32)
      %addonName = getsubstr(%addonName, 0, 32) @ "...";

    %author = getASCIIString(%addon.author);
    %downloads = %addon.downloads;

    if(trim(%addon.summary) $= "")
      %summary = "< No Summary >";
    else
      %summary = getASCIIString(%addon.summary);

    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = (%odd = !%odd) ? "235 235 235 255" : "230 230 230 255";
      position = "10 10";
      extent = "595 50";

      aid = %aid;
    };

    %swatch.title = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<color:444444><font:Verdana Bold:15>" @ %addonName @ "<br><font:verdana:13>" @ %summary;
      position = "10 5";
      extent = "325 45";
    };

    %swatch.author = new GuiMLTextCtrl() {
      horizSizing = "left";
      vertSizing = "bottom";
      text = "<color:333333><just:center><font:verdana:13>" @ %author;
      position = "235 5";
      extent = "120 16";
    };

    %swatch.downloads = new GuiMLTextCtrl() {
      horizSizing = "left";
      vertSizing = "bottom";
      text = "<color:333333><font:verdana:13><just:right>" @ %downloads;
      position = "500 5";
      extent = "85 45";
    };

    %swatch.add(%swatch.title);
    %swatch.add(%swatch.author);
    %swatch.add(%swatch.downloads);

    GlassHighlightSwatch::addToSwatch(%swatch, "10 10 10", "GMM_BoardPage.swatchClick");
    %swatch.hColor = "240 240 240 255";

    %body.add(%swatch);
    %swatch.placeBelow(%last, 0);

    %last = %swatch;
  }

  %body.verticalMatchChildren(10, 10);
  %container.verticalMatchChildren(498, 0);
  GlassModManagerGui.resizePage();
}

function GMM_BoardPage::swatchClick(%this, %swatch) {
  %obj = GlassModManagerGui.openPage(GMM_AddonPage, %swatch.aid);
}

function _glassPageNav(%board, %id) {
  return "<a:glass://board=" @ %board @ "&page=" @ %id @ ">" @ %id @ "</a>";
}

function GMM_BoardPage::createBoardNav(%bid, %page, %pages) {
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
      %pageText = _glassPageNav(%bid, "1") SPC "..." SPC "";
    } else if(%page > 2) {
      %pageText = _glassPageNav(%bid, "1") SPC "";
    }

    if(%page < 2) {
      %pageText = %pageText @ "[" @ %page @ "]" SPC _glassPageNav(%bid, %page+1);
    } else if(%page+1 > %pages) {
      %pageText = %pageText @ _glassPageNav(%bid, %page-1) SPC "[" @ %page @ "]";
    } else {
      %pageText = %pageText @ _glassPageNav(%bid, %page-1) SPC "[" @ %page @ "]" SPC _glassPageNav(%bid, %page+1);
    }

    if(%page+2 == %pages) {
      %pageText = %pageText SPC _glassPageNav(%bid, %pages);
    } else if(%page+2 < %pages) {
      %pageText = %pageText SPC "..." SPC _glassPageNav(%bid, %pages);
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

function GMM_BoardPage::createBoardHeader(%title) {
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
