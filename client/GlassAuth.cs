function GlassAuth::init() {
  GlassFriendsGui_HeaderText.position = "10 13";
  GlassFriendsGui_HeaderText.setText("<just:center><font:verdana:22><color:e74c3c>Disconnected");
  GlassLive::setPowerButton(0);
  GlassFriendsGui_InfoSwatch.color = "210 210 210 255";
  GlassLive_StatusPopUp.setVisible(false);

	if(isObject(GlassAuth)) {
		GlassAuth.delete();
	}

	new ScriptObject(GlassAuth) {
		heartbeatRate = 10; //minutes
	};
}

function GlassAuth::heartbeat(%this) {
	//basically just keeping the session alive
	cancel(%this.heartbeat);
	echo("BLG heartbeat");
	if(Glass.devLocal) {
		%this.ident = "devLocal";
		%this.onAuthSuccess();
		return;
	}
	%this.check();
	return %this.heartbeat = %this.schedule(%this.heartbeatRate * 60 * 1000, "heartbeat");
}

function GlassAuth::check(%this) {
	%url = "http://" @ Glass.address @ "/api/2/auth.php?username=" @ urlenc($Pref::Player::NetName) @ "&blid=" @ getNumKeyId() @ "&action=checkin";
	if(%this.ident !$= "") {
			%url = %url @ "&ident=" @ urlenc(%this.ident);
	}

	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassAuth::verifyAccept() {
	%url = "http://" @ Glass.address @ "/api/2/auth.php?ident=" @ urlenc(GlassAuth.ident) @ "&action=verify&email=" @ urlenc(GlassVerifyAccount_Input.getValue());

	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(GlassVerifyAccountGui);
}

function GlassAuth::verifyDecline() {
	%url = "http://" @ Glass.address @ "/api/2/auth.php?sid=" @ urlenc(GlassAuth.sid) @ "&request=verify&action=reject";
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

function GlassAuth::onAuthSuccess(%this) {
	if(!%this.firstAuth) {
    if(GlassSettings.get("Live::StartupConnect"))
      GlassLive::connectToServer();

    //GlassModManager::placeCall("rtb");
	}

	%this.firstAuth = true;
}

function GlassAuthTCP::onDone(%this) {
	Glass::debug(%this.buffer);

	if(!%error) {
		jettisonParse(collapseEscape(%this.buffer));
		%object = $JSON::Value;
		if($JSON::Type $= "object") {
			GlassAuth.ident = %object.get("ident");

			if(%object.get("status") $= "error") {
				error("Error authenticating: " @ %object.get("error"));
			}

			if(%object.get("status") $= "success") {
				if(%object.get("action") $= "verify") {

					%emails = %object.get("verify_data");
					for(%i = 0; %i < %emails.length; %i++) {
						GlassAuth.emails[%i] = %emails.value[%i];
					}
					GlassAuth.emails = %emails.length;

					GlassVerifyAccount_Accept.enabled = false;
					GlassVerifyAccount_Accept.mcolor = "150 255 150 128";
					GlassVerifyAccount_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/cancel.png");
					GlassVerifyAccount_Input.setText("");
					canvas.pushDialog(GlassVerifyAccountGui);
				} else {
					// echo("Glass auth success");
					GlassAuth.onAuthSuccess();
				}

				if(%object.get("hasGlassAccount")) {
					GlassAuth.hasAccount = true;
				}
			}
		} else {
			warn("Error authenticating with Glass\nIf this continues, please submit a bug report");
			//TODO Send error report, probably feature of 1.1
		}


	} else {
		echo("BLG auth error - " @ %error);
	}

	if(GlassAuth.heartbeat $= "") {
		GlassAuth.heartbeat = GlassAuth.heartbeat();
	}
}

function GlassAuthTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

package GlassAuthPackage {
	function MM_AuthBar::blinkSuccess(%this) {
    parent::blinkSuccess(%this);
    GlassAuth.heartbeat();
    GlassLive.ready = true;
	}

  // function MM_AuthBar::blinkFail(%this) {
    // parent::blinkFail(%this);
  // }
};
activatePackage(GlassAuthPackage);
