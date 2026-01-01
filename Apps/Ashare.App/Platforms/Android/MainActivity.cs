using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Ashare.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    LaunchMode = LaunchMode.SingleTask)]

// Deep Links - Custom Scheme
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "ashare")]

// App Links - HTTPS (requires assetlinks.json on server)
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "ashare.sa",
    AutoVerify = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "www.ashare.sa",
    AutoVerify = true)]

// HTTP fallback for testing
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "http",
    DataHost = "ashare.sa")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Handle deep link on app launch
        HandleDeepLink(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        // Handle deep link when app is already running
        HandleDeepLink(intent);
    }

    private void HandleDeepLink(Intent? intent)
    {
        if (intent?.Action != Intent.ActionView)
            return;

        var uri = intent.Data;
        if (uri == null)
            return;

        try
        {
            var url = uri.ToString();
            System.Diagnostics.Debug.WriteLine($"[DeepLink] Received: {url}");

            // Navigate to the deep link path
            var path = uri.Path;
            if (!string.IsNullOrEmpty(path))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Navigate using Shell or your navigation service
                    // Shell.Current?.GoToAsync(path);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DeepLink] Error handling: {ex.Message}");
        }
    }
}
