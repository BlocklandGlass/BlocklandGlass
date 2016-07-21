function GlassServerList::addServer(%this, %obj) {
  if(isObject(%obj.glassTcp)) {
    return;
  }

  %obj.glassTcp = new TCPObject(GlassServerTCP);
  %obj.glassTcp.serverSO = %obj;
  GlassServerTCP.connect(%obj.ip);
}

function GlassServerTCP::onConnected(%this) {
  echo("connected");
  %this.connected = true;
  $Glass::st[$Glass::stc+0] = %this;
  $Glass::stc++;
  %this.send("listen\t1\r\n");
}

function GlassServerTCP::onLine(%this, %line) {
  echo("c > " @ %line);
  %cmd = getField(%line, 0);
  switch$(%cmd) {
    case "updateField":
      %key = getField(%line, 1);
      %val = collapseEscape(getField(%line, 2));

      if(%key $= "playercount") {
        %this.serverSO.currPlayers = %val;
        %this.serverSO.display();
      } else if(%key $= "maxPlayers") {
        %this.serverSO.maxPlayers = %val;
        %this.serverSO.display();
      } else if(%key $= "brickCount") {
        %this.serverSO.brickcount = %val;
        %this.serverSO.display();
      } else if(%key $= "name") {
        %this.serverSO.brickcount = %val;
        %this.serverSO.display();
      } else if(%key $= "passworded") {
        %this.serverSO.pass = (%val ? "Yes" : "No");
        %this.serverSO.display();
      }
  }
}

package GlassServerList {
  function ServerSO::display(%this) {
    parent::display(%this);
    GlassServerList.addServer(%this);
  }

  function JoinServerGui::joinServer(%gui) {
    parent::joinServer(%gui);
    for(%i = 0; %i < ServerInfoGroup.getCount(); %i++) {
      %sso = ServerInfoGroup.getObject(%i);
      if(isObject(%sso.glassTcp))
        %sso.glassTcp.disconnect();
    }
  }
};
activatePackage(GlassServerList);
