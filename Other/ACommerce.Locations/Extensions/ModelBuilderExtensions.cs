using ACommerce.Locations.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Locations.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// إضافة تكوينات كيانات المواقع إلى DbContext
    /// </summary>
    public static ModelBuilder ApplyLocationsConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new RegionConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new NeighborhoodConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());

        return modelBuilder;
    }
}
