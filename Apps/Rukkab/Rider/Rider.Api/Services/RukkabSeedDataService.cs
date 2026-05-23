using ACommerce.SharedKernel.Abstractions.Repositories;
using Rukkab.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Rukkab.Rider.Api.Services;

public class RukkabSeedDataService
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<RukkabSeedDataService> _logger;
    private readonly ACommerce.DriverMatching.IDriverMatchingService? _matching;

    public RukkabSeedDataService(IRepositoryFactory repositoryFactory, ILogger<RukkabSeedDataService> logger, ACommerce.DriverMatching.IDriverMatchingService? matching = null)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
        _matching = matching;
    }

    public async Task SeedAsync()
    {
        var repo = _repositoryFactory.CreateRepository<RideCategory>();

        var count = await repo.CountAsync();
        if (count > 0)
        {
            _logger.LogInformation("Rukkab seed: categories already present ({Count}), skipping.", count);
        }
        else
        {
            var categories = new List<RideCategory>
            {
                new RideCategory { Name = "Standard", Description = "Regular rides" },
                new RideCategory { Name = "Shared", Description = "Carpool / shared rides" },
                new RideCategory { Name = "Premium", Description = "Higher-end vehicles" }
            };

            await repo.AddRangeAsync(categories);
            _logger.LogInformation("Rukkab seed: added {Count} ride categories", categories.Count);
        }

        // Seed driver locations into the in-memory matching service if available (development)
        if (_matching != null)
        {
            _logger.LogInformation("Rukkab seed: seeding drivers into matching service (dev).");

            await _matching.UpdateDriverLocationAsync("driver-1", new ACommerce.DriverMatching.Models.GeoPoint(40.72, -73.995), ACommerce.DriverMatching.Models.DriverStatus.Available);
            await _matching.UpdateDriverLocationAsync("driver-2", new ACommerce.DriverMatching.Models.GeoPoint(40.721, -73.994), ACommerce.DriverMatching.Models.DriverStatus.Available);
            await _matching.UpdateDriverLocationAsync("driver-3", new ACommerce.DriverMatching.Models.GeoPoint(40.719, -73.996), ACommerce.DriverMatching.Models.DriverStatus.Busy);
        }
        else
        {
            _logger.LogDebug("Rukkab seed: driver matching service not registered; skipping driver seeds.");
        }
    }
}
