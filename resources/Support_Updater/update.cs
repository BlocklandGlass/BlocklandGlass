function updaterDediServerPrompt()
{
	if($Pref::Updater::DediServerPromptDisplayed)
		return;
	$Pref::Updater::DediServerPromptDisplayed = true;
	%url = "bitbucket.org/Greek2me/support_updater/wiki/Dedicated%20Server%20Updates";
	messageBoxYesNo
	(
		"Support_Updater",
		"<spush><font:arial bold:14>Do you run a dedicated server?<spop>" NL
			"\nSupport_Updater can keep your dedicated server up-to-date!" SPC
			"Would you like to see instructions on setting up dedicated" SPC
		"server updates?",
		"goToWebPage(\"" @ %url @ "\");"
	);
}

updaterDediServerPrompt();