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
	%url = "http://" @ Glass.netAddress @ "/api/2/download.php?type=addon_download&id=" @ %this.addonId @ "&branch=" @ %this.branchId;

	%tcp = connectToURL(%url, "GET", "", "GlassDownloadTCP");
	%tcp.downloadObject = %this;

	%this.tcp = %tcp;
}

function GlassDownloadTCP::onConnected(%this) {
	%this.connecting = false;
	%this.connected = true;
}

function GlassDownloadTCP::onLine(%this, %line) {
	//echo(%line);
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
		call(%this.handler["progress"], %this, %float);
}

function GlassDownloadTCP::onDone(%tcp, %error) {
	%this = %tcp.downloadObject;

	if(%error) {
		if(isFunction(%this.handler["failed"]))
			call(%this.handler["failed"], %this, %error);
	} else {
		if(isFunction(%this.handler["done"]))
			call(%this.handler["done"], %this, %error);
	}

	%this.schedule(1, delete);
}

return;

function GlassDownloadTCP::onDone(%this, %error) {
	if(%error) {
		error("An error was encountered downloading file (" @ %error @ ") - need to handle this better");
		if(!$Server::Dedicated) {
			%name = "GlassModManagerGui_DlButton_" @ %this.filedata.id @ "_" @ (%this.fileData.download_branch);
			%name.setValue("<font:Verdana Bold:15><just:center>Download<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));
		}

		GlassModManagerGui::setProgress(1, "Error Downloading Add-On");
		GlassModManager::setAddonStatus(%this.filedata.id, "");
	} else {
		if(isObject(%this.fileData.rtbImportProgress)) {
			%this.fileData.rtbImportProgress.setValue(1);
			%this.fileData.rtbImportProgress.getObject(0).setValue("<shadow:1:1><just:center><font:verdana:12><color:dddddd>Downloaded");
			%filename = %this.fileData.rtbImportProgress.getGroup().import.filename;

			%zip = %this.fileData.filename;
			%name = getsubstr(%zip, 0, strlen(%zip)-4);

			if(%name !$= %filename) {
				fileDelete("Add-Ons/" @ %filename @ ".zip");
			}
			%cl = "Add-Ons/" @ %name @ "/client.cs";

			discoverFile("Add-Ons/" @ %name @ ".zip");
			discoverFile("Add-Ons/" @ %name @ "/*");

			if(isFile(%cl))
				exec(%cl);
			else
				echo("No client.cs, nothing to execute");

			GlassClientManager.downloadFinished(%this.fileData.id);
		}

		if(!$Server::Dedicated) {
			%name = "GlassModManagerGui_DlButton_" @ %this.filedata.id @ "_" @ (%this.fileData.download_branch);
			if(isObject(%name))
				%name.setValue("<font:Verdana Bold:15><just:center>Downloaded<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));
		}

		GlassModManager::setAddonStatus(%this.filedata.id, "installed");

		echo("Successfully downloaded " @ %this.fileData.filename);
	}

	GlassDownloadManagerQueue.fetchFinished();
}

function GlassDownloadTCP::setProgressBar(%this, %float) {
	if(isObject(%this.fileData.rtbImportProgress)) {
		%this.fileData.rtbImportProgress.setValue(%float);
		%this.fileData.rtbImportProgress.getObject(0).setValue("<shadow:1:1><just:center><font:verdana:12><color:999999>Downloading");
	}

	if(!$Server::Dedicated) {
		GlassModManager::setAddonStatus(%this.filedata.id, "downloading");
		cancel(GlassModManagerGui.sch);
		if(%this.fileData.rtbImport) {
			GlassRTBSupport::updateProgressBar(%this.fileData.downloadHandler, %float);
		}

		%name = "GlassModManagerGui_DlButton_" @ %this.filedata.id @ "_" @ (%this.fileData.download_branch);
		if(isObject(%name))
			%name.setValue("<font:Verdana Bold:15><just:center>Downloading..<br><font:verdana:14>" @ strcap(%name.getGroup().mouse.branch));

		if(%float < 1)
			GlassModManagerGui::setProgress(%float, "Downloading " @ %this.fileData.filename @ " (" @ GlassDownloadManagerQueue.getCount() @ " remaining)");
		else
			GlassModManagerGui::setProgress(%float, "All Downloads Finished");
	}
}
