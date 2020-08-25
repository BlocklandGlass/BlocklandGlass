function GlassStatistics::reportMods() {
  GlassStatistics::scanFiles();

  %str = ""; // 1234,stable,1.0.0-alpha.1+1
  for(%i = 0; %i < GlassAddons.getCount(); %i++) {
    %addon = GlassAddons.getObject(%i);

    %str = %str @ "^" @ %addon.id @ "," @ %addon.channel @ "," @ %addon.version;
  }

  %str = getsubstr(%str, 1, strlen(%str)-1);

  %url = "sha=" @ sha1(getComputerName()) @ "&data=" @ urlEnc(%str);

  %tcp = GlassApi.request("stats", %url, "GlassStatTCP", true);
}

function GlassStatistics::scanFiles() {
  if(isObject(GlassAddons)) {
    GlassAddons.delete();
  }
  new ScriptGroup(GlassAddons);
  GlassGroup.add(GlassAddons);

	%pattern = "Add-ons/*/glass.json";
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-19);
    if(strPos(%name, "/") > -1) continue;

		%fo = new FileObject();
		%fo.openForRead(%file);

    %buffer = "";
		while(!%fo.isEOF()) {
			%buffer = %buffer NL %fo.readLine();
		}

		%fo.close();
		%fo.delete();

    if(!jettisonParse(%buffer)) {
      %glassObj = $JSON::Value;

      %go = new ScriptObject(GlassAddonData) {
        id = %glassObj.get("id");
        board = %glassObj.get("board");
        filename = %glassObj.get("filename");
        title = %glassObj.get("title");
      };

      %glassObj.schedule(0,delete);
    } else {
      error("Parse error - " @ $JSON::Error @ " in " @ %file);
    }

    if(isfile("Add-Ons/" @ %name @ "/version.json")) {
      %fo = new FileObject();
  		%fo.openForRead("Add-Ons/" @ %name @ "/version.json");

      %buffer = "";
  		while(!%fo.isEOF()) {
  			%buffer = %buffer NL %fo.readLine();
  		}

  		%fo.close();
  		%fo.delete();

      jettisonParse(%buffer);
      %versionData = $JSON::Value;

      %go.version = %versionData.get("version");
      %go.channel = %versionData.get("channel");

      %versionData.schedule(0,delete);
    } else {
      //warn("\c2Missing version.json!");
      %go.delete();
      continue;
    }

    GlassAddons.add(%go);
	 
	}
}

package GlassStatistics {
  function GlassAuth::onAuthSuccess(%this) {
    parent::onAuthSuccess(%this);
  	GlassStatistics::reportMods();
  }
};
activatePackage(GlassStatistics);
