function GlassAuthS::init() {
  if(!isObject(GlassAuthS)) {
    new ScriptObject(GlassAuthS) {
      ident = "";
      authedOnce = false;
    };
  }
}

function GlassAuthS::heartbeat(%this) {
  %url = "http://" @ Glass.address @ "/api/3/auth.php?username=" @ urlenc($Pref::Player::NetName) @ "&blid=" @ getNumKeyId() @ "&action=checkin&server=1&port=" @ $Server::Port;
	if(%this.ident !$= "") {
			%url = %url @ "&ident=" @ urlenc(%this.ident);
	}

  %clients = "";
  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);

    %status = "";
    if(%cl.bl_id == getNumKeyId()) {
      %status = "H";
    } else if(%cl.isSuperAdmin) {
      %status = "S";
    } else if(%cl.isAdmin) {
      %status = "A";
    } else if(%cl.isModerator) {
      %status = "M";
    }else if(%cl.statusLetter !$= "") {
      %status = getSubStr(%cl.statusLetter, 0, 1);
    }

    %addr = %cl.getAddress();
    if((%idx = strpos(%addr, ":")) > -1) {
      %addr = getSubStr(%addr, 0, %idx);
    }

    %clients = %clients NL %cl.netname TAB %cl.bl_id TAB %status TAB %cl._glassVersion TAB %addr;
  }

  if(%clients !$= "")
    %clients = getsubstr(%clients, 1, strlen(%clients)-1);

  %url = %url @ "&clients=" @ urlenc(expandEscape(%clients));

	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthSTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassAuthSTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

function GlassAuthSTCP::onDone(%this, %buffer) {
  jettisonParse(%this.buffer);
  %res = $JSON::Value;

  Glass::debug(%this.buffer);

  if(%res.action $= "reauth") {
    GlassAuthS.ident = "";
    GlassAuthS.heartbeat();
  } else {
    GlassAuthS.ident = %res.ident;
  }
}

package GlassAuthS {
  function postServerTCPObj::connect(%this, %addr) {
    parent::connect(%this, %addr);

    if(isObject(GlassAuthS))
      GlassAuthS.heartbeat();

    echo("Posting to Glass server");
  }
};
activatePackage(GlassAuthS);
