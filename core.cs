function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "2.0.0-alpha.3";
			address = "test.blocklandglass.com";
			netAddress = "test.blocklandglass.com";
			enableCLI = true;
		};

		if($Pref::Player::NetName $= "Jincux") {
			//Glass.address = Glass.netaddress = "localhost";
			Glass.dev = true;
		}
	}

	$Glass::Debug = true;

	if(%context $= "client") {
		Glass::execClient();
	} else {
		Glass::execServer();
	}
}

function JettisonObject::get(%this, %key) {
	return %this.value[%key];
}
