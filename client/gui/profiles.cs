if(!isObject(GlassClick1Audio)) new AudioProfile(GlassClick1Audio)
{
  filename = "Add-Ons/System_BlocklandGlass/sound/click.wav";
  isLooping = false;
  description = "AudioGui";
  preload = true;
};

if(!isObject(GlassPop1Audio)) new AudioProfile(GlassPop1Audio)
{
  filename = "Add-Ons/System_BlocklandGlass/sound/pop.wav";
  isLooping = false;
  description = "AudioGui";
  preload = true;
};

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

if(!isObject(GlassTextEditProfile)) new GuiControlProfile(GlassTextEditProfile : GuiTextEditProfile)
{
  fontType = "Verdana";
  fillColor = "240 240 240 255";
  borderColor = "150 150 150 255";
  border = "1";
  opaque = "1";
  fontSize = 12;

  fontColor = "70 70 70 255";
  fontColors[0] = "70 70 70";
  fontColors[1] = "255 70 70"; //R
  fontColors[2] = "70 255 70"; //G
  fontColors[3] = "70 70 255"; //B
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
  textOffset = "10 6";
  autoSizeWidth = "0";
  autoSizeHeight = "0";
  returnTab = "0";
  numbersOnly = "0";
  cursorColor = "0 0 0 255";
  bitmap = "Add-Ons/System_BlocklandGlass/image/gui/glassWindow";
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
  fontType = "Verdana Bold";
  fontSize = "15";
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
  fontType = "Verdana Bold";
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

if(!isObject(GlassMLTextEditProfile)) new GuiControlProfile(GlassMLTextEditProfile : GuiMLTextEditProfile) {
  fontType = "Verdana";
  fontSize = "12";
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

if(!isObject(GlassSearchBarProfile)) new GuiControlProfile(GlassSearchBarProfile : GuiTextEditProfile) {
  fontType = "Verdana";
  fontSize = "25";
  fontColors[0] = "64 64 64 255";
  fontColors[1] = "128 128 128 255";

  fillColor = "240 240 240 255";
  borderColor = "128 128 128 255";
};

if(!isObject(GlassSearchResultProfile)) new GuiControlProfile(GlassSearchResultProfile : GuiTextProfile) {
  fontType = "Verdana Bold";
  fontSize = "15";
};

if(!isObject(GlassFriendTextProfile)) new GuiControlProfile(GlassFriendTextProfile : GuiTextProfile) {
  fontType = "Verdana Bold";
  fontSize = "13";
  fontColors[0] = "0 0 0 255"; // black/user
  fontColors[1] = "85 172 238 255"; // blue/self
  fontColors[2] = "46 204 113 255"; // green/friend
  fontColors[3] = "230 126 34 255"; // orange/mod
  fontColors[4] = "231 76 60 255"; // red/admin
  fontColors[5] = "155 89 182 255"; // purple/bot
  fontColors[6] = "150 150 150 255"; // gray/blocked

  fontOutlineColor = "150 150 150 255";
  //doFontOutline = true;

  border = true;
  borderColor = "0 0 0 255";
  borderThickness = 5;
};

if(!isObject(GuiTextVerdanaProfile)) new GuiControlProfile(GuiTextVerdanaProfile : GuiTextProfile) {
  fontType = "Verdana";
  fontSize = 12;
};

if(!isObject(GlassCheckBoxProfile)) new GuiControlProfile(GlassCheckBoxProfile : GuiCheckBoxProfile) {
  fontType = "Verdana";
  fontSize = 12;
  fillColor = "200 200 200 255";
  borderColor = "100 100 100 255";
};

if(!isObject(GlassBlockButtonProfile)) new GuiControlProfile(GlassBlockButtonProfile : BlockButtonProfile) {
  fontType = "Verdana Bold";
  fontSize = 15;
  fontColor = "64 64 64 255";
  soundButtonDown = "GlassPop1Audio";
};

if(!isObject(GlassBlockButtonWhiteProfile)) new GuiControlProfile(GlassBlockButtonWhiteProfile : BlockButtonProfile) {
  fontType = "Verdana Bold";
  fontSize = 15;
  fontColor = "250 250 250 255";
  soundButtonDown = "GlassClick1Audio";
};

if(!isObject(GlassRoundedButtonProfile)) new GuiControlProfile(GlassRoundedButtonProfile : BlockButtonProfile) {
  fontType = "Verdana Bold";
  fontSize = 16;
  fontColor = "220 220 220 255";
  soundButtonDown = "GlassClick1Audio";
};

if(!isObject(GlassUserListButtonProfile)) new GuiControlProfile(GlassUserListButtonProfile : BlockButtonProfile) {
  fontType = "Verdana Bold";
  fontSize = 13;
  fontColor = "220 220 220 255";
  justify = "left";
  fontColors[0] = "0 0 0 255";
  fontColors[1] = "255 255 255 255";
  fontColors[2] = "100 100 100 255";
};

if(!isObject(GlassChatroomTabProfile)) new GuiControlProfile(GlassChatroomTabProfile : GuiCenterTextProfile) {
  fontType = "Verdana Bold";
  fontSize = 15;
  fontColor = "64 64 64 255";
  fontColors[0] = "64 64 64 255";
  fontColors[1] = "200 64 64 255";
};

if(!isObject(GlassProgressProfile)) new GuiControlProfile(GlassProgressProfile : GuiProgressProfile) {
  borderColor = "100 100 100 255";
  fillColor = "46 204 113 200";
  borderThickness = 0.5;
};

package GlassTheme {
  function GuiWindowCtrl::onAdd(%this, %a) {
    if(%this.profile $= "GlassWindowProfile") {
      if(GlassSettings.get("Glass::UseDefaultWindows")) {
        %this.setProfile(BlockWindowProfile);
      }

      %windows = trim(Glass.windows TAB %this.getId());
      Glass.windows = "";
      for(%i = 0; %i < getFieldCount(%windows); %i++) {
        if(isObject(getField(%windows, %i))) {
          Glass.windows = Glass.windows TAB getField(%windows, %i);
        }
      }
    }

    if(isFunction(%this, onAdd))
      return parent::onAdd(%this, %a);
  }
};
activatePackage(GlassTheme);
