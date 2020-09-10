function GMM_ActivityPage::init() {
  GlassGroup.add(new ScriptObject(GMM_ActivityPage) {
    class = "GlassModManagerPage";
  });
}

function GMM_ActivityPage::open(%this) {
  GMM_Navigation.clear();
  GMM_Navigation.addStep("Activity", "GlassModManagerGui.openPage(GMM_ActivityPage);");

  if(isObject(%this.container)) {
    %this.container.deleteAll();
    %this.container.delete();
  }

  %this.container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "635 498";
  };

  GlassModManagerGui.setLoading(true);

  GlassModManager::placeCall("home", "", "GMM_ActivityPage.handleData");

  return %this.container;
}

function GMM_ActivityPage::close() {

}

function GMM_ActivityPage::handleData(%this, %res) {
  GlassModManagerGui.pageDidLoad(%this);
  GlassModManagerGui.setLoading(false);

  %container = GMM_ActivityPage.container;

  %data = %res.data;

  for(%i = 0; %i < %data.length; %i++) {
    %dlg = %data.value[%i];

    switch$(%dlg.type) {
      case "recent":
        %body = GMM_ActivityPage.createNewUploadsDialog(%dlg.uploads, %dlg.updates);

      case "message":
        %body = GMM_ActivityPage.createMessageDialog(%dlg.message);
    }

    %container.add(%body);

    %body.setVisible(true);

    %body.text.setVisible(true);
    %body.text.forceReflow();

    %body.verticalMatchChildren(0, 10);

    if(%last)
      %body.placeBelow(%last, 10);

    %last = %body;
  }

  %container.verticalMatchChildren(498, 0);

  GlassModManagerGui.resizePage();
}

function GMM_ActivityPage::createMessageDialog(%this, %message) {
  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 10";
  };

  %body.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassModManagerMLProfile";
    text = "<font:verdana:13>" @ getLongASCIIString(%message);
    position = "10 10";
    extent = "595 0";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.text);

  return %body;
}

function GMM_ActivityPage::createNewUploadsDialog(%this, %uploads, %updates) {
  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 10";
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

  %text = %text @ "<br><br><font:verdana bold:13>- <color:" @ GlassLive.color_bot @ ">GlassBot";

  %textml = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassModManagerMLProfile";
    text = %text;
    position = "10 10";
    extent = "595 0";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%textml);
  %body.text = %textml;

  return %body;
}
