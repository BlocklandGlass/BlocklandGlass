function Glass::openFeedbackPrompt() {
  if(GlassSettings.cacheFetch("Feedback") !$= "beta1" && !GlassDownloadGui.isAwake()) {
    messageBoxOk("Feedback", "<font:quicksand-bold:24>Welcome to Beta!<br><br><font:quicksand:16>Welcome to Glass Beta! Here's the cumulative changes since v1.0:<br><br> - Add-On dependencies now show up in the mod manager<br> - Server Control has been added<br> - Support_Preferences and Support_Updater now download automatically<br> - Servers can now require certain client mods<br><br>Thank you for taking your time to test!", "Glass::feedbackSeen(1);");
  }
}

function Glass::feedbackSeen(%iter) {
  GlassSettings.cachePut("Feedback", "beta1");
}
