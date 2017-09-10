// Blockland Glass - Mod Downloader

function GlassDownloadManager::init() {
	new ScriptObject(GlassDownloadManager);
	new ScriptGroup(GlassDownloadManagerQueue);
	GlassDownloadManager.queue = GlassDownloadManagerQueue;
}

function GlassDownloadManager::newDownload(%id, %branchId) {
	%obj = new ScriptObject() {
		class = "GlassDownload";

		addonId = %id;
		branchId = %branchId;
	};

	return %obj;
}

function GlassDownloadManager::newRTBDownload(%id) {
	%obj = new ScriptObject() {
		class = "GlassDownload";
		rtb = true;

		addonId = %id;
	};

	return %obj;
}

function GlassDownload::addHandle(%this, %event, %call) {
	//handles:
	// failed
	// progress
	// done
	%this.handler[%event] = %call;
}

function GlassDownload::startDownload(%this) {
	%this.connecting = true;

	%this._fetch();
}

function GlassDownload::_fetch(%this) {
	if(!%this.rtb) {
		%url = "http://" @ Glass.address @ "/api/3/download.php?type=addon_download&id=" @ %this.addonId @ "&branch=" @ %this.branchId;
	} else {
		%url = "http://" @ Glass.address @ "/api/3/download.php?type=rtb&rtbId=" @ %this.addonId;
	}

	%tcp = connectToURL(%url, "GET", %this.location, "GlassDownloadTCP");
	%tcp.downloadObject = %this;

	%this.tcp = %tcp;
}

function GlassDownloadTCP::onConnected(%this) {
	%this.connecting = false;
	%this.connected = true;
}

function GlassDownloadTCP::onLine(%this, %line) {
	%head = getWord(%line, 0);
	if(%head $= "Content-Disposition:") {
		%filename = getSubStr(%line, strPos(%line, "filename="), strlen(%line));
		%filename = getSubStr(%filename, 9, strlen(%filename));
		%filename = getSubStr(%filename, 1, strlen(%filename)-2);
		%this.downloadObject.filename = %filename;
		%this.savePath = "Add-Ons/" @ %filename;

		if(!isWriteableFilename(%this.savePath)) {
			%this.disconnect();

			if(isFunction(%this.handler["unwritable"]))
				call(%this.handler["unwritable"], %this, %float);
		}
	}
}

function GlassDownloadTCP::setProgressBar(%tcp, %float) {
	%this = %tcp.downloadObject;

	if(isFunction(%this.handler["progress"]))
		call(%this.handler["progress"], %this, %float, %tcp);
}

function GlassDownloadTCP::onDone(%tcp, %error) {
	%this = %tcp.downloadObject;

	if(%error) {
		if(isFunction(%this.handler["failed"]))
			call(%this.handler["failed"], %this, %error, %tcp);
	} else {
		if(isFunction(%this.handler["done"]))
			call(%this.handler["done"], %this, %error, %tcp);
	}

	%this.schedule(1, delete);
}
