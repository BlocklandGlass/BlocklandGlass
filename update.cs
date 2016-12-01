if(!$Server::Dedicated) {
  messageBoxYesNo("Restart", "It is highly recommended that you now restart Blockland.", "quit();");
} else {
  warn("It is highly recommended that you now restart the server.");
}