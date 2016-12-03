function GMM_ErrorPage::init() {
  new ScriptObject(GMM_ErrorPage);
}

function GMM_ErrorPage::open(%this, %errorcode, %buffer) {
  GlassModManagerGui.setLoading(false);
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "255 255 255 255";
    position = "0 0";
    extent = "635 498";
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "";
    position = "100 30";
    extent = "300 10";
    minExtent = "10 10";
    autoResize = true;
  };

  %text = "<just:center><font:verdana bold:20><color:ff0000>Error!<br><color:000000>";
  if($Glass::MM::Error[%errorcode] !$= "") {
    %text = %text @ getField($Glass::MM::Error[%errorcode], 0);
    %text = %text @ "<br><br>";
    %text = %text @ "<font:Verdana Bold:15>" @ getField($Glass::MM::Error[%errorcode], 1);
  } else {
    %text = %text @ "Code: " @ %errorcode;
  }

  %text = %text @ "<br><br>";

  if(Glass.dev) {
    %text = %text @ "<just:left><font:Lucida Console:12>" @ %buffer;
  } else {
    //record the event
    %fo = new FileObject();
    %fo.openForWrite("config/client/BLG/error_log/" @ getrealtime() @ ".log");
    %fo.writeLine("Error Code: " @ %errorcode);
    %fo.writeLine("");
    %fo.writeLine(%buffer);
    %fo.close();
    %fo.delete();
  }

  %container.text.setValue(%text);
  %container.add(%container.text);

  %container.text.schedule(1, forceReflow);
  %container.text.schedule(1, setMarginResizeParent, 10, 10);
  %container.schedule(1, forceCenter);

  return %container;
}


$Glass::MM::Error["development"] = "Development\tThis page is still in development!";

$Glass::MM::Error["status_"] = "No Status\tThe API failed to return a status message.";
$Glass::MM::Error["status_error"] = "API Error\tThe API encountered an error completing your request.";
$Glass::MM::Error["status_development"] = "Development\tThis page is still in development!";

$Glass::MM::Error["jettison"] = "Internal Error\tThere was a jettison parse error. If this continues, be sure to report it.";

$Glass::MM::Error["tcpclient_" @ $TCPClient::Error::connectionFailed] = "Failed to Connect\tThe Mod Manager couldn't connect to the Blockland Glass website!";
$Glass::MM::Error["tcpclient_" @ $TCPClient::Error::dnsFailed] = "Failed to Connect\tDNS Failed.";
$Glass::MM::Error["tcpclient_" @ $TCPClient::Error::invalidResponse] = "Invalid Response\tThe server encountered an error!";
$Glass::MM::Error["tcpclient_" @ $TCPClient::Error::connectionTimedOut] = "Timeout\tThe connection timed out.";

//$TCPClient::Error::none = 0;
//$TCPClient::Error::connectionFailed = 1;
//$TCPClient::Error::dnsFailed = 2;
//$TCPClient::Error::invalidResponse = 3;
//$TCPClient::Error::invalidRedirect = 4;
//$TCPClient::Error::invalidDownloadLocation = 5;
//$TCPClient::Error::invalidUrlFormat = 6;
//$TCPClient::Error::connectionTimedOut = 7;
