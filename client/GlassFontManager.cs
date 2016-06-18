if(isObject(GlassFontManager)) {
  GlassFontManager.delete();
}

function GlassFontManager::init() {
  new ScriptObject(GlassFontManager) {
    url = "http://test.blocklandglass.com/api/fonts/";
  };

  GlassFontManager.fetchRepository();

  //if(!isfile("config/client/BLG/font.dat")) {
  //  GlassFontManager.downloadAll();
  //}
  //GlassFontManager.loadData();
}

function GlassFontManager::downloadAll(%this, %act) {
  %fonts = %this.fontsAvailable;
  for(%i = 0; %i < %fonts.length; %i++) {
    %font = %fonts.item[%i];
    %this.downloadFont(%font, %act);
  }
}

function GlassFontManager::downloadMissing(%this) {
  %fo = new FileObject();
  %fo.openForRead("config/client/BLG/fonts.dat");
  while(!%fo.isEOF()) {
    %line = %fo.readLine();
    %this.font[%line] = true;
  }
  %fo.close();
  %fo.delete();

  %fonts = %this.fontsAvailable;
  for(%i = 0; %i < %fonts.length; %i++) {
    %font = %fonts.item[%i];
    if(!%this.font[%font] || !isfile("base/client/ui/cache/" @ %font)) {
      %dl++;
      %this.downloadFont(%font);
    }
  }

  Glass.didDownloadFonts = true;

  if(%dl)
    echo("Downloading missing fonts (" @ %dl @ " of " @ %fonts.length @ ")");
}

function GlassFontManager::fetchRepository(%this) {
  %url = %this.url;
	%method = "GET";
	%downloadPath = "";
	%className = "GlassFontRepo";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
}

function GlassFontManager::downloadFont(%this, %font, %actual) {
  if(%actual) {
    %url = %this.url @ %font;
  	%method = "GET";
  	%downloadPath = "base/client/ui/cache/" @ %font;
  	%className = "GlassFontDownload";

  	%tcp = connectToURL(%url, %method, %downloadPath, %className);
  	%tcp.font = %font;
  } else {
    %this.font[%this.fonts+0] = %font;
    %this.fonts++;
  }
}

function GlassFontManager::prompt(%this) {
  %ctx = GlassDownloadInterface::openContext("Fonts", "<font:arial:16>Blockland Glass needs download some visual resources.<br><br> Press Download to continue");
  %ctx.registerCallback("GlassFontManager::downloadGui");
  %ctx.inhibitClose(true);
  for(%i = 0; %i < GlassFontManager.fonts; %i++) {
    %font = GlassFontManager.font[%i];
    GlassFontManager.dlHandler[%font] = %ctx.addDownload("<font:arial:16>" @ %font);
  }
}

function GlassFontManager::downloadGui(%code) {
  if(%code == 1) {
    for(%i = 0; %i < GlassFontManager.fonts; %i++) {
      %font = GlassFontManager.font[%i];
      GlassFontManager.downloadFont(%font, true);
    }
  } else if(%code == 2) {
    if(GlassDownloadInterface.getCount() == 1) {
      messageBoxOk("Please Restart", "Please restart Blockland for these changes to take effect. Pressing OK will close Blockland.", "quit();");
    }
  } else if(%code == -1) {
    messageBoxOk("Uh-oh", "These fonts are needed to run Blockland Glass!");
  }
}

function GlassFontDownload::setProgressBar(%this, %float) {
  if(isObject(GlassFontManager.dlHandler[%this.font]))
    GlassFontManager.dlHandler[%this.font].setProgress(%float);
}

function GlassFontDownload::onDone(%this, %error) {
  if(!%error) {
    %fo = new FileObject();
    %fo.openForAppend("config/client/BLG/fonts.dat");
    %fo.writeLine(%this.font);
    %fo.close();
    %fo.delete();
    GlassFontManager.font[%this.font] = true;
  }
}

function GlassFontRepo::handleText(%this, %line) {
  %this.buffer = %this.buffer NL %line;
}

function GlassFontRepo::onDone(%this, %error) {
  //echo(%this.buffer);
  if(!%error) {
    %fonts = parseJSON(%this.buffer);
    for(%i = 0; %i < %fonts.length; %i++) {
      %font = %fonts.item[%i];
    }
    GlassFontManager.fontsAvailable = %fonts;

    if(!isfile("config/client/BLG/fonts.dat")) {
      echo("Downloading all fonts (" @ %fonts.length @ " total)");
      GlassFontManager.downloadAll();
    } else {
      GlassFontManager.downloadMissing();
    }
  } else {
    error("Error downloading font: " @ %font);
  }
}

package GlassFontManager {
  function MM_AuthBar::blinkSuccess(%this) {
    if(GlassFontManager.fonts)
      GlassFontManager.prompt();
    parent::blinkSuccess(%this);
  }
};
activatePackage(GlassFontManager);
