package BLPrefBL_IDPackage {
	function GameConnection::autoAdminCheck(%client)
	{
		// new! Record every BL_ID that has joined ever...
		// This feature was requested by Zeblote.
		if($Pref::Server::bl_idRecords $= "") {
			$Pref::Server::bl_idRecords = 0;
		}
		
		%record = -1;
		
		// is this person's BL_ID already recorded?
		for(%i = 0; %i < $Pref::Server::bl_idRecords; %i++) {
			%bl_id = getFirstWord($Pref::Server::bl_idRecord[%i+1]);
			
			if(%client.bl_id == %bl_id) {
				// append to this record.
				%record = %i+1;
				break;
			}
		}
		
		if(%record == -1) {
			// append to new record
			%record = $Pref::Server::bl_idRecords++;
		}
		
		$Pref::Server::bl_idRecord[%record] = %client.bl_id SPC %client.name;
		
		return Parent::autoAdminCheck(%client);
	}
};
activatePackage(BLPrefBL_IDPackage);

function serverCmdPopulateBL_IDListPlayers(%client) {
	for(%i = 0; %i < $Pref::Server::bl_idRecords; %i++) {
		%field = $Pref::Server::bl_idRecord[%i+1];
		
		%wc = getWordCount(%field);
		
		%bl_id = getWord(%field, 0);
		%name = getWords(%field, 1, %wc-1);
		
		//echo(%field);
		
		commandToClient(%client, 'BL_IDListPlayer', %name, %bl_id);
	}
}