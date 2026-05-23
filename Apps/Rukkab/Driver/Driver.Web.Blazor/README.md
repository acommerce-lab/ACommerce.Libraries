# Rukkab Driver — Blazor PWA (thin shell)

This project is a minimal Blazor WebAssembly PWA that references the shared UI library `libs/frontend/Rukkab.UI`.

How to run (development):

```bash
cd Apps/Rukkab/Driver/Driver.Web.Blazor
dotnet run
```

This will host the Blazor WASM dev server. The app expects the backend Driver API available at `http://127.0.0.1:5002` (local dev runner).

To serve statically, build and copy `wwwroot` plus `_framework` output or use `dotnet publish` and serve the `publish/wwwroot` directory.
