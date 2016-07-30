// prepend every function so we're not confusing anyone
// these aren't default functions

// i was considering just creating a dummy scriptobject to store these functions in so they appear on a dump of the object
// but you could just, y'know, look here

// Chris's todo: move my shit in here :c

function BLP_alNum(%str) {
	if(%str $= "") {
		return "";
	}

	// if there's a better way to strip a string of every non-alphanumeric character, do replace this
	%chars = "!@#$%^&*();'\"[]{},./<>?|\\-=_+` ";
	for(%i=0;%i<strLen(%chars);%i++) {
		%str = strReplace(%str, getSubStr(%chars, %i, 1), "");
	}

	return %str;
}

function GameConnection::BLP_isAllowedUse(%this) {
	switch($Pref::BLPrefs::AllowedRank) {
		case 3:
			if(%this.bl_id == 999999 || %this.bl_id == getNumKeyID()) {
				return 1;
			}

		case 2:
			if(%this.isSuperAdmin) {
				return 1;
			}

		case 1:
			if(%this.isAdmin) {
				return 1;
			}
	}
	return 0;
}

function getFirstWord(%str) {
	return getWord(%str, 0);
}
