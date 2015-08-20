//commandToClient(%client, 'GlassPref', %pref.idx, %pref.title, %pref.type, %pref.parameters, %pref.value);

function clientcmdGlassPref(%idx, %title, %type, %parm, %value) {
  echo("Received pref! " @ %idx @":"@ %title @":"@ %type @":"@ %parm @":"@ %value);
}
