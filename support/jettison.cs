// anything parseJSON(string blob)
//  Parses *blob*, a serialized JSON type, and returns a fitting TS representation.
//
//  This does not leak any objects internally, however the output will be an object for certain types.
//  In this case, cleaning up (deleting the object) properly is up to the user.
//
//  Returns a string, number, JSONObject subclass or "" when an error occured.
//
//  Supported types and translations:
//
//   * `hash` -> `JSONHash()`
//   * `array` -> `JSONArray()`
//   * `string` -> native string
//   * `number` -> number in native string
//   * `bool` -> "1" (true) or "0" (false)
//   * `null` -> ""
//
//  Example:
//
//      %blob = "...";
//      %json = parseJSON(%blob);
//      
//      if (%json $= "") {
//        error("ERROR: Failed to parse JSON.");
//        return;
//      }
//      
//      // work with %json
//      
//      if (isJSONObject(%json)) {
//        %json.delete(); // note: deletes any children
//      }

function parseJSON(%blob) {
	return restWords(__scanJSON(%blob, 0));
}

// string getJSONType(data)
//  Analyzes *data* and returns a string description of it's JSON type.
//
//  Possible return values:
//
//   * `"null"` (when *data* is an empty string)
//   * `"string"`
//   * `"number"`
//   * `"array"`
//   * `"hash"`
//
//  Keep in mind:
//
//   * Strings containing valid JSON numbers are interpreted as numbers.
//   * Booleans are lost in translation.
//
//  Example:
//
//      %json = parseJSON("[]");
//      echo(getJSONType(%json)); // array

function getJSONType(%data) {
	if (%data $= "") {
		return "null";
	}

	%length = strLen(%data);

	if (expandEscape(getSubStr(%data, %length - 1, 1)) $= "\\c0") {
		%obj = getSubStr(%data, 0, %length - 1);

		if (%obj.superClass $= JSONObject) {
			return %obj.getJSONType();
		}
	}

	%scan = __scanJSONNumber(%data, 0);

	if (%scan !$= "" && firstWord(%scan) == %length) {
		return "number";
	}

	return "string";
}

// bool isJSONObject(data)
//  Correctly determines if *data* is a reference to a JSON object (hash/array) and returns true or false.
//  Use this instead of `isObject(data)`, as this will not accidentally confuse numbers for existing objects.

function isJSONObject(%data) {
	%length = strLen(%data);

	if (%data !$= "" && expandEscape(getSubStr(%data, %length - 1, 1)) $= "\\c0") {
		return getSubStr(%data, 0, %length - 1).superClass $= JSONObject;
	}

	return 0;
}

// string dumpJSON(data)
//  Recursively builds a JSON string of *data* (as parsed by `parseJSON`) and returns it.
//
//  Keep in mind that `dumpJSON(parseJSON(x))` is not necessarily `x`.
//  See notes for `getJSONType` for further details.

function dumpJSON(%data)
{
	%type = getJSONType(%data);
	switch$(%type)
	{
	case "hash":
		%str = "{";
		for(%i = 0; %i < %data.__length; %i++)
		{
			%key = %data.__key[%i];
			%val = %data.__value[%key];
			%str = %str @ "\"" @ %key @ "\":" @ dumpJSON(%val);
			if(%i < %data.__length - 1)
				%str = %str @ ",";
		}
		return %str @ "}";
	case "array":
		%str = "[";
		for(%i = 0; %i < %data.length; %i++)
		{
			%val = %data.item[%i];
			%str = %str @ dumpJSON(%val);
			if(%i < %data.length - 1)
				%str = %str @ ",";
		}
		return %str @ "]";
	case "number":
		return %data;
	case "string":
		%data = expandEscape(expandEscape(%data));
		%data = strReplace(%data, "\\\\'", "'");
		%data = collapseEscape(%data);

		return "\"" @ %data @ "\"";
	}
	return "null";
}

// string describeJSON(* data, [int depth])
//  Returns a string representing *data*, using `JSONObject::describe` for JSON objects.

function describeJSON(%data, %depth) {
	if (!isJSONObject(%data)) {
		return %data;
	}

	return %data.describe(%depth);
}

// bool saveJSON(data, string filename, [FileObject fo])
//  Saves the given JSON object *data* into a file at *filename*.
//  If saving and loading a lot of files, you may pass your own FileObject through *fo* rather than recreating a lot of them.
//
//  Returns true on a successful write, false otherwise.

function saveJSON(%data, %filename, %fo)
{
	%success = 0;
	%createdFO = 0;
	if(!isObject(%fo) || %fo.getClassName() !$= "FileObject")
	{
		%fo = new FileObject();
		%createdFO = 1;
	}

	%str = dumpJSON(%data);
	if(%str !$= "null" && %str !$= "" && %fo.openForWrite(%filename))
	{
		%fo.writeLine(%str);
		%fo.close();
		%success = 1;
	}
	if(%createdFO)
		%fo.delete();
	if(!isFile(%filename))
		%success = 0;
	return %success;
}

// anything loadJSON(string filename, [string defaultStruct], [FileObject fo])
//  Loads a JSON object from a given file.
//  If the file does not exist, `defaultStruct` is parsed and returned.
//
//  If saving and loading a lot of files, you may pass your own FileObject through *fo* rather than recreating a lot of them.
//  If using your own FileObject with no *defaultStruct*, please provide "" as *defaultStruct*.

function loadJSON(%filename, %default, %fo)
{
	%createdFO = 0;
	if(!isObject(%fo) || %fo.getClassName() !$= "FileObject")
	{
		%fo = new FileObject();
		%createdFO = 1;
	}

	if(isFile(%filename) && %fo.openForRead(%filename))
	{
		%str = "";
		while(!%fo.isEOF())
		{
			%str = %str @ %fo.readLine();
		}
		%fo.close();
		%data = parseJSON(%str);
	}
	if(%data $= "")
		%data = parseJSON(%default);
	if(%createdFO)
		%fo.delete();
	return %data;
}

// Private functions

function __scanJSON(%blob, %index, %type) {
	%index = skipLeftSpace(%blob, %index);

	if (%index >= strLen(%blob)) {
		return "";
	}

	if (getSubStr(%blob, %index, 4) $= "null") {
		return %index + 4 SPC "";
	}

	if (getSubStr(%blob, %index, 4) $= "true") {
		return %index + 4 SPC 1;
	}

	if (getSubStr(%blob, %index, 5) $= "false") {
		return %index + 5 SPC 0;
	}

	%char = getSubStr(%blob, %index, 1);

	if (%char $= "\"") {
		return __scanJSONString(%blob, %index + 1);
	}

	if (%char $= "[") {
		return __scanJSONArray(%blob, %index + 1);
	}

	if (%char $= "{") {
		return __scanJSONHash(%blob, %index + 1);
	}

	return __scanJSONNumber(%blob, %index);
}

function __scanJSONString(%blob, %index) {
	%length = strLen(%blob);

	for (%i = %index; %i < %length; %i++) {
		if (getSubStr(%blob, %i, 1) $= "\"" && getSubStr(%blob, %i - 1, 1) !$= "\\") {
			return %i + 1 SPC collapseEscape(getSubStr(%blob, %index, %i - %index));
		}
	}

	return "";
}

function __scanJSONArray(%blob, %index) {
	%length = strLen(%blob);

	%obj = new ScriptObject() {
		class = JSONArray;
		superClass = JSONObject;
	};

	%first = 0;
	%ready = 1;

	while (1) {
		%index = skipLeftSpace(%blob, %index);

		if (%index >= %length) {
			%obj.delete();
			return "";
		}

		if (getSubStr(%blob, %index, 1) $= "]") {
			if (%first && %ready) {
				%obj.delete();
				return "";
			}

			return %index + 1 SPC %obj @ "\c0";
		}

		if (getSubStr(%blob, %index, 1) $= ",") {
			if (%ready) {
				return "";
			}

			%ready = 1;
			%index++;

			continue;
		}
		else if (!%ready) {
			return "";
		}

		%scan = __scanJSON(%blob, %index);

		if (%scan $= "") {
			%obj.delete();
			return "";
		}

		%index = firstWord(%scan);
		%obj.append(restWords(%scan));

		%first = 1;
		%ready = 0;
	}

	return "";
}

function __scanJSONHash(%blob, %index) {
	%length = strLen(%blob);

	%obj = new ScriptObject() {
		class = JSONHash;
		superClass = JSONObject;
	};

	%first = 0;
	%ready = 1;

	while (1) {
		%index = skipLeftSpace(%blob, %index);

		if (%index >= %length) {
			%obj.delete();
			return "";
		}

		if (getSubStr(%blob, %index, 1) $= "}") {
			if (%first && %ready) {
				%obj.delete();
				return "";
			}

			return %index + 1 SPC %obj @ "\c0";
		}

		if (getSubStr(%blob, %index, 1) $= ",") {
			if (%ready) {
				return "";
			}

			%ready = 1;
			%index++;

			continue;
		}
		else if (!%ready) {
			return "";
		}

		if (getSubStr(%blob, %index, 1) !$= "\"") {
			%obj.delete();
			return "";
		}

		%scan = __scanJSONString(%blob, %index + 1);

		if (%scan $= "") {
			%obj.delete();
			return "";
		}

		%key = restWords(%scan);
		%index = skipLeftSpace(%blob, firstWord(%scan));

		if (getSubStr(%blob, %index, 1) !$= ":") {
			%obj.delete();
			return "";
		}

		%scan = __scanJSON(%blob, %index + 1);

		if (%scan $= "") {
			%obj.delete();
			return "";
		}

		%obj.set(%key, restWords(%scan));
		%index = firstWord(%scan);

		%first = 1;
		%ready = 0;
	}

	return "";
}

function __scanJSONNumber(%blob, %index) {
	%length = strLen(%blob);
	%i = %index;

	if (getSubStr(%blob, %index, 1) $= "-") {
		%i++;
	}

	%allowZeroFirst = 0;
	%allowRadixFirst = 0;

	%first = 0;
	%radix = 0;

	if (!%allowZeroFirst) {
		%start = getSubStr(%blob, %i, 1);
	}

	for (%i; %i < %length; %i++) {
		%chr = getSubStr(%blob, %i, 1);

		if (%chr $= ".") {
			if ((!%allowRadixFirst && !%first) || %radix) {
				return "";
			}
			else {
				%first = 0;
				%radix = 1;
			}
		}
		else {
			%pos = strPos("0123456789", %chr);

			if (%pos == -1) {
				break;
			}

			if (!%first || !%radix) {
				%first++;
			}
		}
	}

	if (!%first) {
		return "";
	}

	if (!%allowZeroFirst && (%radix ? %first : %i - %index) > 1 && %start $= "0") {
		return "";
	}

	return %i SPC getSubStr(%blob, %index, %i - %index);
}

// JSONObject JSONObject
//  This represents a base that all other JSON objects inherit from,
//  and thus these methods apply to all of them.

//  @extends ScriptObject
//  @abstract

function JSONObject() {}

// string JSONObject::getJSONType()
//  Determines the type of the JSON object and returns a string such as "hash" or "array".
//  Custom JSON objects that do not start with "JSON" will break this.

function JSONObject::getJSONType(%this) {
	return strLwr(getSubStr(%this.class, 4, strLen(%this.class)));
}

// string JSONObject::describe([int depth])
//  @see describeJSON

function JSONObject::describe(%this, %depth) {
	return %this.getJSONType() @ "(" @ %this.getID() @ ")";
}

// JSONObject::addParent(->JSONObject parent)
//  Adds a JSON object to another's list of parents, setting it as the main parent if it's the first.

function JSONObject::addParent(%this, %parent) {
	if (%this.parent $= "") {
		%this.parent = %parent;
	}

	if (%this.parents $= "") {
		%this.parents = %parent;
	}
	else {
		%this.parents = %this.parents SPC %parent;
	}
}

// JSONObject::removeParent(->JSONObject parent)
//  Not implemented.

function JSONObject::removeParent(%this, %parent)
{
}

// JSONArray JSONArray
//  A JSON list object, such as `["hello", 3.14]`.
//
//  Items are stored as `array.item[i]`, where `0 <= i < array.length`.
//
//  Iteration example:
//
//      for (%i = 0; %i < %array.length; %i++)
//      {
//        echo(%array.item[%i]);
//      }
//
//  @extends JSONObject
//  @abstract

function JSONArray() {}

function JSONArray::onAdd(%this) {
	%this.length = 0;
}

function JSONArray::onRemove(%this) {
	for (%i = 0; %i < %this.length; %i++) {
		if (isJSONObject(%this.item[%i])) {
			%this.item[%i].delete();
		}
	}
}

// number JSONArray::getLength
//  Returns the number of items in the array.

function JSONArray::getLength(%this) {
	return %this.length;
}

// string JSONArray::describe([int depth])
//  @see describeJSON

function JSONArray::describe(%this, %depth) {
	%string = Parent::describe(%this, %depth);
	%string = %string @ " - " @ %this.length @ " items";

	%indent = repeatString("   ", %depth++);

	for (%i = 0; %i < %this.length; %i++) {
		%string = %string NL %indent @ %i @ ": " @ describeJSON(%this.item[%i], %depth);
	}

	return %string;
}

// anything JSONArray::get(int index, [default])
//  Returns the item at *index*, or *default* ("" if unspecified) if it is out of range.

function JSONArray::get(%this, %index, %default) {
	if (%index < 0 || %index >= %this.length) {
		return %default;
	}

	return %this.item[%index];
}

// JSONArray JSONArray::append(item)
//  Appends *item* to the end of the array, incrementing the length.

function JSONArray::append(%this, %item) {
	if (isJSONObject(%item)) {
		%item.addParent(%this);
	}

	%this.item[%this.length] = %item;
	%this.length++;

	return %this;
}

// JSONArray JSONArray::prepend(item)
//  Prepends *item* to the beginning of the array, incrementing the length and shifting all items up by one.

function JSONArray::prepend(%this, %item) {
	if (isJSONObject(%item)) {
		%item.addParent(%this);
	}

	%this.length++;

	for (%i = %this.length - 1; %i > 0; %i--) {
		%this.item[%i] = %this.item[%i - 1];
	}

	%this.item[0] = %item;
	return %this;
}

// bool JSONArray::contains(item)
//  Returns true if *item* is in the array, false otherwise.
//  When checking strings, case is ignored.

function JSONArray::contains(%this, %item) {
	for (%i = 0; %i < %this.length; %i++) {
		if (%this.item[%i] $= %item) {
			return 1;
		}
	}

	return 0;
}

// bool JSONArray::remove(item, [int max], [bool noDelete])
//  Removes *item* from the array if it is present, returning true if it was, false otherwise.
//
//  If *item* is present multiple times, all occurences are removed (TODO: implement *max* functionality).

function JSONArray::remove(%this, %item, %max, %noDelete) {
	if (%max $= "") {
		%max = 1;
	}

	%found = 0;

	for (%i = 0; %i < %this.length; %i++) {
		if (%this.item[%i] $= %item) {
			if (isJSONObject(%this.item[%i])) {
				if (!%noDelete) {
					%this.item[%i].delete();
				}
				else {
					%this.item[%i].removeParent(%this);
				}
			}

			%found++;
		}

		if (%found) {
			%this.item[%i] = %this.item[%i + 1];
		}
	}

	if (%found) {
		%this.length -= %found;

		for (%i = 0; %i < %found; %i++) {
			%this.item[%this.length + %i] = "";
		}
	}

	return %found;
}

// JSONArray JSONArray::clear([bool noDelete])
//  Removes all items from the array.

function JSONArray::clear(%this, %noDelete) {
	for (%i = 0; %i < %this.length; %i++) {
		if (isJSONObject(%this.item[%i])) {
			if (!%noDelete) {
				%this.item[%i].delete();
			}
			else {
				%this.item[%i].removeParent(%this);
			}
		}

		%this.item[%i] = "";
	}

	%this.length = 0;
	return %this;
}

// JSONHash JSONHash
//  A JSON hash object, such as `{"hello": 3.14, "foo": "bar"}`.
//
//  Keys which are valid TorqueScript identifiers can be accessed as `hash.keyname` directly.
//  All keys (including invalid TorqueScript identifiers) can be accessed using `::get`.
//
//  The number of keys is stored as `hash.length`, unless a `length` key is present.
//  Use `::getLength` for consistency in this case.
//
//  Iteration example:
//
//      for (%i = 0; %i < %hash.getLength(); %i++)
//      {
//        %key = %hash.getKey(%i);
//        echo(%key SPC "=" SPC %hash.get(%key));
//      }
//
//  @extends JSONObject
//  @abstract

function JSONHash() {}

function JSONHash::onAdd(%this) {
	%this.__length = 0;
	%this.length = 0;
}

function JSONHash::onRemove(%this) {
	for (%i = 0; %i < %this.__length; %i++) {
		%item = %this.__value[%this.__key[%i]];

		if (isJSONObject(%item)) {
			%item.delete();
		}
	}
}

// int JSONHash::getLength
//  Returns the number of key->value mappings in the hash.
//  This will work correctly even if a *length* key is present.

function JSONHash::getLength(%this) {
	return %this.__length;
}

// string JSONHash::getKey(int index)
//  Returns the *index*th key name in the hash, "" if out of range.

function JSONHash::getKey(%this, %index) {
	if (%index < 0 || %index >= %this.__length) {
		return %this.__key[%index];
	}

	return "";
}

// bool JSONHash::isKey(string key)
//  Returns true if *key* is present in the hash, false otherwise.

function JSONHash::isKey(%this, %key) {
	return %this.__isKey[%key] ? 1 : 0;
}

// bool JSONHash::isValue(value)
//  Returns true if any of the keys in the hash are set to *value*, false otherwise.

function JSONHash::isValue(%this, %value) {
	for (%i = 0; %i < %this.__length; %i++) {
		if (%this.__value[%this.__key[%i]] $= %value) {
			return 1;
		}
	}

	return 0;
}

// string JSONHash::describe([int depth])
//  @see describeJSON

function JSONHash::describe(%this, %depth) {
	%string = Parent::describe(%this, %depth);
	%string = %string @ " - " @ %this.__length @ " pairs";

	%indent = repeatString("   ", %depth++);

	for (%i = 0; %i < %this.__length; %i++) {
		%key = %this.__key[%i];
		%value = %this.__value[%key];

		%string = %string NL %indent @ %key @ ": " @ describeJSON(%value, %depth);
	}

	return %string;
}

// JSONHash JSONHash::set(string key, value)
//  Sets the value of *key* to *value*,
//  and adds *key* to the hash if it is not present.

function JSONHash::set(%this, %key, %value) {
	if (isJSONObject(%value)) {
		%value.addParent(%this);
	}

	if (%key $= "length") {
		%this.__usePublicLength = 0;
	}

	if (!%this.__isKey[%key]) {
		%this.__isKey[%key] = 1;

		%this.__key[%this.__length] = %key;
		%this.__length++;

		if (!%this.__isKey["length"]) {
			%this.length = %this.__length;
		}
	}

	%this.__value[%key] = %value;

	if (%this.__isKeyNameSane[%key] $= "") {
		%illegal = "class superClass";

		if (striPos(" " @ %illegal @ " ", " " @ %key @ " ") != -1) {
			%this.__isKeyNameSane[%key] = 0;
		}
		else {
			%this.__isKeyNameSane[%key] = sanitizeIdentifier(%key);
		}
	}

	if (%this.__isKeyNameSane[%key]) {
		eval("%this." @ %key @ "=%value;");
	}

	return %this;
}

// anything JSONHash::setDefault(string key, value)
//  Sets the value of *key* to *value* if the key is not present in the hash.
//  Returns the resulting value of *key*, even if it was previously defined.

function JSONHash::setDefault(%this, %key, %value) {
	if (!%this.__isKey[%key]) {
		%this.set(%key, %value);
	}

	return %this.__value[%key];
}

// anything JSONHash::get(string key, [default])
//  Returns the value assigned to *key* if it is present, *default* ("" if unspecified) otherwise.

function JSONHash::get(%this, %key, %default) {
	if (!%this.__isKey[%key]) {
		return %default;
	}

	return %this.__value[%key];
}

// bool JSONHash::remove(string key, [bool noDelete])
//  Removes *key* and it's assigned value from the hash if present.
//  Returns whether or not *key* was in the hash previously.

function JSONHash::remove(%this, %key, %noDelete) {
	if (!%this.__isKey[%key]) {
		return 0;
	}

	if (isJSONObject(%this.__value[%key])) {
		if (!%noDelete) {
			%this.__value[%key].delete();
		}
		else {
			%this.__value[%key].removeParent(%this);
		}
	}

	%this.__isKey[%key] = "";
	%this.__value[%key] = "";

	if (%this.__isKeyNameSane[%key]) {
		eval("%this." @ %key @ "=\"\";");
	}

	%this.__isKeyNameSane[%key] = "";

	for (%i = 0; %i < %this.__length; %i++) {
		if (%this.__key[%i] $= %key) {
			%found++;
		}

		if (%found) {
			%this.__key[%i] = %this.__key[%i + 1];
		}
	}

	if (%found) {
		%this.__length -= %found;

		for (%i = 0; %i < %found; %i++) {
			%this.__key[%this.__length + %i] = "";
		}
	}

	return 1;
}

// JSONHash JSONHash::clear([bool noDelete])
//  Removes all key->value mappings from the hash.

function JSONHash::clear(%this, %noDelete) {
	for (%i = 0; %i < %this.__length; %i++) {
		%key = %this.__key[%i];

		if (isJSONObject(%this.__value[%key])) {
			if (!%noDelete) {
				%this.__value[%key].delete();
			}
			else {
				%this.__value[%key].removeParent(%this);
			}
		}

		%this.__isKey[%key] = "";
		%this.__value[%key] = "";

		if (%this.__isKeyNameSane[%key]) {
			eval("%this." @ %key @ "=\"\";");
		}

		%this.__isKeyNameSane[%key] = "";
		%this.__key[%i] = "";

		%this.item[%i] = "";
	}

	%this.__length = 0;
	%this.length = 0;

	return %this;
}

// skipLeftSpace
//  @private

function skipLeftSpace(%blob, %index) {
	%length = strLen(%blob);

	if (%index >= %length) {
		return %index;
	}

	return %index + (%length - %index - strLen(ltrim(getSubStr(%blob, %index, %length))));
}

// sanitizeIdentifier
//  @private

function sanitizeIdentifier(%blob) {
	%a = "_abcdefghijklmnopqrstuvwxyz";
	%b = "0123456789";

	%length = strLen(%blob);

	for (%i = 0; %i < %length; %i++) {
		%chr = getSubStr(%blob, %i, 1);

		if (striPos(%a, %chr) == -1) {
			if (!%i || striPos(%b, %chr) == -1) {
				return 0;
			}
		}
	}

	return 1;
}

// repeatString
//  @private

function repeatString(%string, %times) {
	for (%i = 0; %i < %times; %i++) {
		%result = %result @ %string;
	}

	return %result;
}

// echoPointInBlob
//  @private

function echoPointInBlob(%blob, %index) {
	echo(%blob NL repeatString(" ", %index) @ "^");
}