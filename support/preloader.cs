///BEGIN PRE-LOAD SCRIPT LAUNCHER///
if(!$PreLoadScriptsRun) {
	//$Pref variables are for backwards compatibility, do not reference them
	$Pref::PreLoadScriptLauncherInstalled = true;
	$Pref::PreLoadScriptLauncherVersion = 3;

	$PreLoadScriptLauncherVersion = 3;
	$PreLoadScriptLauncherInstalled = true;

  %mask = "Add-Ons/*/preload.cs";
  for(%file = findFirstFile(%mask); %file !$= ""; %file = findNextFile(%mask))
    %fileList = setField(%fileList, getFieldCount(%fileList), %file);

  %fileCount = getFieldCount(%fileList);
  for(%fileIndex = 0; %fileIndex < %fileCount; %fileIndex ++) {
    %file = getField(%fileList, %fileIndex);
    %path = filePath(%file);
    %dirName = getSubStr(%path, strPos(%path, "/") + 1, strLen(%path));
    if(strPos(%dirName, "/") == -1 && !$AddonPreloaded__[%dirName]) {
      echo("\n\c4Pre-Loading Add-On:" SPC %dirName);
      exec(%file);
      $AddonPreloaded__[%dirName] = true;
    }
  }

	$PreLoadScriptsRun = true;
}
///END PRE-LOAD SCRIPT LAUNCHER///
