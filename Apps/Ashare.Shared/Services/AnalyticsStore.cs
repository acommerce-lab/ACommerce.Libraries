using System.Collections.Concurrent;

namespace Ashare.Shared.Services;

/// <summary>
/// In-memory store for analytics events
/// Provides real-time access to tracked events for dashboard display
/// </summary>
public class AnalyticsStore
{
    private readonly ConcurrentQueue<AnalyticsEvent> _events = new();
    private readonly ConcurrentDictionary<string, int> _eventCounts = new();
    private readonly ConcurrentDictionary<string, int> _screenViews = new();
    private readonly ConcurrentDictionary<string, int> _searchTerms = new();
    private readonly ConcurrentDictionary<string, decimal> _revenue = new();

    private const int MaxEvents = 1000;

    public int TotalEvents => _events.Count;
    public int TotalScreenViews => _screenViews.Values.Sum();
    public int TotalSearches => _searchTerms.Values.Sum();
    public decimal TotalRevenue => _revenue.Values.Sum();
    public int UniqueUsers { get; private set; }
    public int TotalPurchases { get; private set; }
    public int TotalRegistrations { get; private set; }
    public int TotalLogins { get; private set; }
    public int TotalContentViews { get; private set; }
    public int TotalWishlistAdds { get; private set; }
    public int TotalShares { get; private set; }

    private readonly HashSet<string> _userIds = new();
    private readonly object _lock = new();

    /// <summary>
    /// Add a new analytics event
    /// </summary>
    public void AddEvent(AnalyticsEvent evt)
    {
        _events.Enqueue(evt);

        // Keep only last MaxEvents
        while (_events.Count > MaxEvents && _events.TryDequeue(out _)) { }

        // Update counts
        _eventCounts.AddOrUpdate(evt.EventType, 1, (_, count) => count + 1);

        // Track specific event types
        switch (evt.EventType)
        {
            case "screen_view":
                var screenName = evt.Parameters?.GetValueOrDefault("screen_name")?.ToString() ?? "unknown";
                _screenViews.AddOrUpdate(screenName, 1, (_, count) => count + 1);
                break;

            case "search":
                var term = evt.Parameters?.GetValueOrDefault("search_term")?.ToString() ?? "unknown";
                _searchTerms.AddOrUpdate(term, 1, (_, count) => count + 1);
                break;

            case "purchase":
                TotalPurchases++;
                if (evt.Parameters?.TryGetValue("value", out var value) == true && value is decimal revenue)
                {
                    var currency = evt.Parameters.GetValueOrDefault("currency")?.ToString() ?? "SAR";
                    _revenue.AddOrUpdate(currency, revenue, (_, total) => total + revenue);
                }
                break;

            case "registration":
                TotalRegistrations++;
                break;

            case "login":
                TotalLogins++;
                break;

            case "content_view":
                TotalContentViews++;
                break;

            case "add_to_wishlist":
                TotalWishlistAdds++;
                break;

            case "share":
                TotalShares++;
                break;
        }
    }

    /// <summary>
    /// Track a user ID
    /// </summary>
    public void TrackUser(string userId)
    {
        lock (_lock)
        {
            if (_userIds.Add(userId))
            {
                UniqueUsers++;
            }
        }
    }

    /// <summary>
    /// Get all events
    /// </summary>
    public IEnumerable<AnalyticsEvent> GetEvents(int limit = 100)
    {
        return _events.TakeLast(limit).Reverse();
    }

    /// <summary>
    /// Get events by type
    /// </summary>
    public IEnumerable<AnalyticsEvent> GetEventsByType(string eventType, int limit = 50)
    {
        return _events.Where(e => e.EventType == eventType).TakeLast(limit).Reverse();
    }

    /// <summary>
    /// Get event counts by type
    /// </summary>
    public Dictionary<string, int> GetEventCounts()
    {
        return new Dictionary<string, int>(_eventCounts);
    }

    /// <summary>
    /// Get screen view counts
    /// </summary>
    public Dictionary<string, int> GetScreenViews()
    {
        return new Dictionary<string, int>(_screenViews);
    }

    /// <summary>
    /// Get top search terms
    /// </summary>
    public Dictionary<string, int> GetTopSearchTerms(int limit = 10)
    {
        return _searchTerms
            .OrderByDescending(x => x.Value)
            .Take(limit)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Get revenue by currency
    /// </summary>
    public Dictionary<string, decimal> GetRevenueByCurrency()
    {
        return new Dictionary<string, decimal>(_revenue);
    }

    /// <summary>
    /// Get events from today
    /// </summary>
    public IEnumerable<AnalyticsEvent> GetTodayEvents()
    {
        var today = DateTime.UtcNow.Date;
        return _events.Where(e => e.Timestamp.Date == today);
    }

    /// <summary>
    /// Get hourly event distribution for today
    /// </summary>
    public Dictionary<int, int> GetHourlyDistribution()
    {
        var today = DateTime.UtcNow.Date;
        return _events
            .Where(e => e.Timestamp.Date == today)
            .GroupBy(e => e.Timestamp.Hour)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Clear all stored data
    /// </summary>
    public void Clear()
    {
        while (_events.TryDequeue(out _)) { }
        _eventCounts.Clear();
        _screenViews.Clear();
        _searchTerms.Clear();
        _revenue.Clear();

        lock (_lock)
        {
            _userIds.Clear();
            UniqueUsers = 0;
        }

        TotalPurchases = 0;
        TotalRegistrations = 0;
        TotalLogins = 0;
        TotalContentViews = 0;
        TotalWishlistAdds = 0;
        TotalShares = 0;
    }

    /// <summary>
    /// Get summary statistics
    /// </summary>
    public AnalyticsSummary GetSummary()
    {
        return new AnalyticsSummary
        {
            TotalEvents = TotalEvents,
            UniqueUsers = UniqueUsers,
            TotalScreenViews = TotalScreenViews,
            TotalSearches = TotalSearches,
            TotalPurchases = TotalPurchases,
            TotalRevenue = TotalRevenue,
            TotalRegistrations = TotalRegistrations,
            TotalLogins = TotalLogins,
            TotalContentViews = TotalContentViews,
            TotalWishlistAdds = TotalWishlistAdds,
            TotalShares = TotalShares,
            EventCounts = GetEventCounts(),
            TopScreens = GetScreenViews().OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value),
            TopSearchTerms = GetTopSearchTerms(5),
            RevenueByCurrency = GetRevenueByCurrency()
        };
    }
}

/// <summary>
/// Single analytics event
/// </summary>
public class AnalyticsEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string? EventName { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Analytics summary for dashboard
/// </summary>
public class AnalyticsSummary
{
    public int TotalEvents { get; set; }
    public int UniqueUsers { get; set; }
    public int TotalScreenViews { get; set; }
    public int TotalSearches { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalLogins { get; set; }
    public int TotalContentViews { get; set; }
    public int TotalWishlistAdds { get; set; }
    public int TotalShares { get; set; }
    public Dictionary<string, int> EventCounts { get; set; } = new();
    public Dictionary<string, int> TopScreens { get; set; } = new();
    public Dictionary<string, int> TopSearchTerms { get; set; } = new();
    public Dictionary<string, decimal> RevenueByCurrency { get; set; } = new();
}
