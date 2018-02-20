function GlassLog::init() {
  if(!isObject(GlassLog))
    new ScriptObject(GlassLog) {
      folder         = "config/log/";

  		echoLevel      = 1;
  		echoFormatting = true;
    };

  %time = strreplace(getDateTime(), ":", "_");
  %time = strreplace(%time        , "/", "_");
  %time = strreplace(%time        , " ", "-");

  GlassLog.startTime = %time;

  new FileObject(GlassLogFO);

  //create directory, pretty hacky
  GlassLogFO.openforwrite("config/log/blockland/.blank");
  GlassLogFO.writeline(" ");
  GlassLogFO.close();
  GlassLogFO.openforwrite("config/log/glass/.blank");
  GlassLogFO.writeline(" ");
  GlassLogFO.close();

  fileDelete("config/log/blockland/.blank");
  fileDelete("config/log/glass/.blank");
}

function GlassLog::cleanOld(%type) {

  // build lists

  %file = findFirstFile(%srch = "config/log/" @ %type @ "/*.log");

  while(%file !$= "") {
    %basename = getSubStr(%file, 21, strlen(%file)-21);
    %date     = getSubStr(%basename, strpos(%basename, "-")+1, strlen(%basename));
    %date     = strReplace(%date, ".log", "");
    %prefix   = getSubStr(%basename, 0, strpos(%basename, "-"));

    if(!hasWord(%pres, %prefix)) {
      %pres = trim(%pres SPC %prefix);
    }

    %list[%prefix] = %list[%prefix] SPC %date;

    %file = findNextFile(%srch);
  }

  // clear files from lists

  %cap = GlassSettings.get("Log::LogCount");

  for(%i = 0; %i < getWordCount(%pres); %i++) {
    %prefix = getWord(%pres, %i);

    %list = sl_sort(%list[%prefix]);
    %listLen = getWordCount(%list);

    echo(%prefix @ " has " @ %listLen @ " entries.");

    if(%listLen <= %cap)
      continue;

    %del = %listLen - %cap;
    for(%j = 0; %j < %del; %j++) {
      %name = getWord(%list, %j);
      %path = "config/log/" @ %type @ "/" @ %prefix @ "-" @ %name @ ".log";
      echo("Delete " @ %path);
      fileDelete(%path);
    }
  }

}

function GlassLog::startSessionLog() {
  //begins a full console log for this instance of Blockland
  if(isObject(GlassSessionLogger))
    return;

  %path = GlassLog.folder @ "blockland/";
  %path = %path @ ($Server::Dedicated ? "server-" : "client-");
  %path = %path @ GlassLog.startTime;
  %path = %path @ ".log";

  echo("Beginning session log in \c3" @ %path @ "\c0...");

  new ConsoleLogger(GlassSessionLogger, %path);
  GlassSessionLogger.level = 0;

  echo("Duplicate Log Started\n\n");

  //GlassSessionLogger.attach();
}

function GlassLog::log(%str, %level, %baseLevel) {
  if(!isObject(GlassLogFO))
    new FileObject(GlassLogFO);

  if(%level $= "")
    %level = 1;

  if(%baseLevel $= "")
    %baseLevel = %level;

  %levelText[0] = "debug";
  %levelText[1] = "log";
  %levelText[2] = "error";

  %levelText     = %levelText[%level+0];
  %baseLevelText = %levelText[%baseLevel+0];

  if(%levelText $= "" || %baseLevelText $= "") {
	  echo("[Glass Log] Invalid logging level \"" @ %level @ "\"");
	  return; //??
  }

  %path = GlassLog.folder @ "glass/";
  %path = %path @ ($Server::Dedicated ? "server_" : "client_");
  %path = %path @ %levelText;
  %path = %path @ "-";
  %path = %path @ GlassLog.startTime;
  %path = %path @ ".log";

  %time = getDateTime();

  GlassLogFO.openForAppend(%path);
  if(isFunction("getLineCount")) { // function not present in preload exec

    for(%i = 0; %i < getLineCount(%str); %i++) {
      %line = getLine(%str, %i);
      GlassLogFO.writeLine("[" @ %time @ "]\t[" @ %baseLevelText @ "]\t" @ expandEscape(stripMLControlChars(%line)));
    }

  } else {

    // alternatively write full chunk. bad formatting but not frequent
    GlassLogFO.writeLine("[" @ %time @ "]\t[" @ %baseLevelText @ "]\t" @ expandEscape(stripMLControlChars(%str)));

  }
  GlassLogFO.close();

  if(%level > 0)
    GlassLog::log(%str, %level-1, %baseLevel);

  if(%level == %baseLevel) {
	 if(GlassLog.echoLevel <= %level) {
	   if(GlassLog.echoFormatting) {
		  echo("\c4[Glass " @ strCap(%levelText) @ "] \c0" @ %str);
		} else {
		  echo(%str);
		}
	 }
  }
}

function GlassLog::error(%str) {
  GlassLog::log(%str, 2);
}


function GlassLog::debug(%str) {
  GlassLog::log(%str, 0);
}

function strcap(%str) {
	return strupr(getsubstr(%str, 0, 1)) @ strlwr(getsubstr(%str, 1, strlen(%str)-1));
}

function sl_sort(%str) {
  // I was gonna implement quicksort
  // but that's just a lot of work
  // and this is Blockland..

  %sorted = "";
  for(%i = 0; %i < getWordCount(%str); %i++) {
    %word = getWord(%str, %i);

    %inserted = false;
    for(%j = 0; %j < getWordCount(%sorted); %j++) {
      %cmpWord = getWord(%sorted, %j);

      %cmp = strCmp(%word, %cmpWord);
      if(%cmp != 1) {
        // either a tie or comes before
        %sorted = setWord(%sorted, %j, %word SPC %cmpWord);
        %inserted = true;
        break;
      }
    }

    if(!%inserted) {
      %sorted = trim(%sorted SPC %word);
    }
  }

  return %sorted;
}

function hasWord(%str, %word) {
  for(%i = 0; %i < getWordCount(%str); %i++) {
    if(getWord(%str, %i) $= %word)
      return true;
  }

  return false;
}
