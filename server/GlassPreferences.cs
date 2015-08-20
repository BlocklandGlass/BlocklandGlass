
//====================================
// Admin
//====================================

function removeItemFromList(%list, %item) {
  for(%i = 0; %i < getWordCount(%list); %i++) {
    %id = getWord(trim(%list), %i);
    if(%id !$= %item) {
      %newList = %newList SPC %item;
    }
  }

  return trim(%newList);
}

function addItemToList(%list, %item) {
  return trim(%list SPC %item);
}

function GlassPreferences::addAutoAdmin(%blid, %super) {
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

function GlassPreferences::removeAutoAdmin(%blid) {
  $Pref::Server::AutoAdminList = removeItemFromList($Pref::Server::AutoAdminList, %blid);
  $Pref::Server::AutoSuperAdminList = removeItemFromList($Pref::Server::AutoSuperAdminList, %blid);

  %client = findClientByBL_ID(%blid);
  if(isObject(%client)) {
    %name = %client.name;
  } else {
    %name = "BLID_" @ %blid;
  }

  $Pref::Server::AutoSuperAdminList = addItemToList($Pref::Server::AutoSuperAdminList, %blid);
  messageAll('MsgAdminForce','\c2%1 has been demoted (Manual)',%name);
  if(isObject(%client)) {
    %client.isAdmin = true;
    %client.isSuperAdmin = true;
  }

  if(isObject(%client)) {
    %client.sendPlayerListUpdate();
    commandtoclient(%client,'setAdminLevel',0);
  }
}

//====================================
// Preferences
//====================================

//%addon - must be reference to actual add-on
//%title - title
//%type - bool, slider, text, int
//%parm - none, min max, len, min max
//%default - default
//%callback - update callback
function GlassPreferences::registerPref(%addon, %title, %type, %parm, %default, %callback) {
  if(!isObject(GlassPrefGroup)) {
    new ScriptGroup(GlassPrefGroup);
  }

  %pref = new ScriptObject() {
    class = "GlassPref";

    addon = %addon;
    title = %title;
    type = %type;
    parameters = %parm;
    def = %default;
    callback = %callback;
  };

  switch$(%type) {
    case "bool":
      echo("bool");
      %valid = true;

    case "slider":
      echo("slider with range " @ getWord(%parm, 0) @ " to " @ getWord(%parm, 1));
      %valid = true;

    case "text":
      echo("text with length " @ getWord(%parm, 0));
      %valid = true;

    case "int":
      echo("int with range " @ getWord(%parm, 0) @ " to " @ getWord(%parm, 1));
      %valid = true;
  }

  if(!%valid) {
    %pref.delete();
    error("Invalid preference type!");
    return;
  }

  if(!isfile("Add-Ons/" @ %addon @ "/server.cs")) {
    if(!$Glass::DevMode) {
      %pref.delete();
      error("Add-Ons/" @ %addon @ "/server.cs doesn't exist! Invalid add-on name");
      return;
    } else {
      warn("Add-on not found, however we're in development mode. Carrying on...");
    }
  }

  if($Glass::savedValue[%addon TAB %title] !$= "") {
    %pref.value = $Glass::savedValue[%addon TAB %title];
  } else {
    %pref.value = %def;
  }

  GlassPrefGroup.add(%pref);
  %pref.idx = GlassPrefGroup.idx++;
  GlassPrefGroup.idx[%pref.idx] = %pref;
}

function GlassPreferences::loadPrefs() {
  if($GameModeArg $= "") {
    %file = "config/BLG/server/prefs.cs";
  } else {
    %str = getsubstr($GameModeArg, strpos($GameModeArg, "/")+1, strlen($GameModeArg));
    %addon = getsubstr(%str, 0, strpos(%str, "/"));
    %file = "config/BLG/server/gamemodePrefs/" @ %addon @ ".cs";
  }

  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %pref = GlassPrefGroup.getObject(%i);
    %pref.value = %pref.def;
  }

  %fo = new FileObject();
  %fo.openForRead(%file);
  while(!%fo.isEOF()) {
    %line = %fo.readLine();
    //addon title value
    %addon = getField(%line, 0);
    %title = getField(%line, 1);
    %value = getField(%line, 2);

    $Glass::savedValue[%addon TAB %title] = %value;
  }
  %fo.close();
  %fo.delete();
}

function GlassPref::setValue(%this, %value) {
  %this.value = %value;
  if(%this.callback !$= "") eval(%this.callback @ "(" @ expandEscape(%value) @ ");");
}

function GlassPref::getValue(%this) {
  return %this.value;
}

//====================================
// Server Commands / Communication
//====================================


function serverCmdGlassUpdateSend(%client) {
  messageAll('MsgAdminForce', '\c3%1 \c0updated the server settings.', %client.name);
}

function serverCmdGlassUpdatePref(%client, %prefIdx, %value) {
  if(%client.isAdmin) {
    GlassPrefGroup.idx[%prefIdx].setValue(%value);
    echo("Pref " @ %prefIdx @ " updated to " @ %value);
  } else {
    messageClient(%client, '', "You don't have permission to do that");
  }
}

function GameConnection::sendGlassPrefs(%client) {
  commandToClient(%client, 'GlassPrefStart'); //signals client to wipe old prefs
  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %pref = GlassPrefGroup.getObject(%i);
    echo(%pref.idx TAB %pref.title);
    commandToClient(%client, 'GlassPref', %pref.idx, %pref.title, %pref.addon, %pref.type, %pref.parameters, %pref.value);
  }
  commandToClient(%client, 'GlassPrefEnd'); //signals client to render
}

package GlassPreferences {
  function GameConnection::autoAdminCheck(%client) {
    %ret = parent::autoAdminCheck(%client);
    commandToClient(%client, 'GlassHandshake', BLG.version);
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
activatePackage(GlassPreferences);

if(isObject(GlassPrefGroup)) {
  GlassPrefGroup.deleteAll();
  GlassPrefGroup.delete();
}

//TESTING WOOT!
GlassPreferences::registerPref("System_BlocklandGlass", "Cool kid?", "bool", "", false);
GlassPreferences::registerPref("System_BlocklandGlass", "How cool?", "slider", "0 9000", 6);
GlassPreferences::registerPref("System_BlocklandGlass", "Multiplying factor", "int", "0 15", 1);
GlassPreferences::registerPref("System_BlocklandGlass", "Cool kid club?", "text", "100", "kkk");
GlassPreferences::loadPrefs();
