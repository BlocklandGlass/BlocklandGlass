function GlassAuth::init() {
	if(isObject(GlassAuth)) {
		GlassAuth.delete();
	}

	new ScriptObject(GlassAuth) {
		heartbeatRate = 4; //minutes
	};
}

function GlassAuth::heartbeat(%this) {
	//basically just keeping the session alive
	cancel(%this.heartbeat);
	echo("BLG heartbeat");
	%this.check();
	return %this.heartbeat = %this.schedule(%this.heartbeatRate * 60 * 1000, "heartbeat");
}

function GlassAuth::check(%this) {
	%url = "http://" @ Glass.address @ "/api/auth.php?sid=" @ urlenc(GlassAuth.sid) @ "&request=checkauth&name=" @ urlenc($Pref::Player::NetName) @ "&version=" @ urlenc(Glass.version);
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
}

//note to self: change the object names, this is rough backwards-compatible code

function BLG_Verify::accept(%this) {
	%url = "http://" @ Glass.address @ "/api/auth.php?sid=" @ urlenc(GlassAuth.sid) @ "&request=verify&action=confirm";
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(GlassVerifyAccountGui);
}

function BLG_Verify::decline(%this) {
	%url = "http://" @ Glass.address @ "/api/auth.php?sid=" @ GlassAuth.sid @ "&request=verify&action=reject";
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(GlassVerifyAccountGui);
}

function GlassAuthTCP::onDone(%this) {
	if($Glass::Debug)
		echo(%this.buffer);

	if(!%error) {
		%object = parseJSON(collapseEscape(%this.buffer));
		if(getJSONType(%object) $= "hash") {
			GlassAuth.sid = %object.get("sid");
			//echo("Setting SID: " @ %object.get("sid"));
			if(%object.get("status") $= "error") {
				error("error authenticating: " @ %object.get("error"));
			}

			if(%object.get("status") $= "success") {
				if(%object.get("action") $= "verify") {
					echo("Opening auth dialog");
					%name = %object.get("actiondata").get("name");
					%blid = %object.get("actiondata").get("blid");
					BLG_Verify_Username.setText(%name);
					BLG_Verify_BLID.setText(%blid);
					canvas.pushDialog(GlassVerifyAccountGui);
				} else {
					echo("BLG auth success");
				}

				//echo(%object.get("hasGlassAccount"));
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
		GlassAuth.heartbeat();
		parent::blinkSuccess(%this);
	}
};
activatePackage(GlassAuthPackage);
