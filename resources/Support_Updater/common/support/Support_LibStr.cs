// NAME libstr
// AUTH Clockturn
// DESC String manipulation helper functions.
// IMPLEMENTS
//  striReplace TAKES string, replace, with, @startindex DESC Case-insensitive version of strreplace.
//  getSubStrr TAKES string, start, end DESC Get a substring based on a start and end point.
//  setSubStr TAKES string, start, count, insert DESC Insert a string into another string at a given start point and char count.
//  setSubStrr TAKES string, start, end, insert DESC Insert a string into another string at a given start and end point.
//  strRev TAKES string DESC Reverse a string.
//  getStrBetween TAKES string, pre, post, index DESC Get any substrings of a string that occur within the bounds of other given strings, and return by index of occurrence.
//  getLineCount REMOVED BECAUSE THIS IS DEFAULT NOW
//  getLine TAKES string, int DESC Returns token of that index in string tokenized by newlines only.
//  strMatch TAKES string, pattern DESC Returns true or false for if the given string matches the given pattern. Patterns support wildcards (*).
//  striMatch TAKES string, pattern DESC Case-insensitive version of strMatch.
//  typeOf TAKES anything, boolean DESC Returns whether the string matches masks for integers or floats or is just a string. (Integer, Float or String respectively). If boolean is true, negative numbers return as String.

function strireplace(%str,%rep,%with,%start)
{
	if(%str $= %rep)
	{
		return %with;
	}
	if(%rep $= %with)
	{
		return %str;
	}
	%len = strlen(%rep);
	%alen = strlen(%with);
	%pos = stripos(%str,%rep,%start);
	while(%pos != -1)
	{
		%str = getsubstr(%str,0,%pos) @ %with @ getsubstr(%str,%pos + %len,strlen(%str));
		%start = %pos + %alen;
		%pos = stripos(%str,%rep,%start);
	}
	return %str;
}
function getsubstrr(%str,%start,%end)
{
	return getsubstr(%str,%start,%end - %start);
}
function strrev(%str)
{
	for(%i=0;%i<strlen(%str);%i++)
	{
		%ret = %ret @ getsubstr(%str,%i,1);
	}
	return %ret;
}
function setsubstr(%str,%start,%count,%ins)
{
	return getsubstr(%str,0,%start) @ %ins @ getsubstr(%str,%start + %count,strlen(%str));
}
function setsubstrr(%str,%start,%end,%ins)
{
	return getsubstr(%str,0,%start) @ %ins @ getsubstr(%str,%end,strlen(%str));
}
function getLine(%str,%ind)
{
	%c = getLineCount(%str);
	if(%ind >= %c)
	{
		error("ERROR: getLine() - Index out of range (0-" @ %c - 1 @ "," SPC %ind @ ")");
		return "";
	}
	%pos = strpos(%str,"\n");
	%lpos = 0;
	while(%pos != -1 && %i < %ind)
	{
		%lpos = %pos + 1;
		%pos = strpos(%str,"\n",%lpos);
		%i++;
	}
	if(%pos == -1)
	{
		%pos = strlen(%str);
	}
	return getsubstrr(%str,%lpos,%pos);
}
function getStrBetween(%str,%before,%after,%index)
{
	if(%before $= "" || %after $= "")
	{
		return "";
	}
	%i = 0;
	while(%i <= %index)
	{
		%pos = strpos(%str,%before,%epos);
		if(%pos == -1)
		{
			return "";
		}
		%epos = strpos(%str,%after,%pos + strlen(%before));
		if(%epos == -1)
		{
			return "";
		}
		%content = getSubStr(%str,%pos + strlen(%before),%epos - (%pos + strlen(%before)));
		if(%i == %index)
		{
			return %content;
		}
		%epos += strlen(%after);
		%i++;
	}
	return "";
}
function strMatch(%str,%pattern)
{
	%curidx = 0;
	%str = expandEscape(%str);
	%pattern = expandEscape(%pattern);
	%patfields = strReplace(%pattern,"*","\t") @ "\t.";
	if(strpos(%str,getField(%patfields,0)) != 0)
	{
		return false;
	}
	%last = getField(%patfields,getfieldcount(%patfields)-2);
	if(%last !$= "")
	{
		if(strpos(%str,%last) != strlen(%str) - strlen(%last))
		{
			return false;
		}
	}
	for(%i=1;%i<getFieldCount(%patfields)-2;%i++)
	{
		if(%curidx > strlen(%str))
		{
			return false;
		}
		%mat = getField(%patfields,%i);
		if(%mat $= "")
		{
			continue;
		}
		%p = strPos(%str,%mat,%curidx);
		if(%p == -1)
		{
			return false;
		}
		%curidx = %p + strlen(%mat);
	}
	return true;
}
function striMatch(%str,%pattern)
{
	%curidx = 0;
	%str = expandEscape(%str);
	%pattern = expandEscape(%pattern);
	%patfields = strReplace(%pattern,"*","\t") @ "\t.";
	if(stripos(%str,getField(%patfields,0)) != 0)
	{
		return false;
	}
	%last = getField(%patfields,getfieldcount(%patfields)-2);
	if(%last !$= "")
	{
		if(stripos(%str,%last) != strlen(%str) - strlen(%last))
		{
			return false;
		}
	}
	for(%i=1;%i<getFieldCount(%patfields)-2;%i++)
	{
		if(%curidx > strlen(%str))
		{
			return false;
		}
		%mat = getField(%patfields,%i);
		if(%mat $= "")
		{
			continue;
		}
		%p = striPos(%str,%mat,%curidx);
		if(%p == -1)
		{
			return false;
		}
		%curidx = %p + strlen(%mat);
	}
	return true;
}
function typeOf(%var,%noNeg)
{
	%type = "Integer";
	%num = "0123456789";

	for(%i=0;%i<strlen(%var);%i++)
	{
		%ch = getsubstr(%var,%i,1);
		if(%ch $= "-" && %i == 0)
		{
			// Leading - for negative
			if(%noNeg)
			{
				return "String";
			}
			%a = 1;
			continue;
		}
		if(%ch $= ".")
		{
			// Decimal point
			if(%a || %type $= "Float" || %i == 0)
			{
				return "String";
			} else {
				%type = "Float";
			}
		} else {
			if(stripos(%num,%ch) == -1)
			{
				return "String";
			}
		}
		%a = 0;
	}
	return %type;
}