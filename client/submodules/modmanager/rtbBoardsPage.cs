function GMM_RTBBoardsPage::init() {
  GlassGroup.add(new ScriptObject(GMM_RTBBoardsPage) {
    class = "GlassModManagerPage";
  });
}

function GMM_RTBBoardsPage::open(%this) {

  GMM_Navigation.addStep("RTB Archive", "GlassModManagerGui.openPage(GMM_RTBBoardsPage);");

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
  GlassModManager::placeCall("rtbBoards", "", "GMM_RTBBoardsPage.handleResults");

  return %this.container;
}

function GMM_RTBBoardsPage::close(%this) {
  if(isObject(%this.container)) {
    //preserve the gui object
    %this.container.getGroup().remove(%this.container);
  }
}

function GMM_RTBBoardsPage::handleResults(%this, %obj) {
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
    text = "<just:center><font:Verdana Bold:15><color:ffffff>Return to Blockland Archive";
    position = "0 0";
    extent = "595 16";
  };

  %header.add(%header.text);
  %header.text.centerY();
  GMM_BoardsDisplay.add(%header);

  %last = %header;

  %dark = 0;

  %boards = %obj.boards;
  for(%i = 0; %i < %boards.length; %i++) {
    %dark = !%dark;

    %name = %boards.value[%i];
    %img = "bricks";

    %img = isFile("Add-Ons/System_BlocklandGlass/image/icon/" @ %img @ ".png") ? %img : "ask_and_answer";

    %contain = GMM_RTBBoardsPage::createBoardButton(%name, %img);

    %contain.placeBelow(%last, 0);
    %contain.text.centerY();

    %contain.color = (%dark ? "235 235 235 255" : "230 230 230 255");
    %contain.hcolor = "240 240 240 255";


    GMM_BoardsDisplay.add(%contain);
    %last = %contain;
  }

  GMM_BoardsDisplay.verticalMatchChildren(0, 10);

  %container.add(%body);
  %container.verticalMatchChildren(498, 10);

  GlassModManagerGui.resizePage();
}

function GMM_RTBBoardsPage::createBoardButton(%name, %img) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "200 200 200 255";
    position = "10 10";
    extent = "595 35";

    name = %name;
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

  GlassHighlightSwatch::addToSwatch(%container, "0 0 0", "GMM_RTBBoardsPage::clickBoard");

  return %container;
}

function GMM_RTBBoardsPage::clickBoard(%this) {
  GlassModManagerGui.openPage(GMM_RTBBoardPage, %this.name);
}
