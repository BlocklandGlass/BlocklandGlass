function GlassModManagerGui::fetchAndRenderAddon(%modId) {
  GlassModManager::placeCall("addon", "id" TAB %modId);
}

function GlassModManagerGui::renderAddon(%obj) {
  //obj:
  // authors
  // manager
  // name
  // description
  // tags
  // board
  // dependencies
  //
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %container.title = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand-bold:24><just:left>" @ %obj.name;
    position = "102 30";
    extent = "300 24";
    minextent = "0 0";
    autoResize = true;
  };

  %container.author = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand:16><just:left>by " @ %obj.author;
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %container.info = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand:16><just:left><bitmap:Add-Ons/System_BlocklandGlass/image/icon/accept_button.png>Random info<br>Multi-line shit<br>Wow cool";
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %container.description = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand:16><just:left>" @ %obj.description;
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %num = getRandom(1, 3);
  %branch[0] = "stable";
  %branch[1] = "unstable";
  %branch[2] = "development";
  %branchColor["stable"] = "128 255 128 255";
  %branchColor["unstable"] = "255 255 128 255";
  %branchColor["development"] = "255 128 128 255";


  %xExtent = mfloor((505-70)/3);
  %xMargin = 10;
  %totalWidth = (%xExtent*%num) + (%xMargin*(%num-1));

  for(%i = 0; %i < %num; %i++) {
    %x = ((505-%totalWidth)/2) + (%xExtent*(%i)) + (%xMargin*(%i));

    %branch = %branch[%i];
    %container.download[%branch] = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = %branchColor[%branch];
      position = %x SPC 0;
      extent = %xExtent SPC 35;
    };

    %container.download[%branch].info = new GuiMLTextCtrl() {
      horizSizing = "center";
      vertSizing = "center";
      text = "<font:quicksand-bold:16><just:center>Download<br><font:quicksand:14>" @ strcap(%branch);
      position = "0 0";
      extent = "300 16";
      minextent = "0 0";
      autoResize = true;
    };

    %container.download[%branch].mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonDownloadButton) {
      aid = %aid;
      swatch = %container.download[%branch];
    };

    %container.download[%branch].add(%container.download[%branch].info);
    %container.download[%branch].add(%container.download[%branch].mouse);
    %container.download[%branch].info.setMarginResize(2, 2);
    %container.download[%branch].info.forceCenter();
    %container.add(%container.download[%branch]);
  }


  %container.add(%container.title);
  %container.add(%container.author);
  %container.add(%container.info);
  %container.add(%container.description);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);

  %container.info.setVisible(true);
  %container.info.forceReflow();

  %container.setMarginResize(0, 0);

  %container.title.setMargin(20, 20);
  %container.title.setMarginResize(20);
  %container.author.setMarginResize(20);
  %container.author.placeBelow(%container.title, 1);
  %container.info.setMarginResize(20);
  %container.info.placeBelow(%container.author, 15);
  %container.description.setMarginResize(20);
  %container.description.placeBelow(%container.info, 25);
  for(%i = 0; %i < %num; %i++) {
    %container.download[%branch[%i]].placeBelow(%container.description, 25);
  }

}
