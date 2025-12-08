/**
 * Ashare Analytics - Pixel/SDK Initialization Scripts
 *
 * This file contains the base initialization code for all analytics providers.
 * The actual tracking is done via Blazor JSInterop.
 *
 * IMPORTANT: Replace the placeholder IDs with your actual IDs:
 * - META_PIXEL_ID: Get from https://business.facebook.com/events_manager
 * - GA4_MEASUREMENT_ID: Get from https://analytics.google.com (G-XXXXXXXXXX)
 * - TIKTOK_PIXEL_ID: Get from https://ads.tiktok.com → Assets → Events
 * - SNAPCHAT_PIXEL_ID: Get from https://ads.snapchat.com → Events Manager
 */

// ═══════════════════════════════════════════════════════════════════════════
// Meta (Facebook) Pixel - Base Code
// ═══════════════════════════════════════════════════════════════════════════
!function(f,b,e,v,n,t,s)
{if(f.fbq)return;n=f.fbq=function(){n.callMethod?
n.callMethod.apply(n,arguments):n.queue.push(arguments)};
if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
n.queue=[];t=b.createElement(e);t.async=!0;
t.src=v;s=b.getElementsByTagName(e)[0];
s.parentNode.insertBefore(t,s)}(window, document,'script',
'https://connect.facebook.net/en_US/fbevents.js');

// ═══════════════════════════════════════════════════════════════════════════
// Google Analytics 4 (gtag.js) - Base Code
// ═══════════════════════════════════════════════════════════════════════════
window.dataLayer = window.dataLayer || [];
function gtag(){dataLayer.push(arguments);}
gtag('js', new Date());

// Load gtag.js asynchronously
(function() {
    var gtagScript = document.createElement('script');
    gtagScript.async = true;
    gtagScript.src = 'https://www.googletagmanager.com/gtag/js';
    document.head.appendChild(gtagScript);
})();

// ═══════════════════════════════════════════════════════════════════════════
// TikTok Pixel - Base Code
// ═══════════════════════════════════════════════════════════════════════════
!function (w, d, t) {
    w.TiktokAnalyticsObject=t;var ttq=w[t]=w[t]||[];
    ttq.methods=["page","track","identify","instances","debug","on","off","once","ready","alias","group","enableCookie","disableCookie"];
    ttq.setAndDefer=function(t,e){t[e]=function(){t.push([e].concat(Array.prototype.slice.call(arguments,0)))}};
    for(var i=0;i<ttq.methods.length;i++)ttq.setAndDefer(ttq,ttq.methods[i]);
    ttq.instance=function(t){for(var e=ttq._i[t]||[],n=0;n<ttq.methods.length;n++)ttq.setAndDefer(e,ttq.methods[n]);return e};
    ttq.load=function(e,n){var i="https://analytics.tiktok.com/i18n/pixel/events.js";
    ttq._i=ttq._i||{};ttq._i[e]=[];ttq._i[e]._u=i;ttq._t=ttq._t||{};ttq._t[e]=+new Date;
    ttq._o=ttq._o||{};ttq._o[e]=n||{};
    var o=document.createElement("script");o.type="text/javascript";o.async=!0;
    o.src=i+"?sdkid="+e+"&lib="+t;
    var a=document.getElementsByTagName("script")[0];a.parentNode.insertBefore(o,a)};
}(window, document, 'ttq');

// ═══════════════════════════════════════════════════════════════════════════
// Snapchat Pixel - Base Code
// ═══════════════════════════════════════════════════════════════════════════
(function(e,t,n){
    if(e.snaptr)return;
    var a=e.snaptr=function(){
        a.handleRequest?a.handleRequest.apply(a,arguments):a.queue.push(arguments)
    };
    a.queue=[];
    var s='script';
    r=t.createElement(s);r.async=!0;
    r.src=n;
    var u=t.getElementsByTagName(s)[0];
    u.parentNode.insertBefore(r,u);
})(window,document,'https://sc-static.net/scevent.min.js');

// ═══════════════════════════════════════════════════════════════════════════
// Console log for debugging
// ═══════════════════════════════════════════════════════════════════════════
console.log('[Ashare Analytics] Base scripts loaded. Waiting for initialization...');
