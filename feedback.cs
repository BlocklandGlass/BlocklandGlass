function Glass::openFeedbackPrompt() {
  if(GlassSettings.cacheFetch("Feedback") !$= "1.1.0" && !GlassDownloadGui.isAwake()) {
    //messageBoxOk("Welcome", "<font:quicksand-bold:24>Welcome to Glass 1.1!<br><br><font:quicksand:16>Welcome to Glass 1.1! You'll be happy to find a new <font:quicksand-bold:16>server control<font:quicksand:16> system, which automatically imports all your old RTB preferences!", "Glass::feedbackSeen(1);");
  }
}

function Glass::feedbackSeen(%iter) {
  GlassSettings.cachePut("Feedback", "1.1.0");
}
