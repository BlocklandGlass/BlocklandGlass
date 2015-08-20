//commandToClient(%client, 'GlassPref', %pref.idx, %pref.title, %pref.type, %pref.parameters, %pref.value);

function clientcmdGlassPref(%idx, %title, %addon, %type, %parm, %value) {
  echo("Received pref! " @ %idx @":"@ %title @":"@ %type @":"@ %parm @":"@ %value);
  if(!isObject(GlassPrefs)) {
    new ScriptGroup(GlassPrefs);
  }

  echo("addon: " @ %addon);

  %obj = new ScriptObject() {
    class = GlassPref;

    idx = %idx;
    addon = %addon;
    title = %title;
    type = %type;
    parm = %parm;
    value = %value;
  };
  GlassPrefs.add(%obj);

  if(GlassPrefs.addonCount[%addon] $= "") {
    GlassPrefs.addons = trim(GlassPrefs.addons SPC %addon);
  }

  GlassPrefs.addonItem[GlassPrefs.addonCount[%addon]+0] = %obj;
  GlassPrefs.addonCount[%addon]++;
}

function clientCmdGlassPrefStart() {
  if(isObject(GlassPrefs)) {
    GlassPrefs.delete();
  }
  new ScriptGroup(GlassPrefs);
}

function clientCmdGlassPrefEnd() {
  GlassServerControl::renderPrefs();
}
