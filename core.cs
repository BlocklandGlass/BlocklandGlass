function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "2.0.0-alpha.0.0.0+indev";
			address = "test.blocklandglass.com";
			netAddress = "test.blocklandglass.com";
			enableCLI = true;
		};
	}

	$Glass::Debug = true;

	if(%context $= "client") {
		Glass::execClient();
	} else {
		Glass::execServer();
	}
}
