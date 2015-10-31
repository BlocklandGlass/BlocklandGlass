//================================
// Bridge Blockland Preferences
//================================

$BLPrefs::Version = "0.0.0-alpha+glassbridge.1";

if(!isObject(GlassPrefGroup)) {
	new ScriptGroup(GlassPrefGroup);
}

function GlassPrefGroup::requestPrefs(%this) {
	if(!%this.currentCategory) {
		for(%i = 0; %i < %this.getCount(); %i++) {
			%cat = %this.getObject(%i);
			if(!%cat.downloadedPrefs) {
				%this.currentCategory = %cat;
				commandToServer('getBLPrefCategory', %cat.name);
			}
		}
	}
}

function clientCmdAddCategory(%category, %icon) {
	%obj = new ScriptGroup() {
		class = "GlassPrefCategory";

		name = %category;
		icon = %icon;

		downloadedPrefs = false;
	};
	GlassPrefGroup.add(%obj);
	GlassPrefGroup.requestPrefs();
}

function clientCmdReceivePref(%title, %type, %variable, %value, %params, %legacy) {
	%obj = new ScriptObject() {
		class = "GlassPrefInfo";

		title = %title;
		type = %type;
		variable = %variable;
		value = %value;
		params = %params;

		legacy = %legacy;
	};
	GlassPrefGroup.currentCategory.add(%obj);
}

function clientCmdfinishReceivePref() {
	if($Glass::Debug)
		echo("Downloaded category " @ GlassPrefGroup.currentCategory.name);
	GlassPrefGroup.currentCategory.downloadedPrefs = true;
	GlassPrefGroup.currentCategory = "";
	GlassPrefGroup.requestPrefs();
}

package GlassPrefPackage {
	function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
		return parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Prefs" TAB $BLPrefs::Version NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
	}
};
activatePackage(GlassPrefPackage);
