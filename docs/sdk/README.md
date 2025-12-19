# Broken Arrow Texture Creator SDK

## Overview

The SDK ships a stable C ABI for plugins along with headers and templates. The
public entrypoints are:

- `PluginInit` — called by the host to initialize the plugin and fetch metadata.
- `PluginShutdown` — called by the host during teardown.

Versioning is encoded as `MAJOR.MINOR.PATCH` in the `api_version` field. The
host should reject plugins with a mismatched major version and may accept newer
minor/patch versions depending on compatibility policy.

## C ABI Interface

The core interface lives in `sdk/include/ba_texture_plugin.h`. Plugins must fill
out:

- `BaTexturePluginInfo` with metadata about the plugin.
- `BaTexturePluginExports` with callbacks back into the plugin.

The host provides a `BaTexturePluginHost` pointer for future expansion.

## SDK Layout

```
sdk/
  include/   -> public headers
  lib/       -> import libraries and runtime stubs
  templates/ -> sample plugins
  examples/  -> optional integrations (Python, etc.)
```

Consume the SDK by adding `sdk/include` to your include path and linking against
the appropriate library in `sdk/lib`.

## Python Embedding Decision

The recommended approach is to embed CPython via the C API rather than pybind11.
Reasons:

- The host and plugin already communicate through a C ABI; using the C API keeps
  the boundary simple and language-agnostic.
- The embedding surface is small (initialize, import script, call functions),
  making direct C API usage easier to audit.
- It avoids adding a C++ toolchain requirement for plugin authors.

See `sdk/examples/python_scripting` for a minimal example.

## Plugin Template

A minimal C plugin template is available in `sdk/templates/c_plugin`.

## Versioning Policy

- **Major**: breaking ABI changes. The host must reject mismatched major
  versions.
- **Minor**: backward-compatible additions (new callbacks or struct fields).
- **Patch**: bug fixes; no ABI changes.
