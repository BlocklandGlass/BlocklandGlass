function GlassAuth::init() {
	if(isObject(GlassAuth)) {
		GlassAuth.delete();
	}

	new ScriptObject(GlassAuth);
	GlassAuth.check();
}

function GlassAuth::check(%this) {
	%url = "http://" @ BLG.address @ "/api/auth.php?sid=" @ %this.sid @ "&request=checkauth&name=" @ $Pref::Player::NetName;
	%method = "GET";
	%downloadPath = "";
	%className = "GlassAuthTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
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
				canvas.pushDialog(BLG_VerifyAccount);
			} else {
				echo("BLG auth success");
			}
		}
	} else {

	}
}

function GlassAuthTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}