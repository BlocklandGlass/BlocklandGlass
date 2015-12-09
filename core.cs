function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "1.1.0-beta.2";
			address = "api.blocklandglass.com";
			netAddress = "blocklandglass.com";
			enableCLI = true;
		};
	}

	if(%context $= "client") {
		Glass::execClient();
	} else {
		Glass::execServer();
	}
}
