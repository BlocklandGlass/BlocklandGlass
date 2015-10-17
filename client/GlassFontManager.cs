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

function GlassFontManager::downloadAll(%this) {
  %fonts = %this.fontsAvailable;
  for(%i = 0; %i < %fonts.length; %i++) {
    %font = %fonts.item[%i];
    %this.downloadFont(%font);
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

function GlassFontManager::downloadFont(%this, %font) {
  %url = %this.url @ %font;
	%method = "GET";
	%downloadPath = "base/client/ui/cache/" @ %font;
	%className = "GlassFontDownload";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	%tcp.font = %font;
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
  }
}
