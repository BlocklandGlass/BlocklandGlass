//----------------------------------------------------------------------
// Title:   Support_PreLoad
// Author:  Greek2me
// Version: 3
// Updated: June 24, 2014
//----------------------------------------------------------------------
// Installs a script into "config/main.cs", which is executed
// immediately after Blockland starts, before add-ons have loaded.
//----------------------------------------------------------------------
// Place a file in your add-on called "preload.cs" to have it executed.
//
// Include this code in your own scripts as an *individual file*
// called "Support_PreLoad.cs". Do not modify this code.
//----------------------------------------------------------------------

if($Support_PreLoad::Version > 3)
	return false;
$Support_PreLoad::Version = 3;

//Installs the script launcher.
function installPreLoadScriptLauncher()
{
	if(!isWriteableFileName("config/main.cs"))
		return false;
	%write = new FileObject();
	%write.openForAppend("config/main.cs");
	%write.writeLine(
		"///BEGIN PRE-LOAD SCRIPT LAUNCHER///" NL
		"if(!$PreLoadScriptsRun)" NL
		"{" NL
		"	%mask = \"Add-Ons/*/preload.cs\";" NL
		"	for(%file = findFirstFile(%mask); %file !$= \"\"; %file = findNextFile(%mask))" NL
		"		%fileList = setField(%fileList, getFieldCount(%fileList), %file);" NL
		"	%fileCount = getFieldCount(%fileList);" NL
		"	for(%fileIndex = 0; %fileIndex < %fileCount; %fileIndex ++)" NL
		"	{" NL
		"		%file = getField(%fileList, %fileIndex);" NL
		"		%path = filePath(%file);" NL
		"		%dirName = getSubStr(%path, strPos(%path, \"/\") + 1, strLen(%path));" NL
		"		if(strPos(%dirName, \"/\") == -1)" NL
		"		{" NL
		"			echo(\"\\n\\c4Pre-Loading Add-On:\" SPC %dirName);" NL
		"			exec(%file);" NL
		"		}" NL
		"	}" NL
		"	$PreLoadScriptsRun = true;" NL
		"	$Pref::PreLoadScriptLauncherInstalled = true;" NL
		"}" NL
		"///END PRE-LOAD SCRIPT LAUNCHER///");
	%write.close();
	%write.delete();
	$Pref::PreLoadScriptLauncherInstalled = true;
	echo("Pre-load script launcher installed.");
	return true;
}

if(!$Pref::PreLoadScriptLauncherInstalled || !isFile("config/main.cs"))
	return installPreLoadScriptLauncher();