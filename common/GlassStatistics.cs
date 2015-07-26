function GlassStatistics::reportMods() {
  GlassStatistics::scanFiles();

  %str = ""; // 1234,stable,1.0.0-alpha.1+1
  for(%i = 0; %i < GlassAddons.getCount(); %i++) {
    %addon = GlassAddons.getObject(%i);

    %str = %str @ "^" @ %addon.id @ "," @ %addon.channel @ "," @ %addon.version;
  }

  %str = getsubstr(%str, 1, strlen(%str)-1);
  echo(%str);
}

function GlassStatistics::scanFiles() {
  if(isObject(GlassAddons)) {
    GlassAddons.delete();
  }
  new ScriptGroup(GlassAddons);
	%pattern = "Add-ons/*/glass.json";
	echo("\c1Looking for Glass Add-Ons");
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %name = getsubstr(%file, 8, strlen(%file)-19);
		%fo = new FileObject();
		%fo.openForRead(%file);

    %buffer = "";
		while(!%fo.isEOF()) {
			%buffer = %buffer NL %fo.readLine();
		}

		%fo.close();
		%fo.delete();

    %glassObj = parseJSON(%buffer);

    %go = new ScriptObject(GlassAddonData) {
      id = %glassObj.get("id");
      board = %glassObj.get("board");
      filename = %glassObj.get("filename");
      title = %glassObj.get("title");
    };

    if(isfile("Add-Ons/" @ %name @ "/version.json")) {
      %fo = new FileObject();
  		%fo.openForRead("Add-Ons/" @ %name @ "/version.json");

      %buffer = "";
  		while(!%fo.isEOF()) {
  			%buffer = %buffer NL %fo.readLine();
  		}

  		%fo.close();
  		%fo.delete();

      %versionData = parseJSON(%buffer);

      %go.version = %versionData.get("version");
      %go.channel = %versionData.get("channel");
    } else {
      echo("\c2Missing version.json!");
      %go.delete();
      continue;
    }

    GlassAddons.add(%go);

		echo(" \c1+ Found \c4\"" @ %name @ "\" \c1(" @ %go.id @ ")");
	}
}
