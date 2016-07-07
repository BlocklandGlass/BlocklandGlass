//Author: Xalos

//Check if the library version is higher than ours
if($Library::Datetime::Version > 2) return;
$Library::Datetime::Version = 2;


/// Gets the difference in number of days between two dates.
/// %date1: The first date in MM/DD/YYYY hh:mm:ss format.
/// %date2: The second date in MM/DD/YYYY hh:mm:ss format.
/// Returns: The difference in days.
function DT_getDayDifference(%date1, %date2)
{
	%date1 = strReplace(strReplace(%date1, "/", " "), ":", " ");
	%date2 = strReplace(strReplace(%date2, "/", " "), ":", " ");
	%year1 = getWord(%date1, 2); %leap1 = DT_isLeapYear(%year1);
	%year2 = getWord(%date2, 2); %leap2 = DT_isLeapYear(%year2);
	%offset = "0 31 59 90 120 151 181 212 243 273 304 334";
	%month1 = getWord(%date1, 0); %month2 = getWord(%date2, 0);
	%day1 = getWord(%date1, 1) + getWord(%offset, %month1 - 1) + (%month1 >= 3 && %leap1);
	%day2 = getWord(%date2, 1) + getWord(%offset, %month2 - 1) + (%month2 >= 3 && %leap2);
	%dayDiff = %day2 - %day1;
	%time1 = getWord(%date1, 3) * 3600 + getWord(%date1, 4) * 60 + getWord(%date1, 5);
	%time2 = getWord(%date2, 3) * 3600 + getWord(%date2, 4) * 60 + getWord(%date2, 5);
	%timeDiff = %time2 - %time1;
	%yearDiff = getWord(%date2, 2) - getWord(%date1, 2);
	%dayDiff += %yearDiff * 365;
	return %dayDiff;
}

/// Gets the difference in time between two dates.
/// %date1: The first date in MM/DD/YYYY hh:mm:ss format.
/// %date2: The second date in MM/DD/YYYY hh:mm:ss format.
/// Returns: The difference in time, in seconds.
function DT_getTimeDifference(%date1, %date2)
{
	%date1 = strReplace(strReplace(%date1, "/", " "), ":", " ");
	%date2 = strReplace(strReplace(%date2, "/", " "), ":", " ");
	%year1 = getWord(%date1, 2); %leap1 = DT_isLeapYear(%year1);
	%year2 = getWord(%date2, 2); %leap2 = DT_isLeapYear(%year2);
	%offset = "0 31 59 90 120 151 181 212 243 273 304 334";
	%month1 = getWord(%date1, 0); %month2 = getWord(%date2, 0);
	%day1 = getWord(%date1, 1) + getWord(%offset, %month1 - 1) + (%month1 >= 3 && %leap1);
	%day2 = getWord(%date2, 1) + getWord(%offset, %month2 - 1) + (%month2 >= 3 && %leap2);
	%dayDiff = %day2 - %day1;
	%time1 = getWord(%date1, 3) * 3600 + getWord(%date1, 4) * 60 + getWord(%date1, 5);
	%time2 = getWord(%date2, 3) * 3600 + getWord(%date2, 4) * 60 + getWord(%date2, 5);
	%timeDiff = %time2 - %time1;
	%yearDiff = getWord(%date2, 2) - getWord(%date1, 2);
	%dayDiff += %yearDiff * 365;
	%timeDiff += %dayDiff * 86400;
	return %timeDiff;
}

/// Gets the day of the week for an input date.
/// %date: The date in MM/DD/YYYY hh:mm:ss format.
/// Returns: The day of the week, from 0 to 6 mapping to SMTWTFS.
function DT_getWeekDay(%date)
{
	%date = strReplace(strReplace(%date, "/", " "), ":", " ");
	%day = getWord(%date, 1);
	%month = getWord("0 3 3 6 1 4 6 2 5 0 3 5", getWord(%date, 0) - 1);
	%year = "00"@getWord(%date, 2);
	%yy = getSubStr(%year, strLen(%year) - 2, 2);
	%yQuot = mFloor(%yy / 4);
	%cent = getWord("6 4 2 0", mFloor(%year / 100) % 4);
	%final = %day + %month + %yy + %yQuot + %cent;
	if(%month <= 1) %final--; return %final % 7;
}

/// Checks if a year is a leap year.
/// %year: The year to check against.
/// Returns: 1 if the year is a leapyear and 0 otherwise.
function DT_isLeapYear(%year) { return (%year % 100) && !(%year % 4) || !(%year % 400); }