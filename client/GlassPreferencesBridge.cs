//================================
// Bridge Blockland Preferences
//================================

$BLPrefs::Version = "0.0.0-alpha+glassbridge";

function clientCmdAddCategory(%category, %icon) {
	if($BLPrefs::Client::Exists[%category]) {
		return;
	}

	$BLPrefs::Client::Exists[%category] = true;
	%color = (($BLPrefs::CategoryRows % 2) ? "220 235 255" : "230 245 255") SPC "255";

	%row = new GuiSwatchCtrl(BLPrefCategoryRow) {
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "0" SPC $BLPrefs::CategoryRows*20;
		extent = "142 20";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		color = %color;
		originalColor = %color;
		category = %category;

		new GuiBitmapCtrl() {
			profile = "GuiDefaultProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "4 1";
			extent = "16 16";
			minExtent = "8 2";
			enabled = "1";
			visible = "1";
			clipToParent = "1";
			bitmap = "Add-Ons/Client_BlocklandPreferences/icons/" @ getIconOverride(%category, %icon) @ ".png";
			wrap = "0";
			lockAspectRatio = "1";
			alignLeft = "0";
			alignTop = "0";
			overflowImage = "0";
			keepCached = "0";
			mColor = "255 255 255 255";
			mMultiply = "0";
		};
	};

	%text = new GuiTextCtrl() {
		profile = "BLPrefTextProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "24 1";
		extent = "44 18";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		text = %category;
		maxLength = "255";
	};
	%row.textObj = %text;
	%row.add(%text);

	%button = new GuiBitmapButtonCtrl(BLPrefCategorySwitchButton) {
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		position = "0 0";
		extent = "140 20";
		minExtent = "8 2";
		enabled = "1";
		visible = "1";
		clipToParent = "1";
		command = "requestBLPrefCategory(\"" @ %category @ "\");";
		groupNum = "-1";
		buttonType = "PushButton";
		bitmap = "base/client/ui/btnBlank";
		lockAspectRatio = "0";
		alignLeft = "0";
		alignTop = "0";
		overflowImage = "0";
		mKeepCached = "0";
		mColor = "255 255 255 255";
		text = "";
	};
	%row.add(%button);
	%button.setValue("");

	BLPrefCategoryList.add(%row);

	$BLPrefs::CategoryRows++;
}

function requestBLPrefCategory(%category) {
	BLPrefTitle.setValue(%category);
	$BLPrefs::LastRowPos = 20;
	$BLPrefs::PrefRows = 0;
	BLPrefCategoryList.setRowActive(%category);
	BLPrefPrefList.clearPrefRows();
	commandToServer('getBLPrefCategory', %category);
}

function clientCmdReceivePref(%title, %type, %variable, %value, %params, %legacy) {
	if(%legacy) {
		BLPrefTitle.setValue(BLPrefTitle.getValue() SPC "(Legacy)");
	}

	switch$(%type) {
		case "number" or "string":
			BLPrefPrefList.addTextInput(%title, %variable, %value, %params);

		case "boolean":
			BLPrefPrefList.addCheckboxInput(%title, %variable, %value);
	}
}

package BLPrefClientPackage {
	function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
		Parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, $BLPrefs::Version, %i, %j, %k, %l, %m, %n, %o, %p);
	}
};
activatePackage(BLPrefClientPackage);
