//================================
// Bridge Blockland Preferences
//================================

// valid pref types:
// - playercount [min] [max] (RTB's convenience list type) #
// - wordlist [delim] [max]
// - datablocklist [type] [delim] [max]
// - userlist [delim] [max]
// - datablock [type] [hasNoneOption] #
// - slider [min] [max] [snapTo] [stepValue] #
// - num [min] [max] [decimalpoints] #
// - bool #
// - button #
// - dropdown [item1Name] [item1Var] [item2Name] [item2Var] etc #
// - string [charLimit] [stripML] #
// - colorset #
// - rgb [form] # form = hex, integer

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
	GlassServerControlC::setTab(2);
}

function GlassPrefGroup::doneLoading() {
	GlassServerControlC.loaded = true;
	GlassServerControlC::renderAll();
}

function GlassPrefGroup::sendChangedPrefs(%this) {
	for(%i = 0; %i < %this.getCount(); %i++) {
		%cate = %this.getObject(%i);
		for(%j = 0; %j < %cate.getCount(); %j++) {
			%pref = %cate.getObject(%j);
			if(%pref.localValue !$= %pref.value) {
				%up = true;
				commandToServer('UpdatePref', %pref.variable, %pref.localValue);
				%pref.value = %pref.localValue;
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
	GlassServerControlC.requested = true;
	commandToServer('RequestPrefCategories');
}

function GlassPrefCategory::requestPrefs(%this) {
	commandToServer('RequestCategoryPrefs', %this.id);
}

function GlassPrefGroup::requestPrefs(%this) {
	for(%i = 0; %i < %this.getCount(); %i++) {
		%cat = %this.getObject(%i);
		if(!%cat.downloadedPrefs) {
			commandToServer('RequestCategoryPrefs', %cat.id);
			return;
		}
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
		%pso.value = %value;
		%pso.localValue = %value;

		// TODO something about updating the gui
		if(isobject(%pso.swatch.ctrl)) {
			if(%pso.type !$= "list") {
				%pso.swatch.ctrl.setValue(expandEscape(%pso.value));
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
		GlassServerControlC.setEnabled(true);
	}
}

function clientCmdReceiveCategory(%id, %category, %icon, %last) {
	GlassServerControlC.receivedPrefs = true;
	Glass::debug(%id TAB %category TAB %icon);
	%obj = new ScriptGroup() {
		class = "GlassPrefCategory";

		name = %category;
		icon = %icon;

		id = %id;

		downloadedPrefs = false;
	};
	GlassPrefGroup.add(%obj);
	GlassPrefGroup.cat[%id] = %obj;
	%obj.requestPrefs();
}

function clientCmdReceivePref(%catId, %id, %title, %subcategory, %type, %params, %default, %variable, %value, %last) {
	%obj = new ScriptObject() {
		class = "GlassPrefInfo";

		title = %title;
		type = %type;
		variable = %variable;
		value = %value;
		localValue = %value;
		params = %params;
		subcategory = %subcategory;

		legacy = %legacy;
	};
	GlassPrefGroup.cat[%catId].add(%obj);

	if(%last) {
		GlassPrefGroup.cat[%catId].downloadedPrefs = true;
		GlassPrefGroup.downloaded++;
		if(GlassPrefGroup.downloaded >= GlassPrefGroup.getCount()) {
			GlassPrefGroup::doneLoading();
		}
	}
}

package GlassPrefPackage {
	function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
		GlassPrefGroup::cleanup();
		return parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Prefs" TAB $BLPrefs::Version NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
	}
};
activatePackage(GlassPrefPackage);
