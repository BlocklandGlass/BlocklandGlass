//================================================================
// Functions to create serializable GUI elements
//================================================================

//================================
// Common Functions
//================================

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
  %obj = GlassModManagerGui::fetchAndRenderAddon(%this.aid);
  %obj.action = "render";
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

  //if(isObject(%this.obj))
  //  GlassModManager::downloadAddon(%this.obj);
  //else
  echo(%this.aid);
    GlassModManager::downloadAddonFromId(%this.aid);
}

function GlassModManagerGui_AddonDownloadButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}

exec("./modmanager/trending.cs");
exec("./modmanager/errorPage.cs");
exec("./modmanager/addonPage.cs");
