using Nadena.Shared.Services;

namespace Nadena.Web.Services;

/// <summary>
/// Web implementation of ITrackingConsentService.
/// Browsers have no App Tracking Transparency prompt, so consent stays NotDetermined
/// and the tracking header reflects that. Mirrors the MAUI registration for parity.
/// </summary>
public class WebTrackingConsentService : ITrackingConsentService
{
    public TrackingConsentStatus ConsentStatus { get; private set; } = TrackingConsentStatus.NotDetermined;

    public bool HasRequestedConsent => false;

    public bool IsTrackingAllowed => ConsentStatus == TrackingConsentStatus.Authorized;

    public event Action<TrackingConsentStatus>? ConsentStatusChanged;

    public Task<TrackingConsentStatus> RequestConsentAsync()
    {
        // No ATT prompt on the web; keep current status.
        ConsentStatusChanged?.Invoke(ConsentStatus);
        return Task.FromResult(ConsentStatus);
    }

    public string GetTrackingHeaderValue() => IsTrackingAllowed ? "1" : "0";
}
