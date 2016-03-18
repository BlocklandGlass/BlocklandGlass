function GlassModManagerGui::fetchBoard(%id) {
  //GlassModManager::placeCall("board", "id" TAB %id);
  GlassModManagerGui::renderBoardPage("Client Mods", %listings, 2, 14);
}

function GlassModManagerGui::renderBoardPage(%title, %listings, %page, %maxpage) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %header = GlassModManagerGui::createBoardHeader(%title);
  %listing = GlassModManagerGui::createBoardListing("Cool add-on", "Jincux", 3.9, 11);

  %container.add(%header);
  %container.add(%listing);
  %listing.position = vectorAdd(%header.position, "0 25");

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;
  GlassModManagerGui_MainDisplay.setVisible(true);


  //%container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
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
    text = "<color:ffffff><font:quicksand-bold:20><just:center>" @ %title;
    position = "0 0";
    extent = "225 45";
  };

  %swatch.add(%swatch.text);
  %swatch.text.setVisible(true);
  %swatch.text.setMarginResize(2, 0);
  return %swatch;
}

function GlassModManagerGui::createBoardListing(%title, %author, %stars, %downloads) {
  %swatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "200 200 200 255";
    position = "10 10";
    extent = "485 30";
  };

  %swatch.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<color:333333><font:quicksand-bold:16>" @ %title @ "<font:quicksand:14> by " @ %author;
    position = "10 7";
    extent = "225 45";
  };

  %fullStars = mfloor(%stars);
  %fracStar = mfloor((%stars - %fullStars + 0.125)*4);
  %emptyStars = 5-mceil(%stars);
  %x = 250;
  for(%i = 0; %i < %fullStars; %i++) {
    %swatch.star[%i] = new GuiBitmapCtrl() {
      horizSizing = "center";
      vertSizing = "center";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star.png";
      position = %x SPC "7";
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
      horizSizing = "center";
      vertSizing = "center";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star_frac_" @ %fracStar @ ".png";
      position = %x SPC "7";
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
      horizSizing = "center";
      vertSizing = "center";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/star_empty.png";
      position = %x SPC "7";
      extent = "16 16";
      minextent = "0 0";
      clipToParent = true;
    };
    %swatch.add(%swatch.emptystar[%i]);
    %x += 20;
  }

  %swatch.add(%swatch.title);
  return %swatch;
}
