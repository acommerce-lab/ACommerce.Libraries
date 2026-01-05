// add near other handler mappings
#if ANDROID
using Android.Webkit;
Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("EnableCookies", (handler, view) =>
{
    var webView = handler.PlatformView;
    CookieManager.Instance.SetAcceptCookie(true);
    CookieManager.Instance.SetAcceptThirdPartyCookies(webView, true);
});
#endif

#if IOS
using WebKit;
Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("UseDefaultDataStore", (handler, view) =>
{
    try
    {
        handler.PlatformView.Configuration.WebsiteDataStore = WKWebsiteDataStore.DefaultDataStore;
    }
    catch { }
});
#endif