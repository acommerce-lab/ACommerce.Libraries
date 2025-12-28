using Microsoft.Extensions.Logging;

namespace Ashare.App.Services.Attribution;

/// <summary>
/// Default implementation for platforms that don't require ATT (Android, Windows, etc.)
/// Always allows tracking
/// </summary>
public class DefaultTrackingPermissionService : ITrackingPermissionService
{
    private readonly ILogger<DefaultTrackingPermissionService> _logger;

    public DefaultTrackingPermissionService(ILogger<DefaultTrackingPermissionService> logger)
    {
        _logger = logger;
    }

    public bool RequiresPermissionRequest => false;

    public Task<TrackingPermissionStatus> GetStatusAsync()
    {
        return Task.FromResult(TrackingPermissionStatus.NotRequired);
    }

    public Task<bool> RequestPermissionAsync()
    {
        _logger.LogDebug("[Tracking] Permission not required on this platform");
        return Task.FromResult(true);
    }

    public Task<bool> IsTrackingAllowedAsync()
    {
        return Task.FromResult(true);
    }
}
