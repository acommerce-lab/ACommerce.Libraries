/**
 * Capture marketing click ids / utm and send to backend once per session.
 * - captures: fbclid, gclid, ttclid, sc (ScCid), _fbp, _ttp, utm_*
 * - stores session id cookie and a captured flag to avoid repeated posts
 */

(function () {
    const API_ENDPOINT = '/api/marketing/attribution';
    const CAPTURE_COOKIE = 'ashare_attrib_sent';
    const SESSION_COOKIE = 'ashare_session';
    const LS_SESSION_KEY = 'ashare_session_ls';
    const COOKIE_PATH = '/';
    const COOKIE_TTL_DAYS = 30;

    function readCookie(name) {
        const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? decodeURIComponent(match[2]) : null;
    }

    function setCookie(name, value, days) {
        const expires = new Date(Date.now() + (days || COOKIE_TTL_DAYS) * 86400000).toUTCString();
        // Use SameSite=Lax for broad compatibility; adjust to SameSite=None; Secure if cross-site and HTTPS
        document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=${COOKIE_PATH}; SameSite=Lax`;
    }

    function getParam(name) {
        try {
            const params = new URLSearchParams(window.location.search);
            return params.get(name) || null;
        } catch {
            return null;
        }
    }

    function uuidv4() {
        if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') return crypto.randomUUID();
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    function readLocalSession() {
        try { return localStorage.getItem(LS_SESSION_KEY); } catch { return null; }
    }
    function writeLocalSession(val) {
        try { localStorage.setItem(LS_SESSION_KEY, val); } catch { }
    }

    async function sendAttribution(payload) {
        try {
            await fetch(API_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Session-Id': payload.sessionId
                },
                credentials: 'include',
                body: JSON.stringify(payload)
            });
        } catch (e) {
            console.warn('[Ashare Attribution] Failed to send', e);
        }
    }

    // Main capture logic
    (function captureOnce() {
        try {
            if (readCookie(CAPTURE_COOKIE)) return;

            // session id: prefer localStorage, then cookie, else generate
            let sessionId = readLocalSession() || readCookie(SESSION_COOKIE);
            if (!sessionId) {
                sessionId = uuidv4();
                writeLocalSession(sessionId);
                setCookie(SESSION_COOKIE, sessionId, COOKIE_TTL_DAYS);
            } else {
                // ensure cookie exists for servers that rely on cookies
                setCookie(SESSION_COOKIE, sessionId, COOKIE_TTL_DAYS);
            }

            const payload = {
                sessionId: sessionId,
                utmSource: getParam('utm_source') || null,
                utmMedium: getParam('utm_medium') || null,
                utmCampaign: getParam('utm_campaign') || null,
                utmContent: getParam('utm_content') || null,
                utmTerm: getParam('utm_term') || null,
                clickId: getParam('fbclid') || getParam('gclid') || getParam('ttclid') || getParam('ScCid') || null,
                referrerUrl: document.referrer || null,
                landingPage: window.location.href || null,
                deviceType: /Mobi|Android/i.test(navigator.userAgent) ? 'mobile' : 'desktop',
                country: null,
                city: null,
                fbp: readCookie('_fbp') || null,
                ttp: readCookie('_ttp') || null,
                ttclid: getParam('ttclid') || null,
                gclid: getParam('gclid') || null,
                fbclid: getParam('fbclid') || null,
                scClickId: getParam('ScCid') || getParam('sc') || null,
                userAgent: navigator.userAgent
            };

            Object.keys(payload).forEach(k => { if (payload[k] === null || payload[k] === '') delete payload[k]; });

            sendAttribution(payload).then(() => {
                setCookie(CAPTURE_COOKIE, '1', 1);
            });
        } catch (e) {
            console.warn('[Ashare Attribution] capture failed', e);
        }
    })();
})();