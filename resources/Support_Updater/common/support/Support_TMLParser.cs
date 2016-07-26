//----------------------------------------------------------------------
// Title:   Support_TMLParser
// Author:  Greek2me
// Version: 5
// Updated: June 14, 2015
//----------------------------------------------------------------------
// Custom Torque Markup Language Parser
// Allows for the pasing of custom TML formatting.
// USAGE:
//  - Create a function called "customTMLParser_IDENTIFIER", where
//    IDENTIFIER is a unique name for your parser. See the function,
//    "customTMLParser_default", for an example.
//  - Parse your text like this: parseCustomTML(%str, %obj, %identi);
//    %str is the string to parse, %obj is an object used for parsing,
//    and %identi is the identifier for your parser.
//  - %obj is not required but is HIGHLY recommended as some formatting
//    depends on it.
//  - You may specify multiple parsers by separating them with tabs. The
//    string will be parsed according to all of  them.
//----------------------------------------------------------------------
// REQUIREMENTS:
//  - libstr (Support_LibStr)
//----------------------------------------------------------------------
// Include this code in your own scripts as an *individual file*
// called "Support_TMLParser.cs". Do not modify this code.
//----------------------------------------------------------------------

if($customTMLParser::version >= 5 && !$Debug)
	return;
$customTMLParser::version = 5;

$customTMLParser::leftBracket = "<";
$customTMLParser::rightBracket = ">";
$customTMLParser::div = ":";

$customTMLParser::listBulletIndent = 2;
$customTMLParser::listTextIndent = 3;

function customTMLParser_default(%obj, %value0, %value1, %value2, %value3, %value4, %value5, %value6, %value7, %value8, %value9, %value10, %value11, %value12, %value13, %value14, %value15)
{
	if(%obj.TML_listLevel $= "")
		%obj.TML_listLevel = 0;
	if(%obj.TML_listBulletIndent $= "")
		%obj.TML_listBulletIndent = $customTMLParser::listBulletIndent;
	if(%obj.TML_listTextIndent $= "")
		%obj.TML_listTextIndent = $customTMLParser::listBulletIndent;

	switch$(%value[0])
	{
		case "sPush":
			%obj.TML_fontCacheColors = setField(%obj.TML_fontCacheColors, getFieldCount(%obj.TML_fontCacheColors), %obj.TML_fontColor);
			%obj.TML_fontCacheTypes = setField(%obj.TML_fontCacheTypes, getFieldCount(%obj.TML_fontCacheTypes), %obj.TML_fontType);
			%obj.TML_fontCacheSizes = setField(%obj.TML_fontCacheSizes, getFieldCount(%obj.TML_fontCacheSizes), %obj.TML_fontSize);

		case "sPop":
			%obj.TML_fontColor = getField(%obj.TML_fontCacheColors, getFieldCount(%obj.TML_fontCacheColors) - 1);
			%obj.TML_fontType = getField(%obj.TML_fontCacheTypes, getFieldCount(%obj.TML_fontCacheTypes) - 1);
			%obj.TML_fontSize = getField(%obj.TML_fontCacheSizes, getFieldCount(%obj.TML_fontCacheSizes) - 1);
			%obj.TML_fontCacheColors = removeField(%obj.TML_fontCacheColors, getFieldCount(%obj.TML_fontCacheColors) - 1);
			%obj.TML_fontCacheTypes = removeField(%obj.TML_fontCacheTypes, getFieldCount(%obj.TML_fontCacheTypes) - 1);
			%obj.TML_fontCacheSizes = removeField(%obj.TML_fontCacheSizes, getFieldCount(%obj.TML_fontCacheSizes) - 1);

		case "b":
			if(strLen(%obj.TML_fontType) && striPos(%obj.TML_fontType, "bold") < 0)
				return true TAB "<sPush><font:" @ %obj.TML_fontType SPC "bold:" @ %obj.TML_fontSize @ ">";
			else
				return true TAB "<sPush><font:arial bold:15>";

		case "/b":
			return true TAB "<sPop>";

		case "i":
			if(isObject(%obj))
			{
				if(striPos(%obj.TML_fontType, "italic") < 0)
					return true TAB "<sPush><font:" @ %obj.TML_fontType SPC "italic:" @ %obj.TML_fontSize @ ">";
				else
					return true TAB "<sPush>";
			}
			else
				return true TAB "<sPush><font:arial italic:15>";

		case "/i":
			return true TAB "<sPop>";

		case "u":
			%obj.TML_underline = true;
			return true TAB "<sPush><linkcolor:" @ %obj.TML_fontColor @ "><linkcolorHL:" @ %obj.TML_fontColor @ "><a:&UNDERLINE>";

		case "/u":
			%obj.TML_underline = false;
			return true TAB "</a><sPop>";

		case "font":
			%obj.TML_fontType = %value[1];
			%obj.TML_fontSize = %value[2];

		case "size":
			return true TAB "<sPush><font:" @ %obj.TML_fontType @ ":" @ %value[1] @ ">";

		case "/size":
			return true TAB "<sPop>";

		case "colorHex": //DEPRECATED - USE <COLOR>
			warn("Support_TMLParser - The <colorHex> tag is deprecated. Please use <color> instead.");
			return true TAB "<color:" @ %value[1] @ ">";
		
		case "color":
			%obj.TML_fontColor = %value[1];
			if(%obj.TML_underline)
				return true TAB "<sPush><color:" @ %value[1] @ "><linkcolor:" @ %obj.TML_fontColor @ "><linkcolorHL:" @ %obj.TML_fontColor @ ">";
			else
				return true TAB "<sPush><color:" @ %value[1] @ ">";

		case "/color":
			return true TAB "<sPop>";

		case "/just":
			return true TAB "<just:left>";

		case "h1":
			return true TAB "<sPush><font:arial bold:24>";

		case "/h1":
			return true TAB "<sPop><br>";

		case "h2":
			return true TAB "<sPush><font:arial bold:20>";

		case "/h2":
			return true TAB "<sPop><br>";

		case "h3":
			return true TAB "<sPush><font:arial bold:17>";

		case "/h3":
			return true TAB "<sPop><br>";

		case "ol":
			%obj.TML_listLevel ++;
			%obj.TML_listMode[%obj.TML_listLevel] = "ol";
			%obj.TML_listIndex[%obj.TML_listLevel] = (%value[1] $= "" ? 0 : %value[1] - 1);
			return true TAB "";

		case "/ol":
			%obj.TML_listMode[%obj.TML_listLevel] = "";
			%obj.TML_listIndex[%obj.TML_listLevel] = "";
			%obj.TML_listLevel --;
			if(%obj.TML_listLevel == 0)
				return true TAB "<lmargin%:0>";
			else
				return true TAB "";

		case "ul":
			%obj.TML_listLevel ++;
			%obj.TML_listMode[%obj.TML_listLevel] = "ul";
			return true TAB "";

		case "/ul":
			%obj.TML_listMode[%obj.TML_listLevel] = "";
			%obj.TML_listLevel --;
			if(%obj.TML_listLevel == 0)
				return true TAB "<lmargin%:0>";
			else
				return true TAB "";

		case "li":
			%level = %obj.TML_listLevel;
			%indentBullet = %level * %obj.TML_listBulletIndent + (%level - 1) * %obj.TML_listTextIndent;
			%indentText = %indentBullet + %obj.TML_listTextIndent;

			if(%obj.TML_listMode[%obj.TML_listLevel] $= "ul")
			{
				if(%value[1] $= "" || !isFile(%value[1]))
					%bullet = "<b>+</b>";
				else
					%bullet = "<bitmap:" @ %value[1] @ ">";
				return true TAB "<br><lmargin%:" @ %indentBullet @ ">" @ %bullet @ "<lmargin%:" @ %indentText @ ">";
			}
			else if(%obj.TML_listMode[%obj.TML_listLevel] $= "ol")
			{
				%num = %obj.TML_listIndex[%obj.TML_listLevel] ++;
				return true TAB "<br><lmargin%:" @ %indentBullet @ "><b>" @ %num @ ".</b><lmargin%:" @ %indentText @ ">";
			}
			else
			{
				return true TAB "";
			}

		case "/li":
			if(%obj.TML_listLevel > 0)
			{
				%indentText = %obj.TML_listBulletIndent * (%obj.TML_listLevel - 1) + %obj.TML_listTextIndent;
				return true TAB "<lmargin%:" @ %indentText @ ">";
			}
			else
				return true TAB "";
	}

	return false;
}

//Parses custom TML formatting.
//@param	string string	The string to parse.
//@param	SimObject obj	The GuiMLTextCtrl containing the string. (recommended)
//@param	string parserFunction	Used to parse the string. Place multiple in a tab-delimited list to parse in list order.
//@return	string	A TML-formatted string.
function parseCustomTML(%string, %obj, %parserFunction)
{
	return _parseCustomTML(%string, %obj, %parserFunction, false);
}

function _parseCustomTML(%string, %obj, %parserFunction, %isRecursing)
{
	if(%parserFunction $= "")
		%parserFunction = "default";
	%parserIndex = 0;

	if(!%isRecursing && isObject(%obj) && %obj.getClassName() $= "GuiMLTextCtrl")
	{
		%obj.TML_fontColor = rgbToHex(%obj.profile.fontColor);
		%obj.TML_fontType = %obj.profile.fontType;
		%obj.TML_fontSize = %obj.profile.fontSize;
		%obj.TML_fontCacheColors = %obj.TML_fontColor;
		%obj.TML_fontCacheTypes = %obj.TML_fontType;
		%obj.TML_fontCacheSizes = %obj.TML_fontSize;
	}

	for(%i = 0; %i < strLen(%string); %i ++)
	{
		%char = getSubStr(%string, %i, 1);

		if(%char $= $customTMLParser::leftBracket)
			%start = %i;

		if(%char $= $customTMLParser::rightBracket && %start !$= "")
		{
			%end = %i;

			for(%e = 0; %e < getFieldCount(%parserFunction); %e ++)
			{
				%full = getSubStr(%string, %start, %end-%start+1);
				%contents = getSubStr(%full, 1, strLen(%full) - 2);

				%search = %contents;
				%pos = -1;
				%numValues = 0;
				while(strPos(%search, $customTMLParser::div) >= 0)
				{
					%search = getSubStr(%search, %pos+1, strLen(%search));

					%pos = strPos(%search, $customTMLParser::div);
					if(%pos >= 0)
					{
						%value[%numValues] = getSubStr(%search, 0, %pos);
					}
					else
					{
						%value[%numValues] = %search;
					}
					%numValues ++;
				}
				if(%numValues <= 0 && %contents !$= "")
				{
					%value[0] = %contents;
					%numValues = 1;
				}

				%parser = getField(%parserFunction, %e);
				%replace = call("customTMLParser_" @ %parser, %obj, %value0, 
					%value1, %value2, %value3, %value4, %value5, %value6, %value7, 
					%value8, %value9, %value10, %value11, %value12, %value13, 
					%value14, %value15);
				if(getField(%replace, 0))
				{
					//check if there are more parsers to use
					%replace = getFields(%replace, 1);
					if(striPos(%replace, %full) != -1)
					{
						%sub = true;
						%replace = striReplace(%replace, %full, "&SUB_FULL");
					}
					%obj.TML_skipFont = true;
					%replace = parseCustomTML(%replace, %obj, %parserFunction, true);
					if(%sub)
					{
						%sub = false;
						%replace = striReplace(%replace, "&SUB_FULL", %full);
					}
					//replace %full with %replace
					if(%full !$= %replace)
					{
						%string = setSubStr(%string, %start, %end - %start + 1, %replace);
						%end = %start + strLen(%replace) - 1;
						%i = %end;
					}
					break;
				}
			}

			%start = "";
			%end = "";
			%full = "";
			%contents = "";
			%search = "";
			%replace = "";
			for(%e = 0; %e < %numValues; %e ++)
				%value[%e] = "";
			%numValues = "";
		}
	}

	return %string;
}

//Parses a file containing custom TML formatting.
//@param	string path	The file path.
//@param	SimObject obj	The GuiMLTextCtrl containing the string. (recommended)
//@param	string parserFunction	Used to parse the string. Place multiple in a tab-delimited list to parse in list order.
//@param	FileObject fo	An optional FileObject to use if you are parsing lots of files.
//@return	string	The TML-formatted string.
function parseCustomTMLFile(%path, %obj, %parserFunction, %fo)
{
	%result = "";
	if(%del = !isObject(%fo))
		%fo = new FileObject();
	%fo.openForRead(%path);
	while(!%fo.isEOF())
	{
		%result = %result @ _parseCustomTML(%fo.readLine(), %obj, %parserFunction, false);
	}
	%fo.close();
	if(%del)
		%fo.delete();
	return %result;
}

function rgbToHex(%rgb)
{
	%r = getWord(%rgb, 0);
	%g = getWord(%rgb, 1);
	%b = getWord(%rgb, 2);
	%a = "0123456789ABCDEF";

	%r = getSubStr(%a, (%r - (%r % 16)) / 16, 1) @ getSubStr(%a, %r % 16, 1);
	%g = getSubStr(%a, (%g - (%g % 16)) / 16, 1) @ getSubStr(%a, %g % 16, 1);
	%b = getSubStr(%a, (%b - (%b % 16)) / 16, 1) @ getSubStr(%a, %b % 16, 1);

	return %r @ %g @ %b;
}

package Support_TMLParser
{
	function GuiMLTextCtrl::onURL(%this, %url)
	{
		if(%url !$= "&UNDERLINE")
			return parent::onURL(%this, %url);
	}
};
activatePackage(Support_TMLParser);