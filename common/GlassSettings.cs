if(!isObject(GlassSettings)) {
  new ScriptGroup(GlassSettings);
}

function GlassSettings::init(%context) {
  echo("Loading " @ %context @ " prefs");

  if(%context $= "client") {
    GlassSettings.registerSetting("client", "MM::Keybind", "keyboard\tctrl m");
    GlassSettings.registerSetting("client", "MM::UseDefault", false);
    GlassSettings.registerSetting("client", "MM::Colorset", "Add-Ons/System_BlocklandGlass/colorset_default.txt");
  } else if(%context $= "server") {
    GlassSettings.registerSetting("server", "SC::SAEditRank", 3);
    GlassSettings.registerSetting("server", "SC::AEditRank", 2);
    GlassSettings.registerSetting("server", "SC::RequiredClients", "");
  }

  GlassSettings.loadData(%context);
}

function GlassSettings::registerSetting(%this, %context, %name, %defaultValue, %callback) {
  %obj = new ScriptObject() {
    class = "GlassSetting";

    name = %name;
    value = %defaultValue;
    callback = %callback;

    context = %context;
  };
  %this.obj[%name] = %obj;
  %this.add(%obj);
  %this.schedule(0, "add", %obj);
}

function GlassSettings::loadData(%this, %context) {
  %fo = new FileObject();
  if(isFile("config/" @ %context @ "/glass.conf")) {
    %fo.openForRead("config/" @ %context @ "/glass.conf");
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      echo("original data: " @ getField(%line, 1));
      echo("collapsed data: " @ collapseEscape(getField(%line, 1)));
      %this.loadSetting(getField(%line, 0), collapseEscape(getField(%line, 1)));
    }
  }

  %fo.close();

  if(!%this.cacheLoaded && isFile("config/cache/glass.dat")) {
    %fo.openForRead("config/cache/glass.dat");
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      %name = getField(%line, 0);
      %created = getField(%line, 1);
      %ttl = getField(%line, 2);
      %value = collapseEscape(getField(%line, 3));

      if(%created+%ttl < getRealTime() && %ttl != 0) {
        if($Glass::Debug)
          warn("Cached value [" @ %name @ "] has expired! [ " @ %created @ " | " @ %ttl @ " ]");
      } else {
        %this.cacheCreate(%name, %value, %ttl, %created);
      }
    }

    %this.cacheLoaded = true;
  }

  %fo.delete();

  %this.loaded[%context] = true;
}

function GlassSettings::saveData(%this, %context) {
  %fo = new FileObject();
  %fo.openForWrite("config/" @ %context @ "/glass.conf");
  %fo2 = new FileObject();
  %fo2.openForWrite("config/cache/glass.dat");

  for(%i = 0; %i < %this.getCount(); %i++) {
    %setting = %this.getObject(%i);
    if(%setting.context $= %context) {
      %fo.writeLine(%setting.name TAB expandEscape(%setting.value));
    }

    if(%setting.class $= "GlassCache") {
      %fo2.writeLine(%setting.name TAB %setting.created TAB %setting.ttl TAB expandEscape(%setting.value));
    }
  }

  %fo.close();
  %fo2.close();
  %fo.delete();
  %fo2.delete();
}

function GlassSettings::loadSetting(%this, %name, %value) {
  %obj = GlassSettings.obj[%name];
  if(isObject(%obj)) {
    if($Glass::Debug) {
      echo(" + Loaded pref " @ getField(%line, 0));
    }
    %obj.value = %value; //only do that if loading!
  } else {
    warn("Data found for non-existant pref \"" @ %name @ "\"");
  }
}

function GlassSettings::update(%this, %name, %value) {
  %obj = GlassSettings.obj[%name];
  %obj.value = %value;
  if(%obj.callback !$= "") {
    eval(%obj.callback @ "(\"" @ expandEscape(%name) @ "\",\"" @ %value @ "\");");
  }
}

function GlassSettings::get(%this, %name) {
  return %this.obj[%name].value;
}

function GlassSettings::cacheCreate(%this, %name, %value, %ttl, %time) {
  %obj = new ScriptObject() {
    class = "GlassCache";
    value = %value;

    name = %name;

    created = %time;
    ttl = %ttl; // %ttl -- 0 = infinite
  };

  %this.cache[%name] = %obj;
  %this.add(%obj);
}

function GlassSettings::cachePut(%this, %name, %value, %ttl) {
  if(!isObject(%this.cache[%name])) {
    %this.cacheCreate(%name, %value, %ttl+0, getRealTime());
  } else {
    %this.cache[%name].value = %value;
    %this.cache[%name].created = getRealTime();
  }
}

function GlassSettings::cacheFetch(%this, %name) {
  if(isObject(%this.cache[%name])) {
    return %this.cache[%name].value;
  } else {
    return "";
  }
}

package GlassSettingsPackage {
  function onExit() {
    if(GlassSettings.loaded["client"]) {
      GlassSettings.saveData("client");
    }

    if(GlassSettings.loaded["server"]) {
      GlassSettings.saveData("server");
    }
    parent::onExit();
  }
};
activatePackage(GlassSettingsPackage);
