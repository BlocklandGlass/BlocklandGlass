//goals:
//
// + install fonts
// + install support_updater
// + install system_preferences

function GlassInstallWizard::hasRun() {
  if(GlassSettings.cacheFetch("InstallerRun") !$= "") {
    return true;
  } else {
    return false;
  }
}

function GlassInstallWizard::run() {
  Glass.runInstallWizard = true;
  if(!isObject(GlassInstallWizard_window)) {
    %mm = MainMenuGui.getId();
    exec("Add-Ons/System_BlocklandGlass/client/gui/GlassInstallWizard.gui");
    %mm.delete();
  }

  exec("Add-Ons/System_BlocklandGlass/client/GlassFontManager.cs");
  GlassFontManager::init();

  GlassInstallWizard::populateStep(1);
}

function GlassInstallWizard_window::onWake(%this) {
  echo("installer woken");
}

function GlassInstallWizard::populateStep(%step) {
  if(isObject(%o = "GlassInstallWizard_step" @ %step-1)) { %o.setVisible(0); }
  %o = "GlassInstallWizard_step" @ %step;
  %o.setVisible(1);

  %title[1] = "Fonts";
  %title[2] = "Updater";
  %title[3] = "Preferences";

  GlassInstallWizard_window.setText("Step " @ %step @ "/3 (" @ %title[%step] @ ")");

  switch(%step) {
    case 1:
      GlassInstallWizard_step1_continue.setVisible(0);
      GlassInstallWizard_step1_progress.setValue(1);
      if(isReadOnly("base/client/ui/cache")) {
        GlassInstallWizard_step1_text.setText("It\'s been detected that your fonts folder is <color:ff0000>read-only. <color:000000>Because of this, we can't automatically install the required fonts for you.<br><br>We have two options:<br>1: Make the folder writeable<br><br>2: Manually Install Fonts");
      } else {
        GlassInstallWizard_step1_text.setText("Blockland Glass requires some custom fonts to run correctly. They're automatically installing now...");
        GlassInstallWizard_step1_progress.downloaded = GlassInstallWizard_step1_progress.fin = 0;
        GlassInstallWizard_step1_progress.length = GlassFontManager.fontsAvailable.length;
        GlassFontManager.downloadAll(1);
      }
  }
}

package GlassInstallWizard {
  function canavs::pushDialog(%this, %dlg) {
    warn("push dialog: " @ %dlg.getName());
    parent::pushDialog(%this, %dlg);
  }

  function GlassFontDownload::setProgressBar(%this, %float) {
    parent::setProgressBar(%this, %float);
    GlassInstallWizard_step1_progress.downloaded -= %this.prev;
    GlassInstallWizard_step1_progress.downloaded += %float;
    %this.prev = %float;

    %v = GlassInstallWizard_step1_progress.downloaded/GlassInstallWizard_step1_progress.length;
    echo("set: " @ %v);
    GlassInstallWizard_step1_progress.setValue(%v);
  }

  function GlassFontDownload::onDone(%this, %error) {
    parent::onDone(%this, %error);
    GlassInstallWizard_step1_progress.fin++;

    if(GlassInstallWizard_step1_progress.fin == GlassInstallWizard_step1_progress.length) {
      GlassInstallWizard_step1_continue.setVisible(1);
    }
  }
};
activatePackage(GlassInstallWizard);
