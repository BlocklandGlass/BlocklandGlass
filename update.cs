if(!$Server::Dedicated) {
  messageBoxYesNo("Restart", "It is highly recommended that you now restart Blockland.", "quit();");
} else {
  warn("You must now restart the server.");
  schedule(5000, 0, "quit");
}