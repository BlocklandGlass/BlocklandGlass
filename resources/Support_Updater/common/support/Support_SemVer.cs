//----------------------------------------------------------------------
// Title:   Support_SemVer
// Author:  Greek2me
// Version: 1
// Updated: September 4, 2014
//----------------------------------------------------------------------
// Adds support for the Semantic Versioning System.
// http://semver.org
//----------------------------------------------------------------------
// Include this code in your own scripts as an *individual file*
// called "Support_SemVer.cs". Do not modify this code.
//----------------------------------------------------------------------

if($Support_SemVer::version >= 1 && !$Debug)
	return;
$Support_SemVer::version = 1;

//Compares two version numbers.
//@param	string version1
//@param	string version2
//@return	int	0 if equal, 1 if version1 is greater, 2 if version2 is greater
//@link	semver.org
function semanticVersionCompare(%version1, %version2)
{
	//remove build metadata - we don't care about it
	if((%pos = strPos(%version1, "+")) != -1)
		%version1 = getSubStr(%version1, 0, %pos);
	if((%pos = strPos(%version2, "+")) != -1)
		%version2 = getSubStr(%version2, 0, %pos);

	//separate into version and extension
	if((%pos = strPos(%version1, "-")) != -1)
	{
		%extension1 = getSubStr(%version1, %pos + 1, strLen(%version1));
		%version1 = getSubStr(%version1, 0, %pos);
	}
	if((%pos = strPos(%version2, "-")) != -1)
	{
		%extension2 = getSubStr(%version2, %pos + 1, strLen(%version2));
		%version2 = getSubStr(%version2, 0, %pos);
	}

	//major, minor, and patch version numbers
	%ver1 = strReplace(getField(%version1, 0), ".", " ");
	%ver2 = strReplace(getField(%version2, 0), ".", " ");
	%count = getMax(getWordCount(%ver1), getWordCount(%ver2));
	for(%i = 0; %i < %count; %i ++)
	{
		%n1 = getWord(%ver1, %i);
		%n2 = getWord(%ver2, %i);

		if(%n1 > %n2)
			return 1;
		else if(%n2 > %n1)
			return 2;
	}

	//extension
	%ext1 = strReplace(%extension1, ".", " ");
	%ext2 = strReplace(%extension2, ".", " ");
	%count = getMax(getWordCount(%ext1), getWordCount(%ext2));
	for(%i = 0; %i < %count; %i ++)
	{
		%n1 = getWord(%ext1, %i);
		%n2 = getWord(%ext2, %i);
		%isNum1 = (stripChars(%n1, "abcdefghijklmnopqrstuvwxyz-") $= %n1);
		%isNum2 = (stripChars(%n2, "abcdefghijklmnopqrstuvwxyz-") $= %n2);
		if(%isNum1 && %isNum2)
		{
			if(%n1 > %n2)
				return 1;
			else if(%n2 > %n1)
				return 2;
		}
		else if(%isNum1)
			return 1;
		else if(%isNum2)
			return 2;
		else
		{
			%cmp = striCmp(%n1, %n2);
			if(%cmp == 1)
				return 1;
			else if(%cmp == -1)
				return 2;
		}
	}

	return 0;
}