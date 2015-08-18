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
}

function GlassPreferences::loadPrefs() {
  if($GameModeArg $= "") {
    %file = "config/BLG/server/prefs.cs";
  } else {
    %str = getsubstr($GameModeArg, strpos($GameModeArg, "/")+1, strlen($GameModeArg));
    %addon = getsubstr(%str, 0, strpos(%str, "/"));
    %file = "config/BLG/server/gamemodePrefs/" @ %addon @ ".cs";
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
  eval(%this.callback @ "(" @ expandEscape(%value) @ ");");
}

function GlassPref::getValue(%this) {
  return %this.value;
}

function GameConnection::sendGlassPrefs(%client) {
  for(%i = 0; %i < GlassPrefGroup.getCount(); %i++) {
    %pref = GlassPrefGroup.getObject(%i);
    echo(%pref.idx TAB %pref.title);
    commandToClient(%client, 'GlassPref', %pref.idx, %pref.title, %pref);
  }
}

package GlassPreferences {
  function GameConnection::autoAdminCheck(%client) {
    %ret = parent::autoAdminCheck(%client);

    %client.sendGlassPrefs();

    return %ret;
  }
};
activatePackage(GlassPreferences);
