
//================================================================
//= GlassServerGraphs                                            =
//================================================================

function GlassServerGraphing::init() {
  //all units in seconds
  //all of these should be prefs eventually
  new ScriptGroup(GlassServerGraphs) {
    increments = 15;
    history = 3600*12; //12 hours

    keepOpen = true;
  };

  GlassServerGraphs.loadDefault();
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

    %this.collections += 0;
    %collection = new ScriptObject() {
      class = "GlassCollection";
      name = %name;

      icon = "graph"; //default

      dataCt = 0;

      indexCt = 2;
      index0 = "time";
      index1 = "value";

      id = %this.collections;
    };
    %collection.listeners = new SimSet();
    %this.collections++;

    %this.schedule(1, add, %collection);

    %this.collection[%name] = %collection;
    %this.collection[%this.collections] = %collection;

    return %collection;
  }
}


//================================================================
//= GlassCollections                                             =
//================================================================


function GlassCollection::getFile(%this) {
  return "config/server/glass/collections/" @ strlwr(%this.name) @ ".dat";
}

function GlassCollection::onAdd(%this) {

  if(%this.loaded)
    return;
  else
    %this.loaded = true;

  %file = %this.getFile();

  if(isFile(%file)) {
    echo("loading " @ %file);
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
          %this.data[%dataCt, %key] = collapseEscape(getField(%line, %i));
          //data_12_time
          //data_12_value
        }

        //throw out old data
        %time = %this.data[%dataCt, "time"];
        if(%time !$= "" && strTimeCompare(getDateTime(), %time) > GlassServerGraphs.history) {
          continue; //by continuing without incrementing dataCt, will be overwritten
        }

        %dataCt++;
      }
    }
    %fo.close();
    %fo.delete();

    %this.dataCt = %dataCt;
    %this.indexCt = %indexCt;
  }

  %this.saveData();
}

function GlassCollection::saveData(%this) {
  if(!%this.loaded)
    return;

  if(isObject(%this.fo)) {
    %this.fo.close();
    %this.fo.delete();
  }

  %fo = new FileObject();
  %fo.openForWrite(%this.getFile());

  %line = "";
  for(%j = 0; %j < %this.indexCt; %j++) {
    if(%j > 0) {
      %line = %line @ ",";
    }

    %line = %line @ %this.index[%j];
  }
  %fo.writeLine(%line);

  for(%i = 0; %i < %this.dataCt; %i++) {
    %time = %this.data[%i, "time"];

    //throw out old data
    if(%time !$= "") {
      if(strTimeCompare(getDateTime(), %time) > GlassServerGraphs.history) {
        continue;
      }
    }

    %line = "";
    for(%j = 0; %j < %this.indexCt; %j++) {
      if(%j > 0) {
        %line = %line @ ",";
      }

      %line = %line @ expandEscape(%this.data[%i, %this.index[%j]]);
    }
    %fo.writeLine(%line);
  }

  %fo.close();
  %fo.delete();

  echo("Saved collection " @ %this.name);
}

function GlassCollection::recordData(%this, %value, %time) {
  if(%time $= "") {
    %time = getDateTime();
  }

  if(strTimeCompare(getDateTime(), %time) > GlassServerGraphs.history) {
    return; //too old
  }

  %this.data[%this.dataCt, "time"] = %time;
  %this.data[%this.dataCt, "value"] = %value;

  if(GlassServerGraphs.keepOpen) {
    if(!isObject(%this.fo)) {
      %fo = new FileObject();
      %fo.openForAppend(%this.getFile());
      %this.fo = %fo;
    }

    %this.fo.writeLine(%time @ "," @ %value);
  }

  %this.dataCt++;

  //transmit
  echo("Transmitting new data");
  for(%i = 0; %i < %this.listeners.getCount(); %i++) {
    %cl = %this.listeners.getObject(%i);
    commandToClient(%cl, 'GlassGraphData', %this.id, %time, %value, true);
  }
}


//================================================================
//= Default Graphs                                               =
//================================================================

function GlassServerGraphs::loadDefault(%this) {
  %bricks = %this.getCollection("Bricks");
  %bricks.icon = "brick";
  %bricks.color = "255 0 0";

  %players = %this.getCollection("Players");
  %players.icon = "user";
  %players.color = "0 255 0";

  %timeTo = (%this.increments)-(timeSeconds());

  if(%timeTo == 0) {
    %timeTo = (%this.increments);
  }

  %this.sch = %this.schedule(%timeTo*1000, "defaultTick");
}

function GlassServerGraphs::defaultTick(%this) {
  cancel(%this.sch);

  if(!$Server::Dedicated) {
    if(!isObject(ServerConnection)) {
      for(%i = 0; GlassServerGraphs.getCount(); %i++) {
        GlassServerGraphs.getObject(%i).saveData();
      }
      return;
    }
  }

  %this.getCollection("bricks").recordData(getBrickcount());
  %this.getCollection("players").recordData(ClientGroup.getCount());

  %timeTo = (%this.increments)-(timeSeconds());

  if(%timeTo < 1) {
    %timeTo = (%this.increments);
  }

  %this.sch = %this.schedule(%timeTo*1000, "defaultTick");
}


//================================================================
//= Communications                                               =
//================================================================

function GameConnection::sendGlassGraphs(%client) {
  //sends collection info
  commandToClient(%client, 'GlassGraphsClear');

  for(%i = 0; %i < GlassServerGraphs.getCount(); %i++) {
    %col = GlassServerGraphs.getObject(%i);
    commandToClient(%client, 'GlassGraphAdd', %i, %col.name, %col.icon);
  }

  commandToClient(%client, 'GlassGraphAddDone');
}

function serverCmdGlassGraphRequest(%client, %id, %ct) {
  if(!%client.isAdmin)
    return;

  if(%ct > 1000)
    %ct = 1000;

  if(%ct < 0) {
    %ct = 0;
  }

  %col = GlassServerGraphs.getObject(%id);

  if(!isObject(%col))
    return;

  if(%client._glassGraphListening !$= "") {
    GlassServerGraphs.getObject(%client._glassGraphListening).listeners.remove(%client);
  }
  %col.listeners.add(%client);

  %client._glassGraphListening = %id;

  commandToClient(%client, 'GlassGraphClearData');

  echo("Sending data from " @ %col.name);

  for(%i = %col.dataCt-%ct; %i < %col.dataCt; %i++) {
    if(%i < 0) {
      %i = 0;
    }

    commandToClient(%client, 'GlassGraphData', %col.data[%i-1, "time"], %col.data[%i-1, "value"]);
  }
}

//================================================================
//= Time                                                         =
//================================================================

function timeSeconds() {
  return getSubStr(getDateTime(), 15, 2);
}

//incredibly rough, returns seconds. date1 - date2
//only works in 1 year variance
function strTimeCompare(%datetime1, %datetime2) {
  %month[%a = 1] = 31; //jan
  %month[%a++]   = 28; //feb
  %month[%a++]   = 31; //march
  %month[%a++]   = 30; //april
  %month[%a++]   = 31; //may
  %month[%a++]   = 30; //june
  %month[%a++]   = 31; //july
  %month[%a++]   = 30; //august
  %month[%a++]   = 31; //sept
  %month[%a++]   = 30; //nov
  %month[%a++]   = 31; //dec

  %date1 = getWord(%datetime1, 0);
  %date2 = getWord(%datetime2, 0);

  %time1 = getWord(%datetime1, 1);
  %time2 = getWord(%datetime2, 1);

  %diff = 0;

  //seconds
  %s1 = getSubStr(%time1, 6, 2);
  %s2 = getSubStr(%time2, 6, 2);
  %diff += (%s1-%s2);

  //minutes
  %m1 = getSubStr(%time1, 3, 2);
  %m2 = getSubStr(%time2, 3, 2);
  %diff += (%m1-%m2)*60;

  //hours
  %h1 = getSubStr(%time1, 0, 2);
  %h2 = getSubStr(%time2, 0, 2);
  %diff += (%h1-%h2)*3600;

  //we'll adjust days to be from the beginning of the year
  //days
  %mo1 = getSubStr(%date1, 0, 2);
  %mo2 = getSubStr(%date2, 0, 2);
  %d1 = getSubStr(%date1, 3, 2);
  %d2 = getSubStr(%date2, 3, 2);
  %y1 = getSubStr(%date1, 6, 2);
  %y2 = getSubStr(%date2, 6, 2);

  if(mabs(%y1-%y2) > 1) {
    error("strTimeCompare given difference of > 1 year!");
    return "";
  }

  if(%y1 > %y2) {
    %d2 -= (%y2 % 4 == 0) ? 366 : 365;
  } else if(%y1 < %y2) {
    %d1 -= (%y1 % 4 == 0) ? 366 : 365;
  }

  for(%i = 1; %i < %mo1; %i++) {
    %d1 += %month[%i];

    if(%i == 2 && (%y1 % 4) == 0) {
      //leap year
      %d1 += 1;
    }
  }

  for(%i = 1; %i < %mo2; %i++) {
    %d2 += %month[%i];

    if(%i == 2 && %y2 % 4 == 0) {
      //leap year
      %d2 += 1;
    }
  }

  %diff += (%d1-%d2)*(24*60*60);


  return %diff;
}

package GlassServerGraphs {
  function onExit() {
    for(%i = 0; GlassServerGraphs.getCount(); %i++) {
      GlassServerGraphs.getObject(%i).saveData();
    }
    parent::onExit();
  }
};
activatePackage(GlassServerGraphs);
