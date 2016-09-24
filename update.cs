if(!$Server::Dedicated) {
  messageBoxOk("Restart", "You must now restart Blockland.", "quit();");
} else {
  warn("You must now restart Blockland.");
  schedule(5000, 0, "quit");
}