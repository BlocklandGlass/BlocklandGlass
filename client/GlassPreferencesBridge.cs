//================================
// Bridge Blockland Preferences
//================================

$BLPrefs::Version = "0.0.0-alpha+glassbridge.1";

if(!isObject(GlassPrefGroup)) {
	new ScriptGroup(GlassPrefGroup);
}

function GlassPrefGroup::sendPrefs(%this)
	for(%i = 0; %i < %this.getCount(); %i++) {
		%cate = %this.getObject(%i);
		for(%j = 0; %j < %cate.getCount(); %j++) {
			%pref = %cate.getObject(%j);
			if(%pref.actualvalue !$= %pref.value) {
				commandToServer('updateBLPref', %pref.variable, %pref.value);
				%pref.actualvalue = %pref.value;
			}
		}
	}
}

function GlassPrefGroup::requestPrefs(%this) {
	if(!%this.currentCategory) {
		for(%i = 0; %i < %this.getCount(); %i++) {
			%cat = %this.getObject(%i);
			if(!%cat.downloadedPrefs) {
				%this.currentCategory = %cat;
				commandToServer('getBLPrefCategory', %cat.name);
				return;
			}
		}
		GlassServerControlC::renderPrefCategories();
		GlassServerControlC::renderPrefs();
	}
}

function GlassPrefGroup::findByVariable(%var) { // there's gotta be a better way to do this
	for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
		%group = GlassPrefGroup.getObject(%i);
		for(%j = 0; %j < %group.getCount(); %j++) {
			%pso = %group.getObject(%j);
			if(%pso.variable $= %var) {
				return %pso;
			}
		}
	}

	return false;
}

function clientCmdupdateBLPref(%varname, %value) {
	if(%pso = GlassPrefGroup::findByVariable(%varname)) {
		%pso.realvalue = %value;
		%pso.value = %value;

		// TODO something about updating the gui

		if(isobject(%pso.swatch.ctrl)) {
			%pso.swatch.ctrl.setValue(%pso.value);
		}
	}
}

function clientCmdhasPrefSystem(%version, %permission) {
	if($Glass::Debug)
		echo("Server has pref system! (" @ %version @")");

	if(%permission) {
		commandToServer('GetBLPrefCategories');
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
		actualvalue = %value;
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
