using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ashare.Api.Services;

public class CacheWarmupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public CacheWarmupService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        try
        {
            var baseUrl = _configuration["HostSettings:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("SERVICE_URL")
                ?? "http://localhost:3000";
            
            Log.Information("Starting cache warm-up at {BaseUrl}...", baseUrl);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            
            var tasks = new[]
            {
                WarmUpEndpoint(httpClient, $"{baseUrl}/api/listings/featured?limit=10", "Featured", stoppingToken),
                WarmUpEndpoint(httpClient, $"{baseUrl}/api/listings/new?limit=10", "New", stoppingToken),
                WarmUpEndpoint(httpClient, $"{baseUrl}/api/listings?limit=50", "All", stoppingToken)
            };
            
            await Task.WhenAll(tasks);
            
            Log.Information("Cache warm-up completed successfully!");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Cache warm-up failed (non-critical)");
        }
    }
    
    private static async Task WarmUpEndpoint(HttpClient client, string url, string name, CancellationToken ct)
    {
        try
        {
            await client.GetStringAsync(url, ct);
            Log.Information("{Name} listings cache warmed up", name);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to warm up {Name} cache", name);
        }
    }
}
