exec("./common.cs");

if(!isObject(updaterProgressProfile))
{
	new GuiControlProfile(updaterProgressProfile : GuiProgressProfile)
	{
		border = 0;
		fillColor = "90 230 70 255";
	};
}
if(!isObject(updaterSmallImpactBtnProfile))
{
	new GuiControlProfile(updaterImpactPopupButtonProfile : ImpactBackButtonProfile)
	{
		fontSize = 20;
	};
}

if(!isObject(updaterDlg))
	exec("./client/gui/updaterDlg.gui");
exec("./client/gui/updaterDlg.cs");
if(!isObject(updaterOptDlg))
	exec("./client/gui/updaterOptDlg.gui");

//Open the options menu on first run
if($Pref::Updater::OptDlgShown !$= "0.5.0")
{
	$Pref::Updater::OptDlgShown = "0.5.0";
	canvas.schedule(100, "pushDialog", updaterOptDlg);
}

function updaterInterfaceDisplay()
{
	if(!updaterDlg.isAwake())
		canvas.pushDialog(updaterDlg);
}

function updaterInterfacePushItem(%item)
{
	if(isObject(canvas))
	{
		updaterDlg.generateAddOnSwatch(%item);
		if(updater.fileDownloader.queue.getCount() == 1)
		{
			updaterDlg.clickQueueItem(%item);
		}
	}
}

function updaterInterfacePopItem(%item)
{
	updaterDlg.removeAddOnSwatch(%item.guiSwatch);
}

function updaterInterfaceSelectItem(%item)
{
	updaterDlg.clickQueueItem(%item);
}

function updaterInterfaceOnQueueEmpty()
{
	canvas.popDialog(updaterDlg);

	if(updater.restartRequired)
	{
		if(updater.hasErrors)
			messageBoxOK("", "Updater completed with errors. Some add-ons might not have been updated.\n\nCheck the console for details.");
		messageBoxYesNo("", "Some updates require a restart.\n\nClose Blockland now?", "quit();");
	}
	else if(updater.hasErrors)
		messageBoxOK("", "Updater completed with errors. Some add-ons might not have been updated.\n\nCheck the console for details.");
	else
		messageBoxOK("", "All updates have been completed.");
}