function glassMessageBoxOk(%title, %description, %command) {
  if(GlassSettings.get("Glass::UseDefaultWindows")) {
    %original = %description;
    %description = strreplace(%description, "<font:verdana bold:13>", "<spush><font:verdana bold:13>");

    if(%original !$= %description)
      %description = strreplace(%description, "<font:verdana:13>", "<spop>");

    return messageBoxOk(%title, %description, %command);
  }

  MBOKFrame.isGlass = true;

  MBOKFrame.old["profile"] = MBOKFrame.profile;
  MBOKFrame.old["bitmap"] = MBOKFrame.getObject(1).bitmap;

  MBOKFrame.getObject(1).setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn");
  MBOKFrame.setProfile("GlassWindowProfile");

  messageBoxOk(%title, "<font:verdana:13>" @ %description, %command);
}

function glassMessageBoxYesNo(%title, %description, %command) {
  if(GlassSettings.get("Glass::UseDefaultWindows")) {
    %original = %description;
    %description = strreplace(%description, "<font:verdana bold:13>", "<spush><font:verdana bold:13>");

    if(%original !$= %description)
      %description = strreplace(%description, "<font:verdana:13>", "<spop>");

    return messageBoxYesNo(%title, %description, %command);
  }

  MBYesNoFrame.isGlass = true;

  MBYesNoFrame.old["profile"] = MBOKFrame.profile;
  MBYesNoFrame.old["bitmap"] = MBOKFrame.getObject(1).bitmap;

  MBYesNoFrame.getObject(1).setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn");
  MBYesNoFrame.getObject(2).setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn");
  MBYesNoFrame.setProfile("GlassWindowProfile");

  messageBoxYesNo(%title, "<font:verdana:13>" @ %description, %command);
}

function glassMessageBoxOkCancel(%title, %description, %command) {
  if(GlassSettings.get("Glass::UseDefaultWindows")) {
    %original = %description;
    %description = strreplace(%description, "<font:verdana bold:13>", "<spush><font:verdana bold:13>");

    if(%original !$= %description)
      %description = strreplace(%description, "<font:verdana:13>", "<spop>");

    return messageBoxOkCancel(%title, %description, %command);
  }

  MBOKCancelFrame.isGlass = true;

  MBOKCancelFrame.old["profile"] = MBOKCancelFrame.profile;
  MBOKCancelFrame.old["bitmap"] = MBOKCancelFrame.getObject(1).bitmap;

  MBOKCancelFrame.getObject(1).setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn");
  MBOKCancelFrame.getObject(2).setBitmap("Add-Ons/System_BlocklandGlass/image/gui/btn");
  MBOKCancelFrame.setProfile("GlassWindowProfile");

  messageBoxOkCancel(%title, "<font:verdana:13>" @ %description, %command);
}

package GlassMessageBoxesPackage {

  function Canvas::popDialog(%this, %dlg) {
    parent::popDialog(%this, %dlg);

    if(%dlg $= "MessageBoxOkDlg" && MBOKFrame.isGlass) {
      MBOKFrame.isGlass = false;

      MBOKFrame.setProfile(MBOKFrame.old["profile"]);
      MBOKFrame.getObject(1).setBitmap(MBOKFrame.old["bitmap"]);
    }

    if(%dlg $= "MessageBoxYesNoDlg" && MBYesNoFrame.isGlass) {
      MBYesNoFrame.isGlass = false;

      MBYesNoFrame.setProfile(MBYesNoFrame.old["profile"]);
      MBYesNoFrame.getObject(1).setBitmap(MBYesNoFrame.old["bitmap"]);
      MBYesNoFrame.getObject(2).setBitmap(MBYesNoFrame.old["bitmap"]);
    }

    if(%dlg $= "MessageBoxOkCancelDlg" && MBOKCancelFrame.isGlass) {
      MBOKCancelFrame.isGlass = false;

      MBOKCancelFrame.setProfile(MBOKCancelFrame.old["profile"]);
      MBOKCancelFrame.getObject(1).setBitmap(MBOKCancelFrame.old["bitmap"]);
      MBOKCancelFrame.getObject(2).setBitmap(MBOKCancelFrame.old["bitmap"]);
    }
  }

};

activatePackage(GlassMessageBoxesPackage);