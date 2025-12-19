#include "ba_texture_plugin.h"
#include <Python.h>
#include <stdio.h>

static void RunScript(void) {
    PyObject *name = PyUnicode_FromString("script");
    PyObject *module = PyImport_Import(name);
    Py_XDECREF(name);

    if (module == NULL) {
        PyErr_Print();
        return;
    }

    PyObject *func = PyObject_GetAttrString(module, "run");
    if (func && PyCallable_Check(func)) {
        PyObject *result = PyObject_CallObject(func, NULL);
        if (result == NULL) {
            PyErr_Print();
        }
        Py_XDECREF(result);
    } else {
        PyErr_Print();
    }

    Py_XDECREF(func);
    Py_XDECREF(module);
}

static void PluginShutdownImpl(void) {
    if (Py_IsInitialized()) {
        Py_Finalize();
    }
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
    info->plugin_name = "Python Script Plugin";
    info->plugin_version = "0.1.0";

    exports->api_version = BA_TEXTURE_PLUGIN_API_VERSION;
    exports->shutdown = PluginShutdownImpl;
    exports->on_reload = NULL;

    Py_Initialize();
    if (!Py_IsInitialized()) {
        return BA_TEXTURE_PLUGIN_INIT_FAILED;
    }

    RunScript();

    return BA_TEXTURE_PLUGIN_INIT_OK;
}

void PluginShutdown(void) {
    PluginShutdownImpl();
}
