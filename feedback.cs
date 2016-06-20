function Glass::openFeedbackPrompt() {
  if(GlassSettings.cacheFetch("Feedback") !$= "1.1.0" && !GlassDownloadGui.isAwake()) {
    //messageBoxOk("Welcome", "<font:verdana bold:24>Welcome to Glass 1.1!<br><br><font:verdana:16>Welcome to Glass 1.1! You'll be happy to find a new <font:verdana bold:16>server control<font:verdana:16> system, which automatically imports all your old RTB preferences!", "Glass::feedbackSeen(1);");
  }
}

function Glass::feedbackSeen(%iter) {
  GlassSettings.cachePut("Feedback", "1.1.0");
}
