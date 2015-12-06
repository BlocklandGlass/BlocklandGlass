if($Glass::Modules::Prefs)
  return;

$Glass::Modules::Prefs = true;
//====================================
// Admin
//====================================

function registerGlassPrefs() {
	%cat = "Blockland Glass"; //on the glass client, this will be loaded in to it's own page (settings)
	%icon = "server";

	%promotesa = registerBlocklandPref(%cat, "Who can manage super-admins?", "list", "$Pref::Glass::SAPromoteLevel", GlassSettings.get("SC::SAEditRank"), "Host**3|Super Admin**2", "updateGlassPref", %icon, 0);
	%promotea = registerBlocklandPref(%cat, "Who can manage admins?", "list", "$Pref::Glass::APromoteLevel", GlassSettings.get("SC::AEditRank"), "Host**3|Super Admin**2|Admin**1", "updateGlassPref", %icon, 0);
	%required = registerBlocklandPref(%cat, "Required Client Add-Ons", "string", "$Pref::Glass::ClientAddons", GlassSettings.get("SC::RequiredClients"), "", "updateGlassPref", "", %icon, 0);

  //registerBlocklandPref(%cat, "Colorset (restart)", "string", "$Pref::Glass::Colorset", GlassSettings.get("SC::RequiredClients"), "updateGlassPref", "", %icon, 0);

  %required.announce = false;
}
registerGlassPrefs();

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
    return %this.bl_id == getNumKeyId();
  } else if(%perm == 2) {
    return (%this.bl_id == getNumKeyId() || %this.isSuperAdmin);
  } else if(%perm == 1) {
    return (%this.bl_id == getNumKeyId() || %this.isSuperAdmin || %this.isAdmin);
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

function serverCmdGlassSetAdmin(%client, %blid, %level) {
  if(%blid == getNumKeyId()) {
    return; //host
  }

  if(%level > 0) {
    %sa = %level-1;
    if(%sa) {
      if(%client.checkPermissionLevel($Pref::Glass::SAPromoteLevel)) {
        GlassServerControlS::addAutoAdmin(%blid, 1);
      }
    } else {
      if(%client.checkPermissionLevel($Pref::Glass::APromoteLevel)) {
        GlassServerControlS::addAutoAdmin(%blid, 0);
      }
    }
  } else {
    %theirlevel = getAdminLevelFromBLID(%blid);
    if(%theirlevel == 2) {
      if(%client.checkPermissionLevel($Pref::Glass::SAPromoteLevel)) {
        GlassServerControlS::removeAutoAdmin(%blid);
      }
    } else if(%theirlevel == 1) {
      if(%client.checkPermissionLevel($Pref::Glass::APromoteLevel)) {
        GlassServerControlS::removeAutoAdmin(%blid);
      }
    } else {
      GlassServerControlS::removeAutoAdmin(%blid);
    }
  }

  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);
    if(%cl.isAdmin) {
      GlassServerControlS::sendAdminData(%cl);
    }
  }
}

function GlassServerControlS::addAutoAdmin(%blid, %super) {
  $Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList, %blid);
  $Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList, %blid);

  if(%blid == getNumKeyId()) {
    error("Attempted to promote host to admin.");
    return;
  }

  %client = findClientByBL_ID(%blid);
  if(isObject(%client)) {
    %name = %client.name;
  } else {
    %name = "BLID_" @ %blid;
  }

  if(%super) {
    $Pref::Server::AutoSuperAdminList = addItemToList($Pref::Server::AutoSuperAdminList, %blid);
    messageAll('MsgAdminForce','\c2%1 has become Super Admin (Auto)',%name);
    if(isObject(%client)) {
      %client.isAdmin = true;
      %client.isSuperAdmin = true;
    }
  } else {
    $Pref::Server::AutoAdminList = addItemToList($Pref::Server::AutoAdminList, %blid);
    messageAll('MsgAdminForce','\c2%1 has become Admin (Auto)',%name);
    if(isObject(%client)) {
      %client.isAdmin = true;
      %client.isSuperAdmin = false;
    }
  }

  if(isObject(%client)) {
    %client.sendPlayerListUpdate();
    if(%super) {
      commandtoclient(%client,'setAdminLevel', 2);
    } else {
      commandtoclient(%client,'setAdminLevel', 1);
    }
    commandToClient(%client, 'GlassServerControlEnable', true, %client.BLP_isAllowedUse());
  }
}

function GlassServerControlS::sendAdminData(%cl) {
  %buffer = "";
  $Pref::Server::AutoSuperAdminList = trim($Pref::Server::AutoSuperAdminList);
  for(%i = 0; %i < getWordCount($Pref::Server::AutoSuperAdminList); %i++) {
    %id = getWord($Pref::Server::AutoSuperAdminList, %i);
    %client = findClientByBL_ID(%id);
    if(isObject(%client)) {
      %name = %client.name;
    } else {
      %name = "BLID_" @ %id;
    }

    if(%id != getNumKeyId()) {
      %buffer = %buffer @ %name TAB %id TAB "S\n";
    } else {
      %buffer = %buffer @ %name TAB %id TAB "H\n";
    }
  }
  commandToClient(%cl, 'GlassAdminListing', trim(%buffer));

  %buffer = "";
  $Pref::Server::AutoAdminList = trim($Pref::Server::AutoAdminList);
  for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++) {
    %id = getWord($Pref::Server::AutoAdminList, %i);
    %client = findClientByBL_ID(%id);
    if(isObject(%client)) {
      %name = %client.name;
    } else {
      %name = "BLID_" @ %id;
    }

    %buffer = %buffer @ %name TAB %id TAB "A\n";
  }
  if(%buffer !$= "") commandToClient(%cl, 'GlassAdminListing', trim(%buffer), true);
}

function GlassServerControlS::removeAutoAdmin(%blid) {
  $Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList, %blid);
  $Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList, %blid);

  %client = findClientByBL_ID(%blid);
  if(isObject(%client)) {
    %name = %client.name;
  } else {
    %name = "BLID_" @ %blid;
  }

  messageAll('MsgAdminForce','\c2%1 has been demoted (Manual)',%name);
  if(isObject(%client)) {
    %client.isAdmin = false;
    %client.isSuperAdmin = false;

    %client.sendPlayerListUpdate();
    commandtoclient(%client, 'setAdminLevel', 0);
    commandToClient(%client, 'GlassServerControlEnable', false);
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
    if(%client.isAdmin) {
      commandToClient(%client, 'GlassServerControlEnable', true, %client.BLP_isAllowedUse());
      GlassServerControlS::sendAdminData(%client);
      GlassServerControlS::sendUpdateInfo(%client);
    }
    return %ret;
  }

	function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p) {
    echo(%a TAB %b TAB %c TAB %d TAB %e TAB %f TAB %g TAB %us TAB %i TAB %j TAB %k TAB %l TAB %m TAB %n TAB %o TAB %p);
    %ret = parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p);
		for(%i = 0; %i < getLineCount(%us); %i++) { //being respectful of other mods, not hogging a whole argument
			%line = getLine(%us, %i);
			if(getField(%line, 0) $= "Glass") {
        %this.hasGlass = true;
        %version = getField(%line, 1);
        %clients = strreplace(getField(%line, 2), " ", "\t"); //addons in the client mods category

        %required = strreplace(GlassSettings.get("SC::RequiredClients"), ",", "\t");
        if(%required !$= "") {
          %missingStr = "";

          for(%i = 0; %i < getFieldCount(%required); %i++) {
            %mid = trim(getField(%required, %i));
            if(containsField(%mid, %clients)) {
              %this._glassHasClient[%mid] = true;
            } else {
              %missingStr = %mid TAB GlassSettings.cacheFetch("AddonName_" @ %mid) @  "<br>";
            }
          }

          if(%missingStr !$= "") {
            echo(" +- missing client mods");
            if(!%this.isLocalConnection()) {
              %this.schedule(0, "delete", "Missing Blockland Glass Mods<br><br>" @ %missingStr);
            }
          }
        }
				break;
			}
		}

    %required = GlassSettings.get("SC::RequiredClients");
    if(trim(%required) !$= "" && !%this.hasGlass) { //non-glass clients
      echo(" +- missing client mods AND missing glass");
      for(%i = 0; %i < getFieldCount(%required); %i++) {
        %mid = trim(getField(%required, %i));
        %missingStr = "<a:blocklandglass.com/addon.php?id=" @ %mid @ ">" @ GlassSettings.cacheFetch("AddonName_" @ %mid) @ "</a><br>";
      }
      if(!%this.isLocalConnection()) {
        %this.schedule(0, "delete", "The server host has specified that certain client add-ons are required for this server. You can use <a:blocklandglass.com/dl.php>Blockland Glass</a> to automatically download them for you, or alternatively download them yourself:<br><br>" @ %missingStr);
      }
    }

    return %ret;
	}
};
activatePackage(GlassServerControlS);
