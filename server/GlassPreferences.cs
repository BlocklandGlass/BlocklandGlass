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

function GlassPreferences::sendAdminData(%client) {
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

    case "textarea":
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

  if($Glass::PrefSavedValue[%addon TAB %title] !$= "") {
    %pref.value = $Glass::PrefSavedValue[%addon TAB %title];
  } else {
    %pref.value = %default;
  }

  GlassPrefGroup.add(%pref);

  %pref.idx = GlassPrefGroup.idx++;
  GlassPrefGroup.idx[%pref.idx] = %pref;
  GlassPrefGroup.name[%addon TAB %title] = %pref;

  $Glass::PrefNamesCache = removeItemFromArray($Glass::PrefNamesCache, expandEscape(%addon TAB %title));
  $Glass::PrefNamesCache = addItemToArray($Glass::PrefNamesCache, expandEscape(%addon TAB %title));
  $Glass::PrefSavedValue[%addon TAB %title] = %pref.value;
}

function GlassPreferences::loadPrefs(%doCallback) {
  if(!isObject(GlassPrefGroup)) {
    new ScriptGroup(GlassPrefGroup);
  }

  if($GameModeArg $= "") {
    %file = "config/BLG/server/prefs.dat";
  } else {
    %str = getsubstr($GameModeArg, strpos($GameModeArg, "/")+1, strlen($GameModeArg));
    %addon = getsubstr(%str, 0, strpos(%str, "/"));
    %file = "config/BLG/server/gamemodePrefs/" @ %addon @ ".dat";
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
    %value = collapseEscape(getField(%line, 2));

    $Glass::PrefSavedValue[%addon TAB %title] = %value;
  }
  $Glass::PrefNamesCache = removeItemFromArray($Glass::PrefNamesCache, expandEscape(%addon TAB %title));
  $Glass::PrefNamesCache = addItemToArray($Glass::PrefNamesCache, expandEscape(%addon TAB %title));
  %fo.close();
  %fo.delete();
}

function GlassPreferences::savePrefs() {
  if($GameModeArg $= "") {
    %file = "config/BLG/server/prefs.dat";
  } else {
    %str = getsubstr($GameModeArg, strpos($GameModeArg, "/")+1, strlen($GameModeArg));
    %addon = getsubstr(%str, 0, strpos(%str, "/"));
    %file = "config/BLG/server/gamemodePrefs/" @ %addon @ ".dat";
  }


  %fo = new FileObject();
  %fo.openForWrite(%file);

  for(%i = 0; %i < getFieldCount($Glass::PrefNamesCache); %i++) {
    %dat = collapseEscape(getField($Glass::PrefNamesCache, %i));
    %addon = getField(%dat, 0);
    %title = getField(%dat, 1);

    if(isObject(GlassPrefGroup.name[%addon TAB %title])) {
      echo("is object!");
      %val = GlassPrefGroup.name[%addon TAB %title].getValue();
    } else {
      echo("no object");
      %val = $Glass::PrefSavedValue[%addon TAB %title];
    }
    echo(%val);

    %fo.writeLine(%addon TAB %title TAB expandEscape(%val));
  }

  %fo.close();
  %fo.delete();
}

function GlassPref::setValue(%this, %value) {
  %this.value = %value;
  if(%this.callback !$= "") eval(%this.callback @ "(" @ expandEscape(%value) @ ");");

  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    %client = ClientGroup.getObject(%i);
    if(%client.hasGlass && %client.isAdmin) {
      commandToClient(%client, 'GlassUpdatePref', %this.idx, %value);
    }
  }
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
    echo(" + glass shit");
    %ret = parent::autoAdminCheck(%client);
    commandToClient(%client, 'GlassHandshake', BLG.version);

    if(%ret) {
      commandToClient(%client, 'GlassServerControlEnable', true);
      GlassPreferences::sendAdminData(%client);
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

  function RTB_registerPref(%name,%cat,%pref,%vartype,%mod,%default,%requiresRestart,%hostOnly,%callback) {
    %ret = parent::RTB_registerPref(%name,%cat,%pref,%vartype,%mod,%default,%requiresRestart,%hostOnly,%callback);
    warn("Importing legacy RTB pref! Errors expected.");
    echo(%vartype);
    %type = getWord(%vartype, 0);

    if(%type $= "string") {
      %type = "text";
      %parm = getWord(%vartype, 1);
    }

    GlassPreferences::registerPref(%mod, %name, %type, %parm, %default, %callback);
    return %ret;
  }

  function onExit() {
    GlassPreferences::savePrefs();
    parent::onExit();
  }
};
activatePackage(GlassPreferences);
