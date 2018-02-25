function GlassServers::init() {
  if(GlassSettings.get("Servers::LoadingGUI"))
    GlassLoading::changeGui();

  if(!GlassSettings.get("Servers::EnableFavorites")) {
    if(GlassFavoriteServerSwatch.visible) {
      GlassServerPreview_Favorite.setVisible(false);
      GlassFavoriteServerSwatch.setVisible(false);
    }
    return;
  }

  GlassServerPreview_Favorite.setVisible(true);
  GlassFavoriteServers::changeGui();

  new ScriptObject(GlassFavoriteServers);
  %this = GlassFavoriteServers;

  %favs = GlassSettings.get("Servers::Favorites");
  %this.favorites = 0;
  for(%i = 0; %i < getFieldCount(%favs); %i++) {
    %this.favorite[%this.favorites] = getField(%favs, %i);
    %this.favorites++;
  }

  GlassFavoriteServers.scanServers();
}

//====================================
// Favorite Servers
//====================================

function GlassFavoriteServers::changeGui() {
  if(!isObject(GlassFavoriteServerSwatch)) {
    exec("./gui/GlassFavoriteServer.gui");
  }

  MainMenuButtonsGui.add(GlassFavoriteServerSwatch);
}

function GlassFavoriteServers::toggleFavorite(%this, %ip) {
  if(%ip $= "")
    return;

  %favs = GlassSettings.get("Servers::Favorites");

  for(%i = 0; %i < getFieldCount(%favs); %i++) {
    if(getField(%favs, %i) $= %ip) {
      GlassSettings.update("Servers::Favorites", removeField(%favs, %i));
      glassMessageBoxOk("Favorite Removed", "This server has been removed from your favorites!");
      GlassServerPreview_Favorite.mColor = "46 204 113 220";
      GlassServerPreview_Favorite.setText("Add Favorite");
      GlassServers::init();
      return;
    }
  }

  glassMessageBoxOk("Favorite Added", "This server has been added to your favorites!");
  GlassSettings.update("Servers::Favorites", trim(%favs TAB %ip));
  GlassServerPreview_Favorite.mColor = "231 76 60 220";
  GlassServerPreview_Favorite.setText("Remove Favorite");
  GlassServers::init();
}


function GlassFavoriteServers::buildList(%this) {
  for(%i = 0; %i < GlassFavoriteServerSwatch.getCount(); %i++) {
    %obj = GlassFavoriteServerSwatch.getObject(%i);
	  %name = %obj.getName();

    if(%name !$= "GlassFavoriteServerGui_Text" && %name !$= "GlassFavoriteServerGui_Tutorial" && %name !$= "GlassFavoriteServerGui_NoServers") {
      %obj.deleteAll();
      %obj.delete();
      %i--;
    }
  }

  for(%i = 1; %i <= %this.onlineFavoriteCount; %i++) {
    %swatch = new GuiSwatchCtrl("GlassFavoriteServerGui_Swatch" @ %i) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 35";
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
      position = "7 7";
      extent = "256 37";
      minExtent = "8 2";
      enabled = "1";
      visible = "1";
      clipToParent = "1";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
      selectable = "1";
      autoResize = "1";

      maxBitmapHeight = "13";
    };

    %swatch.add(%swatch.text);
    %swatch.text.setText("<font:verdana bold:15>" @ %this.favorite[%i] @ "<br><just:center><font:verdana:13>Loading...");

    GlassHighlightSwatch::addToSwatch(%swatch, "0 0 0 0", "GlassFavoriteServers::interact");
    GlassFavoriteServerSwatch.add(%swatch);

    if(GlassFavoriteServerSwatch.isAwake()) {
      %swatch.text.forceReflow();
    }

    %swatch.verticalMatchChildren(12, 5);

    if(%placeBelow)
      %swatch.placeBelow(%placeBelow, 5);

    %placeBelow = %swatch;

  	%server = GlassFavoriteServers.onlineFavorite[%i];

  	%passworded = getField(%server, 3);
  	GlassFavoriteServers.renderServer((%passworded ? "passworded" : "online"), %i, getField(%server, 2), getField(%server, 4), getField(%server, 5), getField(%server, 6), getField(%server, 0) @ getField(%server, 1));
  }

  if(%this.favorites $= "" || %this.favorites == 0) {
    GlassFavoriteServerSwatch.extent = "290 60";
  	GlassFavoriteServerGui_Tutorial.setVisible(true);
  	GlassFavoriteServerGui_NoServers.setVisible(false);
  } else if(%this.onlineFavoriteCount == 0) {
	  GlassFavoriteServerGui_NoServers.setVisible(true);
	  GlassFavoriteServerGui_Tutorial.setVisible(false);
  } else {
  	GlassFavoriteServerGui_Tutorial.setVisible(false);
  	GlassFavoriteServerGui_NoServers.setVisible(false);
  }
  GlassFavoriteServerSwatch.verticalMatchChildren(24, 10);
  GlassFavoriteServerSwatch.position = vectorSub(MainMenuButtonsGui.extent, GlassFavoriteServerSwatch.extent);
  if(!GlassFavoriteServerSwatch.visible)
	GlassFavoriteServerSwatch.setVisible(true);
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

    offline = %status $= "offline";
  };

  switch$(%status) {
    case "online":
      %swatch.color = "131 195 243 255";
      %swatch.text.setText("<font:verdana bold:15>" @ %title @ "<br><font:verdana:13>" @ %players @ "/" @ %maxPlayers @ " Players<br><just:right>" @ %map);

    case "passworded":
      %swatch.color = "235 153 80 255";
      %swatch.text.setText("<font:verdana bold:15>" @ trim(%title) @ " <font:verdana:13><bitmap:Add-Ons/System_BlocklandGlass/image/icon/lock><br>" @ %players @ "/" @ %maxPlayers @ " Players<br><just:right>" @ %map);

    case "offline":
      %swatch.color = "220 220 220 255";
      %swatch.text.setText("<font:verdana bold:15>" @ %title @ "<br><font:verdana:13>Offline");
  }

  %swatch.ocolor = %swatch.color;
  %swatch.hcolor = %swatch.color;
  %swatch.pushToBack(%swatch.glassHighlight);

  if(GlassFavoriteServerSwatch.isAwake())
    %swatch.text.forceReflow();

  %swatch.verticalMatchChildren(40, 5);
  for(%i = 1; %i <= %this.onlineFavoriteCount; %i++) {
    %swatch = "GlassFavoriteServerGui_Swatch" @ %i;
    if(%last)
      %swatch.placeBelow(%last, 5);

    %last = %swatch;
  }
}

function GlassFavoriteServers::scanServers() {
	if(!isObject(GlassFavoriteServers) || !GlassSettings.get("Servers::EnableFavorites"))
	  return;

  connectToUrl("master2.blockland.us", "GET", "", "GlassFavoriteServersTCP");
}

function GlassFavoriteServers::interact(%swatch) {
  GlassServerPreviewGui.open(%swatch.server);
}

function GlassFavoriteServersTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassFavoriteServersTCP::onDone(%this, %err) {
  if(%err) {
    GlassFavoriteServers.renderError(%err);
  } else {
	%onlineCount = 0;
	for(%i = 0; %i < getLineCount(%this.buffer); %i++) {
      %line = getLine(%this.buffer, %i);
	  %serverIP = trim(getField(%line, 0) @ ":" @ getField(%line, 1));

	  for(%j = 0; %j < GlassFavoriteServers.favorites; %j++) {
	    %fav = GlassFavoriteServers.favorite[%j];
		if(%fav $= %serverIP) {
		  %passworded = getField(%line, 2);
		  if(!GlassSettings.get("Servers::DisplayPasswordedFavorites") && %passworded)
			  continue;

		  %serverName = getField(%line, 4);
		  %players = getField(%line, 5);
          %maxPlayers = getField(%line, 6);
          %map = getField(%line, 7);

		  GlassFavoriteServers.onlineFavorite[%onlineCount++] = %serverIP TAB %serverPort TAB %serverName TAB %passworded TAB %players TAB %maxPlayers TAB %map;
		}
	  }
	}
	GlassFavoriteServers.onlineFavoriteCount = %onlineCount;
	GlassFavoriteServers.buildList();
  }
}


//====================================
// LoadingGui
//====================================

function GlassLoading::changeGui() {
  if(LoadingGui.isGlass) {
    return;
  }

  exec("./gui/GlassLoadingGui.gui");
  LoadingGui.isGlass = true;
}

function GlassLoadingGui_UserList::update(%this) {
  %this.clear();

  for(%i = 0; %i < NPL_List.rowCount(); %i++) {
    %row = getFields(NPL_List.getRowText(%i), 0, 3);
    %id = NPL_List.getRowId(%i);

    GlassLoadingGui_UserList.addRow(%id, %row);
  }

  NewPlayerListGui.UpdateWindowTitle();
}

function GlassLoadingGui::UpdateWindowTitle(%this) {
  %npl = NPL_Window.getValue();

  %this.setText(%npl);
}

function GlassLoadingGui_UserList::onSelect(%this, %a) {
  NewChatHud.pushToBack(GlassLoadingGui);
  NewChatHud.schedule(50, pushToBack, newChatText);
}

function GlassLoadingGui::onWake(%this) {
  GlassLoadingGui_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");

  if(isObject(ServerConnection)) {
    GlassServerPreview::getServerBuild(ServerConnection.getAddress(), GlassLoadingGui_Image);
  } else {
    GlassServerPreview::getServerBuild("local", GlassLoadingGui_Image);
  }

  GlassLoadingGui.add(LoadingProgress);
  LoadingProgress.position = "15 335";
  LoadingProgress.extent = "590 25";

  GlassLoadingGui.add(LoadingSecondaryProgress);
  LoadingSecondaryProgress.position = "15 335";
  LoadingSecondaryProgress.extent = "590 25";
}

function GlassLoadingGui::onSleep(%this) {
  GlassLoadingGui_Image.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");

  %this.setText("Waiting");
  GlassLoadingGui_UserList.clear();
}

function GlassLoadingGui::close(%this) {
  %pos = 20 SPC (getWord(LoadingGui.extent, 1)-84);
  %ext = (getWord(LoadingGui.extent, 0)-40) SPC 64;

  LoadingGui.add(LoadingProgress);
  LoadingProgress.position = %pos;
  LoadingProgress.extent = %ext;

  LoadingGui.add(LoadingSecondaryProgress);
  LoadingSecondaryProgress.position = %pos;
  LoadingSecondaryProgress.extent = %ext;

  GuiGroup.add(%this);
  LoadingProgressTxt.forceCenter();
}

function GuiTextListCtrl::sortList(%this, %col) {
  if(%this.sortedBy == %col)
  	%this.sortedAsc = !%this.sortedAsc;
  else
  	%this.sortedAsc = 1;

  %this.sortedBy = %col;
  %this.sort(%this.sortedBy, %this.sortedAsc);

}

function GuiTextListCtrl::sortNumList(%this, %col) {
  if(%this.sortedBy == %col)
	%this.sortedAsc = !%this.sortedAsc;
  else
	  %this.sortedAsc = 0;

  %this.sortedBy = %col;
  %this.sortNumerical(%this.sortedBy, %this.sortedAsc);

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

function getServerFromIP(%ip) {
  for(%i=0; %i < ServerInfoGroup.getCount(); %i++) {
    %search = ServerInfoGroup.getObject(%i);
    if(%search.ip $= %ip)
      return %search;
  }
  return -1;
}

function GlassServerPreviewGui::onWake(%this) {
  GlassServerPreviewWindowGui.forceCenter();

  %server = %this.server;

  if(!isObject(%server))
	  return;

  if(%server.pass !$= "No") {
    %img = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/lock>";
    GlassServerPreview_Connect.mColor = "235 153 80 220";
  } else if(%server.currPlayers >= %server.maxPlayers) {
    %img = "<bitmap:Add-Ons/System_BlocklandGlass/image/icon/group_error>";
    GlassServerPreview_Connect.mColor = "237 118 105 220";
  } else {
    GlassServerPreview_Connect.mColor = "84 217 140 220";
  }

  GlassServerPreview_Name.setText("<font:verdana bold:18>" @ trim(%server.name) SPC %img @ "<br><font:verdana:15>" @ %server.currPlayers @ "/" @ %server.maxPlayers SPC "Players");
  GlassServerPreview_Preview.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  GlassServerPreview_Playerlist.clear();
  GlassServerPreview::getServerInfo(%server.ip);
  GlassServerPreviewWindowGui.openServerIP = %server.ip;
  GlassServerPreviewWindowGui.openServerName = %server.name;
  GlassServerPreviewWindowGui.passworded = (%server.pass $= "Yes");

  GlassServerPreview::getServerBuild(%server.ip, GlassServerPreview_Preview);

  if(GlassSettings.get("Servers::EnableFavorites")) {
    for(%i = 0; %i < GlassFavoriteServers.favorites; %i++) {
        %fav = GlassFavoriteServers.favorite[%i];

      if(%fav $= %server.ip) {
        GlassServerPreview_Favorite.mColor = "231 76 60 220";
        GlassServerPreview_Favorite.setText("Remove Favorite");
        break;
      } else {
        GlassServerPreview_Favorite.mColor = "46 204 113 220";
        GlassServerPreview_Favorite.setText("Add Favorite");
      }
    }
  }
}

function GlassServerPreview::connectToServer(%password) {
  %server = GlassServerPreviewWindowGui.openServerIP;
  if(%server $= "")
    return;

  if(GlassServerPreviewWindowGui.passworded && %password $= "") {
    $ServerInfo::Address = %server;
    canvas.pushDialog(JoinServerPassGui);
    return;
  }

  if(isObject(serverConnection))
    disconnectedCleanup();

  connectToServer(%server, %password, "1", "1");
  GlassServerPreviewGui.close();
}


function GlassServerPreview::getServerBuild(%addr, %obj) {
  fileDelete("config/client/BLG/ServerPreview.jpg");

  if(%addr $= "local") {
    %obj.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
    return;
  }

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
  if(!%error) {
    if(isFile("config/client/BLG/ServerPreview.jpg"))
      %this.bitmap.setBitmap("config/client/BLG/ServerPreview.jpg");
    else
      %this.bitmap.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  }
}

function GlassServerPreview::getServerInfo(%addr) {
  %idx = strpos(%addr, ":");

  %ip = getSubStr(%addr, 0, %idx);
  %port = getSubStr(%addr, %idx+1, strlen(%addr));

  %url = "ip=" @ urlEnc(%ip) @ "&port=" @ urlEnc(%port);
  %tcp = GlassApi.request("serverStats", %url, "GlassServerPreviewPlayerTCP");

}

function GlassServerPreviewPlayerTCP::handleText(%this, %text) {
  %this.buffer = %this.buffer NL %text;
}

function GlassServerPreviewPlayerTCP::onDone(%this, %error) {
  if(%error) {
    GlassLog::error("Server Preview error: " @ %error);
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

        if(%cl.status $= "")
          %cl.status = "-";

        GlassServerPreview_Playerlist.addRow(%cl.blid, "  " @ %cl.status TAB getASCIIString(%cl.name) TAB %cl.blid);
      }
    }
  }
}

function joinServerGui::preview(%this) {
  if(JS_ServerList.getSelectedID() == -1)
	  return;

  GlassServerPreviewGui.open();
}

function clientCmdGlass_setLoadingBackground(%url, %fileType, %crc, %showPlayerlist) {
  if(!GlassSettings.get("Servers::LoadingImages"))
	  return;

  if(PlayGui.isAwake())
    return;

  if(LoadingGUI.lastDownload + 2 > $Sim::Time)
	  return;

  if(!%showPlayerlist)
    GlassLoadingGui.close();

  LoadingGUI.lastDownload = $Sim::Time;

  if(%fileType !$= "jpg" && %fileType !$= "png" && %fileType !$= "jpeg") {
  	GlassLog::error("\c2Server sent illegal LoadingGui image format");
  	return;
  }

  %method = "GET";
  %downloadPath = "config/client/BLG/loadingImages/" @ sha1(%url) @ "." @ %fileType;
  %className = "GlassServerBackgroundTCP";

  GlassLog::debug("Downloading loading image from " @ %url @ " to " @ %downloadPath);

  if(isFile(%downloadPath)) {
    if(%crc $= "" || getFileCRC(%downloadPath) $= %crc) {
      GlassLog::debug("Map picture was found locally!");
      LOAD_MapPicture.setBitmap(%downloadPath);
      return;
    }
  }

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
  %tcp.fileType = %fileType;
  %tcp.imageLocation = %downloadPath;
}

function GlassServerBackgroundTCP::onBinChunk(%this, %chunk) {
	if(%chunk >= 2000000) {
		warn("Error - GlassServerBackgroundTCP file is greater than 2mb, stopping download.");
		%this.disconnect();
	}
}

function GlassServerBackgroundTCP::onDone(%this, %error) {
  if(%error) {
    GlassLog::error("GlassServerBackgroundTCP error:" SPC %error);
    return;
  }

  LOAD_MapPicture.setBitmap("base/client/ui/loadingBG");

  if(isFile(%this.imageLocation))
    LOAD_MapPicture.setBitmap(%this.imageLocation);
}

//====================================
// PlayerList
//====================================

PlayerListProfile.fontColors[6] = "244 67 54 255"; // Red
PlayerListProfile.fontColors[7] = "0 90 0 255"; // Green
PlayerListProfile.fontColors[8] = "230 81 0 255"; // Orange
PlayerListProfile.fontColors[9] = "0 121 107 255"; // Teal

function clientCmdGlass_setPlayerlistStatus(%blid, %char, %color) {
  if(strLen(%char) > 1)
    return;

  if(%color $= "")
    %color = 1;

  switch(%color) {
    case 0:
      %color = "\c0";
    case 1:
      %color = "\c1";
    case 2:
      %color = "\c2";
    case 3:
      %color = "\c3";
    case 4:
      %color = "\c4";
    case 5:
      %color = "\c5";
    case 6:
      %color = "\c6";
    case 7:
      %color = "\c7";
    case 8:
      %color = "\c8";
    case 9:
      %color = "\c9";
    default:
      return;
  }

  for(%i = 0; %i < NPL_List.rowCount(); %i++) {
    %row = NPL_List.getRowText(%i);
    %id = NPL_List.getRowId(%i);

    if(getField(%row, 3) $= %blid)
      NPL_List.setRowById(%id, setField(%row, 0, %color @ %char));
  }
}

function Glass::replaceJSWindow() {
  %this = JoinServerGui;
  if(!%this.initializedGlass) {
    %this.initializedGlass = 1;
    joinServerGui.clear();
    joinServerGui.add(GlassJS_window);
    GlassJS_window.setName("JS_window");
  }
}
Glass::replaceJSWindow();

//====================================
// Promoted Events
//====================================

function GlassServers::setEvent(%addr, %title, %desc) {
  $GlassServers::Event::Addr = %addr;
  $GlassServers::Event::Title = %title;
  $GlassServers::Event::Desc = %desc;

  //loop current ServerSO's to put current info

  //should package Master Server Query/ServerSO results to update gui
}

function GlassServers::toggleEvent(%tog) {
  if(%tog !$= "" && JS_window._showingEvent+0 == %tog+0) return;

  %scale = 0 SPC (JS_window._showingEvent ? -1 : 1)*60;

  %sort = "gamemode bricks ping players pass servername ded";
  for(%i = 0; %i < getWordCount(%sort); %i++) {
    %label = getWord(%sort, %i);
    %obj = "JS_sort_" @ %label;
    %obj.position = vectorAdd(%obj.position, %scale);
  }

  JS_scroll.extent = vectorSub(JS_scroll.extent, %scale);
  JS_scroll.position = vectorAdd(JS_scroll.position, %scale);

  if(!JS_window._showingEvent) {
    %swatch = new GuiSwatchCtrl(GlassEventSwatch) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 34";
      extent = getWord(JS_window.extent, 0)-20 SPC getWord(%scale, 1)-10;
      clipToParent = "1";
      color = "255 255 255 255";
    };

    %swatch.text = new GuiMLTextCtrl(GlassEventText) {
      profile = "GuiMLTextProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = vectorSub(%swatch.extent, "20 20");
      text = "";
    };

    JS_window.add(%swatch);
    %swatch.add(%swatch.text);
  } else {
    GlassEventSwatch.deleteAll();
    GlassEventSwatch.delete();
  }

  JS_window._showingEvent = !JS_window._showingEvent;
}

function GlassServers::updateEventDisplay(%title, %playercount, %maxPlayers, %gamemode) {
  %text = "<font:verdana bold:15><color:333333>";

  %text = %text @ %title;
  %text = %text @ "<just:right><font:verdana:15><color:888888>" @ %playercount @ "<color:333333>/" @ %maxPlayers @ " Players";

  %text = %text @ "<br><just:left>";

  %text = %text @ "<color:edb869>[Public Event]";
  %text = %text @ "<just:right><color:888888>" @ %gamemode;
  GlassEventText.setText(%text);
}

package GlassServers {
  function interfaceFunction() {
    GlassLog::debug("\c4Prevented RTB JoinServerGui");
  }

  function MainMenuButtonsGui::onWake(%this) {
    if(isObject(GlassFavoriteServers))
        GlassFavoriteServers.scanServers();

    if(isFunction(%this, onWake))
      parent::onWake(%this);
  }

  function LoadingGui::onWake(%this) {
	  LOAD_MapPicture.setBitmap("base/client/ui/loadingBG");
    if(isFunction(LoadingGui, onWake))
      parent::onWake(%this);

    NewChatHud.add(GlassLoadingGui);
    GlassLoadingGui.forceCenter();
    NewChatHud.pushToBack(GlassLoadingGui);
    NewChatHud.schedule(50, pushToBack, newChatText);
  }

  function LoadingGui::onSleep(%this) {
    if(isFunction(LoadingGui, onSleep))
      parent::onSleep(%this);

    GuiGroup.add(GlassLoadingGui);
  }

  function NewPlayerListGui::UpdateWindowTitle(%this) {
    parent::UpdateWindowTitle(%this);

    //this method doesn't seem to do anything?
    %text = NPL_list.rowCount() @ "/" @ $ServerInfo::MaxPlayers @ " Players - " @ $ServerInfo::Name;
    NPL_Window.setText(%text);

    GlassLoadingGui.UpdateWindowTitle();
  }

  function NewPlayerListGui::update(%this, %client, %name, %blid, %isAdmin, %isSuperAdmin, %score) {
    parent::update(%this, %client, %name, %blid, %isAdmin, %isSuperAdmin, %score);

    GlassLoadingGui_UserList.update();
  }

  function secureClientCmd_ClientDrop(%a, %b) {
    parent::secureClientCmd_ClientDrop(%a, %b);

    GlassLoadingGui_UserList.update();
  }

  function disconnectedCleanup(%a) {
    parent::disconnectedCleanup(%a);

    GlassLoadingGui.onSleep();
  }
};
activatePackage(GlassServers);
