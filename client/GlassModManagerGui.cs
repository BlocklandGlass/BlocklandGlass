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


//
// possible name change or relocation
//

function GlassModManagerGui::loadContext(%this, %context) {
  //contexts are essentially just different starting points
  //for the dynamic guis
  //home, addons, build

  %old = %this.contextTab;
  if(isObject(%old)) {
    %old.mColor = %old.oColor;
  }

  %obj = "GlassModManagerGui_Tab_" @ %context;

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

  %obj.oColor = %obj.mColor;
  %obj.mColor = "131 195 243 180";
  %this.contextTab = %obj;
}

function GlassModManagerGui::setLoading(%this, %bool) {
  if(%this.getId() != GlassModManagerGui.getId()) {
    error("Depreciated GlassModManagerGui::setLoading usage");
    return;
  }

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
  error("Depreciated ::setProgress");
}

exec("./submodules/modmanager/errorPage.cs");

exec("./submodules/modmanager/activityPage.cs");
exec("./submodules/modmanager/searchPage.cs");
exec("./submodules/modmanager/boardsPage.cs");
exec("./submodules/modmanager/boardPage.cs");
exec("./submodules/modmanager/addonPage.cs");

exec("./submodules/modmanager/colorsetsPage.cs");
exec("./submodules/modmanager/myAddonsPage.cs");

exec("./submodules/modmanager/rtbImport.cs");
exec("./submodules/modmanager/rtbAddonPage.cs");
exec("./submodules/modmanager/rtbBoardsPage.cs");
exec("./submodules/modmanager/rtbBoardPage.cs");

exec("./gui/elements/GMM_Navigation.cs");
