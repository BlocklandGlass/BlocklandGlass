function GlassAuth::init() {
  GlassFriendsGui_HeaderText.position = "10 19";
  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Disconnected");
  GlassLive::setPowerButton(0);
  GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  GlassFriendsGui_StatusSelect.setVisible(false);

	if(isObject(GlassAuth)) {
		GlassAuth.delete();
	}

	new ScriptObject(GlassAuth) {
		heartbeatRate = 10; //minutes
    usingDAA = GlassSettings.get("Auth::useDAA");
	};

  GlassAuth.clearIdentity(); //preps blank identity
}

//================================================================
// Authentication
//================================================================

function GlassAuth::heartbeat(%this) {
	cancel(%this.heartbeat);

	%this.check();

	return %this.heartbeat = %this.schedule(%this.heartbeatRate * 60 * 1000, "heartbeat");
}

function GlassAuth::check(%this) {
  %this.validateIdentity();

  %this.lastName = $Pref::Player::NetName;
  %this.lastBlid = getNumKeyId();

  if(getNumKeyId() $= "") {
    return;
  }

  if(%this.usingDAA) {
    %stored = GlassSettings.cacheFetch("Auth::DAA_" @ getNumKeyId());
    if(%stored $= "") {

      if(%this.daa_password !$= "") {
        // we have a password, need info from the server
        // prompt was already filled out
      } else {
        canvas.pushDialog(GlassPasswordGui);
      }

    } else {

      // set some value

    }
  }

	%url = "http://" @ Glass.address @ "/api/3/auth.php?username=" @ urlenc($Pref::Player::NetName) @ "&blid=" @ getNumKeyId() @ "&action=checkin";
	if(%this.ident !$= "") {
		%url = %url @ "&ident=" @ urlenc(%this.ident);
	}

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
    echo("Glass Auth: Identity changed, re-authenicating");
    %this.clearIdentity();
  }
}

function GlassAuth::clearIdentity(%this) {
  %this.lastName = "";
  %this.lastBlid = "";

  if(%this.usingDAA) {
    //clear DAA values
    %this.daa_nonce        = "";
    %this.daa_opaque       = "";
    %this.daa_nonceCounter = 0 ;
    %this.daa_password     = "";
  } else {
    %this.ident = "";
  }

  %this.firstAuth = false;

  cancel(GlassAuth.heartbeat);
}

function GlassAuth::onAuthSuccess(%this) {
	if(!%this.firstAuth) {
    if(GlassSettings.get("Live::StartupConnect"))
      GlassLive::connectToServer();

    //GlassModManager::placeCall("rtb");
  	%this.firstAuth = true;
	}
}

function GlassAuth::onAuthEnd(%this) {
  GlassLive::disconnect();
  %this.clearIdentity();
}

function GlassAuth::onAuthFailed(%this) {
  GlassLive::disconnect();
  %this.clearIdentity();
}

function GlassAuthTCP::onDone(%this) {
	Glass::debug(%this.buffer);

	if(!%error) {
		%jsonError = jettisonParse(collapseEscape(%this.buffer));
		if(%jsonError || $JSON::Type $= "object") {
		   %object = $JSON::Value;

      switch$(%object.status) {
        case "success":
          //successful authentication
          echo("Glass Auth: Success");
    			GlassAuth.ident      = %object.ident;
          GlassAuth.hasAccount = %object.hasGlassAccount;

          if(GlassAuth.usingDAA) {
            if(GlassPasswordGui.isAwake()) {
              canvas.popDialog(GlassPasswordGui);
              glassMessageBoxOk("Success", "DAA authentication sucessful!");
            }
          }

					GlassAuth.onAuthSuccess();

          //there is an unverified web account associated with this blid
          if(%object.action $= "verify") {
            GlassAuth::verifyWebAccount(%object.verify_data);
          }

        case "daa-keys":
          //we requested the need DAA info, prepare response

          //something something hash the password

          %store = "";
          GlassSettings.cachePut("Auth::DAA_" @ getNumKeyId(), %store);

        case "daa-required":
          //we tried to authenticate normally, but DAA is forced
          echo("Glass Auth: DAA-Required");
          GlassAuth.usingDAA = true;
          GlassAuth::requireDAAPrompt(%object.role);

        case "daa-hash-missing":
          glassMessageBoxOk("Hash Missing", "Because DAA is a new system, there is no hash generated for your password yet. As your password is not kept in plain text, you need to simply log out and log back in to the Blockland Glass website and we'll do all the hard work.<br><br>When you're done, press OK and we'll try again.", "GlassAuth::retryDAA();");

        case "barred":
          echo("Glass Auth: BARRED");
          glassMessageBoxOk("Barred", "Sorry,<br><br>You have been barred from using online Blockland Glass services");

        case "error":
          echo("Glass Auth: ERROR");
          if(%object.error !$= "")
            echo(%object.error);

        case "failed":
          echo("Glass Auth: FAILED");
          if(%object.message !$= "")
            echo(%object.message);

          GlassAuth.onAuthFailed();
      }

		} else {
			warn("Glass Auth: INVALID RESPONSE");
		}


	} else {
		echo("Glass Auth: CONNECTION ERROR " @ %error);
	}
}

function GlassAuthTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

//================================================================
// Digest Access Authentication
//================================================================

function GlassAuth::updateDAASetting() {
  %force = GlassSettings.get("Auth::uesDAA");

  if(%force && !GlassAuth.usingDAA && GlassAuth.authed) {
    GlassAuth.clearIdentity();
    GlassAuth.onAuthEnd();

    GlassAuth.heartbeat();
  } else {
    //otherwise, DAA
  }
}

function GlassAuth::requireDAAPrompt(%role) {
  if(strlen(%role) > 0) {
    messageBoxOk("DAA Required", "For the role of <font:verdana bold:12>" @ %role @ "<font:verdana:12>, using Digest Access Authentication is required to ensure Blockland Glass security.", "canvas.pushDialog(GlassPasswordGui);");
  } else {
    messageBoxOk("DAA Required", "You have opted to require Digest Access Authentication to use Blockland Glass. Press OK to insert your password.", "canvas.pushDialog(GlassPasswordGui);");
  }
}

function GlassAuth::submitPassword() {
  %password = trim(GlassPasswordGui_Password.getValue());
  if(strlen(%password) == 0) {
    glassMessageBoxOk("Invalid Password", "Please enter a valid password!");
    return;
  }

  //DAA
  GlassAuth.daa_password = %password;
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

function GlassAuth::verifyWebAccount(%emails) {
  for(%i = 0; %i < %emails.length; %i++) {
    GlassAuth.emails[%i] = %emails.value[%i];
  }
  GlassAuth.emails = %emails.length;

  GlassVerifyAccount_Accept.enabled = false;
  GlassVerifyAccount_Accept.mcolor = "150 255 150 128";
  GlassVerifyAccount_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/cancel.png");
  GlassVerifyAccount_Input.setText("");
  canvas.pushDialog(GlassVerifyAccountGui);
}

function GlassAuth::verifyAccept() {
	%url = "http://" @ Glass.address @ "/api/3/auth.php?ident=" @ urlenc(GlassAuth.ident) @ "&action=verify&email=" @ urlenc(GlassVerifyAccount_Input.getValue());

	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(GlassVerifyAccountGui);
}

function GlassAuth::verifyDecline() {
	%url = "http://" @ Glass.address @ "/api/3/auth.php?sid=" @ urlenc(GlassAuth.sid) @ "&request=verify&action=reject";
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(GlassVerifyAccountGui);
}

function GlassAuth::updateInput() {
	%val = GlassVerifyAccount_Input.getValue();
	for(%i = 0; %i < GlassAuth.emails; %i++) {
		if(%val $= GlassAuth.emails[%i]) {
			GlassVerifyAccount_Accept.enabled = true;
			GlassVerifyAccount_Accept.mcolor = "150 255 150 255";
			GlassVerifyAccount_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/accept_button.png");
			return;
		}
	}

	GlassVerifyAccount_Accept.enabled = false;
	GlassVerifyAccount_Accept.mcolor = "150 255 150 128";
	GlassVerifyAccount_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/cancel.png");
}

package GlassAuthPackage {
	function MM_AuthBar::blinkSuccess(%this) {
    parent::blinkSuccess(%this);
    GlassAuth.heartbeat();
    GlassLive.ready = true;

    if(GlassAuth.blid !$= "") {
      if(GlassAuth.blid !$= getNumKeyId()) {
        GlassLiveConnection.doDisconnect();

        GlassAuth.clearIdentity();
        GlassAuth.heartbeat();
      }
    }

    GlassAuth.blid = getNumKeyId();
	}

  // function MM_AuthBar::blinkFail(%this) {
    // parent::blinkFail(%this);
  // }
};
activatePackage(GlassAuthPackage);
