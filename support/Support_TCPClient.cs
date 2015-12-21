//----------------------------------------------------------------------
// Title:   Support_TCPClient
// Author:  Greek2me
// Version: 11
// Updated: December 4, 2015
//----------------------------------------------------------------------
// Include this code in your own scripts as an *individual file*
// called "Support_TCPClient.cs". Do not modify this code.
//----------------------------------------------------------------------

if($TCPClient::version >= 11 && !$Debug)
	return;
$TCPClient::version = 11;

$TCPClient::timeout = 20000;
$TCPClient::redirectWait = 500;
$TCPClient::retryConnectionWait = 2000;
$TCPClient::retryConnectionCount = 1;

$TCPClient::Error::none = 0;
$TCPClient::Error::connectionFailed = 1;
$TCPClient::Error::dnsFailed = 2;
$TCPClient::Error::invalidResponse = 3;
$TCPClient::Error::invalidRedirect = 4;
$TCPClient::Error::invalidDownloadLocation = 5;
$TCPClient::Error::invalidUrlFormat = 6;
$TCPClient::Error::connectionTimedOut = 7;

$TCPClient::Debug = false;
$TCPClient::PrintErrors = false;

//Creates a connection to the server at the specified URL.
//@param	string url	The URL to connect to. For example, http://mods.greek2me.us/example.php?param=test&whatever.
//@param	string method	The HTTP method to use. (GET, POST, PUT, etc.) Defaults to GET if blank. Be sure to urlEnc() parameters.
//@param	string savePath	The local filepath to save binary files to. Text files will only download in binary mode if this is set.
//@param	string class	The name of a class which can be used to change/extend functionality.
//@see	TCPClient
//@return	TCPObject	The TCP object that is performing the connection.
function connectToURL(%url, %method, %savePath, %class)
{
	if(!strLen(%method))
		%method = "GET";

	%components = urlGetComponents(%url);
	%protocol = getField(%components, 0);
	%server = getField(%components, 1);
	%port = getField(%components, 2);
	%path = getField(%components, 3);
	%query = getField(%components, 4);

	if(%protocol $= "HTTPS")
	{
		warn("WARN: Blockland cannot handle HTTPS links. Trying HTTP port 80 instead.");
		%port = 80;
	}

	return TCPClient(%method, %server, %port, %path, %query, %savePath, %class);
}

//Creates a TCP connection to the specified server. If desired, a custom request can be specified immediately after function call using %tcp.request.
//@param	string method	The HTTP method to use. (GET, POST, PUT, etc.)
//@param	string server	The server to connect to.
//@param	int port	The port to use. Defaults to 80 if blank.
//@param	string path	The location of the file on the server. Defaults to "/" if blank.
//@param	string query	The parameters to be sent with the request. Be sure to use urlEnc() on the parameters. Must be formatted like this: myArg=value&testarg=stuff&whatever=1
//@param	string savePath	The local filepath to save binary files to. Text files will only download in binary mode if this is set.
//@param	string class	The name of a class which can be used to change/extend functionality.
//@return	TCPObject	The TCP object that is performing the connection.
function TCPClient(%method, %server, %port, %path, %query, %savePath, %class)
{
	if(!strLen(%path))
		%path = "/";
	if(!strLen(%port))
		%port = 80;

	%tcp = new TCPObject(TCPClient)
	{
		className = %class;

		protocol = "HTTP/1.0";
		method = %method;
		server = %server;
		port = %port;
		path = %path;
		query = %query;
		savePath = %savePath;

		retryConnectionCount = 0;
	};

	if(!strLen(%server))
	{
		%tcp.onDone($TCPClient::Error::invalidUrlFormat);
		return 0;
	}
	else if(strLen(%savePath) && !isWriteableFileName(%savePath))
	{
		%tcp.onDone($TCPClient::Error::invalidDownloadLocation);
		return 0;
	}

	%tcp.schedule(0, "connect", %server @ ":" @ %port);
	cancel(%tcp.timeoutSchedule);
	%tcp.timeoutSchedule = %tcp.schedule($TCPClient::timeout, "onDone", $TCPClient::Error::connectionTimedOut);

	return %tcp;
}

//Creates the request which is sent to the server. To use with your mod, replace "TCPClient" with your class name.
function TCPClient::buildRequest(%this)
{
	%len = strLen(%this.query);
	%path = %this.path;

	if(%len)
	{
		%type		= "Content-Type: application/x-www-form-urlencoded\r\n";
		if(%this.method $= "GET" || %this.method $= "HEAD")
		{
			%path	= %path @ "?" @ %this.query;
		}
		else
		{
			%length	= "Content-Length:" SPC %len @ "\r\n";
			%body	= %this.query;
		}
	}
	%requestLine	= %this.method SPC %path SPC %this.protocol @ "\r\n";
	%host			= "Host:" SPC %this.server @ "\r\n";
	%ua				= "User-Agent: Torque/1.3\r\n";
	%request = %requestLine @ %host @ %ua @ %length @ %type @ "\r\n" @ %body;

	if(isFunction(%this.className, "buildRequest"))
		return eval(%this.className @ "::buildRequest(%this, %request);");
	return %request;
}

//Called when the connection has been established. To use with your mod, replace "TCPClient" with your class name.
function TCPClient::onConnected(%this)
{
	%this.isConnected = true;

	if(isFunction(%this.className, "onConnected"))
		eval(%this.className @ "::onConnected(%this);");

	%this.httpStatus = "";
	%this.receiveText = false;
	%this.redirect = false;
	cancel(%this.retrySchedule);

	if(strLen(%this.request))
		%request = %this.request;
	else
		%request = %this.buildRequest();

	if($TCPClient::Debug)
		echo(" > REQUEST:\n   >" SPC strReplace(%request, "\r\n", "\n   > "));
	%this.send(%request);

	cancel(%this.timeoutSchedule);
	%this.timeoutSchedule = %this.schedule($TCPClient::timeout, "onDone", $TCPClient::Error::connectionTimedOut);
}

//Called when DNS has failed.
//@private
function TCPClient::onDNSFailed(%this)
{
	%this.onDone($TCPClient::Error::dnsFailed);
}

//Called when the connection has failed.
//@private
function TCPClient::onConnectFailed(%this)
{
	if(%this.retryConnectionCount < $TCPClient::retryConnectionCount)
	{
		%this.retryConnectionCount ++;
		if($TCPClient::PrintErrors)
		{
			warn("WARN (TCPClient): Connection to server" SPC %this.server SPC "failed." SPC
				"Retrying in" SPC $TCPClient::retryConnectionWait @ "ms" SPC
				"(retry" SPC %this.retryConnectionCount SPC "of" SPC $TCPClient::retryConnectionCount @ ")");
		}
		cancel(%this.retrySchedule);
		%this.retrySchedule = %this.schedule(
			$TCPClient::retryConnectionWait,
			"connect", %this.server @ ":" @ %this.port);
	}
	else
		%this.onDone($TCPClient::Error::connectionFailed);
}

//Called when the connection is closed.
//@private
function TCPClient::onDisconnect(%this)
{
	%this.isConnected = false;
	if(!%this.redirect)
		%this.onDone($TCPClient::Error::none);
}

//Called when the connection has completed. To use with your mod, replace "TCPClient" with your class name.
//@param	int error The error message. 0 if no error.
//@return	int	The error message, related to $TCPClient::Error::[none|connectionFailed|dnsFailed|invalidResponse|connectionTimedOut|invalidRedirect|invalidDownloadLocation|invalidUrlFormat]
function TCPClient::onDone(%this, %error)
{
	if(%error && $TCPClient::PrintErrors)
	{
		switch(%error)
		{
			case $TCPClient::Error::connectionFailed: %desc = "Server not found.";
			case $TCPClient::Error::connectionTimedOut: %desc = "Connection timed out.";
			case $TCPClient::Error::dnsFailed: %desc = "DNS lookup failed.";
			case $TCPClient::Error::invalidDownloadLocation: %desc = "Invalid download location:" SPC %this.savePath;
			case $TCPClient::Error::invalidRedirect: %desc = "Cannot handle this redirect.";
			case $TCPClient::Error::invalidResponse: %desc = "Invalid response:" SPC %this.httpStatus;
			case $TCPClient::Error::invalidUrlFormat: %desc = "Invalid URL format.";
		}
		if(strLen(%desc))
			%desc = ": " @ %desc;
		echo("\c2ERROR (TCPClient): error" SPC %error SPC "for connection to" SPC %this.server @ %desc);
	}
	if(isFunction(%this.className, "onDone"))
		eval(%this.className @ "::onDone(%this, %error);");

	if(%this.isConnected)
	{
		%this.disconnect();
		%this.isConnected = false;
	}

	//cleanup and delete
	cancel(%this.timeoutSchedule);
	%this.schedule(0, "delete");

	return %error;
}

//Called when a line is received from the server.
//@param	string line	The line received.
//@see	TCPClient::handleText
//@see	TCPClient::onBinChunk
//@private
function TCPClient::onLine(%this, %line)
{
	if($TCPClient::Debug)
		echo(" > " @ %line);
	cancel(%this.timeoutSchedule);
	%this.timeoutSchedule = %this.schedule($TCPClient::timeout, "onDone", $TCPClient::Error::connectionTimedOut);
	if(%this.receiveText)
	{
		%this.handleText(%line);
	}
	else
	{
		if(strLen(%line))
		{
			if(!%this.httpStatus && strPos(%line, "HTTP/") == 0)
			{
				%this.httpStatus = getWord(%line, 1);
				%this.httpStatusDesc = getWord(%line, 2);
				if(%this.httpStatus >= 400)
				{
					%this.onDone($TCPClient::Error::invalidResponse);
					return;
				}
				else if(%this.httpStatus >= 300)
				{
					if(%this.redirected)
					{
						%this.onDone($TCPClient::Error::invalidRedirect);
						return;
					}
					else
					{
						%this.redirect = true;
					}
				}
			}
			else
			{
				%field = getWord(%line, 0);
				%fieldLength = strLen(%field);
				if(%fieldLength > 0)
				{
					%field = getSubStr(%field, 0, %fieldLength - 1);
					%value = getWords(%line, 1);
					%this.headerField[%field] = %value;
				}

				switch$(%field)
				{
					case "Location":
						if(%this.redirect)
						{
							%this.disconnect();

							if(strPos(%value, "/") == 0)
							{
								%this.path = %value;
							}
							else
							{
								%pos = strPos(%value, "://");
								%url = getSubStr(%value, %pos + 3, strLen(%value));
								%pos = strPos(%url, "/");
								%this.server = getSubStr(%url, 0, %pos) @ ":80";
								%this.path = getSubStr(%url, %pos, strLen(%url));
							}

							%this.redirected = true;
							%this.retrySchedule = %this.scheduleNoQuota($TCPClient::redirectWait, "connect", %this.server);

							return;
						}
				}
			}
		}
		else
		{
			%type = %this.headerField["Content-Type"];
			%savePathLen = strLen(%this.savePath);
			if(strLen(%type) && strPos(%type, "text/") == 0 && !%savePathLen)
			{
				%this.receiveText = true;
			}
			else
			{
				%this.setBinarySize(%this.headerField["Content-Length"]);
			}
		}
	}
	if(isFunction(%this.className, "onLine"))
		eval(%this.className @ "::onLine(%this, %line);");
}

//Called when a binary chunk is received. To use with your mod, replace "TCPClient" with your class name.
//Only called when in binary mode.
//@param	int chunk	The number of bytes received.
//@see	TCPClient::setProgressBar
function TCPClient::onBinChunk(%this, %chunk)
{
	//Ensure that this is a good connection.
	if(%this.httpStatus < 200 || %this.httpStatus >= 300)
		return;

	if(isFunction(%this.className, "onBinChunk"))
		eval(%this.className @ "::onBinChunk(%this, %chunk);");

	%contentLength = %this.headerField["Content-Length"];
	%contentLengthSet = (strLen(%contentLength) > 0);
	if($TCPClient::Debug)
		echo(" > " @ %chunk @ "/" @ %contentLength SPC "bytes");

	if(%contentLengthSet)
		%this.setProgressBar(%chunk / %contentLength);
	if(%chunk >= %contentLength && %contentLengthSet)
	{
		%save = true;
		%done = true;
	}
	else
	{
		//this is a chunked/streaming transfer
		if(!%contentLengthSet)
		{
			%save = true;
			%done = false;
		}
		cancel(%this.timeoutSchedule);
		%this.timeoutSchedule = %this.schedule($TCPClient::timeout, "onDone", $TCPClient::Error::connectionTimedOut);
	}

	if(%save)
	{
		if(strLen(%this.savePath) && isWriteableFileName(%this.savePath))
		{
			%this.saveBufferToFile(%this.savePath);
			if(%done)
			{
				%this.onDone($TCPClient::Error::none);
			}
		}
		else
		{
			%this.onDone($TCPClient::Error::invalidDownloadLocation);
		}
	}
}

//Used when downloading text files. To use with your mod, replace "TCPClient" with your class name.
//Only called when downloading text files and the "savePath" variable is blank.
//@param	string text The text received.
function TCPClient::handleText(%this, %text)
{
	if(isFunction(%this.className, "handleText"))
		eval(%this.className @ "::handleText(%this, %text);");
}

//Used to update a progress bar when downloading a binary file. To use with your mod, replace "TCPClient" with your class name.
//Only called when in binary mode.
//@param	float completed The amount completed, represented as a floating point value from 0.0 to 1.0.
function TCPClient::setProgressBar(%this, %completed)
{
	if(isFunction(%this.className, "setProgressBar"))
		eval(%this.className @ "::setProgressBar(%this, %completed);");
}

//----------------------------------------------------------------------
// Utility Functions
//----------------------------------------------------------------------

//Takes a full URL, like http://example.com/somepage.php?argument=blah&whatever,
// and breaks it down into its components.
//@param	string url
//@return	string	protocol TAB server TAB port TAB path TAB query
function urlGetComponents(%url)
{
	//strip the protocol
	%pos = strPos(%url, "://");
	if(%pos == -1)
	{
		%protocol = "http";
	}
	else
	{
		%protocol = getSubStr(%url, 0, %pos);
		%url = getSubStr(%url, %pos + 3, strLen(%url));
	}

	//separate the server and the path
	%pos = strPos(%url, "/");
	if(%pos == -1)
	{
		%pos = strPos(%url, "?");
		if(%pos == -1)
		{
			%server = %url;
			%path = "/";
		}
		else
		{
			//handle URLs in the form of example.com?argument=blah
			%server = getSubStr(%url, 0, %pos);
			%path = "/" @ getSubStr(%url, %pos, strLen(%url));
		}
	}
	else
	{
		%server = getSubStr(%url, 0, %pos);
		%path = getSubStr(%url, %pos, strLen(%url));
	}

	//separate the server and the port
	%pos = strPos(%server, ":");
	if(%pos == -1)
	{
		switch$(%protocol)
		{
			case "ftp": %port = 21;
			case "telnet": %port = 23;
			case "smtp": %port = 25;
			case "nicname": %port = 43;
			case "http": %port = 80;
			case "https": %port = 443;
			default: %port = 80; //default to 80
		}
	}
	else
	{
		%port = getSubStr(%server, %pos + 1, strLen(%server));
		%server = getSubStr(%server, 0, %pos);
	}

	//separate the path and the query
	%pos = strPos(%path, "?");
	if(%pos != -1)
	{
		%query = getSubStr(%path, %pos + 1, strLen(%path));
		%path = getSubStr(%path, 0, %pos);
	}

	//append "/" to path if needed
	if(getSubStr(%path, strLen(%path) - 1, 1) !$= "/" && strPos(%path, ".") == -1)
		%path = %path @ "/";

	return %protocol TAB %server TAB %port TAB %path TAB %query;
}

//----------------------------------------------------------------------
// Deprecated Functions - DO NOT USE
//----------------------------------------------------------------------

//Creates a TCP connection and sends a POST request to the specified server.
// Additional debugging is available by setting $TCPClient::Debug to true.
//@param	string server	The URL and (optionally) port of the server to connect to.
//@param	string path	The location of the file on the server.
//@param	string query	The parameters to be sent with the POST request. Be sure to use urlEnc() on the parameters. Must be formatted like this: myArg=value&testarg=stuff&whatever=1
//@param	string savePath	The local filepath to save binary (non-text) files to.
//@param	string class	The name of a class which can be used to change/extend functionality.
//@return	TCPObject	The TCP object that is performing the connection.
//@DEPRECATED
function TCPClientPOST(%server, %path, %query, %savePath, %class)
{
	warn("WARN: TCPClientPOST is deprecated! Use TCPClient or connectToURL instead.");
	if((%pos = strPos(%server, ":")) >= 0)
	{
		%port = getSubStr(%server, %pos + 1, strLen(%server));
		%server = getSubStr(%server, 0, %pos);
	}
	else
		%port = 80;
	return TCPClient("POST", %server, %port, %path, %query, %savePath, %class);
}

//Creates a TCP connection and sends a GET request to the specified server.
// Additional debugging is available by setting $TCPClient::Debug to true.
//@param	string server	The URL and (optionally) port of the server to connect to.
//@param	string path	The location of the file on the server.
//@param	string query	The parameters to be sent with the GET request. Be sure to use urlEnc() on the parameters. Must be formatted like this: myArg=value&testarg=stuff&whatever=1
//@param	string savePath	The local filepath to save binary (non-text) files to.
//@param	string class	The name of a class which can be used to change/extend functionality.
//@return	TCPObject	The TCP object that is performing the connection.
//@DEPRECATED
function TCPClientGET(%server, %path, %query, %savePath, %class)
{
	warn("WARN: TCPClientGET is deprecated! Use TCPClient or connectToURL instead.");
	if((%pos = strPos(%server, ":")) >= 0)
	{
		%port = getSubStr(%server, %pos + 1, strLen(%server));
		%server = getSubStr(%server, 0, %pos);
	}
	else
		%port = 80;
	return TCPClient("GET", %server, %port, %path, %query, %savePath, %class);
}
