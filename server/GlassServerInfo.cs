function GlassServerInfo::connectToServer() {
  if(!isObject(GlassServerInfo)) {
    new TCPObject(GlassServerInfo);
  }

  if(!GlassServerInfo.connected)
    GlassServerInfo.connect(Glass.address @ ":27004");
}

function GlassServerInfo::onConnected(%this) {
  cancel(%this.reconnect);
  //no auth needed, ip address alone is enough to validate
  %this.connected = true;
  %obj = JettisonObject();

  %obj.set("type", "string", "identify");
  %obj.set("port", "string", $Server::Port);
  %obj.set("blid", "string", getNumKeyId());

  %obj.set("serverName", "string", $Pref::Server::Name);

  %this.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassServerInfo::onDisconnect(%this) {
  %this.connected = false;
  %this.reconnect = %this.schedule(1000+getRandom(0, 1000), "connectToServer");
}

function GlassServerInfo::onDNSFailed(%this) {
  %this.connected = false;
  %this.reconnect = %this.schedule(1000+getRandom(0, 1000), "connectToServer");
}

function GlassServerInfo::onConnectFailed(%this) {
  %this.connected = false;
  %this.reconnect = %this.schedule(1000+getRandom(0, 1000), "connectToServer");
}

function GlassServerInfo::updateField(%this, %key, %val) {
  if(!%this.connected)
    return;

  %obj = JettisonObject();

  %obj.set("type", "string", "updateValue");
  %obj.set("key", "string", %key);
  %obj.set("value", "string", %val);

  %this.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassServerInfo::onServerClose() {
  if(isObject(GlassServerInfo)) {
    %obj = JettisonObject();

    %obj.set("type", "string", "serverClose");

    GlassServerInfo.send(jettisonStringify("object", %obj) @ "\r\n");
    GlassServerInfo.disconnect();
    GlassServerInfo.delete();
  }
}

package GlassServerInfo {
  function GameConnection::autoAdminCheck(%client) {
    parent::autoAdminCheck(%client);
    GlassServerInfo.updateField("players", ClientGroup.getCount());
  }

  function GameConnection::onClientLeaveGame(%client) {
    parent::onClientLeaveGame(%client);
    GlassServerInfo.updateField("players", ClientGroup.getCount()-1);
  }

  function fxDTSBrick::onAdd(%this) {
    parent::onAdd(%this);
    GlassServerInfo.updateField("brickCount", getBrickcount());
  }

  function fxDTSBrick::onRemove(%this) {
    parent::onRemove(%this);
    GlassServerInfo.updateField("brickCount", getBrickcount());
  }

  function BlocklandPrefSO::updateValue(%this, %value, %updater) {
    parent::updateValue(%this, %value, %updater);
    if(%this.variable $= "$Pref::Server::MaxPlayers") {
      GlassServerInfo.updateField("maxPlayers", $Pref::Server::MaxPlayers);
    } else if(%this.variable $= "$Pref::Server::Name") {
      GlassServerInfo.updateField("name", $Pref::Server::Name);
    } else if(%this.variable $= "$Pref::Server::Password") {
      GlassServerInfo.updateField("passworded", ($Pref::Server::Password !$= ""));
    }
  }

  function destroyServer() {
    GlassServerInfo::onServerClose();
    parent::destroyServer();
  }

  function postServerTCPObj::connect(%this, %addr) {
    parent::connect(%this, %addr);

    GlassServerInfo::connectToServer();
  }
};
activatePackage(GlassServerInfo);
