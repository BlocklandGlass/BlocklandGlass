function GlassAudio::init() {
  for(%volume = 0.2; %volume < 1.1; (%volume = %volume + 0.2)) {
    %name = "GlassAudioVolume" @ strreplace(%volume, ".", "_") @ "Gui";
    
    %obj = "if(!isObject(" @ %name @ ")) new AudioDescription(" @ %name @ ") {";
    %obj = %obj NL "volume = " @ %volume @ ";";
    %obj = %obj NL "isLooping = false;";
    %obj = %obj NL "is3D = false;";
    %obj = %obj NL "type = $GuiAudioType;";
    %obj = %obj NL "};";

    eval(%obj);
  }
}

function GlassAudio::add(%name, %volumeControlled) {
  if(%name $= "") {
    error("No sound name specified.");
    return;
  }

  %filename = %name;
  %name = "Glass" @ strreplace(%name, " ", "_");

  if(isObject(%name @ "Audio") || isObject(%name @ "1Audio")) {
    error("AudioProfile " @ %name @ " already exists.");
    return;
  }

  %location = "Add-Ons/System_BlocklandGlass/sound/" @ %filename @ ".wav";

  if(!isFile(%location)) {
    error(%filename @ ".wav not found.");
    return;
  }

  if(%volumeControlled) {
    for(%volume = 0.2; %volume < 1.1; (%volume = %volume + 0.2)) {
      %unique = strreplace(%volume, ".", "_");
      %description = "GlassAudioVolume" @ %unique @ "Gui";

      if(!isObject(%description)) {
        error("Unable to find AudioDescription " @ %description @ ", aborting...");
        return;
      }
      
      %objname = %name @ %unique @ "Audio";

      %obj = "if(!isObject(" @ %objname @ ")) new AudioProfile(" @ %objname @ ") {";
      %obj = %obj NL "filename = \"" @ %location @ "\";";
      %obj = %obj NL "isLooping = false;";
      %obj = %obj NL "description = \"" @ %description @ "\";";
      %obj = %obj NL "preload = true;";
      %obj = %obj NL "};";

      eval(%obj);
    }
  } else {
    %name = %name @ "1Audio";

    %obj = "if(!isObject(" @ %name @ ")) new AudioProfile(" @ %name @ ") {";
    %obj = %obj NL "filename = \"" @ %location @ "\";";
    %obj = %obj NL "isLooping = false;";
    %obj = %obj NL "description = \"GlassAudioVolume1Gui\";";
    %obj = %obj NL "preload = true;";
    %obj = %obj NL "};";
    
    eval(%obj);
  }
}

function GlassAudio::play(%sound, %volume) {
  if(%volume <= 0 && %volume !$= "")
    return;
  else if(%volume >= 1 || %volume $= "")
    %volume = 1;

  if(((%volume * 100) % (0.2 * 100)) != 0) {
    error("Volume must be divisible by 0.2, got " @ %volume);
    return;
  }

  %sound = "Glass" @ %sound;

  if(%volume == 1) {
    if(!isObject(%sound = %sound @ "1Audio")) {
      error("Non-existent AudioProfile \"" @ %sound @ "\"");
      return;
    }

    alxPlay(%sound);
  } else {
    if(!isObject(%sound @ "0_8Audio")) {
      warn("AudioProfile \"" @ %sound @ "\" is not added as volume controlled, continuing with default volume...");
      alxPlay(%sound @ "1Audio");
      return;
    } else {
      %volume = strreplace(%volume, ".", "_");
      alxPlay(%sound @ %volume @ "Audio");
    }
  }
}

function GlassAudio::updateVolume(%setting) {
  %volume = GlassSettings.get(%setting);

  switch$(%setting) {
    case "Volume::RoomChat":
      GlassAudio::play("chatroomMsg1", %volume);
    case "Volume::FriendStatus":
      GlassAudio::play("friendOnline", %volume);
    case "Volume::DirectMessage":
      GlassAudio::play("userMsgReceived", %volume);
    default:
      error("Non-existent volume setting \"" @ %setting @ "\"");
  }
}