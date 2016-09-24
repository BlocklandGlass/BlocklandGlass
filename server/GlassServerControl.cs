if($Glass::Modules::Prefs)
  return;

$Glass::Modules::Prefs = true;
//====================================
// Admin
//====================================

function updateGlassPref(%value, %client, %pso) {
  if(%pso.title $= "Who can manage super-admins?") {
    GlassSettings.update("SC::SAEditRank", %value);
  } else if(%pso.title $= "Who can manage admins?") {
    GlassSettings.update("SC::AEditRank", %value);
  } else if(%pso.title $= "Required Client Add-Ons") {
    GlassSettings.update("SC::RequiredClients", %value);
    %value = strreplace(%value, ",", "\t");
    messageAll('MsgAdminForce', "\c6 + \c3" @ %client.netname @ "\c6 has updated the required mods.");
    for(%i = 0; %i < getFieldCount(%value); %i++) {
      %mid = trim(getField(%value, %i));
      messageAll('', "\c6 ++ Required: \c3" @ GlassSettings.cacheFetch("AddonName_" @ %mid));
    }

  }
}

function serverCmdglassNameCacheAdd(%client, %id, %name) {
  if(%client.isSuperAdmin)
    GlassSettings.cachePut("AddonName_" @ %id, %name);
}

function GameConnection::checkPermissionLevel(%this, %perm) {
  if(%perm == 3) {
    return %this.bl_id == getNumKeyId() || %this.bl_id == 999999;
  } else if(%perm == 2) {
    return (%this.bl_id == getNumKeyId() || %this.isSuperAdmin || %this.bl_id == 999999);
  } else if(%perm == 1) {
    return (%this.bl_id == getNumKeyId() || %this.isSuperAdmin || %this.isAdmin || %this.bl_id == 999999);
  }
}

function removeItemFromList(%list, %item) {
  for(%i = 0; %i < getWordCount(%list); %i++) {
    %id = getWord(trim(%list), %i);
    if(%id !$= %item) {
      %newList = %newList SPC %id;
    }
  }

  return trim(%newList);
}

function addItemToList(%list, %item) {
  return trim(%list SPC %item);
}

function removeItemFromArray(%list, %item) {
  for(%i = 0; %i < getFieldCount(%list); %i++) {
    %id = getField(trim(%list), %i);
    if(%id !$= %item) {
      %newList = %newList TAB %id;
    }
  }

  return trim(%newList);
}

function addItemToArray(%list, %item) {
  return trim(%list TAB %item);
}

function getAdminLevelFromBLID(%blid) {
  for(%i = 0; %i < getWordCount($Pref::Server::AutoSuperAdminList); %i++) {
    %id = getWord($Pref::Server::AutoSuperAdminList, %i);
    if(%id == %blid) {
      return 2;
    }
  }

  for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++) {
    %id = getWord($Pref::Server::AutoAdminList, %i);
    if(%id == %blid) {
      return 1;
    }
  }

  return false;
}

function serverCmdGlassSetAdmin(%client, %blid, %rank, %auto) {
  if(%blid $= "") {
    return;
  }
  
  if(%blid == getNumKeyID()) {
    return; //host
  }
  
  if(%client.isSuperAdmin || %client.getBLID() == getNumKeyID()) {
    if(%rank > 0) {
      GlassServerControlS::setAdmin(%blid, %rank, %auto);
    } else {
      GlassServerControlS::removeAdmin(%blid, %auto);
    }

    for(%i = 0; %i < ClientGroup.getCount(); %i++) {
      %cl = ClientGroup.getObject(%i);
      if(%cl.isAdmin) {
        GlassServerControlS::sendAdminData(%cl);
      }
    }
  }
}

function serverCmdGetGlassUsers(%client) {
  %users = 0;
  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);
    if(%cl.hasGlass) {
      %users++;
    }
  }
  messageClient(%client, '', %users);
}

function GlassServerControlS::setAdmin(%blid, %rank, %auto) {
  if(%blid == getNumKeyID()) {
    return;
  }
  
  if(%rank <= 0) {
    GlassServerControlS::removeAdmin(%blid, %auto);
    return;
  }
  
  if(isObject(%client = findClientByBL_ID(%blid))) {
    %name = %client.name;
  } else {
    %name = "BLID_" @ %blid;
  }
  
  if(%client.isSuperAdmin && %rank == 2) {
    return;
  } else if((%client.isAdmin && !%client.isSuperAdmin) && %rank == 1) {
    return;
  }
  
  if(%auto) {
    $Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList, %blid);
    $Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList, %blid);
  }
  
  %type = %auto ? "Auto" : "Manual";
  
  if(%rank == 2) {
    if(%auto) {
      $Pref::Server::AutoSuperAdminList = addItemToList($Pref::Server::AutoSuperAdminList, %blid);
    }
    
    messageAll('MsgAdminForce', '\c2%1 is now Super Admin (%2)', %name, %type);
    
    if(isObject(%client)) {
      %client.isAdmin = true;
      %client.isSuperAdmin = true;
    }
  } else if(%rank == 1) {
    if(%auto) {
      $Pref::Server::AutoAdminList = addItemToList($Pref::Server::AutoAdminList, %blid);
    }
    
    messageAll('MsgAdminForce', '\c2%1 is now Admin (%2)', %name, %type);
    
    if(isObject(%client)) {
      %client.isAdmin = true;
      %client.isSuperAdmin = false;
    }
  }

  if(isObject(%client)) {
    %client.sendPlayerListUpdate();
    
    if(%rank == 2) {
      commandToClient(%client, 'setAdminLevel', 2);
    } else if(%rank == 1) {
      commandToClient(%client, 'setAdminLevel', 1);
    }
    
    commandToClient(%client, 'hasPrefSystem', $BLPrefs::Version, %client.BLP_isAllowedUse());
  }
}

function GlassServerControlS::sendAdminData(%cl) {
  %buffer = "";
  
  $Pref::Server::AutoSuperAdminList = trim($Pref::Server::AutoSuperAdminList);
  
  for(%i = 0; %i < getWordCount($Pref::Server::AutoSuperAdminList); %i++) {
    %id = getWord($Pref::Server::AutoSuperAdminList, %i);
    if(isObject(%client = findClientByBL_ID(%id))) {
      %name = %client.name;
    } else {
      %name = "BLID_" @ %id;
    }
    
    %buffer = %buffer @ %name TAB %id TAB "S\n";
    
  }
  
  commandToClient(%cl, 'GlassAdminListing', trim(%buffer));

  %buffer = "";
  
  $Pref::Server::AutoAdminList = trim($Pref::Server::AutoAdminList);
  
  for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++) {
    %id = getWord($Pref::Server::AutoAdminList, %i);
    if(isObject(%client = findClientByBL_ID(%id))) {
      %name = %client.name;
    } else {
      %name = "BLID_" @ %id;
    }

    %buffer = %buffer @ %name TAB %id TAB "A\n";
  }
  
  if(%buffer !$= "") {
    commandToClient(%cl, 'GlassAdminListing', trim(%buffer), true);
  }
}

function GlassServerControlS::removeAdmin(%blid, %auto) {
  if(%auto) {
    $Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList, %blid);
    $Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList, %blid);
  }

  if(isObject(%client = findClientByBL_ID(%blid))) {
    %name = %client.name;
  } else {
    %name = "BLID_" @ %blid;
  }
  
  %type = %auto ? "Auto" : "Manual";
  
  if(%client.isSuperAdmin) {
    messageAll('MsgAdminForce', '\c2%1 is no longer Super Admin (%2)', %name, %type);
  } else if(%client.isAdmin) {
    messageAll('MsgAdminForce', '\c2%1 is no longer Admin (%2)', %name, %type);
  } else {
    if(isObject(%client) && !%client.isSuperAdmin && !%client.isAdmin) {
      return;
    }
    messageAll('MsgAdminForce', '\c2%1 is no longer Admin (%2)', %name, %type);
  }
  
  if(isObject(%client)) {
    %client.isAdmin = false;
    %client.isSuperAdmin = false;

    %client.sendPlayerListUpdate();
    
    commandToClient(%client, 'setAdminLevel', 0);
    // commandToClient(%client, 'GlassServerControlEnable', false);
    commandToClient(%client, 'hasPrefSystem', $BLPrefs::Version, %client.BLP_isAllowedUse());
  }
}

function GlassServerControlS::sendUpdateInfo(%client) {
  %count = updater.fileDownloader.queue.getCount();
	for(%i = 0; %i < %count; %i ++) {
		%item = updater.fileDownloader.queue.getObject(%i);
		%name = %item.name;
		%version = %item.updateVersion;

		commandToClient(%client, 'GlassAddUpdate', %name, %version, (%i==0));
	}

  if(!%count) {
    commandToClient(%client, 'GlassNoUpdates');
  }
}

//====================================
// Server Commands / Communication
//====================================

function containsField(%needle, %haystack) {
  for(%i = 0; %i < getFieldCount(%haystack); %i++) {
    if(getField(%haystack, %i) $= %needle) {
      return true;
    }
  }
  return false;
}

function serverCmdGlassUpdateSend(%client) {
  messageAll('MsgAdminForce', '\c3%1 \c0updated the server settings.', %client.name);
}

package GlassServerControlS {
  function GameConnection::autoAdminCheck(%client) {
    %ret = parent::autoAdminCheck(%client);
    commandToClient(%client, 'GlassHandshake', Glass.version);
    if(%client.isAdmin || %client.bl_id == 999999) {
      commandToClient(%client, 'hasPrefSystem', $BLPrefs::Version, %client.BLP_isAllowedUse());
      GlassServerControlS::sendAdminData(%client);
    }

    if(!%client.hasGlass && GlassClientSupport.idx > 0) {
      commandToClient(%client, 'messageBoxOk', "Recommended Mods", "This server has some optional clients you can download:<br>" @ GlassClientSupport.getLinks());
    }

    return %ret;
  }

	function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p) {
    //echo(%a TAB %b TAB %c TAB %d TAB %e TAB %f TAB %g TAB %us TAB %i TAB %j TAB %k TAB %l TAB %m TAB %n TAB %o TAB %p);
    %parent = parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p);
		for(%i = 0; %i < getLineCount(%us); %i++) { //being respectful of other mods, not hogging a whole argument
			%line = getLine(%us, %i);
			if(getField(%line, 0) $= "Glass") {
        %this.hasGlass = true;
        %version = getField(%line, 1);
        %this._glassVersion = %version;
        %this._glassModsRaw = getField(%line, 2);
        %this._glassBypass = getField(%line, 3);
				break;
			}
		}

    if(GlassClientSupport.required || !%this._glassBypass)
      %res = GlassClientSupport::checkClient(%this, %this._glassModsRaw);

    if(%res != true) {
      return %res;
    }
    return %parent;
	}
};
activatePackage(GlassServerControlS);
