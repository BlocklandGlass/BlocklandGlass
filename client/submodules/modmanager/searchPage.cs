function GMM_SearchPage::init() {
  new ScriptObject(GMM_SearchPage);
}

function GMM_SearchPage::open(%this, %preserve) {
  GMM_Navigation.clear();
  GMM_Navigation.addStep("Search", "GlassModManagerGui.openPage(GMM_SearchPage, true);");

  if(isObject(%this.container)) {
    GlassModManagerGui.schedule(0, pageDidLoad, %this);
    return %this.container;
    //%this.container.deleteAll();
    //%this.container.delete(); //for development only
  }

  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "635 498";
  };

  %container.searchBar = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 50";
  };

  %container.search = new GuiTextEditCtrl(GMM_SearchPage_SearchBar) {
    profile = "GlassSearchBarProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "595 29";
    text = "\c1Search...";
    command = "GMM_SearchPage_SearchBar.onUpdate();";
    accelerator = "enter";
    altcommand = "GMM_SearchPage.search();";
    filler = 1;
  };

  %container.searchBar.add(%container.search);
  %container.add(%container.searchBar);

  %container.searchOptions = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 75";

    new GuiTextCtrl() {
      profile = "GuiTextVerdanaProfile";
      position = "10 27";
      extent = "200 14";
      text = "Search in board:";
    };

    new GuiTextCtrl() {
      profile = "GuiTextVerdanaProfile";
      position = "10 52";
      extent = "200 14";
      text = "Search by author:";
    };
  };

  %container.searchOptions.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:13>Search Options";
    position = "10 7";
    extent = "595 13";
    minextent = "0 0";
    autoResize = true;
  };

  %container.searchOptions.rtb = new GuiCheckBoxCtrl(GMM_SearchPage_RTB) {
    profile = "GlassCheckBoxProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "480 20";
    extent = "200 20";
    text = "Search RTB Archive";
    clipToParent = "1";
    buttonType = "ToggleButton";
    command = "GMM_SearchPage.search();";
  };

  %container.searchOptions.blf = new GuiCheckBoxCtrl() {
    profile = "GlassCheckBoxProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "480 40";
    extent = "200 20";
    text = "Search BLF Index";
    clipToParent = "1";
    buttonType = "ToggleButton";
    visible = false;
  };

  %container.searchOptions.board = new GuiPopUpMenuCtrl(GMM_SearchPage_Board) {
    profile = "GuiPopUpMenuProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "105 25";
    extent = "100 20";
    clipToParent = "1";
    text = "Loading...";
    enabled = false;
  };

  %container.searchOptions.author = new GuiTextEditCtrl(GMM_SearchPage_Author) {
    profile = "GlassTextEditProfile";
    horizSizing = "right";
    vertSizing = "center";
    position = "106 52";
    extent = "100 20";
    clipToParent = "1";
    text = "";
    command = "GMM_SearchPage.search();";
  };

  %container.searchOptions.add(%container.searchOptions.text);

  %container.searchOptions.add(%container.searchOptions.rtb);
  %container.searchOptions.add(%container.searchOptions.blf);

  %container.searchOptions.add(%container.searchOptions.board);
  %container.searchOptions.add(%container.searchOptions.author);

  %container.searchOptions.board.add("Loading...", -1);
  GlassModManager::placeCall("boards", "", "GMM_SearchPage.handleBoardsResult");

  //%container.searchOptions.rtb.placeBelow(%container.searchOptions.text);
  //%container.searchOptions.blf.placeBelow(%container.searchOptions.rtb);

  %container.add(%container.searchOptions);
  %container.searchOptions.verticalMatchChildren(26, 10);
  %container.searchOptions.placeBelow(%container.searchBar, 5);

  %this.container = %container;

  GlassModManagerGui.schedule(0, pageDidLoad, %this);
  return %container;
}

function GMM_SearchPage::close(%this) {
  %this.container.getGroup().remove(%this.container);
}

function GMM_SearchPage::handleBoardsResult(%this, %obj) {
  %container = %this.container;
  %container.searchOptions.board.clear();
  %container.searchOptions.board.add("All", -1);
  for(%j = 0; %j < %obj.groups.length; %j++) {
    %group = %obj.groups.value[%j];
    %boards = %group.boards;
    for(%i = 0; %i < %boards.length; %i++) {
      %board = %boards.value[%i];
      %container.searchOptions.board.add(%board.name, %board.id);
    }
  }

  %container.searchOptions.board.setValue("All");
  %container.searchOptions.board.enabled = true;
}

function GMM_SearchPage::handleSearchResults(%this, %res) {
  %container = %this.container;

  if(isObject(%container.searchResults)) {
    %container.searchResults.deleteAll();
    %container.searchResults.delete();
  }

  %results = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 75";
  };

  if(%res.results.length == 0) {
    %results.text = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<font:verdana bold:13><just:center>No Results";
      position = "10 30";
      extent = "595 33";
      minextent = "0 0";
      autoResize = true;
    };
    %results.add(%results.text);
  } else {
    %results.text = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<font:verdana bold:13>Search Results";
      position = "10 7";
      extent = "595 13";
      minextent = "0 0";
      autoResize = true;
    };
    %results.add(%results.text);

    for(%i = 0; %i < %res.results.length; %i++) {
      %result = %res.results.value[%i];

      if(!%result.isRTB) {
        %resultSwatch = new GuiSwatchCtrl() {
          horizSizing = "right";
          vertSizing = "bottom";
          color = (%odd = !%odd) ? "240 240 240 255" : "235 235 235 255";
          position = "10 25";
          extent = "595 30";

          id = %result.id;
          isRTB = false;
        };

        %resultSwatch.text = new GuiMLTextCtrl() {
          horizSizing = "right";
          vertSizing = "bottom";
          text = "<font:verdana bold:13>" @ %result.title @ "<font:verdana:13> by " @ %result.author.username @ "<br><color:555555><font:verdana:12>" @ (%result.summary $= "" ? "< No Summary > " : %result.summary);
          position = "10 10";
          extent = "575 25";
          minextent = "0 0";
          autoResize = true;
        };
      } else {
        %resultSwatch = new GuiSwatchCtrl() {
          horizSizing = "right";
          vertSizing = "bottom";
          color = (%odd = !%odd) ? "240 240 240 255" : "235 235 235 255";
          position = "10 25";
          extent = "595 30";

          id = %result.rtb_id;
          isRTB = true;
        };

        %resultSwatch.image = new GuiBitmapCtrl() {
          profile = "GuiDefaultProfile";
          extent = "16 16";
          position = "10 10";
          bitmap = "Add-Ons/System_BlocklandGlass/image/icon/bricks.png";

          enabled = "1";
          visible = "1";
          clipToParent = "1";

          wrap = "0";
          lockAspectRatio = "0";
          alignLeft = "0";
          alignTop = "0";

          keepCached = "1";
        };

        %resultSwatch.text = new GuiMLTextCtrl() {
          horizSizing = "right";
          vertSizing = "bottom";
          text = "<font:verdana bold:13>" @ %result.title @ "<font:verdana:13> (" @ %result.filename @ ")<br><color:555555><font:verdana:12>RTB Archive: " @ %result.type;
          position = "33 10";
          extent = "549 25";
          minextent = "0 0";
          autoResize = true;
        };

        %resultSwatch.add(%resultSwatch.image);
      }

      %resultSwatch.add(%resultSwatch.text);
      %results.add(%resultSwatch);
      %resultSwatch.verticalMatchChildren(0, 10);

      if(%result.isRTB)
        %resultSwatch.image.centerY();

      if(%last)
        %resultSwatch.placeBelow(%last, 5);

      GlassHighlightSwatch::addToSwatch(%resultSwatch, "5 5 5", "GMM_SearchPage.interact");

      %last = %resultSwatch;
    }
  }
  %results.verticalMatchChildren(0, 10);

  %container.add(%results);
  %results.placeBelow(%container.searchOptions, 10);

  %container.searchResults = %results;

  %container.verticalMatchChildren(0, 10);
  GlassModManagerGui.resizePage();
}

function GMM_SearchPage::interact(%this, %swatch) {
  if(%swatch.isRTB) {
    echo(%swatch.id);
    GlassModManagerGui.openPage(GMM_RTBAddonPage, %swatch.id);
  } else {
    GlassModManagerGui.openPage(GMM_AddonPage, %swatch.id);
  }
}

function GMM_SearchPage_SearchBar::onUpdate(%this, %a) {
  %text = %this.getValue();
  if(%this.filler) {
    if(strlen(%text) < 10) {
      %this.setValue("\c1Search...");
      %this.setCursorPos(0);
    } else {
      %char = getsubstr(%text, %this.getCursorPos()-1, 1);
      %text = %char;
      %this.setValue(%text);
      %this.filler = false;
    }
  }

  if(strlen(%text) == 0) {
    %this.filler = true;
    %this.setValue("\c1Search...");
    %this.setCursorPos(0);
  }

  if(GlassSettings.get("MM::LiveSearch")) {
    GMM_SearchPage.search();
  }
}

function GMM_SearchPage::search(%this) {

  //name
  if(GMM_SearchPage_SearchBar.filler)
    %query = "";
  else
    %query = trim(GMM_SearchPage_SearchBar.getValue());

  if(strlen(%query) > 0) {
    %search = "name" TAB %query;
  }

  //author
  %author = trim(GMM_SearchPage_Author.getValue());
  if(strlen(%author) > 0) {
    %search = trim(%search NL "author" TAB %author);
  }

  //rtb
  if(GMM_SearchPage_RTB.getValue() && strlen(%query) > 0) {
    %search = trim(%search NL "rtb\t1");
  }

  if(%author $= "" && %query $= "") {
    %this.container.searchResults.deleteAll();
    %this.container.searchResults.delete();
    return;
  }

  //search
  %this.lastTCP = GlassModManager::placeCall("search", %search, "GMM_SearchPage.handleSearchResults");
}
