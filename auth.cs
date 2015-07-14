return; //depreciated
new ScriptObject(BLG_Con) {
	apiPath = "/api/api.php";
};

function BLG_Con::fetchTCP(%this) {
	return new TCPObject() {
		BLG = true;
	};
}

function BLG_Con::setReady(%this, %tcp) {
	%tcp.connect(BLG.address @ ":80");
}

function BLG_Con::pollServer(%this) {
	cancel(%this.retrySch);
	%tcp = %this.fetchTCP();
	%this.sendMessage("init", "BLG_Con.pollServerRet", $Pref::Player::NetName, BLG.version);
}

function BLG_Con::pollServerRet(%this, %line) {
	canvas.pushDialog(GlassModManagerGui);
	%call = getField(%line, 0);
	switch$(%call) {
		case "update":
			//error("YOU FUCKING IDIOT. (0x01)");
			MessageBoxOk("Blockland Glass Update!", "Hey! There's an update for Blockland Glass available! You can go to <a:http://" @ BLG.netAddress @ "/dl.php>" @ BLG.netAddress @ "</a> to pick it up!");

		case "howdy":
			echo("Blockland Glass is operating, apparently");

		case "verify":
			if(getField(%line, 1) $= "password") {
				echo("Account Verification Needed:");
				echo(" + Username:  " @ getField(%line, 2));
				echo(" +     BLID:  " @ getField(%line, 3));

				canvas.pushDialog(BLG_VerifyAccount);
				BLG_Verify_Username.setText(getField(%line, 2));
				BLG_Verify_BLID.setText(getField(%line, 3));
			} else {
				MessageBoxOk("Hey There!", "Welcome to Blockland Glass! It looks like you haven't made an account for Blockland Glass! Visit <a:http://" @ BLG.netAddress @ "/register.php>" @ BLG.netAddress @ "</a> to make an account! No obligations.");
			}

		case "notification":
			error("YOU FUCKING IDIOT. (0x03)");

		case "addon_updates":
			error("YOU FUCKING IDIOT. (0x04)");

		case "auth":
			%status = getField(%line, 1);
			%sid = getField(%line, 2);
			if(%status $= "passed") {
				%this.sid = %sid;
			} else {
				error("YOU FUCKING IDIOT. (0x05)");
				error("BLG Auth Failed");
				%this.retrySch = %this.schedule(5000, pollServer);
			}
	}
}

function BLG_Verify::accept() {
	BLG_Con.sendMessage("verify", "", "confirm");
	canvas.popDialog(BLG_VerifyAccount);
}

function BLG_Verify::decline() {
	BLG_Con.sendMessage("verify", "", "decline");
	canvas.popDialog(BLG_VerifyAccount);
	MessageBoxOk("Well, this is embarassing", "It looks like someone is pretending to be you! The account has been wiped off our site, and you're free to make your own whenever you choose!");
}

function BLG_Con::sendMessage(%this, %call, %retFunc, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6) {
	%tcp = %this.fetchTCP();
	%tcp.call = %call;
	%tcp.callback = %retFunc;
	for(%i = 0; %i < 6; %i++) {
		if(%arg[%i+1] !$= "") {
			%tcp.arg[%i] = %arg[%i+1];
			%tcp.args = %i+1;
		} else {
			break;
		}
	}
	%this.setReady(%tcp);
}

package BLG_NetHack {
	function TCPObject::onConnected(%this) {
		if(%this.BLG) {
			echo("Bitch Ready");
			%str = "call=" @ %this.call @ "&sid=" @ BLG_Con.sid;
			//%this.dump();
			for(%i = 0; %i < %this.args; %i++) {
				%str = %str @ "&arg" @ %i+1 @ "=" @ %this.arg[%i];
			}

			%post = "POST " @ BLG_Con.apiPath @ " HTTP/1.1";
			%post = %post @ "\nHost: " @ BLG.netAddress;
			%post = %post @ "\nUser-Agent: BLG/" @ BLG.version;
			%post = %post @ "\nContent-Length:" SPC strlen(%str);
			%post = %post @ "\nContent-Type: application/x-www-form-urlencoded";
			%post = %post @ "\n\n" @ %str @ "\n";
			echo(%post);
			%this.send(%post);
			%this.header = true;
		} else {
			parent::onConnected(%this);
		}
	}

	function TCPObject::onLine(%this, %line) {
		if(%this.BLG) {
			if(%this.header) {
				if(%line $= "") {
					%this.header = false;
				}
			} else {
				echo(%line);
				%this.buffer = %this.buffer NL %line;
				if(%this.callback !$= "") {
					//echo(%this.callback @ "(\"" @ %line @ "\");");
					eval(%this.callback @ "(\"" @ strreplace(%line, "\"", "\\\"") @ "\");");
				}
			}
		} else {
			parent::onLine(%this, %line);
		}
	}
};
activatePackage(BLG_NetHack);
