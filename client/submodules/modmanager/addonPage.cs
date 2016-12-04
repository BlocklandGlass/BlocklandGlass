function GMM_AddonPage::init() {
  new ScriptObject(GMM_AddonPage);
}

function GMM_AddonPage::open(%this, %modId) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 0 255 0";
    position = "0 0";
    extent = "635 498";
  };

  %body = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 10";
    extent = "615 498";
  };

  %this.container = %container;
  %this.body = %body;
  %container.add(%body);

  GlassModManager::placeCall("addon", "id" TAB %modId, "GMM_AddonPage.handleResults");

  return %container;
}

function GMM_AddonPage::handleResults(%this, %obj) {
  echo("addonpage resx");
  //obj:
  // aid
  // filename
  // boardId
  // board
  // name
  // description
  // date
  // downloads
  // rating
  // screenshots
  // author
  // contributors
  // branches

  %container = %this.container;
  %body = %this.body;


  if($Glass::MM_PreviousBoard == -1 || $Glass::MM_PreviousPage == -1)
    %link = "<a:glass://home><< Back</a>";
  else
    %link = "<a:glass://board=" @ $Glass::MM_PreviousBoard @ "&page=" @ $Glass::MM_PreviousPage @ "><< Back</a>";

  %body.back = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    profile = "GlassModManagerMLProfile";
    text = "<color:333333><font:verdana:15><just:left>" @ %link;
    position = "10 10";
    extent = "75 15";
  };

  %body.add(%body.back);

  %body.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:20><just:left>" @ %obj.name;
    position = "10 30";
    extent = "300 24";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.title);
  %body.title.placeBelow(%body.back, 5);

  %body.author = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana:12><just:left>Uploaded by " @ %obj.author @ "<just:right><color:444444>" @ %obj.date;
    position = "10 30";
    extent = "595 12";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.author);
  %body.author.placeBelow(%body.title, 0);
  %body.verticalMatchChildren(20, 10);

  %downloads = %obj.downloads;

  %info = 4;
  %border = 5;
  %width = mFloor((615+%border)/%info);
  for(%i = 0; %i < %info; %i++) {
    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "255 255 255 255";
      position = (%width)*%i+10 SPC 0;
      extent = %width-%border SPC 36;
    };

    %swatch.image = new GuiBitmapCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      position = "10 10";
      extent = "16 16";
      bitmap = "";
    };

    %swatch.text = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "";
      position = "0 11";
      extent = (%width-%border) SPC "13";
      minextent = (%width-20) SPC "13";
    };

    switch(%i) {
      case 0:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.board);
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/category.png");

      case 1:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.filename);
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/folder_vertical_zipper.png");

      case 2:
        %swatch.text.setText("<font:verdana:13><just:center>Rating");

      case 3:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.downloads @ " downloads");
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/inbox_download.png");
    }

    %swatch.add(%swatch.text);
    %swatch.add(%swatch.image);
    %container.add(%swatch);

    %swatch.placeBelow(%body, %border);
    %container.info[%i] = %swatch;
  }

  for(%i = 0; %i < getWordCount(%obj.description); %i++) {
    %word = getWord(%obj.description, %i);
    if(strpos(%word, "http://") == 0 || strpos(%word, "https://") == 0 || strpos(%word, "glass://") == 0) {
      %word = "<a:" @ %word @ ">" @ %word @ "</a>";
      %obj.description = setWord(%obj.description, %i, %word);
    }
  }

  %description = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 0";
    extent = "615 30";
  };

  %description.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:13>Description<br><br><color:444444><font:verdana:13>" @ %obj.description;
    position = "10 10";
    extent = "595 16";
    minextent = "0 0";
    autoResize = true;
  };

  %description.add(%description.text);

  %container.description = %description;
  %container.add(%container.description);
  %container.description.placeBelow(%container.info0, 5);

  %container.description.text.forceReflow();
  %container.description.verticalMatchChildren(30, 10);
  %container.verticalMatchChildren(0, 0);

  %screenshots = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 0";
    extent = "615 30";
  };

  %screenshots.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:13>Screenshots";
    position = "10 10";
    extent = "595 16";
    minextent = "0 0";
    autoResize = true;
  };

  %screenshots.add(%screenshots.text);

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

    %screenshots.add(%screenshotHolder);
  }

  %screenshots.verticalMatchChildren(36, 10);

  %container.screenshots = %screenshots;
  %container.add(%container.screenshots);
  %container.screenshots.placeBelow(%container.description, 5);

  %container.verticalMatchChildren(0, 10);

  return;

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

  return %container;
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
  glassMessageBoxOk("Rated", "Your rating has been submitted.<br>Thanks for the input!");
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

  return;
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
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
    lockAspectRatio = "0";
    alignLeft = "0";
    alignTop = "0";
    overflowImage = "0";
    mKeepCached = "0";
    mColor = "220 220 220 255";
  };

  %newCommentScroll.add(%newCommentSwat);
  %newCommentSwat.add(%newCommentEdit);
  %swatch.add(%newCommentScroll);
  %swatch.add(%newCommentButton);
  %newCommentButton.centerX();
  %newCommentButton.placeBelow(%newCommentScroll, 10);
  %swatch.verticalMatchChildren(10, 10);

  if(%data.length > 0) {
    %offset = 0;
    %dark = 1;
    for(%i = %data.length - 1; %i > -1; %i--) {
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
        text = "<font:verdana bold:14>" @ getsubstr(%comment.author, 0, 15) @ "<br><font:verdana:13>" @ %comment.authorblid @ "<br><br><font:verdana:10>" @ %comment.date;
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
        extent = "355 16";
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

      if(%i == %data.length - 1) {
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

  GlassOverlay.add(%sprite);
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
