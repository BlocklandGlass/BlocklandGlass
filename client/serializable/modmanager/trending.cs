function GlassModManagerGui::renderHome(%trending, %recent) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 0";
  };

  echo(%container);

  %trendSwat = GlassModManagerGui::renderHome_trending(trim(%trending));
  %recentSwat = GlassModManagerGui::renderHome_recent(trim(%recent));
  %container.add(%trendSwat);
  %container.add(%recentSwat);

  %container.extent = getWord(%container.extent, 0) SPC getWord(%trendSwat.extent, 1);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
}

function GlassModManagerGui::renderHome_trending(%trending) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "10 10";
    extent = "235 0";
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:quicksand-bold:20><just:center>Trending Add-Ons";
    position = "0 0";
    extent = "225 45";
  };
  %container.add(%container.text);

  %y = 25;
  for(%i = 0; %i < getLineCount(%trending); %i++) {
    %info = getLine(%trending, %i);
    %position = getField(%info, 0);
    %name = getField(%info, 1);
    %author = getField(%info, 2);
    %dls = getField(%info, 3);
    %aid = getField(%info, 4);

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
      text = "<font:quicksand:14>" @ %position @ ": <font:quicksand-bold:16>" @ %name @ "<just:right><font:quicksand:16>" @ %dls @ "<br><just:left><font:quicksand:14>by " @ %author;
      position = "7 7";
      extent = "225 45";
    };

    %swatch.mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonButton) {
      aid = %aid;
      swatch = %swatch;
    };

    %swatch.add(%swatch.text);
    %swatch.add(%swatch.mouse);
    %container.add(%swatch);
    echo("after");
    %y += 46;
  }

  %container.extent = "235" SPC %y+25;

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

    %swatch.mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonButton) {
      aid = %aid;
      swatch = %swatch;
    };

    %swatch.add(%swatch.text);
    %swatch.add(%swatch.mouse);
    %container.add(%swatch);
    echo("after");
    %y += 46;
  }

  %container.extent = "235" SPC %y+25;

  return %container;
}

GlassModManagerGui::renderHome("1\tBlockland Glass\tJincux and Nexus\t738\t11\n2\tSlayer\tGreek2Me\t426\t9\n", "Blockland Glass\tJincux\tMonday\t11\nAdmin Chat\tJincux\tSunday\t7");
