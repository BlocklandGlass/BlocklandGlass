function GlassModManagerGui::renderForumTopic(%title, %text, %links) {
  for(%i = 0; %i < getFieldCount(%links); %i++) {
    %link = getField(%links, %i);
    %fileName = fileName(%link);
    echo(%fileName);
    if(strpos(%link, "blocklandglass.com") >= 0 && strpos(%fileName, "addon.php?") == 0) {
      %aid = getSubStr(%filename, strpos(%filename, "=")+1, strlen(%filename));
      %button[%link] = %button[%buttons+0] = "addon" TAB %aid;
    } else if(strpos(%filename, ".zip") >= 0) {
      %button[%link] = %button[%buttons+0] = "zip" TAB %link TAB %filename;
    } else {
      %button[%link] = %button[%buttons+0] = "link" TAB %link;
    }
    echo(%button[%buttons+0]);
    %buttons++;
  }

  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %container.blf = new GuiBitmapCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    bitmap = "Add-Ons/System_BlocklandGlass/image/smflogo.png";
    position = "20 20";
    extent = "400 50";
    minextent = "0 0";
    clipToParent = true;
  };

  %container.blf.mouse = new GuiMouseEventCtrl(GlassModManagerGui_ForumButton) {
    type = "board";
    board = 0;
    swatch = %container.blf;
  };

  %container.blf.add(%container.blf.mouse);

  %container.title = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:quicksand-bold:24><just:left>" @ %title;
    position = "102 30";
    extent = "300 24";
    minextent = "0 0";
    autoResize = true;
  };

  %container.description = new GuiMLTextCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    text = "<font:quicksand:16><just:left>";
    position = "102 30";
    extent = "300 16";
    minextent = "0 0";
  };

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);

  %container.add(%container.blf);
  %container.add(%container.title);
  %container.add(%container.description);

  %container.title.setMarginResize(10);
  %container.title.setVisible(true);
  %container.title.forceReflow();

  %container.description.setMarginResize(10);
  %container.description.setVisible(true);

  %container.blf.setMargin(10, 10);

  %container.title.setMarginResize(10);
  %container.title.placeBelow(%container.blf, 10);

  %container.description.placeBelow(%container.title, 10);

  for(%j = 0; %j < getLineCount(%text); %j++) {
    %ln = getLine(%text, %j);
    if(getWord(%ln, 0) $= "{link") {
      %i = %currentLink+0;
      %currentLink++;
      %button = %button[%i];


      %container.button[%i] = new GuiSwatchCtrl() {
        horizSizing = "right";
        vertSizing = "bottom";
        color = "200 220 200 255";
        position = 0 SPC 0;
        extent = 150 SPC 35;
      };

      switch$(getField(%button, 0)) {
        case "addon":
          %t = "<font:quicksand-bold:16><just:center>Glass Add-On<font:quicksand:14><br>ID " @ getField(%button, 1);

        case "link":
          %t = "<font:quicksand-bold:16><just:center>Url<font:quicksand:14><br>" @ getField(%button, 1);

        case "zip":
          %t = "<font:quicksand-bold:16><just:center>ZIP File<font:quicksand:14><br>" @ getField(%button, 2);
      }
      %container.button[%i].info = new GuiMLTextCtrl(%name) {
        horizSizing = "center";
        vertSizing = "center";
        text = %t;
        position = "0 0";
        extent = "300 16";
        minextent = "0 0";
        autoResize = true;
      };

      %container.button[%i].add(%container.button[%i].info);
      echo("button:" @ %button);
      if(getField(%button, 0) $= "addon") {
        %container.button[%i].mouse = new GuiMouseEventCtrl(GlassModManagerGui_AddonButton) {
          aid = getField(%button, 1);
          swatch = %container.button[%i];
        };
      } else {
        %container.button[%i].mouse = new GuiMouseEventCtrl(GlassModManagerGui_ForumButton) {
          type = "external";
          button = %button;
          link = getField(%button, 1);
          swatch = %container.button[%i];
        };
      }
      echo(%container.button[%i].mouse);
      %container.button[%i].add(%container.button[%i].mouse);

      %container.button[%i].info.setMarginResize(2, 2);
      %container.button[%i].info.forceCenter();

      %container.description.forceReflow();

      %container.add(%container.button[%i]);
      %container.button[%i].forceCenter();
      %container.button[%i].placeBelow(%container.description, 5);
      %container.description.setValue(%container.description.getValue() @ "<br><br><br><br>");
    } else {
      %container.description.setValue(%container.description.getValue() @ %ln);
    }
  }

  %container.description.forceReflow();

  %container.verticalMatchChildren(10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.setVisible(true);
}

function GlassModManagerGui::renderForumBoard(%topics) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  for(%i = 0; %i < getlinecount(%topics); %i++) {
    %topic = getLine(%topics, %i);
    %container.topic[%i] = new GuiSwatchCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      color = "200 200 200 255";
      position = "0 0";
      extent = "505 30";
    };

    %topicTitle = getSubStr(getField(%topic, 0), 0, 40);
    if(%topicTitle !$= getField(%topic, 0))
      %topicTitle = %topicTitle @ "...";


    %container.topic[%i].title = new GuiMLTextCtrl() {
      horizSizing = "right";
      vertSizing = "bottom";
      text = "<font:quicksand-bold:16><just:left>" @ %topictitle @ "<just:right><font:quicksand:16>" @ getField(%topic, 1);
      position = "0 0";
      extent = "300 24";
      minextent = "0 0";
      autoResize = true;
    };

    %container.topic[%i].mouse = new GuiMouseEventCtrl(GlassModManagerGui_ForumButton) {
      type = "topic";
      topic = getField(%topic, 2);
      swatch = %container.topic[%i];
    };

    %container.topic[%i].add(%container.topic[%i].title);
    %container.add(%container.topic[%i]);
    %container.topic[%i].setMarginResize(10);
    %container.topic[%i].title.setMarginResize(10, 7);
    %container.topic[%i].title.forceCenter();
    %container.topic[%i].add(%container.topic[%i].mouse);
    if(%i) {
      %container.topic[%i].placeBelow(%container.topic[%i-1], 5);
    } else {
      %container.topic[%i].setMargin(10, 10);
    }
  }

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);

  %container.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.verticalMatchChildren(498, 10);
  GlassModManagerGui_MainDisplay.setVisible(true);
}

function GlassForumBrowser::getBoard(%page) {
  //http://forum.blockland.us/index.php?board=34.0;wap
  %dec = %page*9;

  %url = "http://forum.blockland.us/index.php?board=34." @ %dec @ ";wap";
  echo(%url);
  %method = "GET";
  %downloadPath = "";
  %className = "GlassForumTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.type = "board";
}

function GlassForumBrowser::getAddon(%topic) {
  //http://forum.blockland.us/index.php?board=34.0;wap
  %dec = %page*9;

  %url = "http://forum.blockland.us/index.php?topic=" @ mFloor(%topic) @ ".0";
  echo(%url);
  %method = "GET";
  %downloadPath = "";
  %className = "GlassForumTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  %tcp.type = "topic";

  GlassModManagerGui::setProgress(0, "Connecting to forum.blockland.us...");
}

function GlassForumBrowser::processPostBuffer(%post, %title) {
  %cleanText = "";



  %str = %post;
  while(strlen(%str) > 0) {

    //tag removal
    if((%pos = strpos(%str, "<")) >= 0) {
      %cleanText = %cleanText @ getSubStr(%str, 0, %pos);

      %tag = getSubStr(%str, %pos, strPos(%str, ">", %pos)-%pos);
      %contents = getSubStr(%tag, 1, strlen(%tag)-1);

      %type = getWord(%contents, 0);
      if(%type $= "a") {
        for(%i = 0; %i < getWordCount(%contents); %i++) {
          %word = getWord(%contents, %i);
          if(strpos(%word, "href") == 0) {
            %href = getSubStr(%word, 6, strlen(%word)-7);
            if(strpos(%href, "http") == 0) {
              %links = %links TAB %href;
            }
            %cleanText = %cleanText NL "{link\t" @ %href @ "}\n";
            echo(%href);
            break;
          }
        }
      }

      switch$(%contents) {
        case "br /":
          %cleanText = %cleanText @ "<br>";

        case "b":
          %cleanText = %cleanText @ "<font:quicksand-bold:16>";

        case "/b":
          %cleanText = %cleanText @ "<font:quicksand:16>";

        case "li":
          %cleanText = %cleanText @ "<br> + ";

        case "/ul":
          %cleanText = %cleanText @ "<br><br>";

        case "div align=\"center\"":
          %cleanText = %cleanText @ "<just:center>";

        case "/div":
          %cleanText = %cleanText @ "<br><just:left>";
      }

      %str = getSubStr(%str, strPos(%str, ">", %pos)+1, strlen(%str));
    } else {
      break;
    }
  }

  %cleanText = urldec(%cleanText);

  echo(trim(%links));
  GlassModManagerGui::setProgress("");
  GlassModManagerGui::renderForumTopic(%title, %cleanText, %links);
}

function urldec(%str) {
  %str = strReplace(%str, "&nbsp;", " ");
  %str = strReplace(%str, "&#039;", "'");
  %str = strReplace(%str, "&quot;", "\"");

  %str = strReplace(%str, "&#9658;", " > ");
  %str = strReplace(%str, "&#9608;", "[]");

  %str = strReplace(%str, "&amp;", "&");

  return %str;
}

function GlassForumTCP::handleText(%this, %text) {
  if(%this.type $= "board") {
    %this.buffer = %this.buffer NL %text;
  } else if(%this.type $= "topic") {
    if(!%this.buffering && %this.buffer !$= "") {
      return;
    }

    %line = %text;
    %text = trim(%text);

    if((%pos = strpos(%text, "<title>")) >= 0) {
      %this.title = getSubStr(%text, %pos+7, strpos(%text, "</title>", %pos)-%pos-7);
      return;
    }

    while(strlen(%text) > 0) {
      if(%this.buffering) {
        %pos1 = strpos(%text, "</div>");
        %pos2 = strpos(%text, "<div");

        if((%pos1 < %pos2 || %pos2 == -1) && %pos1 != -1) {
          %pos = %pos1;
          if(%this.divLevel) {
            %this.divLevel--;
            %this.buffer = %this.buffer @ getSubStr(%text, 0, %pos+6);
            %text = getSubStr(%text, %pos+6, strlen(%text));
            continue;
          } else {
            %this.buffer = %this.buffer @ getSubStr(%text, 0, %pos);
            %this.buffering = false;
            GlassModManagerGui::setProgress(0.5, "Text received.");
            GlassForumBrowser::processPostBuffer(%this.buffer, %this.title);

            %this.disconnect();
            break;
          }
        } else if(%pos2 != -1) {
          %pos = %pos2;
          %this.divLevel++;
          %this.buffer = %this.buffer @ getSubStr(%text, 0, %pos+4);
          %text = getSubStr(%text, %pos+4, strlen(%text));
          continue;
        } else {
          %this.buffer = %this.buffer @ %text;
          break;
        }
      } else {
        if((%pos = strpos(%text, "<div class=\"post\">")) == 0 && %this.buffer $= "") {
          GlassModManagerGui::setProgress(0.25, "Receiving text...");
          %this.buffering = true;
          %this.divLevel = 0;
          %text = getSubStr(%text, %pos+strlen("<div class=\"post\">"), strlen(%text));
          continue;
        }
      }

      break;
    }
  }
}

function GlassForumTCP::onDone(%this, %error) {
  if(%this.type $= "board") {
    %topics = "";
    for(%i = 0; %i < getLineCount(%this.buffer); %i++) {
      %line = trim(getLine(%this.buffer, %i));
      //echo(%line);
      if(%line $= "<card id=\"main\ title=\"Add-Ons\">") {
        %inCard = true;
        %pCount = 0;
        continue;
      }

      if(strpos(%line, "<p>") == 0) {
        %pCount++;
      }

      if(strpos(%line, "<p>Pages: ") == 0 && %pCount > 3) {
        break;
      }

      if(%pCount >= 3) {
        %urlTopic = getSubStr(%line, strpos(%line, "topic=")+6, strlen(%line));
        %urlTopic = getSubStr(%urlTopic, 0, strpos(%urlTopic, ";wap")); //wap links don't include download links, so..

        %text = getSubStr(%line, strpos(%line, "\">")+2, strlen(%line));
        %text = getSubStr(%text, 0, strpos(%text, "</a>"));

        %name = getSubStr(%line, strpos(%line, "</a> - ")+7, strlen(%line));
        %name = getSubStr(%name, 0, strpos(%name, "<br />"));

        echo("\c4" @ %urlTopic @ " \c0links to\c2 " @ %text @ " \c0by\c4 " @ %name);
        %topics = %topics NL urldec(%text) TAB %name TAB %urlTopic;
      }
    }
    GlassModManagerGui::renderForumBoard(getSubStr(%topics, 1, strlen(%topics)-1));
  } else if(%this.type $= "topic") {
    if(%this.buffering) {
      GlassForumBrowser::processPostBuffer(%this.buffer, %this.title);
    }
  }
}
