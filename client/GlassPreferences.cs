//commandToClient(%client, 'GlassPref', %pref.idx, %pref.title, %pref.type, %pref.parameters, %pref.value);

$remapDivision[$remapCount] = "Blockland Glass";
   $remapName[$remapCount] = "Server Settings";
   $remapCmd[$remapCount] = "openGlassSettings";
   $remapCount++;

function openGlassSettings() {
  canvas.pushDialog(GlassServerControlGui);
}

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

  GlassPrefs.idx[%idx] = %obj;

  if(GlassPrefs.addonCount[%addon] $= "") {
    GlassPrefs.addons = trim(GlassPrefs.addons SPC %addon);
  }

  GlassPrefs.addonItem[%addon SPC GlassPrefs.addonCount[%addon]+0] = %obj;
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

function clientCmdGlassUpdatePref(%idx, %value) {
  GlassPrefs.idx[%idx].value = %value;
  GlassServerControl::updatePrefs();
}
