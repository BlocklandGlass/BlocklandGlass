// ==================
// Digest Access Authentication
// ==================
// Makes a plain connection more secure
//
// Author: McTwist (9845)
//

// Values required for a secure connection
//  algorithm
//  From server
//   realm
//   nonce
//   qop (optional)
//   opaque (optional)
//  From client
//   username
//   uri
//   method (optional)
//   nc (require qop)
//   cnounce (require qop)

// Required functionality
exec("./general.cs");
exec("./Support_ISAAC.cs");

// Hashing algorithms
exec("./Lib_SHA256.cs");

// Example
// // Create the digest
// %digest = DigestAccessAuthentication("McTwist", "/api/3/auth.php", sha1);
// // This is done each time you authenticate. It resets everything
// %digest.retrieveIdent(%fromServer);
// // This is done once
// %passHash = %digest.setPassword(%password);
// // This is done when reconnecting
// %digest.restorePassword(%passHash);
// // Set each time the uri and/or method is changed
// %digest.method = %method;
// %digest.uri = %uri;
// // Digest data to get it authenticated
// %json = %digest.digest(%data);

// Create a digest object
function DigestAccessAuthentication(%username, %uri, %algorithm)
{
	return new ScriptObject()
	{
		class = DigestAccessAuthentication;
		username = %username;
		uri = %uri;
		algorithm = %algorithm;
		method = "GET";
	};
}

// Prepared digest
function DigestAccessAuthentication::onAdd(%this)
{
	%this._sess = false;
}

// Apply the server response for first connection
function DigestAccessAuthentication::retrieveIdent(%this, %object)
{
	if (%object.value["realm"] $= "")
		return false;
	if (%object.value["nonce"] $= "")
		return false;
	%this._realm = %object.value["realm"]; // api.blocklandglass.com
	%this._nonce = %object.value["nonce"]; // Temporary connection
	%this._opaque = %object.value["opaque"]; // Session/ident

	%qop = %object.value["qop"]; // auth,auth-int
	while (%qop !$= "")
	{
		%qop = nextToken(%qop, "var", ",");
		// Prefer to auth content
		if (%var $= "auth-int")
			%this._qop = %var;
		else if (%var $= "auth" && %this._qop $= "")
			%this._qop = %var;
		// Make sure it's there
		%this._qop[%var] = true;
	}

	// Generate cnounce
	%this._cnonce = %this.generateRandom(8);
	%this._counter = 1;

	return true;
}

// Set and hash the password together with other variables
function DigestAccessAuthentication::setPassword(%this, %password)
{
	if (!%this.hasIdent())
		return "";

	// Check for session
	if (strstr(%this.algorithm, "-sess") >= 0)
	{
		%this._sess = true;
		%this.algorithm = getSubStr(%this.algorithm, strlen(%this.algorithm) - 5, 5);
	}
	// Set default algorithm
	if (!isFunction(%this.algorithm))
		%this.algorithm = sha1;

	// HA1
	%this._hash1 = %this._passhash = %this.hash(strlwr(%this.username), %this._realm, %password);
	// Apply session round if specified
	if (%this._sess)
		%this._hash1 = %this.hash(%this._hash1, %this._nonce, %this._cnonce);

	return %this._passhash;
}

// Restore previous saves password
function DigestAccessAuthentication::restorePassword(%this, %hash)
{
	if (!%this.hasIdent())
		return;

	// Check for session
	if (strstr(%this.algorithm, "-sess") >= 0)
	{
		%this._sess = true;
		%this.algorithm = getSubStr(%this.algorithm, strlen(%this.algorithm) - 5, 5);
	}
	// Set default algorithm
	if (!isFunction(%this.algorithm))
		%this.algorithm = sha1;

	%this._hash1 = %this._passhash = %hash;
	// Apply session round if specified
	if (%this._sess)
		%this._hash1 = %this.hash(%this._hash1, %this._nonce, %this._cnonce);
}

// Attach the action to a digest
function DigestAccessAuthentication::digest(%this, %data)
{
	if (!%this.hasIdent())
		return "";

	if (%this.username $= "" ||
		%this.uri $= "" ||
		%this._hash1 $= "")
		return "";

	// Pick qop depending on data
	if (isObject(%data))
	{
		if (%this._qop["auth-int"])
			%qop = "auth-int";
	}
	else
	{
		if (%this._qop["auth"])
			%qop = "auth";
	}
	// Pick default
	if (%qop $= "")
	{
		%qop = %this._qop;
	}

	%root = JettisonObject();

	// Username
	%root.set("username", "string", %this.username);

	// Realm
	%root.set("realm", "string", %this._realm);

	// Nonce
	%root.set("nonce", "string", %this._nonce);

	// URI
	%root.set("uri", "string", %this.uri);

	// QOP
	if (%qop !$= "")
		%root.set("qop", "string", %qop);

	// Algorithm
	%root.set("algorithm", "string", %this.algorithm @ (%this._sess ? "-sess" : ""));

	// HA2
	if (%qop $= "auth-int")
		%hash2 = %this.hash(%this.method, %this.uri, %data.toJSON());
	else
		%hash2 = %this.hash(%this.method, %this.uri);

	// Response
	if (%qop $= "auth" || %qop $= "auth-int")
	{
		// Nonce Counter
		%nc = %this._counter;
		%nc = strrepeat("0", 8 - strlen(%nc)) @ %nc;
		%root.set("nc", "string", %nc);

		%this._counter = safe_add(%this._counter, 1);
		// Note: Wont happen in a lifetime, but do it anyway
		if (%this._counter > 0x7FFFFFFE)
			%this._counter = 1;

		// Client Nonce
		%root.set("cnonce", "string", %this._cnonce);

		%response = %this.hash(%this._hash1, %this._nonce, %nc, %this._cnonce, %hash2);
	}
	else
	{
		%response = %this.hash(%this._hash1, %this._nonce, %hash2);
	}

	%root.set("response", "string", %response);

	// Opaque
	%root.set("opaque", "string", %this._opaque);

	// Data
	if (isObject(%data))
		%root.set("data", "object", %data);

	return %root;
}

// Check if ident is correct
function DigestAccessAuthentication::hasIdent(%this)
{
	return %this._realm !$= "" &&
		%this._nonce !$= "";
}

// Hash several values together
// These will be seperated with a delimiter
function DigestAccessAuthentication::hash(%this, %a0, %a1, %a2, %a3, %a4, %a5)
{
	// Set default algorithm
	if (!isFunction(%this.algorithm))
		%this.algorithm = sha1;

	%text = "";
	for (%i = 0; %i < 6; %i++)
		if (%a[%i] !$= "")
			%text = %text @ (%text $= "" ? "" : ":") @ %a[%i];

	return call(%this.algorithm, %text);
}

// Generate a random string of size %num
function DigestAccessAuthentication::generateRandom(%this, %num)
{
	%str = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
	%max = strlen(%str);
	%rand = "";
	for (%i = 0; %i < %num; %i++)
	{
		%rand = %rand @ getSubStr(%str, randc() % %max, 1);
	}
	return %rand;
}
