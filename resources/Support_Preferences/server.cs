// ---=== Blockland Preference System ===---
// -- Contributors:
//    -- TheBlackParrot (BL_ID 18701)
//    -- Jincux (BL_ID 9789)
// 	  -- Chrisbot6 (BL_ID 12233)

if($BLPrefs::didPreload && !$BLPrefs::Debug) {
	echo("Preferences already preloaded, nothing to do here.");
	return;
} else if(!$BLPrefs::PreLoad) {
	warn("Preloader wasn't installed. Some prefs may not be available.");
} else if($BLPrefs::Debug) {
	warn("Re-executing, development mode");
}

if(!isObject(PreferenceContainerGroup)) {
	new ScriptGroup(PreferenceContainerGroup);
}

$Pref::BLPrefs::ServerDebug = true;
%Pref::BLPrefs::iconDefault = "wrench";
$BLPrefs::Version = "1.0.2-beta";

exec("./support/admin.cs");
exec("./support/lesseval.cs");

exec("./server/functions.cs");
exec("./server/compatibility.cs");
exec("./server/handshake.cs");
exec("./server/interaction.cs");
exec("./server/userdata.cs");

if($Pref::PreLoadScriptLauncherVersion < 1) {
	fileCopy("./support/preloader.cs", "config/main.cs");
}

function registerPref(%addon, %dev, %title, %type, %variable, %default, %params, %callback, %legacy, %isSecret, %isHostOnly)
{
	// %leagacy = 1 if it's added via a compatibility wrapper

	if(%dev $= "") {
		%dev = "General";
	}

	// shorthand types
	switch$(%type) {
		case "boolean" or "tf":
			%type = "bool";

		case "number" or "real" or "intiger" or "int":
			%type = "num";

		case "numplayers":
			%type = "playercount";

		case "str" or "mlstring":
			%type = "string";

		case "slide" or "range" or "float":
			%type = "slider";

		case "choice" or "choices" or "list":
			%type = "dropdown";

		case "delimited":
			%type = "wordlist";

		case "users" or "bl_idlist" or "adminlist":
			%type = "userlist";

		case "colour":
			%type = "color";

		case "data":
			%type = "datablock";

		case "datalist" or "delimiteddata":
			%type = "datablocklist";

		case "vec":
			%type = "vector";

		case "call" or "function" or "push" or "callbackButton":
			%type = "button";
	}

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

	%valid = ":playercount:wordlist:datablocklist:userlist:datablock:slider:num:bool:button:dropdown:string:rgb:colorset";

	if(stripos(%valid, ":" @ %type) == -1)
	{
		warn("Invalid pref type:" SPC %type);
		return;
	}

	%groupName = BLP_alNum(%addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(BlocklandPrefGroup) {
			class = "PreferenceGroup";
			title = BLP_alNum(%addon);
			legacy = %legacy;
			category = %addon;
			icon = $Pref::BlPrefs::iconDefault;
		};
	} else {
		%group = (%groupName).getID();
	}

	if(%legacy)
		%group.icon = "bricks";

	for(%i=0;%i<%group.getCount();%i++) {
		if(%variable $= %group.getObject(%i).variable) {
			echo("\c4" @ %variable SPC "has already been registered, skipping...");
			return;
		}
	}

	%pref = new scriptObject(BlocklandPrefSO)
	{
		class = "Preference";
		category = %addon;
		title = %title;
		defaultValue = %default;
		variable = %variable;
		type = %type;
		callback = %callback;
		params = %params;
		legacy = %legacy;
		devision = %dev;
		secret = %isSecret;
		hostOnly = %isHostOnly;
	};
	%group.add(%pref);

	// use this for server-sided validation?
	switch$(%type)
	{
		case "num":
			%pref.minValue = getWord(%params, 0);
			%pref.maxValue = getWord(%params, 1);
			%pref.decimalPoints = getWord(%params, 2);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

		case "playercount":
			%pref.minValue = getWord(%params, 0);
			%pref.maxValue = getWord(%params, 1);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

			%pref.defaultValue = mFloor(%pref.defaultValue);

		case "string":
			%pref.maxLength = getWord(%params, 0);
			%pref.stripML = getWord(%params, 1);

			if(strlen(%pref.defaultValue) > %pref.maxLength)
			{
				%pref.defaultValue = getSubStr(%pref.defaultValue, 0, %pref.maxLength);
			}

		case "slider":
			%pref.minValue = getWord(%params, 0);
			%pref.maxValue = getWord(%params, 1);
			%pref.snapTo = getWord(%params, 2);
			%pref.stepValue = getWord(%params, 3);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

		case "bool":
			if(%pref.defaultValue > 1)
				%pref.defaultValue = 1;

			if(%pref.defaultValue < 0)
				%pref.defaultValue = 0;

		case "dropdown":
			// using the ol rtb list format
			%count = getWordCount(%params) / 2;

			for(%i = 0; %i < %count; %i++) {
				%first = (%i * 2);

				%pref.rowName[%count] = strreplace(getWord(%params, %first), "_", " ");
				%pref.rowValue[%count] = getWord(%params, %first+1);

				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];
			}

			%pref.listCount = %count;

		case "datablock":
			%pref.dataType = getWord(%params, 0);
			%pref.canBeNone = getWord(%params, 1);

			if(!isObject(%pref.defaultValue)) {
				%pref.defaultValue = -1;
				%pref.canBeNone = true;
			}
			else {
				if((%pref.defaultValue).getClassName() != %pref.dataType) {
					%pref.defaultValue = -1;
					%pref.canBeNone = true; // actually make it the first possible datablock in future rather than forcing "NONE" option and setting it
				}
			}

			// populate the pref object with all possible data values
			// IMPORTANT: when setting the actual global, MAKE SURE YOU USE THE DATABLOCK NAME. In all other situations, use its ID.
			%count = 0;

			if(%pref.canBeNone) {
				// add "NONE" option
				%pref.rowName[%count] = "NONE";
				%pref.rowValue[%count] = -1;

				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];

				%count++;
			}

			for(%i = 0; %i < DataBlockGroup.getCount(); %i++) {
				%data = DataBlockGroup.getObject(%i);

				if(%data.getClassName() != %pref.dataType)
					continue;

				%pref.rowName[%count] = %data.uiName !$= "" ? %data.uiName : %data.getName();
				%pref.rowValue[%count] = %data.getId();

				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];

				%count++;
			}

		case "wordlist":
			%pref.delimiter = strreplace(getWord(%params, 0), "_", " ");
			%pref.maxWords = getWord(%params, 1);

			%prefsAsFields = strreplace(%pref.defaultValue, %pref.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %pref.maxWords && %pref.maxWords != -1) {
				%prefsAsFields = getFields(%pref.defaultValue, %pref.maxWords);
			}

			%pref.defaultValue = strreplace(%prefsAsFields, "" TAB "", %pref.delimiter);

		case "userlist":
			%pref.delimiter = strreplace(getWord(%params, 0), "_", " ");
			%pref.maxWords = getWord(%params, 1);

			%prefsAsFields = strreplace(%pref.defaultValue, %pref.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %pref.maxWords && %pref.maxWords != -1) {
				%prefsAsFields = getFields(%pref.defaultValue, %pref.maxWords);
			}

			// make sure EVERY field is a valid number.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%prefsAsFields = setField(%prefsAsFields, %i, mFloor(getField(%prefsAsFields, %i)));
			}

			%pref.defaultValue = strreplace(%prefsAsFields, "" TAB "", %pref.delimiter);

		case "datablocklist":
			%pref.dataType = getWord(%params, 0);
			%pref.delimiter = strreplace(getWord(%params, 1), "_", " ");
			%pref.maxWords = getWord(%params, 2);

			%prefsAsFields = strreplace(%pref.defaultValue, %pref.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %pref.maxWords && %pref.maxWords != -1) {
				%prefsAsFields = getFields(%pref.defaultValue, %pref.maxWords);
			}

			// make sure EVERY field is a valid datablock.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%data = getField(%prefsAsFields, %i);

				%validated = false;

				if(isObject(%data)) {
					if((%data).getClassName() == %pref.dataType) {
						%validated = true;
					}
				}

				if(!%validated) {
					%prefsAsFields = setField(%prefsAsFields, %i, -1);
				}
			}

			%pref.defaultValue = strreplace(%prefsAsFields, "" TAB "", %pref.delimiter);
	}

	return %pref;
}

function registerPrefGroupIcon(%addon, %icon) {
	%groupName = BLP_alNum(%addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(BlocklandPrefGroup) {
			class = "PreferenceGroup";
			title = BLP_alNum(%addon);
			legacy = %legacy;
			category = %addon;
			icon = $Pref::BlPrefs::iconDefault;
		};
	} else {
		%group = (%groupName).getID();
	}

	%group.icon = %icon; // change icon with this function, so they're per category only now
}

function BlocklandPrefSO::onAdd(%obj)
{
	%obj.setName("");
}

function BlocklandPrefSO::getValue(%this) {
	return getGlobalByName(%this.variable); //eval("return " @ %this.variable @ ";");
}

function BlocklandPrefSO::updateValue(%this, %value, %updater) {
	// we need some way to validate the values on this end of things
	//%updater - client that updated value.
	if(isObject(%updater)) {
		%updaterClean = %updater.getId();
	} else {
		%updaterClean = 0;
	}

	// when storing datablocks, use their NAME.
	if(%this.type $= "datablock")
		setGlobalByName(%this.variable, (%value).getName());
	else {
		setGlobalByName(%this.variable, %value);
	}

	if(%this.callback !$= "") {
		// callback(value, client, pref object);
		// callbacks can now only be function names and always get called with the same value set
		call(%this.callback, %value, %updaterClean, %this);
	}
}

function BlocklandPrefSO::validateValue(%this, %value) {
	echo("validating" SPC %value SPC "(" @ %this @ ")");

	// this is where the SO's come in handy
	switch$(%this.type) {
		case "num":
			if(%this.decimalPoints !$= "") {
				%value = mFloatLength(%value, %this.decimalPoints);
			}

			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

		case "string":
			if(strlen(%value) > %this.maxLength) {
				%value = getSubStr(%value, 0, %this.maxLength);
			}
			if(%this.stripML) {
				%value = stripMLControlChars(%value); // sure we couldn't have a MLString type? Yes.
			}

		case "slider":
			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

			%value -= (%value % %this.snapTo);

		case "bool":
			if(%value > 1)
				%value = 1;

			if(%value < 0)
				%value = 0;

		case "dropdown":
			if(%this.valueName[%value] $= "") {
				%value = %this.rowValue[0]; // hacky, but it should work
			}

		case "datablock":
			if(%this.valueName[%value] $= "") {
				%value = %this.rowValue[0];
			}

		case "wordlist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

		case "userlist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB "");

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			// make sure EVERY field is a valid number.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%prefsAsFields = setField(%prefsAsFields, %i, mFloor(getField(%prefsAsFields, %i)));
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

		case "datablocklist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB "" && %this.maxWords != -1);

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			// make sure EVERY field is a valid datablock.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%data = getField(%prefsAsFields, %i);

				%validated = false;

				if(isObject(%data)) {
					if((%data).getClassName() == %this.dataType) {
						%validated = true;
					}
				}

				if(!%validated) {
					%prefsAsFields = setField(%prefsAsFields, %i, -1);
				}
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);
	}
	return %value;
}

function BlocklandPrefSO::findByVariable(%var) { // there's gotta be a better way to do this
	for(%i = 0; %i < PreferenceContainerGroup.getCount(); %i++) {
		%group = PreferenceContainerGroup.getObject(%i);

		for(%j = 0; %j < %group.getCount(); %j++) {
			%pso = %group.getObject(%j);
			if(%pso.variable $= %var) {
				return %pso;
			}
		}
	}

	return false;
}

function BlocklandPrefGroup::onAdd(%this) {
	%this.setName(%this.title @ "Prefs");
	PreferenceContainerGroup.add(%this);
}

// a wrapper to execute everything in the prefs folder
// will be used for older addons without prefs, if asked for them
if(!$BLPrefs::AddedServerSettings) {
	%file = findFirstFile("./server/prefs/*.cs");

  while(%file !$= "")	{
  	exec(%file);
	  %file = findNextFile("./server/prefs/*.cs");
	}
}

$BLPrefs::Init = true;
