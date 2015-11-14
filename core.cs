function Glass::init() {
	new ScriptObject(Glass) {
		version = "1.1.0-alpha.3";
		address = "api.blocklandglass.com";
		netAddress = "blocklandglass.com";
		enableCLI = true;
	};

  Glass::exec();
}
