if($Glass::Modules::Prefs)
  return;

$Glass::Modules::Prefs = true;
//====================================
// Admin
//====================================

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
  }
}

function GlassServerControlS::sendAdminData(%client) {
  %buffer = "";
  for(%i = 0; %i < getWordCount($Pref::Server::AutoSuperAdminList); %i++) {
    %id = getWord($Pref::Server::AutoSuperAdminList, %i);
    %client = findClientByBL_ID(%id);
    if(isObject(%client)) {
      %name = %client.name;
    } else {
      %name = "BLID_" @ %blid;
    }

    %buffer = %buffer @ %name TAB %id TAB "S\n";
  }
  if(%buffer !$= "") commandToClient(%client, 'GlassAdminListing', trim(%buffer));

  %buffer = "";
  for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++) {
    %id = getWord($Pref::Server::AutoAdminList, %i);
    %client = findClientByBL_ID(%id);
    if(isObject(%client)) {
      %name = %client.name;
    } else {
      %name = "BLID_" @ %blid;
    }

    %buffer = %buffer @ %name TAB %id TAB "A\n";
  }
  if(%buffer !$= "") commandToClient(%client, 'GlassAdminListing', trim(%buffer), true);
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

//====================================
// Server Commands / Communication
//====================================

function serverCmdGlassUpdateSend(%client) {
  messageAll('MsgAdminForce', '\c3%1 \c0updated the server settings.', %client.name);
}

package GlassServerControlS {
  function GameConnection::autoAdminCheck(%client) {
    echo(" + glass shit");
    %ret = parent::autoAdminCheck(%client);
    commandToClient(%client, 'GlassHandshake', BLG.version);

    if(%ret) {
      commandToClient(%client, 'GlassServerControlEnable', true);
      GlassServerControlS::sendAdminData(%client);
    }
    return %ret;
  }

  function serverCmdGlassHandshake(%client, %version) {
    parent::serverCmdGlassHandshake(%client, %version);
    echo(%client.name @ " - blg version " @ %version);

    if(%client.isAdmin || %client.isSuperAdmin || %client.isHost) {
      %client.sendGlassPrefs();
      //send permissions, auto admin lists?
    }
  }
};
activatePackage(GlassServerControlS);
