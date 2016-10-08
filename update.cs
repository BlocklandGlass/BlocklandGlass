if(!$Server::Dedicated) {
  messageBoxOk("Restart", "You must now restart Blockland.", "quit();");
} else {
  warn("You must now restart the server.");
  schedule(5000, 0, "quit");
}