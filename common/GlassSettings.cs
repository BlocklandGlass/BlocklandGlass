function GlassSettings::init(%context) {
  if(isObject(GlassSettings))
    return;

    new ScriptGroup(GlassSettings);

    echo("Loading " @ %context @ " prefs");

    if(%context $= "client") {
      GlassSettings.register("client", "MM::Keybind", "ctrl m");
      GlassSettings.register("client", "MM::UseDefault", false);
    } else if(%context $= "server") {
      GlassSettings.register("server", "SC::SAEditRank", 3);
      GlassSettings.register("server", "SC::AEditRank", 2);
    }

    GlassSettings.loadData(%context);;
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
  %fo.delete();

  %this.loaded[%context] = true;
}

function GlassSettings::saveData(%this, %context) {
  %fo = new FileObject();
  %fo.openForWrite("config/" @ %context @ "/glass.conf");

  for(%i = 0; %i < %this.getCount(); %i++) {
    %setting = %this.getObject(%i);
    if(%setting.context $= %context) {
      %fo.writeLine(%setting.name SPC expandEscape(%setting.value));
    }
  }

  %fo.close();
  %fo.delete();
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
