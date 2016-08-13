// client communication commands are gone for now
// will rewrite them when ready - chris
function serverCmdRequestPrefCategories(%client) {
	%group = PreferenceContainerGroup;
	if(!isObject(%group)) {
		echo("\c4" @ %client.name SPC "requested preferences, but the container group doesn't exist. This shouldn't be happening.");
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	// the groups made things SO MUCH SIMPLER
	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4Sending" SPC %row.category @ "...");
		}
		commandToClient(%client, 'ReceiveCategory', %i, %row.category, %row.icon, (%group.getCount()-1 == %i));
	}
}

function serverCmdRequestCategoryPrefs(%client, %anID, %failsafe) {
	%group = PreferenceContainerGroup.getObject(%anID);

	if(%failsafe >= 2)
		return; // this should never happen

	if(!isObject(%group)) {
		echo("\c4" @ %client.name SPC "requested preferences, but the container group doesn't exist. This could be happening due to invalid requests or client bugs.");
		serverCmdRequestCategoryPrefs(%client, 0, %failsafe+1);
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	// the groups made things SO MUCH SIMPLER
	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		if($Pref::BLPrefs::ServerDebug) {
			//echo("\c4Sending" SPC %row.title @ "...");
		}
		commandToClient(%client, 'ReceivePref', %anID, %i, %row.title, %row.devision, %row.type, %row.params, %row.defaultValue, %row.variable, %row.getValue(), (%group.getCount()-1 == %i));
	}
}

function serverCmdUpdatePref(%client, %varname, %newvalue, %announce) {
	//validate!
	if(!%client.BLP_isAllowedUse())
		return;

	//we need to find the object
	%pso = BlocklandPrefSO::findByVariable(%varname);
	if(%pso) {
		if(getSimTime() - %client.lastChange >= 100) {
			messageAll('MsgAdminForce', "\c3" @ %client.name SPC "\c6updated the \c3" @ %pso.category @ "\c6 prefs.");
		}

		%client.lastChange = getSimTime();

		if(%pso.hostOnly) {
			if(%client.bl_id != getNumKeyId() && %client.bl_id != 999999)
				return;
		}

		%newvalue = %pso.validateValue(%newvalue);
		%pso.updateValue(%newvalue, %client);

		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4" @ %client.name @ " (BL_ID: " @ %client.bl_id @ ") set " @ %varname @ " to " @ %newvalue);
		}

		if(%announce) {
			if(%pso.type $= "dropdown" || %pso.type $= "datablock") {
				%displayValue = %pso.valueName[%newvalue];
			} else {
				%displayValue = expandEscape(%newvalue);
			}

			if(!%pso.secret)
				messageAll('', "\c6 + \c3" @ %pso.title SPC "\c6is now\c3" SPC %displayValue);
			else
				messageAll('', "\c6 + \c3" @ %pso.title SPC "\c6was changed.");
		}

		for(%i = 0; %i < ClientGroup.getCount(); %i++) {
			%cl = ClientGroup.getObject(%i);
			if(%cl.hasPrefSystem && %cl.isAdmin) {
				commandToClient(%cl, 'updateBLPref', %varname, %newvalue);
			}
		}
		
		saveBLPreferences();
	} else {
		//so they tried to update a variable that doesn't exist...
		warn("Variable \"" @ %varname @ "\" doesn't exist!");
	}
}
