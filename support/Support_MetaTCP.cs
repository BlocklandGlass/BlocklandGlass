//----------------------------------------------------------------------
// Title:   Support_MetaTCP
// Author:  Jincux/Scout31
// Version: 1
// Updated: July 7th, 2016
//----------------------------------------------------------------------
// Include this code in your own scripts as an *individual file*
// Requires Support_TCPClient
//----------------------------------------------------------------------

if($Support::MetaTCP::Version > 1)
  return;

$Support::MetaTCP::Version = 1;

function getUrlMetadata(%url, %callback) {
  if(strpos(%this.callback, "(") > -1) {
    error("ERROR: Please use a function name for callback");
    return;
  }
  %method = "HEAD";
  %class = "MetaTCP";

  %components = urlGetComponents(%url);
	%protocol = getField(%components, 0);
	%server = getField(%components, 1);
	%port = getField(%components, 2);
	%path = getField(%components, 3);
	%query = getField(%components, 4);

	if(%protocol $= "HTTPS")
	{
		error("ERROR: Blockland cannot handle HTTPS links. Trying HTTP port 80 instead.");
		%port = 80;
	}

  %tcp = TCPClient(%method, %server, %port, %path, %query, %savePath, %class, %options);
  %tcp.callback = %callback;
  return %tcp;
}

function MetaTCP::onLine(%this, %line) {
  %len = strLen(%this.buffer);

  if(strPos(getWord(%line, 0), ":") > -1) {
    %key = getWord(%line, 0);
    %key = getSubStr(%key, 0, strlen(%key)-1);
    %value = getWords(%line, 1);
    %this.header[%key] = %value;
  }

  if(%len) {
    %this.buffer = %this.buffer NL %line;
  } else {
    %this.buffer = %line;
  }
}

function MetaTCP::onDone(%this, %error) {
  eval(%this.callback @ "(%this, %error);");
}
