// Nadena PWA service worker.
// Strategy: network-first for same-origin app files (always fresh when online,
// offline fallback from cache). API calls (cross-origin) are left to the network.
// A fetch handler is required for the browser to offer "Install".
const CACHE = 'nadena-cache-v1';

self.addEventListener('install', function (e) {
    self.skipWaiting();
});

self.addEventListener('activate', function (e) {
    e.waitUntil((async function () {
        const keys = await caches.keys();
        await Promise.all(keys.filter(function (k) { return k !== CACHE; }).map(function (k) { return caches.delete(k); }));
        await self.clients.claim();
    })());
});

self.addEventListener('fetch', function (e) {
    const req = e.request;
    if (req.method !== 'GET') return;

    let url;
    try { url = new URL(req.url); } catch (_) { return; }
    // Only handle same-origin app assets; let the backend API go straight to network.
    if (url.origin !== self.location.origin) return;

    e.respondWith((async function () {
        try {
            const fresh = await fetch(req);
            if (fresh && fresh.ok) {
                const cache = await caches.open(CACHE);
                cache.put(req, fresh.clone()).catch(function () { });
            }
            return fresh;
        } catch (err) {
            const cached = await caches.match(req);
            if (cached) return cached;
            if (req.mode === 'navigate') {
                const index = await caches.match('index.html');
                if (index) return index;
            }
            throw err;
        }
    })());
});

self.addEventListener('message', function (e) {
    if (e.data === 'skipWaiting') self.skipWaiting();
});
