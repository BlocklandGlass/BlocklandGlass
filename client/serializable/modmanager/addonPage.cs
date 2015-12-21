//discoverFile("*"); exec("Add-Ons/System_BlocklandGlass/client/serializable/modmanager/addonPage.cs");

function GlassModManagerGui::fetchAndRenderAddon(%modId) {
  GlassModManager::placeCall("addon", "id" TAB %modId);
}

function GlassModManagerGui::renderAddon(%obj) {
  //obj:
  // authors
  // manager
  // name
  // description
  // tags
  // board
  // dependencies
  //
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %container.title = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand-bold:24><just:left>" @ %obj.name;
    position = "102 30";
    extent = "300 24";
    minextent = "0 0";
    autoResize = true;
  };

  %container.author = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand:16><just:left>by " @ %obj.author;
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %container.info = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand:16><just:left><bitmap:Add-Ons/System_BlocklandGlass/image/icon/tag.png> " @ %obj.board @ "<br><bitmap:Add-Ons/System_BlocklandGlass/image/icon/folder_vertical_zipper.png> " @ %obj.filename @ "<br><bitmap:Add-Ons/System_BlocklandGlass/image/icon/accept_button.png> Approved";
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %container.description = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "<font:quicksand:16><just:left>" @ %obj.description;
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %num = getRandom(1, 3);
  %branch[0] = "stable";
  %branch[1] = "unstable";
  %branch[2] = "development";
  %branchColor["stable"] = "128 255 128 255";
  %branchColor["unstable"] = "255 255 128 255";
  %branchColor["development"] = "255 128 128 255";


  %xExtent = mfloor((505-70)/3);
  %xMargin = 10;
  %totalWidth = (%xExtent*%num) + (%xMargin*(%num-1));

  for(%i = 0; %i < %num; %i++) {
    %x = ((505-%totalWidth)/2) + (%xExtent*(%i)) + (%xMargin*(%i));

    %branch = %branch[%i];
    %container.download[%branch] = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = %branchColor[%branch];
      position = %x SPC 0;
      extent = %xExtent SPC 35;
    };

    %container.download[%branch].info = new GuiMLTextCtrl() {
      horizSizing = "center";
      vertSizing = "center";
      text = "<font:quicksand-bold:16><just:center>Download<br><font:quicksand:14>" @ strcap(%branch);
      position = "0 0";
      extent = "300 16";
      minextent = "0 0";
      autoResize = true;
    };

    %container.download[%branch].mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonDownloadButton) {
      aid = %obj.id;
      obj = %obj;
      swatch = %container.download[%branch];
      branch = %branch;
    };

    %container.download[%branch].add(%container.download[%branch].info);
    %container.download[%branch].add(%container.download[%branch].mouse);
    %container.download[%branch].info.setMarginResize(2, 2);
    %container.download[%branch].info.forceCenter();
    %container.add(%container.download[%branch]);
  }


  %container.add(%container.title);
  %container.add(%container.author);
  %container.add(%container.info);
  %container.add(%container.description);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);

  %container.info.setVisible(true);
  %container.info.forceReflow();

  %container.setMarginResize(0, 0);

  %container.title.setMargin(20, 20);
  %container.title.setMarginResize(20);
  %container.author.setMarginResize(20);
  %container.author.placeBelow(%container.title, 1);
  %container.info.setMarginResize(20);
  %container.info.placeBelow(%container.author, 15);
  %container.description.setMarginResize(20);
  %container.description.placeBelow(%container.info, 25);
  for(%i = 0; %i < %num; %i++) {
    %container.download[%branch[%i]].placeBelow(%container.description, 25);
  }

  %container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
}

function GlassModManagerGui::doDownloadSprite(%origin, %destination, %maxHeight) {
  // writing this, it took me way too long to realize that %var^2 isn't squaring something
  // it works, but uses quite a few shortcut kinematics

  %height = getRandom(-%maxHeight/2, -%maxHeight);
  %accel = 500;

  %vertVelocity = -msqrt(-1*%accel*%height);

  %vertChange = getWord(%destination, 1)-getWord(%origin, 1);

  %finalVertVelocity = msqrt(mpow(%vertVelocity, 2) + (2*%accel*%vertChange));
  %deltaVelocity = %finalVertVelocity - %vertVelocity;
  %duration = %deltaVelocity/%accel;

  %horizVelocity = (getWord(%destination, 0)-getWord(%origin, 0))/%duration;

  %sprite = new GuiBitmapCtrl(GlassDownloadSprite) {
    horizSizing = "center";
    vertSizing = "center";
    bitmap = "Add-Ons/System_BlocklandGlass/image/icon/folder_vertical_zipper.png";
    position = %origin;
    extent = "20 20";
    minextent = "0 0";
    clipToParent = true;

    accel = %accel;
    velocity = %horizVelocity SPC %vertVelocity;
    originalVelocity = %horizVelocity SPC %vertVelocity;
    origin = %origin;
    destination = %destination;
    actualposition = %origin;
    timeElapsed = 0;
  };

  GlassModManagerGui.add(%sprite);
  %sprite.tick();
  echo(%sprite);
}

function GlassDownloadSprite::tick(%this) {
  %accel = %this.accel;
  %vertVelocity = getWord(%this.velocity, 1);

  %tickLength = 10; //ms
  %this.timeElapsed += %tickLength;

  if(%this.timeElapsed > 5000) {
    %this.delete();
    return;
  }

  %newVelocity = %vertVelocity + ((%tickLength/1000) * %accel);

  %vertPos = ((%vertVelocity+%newVelocity)/2) * (%tickLength/1000);
  %vertPos += getWord(%this.actualposition, 1);

  %horizPos = (getWord(%this.originalVelocity, 0)*(%this.timeElapsed/1000))+getWord(%this.origin, 0);

  %this.velocity = getWord(%this.velocity, 0) SPC %newVelocity;

  %this.actualposition = %horizPos SPC %vertPos;
  %this.position = mfloor(%horizPos) SPC mfloor(%vertPos);

  //echo(vectordist(%this.position, %this.destination));
  if(vectordist(%this.position, %this.destination) < 10) {
    %this.delete();
    return;
  }

  GlassModManagerGui.pushToBack(%this);

  %this.schedule(%tickLength, tick);
}
