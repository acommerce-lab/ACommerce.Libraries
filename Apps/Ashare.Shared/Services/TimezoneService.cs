using Microsoft.JSInterop;

namespace Ashare.Shared.Services;

/// <summary>
/// Service for handling timezone conversions and date/time formatting.
/// Converts UTC timestamps to user's local timezone for display.
/// </summary>
public interface ITimezoneService
{
    /// <summary>
    /// Gets the user's current timezone offset in minutes from UTC.
    /// </summary>
    Task<int> GetTimezoneOffsetMinutesAsync();

    /// <summary>
    /// Converts a UTC DateTime to the user's local time.
    /// </summary>
    Task<DateTime> ToLocalTimeAsync(DateTime utcDateTime);

    /// <summary>
    /// Converts a local DateTime to UTC.
    /// </summary>
    Task<DateTime> ToUtcAsync(DateTime localDateTime);

    /// <summary>
    /// Formats a UTC DateTime for display in the user's local timezone.
    /// </summary>
    Task<string> FormatTimeAsync(DateTime utcDateTime, string format = "HH:mm");

    /// <summary>
    /// Formats a relative time string (e.g., "5 minutes ago", "yesterday").
    /// </summary>
    Task<string> FormatRelativeTimeAsync(DateTime utcDateTime, ILocalizationService localization);

    /// <summary>
    /// Gets the user's timezone identifier (e.g., "Asia/Riyadh").
    /// </summary>
    Task<string> GetTimezoneIdAsync();
}

/// <summary>
/// Browser-based timezone service using JavaScript interop.
/// </summary>
public class BrowserTimezoneService : ITimezoneService
{
    private readonly IJSRuntime _jsRuntime;
    private int? _cachedOffset;
    private string? _cachedTimezoneId;

    public BrowserTimezoneService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<int> GetTimezoneOffsetMinutesAsync()
    {
        if (_cachedOffset.HasValue)
            return _cachedOffset.Value;

        try
        {
            // JavaScript's getTimezoneOffset returns offset in minutes (inverted: UTC = local + offset)
            // So we negate it to get the correct offset from UTC
            _cachedOffset = await _jsRuntime.InvokeAsync<int>("eval", "new Date().getTimezoneOffset() * -1");
            return _cachedOffset.Value;
        }
        catch
        {
            // Default to Saudi Arabia timezone (UTC+3 = 180 minutes)
            return 180;
        }
    }

    public async Task<DateTime> ToLocalTimeAsync(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Local)
            return utcDateTime;

        var offsetMinutes = await GetTimezoneOffsetMinutesAsync();
        return utcDateTime.AddMinutes(offsetMinutes);
    }

    public async Task<DateTime> ToUtcAsync(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc)
            return localDateTime;

        var offsetMinutes = await GetTimezoneOffsetMinutesAsync();
        return localDateTime.AddMinutes(-offsetMinutes);
    }

    public async Task<string> FormatTimeAsync(DateTime utcDateTime, string format = "HH:mm")
    {
        var localTime = await ToLocalTimeAsync(utcDateTime);
        return localTime.ToString(format);
    }

    public async Task<string> FormatRelativeTimeAsync(DateTime utcDateTime, ILocalizationService L)
    {
        var localTime = await ToLocalTimeAsync(utcDateTime);
        var now = await ToLocalTimeAsync(DateTime.UtcNow);
        var diff = now - localTime;

        // Same minute
        if (diff.TotalMinutes < 1)
            return L["Now"];

        // Minutes ago
        if (diff.TotalHours < 1)
        {
            var minutes = (int)diff.TotalMinutes;
            return L["MinutesAgo", minutes];
        }

        // Hours ago
        if (diff.TotalDays < 1)
        {
            var hours = (int)diff.TotalHours;
            return L["HoursAgo", hours];
        }

        // Yesterday
        if (diff.TotalDays < 2 && localTime.Date == now.AddDays(-1).Date)
            return L["Yesterday"];

        // Days ago (up to a week)
        if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return L["DaysAgo", days];
        }

        // This year - show month and day
        if (localTime.Year == now.Year)
            return localTime.ToString("MMM d");

        // Older - show full date
        return localTime.ToString("MMM d, yyyy");
    }

    public async Task<string> GetTimezoneIdAsync()
    {
        if (_cachedTimezoneId != null)
            return _cachedTimezoneId;

        try
        {
            _cachedTimezoneId = await _jsRuntime.InvokeAsync<string>(
                "eval",
                "Intl.DateTimeFormat().resolvedOptions().timeZone"
            );
            return _cachedTimezoneId ?? "Asia/Riyadh";
        }
        catch
        {
            return "Asia/Riyadh";
        }
    }
}

/// <summary>
/// MAUI-based timezone service using local device time.
/// </summary>
public class DeviceTimezoneService : ITimezoneService
{
    private readonly TimeZoneInfo _localTimeZone;

    public DeviceTimezoneService()
    {
        _localTimeZone = TimeZoneInfo.Local;
    }

    public Task<int> GetTimezoneOffsetMinutesAsync()
    {
        var offset = _localTimeZone.GetUtcOffset(DateTime.Now);
        return Task.FromResult((int)offset.TotalMinutes);
    }

    public Task<DateTime> ToLocalTimeAsync(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Local)
            return Task.FromResult(utcDateTime);

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _localTimeZone);
        return Task.FromResult(localTime);
    }

    public Task<DateTime> ToUtcAsync(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc)
            return Task.FromResult(localDateTime);

        var utcTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, _localTimeZone);
        return Task.FromResult(utcTime);
    }

    public async Task<string> FormatTimeAsync(DateTime utcDateTime, string format = "HH:mm")
    {
        var localTime = await ToLocalTimeAsync(utcDateTime);
        return localTime.ToString(format);
    }

    public async Task<string> FormatRelativeTimeAsync(DateTime utcDateTime, ILocalizationService L)
    {
        var localTime = await ToLocalTimeAsync(utcDateTime);
        var now = await ToLocalTimeAsync(DateTime.UtcNow);
        var diff = now - localTime;

        if (diff.TotalMinutes < 1)
            return L["Now"];

        if (diff.TotalHours < 1)
        {
            var minutes = (int)diff.TotalMinutes;
            return L["MinutesAgo", minutes];
        }

        if (diff.TotalDays < 1)
        {
            var hours = (int)diff.TotalHours;
            return L["HoursAgo", hours];
        }

        if (diff.TotalDays < 2 && localTime.Date == now.AddDays(-1).Date)
            return L["Yesterday"];

        if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return L["DaysAgo", days];
        }

        if (localTime.Year == now.Year)
            return localTime.ToString("MMM d");

        return localTime.ToString("MMM d, yyyy");
    }

    public Task<string> GetTimezoneIdAsync()
    {
        return Task.FromResult(_localTimeZone.Id);
    }
}
