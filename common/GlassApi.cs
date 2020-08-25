function GlassApi::init() {
  new ScriptObject(GlassApi) {
    queued = 0;
    unauthorizedMax = 3;
  };

  GlassGroup.add(GlassApi);
}

function GlassApi::request(%this, %api, %parameters, %className, %authorized, %plaintext) {
  // we need to mimic to functionality of TCPClient
  %obj = new ScriptObject(GlassApiHandler) {
    className = %className;

    isQueued   = true;
    isRunning  = false;
    isFinished = false;

    _api        = %api;
    _paramaters = %parameters;
    _authorized = %authorized;
    _isJSON     = !%plaintext;
  };

  GlassGroup.add(GlassApiHandler);

  if(!%authorized) {
    %this.doCall(%obj);
    return %obj;
  }

  %this.queue[%this.queued] = %obj;
  %this.queued++;

  if(%this.idle)
    %this.nextInQueue();

  return %obj;
}

function GlassApi::nextInQueue(%this) {
  if(!%this.idle)
    return;

  if(%this.queued <= 0)
    return;

  %this.current = %this.queue[0];
  %this.current.isQueued = false;

  for(%i = 0; %i < %this.queued; %i++) {
    %this.queue[%i] = %this.queue[%i+1];
  }

  %this.queued--;

  %this.idle = false;
  %this.doCall(%this.current);
}

function GlassApi::doCall(%this, %obj) {
  %api        = %obj._api;
  %parameters = %obj._paramaters;
  %authorized = %obj._authorized;

  %obj.isRunning = true;

  if(%authorized && !GlassAuth.isAuthed) {
    %this.waitingForAuth = true;
    GlassAuth.reident();
    return;
  }

  if(strpos(%parameters, "?") == 0)
    %parameters = getSubStr(%parameters, 1, strlen(%parameters)-1);

  if(%authorized && GlassAuth.usingDAA) {
    // if we're using DAA, we need to send to daa.php and encapsulate

    // create object server will interpret
    %request = JettisonObject();
    %request.set("call", "string", %api);

    %data    = JettisonObject();
    %request.set("params", "object", %data);

    %parameters = strReplace(%parameters, "&", "\t");
    for(%i = 0; %i < getFieldCount(%parameters); %i++) {
      %param = getField(%parameters, %i);
      %param = strReplace(%param, "=", "\t");

      %key   = getField(%param, 0);
      %val   = getField(%param, 1);

      %data.set(%key, "string", %val);
    }

    %digest = GlassAuth.daa.digest(%request);
    %json   = jettisonStringify("object", %digest);

    %request.delete();
    %data.delete();
    %digest.delete();

    %tcp = TCPClient("POST", Glass.address, 80, "/api/3/daa.php?ident=" @ GlassAuth.daa_opaque, %json, "", "GlassApiTCP");
    %tcp.glassApiObj = %obj;
  } else {

    if(%authorized) {
      if(strlen(%parameters) > 0)
        %parameters = %parameters @ "&";

      %parameters = %parameters @ "ident=" @ GlassAuth.ident;
    }


    %url = "http://" @ Glass.address @ "/api/3/";
    %url = %url @ %api @ ".php?";
    %url = %url @ %parameters;

  	%method = "GET";
  	%downloadPath = "";
  	%className = "GlassApiTCP";

  	%tcp = connectToURL(%url, %method, %downloadPath, %className);
    %tcp.glassApiObj = %obj;
  }
}

function GlassApiTCP::onDone(%this, %error) {
  %obj = %this.glassApiObj;

  if(!%error) {

    //handle json parsing here as almost every Glass call is JSON
    // even if the call is set to not be JSON, we could still have an
    // "unauthorized" response
    if(strpos(%this.buffer, "{") == 0 && !jettisonParse(%this.buffer)) {

      %object = $JSON::Value;

      // the call is authorized, so we need to make sure there was no
      // unauthorization issues
      if(%obj._authorized && %object.status $= "unauthorized") {
        GlassLog::error("Glass API: Unauthorized!");

        GlassApi.unauthorizedCt++; // keep track of how many times we have retried this

        // there was an authorization issue. we will reident and try again
        if(%obj._authorized && GlassApi.unauthorizedCt < GlassApi.unauthorizedMax) {
          GlassApi.waitingForAuth = true;
          GlassAuth.reident();
          return;
        }
      } else {
        GlassApi.unauthorizedCt = 0;
        %obj.isFinished = true;
        %obj.isRunning  = false;

      	if(isFunction(%obj.className, "onDone"))
      		eval(%obj.className @ "::onDone(%obj, %error, %object);");

      }

      %object.schedule(0,delete);
    } else {
      if(%this._isJSON) {
        GlassLog::error("Glass API: Invalid response for \c1" @ %obj._api @ "\c0!");

        for(%i = 0; %i < getLineCount(%this.buffer); %i++) {
          GlassLog::debug(getLine(%this.buffer, %i));
        }
        // TODO log?
      }
    }
  }


  if(!isObject(%object))
  	if(isFunction(%obj.className, "onDone"))
  		eval(%obj.className @ "::onDone(%obj, %error);");

  GlassApi.idle = true;
  GlassApi.current = "";
  GlassApi.nextInQueue();

  %this.schedule(1000, delete);
}

function GlassApiTCP::onConnected(%this) {
  %obj = %this.glassApiObj;

  if(isFunction(%obj.className, "onConnected"))
		eval(%obj.className @ "::onConnected(%obj);");
}

function GlassApiTCP::buildRequest(%this, %request) {
  %obj = %this.glassApiObj;

  if(isFunction(%obj.className, "buildRequest"))
		return eval(%obj.className @ "::buildRequest(%obj, %request);");
	return %request;
}

function GlassApiTCP::onLine(%this, %line) {
  %obj = %this.glassApiObj;

	if(isFunction(%obj.className, "onLine"))
		eval(%obj.className @ "::onLine(%obj, %line);");
}

function GlassApiTCP::handleText(%this, %text) {
  %obj = %this.glassApiObj;

  if(%this.buffer $= "")
    %this.buffer = %text;
  else
    %this.buffer = %this.buffer NL %text;

	if(isFunction(%obj.className, "handleText"))
		eval(%obj.className @ "::handleText(%obj, %text);");
}

function GlassApiTCP::setProgressBar(%this, %completed) {
  %obj = %this.glassApiObj;

	if(isFunction(%obj.className, "setProgressBar"))
		eval(%obj.className @ "::setProgressBar(%obj, %completed);");
}

function GlassApiTCP::onBinChunk(%this, %chunk) {
  %obj = %this.glassApiObj;

  if(isFunction(%obj.className, "onBinChunk"))
		eval(%obj.className @ "::onBinChunk(%obj, %chunk);");
}

package GlassApi {
  function GlassAuth::onAuthSuccess(%this) {
    parent::onAuthSuccess(%this);

    if(GlassApi.waitingForAuth) {
      GlassApi.waitingForAuth = false;
      GlassApi.doCall(GlassApi.current);
    }
  }
};
activatePackage(GlassApi);
