using ACommerce.Catalog.Attributes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Attributes.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
	public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
	{
		builder.ToTable("AttributeDefinitions");

		builder.HasKey(e => e.Id);

		// منع EF Core من توليد ID تلقائياً - استخدم ID المحدد
		builder.Property(e => e.Id).ValueGeneratedNever();

		builder.Property(e => e.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(e => e.Code)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(e => e.Type)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(e => e.Description)
			.HasMaxLength(1000);

		builder.Property(e => e.ValidationRules)
			.HasMaxLength(2000);

		builder.Property(e => e.DefaultValue)
			.HasMaxLength(500);

		// Indexes
		builder.HasIndex(e => e.Code).IsUnique();
		builder.HasIndex(e => e.Name);
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasMany(e => e.Values)
			.WithOne(v => v.AttributeDefinition)
			.HasForeignKey(v => v.AttributeDefinitionId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
