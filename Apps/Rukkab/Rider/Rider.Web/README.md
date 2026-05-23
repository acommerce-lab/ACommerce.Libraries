# Rukkab Rider — PWA (minimal)

This is a minimal progressive web app for the Rider role. It's a static site designed to be served from any web server (or the .NET app's static files) and supports installability via the web manifest + service worker.

How to use

1. Serve the `www` folder with a static server (for local testing):

```bash
cd Apps/Rukkab/Rider/Rider.Web/www
python3 -m http.server 8081
```

2. Open `http://127.0.0.1:8081` in Chrome/Edge/Firefox and press "Request Ride" to create a ride via the Rider API.

Notes
- This is a simple demo UI; extend with frameworks (React/Vue/Svelte) or tailor styling as needed.
