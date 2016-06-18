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

    case 2:
      if(isFile("Add-Ons/Support_Updater.zip") || isFile("Add-Ons/Support_Updater/client.cs")) {
        GlassInstallWizard_step2_text.setText("We'd ask you to install Support_Updater, but it looks like you already have it! You can go ahead to the next step");
      } else {
        GlassInstallWizard_step2_text.setText("Support_Updater is a great utility to keep your add-ons up to date, whether they're Glass add-ons or not. We'll go ahead and download that for you. When the download is finished, press continue.");
        GlassInstallWizard::installUpdater();
      }

    case 3:
      //to avoid mismatched versions, we can just go ahead and force this one to reinstall
      //alternatively, do a SHA check?
      GlassInstallWizard::installPreferences();
  }
}

function GlassInstallWizard::installUpdater() {
  %url = "http://mods.greek2me.us/storage/Support_Updater.zip";
  %method = "GET";
  %downloadPath = "Add-Ons/Support_Updater.zip";
  %className = "GlassInstallWizardTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.context = "updater";
}

function GlassInstallWizard::installPreferences() {
  %url = "http://test.blocklandglass.com/addons/download.php?id=193&beta=0";
  %method = "GET";
  %downloadPath = "Add-Ons/Support_Preferences.zip";
  %className = "GlassInstallWizardTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.context = "prefs";
}

function GlassInstallWizardTCP::setProgressBar(%this, %float) {
  if(%this.context $= "updater") {
    GlassInstallWizard_step2_progress.setValue(%float);
  } else if(%this.context $= "prefs") {
    GlassInstallWizard_step3_progress.setValue(%float);
  }
}

function GlassInstallWizardTCP::onDone(%this, %error) {
  if(%error) {
    error("Need to handle this better - TCP error " @ %error);
    return;
  }

  if(%this.context $= "updater") {
    GlassInstallWizard_step2_progress.setValue(1);
    GlassInstallWizard_step2_continue.setVisible(true);
  } else if(%this.context $= "prefs") {
    GlassInstallWizard_step3_progress.setValue(1);
    GlassInstallWizard_step3_finish.setVisible(true);
  }
}

function GlassInstallWizard::finished() {
  GlassSettings.cachePut("InstallerRun", "1");
}

package GlassInstallWizard {
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
