function GlassServers::init() {
  GlassFavoriteServers::changeGui();
  GlassLoading::changeGui();

  new ScriptObject(GlassFavoriteServers);
  %this = GlassFavoriteServers;

  %favs = GlassSettings.get("Servers::Favorites");
  %this.favorites = 0;
  for(%i = 0; %i < getFieldCount(%favs); %i++) {
    %this.favorite[%this.favorites] = getField(%favs, %i);
    %this.favorites++;
  }

  GlassFavoriteServers.buildSwatches();
  GlassFavoriteServers.load();
}

//====================================
// Favorite Servers
//====================================

function GlassFavoriteServers::changeGui() {
  if(!isObject(GlassFavoriteServerSwatch)) {
    exec("./gui/elements/GlassHighlightSwatch.cs");
    exec("./gui/GlassFavoriteServer.gui");
  }

  MainMenuButtonsGui.add(GlassFavoriteServerSwatch);
}

function GlassFavoriteServers::addFavorite(%this, %username) {
  if(trim(%username) $= "")
    return;

  %favs = GlassSettings.get("Servers::Favorites");
  for(%i = 0; %i < getFieldCount(%favs); %i++) {
    if(getField(%favs, %i) $= %username) {
      glassMessageBoxOk("Already a favorite!", "You've already favorited this host!");
      return;
    }
  }

  GlassSettings.update("Servers::Favorites", trim(%favs TAB %username));

  %this.favorite[%this.favorites] = %username;
  %this.favorites++;

  %this.buildSwatches();
  %this.load();
}

function GlassFavoriteServers::load(%this) {

  for(%i = 0; %i < %this.favorites; %i++) {
    %this.isFavorite[getHostName(%this.favorite[%i])] = true;
  }

  connectToUrl("master2.blockland.us", "GET", "", "GlassFavoriteServersTCP");
}

function GlassFavoriteServers::buildSwatches(%this) {

  for(%i = 0; %i < GlassFavoriteServerSwatch.getCount(); %i++) {
    %obj = GlassFavoriteServerSwatch.getObject(%i);
    if(%obj.getName() !$= "GlassFavoriteServerGui_Text") {
      %obj.deleteAll();
      %obj.delete();
      %i--;
    }
  }

  for(%i = 0; %i < %this.favorites; %i++) {
    %swatch = new GuiSwatchCtrl("GlassFavoriteServerGui_Swatch" @ %i) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 0";
      extent = "270 47";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      color = "220 220 220 255";
    };

    %swatch.text = new GuiMLTextCtrl() {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "250 27";
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
    };

    %swatch.add(%swatch.text);

    %swatch.text.setText("<font:verdana bold:15>" @ getHostName(%this.favorite[%i]) @ " Server<br><just:center><font:verdana:13>Loading...");

    GlassHighlightSwatch::addToSwatch(%swatch, "0 0 0 0", "GlassFavoriteServers::interact");

    GlassFavoriteServerSwatch.add(%swatch);

    if(%placeBelow $= "")
      %swatch.placeBelow(GlassFavoriteServerGui_Text, 10);
    else
      %swatch.placeBelow(%placeBelow, 5);

    %placeBelow = %swatch;
  }

  GlassFavoriteServerSwatch.verticalMatchChildren(24, 10);
  GlassFavoriteServerSwatch.position = vectorSub(MainMenuButtonsGui.extent, GlassFavoriteServerSwatch.extent);
}

function GlassFavoriteServers::renderServer(%this, %status, %id, %title, %players, %maxPlayers, %map, %addr) {
  %swatch = "GlassFavoriteServerGui_Swatch" @ %id;
  //if(%swatch.text $= "")
    %swatch.text = %swatch.getObject(0);

  %swatch.server = new ScriptObject() {
    name = trim(%title);
    pass = (%status $= "passworded" ? "Yes" : "No");
    currPlayers = %players;
    maxPlayers = %maxPlayers;

    ip = %addr;
  };

  switch$(%status) {
    case "online":
      %swatch.color = "131 195 243 255";
      %swatch.text.setText("<font:verdana bold:15>" @ %title @ "<br><font:verdana:13>" @ %players @ "/" @ %maxPlayers @ " Players<just:right>" @ %map);

    case "passworded":
      %swatch.color = "235 153 80 255";
      %swatch.text.setText("<font:verdana bold:15>" @ trim(%title) @ " <font:verdana:13>(Passworded)<br>" @ %players @ "/" @ %maxPlayers @ " Players<just:right>" @ %map);

    case "offline":
      %swatch.color = "220 220 220 255";
      %swatch.text.setText("<font:verdana bold:15>" @ %title @ "<br><font:verdana:13>Offline");
  }

  %swatch.ocolor = %swatch.color;
  %swatch.hcolor = %swatch.color;
  %swatch.pushToBack(%swatch.glassHighlight);
}

function GlassFavoriteServers::interact(%swatch) {
  %server = %swatch.server;
  GlassServerPreviewGui.open(%server);
}

function getHostName(%name) {
  if(getSubStr(%name, strlen(%name)-1, 1) $= "s") {
    return %name @ "'";
  } else {
    return %name @ "'s";
  }
}

function getNameFromHost(%name) {
  if(getSubStr(%name, strlen(%name)-2, 2) $= "'s") {
    return getSubStr(%name, 0, strlen(%name)-2);
  } else {
    return getSubStr(%name, 0, strlen(%name)-1);
  }
}

function GlassFavoriteServersTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassFavoriteServersTCP::onDone(%this, %err) {
  if(%err) {
    GlassFavoriteServers.renderError(%err);
  } else {
    for(%i = 0; %i < getLineCount(%this.buffer); %i++) {
      %line = getLine(%this.buffer, %i);
      %serverName = getField(%line, 4);

      for(%j = 0; %j < GlassFavoriteServers.favorites; %j++) {
        if(strpos(%serverName, getHostName(GlassFavoriteServers.favorite[%j])) == 0) {
          %players = getField(%line, 5);
          %maxPlayers = getField(%line, 6);
          %passworded = getField(%line, 2);
          %map = getField(%line, 7);

          %addr = getField(%line, 0) @ ":" @ getField(%line, 1);

          %foundServer[%j] = true;

          GlassFavoriteServers.renderServer((%passworded ? "passworded" : "online"), %j, %serverName, %players, %maxPlayers, %map, %addr);
        }
      }
    }

    for(%j = 0; %j < GlassFavoriteServers.favorites; %j++) {
      if(!%foundServer[%j]) {
        %serverName = getHostName(GlassFavoriteServers.favorite[%j]) @ " Server";
        GlassFavoriteServers.renderServer("offline", %j, %serverName);
      }
    }
  }
}

//====================================
// LoadingGui
//====================================

function GlassLoading::changeGui() {
  if(LoadingGui.isGlass) {
    return;
  }

  %loadingGui = LoadingGui.getId();

  exec("./gui/LoadingGui.gui");

  %loadingGui.deleteAll();
  %loadingGui.delete();

  LoadingGui.isGlass = true;
}

function GlassLoadingGui::updateWindowTitle(%this) {
  %npl = NPL_Window.getValue();
  %name = trim(getSubStr(%npl, strPos(%npl, "-") + 2, strLen(%npl)));

  if(%name !$= "")
    %text = "Joining \"" @ %name @ "\"";
  else
    %text = "Joining Server";

  %this.setText(%text);
}

function GlassLoadingGui::onWake(%this) {
  GlassLoadingGui_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  GlassServerPreview::getServerBuild(ServerConnection.getAddress(), GlassLoadingGui_Image);
}

//====================================
// ServerPreview
//====================================

function GlassServerPreviewGui::open(%this, %server) {
  if(%server $= "") {
    %server = ServerInfoGroup.getObject(JS_ServerList.getSelectedID());
  }
  %this.server = %server;
  if(joinServerGui.isAwake()) {
    canvas.popDialog(joinServerGui);
    %this.wakeServerGui = true;
  }
  canvas.pushDialog(GlassServerPreviewGui);
}

function GlassServerPreviewGui::close(%this) {
  canvas.popDialog(GlassServerPreviewGui);
  if(%this.wakeServerGui) {
    canvas.pushDialog(joinServerGui);
    %this.wakeServerGui = false;
  }
}

function GlassServerPreviewGui::onWake(%this) {
  GlassServerPreviewWindowGui.forceCenter();

  %server = %this.server;

  if(!isObject(%server))
	  return;

  if(%server.pass !$= "No")
    %img = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/lock>";

  GlassServerPreview_Name.setText("<font:verdana bold:18>" @ trim(%server.name) SPC %img @ "<br><font:verdana:15>" @ %server.currPlayers @ "/" @ %server.maxPlayers SPC "Players");
  GlassServerPreview_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  GlassServerPreview_Playerlist.clear();
  GlassServerPreview::getServerInfo(%server.ip);

  GlassServerPreview::getServerBuild(%server.ip, GlassServerPreview_Preview);
}

function GlassServerPreview::getServerBuild(%addr, %obj) {
  %addr = strReplace(%addr, ".", "-");
  %addr = strReplace(%addr, ":", "_");
  %url = "http://image.blockland.us/detail/" @ %addr @ ".jpg";
  %method = "GET";
  %downloadPath = "config/client/BLG/ServerPreview.jpg";
  %className = "GlassServerPreviewTCP";

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
  %tcp.bitmap = %obj;
}

function GlassServerPreviewTCP::onDone(%this, %error) {
  if(%error) {
    echo("ERROR:" SPC %error);
  }

  %this.bitmap.setBitmap("config/client/BLG/ServerPreview.jpg");
}

function GlassServerPreview::getServerInfo(%addr) {
  %idx = strpos(%addr, ":");

  %ip = getSubStr(%addr, 0, %idx);
  %port = getSubStr(%addr, %idx+1, strlen(%addr));

  %url = "http://api.blocklandglass.com/api/2/serverStats.php?ip=" @ urlEnc(%ip) @ "&port=" @ urlEnc(%port);
  %method = "GET";
  %downloadPath = "";
  %className = "GlassServerPreviewPlayerTCP";

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
}

function GlassServerPreviewPlayerTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassServerPreviewPlayerTCP::onDone(%this, %error) {
  if(%error) {
    echo("ERROR:" SPC %error);
  } else {
    %err = jettisonParse(%this.buffer);
    if(%err) {
      //parse error, $JSON::Error
      return;
    }

    %result = $JSON::Value;

    if(%result.status $= "error") {
      GlassServerPreview_Playerlist.clear();
      GlassServerPreview_noGlass.setVisible(true);
    } else {
      GlassServerPreview_noGlass.setVisible(false);

      %playerCount = %result.Clients.length;

      if(%result.clients.value[0].name $= "") // empty serv
        return;

      for(%i=0; %i < %playerCount; %i++) {
        %cl = %result.clients.value[%i];
        GlassServerPreview_Playerlist.addRow(%cl.blid, %cl.status TAB %cl.name TAB %cl.blid);
      }
    }
  }
}

function joinServerGui::preview(%this) {
  if(JS_ServerList.getSelectedID() == -1)
	  return;

  GlassServerPreviewGui.open();
}

package GlassServers {
  function joinServerGui::onWake(%this) {
  	if(!%this.initializedGlass) {
  	  %this.initializedGlass = 1;
  	  joinServerGui.clear();
  	  joinServerGui.add(GlassJS_window);
  	  GlassJS_window.setName("JS_window");
  	}
  	parent::onWake(%this);
  }

  function NPL_List::addRow(%this, %id, %val) {
    GlassLoadingGui_UserList.addRow(%id, %val);
    return parent::addRow(%this, %id, %val);
  }

  function NPL_List::sort(%this, %a) {
    parent::sort(%this, %a);
  }

  function NPL_List::sortNumerical(%this, %a) {
    parent::sortNumerical(%this, %a);
  }

  function NPL_List::clear(%this) {
    GlassLoadingGui_UserList.clear();
    parent::clear(%this);
  }

  function MainMenuButtonsGui::onWake(%this) {
    GlassFavoriteServers.load();

    if(isFunction(%this, onWake))
      parent::onWake(%this);
  }

  function LoadingGui::onWake(%this) {
    if(isFunction(LoadingGui, onWake))
      parent::onWake(%this);

    LoadingGui.pushToBack(GlassLoadingGui);
  }
  
  function NewPlayerListGui::UpdateWindowTitle(%gui) {
    parent::UpdateWindowTitle(%gui);
    
    GlassLoadingGui.updateWindowTitle();
  }
};
activatePackage(GlassServers);
