function Glass::init(%context) {
	if(!isObject(Glass)) {
		new ScriptObject(Glass) {
			version = "1.1.3";
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

function Glass::devCheck(%obj) {
	//removing people from dev branch
	for(%i = 0; %i < %obj.getCount(); %i++) {
		%hand = %obj.getObject(%i);
		echo(%hand.name);
		if(%hand.name $= "System_BlocklandGlass" && %hand.channel !$= "stable") {
			%hand.channel = "stable";
		}
	}
}

package GlassForceChannel {
	function UpdaterAddonHandlerSG::readLocalFiles(%this) {
		%ret = parent::readLocalFiles(%this);
		Glass::devCheck(%this);
		return %ret;
	}
};
activatePackage(GlassForceChannel);
