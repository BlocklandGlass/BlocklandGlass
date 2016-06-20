if(!isObject(GlassScrollProfile)) new GuiControlProfile(GlassScrollProfile)
{
   fillColor = "240 240 240 255";
   borderColor = "150 150 150 255";
   border = "1";
   opaque = "1";

   fontColor = "70 70 70 255";
   fontColors[0] = "70 70 70";
   fontColors[1] = "255 70 70"; //R
   fontColors[2] = "70 255 70"; //G
   fontColors[3] = "70 70 255"; //B

   hasBitmapArray = true;
   bitmap = "Add-Ons/System_BlocklandGlass/image/gui/scroll.png";
};

if(!isObject(GlassWindowProfile)) new GuiControlProfile(GlassWindowProfile) {
  tab = "0";
  canKeyFocus = "0";
  mouseOverSelected = "0";
  modal = "1";
  opaque = "1";
  fillColor = "255 255 255 255";
  fillColorHL = "200 200 200 255";
  fillColorNA = "200 200 200 255";
  border = "2";
  borderThickness = "7";
  borderColor = "0 0 0 255";
  borderColorHL = "128 128 128 255";
  borderColorNA = "64 64 64 255";
  fontType = "verdana bold";
  fontSize = "18";
  fontColors[0] = "0 0 0 255";
  fontColors[1] = "255 255 255 255";
  fontColors[2] = "0 0 0 255";
  fontColors[3] = "200 200 200 255";
  fontColors[4] = "0 0 204 255";
  fontColors[5] = "85 26 139 255";
  fontColors[6] = "0 0 0 0";
  fontColors[7] = "0 0 0 0";
  fontColors[8] = "0 0 0 0";
  fontColors[9] = "0 0 0 0";
  fontColor = "50 50 50 255";
  fontColorHL = "255 255 255 255";
  fontColorNA = "0 0 0 255";
  fontColorSEL = "200 200 200 255";
  fontColorLink = "0 0 204 255";
  fontColorLinkHL = "85 26 139 255";
  doFontOutline = "0";
  fontOutlineColor = "255 255 255 255";
  justify = "left";
  textOffset = "10 7";
  autoSizeWidth = "0";
  autoSizeHeight = "0";
  returnTab = "0";
  numbersOnly = "0";
  cursorColor = "0 0 0 255";
  bitmap = "Add-Ons/System_BlocklandGlass/client/gui/glassWindow";
  text = "GuiWindowCtrl test";
  hasBitmapArray = "1";
};


if(!isObject(GlassSideTabProfile)) new GuiControlProfile(GlassSideTabProfile) {
  tab = "0";
  canKeyFocus = "0";
  mouseOverSelected = "0";
  modal = "1";
  opaque = "1";
  fillColor = "149 152 166 255";
  fillColorHL = "171 171 171 255";
  fillColorNA = "221 202 173 255";
  border = "1";
  borderThickness = "1";
  borderColor = "0 0 0 255";
  borderColorHL = "128 128 128 255";
  borderColorNA = "64 64 64 255";
  fontType = "verdana bold";
  fontSize = "18";
  fontColors[0] = "0 0 0 255";
  fontColors[1] = "255 255 255 255";
  fontColors[2] = "0 0 0 255";
  fontColors[3] = "200 200 200 255";
  fontColors[4] = "0 0 204 255";
  fontColors[5] = "85 26 139 255";
  fontColors[6] = "0 0 0 0";
  fontColors[7] = "0 0 0 0";
  fontColors[8] = "0 0 0 0";
  fontColors[9] = "0 0 0 0";
  fontColor = "0 0 0 255";
  fontColorHL = "255 255 255 255";
  fontColorNA = "0 0 0 255";
  fontColorSEL = "200 200 200 255";
  fontColorLink = "0 0 204 255";
  fontColorLinkHL = "85 26 139 255";
  doFontOutline = "0";
  fontOutlineColor = "255 255 255 255";
  justify = "center";
  textOffset = "20 6";
  autoSizeWidth = "0";
  autoSizeHeight = "0";
  returnTab = "0";
  numbersOnly = "0";
  cursorColor = "0 0 0 255";
  bitmap = "base/client/ui/blockScroll.png";
  hasBitmapArray = "1";
};

if(!isObject(GlassModManagerMLProfile)) new GuiControlProfile(GlassModManagerMLProfile : GuiDefaultProfile) {
  fontType = "verdana bold";
  fontSize = "18";
  fontColors[0] = "0 0 0 255";
  fontColors[1] = "255 255 255 255";
  fontColors[2] = "0 0 0 255";
  fontColors[3] = "200 200 200 255";
  fontColors[4] = "0 0 204 255";
  fontColors[5] = "85 26 139 255";
  fontColors[6] = "0 0 0 0";
  fontColors[7] = "0 0 0 0";
  fontColors[8] = "0 0 0 0";
  fontColors[9] = "0 0 0 0";
  fontColor = "0 0 0 255";
  fontColorHL = "255 255 255 255";
  fontColorNA = "0 0 0 255";
  fontColorSEL = "200 200 200 255";
  fontColorLink = "100 100 100 255";
  fontColorLinkHL = "200 200 200 255";
};
