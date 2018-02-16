// ================
// ISAAC fast cryptographic random number generator
// ================
// Author: McTwist
// Description: Generates cryptographic safe random numbers
// License: Free to use
// Source: http://www.burtleburtle.net/bob/rand/isaacafa.html
// ================

// Init vars
$ISAAC::aa = 0;
$ISAAC::bb = 0;
$ISAAC::cc = 0;
$ISAAC::gnt = 0;

// Get a random cryptographic number
function randc()
{
	if (!$ISAAC::gnt--)
	{
		isaac();
		$ISAAC::gnt = 255;
	}
	return $ISAAC::rands[$ISAAC::gnt];
}

// Generate a new list of random numbers
function isaac()
{
	$ISAAC::cc = safe_add($ISAAC::cc, 1);
	$ISAAC::bb = safe_add($ISAAC::bb, $ISAAC::cc);

	for (%i = 0; %i < 256; %i++)
	{
		%x = $ISAAC::mm[%i];

		switch (%i % 4)
		{
			case 0: $ISAAC::aa ^= ($ISAAC::aa << 13);
			case 1: $ISAAC::aa ^= ($ISAAC::aa >> 6);
			case 2: $ISAAC::aa ^= ($ISAAC::aa << 2);
			case 3: $ISAAC::aa ^= ($ISAAC::aa >> 16);
		}

		$ISAAC::aa = safe_add($ISAAC::mm[(%i + 128) % 256], $ISAAC::aa);
		$ISAAC::mm[%i] = %y = safe_add($ISAAC::mm[(%x >> 2) % 256], safe_add($ISAAC::aa, $ISAAC::bb));
		$ISAAC::rands[%i] = $ISAAC::bb = safe_add($ISAAC::mm[(%y >> 10) % 256], %x);
	}
}

// Initialize the randomizer
function isaac_init(%flag)
{
	$ISAAC::aa = $ISAAC::bb = $ISAAC::cc = 0;
	%a = %b = %c = %d = %e = %f = %g = %h = 0x9e3779b9;

	// Generate seed
	if (%flag && $ISAAC::rands[0] $= "")
	{
		for (%i = 0; %i < 256; %i++)
		{
			$ISAAC::rands[%i] = getRandom(0, 0x7fffffff) | 0;
		}
	}

	// Scrambled eggs
	for (%i = 0; %i < 4; %i++)
	{
		// Mix
		%a ^= %b << 11; %d = safe_add(%d, %a); %b = safe_add(%b, %c);
		%b ^= %c >> 2;  %e = safe_add(%e, %b); %c = safe_add(%c, %d);
		%c ^= %d << 8;  %f = safe_add(%f, %c); %d = safe_add(%d, %e);
		%d ^= %e >> 16; %g = safe_add(%g, %d); %e = safe_add(%e, %f);
		%e ^= %f << 10; %h = safe_add(%h, %e); %f = safe_add(%f, %g);
		%f ^= %g >> 4;  %a = safe_add(%a, %f); %g = safe_add(%g, %h);
		%g ^= %h << 8;  %b = safe_add(%b, %g); %h = safe_add(%h, %a);
		%h ^= %a >> 9;  %c = safe_add(%c, %h); %a = safe_add(%a, %b);
	}

	// Fill in mm with messy stuff
	for (%i = 0; %i < 256; %i++)
	{
		if (%flag)
		{
			%a = safe_add(%a, $ISAAC::rands[%i]);
			%b = safe_add(%b, $ISAAC::rands[%i + 1]);
			%c = safe_add(%c, $ISAAC::rands[%i + 2]);
			%d = safe_add(%d, $ISAAC::rands[%i + 3]);
			%e = safe_add(%e, $ISAAC::rands[%i + 4]);
			%f = safe_add(%f, $ISAAC::rands[%i + 5]);
			%g = safe_add(%g, $ISAAC::rands[%i + 6]);
			%h = safe_add(%h, $ISAAC::rands[%i + 7]);
		}
		// Mix
		%a ^= %b << 11; %d = safe_add(%d, %a); %b = safe_add(%b, %c);
		%b ^= %c >> 2;  %e = safe_add(%e, %b); %c = safe_add(%c, %d);
		%c ^= %d << 8;  %f = safe_add(%f, %c); %d = safe_add(%d, %e);
		%d ^= %e >> 16; %g = safe_add(%g, %d); %e = safe_add(%e, %f);
		%e ^= %f << 10; %h = safe_add(%h, %e); %f = safe_add(%f, %g);
		%f ^= %g >> 4;  %a = safe_add(%a, %f); %g = safe_add(%g, %h);
		%g ^= %h << 8;  %b = safe_add(%b, %g); %h = safe_add(%h, %a);
		%h ^= %a >> 9;  %c = safe_add(%c, %h); %a = safe_add(%a, %b);

		$ISAAC::mm[%i] = %a;
		$ISAAC::mm[%i + 1] = %b;
		$ISAAC::mm[%i + 2] = %c;
		$ISAAC::mm[%i + 3] = %d;
		$ISAAC::mm[%i + 4] = %e;
		$ISAAC::mm[%i + 5] = %f;
		$ISAAC::mm[%i + 6] = %g;
		$ISAAC::mm[%i + 7] = %h;
	}

	// An another round to make merry go round
	if (%flag)
	{
		for (%i = 0; %i < 256; %i++)
		{
			%a = safe_add(%a, $ISAAC::mm[%i]);
			%b = safe_add(%b, $ISAAC::mm[%i + 1]);
			%c = safe_add(%c, $ISAAC::mm[%i + 2]);
			%d = safe_add(%d, $ISAAC::mm[%i + 3]);
			%e = safe_add(%e, $ISAAC::mm[%i + 4]);
			%f = safe_add(%f, $ISAAC::mm[%i + 5]);
			%g = safe_add(%g, $ISAAC::mm[%i + 6]);
			%h = safe_add(%h, $ISAAC::mm[%i + 7]);

			// Mix
			%a ^= %b << 11; %d = safe_add(%d, %a); %b = safe_add(%b, %c);
			%b ^= %c >> 2;  %e = safe_add(%e, %b); %c = safe_add(%c, %d);
			%c ^= %d << 8;  %f = safe_add(%f, %c); %d = safe_add(%d, %e);
			%d ^= %e >> 16; %g = safe_add(%g, %d); %e = safe_add(%e, %f);
			%e ^= %f << 10; %h = safe_add(%h, %e); %f = safe_add(%f, %g);
			%f ^= %g >> 4;  %a = safe_add(%a, %f); %g = safe_add(%g, %h);
			%g ^= %h << 8;  %b = safe_add(%b, %g); %h = safe_add(%h, %a);
			%h ^= %a >> 9;  %c = safe_add(%c, %h); %a = safe_add(%a, %b);

			$ISAAC::mm[%i] = %a;
			$ISAAC::mm[%i + 1] = %b;
			$ISAAC::mm[%i + 2] = %c;
			$ISAAC::mm[%i + 3] = %d;
			$ISAAC::mm[%i + 4] = %e;
			$ISAAC::mm[%i + 5] = %f;
			$ISAAC::mm[%i + 6] = %g;
			$ISAAC::mm[%i + 7] = %h;
		}
	}

	// Generate first batch
	isaac();
	$ISAAC::gnt = 256;
}

// Safely add two values together like they are unsigned
// Algorithm: https://github.com/rubycon/isaac.js/blob/master/isaac.js#L106
function safe_add(%a, %b)
{
	%lsb = (%a & 0xffff) + (%b & 0xffff);
	%msb = (%a >> 16) + (%b >> 16) + (%lsb >> 16);
	return (%msb << 16) | (%lsb & 0xffff);
}

// Initialize the first time
isaac_init(1);
