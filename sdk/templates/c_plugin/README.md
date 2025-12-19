# C Plugin Template

This template provides a minimal C ABI plugin that exports `PluginInit` and
`PluginShutdown`.

## Build (example)

```bash
cc -fPIC -shared -I../../include -o libexample_plugin.so plugin.c
```

Copy the resulting shared library into the application's plugin directory.
