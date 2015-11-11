function Glass::openFeedbackPrompt() {
  if(isfile("config/client/BLG/feedback.cs"))
    exec("config/client/BLG/feedback.cs");

  if($Glass::Feedback < 1) {
    messageBoxOk("Feedback", "<font:quicksand-bold:24>Alpha Feedback<br><br><font:quicksand:16>Hey there. You're on the development branch of Blockland Glass, and we've officially entered our Alpha phase.<br><br>Currently, we're focusing testing on the new preferences system. If you didn't know, there's a full-fledge RTB-pref replacement available that should already have ALL your old RTB prefs on it!<br><br>Please, report any bugs you find with the system over in the <a:http://forum.blockland.us/index.php?topic=282486.0>development topic</a>. Active testing will help make Glass better for everyone.<br><br>Thanks,<br>Jincux", "Glass::feedbackSeen(1);");
  }
}

function Glass::feedbackSeen(%iter) {
  $Glass::Feedback = %iter;
  export("$Glass::Feedback", "config/client/BLG/feedback.cs");
}
