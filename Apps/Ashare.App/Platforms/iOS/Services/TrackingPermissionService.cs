#if IOS
using AppTrackingTransparency;
using Foundation;
using Ashare.App.Services.Attribution;
using Microsoft.Extensions.Logging;

namespace Ashare.App.Platforms.iOS.Services;

/// <summary>
/// iOS implementation of App Tracking Transparency (ATT)
/// Required for iOS 14.5+ to request permission before tracking
/// </summary>
public class TrackingPermissionService : ITrackingPermissionService
{
    private readonly ILogger<TrackingPermissionService> _logger;

    public TrackingPermissionService(ILogger<TrackingPermissionService> logger)
    {
        _logger = logger;
    }

    public bool RequiresPermissionRequest =>
        UIKit.UIDevice.CurrentDevice.CheckSystemVersion(14, 5);

    public async Task<TrackingPermissionStatus> GetStatusAsync()
    {
        if (!RequiresPermissionRequest)
        {
            return TrackingPermissionStatus.NotRequired;
        }

        var status = ATTrackingManager.TrackingAuthorizationStatus;
        return MapStatus(status);
    }

    public async Task<bool> RequestPermissionAsync()
    {
        if (!RequiresPermissionRequest)
        {
            _logger.LogDebug("[ATT] iOS version < 14.5, permission not required");
            return true;
        }

        var currentStatus = ATTrackingManager.TrackingAuthorizationStatus;

        if (currentStatus != ATTrackingManagerAuthorizationStatus.NotDetermined)
        {
            _logger.LogDebug("[ATT] Permission already determined: {Status}", currentStatus);
            return currentStatus == ATTrackingManagerAuthorizationStatus.Authorized;
        }

        try
        {
            _logger.LogInformation("[ATT] Requesting tracking permission...");

            var status = await ATTrackingManager.RequestTrackingAuthorizationAsync();

            _logger.LogInformation("[ATT] User response: {Status}", status);

            return status == ATTrackingManagerAuthorizationStatus.Authorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ATT] Error requesting permission");
            return false;
        }
    }

    public async Task<bool> IsTrackingAllowedAsync()
    {
        var status = await GetStatusAsync();
        return status == TrackingPermissionStatus.Authorized ||
               status == TrackingPermissionStatus.NotRequired;
    }

    private static TrackingPermissionStatus MapStatus(ATTrackingManagerAuthorizationStatus status)
    {
        return status switch
        {
            ATTrackingManagerAuthorizationStatus.NotDetermined => TrackingPermissionStatus.NotDetermined,
            ATTrackingManagerAuthorizationStatus.Restricted => TrackingPermissionStatus.Restricted,
            ATTrackingManagerAuthorizationStatus.Denied => TrackingPermissionStatus.Denied,
            ATTrackingManagerAuthorizationStatus.Authorized => TrackingPermissionStatus.Authorized,
            _ => TrackingPermissionStatus.NotDetermined
        };
    }
}
#endif
