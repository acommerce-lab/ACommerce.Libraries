using ACommerce.Client.AppConfig.Services;
using Microsoft.JSInterop;

namespace Ashare.Shared.Services;

/// <summary>
/// يطبّق الـ Theme tokens القادمة من AppConfig snapshot على CSS variables عبر JSInterop،
/// وأيضاً يستجيب لتبديل Light/Dark من ThemeService.
///
/// ينشئ <see cref="ApplyAsync"/> عنصراً واحداً &lt;style id="ashare-remote-theme"&gt; يحوي
/// :root override + .ac-dark override، لذا أي تعديل من DB ينعكس فوراً عند Refresh.
/// </summary>
public sealed class RemoteThemeApplier(
    AppConfigStore store,
    ThemeService theme,
    IJSRuntime js) : IDisposable
{
    private const string StyleElementId = "ashare-remote-theme";
    private bool _subscribed;

    /// <summary>تنشيط — يُستدعى مرة واحدة بعد إقلاع الـ host (مثلاً في MainLayout.OnInitialized).</summary>
    public async Task InitializeAsync()
    {
        if (!_subscribed)
        {
            store.SnapshotChanged += OnSnapshotChanged;
            theme.OnThemeChanged += OnSnapshotChanged;
            _subscribed = true;
        }
        await ApplyAsync();
    }

    private void OnSnapshotChanged() => _ = ApplyAsync();

    public async Task ApplyAsync()
    {
        var snap = store.Current;
        if (snap == null) return;

        // كل التوكنز تُسقَط داخل :root كمتغيرات --ashare-{key}، وداخل .ac-dark للأنماط الداكنة.
        var lightDecls = BuildDeclarations(snap.Theme.GetValueOrDefault("light"));
        var darkDecls  = BuildDeclarations(snap.Theme.GetValueOrDefault("dark"));

        if (string.IsNullOrEmpty(lightDecls) && string.IsNullOrEmpty(darkDecls)) return;

        var css = $":root {{\n{lightDecls}\n}}\n.ac-dark {{\n{darkDecls}\n}}\n";

        try
        {
            await js.InvokeVoidAsync("AshareTheme.applyRemote", StyleElementId, css);
        }
        catch
        {
            // JSInterop may not be ready during prerender — silent fallback to CSS defaults.
        }
    }

    private static string BuildDeclarations(Dictionary<string, string>? bucket)
    {
        if (bucket == null || bucket.Count == 0) return string.Empty;
        return string.Join("\n", bucket.Select(kv => $"  --ashare-{kv.Key}: {kv.Value};"));
    }

    public void Dispose()
    {
        if (_subscribed)
        {
            store.SnapshotChanged -= OnSnapshotChanged;
            theme.OnThemeChanged -= OnSnapshotChanged;
            _subscribed = false;
        }
    }
}
