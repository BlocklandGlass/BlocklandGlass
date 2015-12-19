function GlassModManagerGui::loadErrorPage(%errorcode, %buffer) {
  %container = new GuiSwatchCtrl() {
    horizSizing = "right";
    vertSizing = "bottom";
    color = "0 0 0 0";
    position = "0 0";
    extent = "505 498";
  };

  %container.text = new GuiMLTextCtrl() {
    horizSizing = "center";
    vertSizing = "center";
    text = "";
    position = "102 30";
    extent = "300 498";
    autoResize = true;
  };

  %text = "<just:center><font:quicksand-bold:20><color:ff0000>Error!<br><color:000000>";
  if($GlassError[%errorcode] !$= "") {
    %text = %text @ getField($GlassError[%errorcode], 0);
    %text = %text @ "<br><br>";
    %text = %text @ "<font:quicksand-bold:16>" @ getField($GlassError[%errorcode], 1);
  } else {
    %text = %text @ "Code: " @ %errorcode;
  }

  %text = %text @ "<br><br>";

  if($Glass::Debug) {
    %text = %text @ "<just:left><font:Lucida Console:12>" @ %buffer;
  }

  %container.text.setValue(%text);

  %container.add(%container.text);

  GlassModManagerGui_MainDisplay.extent = %container.extent;

  GlassModManager::setLoading(false);

  GlassModManagerGui_MainDisplay.deleteAll();
  GlassModManagerGui_MainDisplay.add(%container);
}


$GlassError["development"] = "Development\tThis page is still in development!";

$GlassError["status_"] = "No Status\tThe API failed to return a status message.";
$GlassError["status_error"] = "API Error\tThe API encountered an error completing your request.";
$GlassError["status_development"] = "Development\tThis page is still in development!";
