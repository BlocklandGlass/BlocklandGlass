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
  %path = %path @ GlassLog.startTime;
  %path = %path @ ($Server::Dedicated ? "-server-" : "-client-");
  %path = %path @ %levelText;
  %path = %path @ ".log";

  %time = getDateTime();

  GlassLogFO.openForAppend(%path);
  if(isFunction("getLineCount")) { // function not present in preload exec

    for(%i = 0; %i < getLineCount(%str); %i++) {
      %line = getLine(%str, %i);
      GlassLogFO.writeLine("[" @ %time @ "]\t[" @ %baseLevelText @ "]\t" @ expandEscape(%line));
    }

  } else {

    // alternatively write full chunk. bad formatting but not frequent
    GlassLogFO.writeLine("[" @ %time @ "]\t[" @ %baseLevelText @ "]\t" @ expandEscape(%str));

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
