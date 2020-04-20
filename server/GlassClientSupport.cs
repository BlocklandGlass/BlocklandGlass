if($Glass::Hook::RequiredClient)
  return;

$Glass::Hook::RequiredClient = true;

function GlassClientSupport::init() {
  if(isObject(GlassClientSupport))
    return;

  new ScriptObject(GlassClientSupport);
}

//old : name, glassid, version, optional
//new : name, glassid, optional
function registerRequiredClient(%name, %glassId, %arg3, %arg4) {
  GlassClientSupport::init();

  if(%arg3 !$= 0 && %arg3 !$= 1 && %arg3 !$= "") {
    %version = %arg3;
    %optional = %arg4;
    warn("registerRequiredClient deprecated format\nPlease use: registerRequiredClient(name, id, optional)");
  } else {
    %optional = %arg3;
  }

  GlassClientSupport.registerRequiredAddon(%name, %glassId, %optional);
}

function GlassClientSupport::registerRequiredAddon(%this, %name, %glassId, %optional) {
  if(%this.requested[%glassId])
    return; //already registed

  %idx = %this.idx+0;
  %this.name[%idx] = %name;
  %this.id[%idx] = %glassId;
  %this.optional[%idx] = %optional;

  %this.requested[%glassId] = true;

  if(!%optional)
    %this.required = true;

  %this.idx++;
}

function GlassClientSupport::checkClient(%client, %mods) {
  %this = GlassClientSupport;
  if(%this.idx == 0) {
    return true;
  }

  //Record what mods we have in a indexed way
  for(%i = 0; %i < getWordCount(%mods); %i++) {
    %words = getWord(%mods, %i);
    %client._glassMod[getWord(%words, 0)] = true;
  }

  //Check required mods against reported
  for(%i = 0; %i < %this.idx; %i++) {
    if(!%client._glassMod[%this.id[%i]]) {
      %missing = trim(%missing TAB %i);
    }
  }


  if(%missing $= "") {
    echo("Has all required clients");
    return true;
  }

  if(%client.hasGlass || %client.hasRequiredClients) {
    if(%client.hasGlass)
      echo("Missing clients, has Glass");

    if(%client.hasRequiredClients)
      echo("Missing clients, has Support_RequiredClients");

    %missingStr = "";
    for(%i = 0; %i < getFieldCount(%missing); %i++) {
      %idx = getField(%missing, %i);
      %missingStr = %missingStr TAB %this.name[%idx] @ "^" @ %this.id[%idx];
    }

    %missingStr = trim(%missingStr);

    if(%this.required) {
      return "MISSING\t" @ %missingStr;
    } else {
      return "MISSING_OPT\t" @ %missingStr;
    }
  } else {
    echo("Missing clients, no Glass");
    if(%this.required) {
      %client.schedule(0, "delete", "Missing Blockland Glass<br><br>This server uses <a:http://blocklandglass.com/dl.php>Blockland Glass</a> to manage client mods. Please download Blockland Glass to access this server's client.");
    } else {
      //will show message
      %client.displayRequiredClients = true;
    }
  }
  return true;
}

function GlassClientSupport::getLinks(%this) {
  for(%i = 0; %i < %this.idx; %i++) {
    %missingStr = %missingStr @ "<br><a:http://blocklandglass.com/addon.php?id=" @ %this.id[%i] @ ">" @ %this.name[%i] @ "</a>";
  }
  return %missingStr;
}
