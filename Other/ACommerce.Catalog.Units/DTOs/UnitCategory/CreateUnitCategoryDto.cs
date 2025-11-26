namespace ACommerce.Catalog.Units.DTOs.UnitCategory;

// DTOs ?????? ??? UnitCategory
public class CreateUnitCategoryDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public Guid? BaseUnitId { get; set; }
    public int SortOrder { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
