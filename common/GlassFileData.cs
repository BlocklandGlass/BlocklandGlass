function GlassFileData::create(%name, %id, %branch, %filename) {
	return new ScriptObject() {
		class = GlassFileData;

		name = %name;
		id = %id;
		branch = %branch;
		filename = %filename;
	};
}