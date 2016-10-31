function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "3.2.0-alpha+indev";
			address = "api.blocklandglass.com";
			netAddress = "blocklandglass.com";
			enableCLI = true;

			liveAddress = "test.blocklandglass.com";
			livePort = 27005; //27005 is dev, 27002 is stable/public
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
