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
};
activatePackage(GlassServers);

function GlassServerPreviewGui::onWake(%this) {
  GlassServerPreviewWindowGui.forceCenter();
  %server = ServerInfoGroup.getObject(JS_ServerList.getSelectedID());
  
  if(!isObject(%server))
	  return;

  GlassServerPreview_Name.setText("<font:verdana bold:22>" @ %server.name);
  GlassServerPreview_Playercount.setText("<font:verdana bold:18>" @ %server.currPlayers @ "/" @ %server.maxPlayers SPC "Players");
  GlassServerPreview_preview.setBitmap("Add-Ons/System_BlocklandGlass/image/gui/noImage.png");
  GlassServerPreview_Playerlist.clear();
  GlassServerPreview::getServerInfo(%server.ip);
  
  GlassServerPreview::getServerBuild(%server.ip);
}

function GlassServerPreview::getServerBuild(%addr) {
  %addr = strReplace(%addr, ".", "-");
  %addr = strReplace(%addr, ":", "_");
  %url = "http://image.blockland.us/detail/" @ %addr @ ".jpg";
  %method = "GET";
  %downloadPath = "config/client/BLG/ServerPreview.jpg";
  %className = "GlassServerPreviewTCP";

  %tcp = connectToUrl(%url, %method, %downloadPath, %className);
}

function GlassServerPreviewTCP::onDone(%this, %error) {
  if(%error) {
    echo("ERROR:" SPC %error);
  }
  GlassServerPreview_preview.setBitmap("config/client/BLG/ServerPreview.jpg");
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
	
	if(%result.status $= "error")
		GlassServerPreview_noGlass.setVisible(1);
	else {
	  for(%i=0; %i < %result.valueClients.length; %i++) {
		GlassServerPreview_noGlass.setVisible(0);
	    %cl = %result.valueClients.value[%i];
	    GlassServerPreview_Playerlist.addRow(%cl.blid, %cl.status TAB %cl.name TAB %cl.blid);
	  }
	}
  }
}

function joinServerGui::preview(%this) {
  if(JS_ServerList.getSelectedID() == -1)
	  return;
  canvas.pushDialog(GlassServerPreviewGui);
}