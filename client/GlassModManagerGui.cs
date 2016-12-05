//================================================================
// Functions to create serializable GUI elements
//================================================================

//================================
// Common Functions
//================================

function GlassModManagerGui::setPane(%pane) {
  error("Depreciated GlassModManagerGui::setPane");
}

function GlassModManagerGui::openPage(%this, %page, %arg1, %arg2, %arg3, %arg4) {
  if(isObject(%page)) {
    %this.oldPage = %this.page;

    %content = %page.open(%arg1, %arg2, %arg3, %arg4);

    %this.page = %page;
    %this.pageContent = %content;
  }
}

function GlassModManagerGui::pageDidLoad(%this, %page) {
  if(%this.page.getId() == %page.getId()) {
    if(isObject(%this.oldPage))
      %this.oldPage.close();
    GlassModManagerGui_MainDisplay.deleteAll();

    GlassModManagerGui_MainDisplay.add(%this.pageContent);
    GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 0);
    GlassModManagerGui_MainDisplay.setVisible(true);
    GlassModManagerGui_MainDisplay.getGroup().scrollToTop();
  }
}

function GlassModManagerGui::resizePage(%this) {
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 0);
  GlassModManagerGui_MainDisplay.setVisible(true);
}

function GlassModManagerGui::loadContext(%context) {
  //contexts are essentially just different starting points
  //for the dynamic guis
  //home, addons, build


  switch$(%context) {
    case "activity":
      GlassModManagerGui.openPage(GMM_ActivityPage);

    case "boards":
      GlassModManagerGui.openPage(GMM_BoardsPage);

    case "search":
      GlassModManagerGui.openPage(GMM_SearchPage);

    case "colorset":
      GlassModManagerGui.openPage(GMM_ColorsetsPage);

    case "myaddons":
      GlassModManagerGui.openPage(GMM_MyAddonsPage);
  }
}

function GlassModManagerGui::setLoading(%this, %bool) {
  %swatch = GlassModManagerGui_LoadingSwatch;
  %swatch.loading = %bool;

  if(%swatch.sch $= "") {
    %swatch.sch = %swatch.schedule(100, tick);
  }
}

function GlassModManagerGui_LoadingSwatch::tick(%this) {
  cancel(%this.sch);
  %this.sch = "";

  if(%this.loading) {
    if(%this.opacity < 128) {
      %this.opacity += 10;
      %this.visible = true;
    } else {
      return;
    }
  } else {
    if(%this.opacity > 0) {
      %this.opacity -= 20;
    } else {
      %this.visible = false;
      return;
    }
  }

  if(%this.opacity < 0) {
    %this.opacity = 0;
    %this.visible = false;
  } else if(%this.opacity > 128) {
    %this.opacity = 128;
    %this.visible = true;
  }

  %this.color = "200 200 200" SPC %this.opacity;

  %this.sch = %this.schedule(10, tick);
}

function GlassModManagerGui::animationTick(%this) {
  %obj = GlassModManagerGui_LoadingAnimation;
  cancel(%obj.schedule);

  %obj.frame++;
  if(%obj.frame > 22) {
    %obj.frame = 1;
  }
  GlassModManagerGui_LoadingAnimation.setBitmap("Add-Ons/System_BlocklandGlass/image/loading_animation/" @ %obj.frame @ ".png");
  %obj.schedule = %this.schedule(100, "animationTick");
}

function GlassModManagerGui::setProgress(%float, %text) {
  if(%float $= "" || isObject(%float)) {
    GlassModManagerGui_Window.extent = "675 550";
    GlassModManagerGui_ProgressBar.setVisible(false);
  } else {
    GlassModManagerGui_Window.extent = "675 585";
    GlassModManagerGui_ProgressBar.setVisible(true);

    GlassModManagerGui_ProgressBar.setValue(%float);
    GlassModManagerGui_ProgressBar.getObject(0).setText(%text);
  }
}

//================================
// Gui Classes
//================================

function GlassModManagerGui_AddonButton::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = vectoradd(%swatch.color, "20 20 20");
}

function GlassModManagerGui_AddonButton::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = %swatch.ocolor;
}

function GlassModManagerGui_AddonButton::onMouseDown(%this) {
  if(!%this.rtb) {
    %obj = GlassModManagerGui::fetchAndRenderAddon(%this.aid);
    %obj.action = "render";
  } else {
    GlassModManager::placeCall("rtbaddon", "id" TAB %this.aid);
  }
}

function GlassModManagerGui_AddonButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}



function GlassModManagerGui_AddonDownloadButton::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = "255 255 255";
}

function GlassModManagerGui_AddonDownloadButton::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = %swatch.ocolor;
}

function GlassModManagerGui_AddonDownloadButton::onMouseDown(%this, %a, %pos, %c, %d, %e) {
  GlassModManagerGui::doDownloadSprite(%pos, vectorAdd(GlassModManagerGui_ProgressBar.getCanvasPosition(), GlassModManagerGui_ProgressBar.getCenter()), 100);

  %this.swatch.info.setValue("<font:Verdana Bold:15><just:center>Queued..<br><font:verdana:14>" @ strcap(%this.branch));
  //something about redrawing the button?

  %tcp = GlassModManager::downloadAddon(%this.aid);
}

function GlassModManagerGui_AddonDownloadButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}



function GlassModManagerGui_ForumButton::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = vectoradd(%swatch.color, "20 20 20");
}

function GlassModManagerGui_ForumButton::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = %swatch.ocolor;
}

function GlassModManagerGui_ForumButton::onMouseDown(%this) {
  if(%this.type $= "topic") {
    GlassForumBrowser::getAddon(%this.topic);
  } else if(%this.type $= "board") {
    GlassForumBrowser::getBoard(%this.board);
  } else if(%this.type $= "external") {
    glassMessageBoxYesNo("Open Web Browser", "Pressing yes will open your web browser to the following page:<br><br>" @ %this.link, "gotoWebpage(\"" @ expandEscape(%this.link) @ "\");");
  }
}

function GlassModManagerGui_ForumButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}



function GlassModManagerGui_ScreenshotButton::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = "255 255 255";
}

function GlassModManagerGui_ScreenshotButton::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = %swatch.ocolor;
}

function GlassModManagerGui_ScreenshotButton::onMouseDown(%this, %a, %pos, %c, %d, %e) {
  GlassModManagerGui::downloadAndDisplayScreenshot(%this.aid, %this.screenshotId); // TODO check to see if it's downloaded, if now download; render
}

function GlassModManagerGui_AddonScreenshotButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}


function GlassModManagerGui_BoardButton::onMouseEnter(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color ="240 240 240 255";
}

function GlassModManagerGui_BoardButton::onMouseLeave(%this) {
  %swatch = %this.swatch;
  if(%swatch.ocolor $= "") %swatch.ocolor = %swatch.color;

  %swatch.color = %swatch.ocolor;
}

function GlassModManagerGui_BoardButton::onMouseDown(%this) {
  GlassModManagerGui.openPage(GMM_BoardPage, %this.bid);
}

function GlassModManagerGui_BoardButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}

exec("./submodules/modmanager/trending.cs");
exec("./submodules/modmanager/errorPage.cs");
exec("./submodules/modmanager/addonPage.cs");
exec("./submodules/modmanager/boardsPage.cs");
exec("./submodules/modmanager/boardPage.cs");
exec("./submodules/modmanager/colorsetsPage.cs");
exec("./submodules/modmanager/searchPage.cs");
exec("./submodules/modmanager/rtbImport.cs");
exec("./submodules/modmanager/myAddonsPage.cs");
exec("./gui/elements/GMM_Navigation.cs");
