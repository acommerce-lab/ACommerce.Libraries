using Foundation;
using UIKit;

namespace Ashare.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    /// <summary>
    /// Handle Universal Links (iOS 9+)
    /// </summary>
    public override bool ContinueUserActivity(
        UIApplication application,
        NSUserActivity userActivity,
        UIApplicationRestorationHandler completionHandler)
    {
        if (userActivity.ActivityType == NSUserActivityType.BrowsingWeb)
        {
            var url = userActivity.WebPageUrl;
            if (url != null)
            {
                HandleDeepLink(url.ToString());
            }
        }

        return base.ContinueUserActivity(application, userActivity, completionHandler);
    }

    /// <summary>
    /// Handle Custom URL Scheme (ashare://)
    /// </summary>
    public override bool OpenUrl(
        UIApplication application,
        NSUrl url,
        NSDictionary options)
    {
        HandleDeepLink(url.ToString());
        return base.OpenUrl(application, url, options);
    }

    /// <summary>
    /// Handle deep link
    /// </summary>
    private void HandleDeepLink(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[DeepLink iOS] Received: {url}");

            // Navigate to the deep link path
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath;
                if (!string.IsNullOrEmpty(path) && path != "/")
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Navigate using Shell or your navigation service
                        // Shell.Current?.GoToAsync(path);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DeepLink iOS] Error handling: {ex.Message}");
        }
    }
}
