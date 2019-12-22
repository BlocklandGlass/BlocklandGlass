function GlassAuth::init() {
  // THIS NEEDS TO BE MOVED
  GlassFriendsGui_HeaderText.position = "10 19";
  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Disconnected");
  GlassLive::setPowerButton(0);
  GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  GlassFriendsGui_StatusSelect.setVisible(false);

	if(isObject(GlassAuth)) {
    GlassLog::log("\c2Glass Auth Re-initializing");
		GlassAuth.delete();
	}

	new ScriptObject(GlassAuth) {
    debug         = false;

		heartbeatRate = 5; //minutes
    usingDAA      = false;
	};

  GlassAuth.clearIdentity(); //preps blank identity
}

//================================================================
// Authentication
//================================================================

function GlassAuth::heartbeat(%this) {
	%this.authCheck();

	return %this.scheduleHeartbeat();
}

function GlassAuth::scheduleHeartbeat(%this) {
	cancel(%this.heartbeat);
  return %this.heartbeat = %this.schedule(%this.heartbeatRate * 60 * 1000, "heartbeat");
}

function GlassAuth::authCheck(%this) {
  // do we have a blockland account?
  if(getNumKeyId() $= "") {
    return;
  }

  GlassLog::debug("Glass Auth: \c2authCheck");

  if(%this.authing) {
    //make sure there's only one auth request going
    if(%this.debug)
      error("\c2  Auth in progress!");
    return;
  }
  %this.authing = true;


  %this.setUseDAA(); // loads .forceDAA from the GlassSetting

  // should we be using DAA?
  if(%this.forceDAA == -1) {
    %useDAA = false;
  } else if(%this.forceDAA == 1) {
    %useDAA = true;
  } else {
    // only use DAA if a hash exists
    %useDAA = (GlassSettings.cacheFetch("Auth::DAA_" @ getNumKeyId()) !$= "");
  }

  if(%useDAA != %this.usingDAA) {
    if(%this.isAuthed) {
      %this.clearIdentity();
    }
  }

  %this.usingDAA = %useDAA;

  %hash = GlassSettings.cacheFetch("Auth::DAA_" @ getNumKeyId());
  if(%hash !$= "")
    GlassLog::debug("\c1  Hash    : " @ (%hash $= "" ? "" : (getSubStr(%hash, 0, 5) @ "....." @ getSubStr(%hash, strLen(%hash)-5, 5)) ));
    GlassLog::debug("\c1  ForceDAA: " @ %this.forceDAA);
    GlassLog::debug("\c1  usingDAA: " @ %this.usingDAA);

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

function GlassAuth::reident(%this) {
  // end the old session, start a new
  if(%this.authing) return;

  %this.clearIdentity();
  %this.authCheck();
}

function GlassAuth::startNewAuth(%this) {
  %url = "http://" @ Glass.address @ "/api/3/auth.php?username=" @ urlenc($Pref::Player::NetName) @ "&blid=" @ getNumKeyId() @ "&action=ident&authType=" @ (%this.usingDAA ? "daa" : "default");

  %method = "GET";
  %downloadPath = "";
  %className = "GlassAuthTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassAuth::checkinDefault(%this) {
  %url = "http://" @ Glass.address @ "/api/3/auth.php?";
  %url = %url @ "&action=checkin";

  %url = %url @ "&username=" @ urlenc($Pref::Player::NetName);
  %url = %url @ "&blid=" @ getNumKeyId();

  %url = %url @ "&authType=" @ (%this.usingDAA ? "daa" : "default");
  %url = %url @ "&ident=" @ %this.ident;

  %method = "GET";
  %downloadPath = "";
  %className = "GlassAuthTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassAuth::validateIdentity(%this) {
  if(%this.lastName !$= $Pref::Player::NetName && %this.lastName !$= "") {
    %changed = true;
  } else if(%this.lastBlid !$= getNumKeyId() && %this.lastBlid !$= "") {
    %changed = true;
  } else {
    %changed = false;
  }

  if(%changed) {
    GlassLog::log("Glass Auth: \c1Identity changed, re-authenticating");
    %this.clearIdentity();
  }
}

function GlassAuth::clearIdentity(%this) {
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

  if(isObject(%this.verify_digest))
    %this.verify_digest.delete();

  %this.verify_daa_hash = "";
  %this.verify_digest = "";

  %this.firstAuth = false;
  %this.isAuthed  = false;

  cancel(GlassAuth.heartbeat);
}

function GlassAuth::onAuthSuccess(%this) {
  %this.authing  = false;
  %this.isAuthed = true;
	if(!%this.firstAuth) {
    if(GlassSettings.get("Live::StartupConnect"))
      GlassLive::connectToServer();

    //GlassModManager::placeCall("rtb");
  	%this.firstAuth = true;
	}

  if(GlassAuth.usingDAA) {
    if(GlassPasswordGui.isAwake()) {
      canvas.popDialog(GlassPasswordGui);
      glassMessageBoxOk("Success", "DAA authentication successful!");
    }
  }

  %this.scheduleHeartbeat();
}

function GlassAuth::onAuthEnd(%this) {
  GlassLive::disconnect();
  %this.clearIdentity();
  %this.authing = false;
}

function GlassAuth::onAuthFailed(%this) {
  GlassLive::disconnect();
  %this.clearIdentity();
  %this.authing = false;
}

function GlassAuthTCP::onDone(%this, %error) {
	Glass::debug(%this.buffer);

	if(!%error) {
		%jsonError = jettisonParse(collapseEscape(%this.buffer));
		if(!%jsonError && $JSON::Type $= "object") {
		   %object = $JSON::Value;

      switch$(%object.status) {
        case "success":
          //successful authentication
          %time = getSubStr(getWord(getDateTime(), 1), 0, 5);
          if(!GlassAuth.isAuthed)
            GlassLog::log("Glass Auth: \c4SUCCESS \c3" @ %time);
          else
            GlassLog::log("Glass Auth: \c4RENEWED \c3" @ %time);

          GlassAuth.hasAccount = %object.hasGlassAccount;

          if(GlassAuth.usingDAA) {
            GlassAuth.ident = GlassAuth.daa_opaque;
          } else {
            GlassAuth.ident = %object.ident;
          }

          //there is an unverified web account associated with this blid
          if(%object.action $= "verify") {
            GlassAuth.verifyWebAccount(%object.daa);
          } else {
  					GlassAuth.onAuthSuccess();
          }

        //================================================
        // DigestAccessAuthentication
        //================================================

        case "daa-keys":
          //we requested the need DAA info, prepare response
          if(!GlassAuth.usingDAA) {
            GlassLog::error("Glass Auth: \c1Got DAA ident, but not using DAA!");
            return;
          }
          GlassLog::debug("Glass Auth: \c5daa-keys");

          GlassAuth.startDAA(%object.daa);


        case "daa-required":
          //we tried to authenticate normally, but DAA is forced
          GlassLog::log("Glass Auth: \c5daa-required");

          if(GlassAuth.forceDAA == -1) {
            // DAA is set to never!
            glassMessageBoxOk("Secure Authentication Required", "You have set your Glass authentication settings to never use Digest Access Authentication, but the authentication server has required you to do so. To use Glass, please update your settings.");
            return;
          }

          GlassAuth.usingDAA = true;
          GlassAuth.startDAA(%object.daa, true, %object.role);

        case "daa-hash-missing":
          glassMessageBoxOk("Hash Missing", "Because DAA is a new system, there is no hash generated for your password yet. As your password is not kept in plain text, you need to simply log out and log back in to the Blockland Glass website and we'll do all the hard work.<br><br>When you're done, press OK and we'll try again.", "GlassAuth.reident();");


        //================================================
        // Web Account Verification
        //================================================

        case "verify-success":
          GlassAuth.accountVerficationSuccess();

        case "verify-failed":
          GlassAuth.accountVerficationFailed();

        //================================================
        // Auth Results
        //================================================


        case "barred":
          GlassLog::log("Glass Auth: \c2BARRED");
          glassMessageBoxOk("Barred", "Sorry,<br><br>You have been barred from using online Blockland Glass services.");

        case "error":
          GlassLog::error("Glass Auth: \c2ERROR");
          if(%object.error !$= "")
            GlassLog::error(%object.error);

        case "bl-failed":
          GlassLog::error("Glass Auth: \c2BL-FAILED");
			 glassMessageBoxYesNo("Blockland Auth Failed", "We were unable to authenticate you with Blockland Glass because you are not successfully authenticated with Blockland. Would you like to reauthenticate?", "auth_init_client();", "");
			 GlassAuth.authing = false;

        case "failed":
          GlassLog::log("Glass Auth: \c2FAILED");
          if(%object.message !$= "")
            Glasslog::log("            \c2" @ %object.message);

          GlassAuth.failedCt++;
          GlassAuth.onAuthFailed();

          if(GlassAuth.failedCt >= 3) {
            if(GlassAuth.usingDAA) {
              // we're using DAA and got a failure. clear the password
              GlassSettings.cachePut("Auth::DAA_" @ getNumKeyId(), "");
              glassMessageBoxYesNo("Authentication Failed", "We were unable to successfully authenticate your Glass account. Would you like to try again?", "GlassAuth.reident();");
            }
          } else {
            GlassLog::debug("\c3Retrying connection...");
            GlassAuth.reident();
          }


        case "unauthorized":
          if(%object.expired)
            GlassLog::log("Glass Auth: \c5expired");
          else
            GlassLog::log("Glass Auth: \c5unauthorized");

          // something has gone wrong or the key expired
          GlassAuth.authing = false;
          GlassAuth.reident();

        default:
          GlassLog::error("Glass Auth: \c2UNKNOWN RESPONSE (" @ %object.status @ ")");
          GlassLog::debug(%this.buffer);
      }

		} else {
		  GlassLog::error("Glass Auth: \c2INVALID RESPONSE");
      GlassLog::debug(%this.buffer);

      GlassAuth.authing = false;
      GlassAuth.schedule(5*1000, reident);
		}


	} else {
	  GlassLog::error("Glass Auth: \c2CONNECTION ERROR " @ %error);
     // we have to assume that this is the end of authing, as there was an error
     // this allows us to retry
     GlassAuth.authing = false;
     GlassAuth.schedule(5*1000, reident);
	}
}

function GlassAuthTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

//================================================================
// Digest Access Authentication
//================================================================

function GlassAuth::startDAA(%this, %data, %required, %role) {
  // we're starting a new exchange
  if(isObject(%this.daa))
    %this.daa.delete();

  %this.daa = DigestAccessAuthentication(getNumKeyId(), "/api/3/auth.php", "sha1");
  %this.daa.method = "POST";

  // take in info from server, make appropriate hashes
  %res = GlassAuth.daa.retrieveIdent(%data);
  if(%res) {
    // we need these to reauthenticate future requests
    GlassAuth.daa_nonce  = %data.nonce;
    GlassAuth.daa_opaque = %data.opaque;

    // get the username/realm/password hash for this user
    %hash = GlassSettings.cacheFetch("Auth::DAA_" @ getNumKeyId());
    if(%hash $= "") {
      // there is no stored hash!

      if(%required) {
        // the server has required the client to use DAA, we'll give them a prompt
        GlassAuth::requireDAAPrompt(%role);
      } else {
        // open the password prompt gui
        canvas.pushDialog(GlassPasswordGui);
      }

    } else {

      // load in the password
      GlassAuth.daa.restorePassword(%hash);
      GlassAuth.sendDigestIdent();
    }

  } else {
    GlassLog::error("Glass Auth: \c2Invalid DAA-KEYS response!");
  }
}

function GlassAuth::sendDigestIdent(%this) {
  // create the object we want the server to receive
  %authData = JettisonObject();
  %authData.set("blid", "string", getNumKeyId());
  %authData.set("name", "string", $Pref::Player::NetName);

  // encapsulate and stringify
  %obj = %this.daa.digest(%authData);
  %json = jettisonStringify("object", %obj);

  %tcp = TCPClient("POST", Glass.address, 80, "/api/3/auth.php?action=daa-ident&ident=" @ %this.daa_opaque, %json, "", "GlassAuthTCP");

  %authData.delete();
  %obj.delete();
}

function GlassAuth::checkinDAA(%this) {
  %authData = JettisonObject();
  %authData.set("blid", "string", getNumKeyId());
  %authData.set("name", "string", $Pref::Player::NetName);

  // encapsulate and stringify
  %obj = %this.daa.digest(%authData);
  %json = jettisonStringify("object", %obj);

  %tcp = TCPClient("POST", Glass.address, 80, "/api/3/auth.php?action=daa-checkin&ident=" @ %this.daa_opaque, %json, "", "GlassAuthTCP");

  %authData.delete();
  %obj.delete();
}

function GlassAuth::updateDAASetting() {
  %this = GlassAuth;

  %this.setUseDAA();

  if(%this.forceDAA == -1) {
    %useDAA = false;
  } else if(%this.forceDAA == 1) {
    %useDAA = true;
  } else {
    // only use DAA if a hash exists
    %useDAA = (GlassSettings.cacheFetch("Auth::DAA_" @ getNumKeyId()) $= "");
  }

  if(%useDAA != %this.usingDAA) {
    if(%this.isAuthed) {
      %this.clearIdentity();
    }

    %this.usingDAA = %useDAA;
    %this.authCheck();
  }
}

function GlassAuth::setUseDAA(%this) {
  %setting = GlassSettings.get("Auth::useDAA");

  switch$(GlassSettings.get("Auth::useDAA")) {
    case "Always":
      GlassAuth.forceDAA = 1;

    case "Never":
      GlassAuth.forceDAA = -1;

    case "Default":
      GlassAuth.forceDAA = 0;
  }
}

function GlassAuth::requireDAAPrompt(%role) {
  if(strlen(%role) > 0) {
    glassMessageBoxOk("DAA Required", "For the role of <font:verdana bold:13>" @ %role @ "<font:verdana:13>, using Digest Access Authentication is required to ensure Blockland Glass security.", "canvas.pushDialog(GlassPasswordGui);");
  } else {
    glassMessageBoxOk("DAA Required", "You have opted to require Digest Access Authentication to use Blockland Glass. Press OK to insert your password.", "canvas.pushDialog(GlassPasswordGui);");
  }
}

function GlassAuth::submitPassword() {
  %password = trim(GlassPasswordGui_Password.getValue());
  if(strlen(%password) == 0) {
    glassMessageBoxOk("Invalid Password", "Please enter a valid password!");
    return;
  }

  //DAA
  GlassAuth.usingDAA = true;

  %hash = GlassAuth.daa.setPassword(%password);
  GlassSettings.cachePut("Auth::DAA_" @ getNumKeyId(), %hash);
  GlassAuth.daa_hash = %hash;
  GlassAuth.sendDigestIdent();
}

function GlassPasswordGui::onWake(%this) {
  GlassPasswordGui_Password.setValue("");
}

function GlassPasswordGui::onSleep(%this) {
  GlassPasswordGui_Password.setValue("");
}

//================================================================
// Account Verification
//================================================================

function GlassAuth::verifyWebAccount(%this, %daa) {
  // verify daa is kept seperate as verification may fail or the user
  // hasn't actually registered an account
  %this.verify_daa = %daa;

  canvas.pushDialog(GlassVerifyAccountGui);
}

function GlassAuth::attemptAccountVerification(%this, %email, %pass) {
  %daa = %this.verify_daa;

  if(!isObject(%this.verify_digest)) {
    %digest = DigestAccessAuthentication(getNumKeyId(), "/api/3/auth.php", "sha1");
    %digest.method = "POST";
    %digest.retrieveIdent(%daa);
  } else {
    %digest = %this.verify_digest;
  }


  %this.verify_daa_hash = %digest.setPassword(%pass);
  %this.verify_digest = %digest;

  %data = JettisonObject();
  %data.set("email", "string", %email);

  // encapsulate and stringify
  %obj = %digest.digest(%data);
  %json = jettisonStringify("object", %obj);

  %path = "/api/3/auth.php";
  %path = %path @ "?action=daa-verify-account";
  %path = %path @ "&ident=" @ %daa.opaque;

  %tcp = TCPClient("POST", Glass.address, 80, %path, %json, "", "GlassAuthTCP");

  %data.delete();
  %obj.delete();
}

function GlassAuth::accountVerficationSuccess(%this) {
  // we can continue using DAA!
  GlassSettings.cachePut("Auth::DAA_" @ getNumKeyId(), %this.verify_daa_hash);
  %this.verify_daa_hash = "";
  %this.verify_digest.delete();

  if(%this.forceDAA != -1 && !%this.usingDAA) // if we're not using DAA and it's not disabled, lets use it
    %this.reident();

  canvas.popDialog(GlassVerifyAccountGui);
  glassMessageBoxOk("Verification Successful", "You have successfully verified your account!");
}

function GlassAuth::accountVerficationFailed(%this) {
  glassMessageBoxOk("Verification Failed", "Either your e-mail address or password were incorrect. Please try again!");
  %this.verify_daa_hash = "";
}

function GlassVerifyAccountGui::onWake(%this) {
  GlassVerifyAccountGui_Accept.enabled = false;
  GlassVerifyAccountGui_Accept.mcolor = "200 200 200 128";

  GlassVerifyAccountGui_Email.setText("");
  GlassVerifyAccountGui_Password.setText("");
}

function GlassVerifyAccountGui::onSleep(%this) {
  GlassVerifyAccountGui_Accept.enabled = false;
  GlassVerifyAccountGui_Accept.mcolor = "200 200 200 128";

  GlassVerifyAccountGui_Email.setText("");
  GlassVerifyAccountGui_Password.setText("");
}

function GlassVerifyAccountGui::next(%this) {
  GlassVerifyAccountGui_Password.makeFirstResponder(true);
}

function GlassVerifyAccountGui::submit(%this) {
	%email = GlassVerifyAccountGui_Email.getValue();
	%pass  = GlassVerifyAccountGui_Password.getValue();

  GlassAuth.attemptAccountVerification(%email, %pass);
}

function GlassVerifyAccountGui::decline(%this, %conf) {
  if(!%conf) {
    glassMessageBoxYesNo("Are You Sure", "Are you sure you didn't register an account? Clicking \"Yes\" will delete any accounts claiming your BL_ID", "GlassVerifyAccountGui.decline(true);");
    return;
  }

  if(isObject(GlassAuth.verify_digest))
    GlassAuth.verify_digest.delete();

  GlassAuth.verify_daa_hash = "";
  GlassAuth.verify_digest = "";

	%url = "http://" @ Glass.address @ "/api/3/auth.php";
  %url = %url @ "?ident=" @ urlenc(GlassAuth.ident);
  %url = %url @ "&action=verify-reject";

	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);

  canvas.popDialog(GlassVerifyAccountGui);
}

function GlassVerifyAccountGui::updateInput() {
	%email = GlassVerifyAccountGui_Email.getValue();
	%pass  = GlassVerifyAccountGui_Password.getValue();

  %valid = false;
  if(strpos(%email, "@") > 0 && strpos(%email, ".") > 0) {
    if(strlen(%pass) > 0) {
      %valid = true;
    }
  }

	GlassVerifyAccountGui_Accept.enabled = %valid;
	GlassVerifyAccountGui_Accept.mcolor = (%valid ? "84 217 140 128" : "200 200 200 128");
}

package GlassAuthPackage {
	function MM_AuthBar::blinkSuccess(%this) {
    parent::blinkSuccess(%this);
    GlassAuth.schedule(0, authCheck); //getNumKeyId doesn't update immediately
	}

  // function MM_AuthBar::blinkFail(%this) {
    // parent::blinkFail(%this);
  // }
};
activatePackage(GlassAuthPackage);
