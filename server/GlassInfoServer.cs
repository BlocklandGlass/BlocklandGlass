function GlassInfoServer::init() {
  new ScriptObject(GlassInfoServer);
  GlassInfoServer.connectionGroup = new ScriptGroup();
}

function GlassInfoServer::listen(%this) {
  if(isObject(%this.tcp))
    return;

  %this.tcp = new TCPObject(GlassInfoListener);
  %port = %this.tcp.attemptListen($Server::Port); //port is opened and unused by default

  %this.tcp.port = %port;
}

function GlassInfoServer::updateField(%this, %key, %value) {
  for(%i = 0; %i < %this.connectionGroup.getCount(); %i++) {
    %con = %this.connectionGroup.getObject(%i);
    if(%con.listening) {
      %con.send("updateField" TAB %key TAB expandEscape(%value) @ "\r\n");
    }
  }

  %this.field[%key] = %value;
}

//================================
// Listener
//================================

function GlassInfoListener::attemptListen(%this, %port) {
  %res = %this.listen(%port);

  if(!%res) {
    return %this.attemptListen(%port+1);
  } else {
    return %port;
  }
}

function GlassInfoListener::onConnectionRequest(%this, %addr, %id) {
  %con = new TCPObject(GlassInfoClient, %id) {
    addr = %addr;
  };

  GlassInfoServer.connectionGroup.add(%con);
}

//================================
// Client
//================================

function GlassInfoClient::onLine(%this, %line) {
  %cmd = getField(%line, 0);
  switch$(%cmd) {
    case "listen":
      %this.listening = getField(%line, 1);
  }
}

function GlassInfoClient::onDisconnected(%this) {
  %this.schedule(0, delete);
}

//================================
// Fields
//================================

package GlassInfoServer {
  function GameConnection::autoAdminCheck(%client) {
    parent::autoAdminCheck(%client);
    GlassInfoServer.updateField("playercount", ClientGroup.getCount());
  }

  function GameConnection::onClientLeaveGame(%client) {
    parent::onClientLeaveGame(%client);
    GlassInfoServer.updateField("playercount", ClientGroup.getCount()-1);
  }
};
activatePackage(GlassInfoServer);
