namespace Restaurant.Driver.App.Services;

/// <summary>
/// خدمة تتبع موقع السائق
/// </summary>
public class LocationService
{
    private CancellationTokenSource? _locationCts;
    private bool _isTracking;

    public event Action<Location>? OnLocationChanged;

    public bool IsTracking => _isTracking;

    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);
            return location;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocationService] Error getting location: {ex.Message}");
            return null;
        }
    }

    public async Task StartTrackingAsync(TimeSpan interval)
    {
        if (_isTracking) return;

        _isTracking = true;
        _locationCts = new CancellationTokenSource();

        Console.WriteLine("[LocationService] Started tracking");

        try
        {
            while (!_locationCts.Token.IsCancellationRequested)
            {
                var location = await GetCurrentLocationAsync();
                if (location != null)
                {
                    OnLocationChanged?.Invoke(location);
                }

                await Task.Delay(interval, _locationCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[LocationService] Tracking cancelled");
        }
        finally
        {
            _isTracking = false;
        }
    }

    public void StopTracking()
    {
        _locationCts?.Cancel();
        _isTracking = false;
        Console.WriteLine("[LocationService] Stopped tracking");
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
    {
        var loc1 = new Location(lat1, lon1);
        var loc2 = new Location(lat2, lon2);
        return Location.CalculateDistance(loc1, loc2, DistanceUnits.Kilometers);
    }
}
