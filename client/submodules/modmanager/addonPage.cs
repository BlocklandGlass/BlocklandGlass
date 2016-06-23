//discoverFile("*"); exec("Add-Ons/System_BlocklandGlass/client/submodules/modmanager/addonPage.cs");

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
    color = "255 255 255 255";
    position = "10 10";
    extent = "485 498";
  };

  %container.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:24><just:left>" @ %obj.name;
    position = "102 30";
    extent = "300 24";
    minextent = "0 0";
    autoResize = true;
  };

  %container.author = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana:16><just:left>by " @ %obj.author;
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %container.info = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana:16><just:left><bitmap:Add-Ons/System_BlocklandGlass/image/icon/tag.png> " @ %obj.board @ "<br><bitmap:Add-Ons/System_BlocklandGlass/image/icon/folder_vertical_zipper.png> " @ %obj.filename @ "<br><bitmap:Add-Ons/System_BlocklandGlass/image/icon/accept_button.png> Approved";
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %container.description = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana:16><just:left>" @ %obj.description;
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
    autoResize = true;
  };

  %branchColor["stable"] = "128 255 128 255";
  %branchColor["unstable"] = "255 255 128 255";
  %branchColor["development"] = "255 128 128 255";


  %num = getWordCount(%obj.branches);
  %xExtent = mfloor((505-70)/3);
  %xMargin = 10;
  %totalWidth = (%xExtent*%num) + (%xMargin*(%num-1));

  for(%i = 0; %i < getWordCount(%obj.branches); %i++) {
    %bid = getword(%obj.branches, %i);
    %branch = %obj.branchName[%bid];

    %x = ((505-%totalWidth)/2) + (%xExtent*(%i)) + (%xMargin*(%i));

    %status = GlassModManager::getAddonStatus(%obj.id);
    switch$(%status) {
      case "installed":
        %text = "Installed";
        %action = "";

      case "downloading":
        %text = "Downloading..";
        %action = "";

      case "queued":
        %text = "Queued..";
        %action = "";

      case "outdated":
        %text = "Update";
        %action = "update";

      default:
        %text = "Download";
        %action = "download";
    }

    %container.download[%branch] = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = %branchColor[%branch];
      position = %x SPC 0;
      extent = %xExtent SPC 35;
    };

    %name = "GlassModManagerGui_DlButton_" @ %obj.id @ "_" @ (%i+1);
    echo(%name);
    %container.download[%branch].info = new GuiMLTextCtrl(%name) {
      horizSizing = "center";
      vertSizing = "center";
      text = "<font:verdana bold:16><just:center>" @ %text @ "<br><font:verdana:14>" @ strcap(%branch);
      position = "0 0";
      extent = "300 16";
      minextent = "0 0";
      autoResize = true;
    };

    echo("swat:" @ %container.download[%branch]);

    %container.download[%branch].add(%container.download[%branch].info);

    if(%action !$= "") {
      %container.download[%branch].mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonDownloadButton) {
        aid = %obj.id;
        obj = %obj;
        swatch = %container.download[%branch];
        branch = %branch;
      };
      %container.download[%branch].add(%container.download[%branch].mouse);
    }

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

  %container.info.setMarginResize(20);
  %container.description.setMarginResize(20);

  %container.info.setVisible(true);
  %container.info.forceReflow();
  %container.description.setVisible(true);
  %container.description.forceReflow();

  %container.setMarginResize(10, 10);

  %container.title.setMargin(20, 20);
  %container.title.setMarginResize(20);
  %container.author.setMarginResize(20);
  %container.author.placeBelow(%container.title, 1);
  %container.info.placeBelow(%container.author, 15);
  %container.description.placeBelow(%container.info, 25);
  for(%i = 0; %i < %num; %i++) {
    %bid = getword(%obj.branches, %i);
    %branch = %obj.branchName[%bid];
    %container.download[%branch].placeBelow(%container.description, 25);
  }

  %container.verticalMatchChildren(10, 10);

  //================================
  // comments
  //================================

  %comments = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "485 20";
  };

  GlassModManagerGui_MainDisplay.add(%comments);

  %comments.add(%an = GlassModManagerGui::createLoadingAnimation());
  %comments.verticalMatchChildren(34, 10);
  %an.forceCenter();
  %comments.placeBelow(%container, 10);

  GlassModManagerGui::loadAddonComments(%obj.id, %comments);

  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.getGroup().scrollToTop();
}

function GlassModManagerGui::loadAddonComments(%id, %swatch) {
  %swatch.setName("GlassModManagerGui_AddonComments");

  %tcp = GlassModManager::placeCall("comments", "id" TAB %id);
}

function GlassModManagerGui::renderAddonComments(%data) {
  if(!isObject(GlassModManagerGui_AddonComments))
    return;

  %swatch = GlassModManagerGui_AddonComments;
  %swatch.deleteAll();

  if(%data.length == 0) {
    %text = new GuiMLTextCtrl() {
      horizSizing = "center";
      vertSizing = "center";
      text = "<font:verdana bold:14><just:center>No Comments!";
      position = "10 10";
      extent = "300 14";
      minextent = "0 0";
      autoResize = true;
    };
    %swatch.add(%text);
    %swatch.verticalMatchChildren(10, 10);
    %text.forceCenter();
  } else {
    %offset = 0;
    for(%i = 0; %i < %data.length; %i++) {
      %comment = %data.value[%i];
      %swat = new GuiSwatchCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        color = "255 255 255 255";
        position = 0 SPC %offset;
        extent = "485 20";
      };

      %auth = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana bold:14>" @ %comment.author @ "<br><font:verdana:13>" @ %comment.authorblid @ "<br><br><font:verdana:10>" @ %comment.date;
        position = "10 10";
        extent = "125 16";
        minextent = "0 0";
        autoResize = true;
      };

      %text = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana:13>" @ %comment.text;
        position = "135 10";
        extent = "340 16";
        minextent = "0 0";
        autoResize = true;
      };
      %swat.add(%auth);
      %swat.add(%text);

      %swatch.add(%swat);

      %auth.forceReflow();
      %text.forceReflow();
      %swat.verticalMatchChildren(0, 10);
      %offset += getWord(%swat.extent, 1);
    }

    %swatch.verticalMatchChildren();
  }

  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);

  %scroll = GlassModManagerGui_MainDisplay.getGroup();
  %scroll.clear();
  %scroll.add(GlassModManagerGui_MainDisplay);
  GlassModManagerGui_MainDisplay.getGroup().scrollToTop();
}

function GlassModManagerGui::createLoadingAnimation() {
  %animation = new GuiBitmapCtrl(GlassLoadingAnimation) {
    profile = "GuiDefaultProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "365 10";
    extent = "43 11";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    bitmap = "Add-Ons/System_BlocklandGlass/image/loading_animation/1";
    anim_path = "Add-Ons/System_BlocklandGlass/image/loading_animation/";
    anim_tick = 1;
    anim_max = 4;
    wrap = "0";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    keepCached = "0";
    mColor = "100 100 100 100";
    mMultiply = "1";
  };
  %animation.tick();
  return %animation;
}

function GlassLoadingAnimation::tick(%this) {
  cancel(%this.anim_sch);
  %this.setBitmap(%this.anim_path @ %this.anim_tick);

  %this.anim_tick++;
  if(%this.anim_tick > %this.anim_max) {
    %this.anim_tick = 1;
  }
  %this.anim_sch = %this.schedule(100, tick);
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

  if(vectordist(%this.position, %this.destination) < 10) {
    %this.delete();
    return;
  }

  GlassModManagerGui.pushToBack(%this);

  %this.schedule(%tickLength, tick);
}
