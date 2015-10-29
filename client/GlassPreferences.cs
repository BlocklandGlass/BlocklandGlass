//commandToClient(%client, 'GlassPref', %pref.idx, %pref.title, %pref.type, %pref.parameters, %pref.value);

$remapDivision[$remapCount] = "Blockland Glass";
   $remapName[$remapCount] = "Server Settings";
   $remapCmd[$remapCount] = "openGlassSettings";
   $remapCount++;

function openGlassSettings() {
  if(GlassServerControl.enabled) {
    canvas.pushDialog(GlassServerControlGui);
  }
}
