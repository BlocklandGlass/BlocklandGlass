function updaterDlg::onWake(%this)
{
	if(MainMenuButtonsGUI.isAwake())
	{
		%this.restoreMenuButtons = true;
		canvas.popDialog(MainMenuButtonsGUI);
	}
}

function updaterDlg::onSleep(%this)
{
	if(%this.restoreMenuButtons)
		canvas.pushDialog(MainMenuButtonsGUI);
}

function updaterDlg::clickBack(%this)
{
	canvas.popDialog(%this);
}

function updaterDlg::clickUpdate(%this)
{
	updaterDlgBackButton.setText(" << HIDE");
	updaterDlgUpdateButton.setActive(0);

	updater.doUpdates();
}

function updaterDlg::clickQueueItem(%this, %queueObj)
{
	%count = updaterDlgAddOnSwatch.getCount();
	for(%i = 0; %i < %count; %i ++)
		updaterDlgAddOnSwatch.getObject(%i).setColor("0 0 0 110");
	%queueObj.guiSwatch.setColor("255 255 255 110");

	%this.viewItem = %queueObj;

	%info = "<color:ffffff><h1>" @ %queueObj.name @ "</h1>"
		NL "<ul><li><b>Version:</b>" SPC %queueObj.updateVersion @ "</li>"
		NL "<b>Restart Required?</b>" SPC (%queueObj.updateRestartRequired ? "Yes" : "No") @ "</li>"
		NL "<b>Repository:</b>" SPC %queueObj.repository.url @ "</li></ul>";
	if(strLen(%queueObj.updateDescription))
		%info = %info NL "\n<h2>Description:</h2>" @ %queueObj.updateDescription;
	%info = parseCustomTML(%info, %text, "default");
	updaterDlgInfoText.setText(%info);

	%this.viewChangeLog(%queueObj);
}

function updaterDlg::clickQueueItemOpt(%this, %queueObj)
{
	if(isObject(%this.queueOptMenu))
	{
		%qo = %this.queueOptMenu.queueObj;
		%this.queueOptMenu.delete();
		if(%qo == %queueObj)
			return;
	}

	%swatch = %queueObj.guiSwatch;
	%ext = %swatch.getExtent();
	%posX = getWord(%ext, 0) - 165;
	%posY = 8;

	%this.queueOptMenu = new GuiSwatchCtrl()
	{
		profile = "GuiDefaultProfile";
		horizSizing = "left";
		vertSizing = "bottom";
		position = %posX SPC %posY;
		extent = "120 60";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		color = "0 0 0 164";
		queueObj = %queueObj;
	};
	%swatch.add(%this.queueOptMenu);
	%this.queueOptMenu.skipBtn = new GuiBitmapButtonCtrl()
	{
		profile = "updaterImpactPopupButtonProfile";
		horizSizing = "width";
		vertSizing = "bottom";
		position = "5 5";
		extent = "115 25";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		command = "updaterDlg.clickQueueItemSkip(" @ %queueObj @ ");";
		text = "Update later";
		groupNum = "-1";
		buttonType = "PushButton";
		bitmap = "base/client/ui/btnBlank";
		lockAspectRatio = "0";
		alignLeft = "0";
		alignTop = "0";
		overflowImage = "0";
		mKeepCached = "0";
		mColor = "255 255 255 255";
	};
	%this.queueOptMenu.add(%this.queueOptMenu.skipBtn);
	if(%queueObj.name !$= "Support_Updater")
	{
		%this.queueOptMenu.ignoreBtn = new GuiBitmapButtonCtrl()
		{
			profile = "updaterImpactPopupButtonProfile";
			horizSizing = "width";
			vertSizing = "bottom";
			position = "5 30";
			extent = "115 25";
			minExtent = "8 2";
			enabled = "1";
			visible = "1";
			clipToParent = "1";
			command = "updaterDlg.clickQueueItemIgnore(" @ %queueObj @ ");";
			text = "Ignore updates";
			groupNum = "-1";
			buttonType = "PushButton";
			bitmap = "base/client/ui/btnBlank";
			lockAspectRatio = "0";
			alignLeft = "0";
			alignTop = "0";
			overflowImage = "0";
			mKeepCached = "0";
			mColor = "255 255 255 255";
		};
		%this.queueOptMenu.add(%this.queueOptMenu.ignoreBtn);
	}
}

function updaterDlg::clickQueueItemSkip(%this, %queueObj)
{
	%this.removeAddOnSwatch(%queueObj.guiSwatch);
	%queueObj.delete();

	if(updater.fileDownloader.queue.getCount() < 1)
		canvas.popDialog(%this);
}

function updaterDlg::clickQueueItemIgnore(%this, %queueObj)
{
	messageBoxYesNo("Warning", "Are you sure you want to disable all future updates for" SPC %queueObj.name @ "?",
		"$Pref::Updater::Ignore[" @ %queueObj.name @ "]=true;" @ %this @ ".clickQueueItemSkip(" @ %queueObj @ ");");
}

function updaterDlg::viewChangeLog(%this, %queueObj)
{
	if(strLen(%queueObj.updateChangeLog) > 0)
	{
		if(strLen(%queueObj.updateChangeLogText) > 0)
		{
			if(!%queueObj.updateChangeLogParsed)
			{
				%queueObj.updateChangeLogText = parseCustomTML(
					"<color:ffffff><linkcolor:cccccc>" @ %queueObj.updateChangeLogText,
					updaterDlgChangeLogText,
					"updaterChangeLog\tdefault");
				%queueObj.updateChangeLogParsed = true;
			}
			updaterDlgChangeLogText.setText(%queueObj.updateChangeLogText);
		}
		else
		{
			updater.fileDownloader.downloadChangeLog(%queueObj);
			updaterDlgChangeLogText.setText("<color:ffffff><just:center>\n\n\n\nDownloading change log...");
		}
	}
	else
	{
		updaterDlgChangeLogText.setText("<color:ffffff><just:center>\n\n\n\nNo change log is available.");
	}
}

function updaterDlg::generateAddOnSwatch(%this, %queueObj)
{
	%parExt = updaterDlgAddOnSwatch.getExtent();
	%parExtX = getWord(%parExt, 0);
	%parExtY = getWord(%parExt, 1);
	%extX = %parExtX - 5;
	%extY = 75;
	%posX = 0;
	%posY = %parExtY;

	%path = "Add-Ons/" @ %queueObj.name @ "/";
	if(isFile(%path @ "thumb.jpg"))
		%image = %path @ "thumb.jpg";
	else if(isFile(%path @ "thumb.png"))
		%image = %path @ "thumb.png";
	else
		%image = "Add-Ons/GameMode_Custom/thumb.jpg";

	%swatch = new GuiSwatchCtrl()
	{
		profile = "GuiDefaultProfile";
		horizSizing = "width";
		vertSizing = "bottom";
		position = %posX SPC %posY;
		extent = %extX SPC %extY;
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		color = "0 0 0 110";
	};

	%swatch.progress = new GuiProgressCtrl()
	{
		profile = "updaterProgressProfile";
		horizSizing = "width";
		vertSizing = "bottom";
		position = "0 0";
		extent = %extX SPC %extY;
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
	};
	%swatch.add(%swatch.progress);
	%swatch.image = new GuiBitmapCtrl()
	{
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "5 5";
		extent = "64 64";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		bitmap = %image;
		wrap = "0";
		lockAspectRatio = "0";
		alignLeft = "0";
		alignTop = "0";
		overflowImage = "0";
		keepCached = "0";
		mColor = "255 255 255 255";
		mMultiply = "0";
	};
	%swatch.add(%swatch.image);
	%swatch.text = new GuiMLTextCtrl()
	{
		profile = "GuiMLTextProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "77 3";
		extent = "502 70";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		lineSpacing = "2";
		allowColorChars = "0";
		maxChars = "-1";
		text = "";
		maxBitmapHeight = "-1";
		selectable = "0";
		autoResize = "0";
	};
	%swatch.add(%swatch.text);
	%swatch.button = new GuiBitmapButtonCtrl()
	{
		profile = "GuiDefaultProfile";
		horizSizing = "width";
		vertSizing = "bottom";
		position = "0 0";
		extent = %extX SPC %extY;
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		command = "updaterDlg.clickQueueItem(" @ %queueObj @ ");";
		text = " ";
		groupNum = "-1";
		buttonType = "PushButton";
		bitmap = "base/client/ui/btnBlank";
		lockAspectRatio = "0";
		alignLeft = "0";
		alignTop = "0";
		overflowImage = "0";
		mKeepCached = "0";
		mColor = "255 255 255 255";
	};
	%swatch.add(%swatch.button);
	%swatch.optButton = new GuiBitmapButtonCtrl()
	{
		profile = "ImpactTextProfile";
		horizSizing = "left";
		vertSizing = "bottom";
		position = %extX - 40 SPC 1;
		extent = "22 47";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		command = "updaterDlg.clickQueueItemOpt(" @ %queueObj @ ");";
		text = "...";
		groupNum = "-1";
		buttonType = "PushButton";
		bitmap = "base/client/ui/btnBlank";
		lockAspectRatio = "0";
		alignLeft = "0";
		alignTop = "0";
		overflowImage = "0";
		mKeepCached = "0";
		mColor = "255 255 255 255";
	};
	%swatch.add(%swatch.optButton);

	%info = "<color:ffffff><h2>" @ %queueObj.name @ "</h2>" @
		"<ul><li><b>Version:</b>" SPC %queueObj.updateVersion @ "</li></ul>";
	%info = parseCustomTML(%info, %swatch.text, "default");
	%swatch.text.setText(%info);

	updaterDlgAddOnSwatch.extent = %parExtX SPC %parExtY + %extY + 5;
	updaterDlgAddOnSwatch.add(%swatch);

	%queueObj.guiSwatch = %swatch;
}

function updaterDlg::removeAddOnSwatch(%this, %swatch)
{
	%posY = getWord(%swatch.getPosition(), 1);
	%extY = getWord(%swatch.getExtent(), 1);
	%swatch.delete();

	%count = updaterDlgAddOnSwatch.getCount();
	for(%i = 0; %i < %count; %i ++)
	{
		%obj = updaterDlgAddOnSwatch.getObject(%i);
		%pos = %obj.getPosition();
		%pX = getWord(%pos, 0);
		%pY = getWord(%pos, 1);
		if(%pY > %posY + %extY)
		{
			%ext = %obj.getExtent();
			%eX = getWord(%ext, 0);
			%eY = getWord(%ext, 1);
			%obj.resize(%pX, %pY - 80, %eX, %eY);
		}
	}

	%pos = updaterDlgAddOnSwatch.getPosition();
	%posX = getWord(%pos, 0);
	%posY = getWord(%pos, 1);
	%ext = updaterDlgAddOnSwatch.getExtent();
	%extX = getWord(%ext, 0);
	%extY = getWord(%ext, 1);
	updaterDlgAddOnSwatch.resize(%posX, %posY, %extX, %extY - 80);
}