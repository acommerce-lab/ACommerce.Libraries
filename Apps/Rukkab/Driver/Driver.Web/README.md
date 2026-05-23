# Rukkab Driver — PWA (minimal)

This is a minimal progressive web app for the Driver role. It's a static site designed to be served from any web server (or the .NET app's static files) and supports installability via the web manifest + service worker.

How to use

1. Serve the `www` folder with a static server (for local testing):

```bash
cd Apps/Rukkab/Driver/Driver.Web/www
python3 -m http.server 8082
```

2. Open `http://127.0.0.1:8082` in Chrome/Edge/Firefox. Use the input to enter a ride id and call Accept/Arrive/Start/Complete.
3. To install as PWA, use the browser's Install App UI (manifest + service worker enable installation).

Notes
- This is a simple demo UI; extend with frameworks (React/Vue/Svelte) or tailor styling as needed.
