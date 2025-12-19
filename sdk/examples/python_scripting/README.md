# Python Scripting Example

This example shows how a plugin can embed CPython to run a small script.

## Files

- `plugin_python.c`: C plugin that initializes Python and runs `script.py`.
- `script.py`: Example script invoked by the plugin.

## Build (example)

```bash
cc -fPIC -shared -I../../include \
  $(python3-config --includes) \
  -o libpython_plugin.so plugin_python.c \
  $(python3-config --ldflags)
```

Make sure `script.py` is available in the working directory or update the
script path inside the plugin.
