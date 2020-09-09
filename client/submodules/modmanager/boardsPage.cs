function GMM_BoardsPage::init() {
  GlassGroup.add(new ScriptObject(GMM_BoardsPage) {
    class = "GlassModManagerPage";
  });
}

function GMM_BoardsPage::open(%this) {

  GMM_Navigation.clear();
  GMM_Navigation.addStep("Boards", "GlassModManagerGui.openPage(GMM_BoardsPage);");

  if(%this.loaded && isObject(%this.container)) {
    GlassModManagerGui.schedule(0, pageDidLoad, %this);
    return %this.container;
  } else {
    if(isObject(%this.container)) {
      %this.container.deleteAll();
      %this.container.delete();
    }
  }

  %this.container = new GuiSwatchCtrl(GMM_BoardsContainer) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "635 498";
  };

  %this.container.nav = GMM_Navigation.createSwatch();
  %this.container.add(%this.container.nav);

  GlassModManagerGui.setLoading(true);
  GlassModManager::placeCall("boards", "", "GMM_BoardsPage.handleResults");

  return %this.container;
}

function GMM_BoardsPage::close(%this) {
  if(isObject(%this.container)) {
    if(isObject(%this.container.getGroup())) {
      %this.container.getGroup().remove(%this.container);
    }
  }
}

function GMM_BoardsPage::handleResults(%this, %obj) {
  GlassModManagerGui.pageDidLoad(%this);
  %this.loaded = true;
  %container = %this.container;

  GlassModManagerGui.setLoading(false);

  %body = new GuiSwatchCtrl(GMM_BoardsDisplay) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 64";
  };
  %body.placeBelow(%container.nav, 10);

  %last = "";

  for(%j = 0; %j < %obj.groups.length; %j++) {
    %group = %obj.groups.value[%j];

    %boards = %group.boards;

    %header = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "84 217 140 255";
      position = "10 10";
      extent = "595 25";
    };

    %header.text = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<just:center><font:Verdana Bold:15><color:ffffff>" @ %group.name;
      position = "0 0";
      extent = "595 16";
    };

    %header.add(%header.text);
    %header.text.centerY();
    GMM_BoardsDisplay.add(%header);

    if(isObject(%last)) {
      %header.placeBelow(%last, 20);
    }

    %last = %header;

    %dark = 0;

    for(%i = 0; %i < %boards.length; %i++) {
      %board = %boards.value[%i];
      %dark = !%dark;

      %name = %board.name;
      %id = %board.id;
      %img = %board.icon;

      %img = isFile("Add-Ons/System_BlocklandGlass/image/icon/" @ %img @ ".png") ? %img : "ask_and_answer";

      %contain = GMM_BoardsPage::createBoardButton(%name, %img, %id);

      %contain.placeBelow(%last, 0);
      %contain.text.centerY();

      %contain.color = (%dark ? "235 235 235 255" : "230 230 230 255");
      %contain.hcolor = "240 240 240 255";


      GMM_BoardsDisplay.add(%contain);
      %last = %contain;
    }
  }

  %rtb = GMM_BoardsPage::createBoardButton("Return to Blockland Archive", "bricks", "rtb");
  %rtb.placeBelow(%last, 20);
  %rtb.text.centerY();
  %rtb.color = (!%dark ? "210 210 210 255" : "220 220 220 255");
  %rtb.ocolor = %rtb.color;
  GMM_BoardsDisplay.add(%rtb);

  %rtb.mouse = new GuiMouseEventCtrl(GMM_BoardsPage_RTBButton) {
    extent = %rtb.extent;
    position = "0 0";
    swatch = %rtb;
  };
  %rtb.add(%rtb.mouse);

  GMM_BoardsDisplay.verticalMatchChildren(0, 10);

  %container.add(%body);

  %container.verticalMatchChildren(498, 10);

  GlassModManagerGui.resizePage();
}

function GlassModManagerGui::createSearchBar() {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 50";
  };

  %search = new GuiTextEditCtrl(GlassModManagerGui_SearchBar) {
    profile = "GlassSearchBarProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "595 29";
    text = "\c1Search...";
    command = "GlassModManagerGui_SearchBar.onUpdate();";
    accelerator = "enter";
    altcommand = "GlassModManagerGui_SearchBar.search();";
    filler = 1;
  };

  %container.add(%search);
  return %container;
}

function GMM_BoardsPage::createBoardButton(%name, %img, %id) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "200 200 200 255";
    position = "10 10";
    extent = "595 35";

    boardId = %id;
  };

  %container.icon = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %img;
    position = "10 10";
    extent = "16 16";
    minextent = "0 0";
    clipToParent = true;
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:Verdana Bold:15>" @ %name;
    position = "35 0";
    extent = "225 16";
  };


  %container.add(%container.icon);
  %container.add(%container.text);

  GlassHighlightSwatch::addToSwatch(%container, "0 0 0", "GMM_BoardsPage::clickBoard");

  return %container;
}

function GMM_BoardsPage::clickBoard(%this) {
  GlassModManagerGui.openPage(GMM_BoardPage, %this.boardId);
}

function GMM_BoardsPage_RTBButton::onMouseDown(%this) {
  %this.down = true;
}

function GMM_BoardsPage_RTBButton::onMouseUp(%this) {
  if(%this.down) {
    GlassModManagerGui.openPage(GMM_RTBBoardsPage);
  }
  %this.down = false;
}

function GMM_BoardsPage_RTBButton::onMouseEnter(%this) {
  %this.swatch.color = vectoradd(%this.swatch.color, "20 20 20");
}

function GMM_BoardsPage_RTBButton::onMouseLeave(%this) {
  %this.swatch.color = %this.swatch.ocolor;
  %this.down = false;
}
