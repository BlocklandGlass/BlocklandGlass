// Blockland Glass - Mod Downloader

function GlassDownloadManager::init() {
	new ScriptObject(GlassDownloadManager);
	new ScriptGroup(GlassDownloadManagerQueue);
	GlassDownloadManager.queue = GlassDownloadManagerQueue;
}


function GlassDownloadManager::fetchAddon(%this, %addonHandler) {
	echo("Fetching: " @ %addonHandler.name);
	%this.queue.add(%addonHandler);
	%this.queue.fetchNext();
}

function GlassDownloadManagerQueue::fetchNext(%this) {
	echo("Trying next");
	if(%this.busy || %this.getCount() == 0)
		return;
	
	echo("Downloading");

	%this.busy = true;

	%fileData = %this.getObject(0);

	%url = "http://" @ BLG.netAddress @ "/api/support_updater/download.php?id=" @ %fileData.id @ "&branch=" @ %fileData.branch;
	%method = "GET";
	%downloadPath = "Add-Ons/" @ %fileData.filename;
	%className = "GlassDownloadTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	%tcp.fileData = %fileData;
}

function GlassDownloadManagerQueue::fetchFinished(%this) {
	%this.remove(%this.getObject(0));
	%this.busy = false;
	%this.fetchNext();
}

function GlassDownloadTCP::onDone(%this, %error) {
	if(%error) {
		error("An error was encountered downloading file - need to handle this better");
	} else {
		echo("Successfully downloaded " @ %this.fileData.filename);
	}

	GlassDownloadManagerQueue.fetchFinished();
}

function GlassDownloadTCP::setProgressBar(%this, %float) {
	echo("Progress: " @ %float);	
}