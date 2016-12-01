function GlassModManagerGui::renderHome(%data) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };


  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;

  for(%i = 0; %i < %data.length; %i++) {
    %dlg = %data.value[%i];

    %body = GlassModManagerGui::createNewUploadsDialog(%dlg.uploads, %dlg.updates);

    %container.add(%body);

    %body.setVisible(true);

    %body.text.setVisible(true);
    %body.text.forceReflow();

    %body.verticalMatchChildren(0, 10);
  }

  %container.verticalMatchChildren(498, 0);
  
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.getGroup().scrollToTop();
  GlassModManagerGui_MainDisplay.setVisible(true);
}

function GlassModManagerGui::createNewUploadsDialog(%uploads, %updates) {
  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "485 10";
  };

  if(%uploads.length > 0) {
    if(%uploads.length > 5)
      %text = "<font:verdana bold:13>Hey there!<br><br><font:verdana:13>We've got a lot of new uploads for you:<br><br>";
    else
      %text = "<font:verdana bold:13>Hey there!<br><br><font:verdana:13>We've got some new uploads for you:<br><br>";

    for(%i = 0; %i < %uploads.length; %i++) {
      %u = %uploads.value[%i];
      %name = GetASCIIString(%u.name);
      %id = %u.id;
      %author = GetASCIIString(%u.author);
      %board = GetASCIIString(%u.board);

      %text = %text @ "<font:verdana bold:14>  +<font:verdana:13> <a:glass://aid-" @ %id @ ">" @ %name @ "</a> in <font:verdana bold:13>" @ %board @ "<br>";

    }
  } else {
    %text = "<font:verdana bold:13>Hey there!<br><br><font:verdana:13>There's no new uploads today.<br><br>";
  }

  if(%updates.length > 0) {
    if(%updates.length > 5)
      %text = %text @ "<br><font:verdana:13>There's been a lot of recent updates:<br><br>";
    else
      %text = %text @ "<br><font:verdana:13>There's been some recent updates:<br><br>";

    for(%i = 0; %i < %updates.length; %i++) {
      %u = %updates.value[%i];
      %name = %u.name;
      %id = %u.id;
      %version = %u.version;

      %text = %text @ "<font:verdana bold:14>  +<font:verdana:13> <a:glass://aid-" @ %id @ ">" @ %name @ "</a> to <font:verdana bold:13>v" @ %version @ "<br>";

    }
  } else {
    %text = %text @ "<br><font:verdana:13>There's been no recent updates.<br><br>";
  }

  %text = %text @ "<br><br><font:verdana bold:13>- <color:" @ GlassLive.color_bot @ ">GlassBot<font:verdana:13><color:000000><just:right>" @ getSubStr(getDateTime(), 9, 5);

  %textml = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassModManagerMLProfile";
    text = %text;
    position = "10 10";
    extent = "465 0";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%textml);
  %body.text = %textml;

  return %body;
}
