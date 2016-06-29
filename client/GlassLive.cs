function GlassLive::sendRoomMessage(%msg) {
  %obj = JettisonObject();
  %obj.set("type", "string", "roomChat");
  %obj.set("message", "string", %msg);

  GlassNotificationTCP.send(jettisonStringify("object", %obj) @ "\r\n");
}

function GlassLive::pushMessage(%msg) {
  %val = GlassChatroomGui_Text.getValue();
  if(%val !$= "")
    %val = %val @ "<br>" @ %msg;
  else
    %val = %msg;

  GlassChatroomGui_Text.setValue(%val);
  GlassChatroomGui_Text.forceReflow();
  GlassChatroomGui_TextSwatch.verticalMatchChildren();
  GlassChatroomGui_TextSwatch.setVisible(true);
  GlassChatroomGui_TextSwatch.getGroup().scrollToBottom();
}
