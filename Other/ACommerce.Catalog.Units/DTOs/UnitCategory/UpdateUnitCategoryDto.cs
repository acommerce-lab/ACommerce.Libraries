namespace ACommerce.Catalog.Units.DTOs.UnitCategory;

public class UpdateUnitCategoryDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? BaseUnitId { get; set; }
    public int SortOrder { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
