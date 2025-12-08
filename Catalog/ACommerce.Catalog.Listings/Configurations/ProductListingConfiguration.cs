using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ACommerce.Catalog.Listings.Entities;

namespace ACommerce.Catalog.Listings.Configurations;

public class ProductListingConfiguration : IEntityTypeConfiguration<ProductListing>
{
        public void Configure(EntityTypeBuilder<ProductListing> builder)
        {
                builder.HasIndex(x => x.IsDeleted);
                builder.HasIndex(x => x.IsActive);
                builder.HasIndex(x => x.Status);
                builder.HasIndex(x => x.VendorId);
                builder.HasIndex(x => x.ProductId);
                builder.HasIndex(x => x.CategoryId);
                builder.HasIndex(x => x.CreatedAt);
                builder.HasIndex(x => new { x.IsActive, x.Status, x.IsDeleted });
                builder.HasIndex(x => new { x.VendorId, x.IsActive, x.IsDeleted });
                builder.HasIndex(x => new { x.CategoryId, x.IsActive, x.Status });
        }
}
