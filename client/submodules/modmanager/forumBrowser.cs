function GlassModManagerGui::renderForumTopic(%title, %text, %links) {
  for(%i = 0; %i < getFieldCount(%links); %i++) {
    %link = getField(%links, %i);
    %fileName = fileName(%link);
    echo(%fileName);
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
    text = "<font:quicksand:16><just:left>" @ %text;
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
  %container.description.forceReflow();

  %container.blf.setMargin(10, 10);

  %container.title.setMarginResize(10);
  %container.title.placeBelow(%container.blf, 10);

  %container.description.placeBelow(%container.title, 10);

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

  %cleanText = strReplace(%cleanText, "&nbsp;", " ");
  %cleanText = strReplace(%cleanText, "&#039;", "'");

  echo(trim(%links));
  GlassModManagerGui::setProgress("");
  GlassModManagerGui::renderForumTopic(%title, %cleanText, %links);
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
            echo("Buffer done");
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
          echo("Buffer!");
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

        %name = getSubStr(%line, strpos(%line, "</a> - ")+8, strlen(%line));
        %name = getSubStr(%line, 0, strpos(%line, "<br />"));

        echo("\c4" @ %urlTopic @ " \c0links to\c2 " @ %text @ " \c0by\c4 " @ %name);
      }
    }
  } else if(%this.type $= "topic") {
    if(%this.buffering) {
      GlassForumBrowser::processPostBuffer(%this.buffer, %this.title);
    }
  }
}
