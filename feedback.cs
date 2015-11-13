function Glass::openFeedbackPrompt() {
  if(GlassSettings.cacheFetch("Feedback") !$= "alpha3") {
    messageBoxOk("Feedback", "<font:quicksand-bold:24>Alpha 3<br><br><font:quicksand:16>Welcome to Glass Alpha 3!<br><br>New in this build is a whole new (backend) setting and cache system. This has led to some changes to the mod manager, specifically the keybind and colorset system.<br><br>Please focus your testing on these, and let us know if you find anything. Also, if your previous colorset and keybind settings haven't transfered over, please notify us of that.<br><br>Thanks,<br>The Glass Team", "Glass::feedbackSeen(1);");
  }
}

function Glass::feedbackSeen(%iter) {
  GlassSettings.cachePut("Feedback", "alpha3");
}
