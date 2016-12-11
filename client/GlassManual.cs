function GlassManual::init() {
  if(isObject(GlassManualWindow)) {
    GlassManualWindow.scan();
    GlassManualWindow.read("1. Credits");
  }
}

function GlassManualWindow::scan(%this) {
  GlassManualGui_Text.clear();
  GlassManualGui_List.clear();

  %pattern = "Add-Ons/System_BlocklandGlass/resources/docs/*";
  %id = -1;
	while((%file $= "" ? (%file = findFirstFile(%pattern)) : (%file = findNextFile(%pattern))) !$= "") {
    %text = strreplace(%file, "Add-Ons/System_BlocklandGlass/resources/docs/", "");

    %doc = getsubstr(%text, 0, strlen(%text) - 4);

    GlassManualGui_List.addRow(%id++, %doc);
  }
  
  GlassManualGui_List.sortNumerical(0, true);
}

function GlassManualWindow::read(%this, %doc) {
  GlassManualGui_Text.setText("<just:center>\n");

  %fo = new FileObject();
  %fo.openForRead("Add-Ons/System_BlocklandGlass/resources/docs/" @ %doc @ ".txt");
  while(!%fo.isEOF())
    GlassManualGui_Text.addText(%fo.readLine() @ "\n", true);
  %fo.close();
  %fo.delete();
}

function GlassManualGui_List::onSelect(%this, %rowID, %rowText) {
  %doc = trim(%rowText);
  GlassManualWindow.read(%doc);
}