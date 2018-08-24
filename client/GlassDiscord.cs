function GlassDiscord::getCode() {
  if(!GlassAuth.isAuthed) {
    glassMessageBoxOk("Not Authed", "You haven't authenticated with the Glass servers yet!");
    return;
  }

  %url = "http://" @ Glass.address @ "/api/3/discord_gen.php?ident=" @ urlenc(GlassAuth.ident);
  setClipboard(%url);

  %method = "GET";
  %downloadPath = "";
  %className = "GlassDiscordTCP";

  %tcp = connectToURL(%url, %method, %downloadPath, %className);
  GlassDiscordGui_Button.enabled = false;
}

function GlassDiscord::reset() {
  GlassDiscordGui_Button.setVisible(true);
  GlassDiscordGui_Code.setVisible(false);
  GlassDiscordGui_Timer.setVisible(false);
  GlassDiscordGui_Button.enabled = true;
}

function GlassDiscordGui::onWake(%this) {
  if($Glass::DiscordExpire - getRealTime() > 0) {
    GlassDiscord_timer();
  } else {
    GlassDiscord::reset();
  }
  echo("wake");
}

function GlassDiscordGui::onSleep(%this) {
  cancel($Glass::DiscordSch);
  echo("Sleepy");
}

function GlassDiscordTCP::handleText(%this, %line) {
	%this.buffer = %this.buffer NL %line;
}

function GlassDiscordTCP::onDone(%this, %error) {
	Glass::debug(%this.buffer);

  if(%error) {
    glassMessageBoxOk("Failed", "There was an error retreiving your Discord code!");
    echo(%error);
    return;
  }

  GlassDiscordGui_Code.setValue(trim(%this.buffer));

  GlassDiscordGui_Button.setVisible(false);
  GlassDiscordGui_Code.setVisible(true);
  GlassDiscordGui_Timer.setVisible(true);
  $Glass::DiscordExpire = getRealTime() + 300000;

  GlassDiscord_timer();
}

function GlassDiscord_timer() {
  cancel($Glass::DiscordSch);

  %seconds = mFloor(($Glass::DiscordExpire - getRealTime()) / 1000);

  if(%seconds < 0) {
    GlassDiscord::reset();
    return;
  }

  %m = mFloor(%seconds/60);
  %s = %seconds % 60;

  if(%s < 10) {
    %s = "0" @ %s;
  }

  GlassDiscordGui_Timer.setText("\c2Expires in \c4" @ %m @ ":" @ %s);
  $Glass::DiscordSch = schedule(250, 0, GlassDiscord_timer);
}
