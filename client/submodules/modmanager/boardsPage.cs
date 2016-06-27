function GlassModManagerGui::renderBoards(%boards) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %body = new GuiSwatchCtrl(GlassModManagerGui_AddonDisplay) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "485 64";
  };

  %searchRes = new GuiSwatchCtrl(GlassModManagerGui_SearchResults) {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "485 64";
    visible = false;
  };

  GlassModManagerGui_AddonDisplay.deleteAll();

  %ypos = 10;

  %dark = 0;

  for(%i = 0; %i < getLineCount(%boards); %i++) {
    %dark = !%dark;

    %board = getLine(%boards, %i);
    %name = getField(%board, 0);
    %id = getField(%board, 1);
    %desc = getField(%desc, 2);
    %img = getField(%desc, 3);

    %contain = GlassModManagerGui::createBoardButton(%name, "star", %id);
    %contain.position = 10 SPC %yPos;
    %contain.text.centerY();

    %contain.color = (%dark ? "210 210 210 255" : "220 220 220 255");

    %yPos += 35;

    GlassModManagerGui_AddonDisplay.add(%contain);
    %contain.mouse.onAdd();
  }

  %rtb = GlassModManagerGui::createBoardButton("Return to Blockland Archive", "bricks", "rtb");
  %rtb.position = 10 SPC %yPos;
  %rtb.text.centerY();
  %rtb.color = (!%dark ? "210 210 210 255" : "220 220 220 255");
  %rtb.ocolor = %rtb.color;
  GlassModManagerGui_AddonDisplay.add(%rtb);
  %rtb.mouse.delete();

  %rtb.mouse = new GuiMouseEventCtrl(GlassModManagerGui_RTBButton) {
    extent = %rtb.extent;
    position = "0 0";
    swatch = %rtb;
  };
  %rtb.add(%rtb.mouse);

  GlassModManagerGui_AddonDisplay.verticalMatchChildren(0, 10);

  %search = GlassModManagerGui::createSearchBar();
  %container.add(%search);
  %container.add(%body);
  %container.add(%searchRes);
  %body.placeBelow(%search, 10);
  %searchRes.placeBelow(%search, 10);

  %container.verticalMatchChildren(498, 10);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;
  GlassModManagerGui_MainDisplay.setVisible(true);

  //%container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
}

function GlassModManagerGui::createSearchBar() {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "485 50";
  };

  %search = new GuiTextEditCtrl(GlassModManagerGui_SearchBar) {
    profile = "GlassSearchBarProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "465 29";
    text = "\c1Search...";
    command = "GlassModManagerGui_SearchBar.onUpdate();";
    accelerator = "enter";
    altcommand = "GlassModManagerGui_SearchBar.search();";
    filler = 1;
  };

  %container.add(%search);
  return %container;
}

function GlassModManagerGui::SearchResults(%res) {
  GlassModManagerGui_SearchResults.deleteAll();
  %y = 10;
  for(%i = 0; %i < %res.length; %i++) {
    %result = %res.value[%i];

    %swat = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "235 235 235 255";
      position = 10 SPC %y;
      extent = "465 50";
    };

    %search = new GuiTextCtrl() {
      profile = "GlassSearchResultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      color = "255 255 255 255";
      position = "10 10";
      extent = "465 25";
      text = %result.title;
      filler = 1;
    };
    %swat.add(%search);
    %swat.verticalMatchChildren(0, 5);
    %y += getWord(%swat.getExtent(), 1) + 5;
    GlassModManagerGui_SearchResults.add(%swat);
  }

  if(%res.length == 0) {
    %text = new GuiTextCtrl() {
      profile = "GlassSearchResultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      color = "255 255 255 255";
      position = "10 10";
      extent = "465 25";
      text = "No results found";
      filler = 1;
    };
    GlassModManagerGui_SearchResults.add(%text);
  }

  GlassModManagerGui_SearchResults.verticalMatchChildren(20, 10);
}

function GlassModManagerGui_SearchBar::onUpdate(%this, %a) {
  %text = %this.getValue();
  if(%this.filler) {
    if(strlen(%text) < 10) {
      %this.setValue("\c1Search...");
      %this.setCursorPos(0);
      GlassModManagerGui_SearchResults.setVisible(false);
      GlassModManagerGui_AddonDisplay.setVisible(true);
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
    GlassModManagerGui_SearchResults.setVisible(false);
    GlassModManagerGui_AddonDisplay.setVisible(true);
  }

  if(!%this.filler) {
    if(GlassModManager.liveSearch) {
      GlassModManagerGui_SearchBar.search();
    }
  }
}

function GlassModManagerGui_SearchBar::search(%this) {
  %query = trim(%this.getValue());
  if(%this.filler)
    return;

  if(strlen(%query) == 0)
    return;

  GlassModManagerGui_SearchResults.clear();
  %loading = GlassModManagerGui::createLoadingAnimation();
  GlassModManagerGui_SearchResults.add(%loading);
  %loading.forceCenter();


  GlassModManagerGui_SearchResults.setVisible(true);
  GlassModManagerGui_AddonDisplay.setVisible(false);

  %this.lastTCP = GlassModManager::placeCall("search", "type\taddon\nby\tname" NL "query" TAB %query);
}

function GlassModManagerGui::createBoardButton(%name, %img, %id) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "200 200 200 255";
    position = "10 10";
    extent = "465 35";
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
    text = "<font:verdana bold:16>" @ %name;
    position = "35 0";
    extent = "225 16";
  };

  %container.mouse = new GuiMouseEventCtrl(GlassModManagerGui_BoardButton) {
    bid = %id;
    swatch = %container;
  };

  %container.add(%container.icon);
  %container.add(%container.text);
  %container.add(%container.mouse);

  return %container;
}

function GlassModManagerGui_RTBButton::onMouseDown(%this) {
  %this.lastTCP = GlassModManager::placeCall("board", "id\trtb\npage\t1");
}

function GlassModManagerGui_RTBButton::onMouseEnter(%this) {
  %this.swatch.color = vectoradd(%this.swatch.color, "20 20 20");
}

function GlassModManagerGui_RTBButton::onMouseLeave(%this) {
  %this.swatch.color = %this.swatch.ocolor;
}
