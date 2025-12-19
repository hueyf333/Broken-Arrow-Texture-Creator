#ifndef BA_TEXTURE_PLUGIN_H
#define BA_TEXTURE_PLUGIN_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>

#define BA_TEXTURE_PLUGIN_API_VERSION_MAJOR 1
#define BA_TEXTURE_PLUGIN_API_VERSION_MINOR 0
#define BA_TEXTURE_PLUGIN_API_VERSION_PATCH 0

#define BA_TEXTURE_PLUGIN_API_VERSION \
    ((uint32_t)(BA_TEXTURE_PLUGIN_API_VERSION_MAJOR << 16) | \
     (uint32_t)(BA_TEXTURE_PLUGIN_API_VERSION_MINOR << 8) | \
     (uint32_t)(BA_TEXTURE_PLUGIN_API_VERSION_PATCH))

typedef struct BaTexturePluginHost BaTexturePluginHost;

typedef struct BaTexturePluginInfo {
    uint32_t api_version;
    const char *plugin_name;
    const char *plugin_version;
} BaTexturePluginInfo;

typedef struct BaTexturePluginExports {
    uint32_t api_version;
    void (*shutdown)(void);
    void (*on_reload)(void);
} BaTexturePluginExports;

typedef enum BaTexturePluginInitResult {
    BA_TEXTURE_PLUGIN_INIT_OK = 0,
    BA_TEXTURE_PLUGIN_INIT_INCOMPATIBLE = 1,
    BA_TEXTURE_PLUGIN_INIT_FAILED = 2
} BaTexturePluginInitResult;

#if defined(_WIN32)
#define BA_TEXTURE_PLUGIN_EXPORT __declspec(dllexport)
#else
#define BA_TEXTURE_PLUGIN_EXPORT __attribute__((visibility("default")))
#endif

BA_TEXTURE_PLUGIN_EXPORT BaTexturePluginInitResult PluginInit(
    const BaTexturePluginHost *host,
    BaTexturePluginInfo *info,
    BaTexturePluginExports *exports);

BA_TEXTURE_PLUGIN_EXPORT void PluginShutdown(void);

#ifdef __cplusplus
}
#endif

#endif
