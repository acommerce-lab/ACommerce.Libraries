// Nadena PWA helper: service worker registration, install button, update prompt,
// and iOS "Add to Home Screen" instructions.
(function () {
    'use strict';

    var deferredPrompt = null;

    function el(tag, cls, html) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (html != null) e.innerHTML = html;
        return e;
    }
    function remove(id) { var n = document.getElementById(id); if (n) n.remove(); }
    function isIos() { return /iphone|ipad|ipod/i.test(navigator.userAgent); }
    function isStandalone() {
        return (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) ||
               window.navigator.standalone === true;
    }

    // ── Install bar (Android / Windows / Chrome / Edge) ──────────────────
    function showInstallBar() {
        if (document.getElementById('nadena-install') || isStandalone()) return;
        var bar = el('div', 'nadena-pwa-bar');
        bar.id = 'nadena-install';
        bar.setAttribute('dir', 'rtl');
        bar.appendChild(el('span', 'nadena-pwa-icon', '⬇️'));
        var txt = el('div', 'nadena-pwa-text');
        txt.appendChild(el('strong', null, 'ثبّت تطبيق نادينا'));
        txt.appendChild(el('small', null, 'وصول أسرع من شاشتك الرئيسية'));
        bar.appendChild(txt);
        var install = el('button', 'nadena-pwa-btn', 'تثبيت');
        install.onclick = async function () {
            if (!deferredPrompt) { remove('nadena-install'); return; }
            deferredPrompt.prompt();
            try { await deferredPrompt.userChoice; } catch (_) { }
            deferredPrompt = null;
            remove('nadena-install');
        };
        bar.appendChild(install);
        var close = el('button', 'nadena-pwa-close', '×');
        close.setAttribute('aria-label', 'إغلاق');
        close.onclick = function () { remove('nadena-install'); };
        bar.appendChild(close);
        document.body.appendChild(bar);
    }

    // ── iOS instructions (Safari can't auto-install) ─────────────────────
    function showIosHint() {
        if (document.getElementById('nadena-ios') || isStandalone()) return;
        if (localStorage.getItem('nadena-ios-dismissed') === '1') return;
        var bar = el('div', 'nadena-pwa-bar nadena-pwa-ios');
        bar.id = 'nadena-ios';
        bar.setAttribute('dir', 'rtl');
        bar.appendChild(el('span', 'nadena-pwa-icon', '📲'));
        var txt = el('div', 'nadena-pwa-text');
        txt.appendChild(el('strong', null, 'أضف نادينا إلى الشاشة الرئيسية'));
        txt.appendChild(el('small', null, 'اضغط زر المشاركة ↑ ثم «إضافة إلى الشاشة الرئيسية»'));
        bar.appendChild(txt);
        var close = el('button', 'nadena-pwa-close', '×');
        close.setAttribute('aria-label', 'إغلاق');
        close.onclick = function () { remove('nadena-ios'); localStorage.setItem('nadena-ios-dismissed', '1'); };
        bar.appendChild(close);
        document.body.appendChild(bar);
    }

    // ── Update available bar ─────────────────────────────────────────────
    function showUpdateBar(reg) {
        if (document.getElementById('nadena-update')) return;
        var bar = el('div', 'nadena-pwa-bar nadena-pwa-update');
        bar.id = 'nadena-update';
        bar.setAttribute('dir', 'rtl');
        bar.appendChild(el('span', 'nadena-pwa-icon', '✨'));
        var txt = el('div', 'nadena-pwa-text');
        txt.appendChild(el('strong', null, 'يتوفّر تحديث جديد'));
        txt.appendChild(el('small', null, 'أعد التحميل للحصول على أحدث إصدار'));
        bar.appendChild(txt);
        var update = el('button', 'nadena-pwa-btn', 'تحديث');
        update.onclick = function () {
            if (reg && reg.waiting) reg.waiting.postMessage('skipWaiting');
            else window.location.reload();
        };
        bar.appendChild(update);
        document.body.appendChild(bar);
    }

    // ── Service worker registration + update detection ───────────────────
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', function () {
            navigator.serviceWorker.register('service-worker.js').then(function (reg) {
                // already waiting (update ready from a previous load)
                if (reg.waiting && navigator.serviceWorker.controller) showUpdateBar(reg);

                reg.addEventListener('updatefound', function () {
                    var nw = reg.installing;
                    if (!nw) return;
                    nw.addEventListener('statechange', function () {
                        if (nw.state === 'installed' && navigator.serviceWorker.controller) {
                            showUpdateBar(reg);
                        }
                    });
                });

                // check for updates periodically
                setInterval(function () { reg.update().catch(function () { }); }, 60 * 1000);
            }).catch(function (e) { console.warn('[PWA] SW registration failed:', e); });

            var refreshing = false;
            navigator.serviceWorker.addEventListener('controllerchange', function () {
                if (refreshing) return;
                refreshing = true;
                window.location.reload();
            });
        });
    }

    // beforeinstallprompt fires only when the app is installable (Android/desktop Chromium)
    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredPrompt = e;
        showInstallBar();
    });

    window.addEventListener('appinstalled', function () {
        deferredPrompt = null;
        remove('nadena-install');
    });

    // iOS hint (no beforeinstallprompt on Safari)
    window.addEventListener('load', function () {
        if (isIos() && !isStandalone()) setTimeout(showIosHint, 3000);
    });
})();
