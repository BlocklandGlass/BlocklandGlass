// Blockland Glass - Mod Downloader

function GlassDownloadManager::init() {
	new ScriptObject(GlassDownloadManager);
	new ScriptGroup(GlassDownloadManagerQueue);
	GlassDownloadManager.queue = GlassDownloadManagerQueue;
}


function GlassDownloadManager::fetchAddon(%this, %addonHandler, %branch) {
	%addonHandler.download_branch = %branch;
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

	%url = "http://" @ Glass.netAddress @ "/api/support_updater/download.php?id=" @ %fileData.id @ "&branch=" @ %fileData.download_branch @ "&ingame=1";
	%method = "GET";
	%downloadPath = "Add-Ons/" @ %fileData.filename;
	%className = "GlassDownloadTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	%tcp.fileData = %fileData;

	return %tcp;
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
	if(!$Server::Dedicated)
		GlassModManagerGui.schedule(2000, setProgress);
}

function GlassDownloadTCP::setProgressBar(%this, %float) {
	if(!$Server::Dedicated) {
		cancel(GlassModManagerGui.sch);
		if(%this.fileData.rtbImport) {
			GlassRTBSupport::updateProgressBar(%this.fileData.downloadHandler, %float);
		}

		if(%float < 1)
			GlassModManagerGui::setProgress(%float, "Downloading " @ %this.fileData.filename @ " (" @ GlassDownloadManagerQueue.getCount() @ " remaining)");
		else
			GlassModManagerGui::setProgress(%float, "All Downloads Finished");
	}
}
