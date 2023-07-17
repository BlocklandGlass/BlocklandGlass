// instructions for adding a setting
// - add pref to %settings variable in glasslive::init() below.
// - register setting in glasssettings::init() in common/glassettings.cs
// - if setting is to be changed by the user at will via the glass settings gui, add corresponding .drawsetting() for pref in glasslive::init() below.

function GlassLive::init() {
  if(!isObject(GlassLive)) {
    GlassGroup.add(new ScriptObject(GlassLive) {
      // color_blocked = "969696";
      color_default = "222222";
      color_self = "55acee";
      color_friend = "2ecc71";
      color_mod = "e67e22";
      color_admin = "e74c3c";
      color_bot = "9b59b6";
    });
  }

  GlassSettingsWindow.setVisible(false);
  GlassOverlayGui.add(GlassSettingsWindow);

  GlassManualWindow.setVisible(false);
  GlassOverlayGui.add(GlassManualWindow);

  GlassOverlay::setVignette();

  if(GlassSettings.get("Live::OverlayLogo") && !GlassLiveLogo.visible)
    GlassLiveLogo.setVisible(true);

  GlassOverlay::updateButtonAlignment();
}

//================================================================
//= System-level methods                                         =
//================================================================

function GlassLive::updateSetting(%category, %setting) {
  %box = "GlassSettingsGui_Prefs_" @ %setting;
  GlassSettings.update(%category @ "::" @ %setting, %box.getValue());
  %box.setValue(GlassSettings.get(%category @ "::" @ %setting));

  if(strlen(%callback = GlassSettings.obj[%setting].callback)) {
    if(isFunction(%callback)) {
      call(%callback);
    }
  }
}
///////////

//================================================================
//= Scroll?                                                      =
//================================================================

function GlassHighlightMouse::scrollLoop(%this, %text, %reset) {
  %icon = %text.getGroup().icon;
  %buttonChat = %text.getGroup().buttonChat;
  %unblock = %text.getGroup().unblock;

  if(%reset) {
    %this._scrollOrigin = %text.position;
    if(isObject(%icon))
      %this._scrollOrigin_Icon = %icon.position;
    %this._scrollOffset = 0;
    if(isObject(%unblock) || isObject(%buttonChat))
      %this._scrollRange = getWord(%text.extent, 0)-getWord(%this.extent, 0)+getWord(%text.position, 0)+25;
    else
      %this._scrollRange = getWord(%text.extent, 0)-getWord(%this.extent, 0)+getWord(%text.position, 0)+50;
  }

  %text.position = vectorSub(%this._scrollOrigin, %this._scrollOffset);
  if(isObject(%icon))
    %icon.position = vectorSub(%this._scrollOrigin_Icon, %this._scrollOffset);

  if(%this._scrollOffset >= %this._scrollRange) {
    %this._scrollOffset = 0;
    // %this.scrollTick = %this.schedule(2000, scrollLoop, %text);
  } else {
    %this._scrollOffset++;
    %this.scrollTick = %this.schedule(25, scrollLoop, %text);
  }
}

function GlassHighlightMouse::scrollEnd(%this, %text) {
  cancel(%this.scrollTick);
  %text.position = %this._scrollOrigin;

  %icon = %text.getGroup().icon;
  if(isObject(%icon))
    %icon.position = %this._scrollOrigin_Icon;

  %this.scrollTick = "";
}


//================================================================
//= Packages                                                     =
//================================================================

package GlassLivePackage {
  function Crouch(%bool) {
    if(GlassOverlayGui.isAwake())
      %bool = 0;

    return parent::Crouch(%bool);
  }
};
activatePackage(GlassLivePackage);
