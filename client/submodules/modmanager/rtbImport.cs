function GlassModManagerGui::openRTBImport(%addons) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "495 64";
  };

  for(%i = 0; %i < %addons.length; %i++) {
    %import = %addons.value[%i];

    %swatch = new GuiSwatchCtrl() {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "465 62";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "230 230 230 255";
      import = %import;
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 9";
      extent = "356 16";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      maxBitmapHeight = "-1";
      selectable = "1";
      autoResize = "1";
      text = "<font:verdana bold:14><bitmap:Add-Ons/System_BlocklandGlass/image/icon/bricks> <color:dd0000>" @ %import.filename @ ".zip<color:666666> -> <color:00cc66>" @ %import.glass_name;
    };

    %swatch.progress = new GuiProgressCtrl() {
      profile = "GuiProgressProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 29";
      extent = "444 23";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
    };

    %swatch.progresstext = new GuiMLTextCtrl() {
       profile = "GuiMLTextProfile";
       horizSizing = "center";
       vertSizing = "center";
       position = "44 5";
       extent = "356 14";
       minExtent = "8 2";
       enabled = "1";
       visible = "1";
       clipToParent = "1";
       lineSpacing = "2";
       allowColorChars = "0";
       maxChars = "-1";
       maxBitmapHeight = "-1";
       selectable = "1";
       autoResize = "1";
       text = "<shadow:1:1><just:center><font:verdana:12><color:999999>Waiting...";
    };
    %swatch.add(%swatch.text);
    %swatch.add(%swatch.progress);
    %swatch.progress.add(%swatch.progresstext);

    %container.add(%swatch);
    if(%last !$= "")
      %swatch.placeBelow(%last, 10);

    %last = %swatch;
  }

  glassMessageBoxOk("RTB Reclamation", "<font:verdana:12>Some of your old RTB add-ons have updates available through Glass! We'll go ahead and fetch those for you!", "GlassModManager::doRTBImport();");

  %container.verticalMatchChildren(0, 10);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
  GlassModManagerGui_MainDisplay.extent = %container.extent;
  GlassModManagerGui_MainDisplay.setVisible(true);

  //%container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
}

function GlassModManager::doRTBImport() {
  %container = GlassModManagerGui_MainDisplay.getObject(0);
  for(%i = 0; %i < %container.getCount(); %i++) {
    %swatch = %container.getObject(%i);
    %ret = GlassModManager::downloadAddonFromId(%swatch.import.glass_id);
    %ret.rtbImportProgress = %swatch.progress;
  }
}
