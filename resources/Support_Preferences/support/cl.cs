// apparently getFirstWord sometimes doesn't exist.
// --------------------------------------
function getFirstWord(%value) {
	return getWord(%value, 0);
}

// convenience from Server_Prefs
function clampFloat(%float, %min, %max) {
	%decPos = strPos(%float, ".");
    %floatLen = strLen(getSubStr(%float, %decPos+1, 2));
    if(%decPos != -1 && %floatLen != 0)
    {
        %value = mFloatLength(%float, %floatLen);
   
        if(%value > %max)
			%value = max;
      
        if(%value < %min)
			%value = %min;
    }
	else if(%decPos != -1)
    {
		%value = mClamp(%float, %min, %max) @ ".";
	}
    else 
    {
		%value = mClamp(%float, %min, %max);
	}
	
	return %value;
}