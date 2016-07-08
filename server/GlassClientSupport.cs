if($Glass::Hook::RequiredClient)
  return;

$Glass::Hook::RequiredClient = true;

function GlassClientSupport::init() {
  if(isObject(GlassClientSupport))
    return;

  new ScriptObject(GlassClientSupport);
}

function registerRequiredClient(%name, %glassId, %version, %optional) {
  GlassClientSupport::init();

  GlassClientSupport.registerRequiredAddon(%name, %glassId, %version, %optional);
}

function GlassClientSupport::registerRequiredAddon(%this, %name, %glassId, %version, %optional) {
  if(%this.required[%glassId])
    return; //already registed

  %idx = %this.idx+0;
  %this.name[%idx] = %name;
  %this.id[%idx] = %glassId;
  %this.version[%idx] = %version;
  %this.optional[%idx] = %optional;

  if(!%optional) {
    %this.required = true;
  }

  %this.idx++;
}

function GlassClientSupport::checkClient(%client, %mods) {
  %this = GlassClientSupport;
  if(!%this.required)
    return;

  for(%i = 0; %i < getWordCount(%mods); %i++) {
    %words = strreplace(getWord(%mods, %i), "|", " ");
    %client._glassMod[getWord(%words, 0)] = getWord(%words, 1);
  }

  for(%i = 0; %i < %this.idx; %i++) {
    if(%client._glassMod[%this.id[%i]] !$= %this.version[%i]) {
      %missing = trim(%missing TAB %i);
    }
  }

  if(%missing $= "") {
    echo("Has all required clients");
  } else {
    echo("Missing clients");
    if(%client.hasGlass) {
      for(%i = 0; %i < getFieldCount(%missing); %i++) {
        %idx = getField(%missing, %i);
        %missingStr = %missingStr TAB %this.name[%i] @ "^" @ %this.id[%i];
      }
      %missingStr = trim(%missingStr);
      return "MISSING\t" @ %missingStr;
    } else {
      %client.schedule(0, "delete", "Missing Blockland Glass<br><br>This server uses <a:http://blocklandglass.com/dl.php>Blockland Glass</a> to manage client mods. Please download Blockland Glass to access this server's client.");
    }
  }
  return true;
}
