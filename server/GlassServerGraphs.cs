
//================================================================
//= GlassServerGraphs                                            =
//================================================================

function GlassServerGraphs::init() {
  //all units in seconds
  //all of these should be prefs eventually
  new ScriptGroup(GlassServerGraphs) {
    increments = 15;
    history = 3600*12; //12 hours

    keepOpen = true;
  };
}

function GlassServerGraphs::newEvent(%this, %name, %time) {
  //creates line on graph, such as server start, server shutdown, map change, etc
}

function GlassServerGraphs::getCollection(%this, %name) {
  if(isObject(%this.collection[%name])) {
    return %this.collection[%name];
  } else {

    %illegal = "!@#$%^&*()+/\\\t\n\r?<>{}|:;";
    for(%i = 0; %i < strlen(%illegal); %i++) {
      if(strpos(%name, getSubStr(%illegal, %i, 1)) > -1) {
        error("Invalid collection name \"" @ %name @ "\"");
        return false;
      }
    }

    %collection = new ScriptObject() {
      class = "GlassCollection";
      name = %name;

      icon = "graph"; //default
    };
    %this.schedule(1, add, %collection);
    %this.collection[%name] = %collection;
    return %this.collection;
  }
}


//================================================================
//= GlassCollections                                             =
//================================================================


function GlassCollection::getFile(%this) {
  return "config/server/glass/collections/" @ strlwr(%this.name) @ ".dat";
}

function GlassCollection::onAdd(%this) {
  %file = %this.getFile();

  if(isFile(%file)) {
    %fo = new FileObject();
    %fo.openForRead(%file);
    %indexCt = -1;
    %dataCt = 0;
    while(!%fo.isEOF()) {
      %line = %fo.readLine();
      %line = strreplace(%line, "\t", "\\t");
      %line = strreplace(%line, ",", "\t");
      //CSV
      if(%indexCt == -1) {
        for(%i = 0; %i = getFieldCount(%line); %i++) {
          %index[%i] = collapseEscape(getField(%line, %i));
          %this.index[%i] = %index[%i];
        }
        %indexCt = getFieldCount(%line);
      } else {
        for(%i = 0; %i = getFieldCount(%line); %i++) {
          %key = %index[%i];
          if(%key $= "") {
            warn("More data than keys!");
          }
          %this.data[%dataCt][%key] = collapseEscape(getField(%line, %i));
          //data_12_time
          //data_12_value
        }
        %dataCt++;
      }
    }
    %fo.close();
    %fo.delete();

    %this.dataCt = %dataCt;
    %this.indexCt = %indexCt;
  } else {
    //create
  }
}

function GlassCollection::recordData(%this, %value, %time) {
  if(%time $= "") {
    %time = getRealTime();
  }

  if(%time < getRealTime()-GlassServerGraphs.history) {
    //too old
    return;
  }

  %this.data[%this.dataCt]["time"] = %time;
  %this.data[%this.dataCt]["value"] = %value;

  if(GlassServerGraphs.keepOpen) {
    if(!isObject(%this.fo)) {
      %fo = new FileObject();
      %fo.openForAppend(%this.getFile());
      %this.fo = %fo;
    }

    %this.fo.writeLine(%time @ "," @ %value);
  }

  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);
    if(%cl.isAdmin) {
      commandToClient(%cl, 'Glass_ServerGraphData', %this.name, %time, %value);
    }
  }
}


//================================================================
//= Default Graphs                                               =
//================================================================

function GlassServerGraphs::default(%this) {
  %bricks = %this.getCollection("Bricks");
  %bricks.icon = "brick";
  %bricks.color = "255 0 0";

  %players = %this.getCollection("Players");
  %players.icon = "user";
  %players.color = "0 255 0";

  %nextTime = %this.increments*mCeil(getRealTime()/%this.increments);
  %timeTo = %nextTime-getRealTime();

  %this.sch = %this.schedule(%timeTo, "defaultTick");
}

function GlassServerGraphs::defaultTick(%this) {
  cancel(%this.sch);


  %this.getCollection("bricks").recordData(getBrickcount());
  %this.getCollection("players").recordData(ClientGroup.getCount());


  %nextTime = %this.increments*mCeil(getRealTime()/%this.increments);
  %timeTo = %nextTime-getRealTime();
  %this.sch = %this.schedule(%timeTo, "defaultTick");
}
