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

    addonId = %modId;
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

function GMM_AddonPage::close(%this) {
  //nothing special
}

function GMM_AddonPage::handleResults(%this, %obj) {
  //obj:
  // aid
  // filename
  // boardId
  // board
  // name
  // description
  // date
  // downloads
  // screenshots
  // author
  // contributors
  // branches

  if(%obj.status !$= "success") {
    %this.handleNonSuccess(%obj);
    return;
  }

  %container = %this.container;
  GlassModManagerGui.pageDidLoad(%this);

  GMM_Navigation.addStep(%obj.name, "GlassModManagerGui.openPage(GMM_AddonPage, " @ expandEscape(%obj.aid) @ ");");

  %container.nav = GMM_Navigation.createSwatch();
  %container.add(%container.nav);

  %body = %this.body;

  %body.placeBelow(%container.nav, 10);

  %body.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:20><just:left>" @ getASCIIString(%obj.name);
    position = "10 10";
    extent = "600 24";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.title);

  %body.author = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana:12><just:left>Uploaded by " @ getASCIIString(%obj.author) @ "<just:right><color:444444>" @ getASCIIString(%obj.date);
    position = "10 20";
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
    if(%i == 1) {
      %w = (%width*2)-%border;
    } else if(%i == 2) {
      continue;
    } else {
      %w = %width-%border;
    }
    %swatch = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "255 255 255 255";
      position = (%width)*%i+10 SPC 0;
      extent = %w SPC 36;
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
      position = "26 11";
      extent = (%w-26) SPC 13;
      minextent = (%w-26) SPC "13";
    };

    switch(%i) {
      case 0:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.board);
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/category.png");

      case 1:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.filename);
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/folder_vertical_zipper.png");

      case 3:
        %swatch.text.setText("<font:verdana:13><just:center>" @ %obj.downloads @ " downloads");
        %swatch.image.setBitmap("Add-Ons/System_BlocklandGlass/image/icon/inbox_download.png");
    }

    %swatch.add(%swatch.text);
    %swatch.add(%swatch.image);
    %container.add(%swatch);

    %swatch.text.centerY();

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
    text = "<font:verdana bold:13>Description<br><br><color:444444><font:verdana:13>" @ getLongASCIIString(%obj.description);
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

  if(%obj.screenshots.length > 0) {
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
  }

  %download = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 0";
    extent = "615 30";
  };

  %download.dlButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonWhiteProfile";
    position = mfloor((595/2)-125) SPC 10;
    extent = "120 35";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

    text = "Download";

    command = "GMM_AddonPage.downloadClick(" @ %obj.aid @ ");";

    mColor = "84 217 140 255";
  };

  %download.commentButton = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonWhiteProfile";
    position = mfloor((595/2)+5) SPC 10;
    extent = "120 35";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

    text = "Comment";

    command = "GMM_AddonPage.commentClick(" @ %obj.aid @ ");";

    mColor = "131 195 243 255";
  };

  %download.progress = new GuiProgressCtrl() {
    profile = "GlassProgressProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 0";
    extent = "595 25";
    minExtent = "8 2";
    enabled = "1";
    visible = "0";
    clipToParent = "1";
  };

  %download.progress.text = new GuiTextCtrl() {
    profile = "GuiProgressTextProfile";
    horizSizing = "center";
    vertSizing = "center";
    position = "0 0";
    extent = "595 25";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    text = "Connecting...";
    maxLength = "255";
  };

  %download.progress.add(%download.progress.text);
  %download.progress.text.centerY();

  //%download.add(%download.text);
  %download.add(%download.dlButton);
  %download.add(%download.commentButton);
  //%download.dlButton.placeBelow(%download.text);
  //%download.commentButton.placeBelow(%download.text);

  %download.add(%download.progress);
  %download.progress.placeBelow(%download.dlButton, 10);

  %download.verticalMatchChildren(30, 10);

  %container.download = %download;
  %container.add(%download);
  if(%obj.screenshots.length)
    %container.download.placeBelow(%container.screenshots, 10);
  else
    %container.download.placeBelow(%container.description, 10);

  %activity = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "10 0";
    extent = "615 30";
  };

  %activity.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:13>Activity";
    position = "10 10";
    extent = "595 16";
    minextent = "0 0";
    autoResize = true;
  };

  %activity.add(%activity.text);
  %container.add(%activity);
  %activity.placeBelow(%download, 10);

  for(%i = 0; %i < %obj.activity.length; %i++) {
    %action = %obj.activity.value[%i];

    %type = %action.type;
    %date = %action.date;

    if(%type $= "comment") {
      if(%last.type $= "comment") {
        %spacer = new GuiSwatchCtrl() {
          horizSizing = "right";
          vertSizing = "bottom";
          color = "84 217 140 128";
          position = "10 30";
          extent = "595 1";
        };
        %activity.add(%spacer);
        %spacer.placeBelow(%last);
        %last = %spacer;
      }

      if(%action.title !$= "") {
        switch$(%action.title) {
          case "Administrator":
            %color = GlassLive.color_admin;

          case "Moderator":
            %color = GlassLive.color_mod;

          case "Mod Reviewer":
            %color = GlassLive.color_friend;
        }
      }

      %swatch = new GuiSwatchCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        //color = ((%odd = !%odd) ? "230 230 230" : "240 240 240") SPC 255;
        color = "240 240 240 255";
        position = "10 30";
        extent = "595 30";
      };

      %swatch.author = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana bold:13>" @ getASCIIString(%action.author) @ "<br><font:verdana:12><color:333333>" @ %action.authorBlid @ "<br>" @ (%action.title !$= "" ? "<font:verdana bold:12><color:" @ %color @ ">" @ %action.title : "") @ "<br><br>";
        position = "10 10";
        extent = "125 16";
        minextent = "0 0";
        autoResize = true;
      };

      %swatch.text = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana:13><color:333333>" @ getLongASCIIString(%action.comment);
        position = "145 10";
        extent = "440 16";
        minextent = "0 0";
        autoResize = true;
      };

      %swatch.date = new GuiTextCtrl(Date) {
        horizSizing = "right";
        vertSizing = "bottom";

        profile = "GuiTextVerdanaProfile";
        text = %action.date;
        extent = "125 16";
        position = "10 0";
      };

      %swatch.add(%swatch.author);
      %swatch.add(%swatch.text);
      %swatch.add(%swatch.date);

      %activity.add(%swatch);

      %swatch.text.forceReflow();
      %swatch.author.forceReflow();
      %swatch.verticalMatchChildren(70, 10);

      %swatch.date.position = 10 SPC getWord(%swatch.extent, 1)-26;
    } else if(%type $= "update") {
      %swatch = new GuiSwatchCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        color = "84 217 140 170";
        position = "10 30";
        extent = "595 30";
      };

      %swatch.title = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana:13>Updated to <font:verdana bold:13>" @ %action.version;
        position = "10 10";
        extent = "125 16";
        minextent = "0 0";
        autoResize = true;
      };

      %swatch.text = new GuiMLTextCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        text = "<font:verdana bold:13>Change Log:<br><br><font:verdana:12><color:333333>" @ (%action.changelog !$= "" ? getLongASCIIString(%action.changelog) : "None");
        position = "145 10";
        extent = "440 16";
        minextent = "0 0";
        autoResize = true;
      };

      %swatch.date = new GuiTextCtrl(Date) {
        horizSizing = "right";
        vertSizing = "bottom";

        profile = "GuiTextVerdanaProfile";
        text = %action.date;
        extent = "125 16";
        position = "10 0";
      };

      %swatch.add(%swatch.title);
      %swatch.add(%swatch.text);
      %swatch.add(%swatch.date);
      %activity.add(%swatch);

      %swatch.text.forceReflow();
      %swatch.title.forceReflow();
      %swatch.verticalMatchChildren(0, 10);

      %swatch.date.position = 10 SPC getWord(%swatch.extent, 1)-26;
    }


    %swatch.type = %type;

    if(%last)
      %swatch.placeBelow(%last);

    %last = %swatch;
  }

  %container.activity = %activity;

  %activity.verticalMatchChildren(20, 10);
  %container.verticalMatchChildren(0, 10);
  GlassModManagerGui.resizePage();

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

function GMM_AddonPage::handleNonSuccess(%this, %obj) {
  GlassModManagerGui.pageDidLoad(%this);

  switch$(%obj.status) {
    case "notfound":
      %title = "Not Found";
      %message = "The add-on you tried to access was not found! If you believe this is an error, please post on the forums.";

    case "notapproved":
      %title = "Not Approved";
      %message = "The add-on you tried to access is not approved yet!";

    case "deleted":
      %title = "Deleted";
      %message = "The add-on you tried to access has been deleted from the Glass Mod Manager!";

    case "private":
      %title = "Private";
      %message = "You don't have permission to access this add-on.";

    default:
      %message = "Unknown status: " @ %obj.status;
  }

  GMM_Navigation.addStep(%title, "");

  %container = %this.container;
  %container.nav = GMM_Navigation.createSwatch();
  %container.add(%container.nav);

  %body = %this.body;
  %body.placeBelow(%container.nav, 10);

  %body.text = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:verdana bold:15>" @ %title @ "<br><br><color:444444><font:verdana:13>" @ getASCIIString(%message);
    position = "10 10";
    extent = "595 16";
    minextent = "0 0";
    autoResize = true;
  };

  %body.add(%body.text);

  %body.text.forceReflow();
  %body.verticalMatchChildren(30, 10);
  %container.verticalMatchChildren(0, 0);
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

function GMM_AddonPage::downloadClick(%this, %swatch) {
  %download = %this.container.download;
  %download.progress.setVisible(true);
  %download.verticalMatchChildren(30, 10);

  %activity = %this.container.activity;
  %activity.placeBelow(%download, 10);

  %download.progress.setValue(0);
  %download.progress.text.setValue("Connecting...");

  %dl = GlassDownloadManager::newDownload(%this.container.addonId, 1);
  %dl.progressBar = %download.progress;
  %dl.progressText = %download.progress.text;

  %dl.addHandle("done", "GMM_AddonPage_downloadDone");
  %dl.addHandle("progress", "GMM_AddonPage_downloadProgress");
  %dl.addHandle("failed", "GMM_AddonPage_downloadFailed");
  %dl.addHandle("unwritable", "GMM_AddonPage_downloadUnwritable");

  %dl.startDownload();

  %this.container.verticalMatchChildren(498, 10);
  GlassModManagerGui.resizePage();

  //GlassModManager.downloadAddon(%this.container.addonId, %download.progress, %download.progress.text);
}

function GMM_AddonPage::hideDownload(%this) {
  %download = %this.container.download;
  %download.progress.setVisible(false);
  %download.verticalMatchChildren(30, 10);

  %activity = %this.container.activity;
  %activity.placeBelow(%download, 10);

  %this.container.verticalMatchChildren(498, 10);
  GlassModManagerGui.resizePage();
}

function GMM_AddonPage_downloadDone(%dl, %err, %tcp) {
  %this = GMM_AddonPage;
  %this.schedule(500, hideDownload);


  setModPaths(getModPaths());


  %file = getsubstr(%tcp.savePath, 8, strlen(%tcp.savePath) - 12);

  if(isFile("Add-Ons/" @ %file @ "/client.cs"))
    exec("Add-Ons/" @ %file @ "/client.cs");

  GMM_ColorsetsPage.container.delete();
  GMM_ColorsetsPage.populateColorsets();

  GMM_MyAddonsPage.container.delete();
  GMM_MyAddonsPage.populateAddons();
}

function GMM_AddonPage_downloadProgress(%dl, %float, %tcp) {
  %this = GMM_AddonPage;

  cancel(GlassModManagerGui.progressSch);

  %fileSize = %tcp.headerField["Content-Length"];

  %dl.progressBar.setValue(%float);
  %dl.progressText.setText("Downloaded " @ stringifyFileSize(%float*%fileSize, 2));
}

function GMM_AddonPage_downloadFailed(%dl, %error) {
  %this = GMM_AddonPage;

  error("Failed to download add-on " @ %dl.addonId);
}

function GMM_AddonPage_downloadUnwritable(%dl) {
  %this = GMM_AddonPage;

  error("Download path unwritable");
  messageBoxOk("Unwritable", "Your add-ons folder appears to be read-only! We're unable to download anything.");
}

function GMM_AddonPage::commentClick(%this) {
  %window = GlassModManagerGui_CommentWindow;
  %window.setVisible(true);
  %window.deleteAll();

  %window.scroll = new GuiScrollCtrl() {
    profile = "GlassScrollProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "10 35";
    extent = "380 155";
    minExtent = "8 2";
    enabled = "1";
    visible = "1";
    clipToParent = "1";
    willFirstRespond = "0";
    hScrollBar = "alwaysOff";
    vScrollBar = "alwaysOn";
    constantThumbHeight = "0";
    childMargin = "5 5";
    rowHeight = "40";
    columnWidth = "30";
  };

  %window.textSwatch = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 0";
    position = "5 5";
    extent = "365 14";
  };

  %window.textEdit = new GuiMLTextEditCtrl(GMM_AddonPage_CommentText) {
    profile = "GlassMLTextEditProfile";
    horizSizing = "right";
    vertSizing = "bottom";
    position = "0 0";
    minextent = "355 14";
    extent = "355 14";
    autoResize = true;
  };

  %window.submit = new GuiBitmapButtonCtrl() {
    profile = "GlassBlockButtonWhiteProfile";

    position = "340 197";
    extent = "50 20";
    bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";

    text = "Post";

    command = "GMM_AddonPage.submitComment();";

    mColor = "84 217 140 255";
  };

  %window.textSwatch.add(%window.textEdit);
  %window.scroll.add(%window.textSwatch);
  %window.add(%window.scroll);
  %window.add(%window.submit);

  %window.forceCenter();
}

function GMM_AddonPage_CommentText::onResize(%this) {

  %window = GlassModManagerGui_CommentWindow;

  if(%this.lastValue !$= %this.getValue()) {
    %this.lastValue = %this.getValue();

    %this.extent = "355 0";
    %window.textSwatch.extent = "365 0";

    %this.forceReflow();
    return;
  }

  %window.textSwatch.extent = 365 SPC getWord(%this.extent, 1);
  %window.textSwatch.setVisible(true);

  %window.scroll.scrollToBottom();
  %window.textEdit.makeFirstResponder(true);
}

function GMM_AddonPage::submitComment(%this) {
  %text = GMM_AddonPage_CommentText.getValue();
  if(strlen(trim(%text)) == 0) {
    glassMessageBoxOk("Blank Comment", "You didn't comment anything!");
    GMM_AddonPage_CommentText.setValue("");
    return;
  }

  GMM_AddonPage_CommentText.enabled = false;
  GlassModManager::placeCall("comment", %str = "id" TAB %this.container.addonId NL "newcomment" TAB expandEscape(%text), "GMM_AddonPage.commentSubmitted");
}

function GMM_AddonPage::commentSubmitted(%this, %res) {
  if(%res.status $= "success") {
    glassMessageBoxOk("Comment Submitted", "Your comment has been posted.");
    GlassModManagerGui_CommentWindow.setVisible(false);
    GMM_Navigation.steps--;
    GlassModManagerGui.openPage(GMM_AddonPage, %this.container.addonId);
  } else {
    glassMessageBoxOk("Comment Failed", "Your comment could not be posted.");
  }
}
