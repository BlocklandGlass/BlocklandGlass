function GameConnection::checkAdminStatus(%client) {
	%bl_id = %client.bl_id;
	
	if(%bl_id == getNumKeyId() || %bl_id == 999999) {
		return 2; // host
	}
	
	// search super admin list
	for(%i = 0; %i < getWordCount($Pref::Server::AutoSuperAdminList); %i++) {
		%id = getWord($Pref::Server::AutoSuperAdminList, %i);
		
		if(%id == %bl_id) {
			return 2; // super admin
		}
	}
	
	// search admin list
	for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++) {
		%id = getWord($Pref::Server::AutoAdminList, %i);
		
		if(%id == %bl_id) {
			return 1; // admin
		}
	}
	
	// not found anywhere, so...
	return 0; // pleb
}