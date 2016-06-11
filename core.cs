function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "2.0.0-alpha.0.0.0+prealpha";
			//address = "test.blocklandglass.com";
			//netAddress = "test.blocklandglass.com";
			address = "localhost";
			netAddress = "localhost";
			enableCLI = true;
		};

		if($Pref::Player::NetName $= "Jincux") {
			Glass.address = Glass.netaddress = "localhost";
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
