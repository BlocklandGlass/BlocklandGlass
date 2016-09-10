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
    text = "<font:verdana:13><just:left>by " @ %obj.author;
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

  %rate = %obj.rating;
  %x = 380;
  for(%i = 0; %i < 5; %i++) {
    if(%rate >= 1) {
      %bitmap = "star";
    } else if(%rate >= 0.75) {
      %bitmap = "star_frac_3";
    } else if(%rate >= 0.50) {
      %bitmap = "star_frac_2";
    } else if(%rate >= 0.25) {
      %bitmap = "star_frac_1";
    } else {
      %bitmap = "star_empty";
    }
    %container.star[%i+1] = new GuiBitmapCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      bitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap;
      dbitmap = "Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap;
      position = %x SPC 10;
      extent = "16 16";
      minextent = "0 0";
      clipToParent = true;
    };

    %x += 20;
    %rate -= 1;

    %container.add(%container.star[%i+1]);
  }

  %container.ratingmouse = new GuiMouseEventCtrl(GlassModManagerGui_RatingMouse) {
    aid = %obj.id;
    profile = "GuiDefaultProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = %container.star1.position;
    extent = "100 25";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    lockMouse = "0";
  };

  %container.add(%container.ratingmouse);

  %branchColor["stable"] = "128 255 128 255";
  %branchColor["beta"] = "255 128 128 255";


  %num = getWordCount(%obj.branches);
  %xExtent = mfloor((505-70)/3);
  %xMargin = 10;
  %totalWidth = (%xExtent*%num) + (%xMargin*(%num-1));

  for(%i = 0; %i < getWordCount(%obj.branches); %i++) {
    %bid = getword(%obj.branches, %i);
    %branch = %obj.branchName[%bid];

    %x = ((485-%totalWidth)/2) + (%xExtent*(%i)) + (%xMargin*(%i));

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
    %container.download[%branch].info = new GuiMLTextCtrl(%name) {
      horizSizing = "center";
      vertSizing = "center";
      text = "<font:Verdana Bold:15><just:center>" @ %text @ "<br><font:verdana:14>" @ strcap(%branch);
      position = "0 0";
      extent = "300 16";
      minextent = "0 0";
      autoResize = true;
    };

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

  %x = 10;
  for(%i = 0; %i < %obj.screenshots.length; %i++) {
    %ss = %obj.screenshots.value[%i];
    %screenshotHolder = new GuiSwatchCtrl(GlassScreenshot) {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "200 200 200 255";
      position = %x SPC 10;
      extent =  "96 96";

      thumb = %ss.thumbnail;
      url = %ss.url;
      id = %ss.id;
      display_extent = %ss.extent;
    };

    %screenshotHolder.loadThumb();

    %x += 106;

    %container.add(%screenshotHolder);
    %screenshotHolder.placeBelow(%container.downloadStable, 25);
  }

  %container.verticalMatchChildren(0, 10);

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

  GlassModManagerGui_MainDisplay.container = %container;
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.setVisible(true);
  GlassModManagerGui_MainDisplay.getGroup().scrollToTop();
}

function GlassScreenshot::loadThumb(%this) {
  %loading = GlassModManagerGui::createLoadingAnimation();
  %this.add(%loading);
  %loading.forceCenter();

  %url = %this.thumb;
  %method = "GET";
  %downloadPath = "config/client/cache/" @ %this.id @ "_thumb";
  %className = "GlassScreenshotTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.swatch = %this;
  %tcp.thumb = 1;

  %this.tcp = %tcp;
}

function GlassScreenshot::loadScreenshot(%this) {
  %loading = GlassModManagerGui::createLoadingAnimation();
  GlassModManagerImage.add(%loading);
  GlassModManagerImage.loading = %loading;
  %loading.forceCenter();

  %url = %this.url;
  %method = "GET";
  %downloadPath = "config/client/cache/" @ %this.id;
  %className = "GlassScreenshotTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.swatch = %this;
  %tcp.thumb = 0;

  %this.tcp = %tcp;
}

function GlassScreenshotTCP::onLine(%this, %line) {
  if(strpos(%line, "Content-Disposition:") == 0) {
    %filename = getword(%line, 2);
    %filename = getsubstr(%filename, strpos(%filename, "\"")+1, strlen(%filename)-11);
    %ext = getSubStr(%filename, stripos(%filename, ".")+1, 4);
    %this.ext = %ext;
  }
}

function GlassScreenshotTCP::onDone(%this, %error) {
  if(%error) {
    %swatch = %this.swatch;
    %swatch.deleteAll();
    %swatch.color = "255 150 150 255";
    %text = new GuiTextCtrl() {
      profile = "GuiTextVerdanaProfile";
      text = "Error";
      extent = "96 18";
    };
    %swatch.add(%text);
    %text.forceCenter();
  } else {
    if(%this.thumb) {
      %swatch = %this.swatch;
      fileCopy("config/client/cache/" @ %swatch.id @ "_thumb", "config/client/cache/" @ %swatch.id @ "_thumb." @ %this.ext);
      %bitmap = new GuiBitmapCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        bitmap = "config/client/cache/" @ %swatch.id @ "_thumb." @ %this.ext;
        position = "0 0";
        extent = "96 96";
        minextent = "0 0";
        clipToParent = true;
      };

      %swatch.highlight = new GuiSwatchCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        color = "200 200 200 0";
        position = "0 0";
        extent =  "96 96";

        thumb = %ss.thumbnail;
        url = %ss.url;
        id = %ss.id;
        display_extent = %ss.extent;
      };

      %swatch.mouse = new GuiMouseEventCtrl(GlassScreenshotMouse) {
        swatch = %swatch;
        highlight = %swatch.highlight;
        profile = "GuiDefaultProfile";
        horizSizing = "right";
        vertSizing = "bottom";
        position = "0 0";
        extent = %swatch.extent;
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        lockMouse = "0";
      };

      %swatch.deleteAll();
      %swatch.add(%bitmap);
      %swatch.add(%swatch.highlight);
      %swatch.add(%swatch.mouse);
    } else {
      %swatch = %this.swatch;
      fileCopy("config/client/cache/" @ %swatch.id, "config/client/cache/" @ %swatch.id @ "." @ %this.ext);
      GlassModManagerImageCtrl.extent = %swatch.display_extent;
      GlassModManagerImageCtrl.setBitmap("config/client/cache/" @ %swatch.id @ "." @ %this.ext);
      GlassModManagerImageCtrl.setVisible(true);
      GlassModManagerImageCtrl.forceCenter();
      GlassModManagerImage.loading.delete();
    }
  }
}

function GlassScreenshotMouse::onMouseEnter(%this) {
  %this.highlight.color = "255 255 255 128";
}

function GlassScreenshotMouse::onMouseLeave(%this) {
  %this.highlight.color = "255 255 255 0";
}

function GlassScreenshotMouse::onMouseDown(%this) {
  GlassModManagerImageCtrl.setVisible(false);
  canvas.pushDialog(GlassModManagerImage);
  %this.swatch.loadScreenshot();
}

function GlassModManagerGui::displayAddonRating(%rating) {
  messageBoxOk("Thanks!", "Your rating has been submitted. Thanks for the input!");
  %rate = %rating;
  %x = 380;
  for(%i = 0; %i < 5; %i++) {
    if(%rate >= 1) {
      %bitmap = "star";
    } else if(%rate >= 0.75) {
      %bitmap = "star_frac_3";
    } else if(%rate >= 0.50) {
      %bitmap = "star_frac_2";
    } else if(%rate >= 0.25) {
      %bitmap = "star_frac_1";
    } else {
      %bitmap = "star_empty";
    }
    GlassModManagerGui_MainDisplay.container.star[%i+1].setBitmap("Add-Ons/System_BlocklandGlass/image/icon/" @ %bitmap);

    %x += 20;
    %rate -= 1;
  }
}

function GlassModManagerGui_RatingMouse::onMouseEnter(%this, %x, %pos) {
  GlassModManagerGui_RatingMouse::onMouseMove(%this, %x, %pos);
}

function GlassModManagerGui_RatingMouse::onMouseMove(%this, %x, %pos, %y) {
  %x = getWord(%pos, 0)-getWord(%this.getScreenPosition(), 0);
  %rating = mceil((%x-4)/20);

  if(%rating < 1)
    %rating = 1;

  for(%i = 0; %i < 5; %i++) {
    %star = %this.getGroup().star[%i+1];
    if(%rating > %i)
      %star.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/star");
    else
      %star.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/star_empty");
  }
}

function GlassModManagerGui_RatingMouse::onMouseLeave(%this, %x, %pos, %y) {
  for(%i = 0; %i < 5; %i++) {
    %star = %this.getGroup().star[%i+1];
    %star.setBitmap(%star.dbitmap);
  }
}

function GlassModManagerGui_RatingMouse::onMouseDown(%this, %x, %pos, %y) {
  %x = getWord(%pos, 0)-getWord(%this.getScreenPosition(), 0);
  %rating = mceil((%x-4)/20);

  if(%rating < 1)
    %rating = 1;

  %tcp = GlassModManager::placeCall("rating", "id" TAB %this.aid NL "rating" TAB %rating);
}

function GlassModManagerGui::loadAddonComments(%id, %swatch) {
  %swatch.setName("GlassModManagerGui_AddonComments");
  GlassModManagerGui_AddonComments.currentAddon = %id;

  %tcp = GlassModManager::placeCall("comments", "id" TAB %id);
}

function GlassModManagerGui::renderAddonComments(%data) {
  if(!isObject(GlassModManagerGui_AddonComments))
    return;

  %swatch = GlassModManagerGui_AddonComments;
  %swatch.deleteAll();

  %newCommentScroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 10";
    extent = "465 100";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "0 0";
    rowHeight = "40";
    columnWidth = "30";
  };

  %newCommentSwat = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "240 240 240 255";
    position = "10 10";
    extent = "465 97";
  };

  %newCommentEdit = new GuiMLTextEditCtrl(GlassModManagerGui_newComment) {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "";
    position = "5 5";
    extent = "445 90";
    extent = "445 90";
    autoResize = true;
    profile = "GlassMLTextEditProfile";
    command = "GlassModManagerGui_newComment.onUpdate();";
  };

  %newCommentButton = new GuiBitmapButtonCtrl() {
    profile = "BlockButtonProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "415 175";
    extent = "60 20";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    command = "GlassModManagerGui::submitComment();";
    text = "Post";
    groupNum = "-1";
    buttonType = "PushButton";
    bitmap = "base/client/ui/button1";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "220 220 220 255";
  };

  %newCommentDiv = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "128 128 128 255";
    position = "10 10";
    extent = "465 1";
  };

  %newCommentScroll.add(%newCommentSwat);
  %newCommentSwat.add(%newCommentEdit);
  %swatch.add(%newCommentScroll);
  %swatch.add(%newCommentButton);
  %swatch.add(%newCommentDiv);
  %newCommentButton.centerX();
  %newCommentButton.placeBelow(%newCommentScroll, 10);
  %newCommentDiv.placeBelow(%newCommentButton, 10);
  %swatch.verticalMatchChildren(10, 10);

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
    %text.centerX();
    %text.placeBelow(%newCommentDiv, 10);
    %swatch.verticalMatchChildren(10, 10);
  } else {
    %offset = 0;
    %dark = 1;
    for(%i = 0; %i < %data.length; %i++) {
      %comment = %data.value[%i];
      %swat = new GuiSwatchCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        color = (%dark ? "235 235 235 255" : "245 245 245 255");
        position = 10 SPC %offset;
        extent = "465 20";
      };

      %dark = !%dark;

      %auth = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana bold:14>" @ %comment.author @ "<br><font:verdana:13>" @ %comment.authorblid @ "<br><br><font:verdana:10>" @ %comment.date;
        position = "10 10";
        extent = "125 16";
        minextent = "0 0";
        autoResize = true;
      };

      %comment.text = strLimitRep(%comment.text, "<br>", 2);

      %text = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana:13>" @ %comment.text;
        position = "115 10";
        extent = "360 16";
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

      if(%i == 0) {
        %swat.placeBelow(%newCommentButton, 10);
      } else {
        %swat.placeBelow(%lastSwat);
      }
      %lastSwat = %swat;
    }

    %swatch.verticalMatchChildren(10, 10);
  }

  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);

  %scroll = GlassModManagerGui_MainDisplay.getGroup();
  %scroll.clear();
  %scroll.add(GlassModManagerGui_MainDisplay);
  GlassModManagerGui_MainDisplay.getGroup().scrollToTop();
  GlassModManagerGui_MainDisplay.getGroup().makeFirstResponder(1);
}

function GlassModManagerGui_newComment::onResize(%this, %thisx, %thisy) {
  %x = getWord(%this.getGroup().extent, 0);
  %y = %thisY + 10;

  if(%y < 97) {
    %y = 97;
  }

  if(%this.getGroup().extent !$= (%x SPC %y)) {
    %this.getGroup().extent = %x SPC %y;
    %this.getGroup().setVisible(true);
    %this.makeFirstResponder(1);
    %this.getGroup().getGroup().scrollToBottom();
  }
}

function GlassModManagerGui::submitComment() {
  %text = GlassModManagerGui_newComment.getValue();
  %text = strReplace(%text, "\t", "");
  %text = strLimitRep(%text, "\n", 2);
  %text = trim(%text);

  %input = %text;
  %text = "";
  for(%i = 0; %i < getLineCount(%input); %i++) {
    if(%text !$= "")
      %text = %text @ "\n";

    %text = %text @ stripMLControlChars(getLine(%input, %i));
  }

  %text = expandEscape(%text);

  if(%text !$= "")
    GlassModManager::placeCall("comments", "id" TAB GlassModManagerGui_AddonComments.currentAddon NL "newcomment" TAB %text);
}

function strLimitRep(%str, %char, %limit) {
  for(%i = 0; %i < %limit; %i++) {
    %delimiter = %delimiter @ %char;
  }
  %remover = %delimiter @ %char;

  while(true) {
    %lastStr = %str;
    %str = strReplace(%str, %remover, %delimiter);

    if(strlen(%lastStr) == strlen(%str)) {
      break;
    }
  }

  return %str;
}

function GlassModManagerGui::createLoadingAnimation() {
  %animation = new GuiBitmapCtrl(GlassLoadingAnimation) {
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
