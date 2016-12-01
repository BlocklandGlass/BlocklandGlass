package GlassLoadingScreen {
  function gameConnection::startLoad(%this) {
	if($LoadingScreen::Enabled)
		commandToClient(%this, 'Glass_setLoadingBackground', $LoadingScreen::Url, $LoadingScreen::FileType);
	
	parent::startLoad(%this);
  }
  
   function destroyServer() {
	$LoadingScreen::Enabled = 0;
	$LoadingScreen::Url = "";
	$LoadingScreen::FileType = "";
    parent::destroyServer();
  }
};
activatePackage(GlassLoadingScreen);

function registerLoadingScreen(%url, %fileType) {
	if(%url $= "" || %fileType $= "") {
		warn("Error usage: registerLoadingScreen(url, filetype)");
		return;
	}
	if(%fileType !$= "jpg" && %fileType !$= "png" && %fileType !$= "jpeg") {
		warn("Error registerLoadingScreen: file type can only be jpg, png, or jpeg.");
		return;
	}
	$LoadingScreen::Enabled = 1;
	$LoadingScreen::Url = %url;
	$LoadingScreen::FileType = %fileType;
}