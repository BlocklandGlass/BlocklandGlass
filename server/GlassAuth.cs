
function GlassAuthS::init() {
	if(isObject(GlassAuthS)) {
    echo("\c2Glass Server Auth Re-initializing");
		GlassAuthS.delete();
	}

	new ScriptObject(GlassAuthS) {
    debug    = false;
    usingDAA = false; //GlassSettings only loads client sided!
	};

  GlassAuthS.clearIdentity(); //preps blank identity
}

//================================================================
// Authentication
//================================================================

function GlassAuthS::authCheck(%this) {
  // do we have a blockland account?
  if(getNumKeyId() $= "") {
    return;
  }

	if(%this.debug)
    echo("Glass Server Auth: \c2authCheck");

  if(%this.authing) {
    //make sure there's only one auth request going
    if(%this.debug)
      error("\c2  Auth in progress!");
    return;
  }
	%this.authing = true;

  // ensure that the current auth is for the current user
  %this.validateIdentity();
  %this.lastName = $Pref::Player::NetName;
  %this.lastBlid = getNumKeyId();

  if(%this.isAuthed) {
    // the session is authenticated, send a request to keep alive
    if(%this.usingDAA) {
      %this.checkinDAA();
    } else {
      %this.checkinDefault();
    }

  } else {

    // we are not authenticated. send initial "ident" request
    %this.startNewAuth();

  }
}

function GlassAuthS::reident(%this) {
	if(%this.authing) return;

  // end the old session, start a new
  %this.clearIdentity();
  %this.authCheck();
}

function GlassAuthS::startNewAuth(%this) {
  %url = "http://" @ Glass.address @ "/api/3/auth.php?";
  %url = %url @ "&action=ident";

  %url = %url @ "&username=" @ urlenc($Pref::Player::NetName);
  %url = %url @ "&blid=" @ getNumKeyId();
  %url = %url @ "&port=" @ $Server::Port;

  %url = %url @ "&server=1";
  %url = %url @ "&authType=" @ (%this.usingDAA ? "daa" : "default");
  %url = %url @ "&ident=" @ %this.ident;

  %url = %url @ "&clients=" @ urlEnc(%this.getClientList());

  %method = "GET";
  %downloadPath = "";
  %className = "GlassAuthServerTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassAuthS::checkinDefault(%this) {
  %url = "http://" @ Glass.address @ "/api/3/auth.php?";
  %url = %url @ "&action=checkin";

  %url = %url @ "&username=" @ urlenc($Pref::Player::NetName);
  %url = %url @ "&blid=" @ getNumKeyId();
  %url = %url @ "&port=" @ $Server::Port;

  %url = %url @ "&server=1";
  %url = %url @ "&authType=" @ (%this.usingDAA ? "daa" : "default");
  %url = %url @ "&ident=" @ %this.ident;

  %url = %url @ "&clients=" @ urlEnc(%this.getClientList());
  echo(urlEnc(%this.getClientList()));



  %method = "GET";
  %downloadPath = "";
  %className = "GlassAuthServerTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassAuthS::validateIdentity(%this) {
  if(%this.lastName !$= $Pref::Player::NetName && %this.lastName !$= "") {
    %changed = true;
  } else if(%this.lastBlid !$= getNumKeyId() && %this.lastBlid !$= "") {
    %changed = true;
  } else {
    %changed = false;
  }

  if(%changed) {
    echo("Glass Server Auth: Identity changed, re-authenticating");
    %this.clearIdentity();
  }
}

function GlassAuthS::clearIdentity(%this) {
  %this.lastName = "";
  %this.lastBlid = "";

  if(%this.usingDAA) {
    if(isObject(%this.daa))
      %this.daa.delete();
    //clear DAA values
    %this.daa_nonce        = "";
    %this.daa_opaque       = "";
    %this.daa_nonceCounter = 0 ;
    %this.daa_hash         = "";
    %this.daa              = "";
  } else {
    %this.ident = "";
  }

  %this.isAuthed  = false;
  %this.firstAuth = false;
}

function GlassAuthS::onAuthSuccess(%this) {
	%this.authing  = false;
  %this.isAuthed = true;
	if(!%this.firstAuth) {
  	%this.firstAuth = true;
	}
}

function GlassAuthS::onAuthEnd(%this) {
	%this.authing = false;
  %this.clearIdentity();
}

function GlassAuthS::onAuthFailed(%this) {
	%this.authing = false;
  %this.clearIdentity();
}

function GlassAuthServerTCP::onDone(%this) {
	Glass::debug(%this.buffer);

	if(!%error) {
		%jsonError = jettisonParse(collapseEscape(%this.buffer));
		if(!%jsonError && $JSON::Type $= "object") {
		   %object = $JSON::Value;

      switch$(%object.status) {
        case "success":
          //successful authentication
          if(!GlassAuthS.isAuthed)
            echo("Glass Auth: \c4SUCCESS");
          else
            echo("Glass Auth: \c4RENEWED \c3" @ getSubStr(getWord(getDateTime(), 1), 0, 5));

    			GlassAuthS.ident      = %object.ident;
          GlassAuthS.hasAccount = %object.hasGlassAccount;

					GlassAuthS.onAuthSuccess();

          if(GlassAuthS.usingDAA) {
            GlassAuthS.ident = GlassAuthS.daa_opaque;
          }

        case "daa-keys":
          //we requested the need DAA info, prepare response
          if(!GlassAuthS.usingDAA) {
            echo("Glass Server Auth: \c2Got DAA ident, but not using DAA!");
            return;
          }

          GlassAuthS.startDAA(%object.daa);

        case "daa-hash-missing":
          echo("Glass Server Auth: \c2daa-hash-missing, using default auth");
          GlassAuthS.usingDAA = false;

        case "barred":
          echo("Glass Server Auth: \c2BARRED");

        case "error":
          echo("Glass Server Auth: \c2ERROR");
          if(%object.error !$= "")
            echo(%object.error);

        case "failed":
          echo("Glass Server Auth: \c2FAILED");
          if(%object.message !$= "")
            echo(%object.message);

					GlassAuthS.failedCt++;
          GlassAuthS.onAuthFailed();

				case "unauthorized":
          if(%object.expired)
            echo("Glass Server Auth: \c5expired");
          else
            echo("Glass Server Auth: \c5unauthorized");

          // something has gone wrong or the key expired
          GlassAuthS.authing = false;
          GlassAuthS.reident();
        default:
          echo("Glass Server Auth: \c2UNKNOWN RESPONSE (" @ %object.status @ ")");
          echo(%this.buffer);
      }

		} else {
			echo("Glass Server Auth: \c2INVALID RESPONSE");
      echo(%this.buffer);

      GlassAuthS.authing = false;
      GlassAuthS.schedule(10*1000, reident);
		}


	} else {
		echo("Glass Server Auth: \c2CONNECTION ERROR " @ %error);

    GlassAuthS.authing = false;
    GlassAuthS.schedule(10*1000, reident);
	}
}

function GlassAuthServerTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

//================================================================
// Digest Access Authentication
//================================================================

function GlassAuthS::startDAA(%this, %data, %required, %role) {
  // we're starting a new exchange
  if(isObject(%this.daa))
    %this.daa.delete();

  %this.daa = DigestAccessAuthentication(getNumKeyId(), "/api/3/auth.php", "sha1");
  %this.daa.method = "POST";

  // take in info from server, make appropriate hashes
  %res = GlassAuthS.daa.retrieveIdent(%data);
  if(%res) {
    // we need these to reauthenticate future requests
    GlassAuthS.daa_nonce  = %data.nonce;
    GlassAuthS.daa_opaque = %data.opaque;

    // get the username/realm/password hash for this user
    %hash = GlassSettings.cacheFetch("Auth::DAA_" @ getNumKeyId());
    if(%hash $= "") {
      // there is no stored hash!

      if(%required) {
        // the server has required the client to use DAA, we'll give them a prompt
        GlassAuthS::requireDAAPrompt(%role);
      } else {
        // open the password prompt gui
        GlassAuthS.usingDAA = false;
        GlassAuthS.reident();
      }

    } else {

      // load in the password
      GlassAuthS.daa.restorePassword(%hash);
      GlassAuthS.sendDigestIdent();
    }

  } else {
    echo("Glass Server Auth: Invalid DAA-KEYS response!");
  }
}

function GlassAuthS::sendDigestIdent(%this) {
  // create the object we want the server to receive
  %authData = JettisonObject();
  %authData.set("blid", "string", getNumKeyId());
  %authData.set("name", "string", $Pref::Player::NetName);
  %authData.set("clients", "object", %this.getClientListObj());

  // encapsulate and stringify
  %obj = %this.daa.digest(%authData);
  %json = jettisonStringify("object", %obj);

  %tcp = TCPClient("POST", Glass.address, 80, "/api/3/auth.php?server=1&action=daa-ident&ident=" @ %this.daa_opaque, %json, "", "GlassAuthServerTCP");

  %authData.delete();
  %obj.delete();
}

function GlassAuthS::checkinDAA(%this) {
  %authData = JettisonObject();
  %authData.set("blid", "string", getNumKeyId());
  %authData.set("name", "string", $Pref::Player::NetName);
  %authData.set("clients", "array", %this.getClientListObj());

  // encapsulate and stringify
  %obj = %this.daa.digest(%authData);
  %json = jettisonStringify("object", %obj);

  %tcp = TCPClient("POST", Glass.address, 80, "/api/3/auth.php?server=1&action=daa-checkin&ident=" @ %this.daa_opaque, %json, "", "GlassAuthServerTCP");

  %authData.delete();
  %obj.delete();
}

function GlassAuthS::getClientList(%this) {

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
    } else if(%cl.statusLetter !$= "") {
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

  return expandEscape(%clients);
}

function GlassAuthS::getClientListObj(%this) {

  %listObj = JettisonArray();

  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);
    %clObj = JettisonObject();

    %status = "";
    if(%cl.bl_id == getNumKeyId()) {
      %status = "H";
    } else if(%cl.isSuperAdmin) {
      %status = "S";
    } else if(%cl.isAdmin) {
      %status = "A";
    } else if(%cl.isModerator) {
      %status = "M";
    } else if(%cl.statusLetter !$= "") {
      %status = getSubStr(%cl.statusLetter, 0, 1);
    }

    %addr = %cl.getAddress();
    if((%idx = strpos(%addr, ":")) > -1) {
      %addr = getSubStr(%addr, 0, %idx);
    }

    %clObj.set("name", "string", %cl.netname);
    %clObj.set("blid", "string", %cl.bl_id);
    %clObj.set("status", "string", %status);
    %clObj.set("version", "string", %cl._glassVersion);
    %clObj.set("ip", "string", %addr);

    %listObj.push("object", %clObj);
  }


  return %listObj;
}

package GlassAuthS {
  function postServerTCPObj::connect(%this, %addr) {
    parent::connect(%this, %addr);

    if(isObject(GlassAuthS))
      GlassAuthS.authCheck();

    echo("Posting to Glass server");
  }
};
activatePackage(GlassAuthS);
