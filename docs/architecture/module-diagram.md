# Module Architecture

## Module boundaries

```
include/
  core/   -> Public API headers for the core module.
  tools/  -> Public API headers for shared tools/utilities.
  render/ -> Public API headers for rendering-facing utilities.
  ui/     -> Public API headers for UI-facing code.

src/
  core/   -> Internal implementation of the core module.
  tools/  -> Internal implementation of the tools module.
  render/ -> Internal implementation of the render module.
  ui/     -> Internal implementation of the UI module.
```

## Dependency direction

```
core   (dependency-free)
tools  (dependency-free)
render (dependency-free)
   \      |       /
        ui
```

* UI depends on `core`, `tools`, and `render`.
* The `core` module does not depend on any other module.

## Build targets

* `core` → `src/core`, public headers in `include/core`
* `tools` → `src/tools`, public headers in `include/tools`
* `render` → `src/render`, public headers in `include/render`
* `ui` → `src/ui`, public headers in `include/ui`, links `core`, `tools`, `render`
