function GlassServerList::addServer(%this, %obj) {
  if(isObject(%obj.glassTcp)) {
    return;
  }

  %obj.glassTcp = new TCPObject(GlassServerTCP);
  %obj.glassTcp.serverSO = %obj;
  GlassServerTCP.connect(%obj.ip);
}

function GlassServerTCP::onConnected(%this) {
  %this.send("listen\t1");
}

function GlassServerTCP::onLine(%this, %line) {
  %cmd = getField(%line, 0);
  switch$(%cmd) {
    case "updateField":
      %key = getField(%line, 1);
      %val = collapseEscape(getField(%line, 2));

      if(%key $= "playercount") {
        %this.serverSO.currPlayers = %val;
        %this.serverSO.display();
      }
  }
}

package GlassServerList {
  function ServerSO::display(%this) {
    parent::display(%this);
    GlassServerList.addServer(%this);
  }
};
activatePackage(GlassServerList);
