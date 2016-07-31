function GlassServerList::addServer(%this, %obj) {
  if(isObject(%obj.glassTcp)) {
    %obj.glassTcp.delete();
    return;
  }

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
    text = "<tab:300, 365><font:verdana bold:15>" @ %obj.name @ "\t<font:verdana:15>" @ %obj.currPlayers @ "/" @ %obj.maxPlayers @ "\tCustom\t<just:right><bitmap:Add-Ons/System_BlocklandGlass/image/icon/comment>";
  };
  %swatch.add(%swatch.text);
  return %swatch;
}

function GlassServerTCP::onConnected(%this) {
  echo("connected");
  %this.swatch.color = "255 0 0 255";
  %this.connected = true;
  $Glass::st[$Glass::stc+0] = %this;
  $Glass::stc++;
  %this.send("listen\t1\r\n");
}

function GlassServerTCP::onLine(%this, %line) {
  echo("c > " @ %line);
  %cmd = getField(%line, 0);
  switch$(%cmd) {
    case "updateField":
      %key = getField(%line, 1);
      %val = collapseEscape(getField(%line, 2));

      if(%key $= "playercount") {
        %this.serverSO.currPlayers = %val;
        %this.serverSO.display();
      } else if(%key $= "maxPlayers") {
        %this.serverSO.maxPlayers = %val;
        %this.serverSO.display();
      } else if(%key $= "brickCount") {
        %this.serverSO.brickcount = %val;
        %this.serverSO.display();
      } else if(%key $= "name") {
        %this.serverSO.brickcount = %val;
        %this.serverSO.display();
      } else if(%key $= "passworded") {
        %this.serverSO.pass = (%val ? "Yes" : "No");
        %this.serverSO.display();
      }
  }
}

package GlassServerList {
  function ServerSO::display(%this) {
    parent::display(%this);
    GlassServerList.addServer(%this);
  }

  function JoinServerGui::joinServer(%gui) {
    parent::joinServer(%gui);
    for(%i = 0; %i < ServerInfoGroup.getCount(); %i++) {
      %sso = ServerInfoGroup.getObject(%i);
      if(isObject(%sso.glassTcp))
        %sso.glassTcp.disconnect();
    }
  }
};
activatePackage(GlassServerList);
