//================================
// Bridge Blockland Preferences
//================================

$BLPrefs::Version = "1.0.0+glassbridge.2";

if(!isObject(GlassPrefGroup)) {
	new ScriptGroup(GlassPrefGroup);
}

function GlassPrefGroup::cleanup() {
	for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
		%cat = GlassPrefGroup.getObject(%i);
		%cat.deleteAll();
		%cat.delete();
	}
	GlassPrefGroup.delete();
	new ScriptGroup(GlassPrefGroup);
}

function GlassPrefGroup::sendChangedPrefs(%this) {
	for(%i = 0; %i < %this.getCount(); %i++) {
		%cate = %this.getObject(%i);
		for(%j = 0; %j < %cate.getCount(); %j++) {
			%pref = %cate.getObject(%j);
			if(%pref.localValue !$= %pref.value) {
				%up = true;
				commandToServer('UpdatePref', %pref.variable, %pref.value);
				%pref.actualvalue = %pref.value;
			}
		}
	}

	if(%up) {
		messageBoxOk("Settings Updated", "The server settings were updated");
	} else {
		messageBoxOk("No Changes!", "There was nothing to update!");
	}
}

function GlassPrefBridge::requestPreferences() {
	GlassServerControlC.requestedPrefs = true;
	commandToServer('RequestPrefCategories');
}



function GlassPrefGroup::requestPrefs(%this) {
	if(!%this.currentCategory) {
		for(%i = 0; %i < %this.getCount(); %i++) {
			%cat = %this.getObject(%i);
			if(!%cat.downloadedPrefs) {
				%this.currentCategory = %cat;
				commandToServer('RequestCategoryPrefs', %cat.id);
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
	echo("Updating " @ %varname @ " to " @ %value);
	if(%pso = GlassPrefGroup::findByVariable(%varname)) {
		%pso.value = %value;
		%pso.localValue = %value;

		// TODO something about updating the gui
		if(isobject(%pso.swatch.ctrl)) {
			if(%pso.type !$= "list") {
				%pso.swatch.ctrl.setValue(%pso.value);
			} else {
				%pso.swatch.ctrl.setSelected(%pso.value);
			}
		}
	}
}

function clientCmdhasPrefSystem(%version, %permission) {
	if($Glass::Debug)
		echo("Server has pref system! (" @ %version @")");

	if(%permission) {
		GlassPrefGroup.requested = true;
		commandToServer('GetBLPrefCategories');
	}
}

function clientCmdReceiveCategory(%id, %category, %icon, %last) {
	echo(%id TAB %category TAB %icon);
	%obj = new ScriptGroup() {
		class = "GlassPrefCategory";

		name = %category;
		icon = %icon;

		id = %id;

		downloadedPrefs = false;
	};
	GlassPrefGroup.add(%obj);

	GlassPrefGroup.requestPrefs();
}

function clientCmdReceivePref(%catId, %id, %title, %subcategory, %type, %params, %default, %returnName, %value, %last) {
	echo("Received Pref");
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

	echo(%last);
	if(%last) {
		GlassPrefGroup.currentCategory.downloadedPrefs = true;
		GlassPrefGroup.currentCategory = "";
		GlassPrefGroup.requestPrefs();
	}
}

package GlassPrefPackage {
	function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
		GlassPrefGroup::cleanup();
		return parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Prefs" TAB $BLPrefs::Version NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
	}
};
activatePackage(GlassPrefPackage);
