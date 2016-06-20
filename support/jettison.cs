/// JSON type names and their respective values
///
///  * `"string"` - A TorqueScript string encoded as UTF-8
///  * `"number"`
///  * `"boolean"` - 1 (true) or 0 (false)
///  * `"null"` - Empty string
///  * `"object"` - Any TorqueScript object. Should implement `::toJSON()`
///    for string serialization. Use `.class` to disambiguate.

/// jettisonParse: (text: string) -> boolean
/// Deserialize the JSON string `text`.
///
/// On success, returns false, setting `$JSON::Value` and `$JSON::Type`.
///
/// On failure, returns true, setting `$JSON::Error` and `$JSON::Index`.
///
/// Example:
///
///     if (jettisonParse(%text)) {
///         error("JSON error (at " @ $JSON::Index @ "): " @ $JSON::Error);
///         return;
///     }
///
///     echo($JSON::Value @ ": " @ $JSON::Type);
function jettisonParse(%text) {
  %length = strlen(%text);

  if (__jettisonParse(%text, 0, %length)) {
    // Actual JSON error; pass it through.
    return true;
  }

  // Skip any whitespace at the end
  %index = $JSON::Index;

  while (strpos(" \t\r\n", getSubStr(%text, %index, 1)) != -1) {
    %index++;
  }

  // If we aren't at the end already, that means that %text consists of more
  // than a single JSON value, such as `"[1, 2, 3] foo"`.
  // Generate an error for this.
  if (%index < %length) {
    // Clean up the parsed value if necessary.
    if ($JSON::Type $= "object" && isObject($JSON::Value)) {
      $JSON::Value.delete();
    }

    $JSON::Error = "expected EOF";
    $JSON::Index = %index;

    return true;
  }

  // Everything went well!
  return false;
}

/// jettisonStringify: (type: string, value: *) -> string
/// Serialize an arbitrary value into a JSON string.
///
/// Always returns valid JSON, unless:
///  * `type` is not a valid JSON type - returns error string
///  * `type == "object"` but `value` does not implement `::toJSON()` -
///    missing function console error message and empty string return
function jettisonStringify(%type, %value) {
  switch$ (%type) {
    case "null":
      return "null";

    case "boolean":
      return %value ? "true" : "false";

    case "string":
      %length = strlen(%value);

      // Not using `expandEscape` here because its specifics differ from
      // the JSON specification. Most notably, it escapes ' and does not
      // escape backspace or linefeed to the right sequences.
      for (%i = 0; %i < %length; %i++) {
        %chr = getSubStr(%value, %i, 1);

        switch$ (%chr) {
          case   "\"": %out = %out @ "\\\"";
          case   "\\": %out = %out @ "\\\\";
          case "\x08": %out = %out @ "\\b";
          case "\x0C": %out = %out @ "\\f";
          case   "\n": %out = %out @ "\\n";
          case   "\r": %out = %out @ "\\r";
          case   "\t": %out = %out @ "\\t";
          // TODO: review other special characters in ASCII that would be
          // turned into Unicode escapes in a compliant implementation
          default    : %out = %out @ %chr;
        }
      }

      return "\"" @ %out @ "\"";

    case "number":
      // TODO: *Anything* correct here.
      return %value;

    case "object":
      return %value.toJSON();

    default:
      // This is a "silent error", but it's better than being loud in the
      // console every time something goes wrong. This is still invalid JSON,
      // so any attempt to read it will fail as intended. As such, you can
      // also check for this error by seeing if the first character is `<`.
      return "<unknown JSON type '" @ %type @ "'>";
  }
}

$JSONUtil::Digit["0"] = true;
$JSONUtil::Digit["1"] = true;
$JSONUtil::Digit["2"] = true;
$JSONUtil::Digit["3"] = true;
$JSONUtil::Digit["4"] = true;
$JSONUtil::Digit["5"] = true;
$JSONUtil::Digit["6"] = true;
$JSONUtil::Digit["7"] = true;
$JSONUtil::Digit["8"] = true;
$JSONUtil::Digit["9"] = true;

function __jettisonParse(%text, %index, %length) {
  // Skip any whitespace before what we're actually looking for.
  // Set %chr in the worst way possible here so we don't have to do an extra
  // string read and assignment after the loop.
  while (strpos(" \t\r\n", %chr = getSubStr(%text, %index, 1)) != -1) {
    %index++;
  }

  // This function being called means that we *need* a JSON value to be
  // parsed. If we can't do so, explicitly fail.
  if (%index >= %length) {
    $JSON::Error = "unexpected EOF";
    $JSON::Index = %index;

    return true;
  }

  switch$ (%chr) {
    case "\"":
      %start = %index;

      while (%index++ < %length) {
        %chr = getSubStr(%text, %index, 1);

        if (%chr $= "\"") {
          $JSON::Value = %out;
          $JSON::Type = "string";
          $JSON::Index = %index + 1;

          return false;
        }

        if (%chr $= "\\") {
          if (%index++ >= %length) {
            $JSON::Error = "unfinished string escape";
            $JSON::Index = %index - 1;

            return true;
          }

          // This `%index++ - 1` is gross. I wish TS had proper
          // unary assignment operators (in terms of what they evaluate to).
          switch$ (getSubStr(%text, %index, 1)) {
            case "\"": %out = %out @ "\"";
            case "\\": %out = %out @ "\\";
            case  "/": %out = %out @ "/";
            case  "b": %out = %out @ "\x08";
            case  "f": %out = %out @ "\x0C";
            case  "n": %out = %out @ "\n";
            case  "r": %out = %out @ "\r";
            case  "t": %out = %out @ "\t";
            case  "u":
              $JSON::Error = "TODO: unicode codepoint escape";
              $JSON::Index = %index - 1;

              return true;

            default:
              $JSON::Error = "invalid string escape";
              $JSON::Index = %index - 1;

              return true;
          }

          continue;
        }

        %out = %out @ %chr;
      }

      $JSON::Error = "unclosed string";
      $JSON::Index = %start;

      return true;

    case "{":
      //   Init (1): close -> END, item  -> Mid
      //    Mid (2): close -> END, comma -> Expect
      // Expect (3): item  -> Mid
      %state = 1;
      %start = %index;

      %index++;

      %object = new ScriptObject() {
        class = "JettisonObject";
        keyCount = 0;
      };

      while (true) {
        // Skip internal whitespace
        while (strpos(" \t\r\n", %chr = getSubStr(%text, %index, 1)) != -1) {
          %index++;
        }

        if (%index >= %length) {
          %object.delete();

          $JSON::Error = "unclosed object";
          $JSON::Index = %start;

          return true;
        }

        if (%chr $= "}") {
          if (%state != 3) {
            $JSON::Value = %object;
            $JSON::Type = "object";
            $JSON::Index = %index + 1;

            return false;
          } else {
            %object.delete();

            $JSON::Error = "trailing comma in object";
            $JSON::Index = %index;

            return true;
          }
        }

        if (%chr $= ",") {
          if (%state == 2) {
            %state = 3;
            %index++;

            continue;
          } else {
            %object.delete();

            $JSON::Error = %state == 3
              ? "double comma in object"
              : "starting comma in object";

            $JSON::Index = %index;
            return true;
          }
        }

        // First grab the object key
        if (%chr !$= "\"") {
          $JSON::Error = "object keys must be strings";
          $JSON::Index = %index;

          return true;
        }

        // TODO: This only needs to check for JSON strings. Consider moving
        // that into a specialized function.
        if (__jettisonParse(%text, %index, %length)) {
          %object.delete();
          return true;
        }

        if ($JSON::Type !$= "string") { // ???
          %object.delete();

          $JSON::Error = "mad world";
          $JSON::Index = %index;

          if ($JSON::Type $= "object") {
            $JSON::Value.delete();
          }

          return true;
        }

        %key = $JSON::Value;
        %index = $JSON::Index;

        // Now look for the : separating keys and values
        // Skip more internal whitespace
        while (strpos(" \t\r\n", %chr = getSubStr(%text, %index, 1)) != -1) {
          %index++;
        }

        if (%chr !$= ":") {
          %object.delete();

          $JSON::Error = "expected : after object key";
          $JSON::Index = %index;

          return true;
        }

        // Now we can grab the value
        // Skip *even* more internal whitespace
        while (strpos(" \t\r\n", %chr = getSubStr(%text, %index++, 1)) != -1) {}

        if (__jettisonParse(%text, %index, %length)) {
          %object.delete();
          return true;
        }

        %object.set(%key, $JSON::Type, $JSON::Value);

        %index = $JSON::Index;
        %state = 2;
      }

    case "[":
      //   Init (1): close -> END, item  -> Mid
      //    Mid (2): close -> END, comma -> Expect
      // Expect (3): item  -> Mid
      %state = 1;
      %start = %index;

      %index++;

      %array = new ScriptObject() {
        class = "JettisonArray";
        length = %arrayLength = 0;
      };

      while (true) {
        // Skip internal whitespace
        while (strpos(" \t\r\n", %chr = getSubStr(%text, %index, 1)) != -1) {
          %index++;
        }

        if (%index >= %length) {
          %array.delete();

          $JSON::Error = "unclosed array";
          $JSON::Index = %start;

          return true;
        }

        if (%chr $= "]") {
          if (%state != 3) {
            $JSON::Value = %array;
            $JSON::Type = "object";
            $JSON::Index = %index + 1;

            return false;
          } else {
            %array.delete();

            $JSON::Error = "trailing comma in array";
            $JSON::Index = %index;

            return true;
          }
        }

        if (%chr $= ",") {
          if (%state == 2) {
            %state = 3;
            %index++;

            continue;
          } else {
            %array.delete();

            $JSON::Error = %state == 3
              ? "double comma in array"
              : "starting comma in array";

            $JSON::Index = %index;
            return true;
          }
        }

        if (__jettisonParse(%text, %index, %length)) {
          %array.delete();
          return true;
        }

        %index = $JSON::Index;
        %state = 2;

        %array.type[%arrayLength] = $JSON::Type;
        %array.value[%arrayLength] = $JSON::Value;
        %arrayLength = %array.length++;
      }

    case "-" or
        "0" or "1" or "2" or "3" or "4" or
        "5" or "6" or "7" or "8" or "9":
      %start = %index;

      // TODO: Specification-compliant number parsing.
      // In general, this just means verifying that it's correct.
      // There's a prototype of that below.

      // if (%chr $= "-") {
      //   %sign = -1;
      //   %chr = getSubStr(%text, %index++, 1);
      // } else {
      //   %sign = 1;
      // }
      //
      // if (%chr $= "0") {
      //   %chr = getSubStr(%text, %index++, 1);
      // } else if ($JSONUtil::Digit[%chr]) {
      //   $JSON::Error = "TODO: parse decimal part";
      //   $JSON::Index = %index;
      //
      //   return true;
      // } else {
      //   $JSON::Error = "invalid number";
      //   $JSON::Index = %start;
      //
      //   return true;
      // }
      //
      // if (%chr $= ".") {
      //   $JSON::Error = "TODO: parse fractional part";
      //   $JSON::Index = %index;
      //
      //   return true;
      // }
      //
      // if (%chr $= "e") {
      //   $JSON::Error = "TODO: parse exponent part";
      //   $JSON::Index = %index;
      //
      //   return true;
      // }

      // In the mean time, let's just look for "generally valid characters",
      // and `getSubStr` them. In most cases this will be enough.
      while (strpos("0123456789.-eE+", getSubStr(%text, %index++, 1)) != -1) {}

      // This is cheap.
      $JSON::Value = getSubStr(%text, %start, %index - %start);
      $JSON::Type = "number";
      $JSON::Index = %index;

      return false;
  }

  // Case-sensitive multi character sequences
  if (strcmp("false", getSubStr(%text, %index, 5)) == 0) {
    $JSON::Value = false;
    $JSON::Type = "boolean";
    $JSON::Index = %index + 5;

    return false;
  }

  // TODO: Is it worth it to cache the 4 character prefix here?
  // It's reused for "null", but that's only a single time.
  if (strcmp("true", getSubStr(%text, %index, 4)) == 0) {
    $JSON::Value = true;
    $JSON::Type = "boolean";
    $JSON::Index = %index + 4;

    return false;
  }

  if (strcmp("null", getSubStr(%text, %index, 4)) == 0) {
    $JSON::Value = "";
    $JSON::Type = "null";
    $JSON::Index = %index + 4;

    return false;
  }

  // There's nothing else this could possibly be. Give up!
  $JSON::Error = "unknown token";
  $JSON::Index = %index;

  return true;
}

// --------------------------------------------------------
// Field access helper for JettisonObject

function SimObject::getField(%this, %name) {
  switch (stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%name, 0, 1))) {
    case  0: return %this._[getSubStr(%name, 1, strlen(%name))];
    case  1: return %this.a[getSubStr(%name, 1, strlen(%name))];
    case  2: return %this.b[getSubStr(%name, 1, strlen(%name))];
    case  3: return %this.c[getSubStr(%name, 1, strlen(%name))];
    case  4: return %this.d[getSubStr(%name, 1, strlen(%name))];
    case  5: return %this.e[getSubStr(%name, 1, strlen(%name))];
    case  6: return %this.f[getSubStr(%name, 1, strlen(%name))];
    case  7: return %this.g[getSubStr(%name, 1, strlen(%name))];
    case  8: return %this.h[getSubStr(%name, 1, strlen(%name))];
    case  9: return %this.i[getSubStr(%name, 1, strlen(%name))];
    case 10: return %this.j[getSubStr(%name, 1, strlen(%name))];
    case 11: return %this.k[getSubStr(%name, 1, strlen(%name))];
    case 12: return %this.l[getSubStr(%name, 1, strlen(%name))];
    case 13: return %this.m[getSubStr(%name, 1, strlen(%name))];
    case 14: return %this.n[getSubStr(%name, 1, strlen(%name))];
    case 15: return %this.o[getSubStr(%name, 1, strlen(%name))];
    case 16: return %this.p[getSubStr(%name, 1, strlen(%name))];
    case 17: return %this.q[getSubStr(%name, 1, strlen(%name))];
    case 18: return %this.r[getSubStr(%name, 1, strlen(%name))];
    case 19: return %this.s[getSubStr(%name, 1, strlen(%name))];
    case 20: return %this.t[getSubStr(%name, 1, strlen(%name))];
    case 21: return %this.u[getSubStr(%name, 1, strlen(%name))];
    case 22: return %this.v[getSubStr(%name, 1, strlen(%name))];
    case 23: return %this.w[getSubStr(%name, 1, strlen(%name))];
    case 24: return %this.x[getSubStr(%name, 1, strlen(%name))];
    case 25: return %this.y[getSubStr(%name, 1, strlen(%name))];
    case 26: return %this.z[getSubStr(%name, 1, strlen(%name))];
    default: return "";
  }
}

function SimObject::setField(%this, %name, %value) {
  switch (stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%name, 0, 1))) {
    case  0: return %this._[getSubStr(%name, 1, strlen(%name))] = %value;
    case  1: return %this.a[getSubStr(%name, 1, strlen(%name))] = %value;
    case  2: return %this.b[getSubStr(%name, 1, strlen(%name))] = %value;
    case  3: return %this.c[getSubStr(%name, 1, strlen(%name))] = %value;
    case  4: return %this.d[getSubStr(%name, 1, strlen(%name))] = %value;
    case  5: return %this.e[getSubStr(%name, 1, strlen(%name))] = %value;
    case  6: return %this.f[getSubStr(%name, 1, strlen(%name))] = %value;
    case  7: return %this.g[getSubStr(%name, 1, strlen(%name))] = %value;
    case  8: return %this.h[getSubStr(%name, 1, strlen(%name))] = %value;
    case  9: return %this.i[getSubStr(%name, 1, strlen(%name))] = %value;
    case 10: return %this.j[getSubStr(%name, 1, strlen(%name))] = %value;
    case 11: return %this.k[getSubStr(%name, 1, strlen(%name))] = %value;
    case 12: return %this.l[getSubStr(%name, 1, strlen(%name))] = %value;
    case 13: return %this.m[getSubStr(%name, 1, strlen(%name))] = %value;
    case 14: return %this.n[getSubStr(%name, 1, strlen(%name))] = %value;
    case 15: return %this.o[getSubStr(%name, 1, strlen(%name))] = %value;
    case 16: return %this.p[getSubStr(%name, 1, strlen(%name))] = %value;
    case 17: return %this.q[getSubStr(%name, 1, strlen(%name))] = %value;
    case 18: return %this.r[getSubStr(%name, 1, strlen(%name))] = %value;
    case 19: return %this.s[getSubStr(%name, 1, strlen(%name))] = %value;
    case 20: return %this.t[getSubStr(%name, 1, strlen(%name))] = %value;
    case 21: return %this.u[getSubStr(%name, 1, strlen(%name))] = %value;
    case 22: return %this.v[getSubStr(%name, 1, strlen(%name))] = %value;
    case 23: return %this.w[getSubStr(%name, 1, strlen(%name))] = %value;
    case 24: return %this.x[getSubStr(%name, 1, strlen(%name))] = %value;
    case 25: return %this.y[getSubStr(%name, 1, strlen(%name))] = %value;
    case 26: return %this.z[getSubStr(%name, 1, strlen(%name))] = %value;
    default: return %value;
  }
}

// --------------------------------------------------------
// JettisonObject implementation

$JettisonObject::IllegalName["class"] = true;
$JettisonObject::IllegalName["superClass"] = true;
$JettisonObject::IllegalName["keyCount"] = true;

function JettisonObject() {
  return new ScriptObject() {
    class = "JettisonObject";
    keyCount = 0;
  };
}

function JettisonObject::onRemove(%this) {
  for (%i = 0; %i < %this.keyCount; %i++) {
    %key = %this.keyName[%i];
    %value = %this.value[%key];

    if (isObject(%value) && %this.type[%key] $= "object") {
      %value.delete();
    }
  }
}

function JettisonObject::toJSON(%this) {
  for (%i = 0; %i < %this.keyCount; %i++) {
    %key = %this.keyName[%i];

    if (%i) { // sacrificing DRY for performance, woo
      %out = %out @
        "," @ jettisonStringify("string", %key) @
        ":" @ jettisonStringify(%this.type[%key], %this.value[%key]);
    } else {
      %out = %out @
              jettisonStringify("string", %key) @
        ":" @ jettisonStringify(%this.type[%key], %this.value[%key]);
    }
  }

  return "{" @ %out @ "}";
}

function JettisonObject::set(%this, %key, %type, %value) {
  if (!%this.keyExists[%key]) {
    // TODO: Review how slow this is.
    // Being able to access keys as fields is awesome, though.
    %this.keyLegal[%key] = !$JettisonObject::IllegalName[%key] &&
      (strlen(%key) <= 4 || getSubStr(%key, 0, 4) !$= "type"     ) &&
      (strlen(%key) <= 5 || getSubStr(%key, 0, 5) !$= "value"    ) &&
      (strlen(%key) <= 7 || getSubStr(%key, 0, 7) !$= "keyName"  ) &&
      (strlen(%key) <= 8 || getSubStr(%key, 0, 8) !$= "keyLegal" ) &&
      (strlen(%key) <= 9 || getSubStr(%key, 0, 9) !$= "keyExists");

    %this.keyExists[%key] = true;
    %this.keyName[%this.keyCount] = %key;
    %this.keyCount++;
  }

  %this.type[%key] = %type;
  %this.value[%key] = %value;

  if (%this.keyLegal[%key]) {
    %this.setField(%key, %value);
  }
}

function JettisonObject::remove(%this, %key) {
	// TODO: Cache key indices for rapid key removal.

	for (%i = 0; %i < %this.keyCount; %i++) {
		if (%this.keyName[%i] $= %key) {
			break;
		}
	}

	if (%i >= %this.keyCount) {
		return false;
	}

	if (%this.keyLegal[%key]) {
		%this.setField(%key, "");
	}

	%this.type[%key] = "";
	%this.value[%key] = "";

	%this.keyExists[%key] = "";
	%this.keyLegal[%key] = "";

	%this.keyName[%i] = %this.keyName[%this.keyCount--];
	%this.keyName[%this.keyCount] = "";

	return true;
}

// --------------------------------------------------------
// JettisonArray implementation

function JettisonArray() {
  return new ScriptObject() {
    class = "JettisonArray";
    length = 0;
  };
}

function JettisonArray::onRemove(%this) {
  for (%i = 0; %i < %this.length; %i++) {
    %value = %this.value[%i];

    if (isObject(%value) && %this.type[%i] $= "object") {
      %value.delete();
    }
  }
}

function JettisonArray::toJSON(%this) {
  for (%i = 0; %i < %this.length; %i++) {
    if (%i) {
      %out = %out @ "," @ jettisonStringify(%this.type[%i], %this.value[%i]);
    } else {
      %out = %out       @ jettisonStringify(%this.type[%i], %this.value[%i]);
    }
  }

  return "[" @ %out @ "]";
}

function JettisonArray::push(%this, %type, %value) {
  %this.type[%this.length] = %type;
  %this.value[%this.length] = %value;
  %this.length++;
}

// ...

// function testJSON(%text) {
//   if (jettisonParse(%text)) {
//     warn("ERROR: " @ $JSON::Index @ ": " @ $JSON::Error);
//     return;
//   }
//
//   echo($JSON::Type @ ": " @ $JSON::Value);
//
//   // Handle buffer overruns
//   %string = " -> " @ jettisonStringify($JSON::Type, $JSON::Value);
//   echo(getSubStr(%string, 0, 1023));
//
//   if ($JSON::Type $= "object") {
//     $JSON::Value.delete();
//   }
// }

// File I/O helpers
function jettisonReadFile(%filename) {
  %file = new FileObject();

  if (!%file.openForRead(%filename)) {
    %file.delete();

    $JSON::Error = "failed to open file for reading";
    $JSON::Index = "";

    return true;
  }

  while (!%file.isEOF()) {
    %text = %text @ %file.readLine();
  }

  %file.close();
  %file.delete();

  return jettisonParse(%text);
}

function jettisonWriteFile(%filename, %type, %value) {
  %file = new FileObject();

  if (!%file.openForWrite(%filename)) {
    return "failed to open file for reading";
  }

  %file.writeLine(jettisonStringify(%type, %value));

  %file.close();
  %file.delete();

  return "";
}
