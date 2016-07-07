function GlassRequiredClients::init() {
  if(isObject(GlassRequiredClients))
   GlassRequiredClients.delete();

  %this = new ScriptObject(GlassRequiredClients);

  %pattern = "Add-ons/*/glass.json";
	//echo("\c1Looking for Glass Add-Ons");
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %error = jettisonReadFile(%file);
    if(%error) {
      echo("Jettison read error");
      continue;
    }

    %value = $JSON::Value;

    %this.hasAddon[%value.id] = true;
  }
}

package GlassRequiredClients {
  function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
    return parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "Glass" TAB Glass.version TAB GlassClientManager.getClients() NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
  }
};
activatePackage(GlassRequiredClients);
