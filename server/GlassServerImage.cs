package GlassLoadingScreen {
  function gameConnection::startLoad(%this) {
    if($LoadingScreen::Enabled)
      commandToClient(%this, 'Glass_setLoadingBackground', $LoadingScreen::Url, $LoadingScreen::FileType, $LoadingScreen::CRC, $LoadingScreen::Playerlist);

    parent::startLoad(%this);
  }

  function destroyServer() {
    $LoadingScreen::Enabled = 0;
    $LoadingScreen::Url = "";
    $LoadingScreen::FileType = "";
  	$LoadingScreen::CRC = "";
    $LoadingScreen::Playerlist = true;
    parent::destroyServer();
  }
};
activatePackage(GlassLoadingScreen);

function registerLoadingScreen(%url, %fileType, %crc, %hidePlayerList) {
	if(trim(%url) $= "" || trim(%fileType) $= "") {
		warn("Error usage: registerLoadingScreen( url, filetype[, crc[, hidePlayerList]] )");
		return;
	}
	if(%fileType !$= "jpg" && %fileType !$= "png" && %fileType !$= "jpeg") {
		warn("Error registerLoadingScreen: file type can only be jpg, png, or jpeg.");
		return;
	}
	$LoadingScreen::Enabled = 1;
	$LoadingScreen::Url = %url;
	$LoadingScreen::FileType = %fileType;
	$LoadingScreen::CRC = %crc;
  $LoadingScreen::Playerlist = !%hidePlayerList;
}
