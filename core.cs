function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "3.2.0";
			address = "api.blocklandglass.com";
			netAddress = "blocklandglass.com";
			enableCLI = true;

			liveAddress = "blocklandglass.com";
			livePort = 27002;
		};
	}

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
