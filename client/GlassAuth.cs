function GlassAuth::init() {
	if(isObject(GlassAuth)) {
		GlassAuth.delete();
	}

	new ScriptObject(GlassAuth) {
		heartbeatRate = 4;
	};
	GlassAuth.heartbeat();
}

function GlassAuth::heartbeat(%this) {
	//basically just keeping the session alive
	cancel(%this.heartbeat);
	echo("BLG heartbeat");
	%this.check();
	%this.heartbeat = %this.schedule(%this.heartbeatRate * 60 * 1000, "heartbeat");
}

function GlassAuth::check(%this) {
	%url = "http://" @ BLG.address @ "/api/auth.php?sid=" @ GlassAuth.sid @ "&request=checkauth&name=" @ $Pref::Player::NetName @ "&version=" @ BLG.version;
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
}

//note to self: change the object names, this is rough backwards-compatible code

function BLG_Verify::accept(%this) {
	%url = "http://" @ BLG.address @ "/api/auth.php?sid=" @ GlassAuth.sid @ "&request=verify&action=confirm";
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(BLG_VerifyAccount);
}

function BLG_Verify::decline(%this) {
	%url = "http://" @ BLG.address @ "/api/auth.php?sid=" @ GlassAuth.sid @ "&request=verify&action=reject";
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	canvas.popDialog(BLG_VerifyAccount);
}

function GlassAuthTCP::onDone(%this) {
	echo(%this.buffer);
	if(!%error) {
		%object = parseJSON(%this.buffer);
		GlassAuth.sid = %object.get("sid");
		echo("Setting SID: " @ %object.get("sid"));
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
				canvas.pushDialog(BLG_VerifyAccount);
			} else {
				echo("BLG auth success");
			}
		}

		if(GlassAuth.heartbeat $= "") {
			GlassAuth.heartbeat = GlassAuth.heartbeat();
		}
	} else {

	}
}

function GlassAuthTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}
