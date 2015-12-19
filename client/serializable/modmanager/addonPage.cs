function GlassModManagerGui::fetchAndRenderAddon(%modId) {
  GlassModManager::placeCall("addon", "id" TAB %modId);
}

function GlassModManagerGui::renderAddon(%modId, %name, %filename, %description, %branches) {

}
