function GlassManual::init() {
  if(isObject(GlassManualWindow)) {
    GlassManualWindow.scan();
  }
}

function GlassManualWindow::onWake(%this) {
  if(!%this.firstWake) {
    GlassManualWindow.read("1 Credits");
    GlassManualGui_List.setSelectedRow(0);
    %this.firstWake = true;
  }
}

function GlassManualWindow::downloadDocListing(%this) {
  %tcp = GlassApi.request("docs", "", "GlassManualTCP");
  %tcp.requestType = "list";
}

function GlassManualWindow::scan(%this) {
  GlassManualGui_Text.clear();
  GlassManualGui_List.clear();

  %pattern = "Add-Ons/System_BlocklandGlass/docs/*";
  %id = -1;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %text = strreplace(%file, "Add-Ons/System_BlocklandGlass/docs/", "");

    %doc = getsubstr(%text, 0, strlen(%text) - 4);
    %indents = "";

    for(%i = 0; %i < strlen(%doc); %i++) {
      %letter = getsubstr(%doc, %i, 1);
      if(%letter $= ".") {
        %indents = %indents @ "  ";
      }
    }

    GlassManualGui_List.addRow(%id++, %indents @ %doc);
  }

  GlassManualGui_List.sortNumerical(0, true);
  %this.downloadDocListing();
}

function GlassManualWindow::downloadDoc(%this, %doc) {
  %tcp = GlassApi.request("docs", "doc=" @ urlenc(%doc), "GlassManualTCP", false, true);
  %tcp.requestType = "doc";
  %tcp.doc  = %doc;

  %this.loadingDoc = %doc;
}

function GlassManualWindow::read(%this, %doc) {
  if(GlassManualWindow.useRemote) {
    %this.downloadDoc(%doc);
    return;
  }

  GlassManualGui_Text.setText("<font:verdana:12><color:333333>");

  %fo = new FileObject();
  %fo.openForRead("Add-Ons/System_BlocklandGlass/docs/" @ %doc @ ".txt");
  while(!%fo.isEOF())
    GlassManualGui_Text.addText(collapseEscape(%fo.readLine()) @ "\n", true);
  %fo.close();
  %fo.delete();

  GlassManualGui_Text.setText(trim(GlassManualGui_Text.getText()));

  GlassManualGui_Text.forceReflow();
  GlassManualGui_Container.verticalMatchChildren(20, 10);
  GlassManualGui_Container.setVisible(true);
}

function GlassManualGui_List::onSelect(%this, %rowID, %rowText) {
  %doc = trim(%rowText);

  GlassManualWindow.read(%doc);
}

function GlassManualTCP::handleText(%this, %line) {
  if(%this.buffer $= "")
    %this.buffer = %line;
  else
    %this.buffer = %this.buffer NL %line;
}

function GlassManualTCP::onDone(%this, %error) {
  if(%error)
    return; //will default to local

  if(%this.requestType $= "list") {
    GlassManualWindow.useRemote = true;
    GlassManualGui_List.clear();

    for(%j = 0; %j < getLineCount(%this.buffer); %j++) {%text = strreplace(%file, "Add-Ons/System_BlocklandGlass/docs/", "");
      %text = getLine(%this.buffer, %j);

      if(strlen(trim(%text)) == 0)
        continue;

      %doc = getsubstr(%text, 0, strlen(%text) - 4);
      %indents = "";

      for(%i = 0; %i < strlen(%doc); %i++) {
        %letter = getsubstr(%doc, %i, 1);
        if(%letter $= ".") {
          %indents = %indents @ "  ";
        }
      }

      GlassManualGui_List.addRow(%id++, %indents @ %doc);
    }

    GlassManualGui_List.sortNumerical(0, true);
  } else {
    if(GlassManualWindow.loadingDoc !$= %this.doc) {
      return;
    }


    GlassManualGui_Text.setText("<font:verdana:12><color:333333>");

    GlassManualGui_Text.addText(collapseEscape(%this.buffer) @ "\n", true);
    GlassManualGui_Text.setText(trim(GlassManualGui_Text.getText()));

    GlassManualGui_Text.forceReflow();
    GlassManualGui_Container.verticalMatchChildren(20, 10);
    GlassManualGui_Container.setVisible(true);
  }
}
