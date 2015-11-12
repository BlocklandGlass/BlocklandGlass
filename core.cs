function Glass::init() {
  Glass::exec();

	new ScriptObject(Glass) {
		version = "1.1.0-alpha.2";
		address = "api.blocklandglass.com";
		netAddress = "blocklandglass.com";
		enableCLI = true;
	};
}
