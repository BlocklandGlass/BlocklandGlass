function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "3.1.4";
			address = "api.blocklandglass.com";
			netAddress = "blocklandglass.com";
			enableCLI = true;

			liveAddress = "test.blocklandglass.com";
			livePort = 27005; //27005 is dev
		};

		//enable debug mode for me
		//if($Pref::Player::NetName $= "Jincux" || $Pref::Player::NetName $= "BLG") {
			//Glass.dev = true;
			//Glass.devLocal = false;
			//Glass.address = "localhost";
		//}
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
