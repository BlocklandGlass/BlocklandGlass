function GlassTempered::init() {
  connectToURL("http://lakeys.github.io/tempered_glass/repository.txt", "GET", "", GlassTemperedTCP);
}

function GlassTempered::doIgnore(%liveVersion) {
  GlassSettings.update("Tempered::IgnoreVersion", %liveVersion);
}

function GlassTempered::checkVersion(%liveVersion, %updateMsg) {
  echo("Tempered Glass: Version check comparing live version " @ %liveVersion @ " to our local version, " @ Glass.temperedPatchVersion);

  %currentVersion = Glass.temperedPatchVersion;

  %ignoreVersion = GlassSettings.get("Tempered::IgnoreVersion");

  if(%liveVersion == %ignoreVersion) {
    return;
  }

  if(%liveVersion > %currentVersion && !$Pref::TemperedGlass::HideNotifications[%liveVersion]) {
    glassMessageBoxYesNo("Tempered Glass Update", "A new patch for Tempered Glass is available. You must install the update manually.<br><br>Message included with this update: " @ %updateMsg @ "<br><br>Select \"Yes\" to hide further notifications about this update.", "GlassTempered::doIgnore(" @ %liveVersion @ ");", "");
  }
}

function GlassTemperedTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassTemperedTCP::onDone(%this, %error) {
  %warningMsg = "Unable to establish a connection to verify your current version of Tempered Glass. You may need to update Blockland Glass.";
  if(%error) {
    warn("Tempered Glass - Failed to connect to server. Error: " @ %error);
    glassMessageBoxOk("Tempered Glass Warning", %warningMsg);
  }
  else {
    %jsonError = jettisonParse(%this.buffer);
    if(!%jsonError) {
      %object = $JSON::Value;

      %liveVersion = %object.get("currentVersion");
      %updateMsg = %object.get("updateMsg");
      GlassTempered::checkVersion(%liveVersion, %updateMsg);
    }
    else if(%jsonError) {
      warn("Tempered Glass - Failed to parse data from server. Error: " @ $JSON::Error);
      glassMessageBoxOk("Tempered Glass Warning", %warningMsg);
    }
  }
}

// Blockland Glass 2021 edition
