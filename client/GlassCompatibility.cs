package GlassEnableButton {
	function ServerSettingsGui::onWake(%this) {
		parent::onWake(%this);
    
    if(isObject(ServerSettingsGui_RTBLabel) && ServerSettingsGui_RTBLabel.visible && !isObject(ServerSettingsGui_BLGLabel)) {
      %y = getWord(ServerSettingsGui_RTBLabel.position, 1) + 40;
    } else {
      %y = getWord(ServerSettingsGui_AdminPasswordLabel.position, 1) + 40;
    }
    
    if(!isObject(ServerSettingsGui_BLGLabel)) {
      %label = new GuiTextCtrl(ServerSettingsGui_BLGLabel) {
        profile = "ImpactTextProfile";
        horizSizing = "relative";
        vertSizing = "relative";
        position = "0 " @ %y;
        extent = "160 33";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        text = "BLG: ";
        maxLength = "255";
      };

      %button = new GuiCheckBoxCtrl(ServerSettingsGui_UseBLG) {
        profile = "ImpactCheckProfile";
        horizSizing = "relative";
        vertSizing = "relative";
        position = "170 " @ %y;
        extent = "168 33";
        minExtent = "8 2";
        enabled = "1";
        visible = "1";
        clipToParent = "1";
        variable = "$ServerSettingsGui::UseBLG";
        text = "Use BLG";
        groupNum = "-1";
        buttonType = "ToggleButton";
      };
      
      %this.add(%label);
      %this.add(%button);
    }
	}
};
activatePackage(GlassEnableButton);

function GlassCompatibility::oRBs_warning() {
  glassMessageBoxYesNo("oRBs Warning", "<font:verdana bold:13>Please read carefully.<br><br><font:verdana:13>Blockland Glass no longer supports oRBs.<br><br>oRBs has been operating as life support for a system (RTB) that has been left abandoned by its original creator since 2014 -- oRBs is not the way forward for the community, it is a step backwards.<br><br>oRBs, and any other third party RTB \"variation\" that seeks to continue its original service or development will never be able to bring back the old RTB service in its entirety with new updates: it is dead.<br><br><font:verdana bold:13><color:FF0000>You can choose to delete oRBs now; otherwise, this warning will not be shown again.", "GlassCompatibility::oRBs_delete();");
  
  echo("oRBs warning dialog window displayed, this should only happen once (unless forced).");
  
  GlassSettings.update("Live::oRBsNotified", true);
}

function GlassCompatibility::oRBs_delete() {
  if(isFile("Add-Ons/System_oRBs.zip")) {
    warn("User acknowledged, deleting oRBs...");
    
    fileDelete("Add-Ons/System_oRBs.zip");
    
    exec("Add-Ons/System_BlocklandGlass/update.cs");
  }
}

function GlassCompatibility::oRBs_find() {
  if(!GlassSettings.get("Live::oRBsNotified")) {
    // if(getFileCRC("Add-Ons/System_oRBs.zip") == -2099294939) {
    if(isFile("Add-Ons/System_oRBs.zip")) {
      schedule(2500, 0, eval, "GlassNotificationManager::newNotification(\"oRBs Warning\", \"Blockland Glass has stopped supporting oRBs, click here for further information.\", \"warning\", 1, \"GlassCompatibility::oRBs_warning();\");");
      schedule(2500, 0, "alxPlay", GlassUserMentionedAudio);
    }
  }
}

GlassCompatibility::oRBs_find();