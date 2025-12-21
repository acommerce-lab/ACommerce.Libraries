using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Marketplace.GCP.Controllers;

/// <summary>
/// Health check controller for load balancers and monitoring
/// </summary>
[ApiController]
[AllowAnonymous]
public class HealthzController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public HealthzController(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Simple health check endpoint for load balancers
    /// </summary>
    [HttpGet("/healthz")]
    public async Task<IActionResult> Get()
    {
        var dbStatus = "Unknown";
        var dbProvider = "Unknown";
        var dbError = (string?)null;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            if (dbContext != null)
            {
                dbProvider = dbContext.Database.ProviderName ?? "Unknown";
                var canConnect = await dbContext.Database.CanConnectAsync();
                dbStatus = canConnect ? "Connected" : "Disconnected";
            }
            else
            {
                dbStatus = "DbContext not found";
            }
        }
        catch (Exception ex)
        {
            dbStatus = "Error";
            dbError = ex.Message;
        }

        return Ok(new
        {
            Status = "Healthy",
            Service = "ACommerce Marketplace API",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow,
            Environment = new
            {
                ASPNETCORE_ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set",
                HasDatabaseConnectionString = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")),
                HasRedisConnectionString = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")),
                PORT = Environment.GetEnvironmentVariable("PORT") ?? "Not set",
                K_SERVICE = Environment.GetEnvironmentVariable("K_SERVICE") ?? "Not set",
                K_REVISION = Environment.GetEnvironmentVariable("K_REVISION") ?? "Not set"
            },
            Database = new
            {
                Status = dbStatus,
                Provider = dbProvider,
                Error = dbError
            },
            Runtime = new
            {
                DotNetVersion = Environment.Version.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = $"{Environment.WorkingSet / 1024 / 1024} MB"
            }
        });
    }

    /// <summary>
    /// Debug endpoint to list all registered routes
    /// </summary>
    [HttpGet("/healthz/routes")]
    public IActionResult GetRoutes([FromServices] IEnumerable<EndpointDataSource> endpointSources)
    {
        var routes = endpointSources
            .SelectMany(es => es.Endpoints)
            .Select(e => new
            {
                Route = (e as RouteEndpoint)?.RoutePattern?.RawText ?? "N/A",
                DisplayName = e.DisplayName,
                Methods = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? Array.Empty<string>()
            })
            .OrderBy(r => r.Route)
            .ToList();

        return Ok(new
        {
            TotalRoutes = routes.Count,
            Routes = routes
        });
    }
}
