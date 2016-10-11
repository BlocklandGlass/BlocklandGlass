if(!isObject(GlassServerList)) {
  new ScriptObject(GlassServerList);
}

if(!isObject(GlassServerListGui)) {
  exec("Add-Ons/System_BlocklandGlass/client/gui/GlassServerListGui.gui");
  exec("Add-Ons/System_BlocklandGlass/client/gui/JoinServerGui.gui");
}

function GlassServerList::doLiveUpdate(%this, %ip, %port, %key, %val) {
  if(!JS_Window.glassTouched)
    %this.modifyJoinGui();

  if(!isObject(ServerInfoGroup))
    return;

  for(%i = 0; %i < ServerInfoGroup.getCount(); %i++) {
    %obj = ServerInfoGroup.getObject(%i);
    if(%obj.ip $= (%ip@":"@ %port)) {
      %serverSo = %obj;
      break;
    }
  }

  if(%serverSO $= "") {
    //echo("couldnt find serverso for " @ %ip @ ":" @ %port);
    return;
  }

  if(%key $= "players") {
    %serverSO.currPlayers = %val;
    %serverSO.display();
  } else if(%key $= "maxPlayers") {
    %serverSO.maxPlayers = %val;
    %serverSO.display();
  } else if(%key $= "brickCount") {
    %serverSO.brickcount = %val;
    %serverSO.display();
  } else if(%key $= "name") {
    %serverSO.brickcount = %val;
    %serverSO.display();
  } else if(%key $= "passworded") {
    %serverSO.pass = (%val ? "Yes" : "No");
    %serverSO.display();
  } else if(%key $= "hasGlass") {
    %serverSO.hasGlass = %val;
    %serverSO.display();
  }
}

function GlassServerList::modifyJoinGui(%this) {
  if(isObject(JS_window_glass)) {
    JS_Window.delete();
    JS_Window_glass.setName("JS_Window");
    JoinServerGui.add(JS_Window);
  }

  JS_Window.glassTouched = true;
}

function GlassServerList::display(%this, %obj) {
  if(!JS_Window.glassTouched)
    %this.modifyJoinGui();
  return;
  if(!isObject(%this.listing[%obj.addr]))
    %this.addServer(%obj);
}

function GlassServerList::addServer(%this, %obj) {
  if(isObject(%obj.glassTcp)) {
    %obj.glassTcp.delete();
    return;
  }

  return;
  %obj.glassTcp = new TCPObject(GlassServerTCP);
  %obj.glassTcp.serverSO = %obj;
  echo("connecting to " @ %obj.ip);
  GlassServerTCP.connect(%obj.ip);

  %bar = %this.createServerBar(%obj);
  GlassServerListGui_ScrollSwatch.add(%bar);
  %bar.placeBelow(GlassServerListGui_ScrollSwatch.getObject(GlassServerListGui_ScrollSwatch.getCount()-2), 5);
  GlassServerListGui_ScrollSwatch.verticalMatchChildren(0, 10);

  %obj.glassTcp.swatch = %bar;
  %obj.glassTcp.serverSo.swatch = %bar;
}

function GlassServerList::clearAll(%this) {
  GlassServerListGui_ScrollSwatch.deleteAll();
}

function GlassServerList::doQuery(%this) {
  if(GlassLiveConnection.connected) {
    %obj = JettisonObject();
    %obj.set("type", "string", "queryServerList");

    GlassLiveConnection.send(jettisonStringify("object", %obj) @ "\r\n");
  }
}

function GlassServerList::createServerBar(%this, %obj) {
  %swatch = new GuiSwatchCtrl() {
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "580 25";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    color = "200 200 200 255";
  };

  %swatch.text = new GuiMLTextCtrl() {
    profile = "GuiMLTextProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "5 5";
    extent = "570 16";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lineSpacing = "2";
    allowColorChars = "0";
    maxChars = "-1";
    maxBitmapHeight = "-1";
    selectable = "1";
    autoResize = "1";
    text = "<tab:400, 465><font:verdana bold:15>" @ %obj.name @ "\t<font:verdana:15>" @ %obj.currPlayers @ "/" @ %obj.maxPlayers @ "\tCustom\t<just:right><bitmap:Add-Ons/System_BlocklandGlass/image/icon/comment>";
  };
  %swatch.add(%swatch.text);
  return %swatch;
}

package GlassServerList {
  function ServerSO::display(%this) {
    parent::display(%this);
    GlassServerList.display(%this);
    //echo(%this);
  }

  function JoinServerGui::joinServer(%gui) {
    parent::joinServer(%gui);
    for(%i = 0; %i < ServerInfoGroup.getCount(); %i++) {
      %sso = ServerInfoGroup.getObject(%i);
      if(isObject(%sso.glassTcp))
        %sso.glassTcp.disconnect();
    }
  }

  function Canvas::pushDialog(%canvas, %dlg) {
    if(%dlg.getName() $= "JoinServerGui") {
      if(!$Glass::SL) {
        //canvas.pushDialog(GlassServerListGui);
      }

      //glassMessageBoxYesNo("Glass Server List", "Hey there! Do you want to use the Glass server list? Here's some neat features:<br><br>+ Live updating list (no refreshing)<br>+ Detailed server information<br>+ Other stuff");
    }

    parent::pushDialog(%canvas, %dlg);
  }

  function ServerInfoSO_ClearAll() {
    parent::ServerInfoSO_ClearAll();
    GlassServerList.clearAll();
    GlassServerList.doQuery();
  }

  function ServerSO::serialize(%this) {
    %str = parent::serialize(%this);
    //echo("bef:" @ %str);
    %str = setField(%str, 9, (%this.hasGlass ? "Yes" : "No") TAB getField(%str, 9));
    //echo("aft:" @ %str);
    return %str;
  }

  function JS_sortList(%id) {
    if(%id > 9) {
      %id++;
    }
    return parent::JS_sortList(%id);
  }
};
activatePackage(GlassServerList);
