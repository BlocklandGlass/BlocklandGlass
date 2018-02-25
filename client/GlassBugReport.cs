function GlassBugReport::openGui() {
  canvas.popDialog(GlassBugReportGui);
  GlassBugReportGui.populateList();
  GlassBugReportGui_Addon.setSelected(11);
  canvas.pushDialog(GlassBugReportGui);
}

function GlassBugReport::crashPromptYes() {
  GlassBugReportGui.populateList();
  canvas.pushDialog(GlassBugReportGui);
  GlassBugReportGui_Addon.setSelected(11);
  GlassBugReportGui_Addon.enabled = false;
}

function GlassBugReportGui::populateList(%this) {
  GlassBugReportGui_Addon.clear();
  for(%i = 0; %i < GlassAddons.getCount(); %i++) {
    %obj = GlassAddons.getObject(%i);
    GlassBugReportGui_Addon.add(%obj.title, %obj.id);
  }
  GlassBugReportGui_Addon.sort();
}

function GlassBugReportGui::submit(%this) {
  %aid   = GlassBugReportGui_Addon.getSelected();
  %title = GlassBugReportGui_Title.getValue();
  %body  = GlassBugReportGui_Body.getValue();

  if(strlen(%title) < 5) {
    glassMessageBoxOk("Uh-Oh", "Submit a longer title please!");
    return;
  }

  if(strlen(%body) < 5) {
    glassMessageBoxOk("Uh-Oh", "Submit a longer description please!");
    return;
  }

  glassMessageBoxOk("Bug Report Sent", "Thanks for your feedback!");

  %title = urlEnc(%title);
  %body  = urlEnc(%body);

  %url =        "title="  @ %title;
  %url = %url @ "&body="  @ %body;
  %url = %url @ "&aid="   @ %aid;

	%className = "GlassBugReportTCP";
  %tcp = GlassApi.request("bugReport", %url, %className, true);

  canvas.popDialog(GlassBugReportGui); //submit before this
}

function GlassBugReportGui::onSleep(%this) {
  GlassBugReportGui_Addon.enabled = true;
  GlassBugReportGui_Title.setValue("");
  GlassBugReportGui_Body.setValue("");
}

function GlassBugReportButton::onMouseEnter(%this) {
  GlassBugReportIcon.mColor = "255 255 255 255";
}

function GlassBugReportButton::onMouseLeave(%this) {
  GlassBugReportIcon.mColor = "255 255 255 150";
}

function GlassBugReportButton::onMouseUp(%this) {
  GlassBugReport::openGui();
}
