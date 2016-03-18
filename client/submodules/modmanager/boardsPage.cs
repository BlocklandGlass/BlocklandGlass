function GlassModManagerGui::renderBoards(%boards) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %xMargin = 10;
  %width = mfloor((getWord(%container.extent, 0)-(3*%xMargin))/2);

  %ypos = 10;

  for(%i = 0; %i < getLineCount(%boards); %i++) {
    %board = getLine(%boards, %i);
    %name = getField(%board, 0);
    %id = getField(%board, 1);
    %desc = getField(%desc, 2);
    %img = getField(%desc, 3);

    %contain = GlassModManagerGui::createBoardButton(%name, %desc, %img, %id);
    %contain.extent = %width SPC mfloor(%width/2);
    %contain.position = 10+(mFloor(%i/2) != %i/2 ? %width+10 : 0) SPC %yPos;
    %contain.text.forceCenter();
    %contain.text.position = getWord(%contain.text.position, 0) SPC 10;

    if(mFloor(%i/2) != %i/2) { //even
      %yPos += mfloor(%width/2)+10;
    }

    %container.add(%contain);
    %contain.mouse.onAdd();
  }

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;
  GlassModManagerGui_MainDisplay.setVisible(true);


  //%container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
}

function GlassModManagerGui::createBoardButton(%name, %desc, %img, %id) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "200 200 200 255";
    position = "10 10";
    extent = "235 0";
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:quicksand-bold:20><just:center>" @ %name;
    position = "0 0";
    extent = "225 45";
  };

  %container.mouse = new GuiMouseEventCtrl(GlassModManagerGui_BoardButton) {
    bid = %id;
    swatch = %container;
  };

  %container.add(%container.text);
  %container.add(%container.mouse);

  return %container;
}

function GlassModManagerGui::renderHome_recent(%recent) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "255 10";
    extent = "235 0";
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:quicksand-bold:20><just:center>New Add-Ons";
    position = "0 0";
    extent = "225 45";
  };
  %container.add(%container.text);

  %y = 25;
  for(%i = 0; %i < getLineCount(%recent); %i++) {
    %info = getLine(%recent, %i);
    %name = getField(%info, 0);
    %author = getField(%info, 1);
    %date = getField(%info, 2);
    %aid = getField(%info, 3);

    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "200 200 200 255";
      position = 0 SPC (0+%y);
      extent = "235 45";
    };

    %swatch.text = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<font:quicksand-bold:16>" @ %name @ "<just:right><font:quicksand:14>" @ %date @ "<br><just:left><font:quicksand:14>by " @ %author;
      position = "7 7";
      extent = "225 45";
    };



    %swatch.add(%swatch.text);
    %swatch.add(%swatch.mouse);
    %container.add(%swatch);
    %y += 46;
  }

  %container.extent = "235" SPC %y+25;

  return %container;
}
