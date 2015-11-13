function GlassSettings::init(%context) {
  if(isObject(GlassSettings))
    return;

    new ScriptGroup(GlassSettings);

    echo("Loading " @ %context @ " prefs");

    if(%context $= "client") {
      GlassSettings.register("client", "MM::Keybind", "keyboard\tctrl m");
      GlassSettings.register("client", "MM::UseDefault", false);
      GlassSettings.register("client", "MM::Colorset", "Add-Ons/System_BlocklandGlass/colorset_default.txt");
    } else if(%context $= "server") {
      GlassSettings.register("server", "SC::SAEditRank", 3);
      GlassSettings.register("server", "SC::AEditRank", 2);
    }

    GlassSettings.loadData(%context);
}

function GlassSettings::register(%this, %context, %name, %defaultValue, %callback) {
  %obj = new ScriptObject() {
    class = "GlassSetting";

    name = %name;
    value = %defaultValue;
    callback = %callback;

    context = %context;
  };

  %this.obj[%name] = %obj;
  %this.add(%obj);
}

function GlassSettings::loadData(%this, %context) {
  %fo = new FileObject();
  %fo.openForRead("config/" @ %context @ "/glass.conf");
  while(!%fo.isEOF()) {
    %line = %fo.readLine();
    %this.loadSetting(getField(%line, 0), getField(%line, 1));
  }

  %fo.close();

  if(!%this.cacheLoaded) {
    %fo.openForRead("cache/glass.dat");
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
  %fo2.openForWrite("cache/glass.dat");

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

function GlassSettings::update(%name, %value) {
  %obj = GlassSettings.obj[%name];
  %obj.value = %value;
  if(%obj.callback !$= "") {
    eval(%obj.callback @ "(\"" @ expandEscape(%name) @ "\",\"" @ %value @ "\");");
  }
}

function GlassSettings::get(%name) {
  return GlassSettings.obj[%name].value;
}

function GlassSettings::cacheCreate(%this, %name, %value, %ttl, %time) {
  %obj = new ScriptObject() {
    class = "GlassCache";
    value = %value;

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
