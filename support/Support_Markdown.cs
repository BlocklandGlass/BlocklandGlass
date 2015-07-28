//markdown support specific to GlassModManager
//I really wouldn't suggest reusing this

function parseMarkdown(%text) {
  %finalText = "";

  %markdown["h1"] = "<font:arial bold:20>";
  %markdown["h2"] = "<font:arial bold:18>";
  %markdown["h3"] = "<font:arial bold:16>";
  %markdown["h4"] = "<font:arial bold:14>";

  %inBold = false;
  %underscoreCount = 0;
  %headerScale = 0;
  for(%i = 0; %i < strlen(%text); %i++) {
    %char = getsubstr(%text, %i, 1);
    if(%char $= "#") {
      //header
      %headerScale++;
      continue;
    } else if(%headerScale > 0) {
      %finalText = %finalText @ %markdown["h" @ %headerScale];
      %headerScale = 0;
    }

    if(%char $= "_") {
      %underscoreCount++;
      continue;
    } else if(%underscoreCount == 2) {
      %underscoreCount = 0;
      if(!%inBold) {
        %finalText = %finalText @ "<font:arial bold:14>";
      } else {
        %finalText = %finalText @ "<font:arial:14>";
      }

      %inBold = !%inBold;
    } else if(%underscoreCount > 0) {
      for(%a = 0; %a < %underscoreCount; %a++) {
        %finalText = %finalText @ "_";
      }
      %underscoreCount = 0;
    }

    if(%char $= "\n") {
      %finalText = %finalText @ "<br><font:arial:14>";
      continue;
    }

    if(%char $= ">") {
      //not supported
      continue;
    }

    if(%char $= "*") {
      %finalText = %finalText @ "    + ";
      continue;
    }

    %finalText = %finalText @ %char;
  }

  return %finalText;
}
