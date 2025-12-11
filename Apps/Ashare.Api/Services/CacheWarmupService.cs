using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text;
using System.Text.Json;

namespace Ashare.Api.Services;

public class CacheWarmupService(
    IServiceProvider serviceProvider,
    IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        try
        {
            var baseUrl = configuration["HostSettings:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("SERVICE_URL")
                ?? "http://localhost:3000";
            
            Log.Information("Starting cache warm-up at {BaseUrl}...", baseUrl);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            
            var tasks = new List<Task>
            {
                WarmUpEndpoint(httpClient, $"{baseUrl}/api/listings/featured?limit=10", "Featured", stoppingToken),
                WarmUpEndpoint(httpClient, $"{baseUrl}/api/listings/new?limit=10", "New", stoppingToken),
                WarmUpEndpoint(httpClient, $"{baseUrl}/api/listings?limit=50", "All", stoppingToken),
                WarmUpSearchEndpoint(httpClient, baseUrl, stoppingToken)
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await client.GetStringAsync(url, ct);
            stopwatch.Stop();
            Log.Information("{Name} listings cache warmed up in {ElapsedMs}ms", name, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to warm up {Name} cache", name);
        }
    }
    
    private static async Task WarmUpSearchEndpoint(HttpClient client, string baseUrl, CancellationToken ct)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var filters = new[]
            {
                new { propertyName = "IsActive", value = (object)true, @operator = "Equals" },
                new { propertyName = "Status", value = (object)1, @operator = "Equals" }
            };

            var searchRequest = new
            {
                pageNumber = 1,
                pageSize = 50,
                filters = filters,
                orderBy = "CreatedAt",
                ascending = false
            };

            var json = JsonSerializer.Serialize(searchRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/listings/search", content, ct);
            response.EnsureSuccessStatusCode();

            stopwatch.Stop();
            Log.Information("Search listings cache warmed up in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to warm up Search cache");
        }
    }
}
