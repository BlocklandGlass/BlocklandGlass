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

	if(%this.queue.getCount() == 1) { //we just added the first
		GlassModManagerGui::setProgress(0, "Connecting...");
	}

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
	if(%this.busy || %this.getCount() == 0)
		return;

	%this.busy = true;

	%fileData = %this.getObject(0);

	%url = "http://" @ Glass.netAddress @ "/api/2/download.php?type=addon_download&id=" @ %fileData.id @ "&branch=" @ %fileData.download_branch;

	%method = "GET";
	%downloadPath = "Add-Ons/" @ %fileData.filename;
	%className = "GlassDownloadTCP";

	%tcp = connectToURL(%url, %method, %downloadPath, %className);
	//%tcp = GlassDownloadManager::fakeDownload(getRandom(2000, 5000));
	%tcp.fileData = %fileData;
//	%tcp.path = "/addons/6_1";

	return %tcp;
}

function GlassDownloadManagerQueue::fetchFinished(%this) {
	%this.remove(%this.getObject(0));
	%this.busy = false;
	%this.fetchNext();
}

function GlassDownloadTCP::onLine(%this, %line) { // a little shortcut because AWS is touchy w/ arguments
	if(%this.redirected && !%this.redirectCleaned) {
		%this.query = "";

		%this.redirectCleaned = true;
	}
}

function GlassDownloadTCP::onDone(%this, %error) {
	if(%error) {
		error("An error was encountered downloading file (" @ %error @ ") - need to handle this better");
		if(!$Server::Dedicated) {
			%name = "GlassModManagerGui_DlButton_" @ %this.filedata.id @ "_" @ (%this.fileData.download_branch);
			%name.setValue("<font:verdana bold:16><just:center>Download<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));
		}

		GlassModManagerGui::setProgress(1, "Error Downloading Add-On");
		GlassModManager::setAddonStatus(%this.filedata.id, "");
	} else {
		if(!$Server::Dedicated) {
			%name = "GlassModManagerGui_DlButton_" @ %this.filedata.id @ "_" @ (%this.fileData.download_branch);
			%name.setValue("<font:verdana bold:16><just:center>Downloaded<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));
		}

		GlassModManager::setAddonStatus(%this.filedata.id, "installed");

		echo("Successfully downloaded " @ %this.fileData.filename);
	}

	GlassDownloadManagerQueue.fetchFinished();
	if(!$Server::Dedicated)
		GlassModManagerGui.sch = GlassModManagerGui.schedule(2000, setProgress);
}

function GlassDownloadTCP::setProgressBar(%this, %float) {
	if(!$Server::Dedicated) {
		GlassModManager::setAddonStatus(%this.filedata.id, "downloading");
		cancel(GlassModManagerGui.sch);
		if(%this.fileData.rtbImport) {
			GlassRTBSupport::updateProgressBar(%this.fileData.downloadHandler, %float);
		}

		%name = "GlassModManagerGui_DlButton_" @ %this.filedata.id @ "_" @ (%this.fileData.download_branch);
		%name.setValue("<font:verdana bold:16><just:center>Downloading..<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));

		if(%float < 1)
			GlassModManagerGui::setProgress(%float, "Downloading " @ %this.fileData.filename @ " (" @ GlassDownloadManagerQueue.getCount() @ " remaining)");
		else
			GlassModManagerGui::setProgress(%float, "All Downloads Finished");
	}
}
