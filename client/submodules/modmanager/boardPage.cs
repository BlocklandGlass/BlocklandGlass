function GlassModManagerGui::fetchBoard(%id, %page) {
  if(%page < 1) %page = 1;

  GlassModManager::placeCall("board", "id" TAB %id NL "page" TAB %page);
  
  // %id = 1;
  // %name = "Blockland Glass";
  // %author = "Jincux";
  // %rating = 3.2;
  // %downloads = 17381;
  // %listing = %id TAB %name TAB %author TAB %rating TAB %downloads;
  // GlassModManagerGui::renderBoardPage(1, "Client Mods", %listing NL %listing, 11, 14);
}

function GlassModManagerGui::renderBoardPage(%id, %title, %listings, %page, %maxpage, %rtb) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "635 498";
  };

  %nav = GlassModManagerGui::createBoardNav(%id, %page, %maxpage);
  %header = GlassModManagerGui::createBoardHeader(%title);

  %container.add(%nav);
  %container.add(%header);
  %header.placeBelow(%nav);

  for(%i = 0; %i < getLineCount(%listings); %i++) {
    %line = getLine(%listings, %i);
    %id = getField(%line, 0);
    %name = getASCIIString(getField(%line, 1));
    %author = getASCIIString(getField(%line, 2));
    %rating = getField(%line, 3);
    %downloads = getField(%line, 4);

    if(strLen(%name)) {
      %listing = GlassModManagerGui::createBoardListing(%id, %name, %author, %rating, %downloads, %odd = !%odd, %rtb);
      %listing.placeBelow(%container.getObject(%container.getCount()-1), 0);
      %container.add(%listing);
    }
  }

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;
  GlassModManagerGui_MainDisplay.setVisible(true);


  //%container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
}

function _glassPageNav(%board, %id) {
  return "<a:glass://board=" @ %board @ "&page=" @ %id @ ">" @ %id @ "</a>";
}

function GlassModManagerGui::createBoardNav(%bid, %page, %pages) {
  $Glass::MM_PreviousBoard = %bid;
  $Glass::MM_PreviousPage = %page;

  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "100 100 100 0";
    position = "10 10";
    extent = "485 25";
  };

  %back = "<a:glass://boards><< Back</a>";

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
    text = "<color:333333><font:verdana:16><just:left>" @ %back @ "<just:right>" @ %pageText;
    position = "0 0";
    extent = "485 45";
  };

  %swatch.add(%swatch.text);
  return %swatch;
}

function GlassModManagerGui::createBoardHeader(%title) {
  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "100 100 100 255";
    position = "10 10";
    extent = "485 25";
  };

  %swatch.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<color:ffffff><font:verdana bold:20><just:center>" @ %title;
    position = "0 0";
    extent = "225 45";
  };

  %swatch.add(%swatch.text);
  %swatch.text.setVisible(true);
  %swatch.text.setMarginResize(2, 0);
  return %swatch;
}

function GlassModManagerGui::createBoardListing(%id, %title, %author, %stars, %downloads, %odd, %rtb) {
  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = %odd ? "200 200 200 255" : "190 190 190 255";
    position = "10 10";
    extent = "485 40";
  };

  %swatch.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<color:333333><font:Verdana Bold:15>" @ %title @ "<br><font:verdana:12>Uploaded by <font:verdana bold:12>" @ %author;
    position = "10 7";
    extent = "325 45";
  };

  %dlStr = "";
  for(%i = strlen(%downloads); %i >= 0; %i--) {
    %dlStr = getsubstr(%downloads, %i, 1) @ %dlStr;
    if(mfloor((strlen(%downloads)-%i)/3) == (strlen(%downloads)-%i)/3) {
      %dlStr = "," @ %dlStr;
    }
  }
  %dlStr = getsubstr(%dlStr, 0, strlen(%dlstr)-1);
  if(strpos(%dlStr, ",") == 0) %dlStr = getsubstr(%dlStr, 1, strlen(%dlstr));

  %swatch.downloads = new GuiMLTextCtrl() {
    horizSizing = "left";
    vertSizing = "bottom";
    text = "<color:333333><font:verdana:16><just:right>" @ %dlstr;
    position = "375 7";
    extent = "100 45";
  };

  if(!%rtb) {
    %fullStars = mfloor(%stars);
    %fracStar = mfloor((%stars - %fullStars + 0.125)*4);
    %emptyStars = 4-%fullStars;
    %x = 300;
    for(%i = 0; %i < %fullStars; %i++) {
      %swatch.star[%i] = new GuiBitmapCtrl() {
        horizSizing = "left";
        vertSizing = "bottom";
        bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star.png";
        position = %x SPC "12";
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
        position = %x SPC "12";
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
        position = %x SPC "12";
        extent = "16 16";
        minextent = "0 0";
        clipToParent = true;
      };
      %swatch.add(%swatch.emptystar[%i]);
      %x += 20;
    }
  }

  %swatch.mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonButton) {
    rtb = %rtb;
    aid = %id;
    swatch = %swatch;
  };

  %swatch.add(%swatch.downloads);
  %swatch.add(%swatch.title);
  %swatch.add(%swatch.mouse);
  return %swatch;
}
