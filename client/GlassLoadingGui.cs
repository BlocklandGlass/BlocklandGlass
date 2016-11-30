function GlassLoading::changeGui() {
  if(LoadingGui.isGlass) {
    return;
  }

  %loadingGui = LoadingGui.getId();

  exec("./gui/LoadingGui.gui");

  %loadingGui.deleteAll();
  %loadingGui.delete();

  LoadingGui.isGlass = true;
}

package GlassLoading {
  function NPL_List::addRow(%this, %id, %val) {
    GlassLoadingGui_UserList.addRow(%id, %val);
    echo("Add row: " NL %id NL %val NL %index);
    return parent::addRow(%this, %id, %val);
  }

  function NPL_List::sort(%this, %a) {
    echo("sort!" @ %a);
    parent::sort(%this, %a);
  }

  function NPL_List::sortNumerical(%this, %a) {
    echo("sort num!" @ %a);
    parent::sortNumerical(%this, %a);
  }

  function NPL_List::clear(%this) {
    GlassLoadingGui_UserList.clear();
    parent::clear(%this);
  }
};
activatePackage(GlassLoading);
