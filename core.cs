function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "2.0.1+indev";
			address = "blocklandglass.com";
			netAddress = "blocklandglass.com";
			enableCLI = true;
		};

		if($Pref::Player::NetName $= "Jincux" || $Pref::Player::NetName $= "BLG") {
			Glass.dev = true;
			Glass.devLocal = true;
			Glass.address = "localhost";
		}
	}

	//$Glass::Debug = true;

	if(%context $= "client") {
		Glass::execClient();
	} else {
		Glass::execServer();
	}
}

function Glass::debug(%text) {
	if(Glass.dev) {
		echo(%text);
	}
}

function JettisonObject::get(%this, %key) {
	return %this.value[%key];
}
