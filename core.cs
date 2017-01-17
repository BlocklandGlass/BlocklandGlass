function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "4.0.2";
			address = "api.blocklandglass.com"; //api address
			netAddress = "blocklandglass.com"; //url address
			enableCLI = true;

			liveAddress = "blocklandglass.com";
			livePort = 27002;
		};
	}

	if(isFile("Add-Ons/System_BlocklandGlass/dev/config.json")) {
		exec("./support/jettison.cs");
		%err = jettisonReadFile("Add-Ons/System_BlocklandGlass/dev/config.json");
		if(%err) {
			error("Unable to read dev config");
		} else {
			warn("Using dev config!");
			%config = $JSON::Value;
			Glass.address = %config.address;
			Glass.netAddress = %config.netAddress;

			Glass.liveAddress = %config.liveAddress;
			Glass.livePort = %config.livePort;

			Glass.dev = %config.debug;
		}
	}

	if(isFile("Add-Ons/System_BlocklandGlass/dev/core.cs")) {
		exec("Add-Ons/System_BlocklandGlass/dev/core.cs");
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
