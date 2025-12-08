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
            
            Log.Information("🔥 Starting cache warm-up at {BaseUrl}...", baseUrl);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            
            await httpClient.GetStringAsync($"{baseUrl}/api/listings/featured?limit=10", stoppingToken);
            Log.Information("✅ Featured listings cache warmed up");
            
            await httpClient.GetStringAsync($"{baseUrl}/api/listings/new?limit=10", stoppingToken);
            Log.Information("✅ New listings cache warmed up");
            
            await httpClient.GetStringAsync($"{baseUrl}/api/listings?limit=50", stoppingToken);
            Log.Information("✅ All listings cache warmed up");
            
            Log.Information("🚀 Cache warm-up completed successfully!");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "⚠️ Cache warm-up failed (non-critical)");
        }
    }
}
