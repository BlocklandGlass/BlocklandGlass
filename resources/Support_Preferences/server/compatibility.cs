$RTB::RTBR_ServerControl_Hook = true; // RTB is totally on guys

if(isFile("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs"))
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
$RTB::Hooks::ServerControl = true; // RTB is totally on guys

if(isFile("Add-Ons/System_oRBs/hooks/serverControl.cs"))
	exec("Add-Ons/System_oRBs/hooks/serverControl.cs");
$ORBS::Hooks::ServerControl = true; // yup oRBs too

package BLPrefCompatibilityPackage {
	function RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
		if(isFunction("RTB_registerPref")) {
			parent::RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);
		}

		%type = getWord(%params, 0);

		// do some wizardry to sort certain legacy addons better
		%addon = strReplace(%addon, "|", "" TAB "");
		%addon = strReplace(%addon, "-", "" TAB "");
		%addon = strReplace(%addon, ":", "" TAB "");
		%addon = strReplace(%addon, ";", "" TAB "");

		%cat = getField(%addon, 0);
		%sub = getFields(%addon, 1);

		// remove space from the start of %sub
		if(strpos(%sub, " ") == 0) {
			%sub = getSubStr(%sub, 1, strlen(%sub) - 1);
		}

		if(%sub $= "") {
			%sub = "General";
		}

		// type checks moved to server.cs, considering them shorthand
		// some prefs have "$", some don't
		registerPref(%cat, %sub, %name, %type, "$" @ strReplace(%variable, "$", ""), %default, getWords(%params, 1), %callback, 1);
	}

	// because oRBs is the same thing at this point -_-
	// so much for "innovation", amirite?
	function oRBs_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
		RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);

		if(isFunction("oRBs_registerPref")) {
			parent::oRBs_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);
		}
	}
};
activatePackage(BLPrefCompatibilityPackage);
