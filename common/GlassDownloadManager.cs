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

function GlassDownloadManager::fakeDownload(%duration) {
	%obj = new ScriptObject(GlassDownloadTCP) {

	};
	%steps = mceil(%duration/50);
	for(%i = 0; %i <= %steps; %i++) {
		%obj.schedule(%i*50, setProgressBar, (%i*50)/%duration);
	}
	%obj.schedule(%duration+50, onDone, false);

	return %obj;
}

function GlassDownloadManagerQueue::fetchNext(%this) {
	echo("Trying next");
	echo(%this.getCount());
	if(%this.busy || %this.getCount() == 0)
		return;

	echo("Downloading");

	%this.busy = true;

	%fileData = %this.getObject(0);

	%url = "http://" @ Glass.netAddress @ "/api/support_updater/download.php?id=" @ %fileData.id @ "&branch=" @ %fileData.download_branch @ "&ingame=1";
	%method = "GET";
	%downloadPath = "Add-Ons/" @ %fileData.filename;
	%className = "GlassDownloadTCP";

	//%tcp = connectToURL(%url, %method, %downloadPath, %className);
	%tcp = GlassDownloadManager::fakeDownload(getRandom(2000, 5000));
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
		%this.button.info.setValue("<font:quicksand-bold:16><just:center>Downloaded<br><font:quicksand:14>" @ strcap(%this.branchName));
		echo("Successfully downloaded " @ %this.fileData.filename);
	}

	GlassDownloadManagerQueue.fetchFinished();
	if(!$Server::Dedicated)
		GlassModManagerGui.sch = GlassModManagerGui.schedule(2000, setProgress);
}

function GlassDownloadTCP::setProgressBar(%this, %float) {
	if(!$Server::Dedicated) {
		cancel(GlassModManagerGui.sch);
		if(%this.fileData.rtbImport) {
			GlassRTBSupport::updateProgressBar(%this.fileData.downloadHandler, %float);
		}

		%this.button.info.setValue("<font:quicksand-bold:16><just:center>Downloading..<br><font:quicksand:14>" @ strcap(%this.branchName));

		if(%float < 1)
			GlassModManagerGui::setProgress(%float, "Downloading " @ %this.fileData.filename @ " (" @ GlassDownloadManagerQueue.getCount() @ " remaining)");
		else
			GlassModManagerGui::setProgress(%float, "All Downloads Finished");
	}
}
