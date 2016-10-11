function glassMessageBoxOk(%title, %description, %command) {
  // messageBoxOk(%title, "<font:verdana:13>" @ %description, %command);
  // return;
  
  MBOKFrame.isGlass = true;
  
  MBOKFrame.old["profile"] = MBOKFrame.profile;
  MBOKFrame.old["bitmap"] = MBOKFrame.getObject(1).bitmap;

  MBOKFrame.getObject(1).bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
  MBOKFrame.profile = "GlassWindowProfile";
  
  messageBoxOk(%title, "<font:verdana:13>" @ %description, %command);
}

function glassMessageBoxYesNo(%title, %description, %command) {
  // messageBoxYesNo(%title, "<font:verdana:13>" @ %description, %command);
  // return;
  
  MBYesNoFrame.isGlass = true;
  
  MBYesNoFrame.old["profile"] = MBOKFrame.profile;
  MBYesNoFrame.old["bitmap"] = MBOKFrame.getObject(1).bitmap;

  MBYesNoFrame.getObject(1).bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
  MBYesNoFrame.getObject(2).bitmap = "Add-Ons/System_BlocklandGlass/image/gui/btn";
  MBYesNoFrame.profile = "GlassWindowProfile";
  
  messageBoxYesNo(%title, "<font:verdana:13>" @ %description, %command);
}

package GlassMessageBoxesPackage {

  function Canvas::popDialog(%this, %dlg) {
    parent::popDialog(%this, %dlg);
    
    if(%dlg $= "MessageBoxOkDlg" && MBOKFrame.isGlass) {
      MBOKFrame.isGlass = false;
      
      MBOKFrame.profile = MBOKFrame.old["profile"];
      MBOKFrame.getObject(1).bitmap = MBOKFrame.old["bitmap"];
    }
    
    if(%dlg $= "MessageBoxYesNoDlg" && MBYesNoFrame.isGlass) {
      MBYesNoFrame.isGlass = false;
      
      MBYesNoFrame.profile = MBYesNoFrame.old["profile"];
      MBYesNoFrame.getObject(1).bitmap = MBYesNoFrame.old["bitmap"];
      MBYesNoFrame.getObject(2).bitmap = MBYesNoFrame.old["bitmap"];
    }
  }
  
};

activatePackage(GlassMessageBoxesPackage);