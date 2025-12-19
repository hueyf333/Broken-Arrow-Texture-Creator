#include "ba_texture_plugin.h"
#include <string.h>

static void PluginShutdownImpl(void) {
    /* TODO: release any plugin resources. */
}

static void PluginReloadImpl(void) {
    /* TODO: reset plugin state after hot reload. */
}

BaTexturePluginInitResult PluginInit(
    const BaTexturePluginHost *host,
    BaTexturePluginInfo *info,
    BaTexturePluginExports *exports) {
    (void)host;

    if (info == NULL || exports == NULL) {
        return BA_TEXTURE_PLUGIN_INIT_FAILED;
    }

    info->api_version = BA_TEXTURE_PLUGIN_API_VERSION;
    info->plugin_name = "Example C Plugin";
    info->plugin_version = "0.1.0";

    exports->api_version = BA_TEXTURE_PLUGIN_API_VERSION;
    exports->shutdown = PluginShutdownImpl;
    exports->on_reload = PluginReloadImpl;

    return BA_TEXTURE_PLUGIN_INIT_OK;
}

void PluginShutdown(void) {
    PluginShutdownImpl();
}
