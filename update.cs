if(BLG.version !$= "1.0.1") {
  canvas.popDialog(GlassUpdatesGui);
  messageBoxOk("Error Repair", "Because your previous version of Blockland Glass had a bug, we went ahead and prematurely closed the update dialog. Don't worry, everything's fine. We fixed it. Please restart Blockland", "quit();");
}
