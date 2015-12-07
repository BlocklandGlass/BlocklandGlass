//================================================================
// Functions to create serializable GUI elements
//================================================================

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
  messageBoxOk("Load add-on", "This should load to addon id " @ %this.aid);
}

function GlassModManagerGui_AddonButton::onAdd(%this) {
  %this.extent = %this.swatch.extent;
  %this.position = "0 0";
}

exec("./modmanager/trending.cs");
