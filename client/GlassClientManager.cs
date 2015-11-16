function GlassClientManager::clean() {

}

function GlassClientManager::addRequiredMod(%modid) {

}

package GlassClientManager {
  function GameConnection::onConnectionDropped(%this, %msg) {
    parent::onConnectionDropped(%this, %msg);
    if(strpos(%msg, "Missing Blockland Glass Mods") == 0) {
      echo(" +- Glass Mods Missing!");
      %lines = strreplace(%msg, "<br>", "\n");
      for(%i = 2; %i < getLineCount(%lines); %i++) {
        %mod = getLine(%lines, %i);
        //populate a gui with the following
        //do something to close the message box?
        %mid = getWord(stripmlcontrolchars(%mod), 2);
        GlassClientManager::clean();
        GlassClientManager::addRequiredMod(%mid);
      }
    }
  }
};
activatePackage(GlassClientManager);
