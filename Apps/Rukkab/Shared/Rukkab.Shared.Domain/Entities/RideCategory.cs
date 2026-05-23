using ACommerce.SharedKernel.Abstractions.Entities;

namespace Rukkab.Shared.Domain.Entities;

public class RideCategory : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Domain properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
