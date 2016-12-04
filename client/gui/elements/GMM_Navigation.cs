function GMM_Navigation::init() {
  new ScriptObject(GMM_Navigation);
}

function GMM_Navigation::clear(%this) {
  %this.steps = 0;
}

function GMM_Navigation::addStep(%this, %text, %callback) {
  %this.step[%this.steps+0] = %text;
  %this.stepCall[%this.steps+0] = %callback;

  %this.steps++;
}

function GMM_Navigation::selectStep(%this, %i) {
  if(%i == %this.steps-1)
    return;

  %this.steps = %i;
  if(%this.stepCall[%i] !$= "")
    eval(%this.stepCall[%i]);
}

function GMM_Navigation::createSwatch(%this) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 36";

    new GuiBitmapCtrl() {
      profile = "GuiDefaultProfile";
      position = "10 10";
      extent = "16 16";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/glassLogo";
    };
  };

  %x = 36;

  for(%i = 0; %i < %this.steps; %i++) {
    %btn = new GuiBitmapButtonCtrl() {
      profile = "GlassBlockButtonProfile";
      position = %x SPC 8;
      extent = 20+(strlen(%this.step[%i])*8) SPC "20";
      bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

      text = %this.step[%i];

      command = "GMM_Navigation.selectStep(" @ %i @ ");";
    };

    %x += 20+(strlen(%this.step[%i])*8) + 10;

    %container.add(%btn);
  }

  return %container;
}
