//================================================
// General scripts
//
// Generic scripts that have no real place to be placed
//
//================================================

// Repeat a string an amount of times
function strrepeat(%str, %num)
{
	%ret = "";
	for (%i = 0; %i < %num; %i++)
		%ret = %ret @ %str;
	return %ret;
}

// Converts a number to a string correctly
function num2str(%num)
{
	if (mAbs(%num) < 1000000)
		return "" @ %num;
	if (%num < 0)
	{
		%neg = true;
		%num = -%num;
	}
	%decimals = %num - mFloor(%num);
	%decilen = strlen(%decimals);
	%million = mFloor(%num / 1000000);
	%num = %num % 1000000;
	%len = 6 - strlen(%num);
	for (%i = 0; %i < %len; %i++)
		%num = 0 @ %num;
	return (%neg ? "-" : "") @ %million @ %num @ (%decilen > 1 ? getSubStr(%decimals, 1, %decilen) : "");
}
