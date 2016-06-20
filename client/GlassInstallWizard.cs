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

  GlassInstallWizard_step1.setVisible(0);
  GlassInstallWizard_step2.setVisible(0);
  GlassInstallWizard_step3.setVisible(0);

  exec("Add-Ons/System_BlocklandGlass/client/GlassFontManager.cs");
  GlassFontManager::init();

  schedule(0, 0, eval, "GlassInstallWizard::populateStep(1);");
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
      if(!GlassFontManager::hasFonts()) {
        GlassInstallWizard_step1_continue.setVisible(0);
        GlassInstallWizard_step1_progress.setValue(0);
        if(isReadOnly("base/client/ui/cache")) {
          GlassInstallWizard_step1_text.setText("It\'s been detected that your fonts folder is <color:ff0000>read-only. <color:000000>Because of this, we can't automatically install the required fonts for you.<br><br>We have two options:<br>1: <a:" @ Glass.netaddress @ "/help/readonly.php>Make the folder writeable</a><br><br>2: <a:" @ Glass.netaddress @ "/help/fonts.php>Manually Install Fonts</a><br><br>After you've done either of those, restart Blockland.");
          GlassInstallWizard_step1_progress.setVisible(false);
          GlassInstallWizard_step1_continue.command = "quit();";
          GlassInstallWizard_step1_continue.setText("Quit");
        } else {
          GlassInstallWizard_step1_text.setText("Blockland Glass requires some custom fonts to run correctly. They're automatically installing now...");
          GlassInstallWizard_step1_progress.downloaded = GlassInstallWizard_step1_progress.fin = 0;
          GlassInstallWizard_step1_progress.length = GlassFontManager.fontsAvailable.length;
          GlassFontManager.downloadAll(1);
        }
      } else {
        GlassInstallWizard_step1_text.setText("We would be downloading the needed fonts for you, but it looks like you already have all the fonts that you'll need! Proceed to the next step.");
        GlassInstallWizard_step1_progress.setVisible(0);
        GlassInstallWizard_step1_continue.setVisible(1);
      }

    case 2:
      GlassSettings.cachePut("FontsRunOnce", 1);
      GlassInstallWizard_step2_progress.setValue(0);
      if(isFile("Add-Ons/Support_Updater.zip") || isFile("Add-Ons/Support_Updater/client.cs")) {
        GlassInstallWizard_step2_text.setText("We'd ask you to install Support_Updater, but it looks like you already have it! You can go ahead to the next step.");

        GlassInstallWizard_step2_continue.setVisible(1);
        GlassInstallWizard_step2_progress.setVisible(0);
      } else {
        GlassInstallWizard_step2_continue.setVisible(0);
        GlassInstallWizard_step2_progress.setVisible(1);

        GlassInstallWizard_step2_text.setText("Support_Updater is a great utility to keep your add-ons up to date, whether they're Glass add-ons or not. We'll go ahead and download that for you. When the download is finished, press continue.");
        GlassInstallWizard::installUpdater();
      }

    case 3:
      GlassInstallWizard_step3_text.setText("Now we're going to download Support_Preferences, an underlying preference system that will utilize both your RTB and oRBs preferences. After that, you're all set!");
      GlassInstallWizard_step3_finish.setVisible(0);
      GlassInstallWizard_step3_progress.setValue(0);
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
  %url = "http://cdn.blocklandglass.com/addons/Support_Preferences.zip";
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
    messageBoxOk("Download Error", "There was an error with your download. You'll need to restart the installer", "quit();");
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
  messageBoxOk("Reset", "Blockland will now shutdown. You'll need to start it back up again.", "quit();");
}

package GlassInstallWizard {
  function canvas::pushDialog(%this, %dlg) {
    if(%dlg.getName() !$= "messageBoxOkDlg" && %dlg.getName() !$= "consoleDlg" && Glass.runInstallWizard) {
      return;
    }

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
