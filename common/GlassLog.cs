function GlassLog::init() {
  if(!isObject(GlassLog))
    new ScriptObject(GlassLog) {
      folder = "config/log/";
    };

  %time = strreplace(getDateTime(), ":", "_");
  %time = strreplace(%time        , "/", "_");
  %time = strreplace(%time        , " ", "-");

  GlassLog.startTime = %time;

  new FileObject(GlassLogFO);

  //create directory, pretty hacky
  GlassLogFO.openForWrite("config/log/blockland/.blank");
  GlassLogFO.write(" ");
  GlassLogFO.close();
  GlassLogFO.openForWrite("config/log/glass/.blank");
  GlassLogFO.write(" ");
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

  //GlassSessionLogger.attach();
}

function GlassLog::log(%str, %level, %baseLevel) {
  if(!isObject(GlassLogFO))
    new FileObject(GlassLogFO);

  if(%level $= "")
    %level = 1;

  if(%baseLevel $= "")
    %baseLevel = %level;

  switch(%level+0) {
    case 0:
      //extremely verbose output containing everything
      %levelText = "debug";

    case 1:
      //standard output with errors
      %levelText = "log";

    case 2:
      //errors only
      %levelText = "error";

    default:
      error("[Glass Log] Invalid logging level!");
      return;

  }

  %path = GlassLog.folder @ "glass/";
  %path = %path @ GlassLog.startTime;
  %path = %path @ ($Server::Dedicated ? "-server-" : "-client-");
  %path = %path @ %levelText;
  %path = %path @ ".log";

  %time = getDateTime();

  GlassLogFO.openForAppend(%path);
  for(%i = 0; %i < getLineCount(%str); %i++) {
    %line = getLine(%str, %i);
    GlassLogFO.writeLine("[" @ %time @ "]\t[" @ %levelText @ "]\t" @ expandEscape(%line));
  }
  GlassLogFO.close();

  if(%level > 0)
    GlassLog::log(%str, %level-1, %baseLevel);
}
