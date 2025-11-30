namespace ACommerce.Templates.Customer.Pages;

/// <summary>
/// نموذج الفئة لصفحة إنشاء العرض
/// </summary>
public class ListingCategory
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Icon { get; set; }
	public string? Image { get; set; }
}

/// <summary>
/// نموذج تعريف الخاصية من الباك اند
/// </summary>
public class ListingAttributeDefinition
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public string Type { get; set; } = "Text";
	public string? Description { get; set; }
	public bool IsRequired { get; set; }
	public bool IsFilterable { get; set; }
	public bool IsVisibleInList { get; set; }
	public bool IsVisibleInDetail { get; set; }
	public int SortOrder { get; set; }
	public string? ValidationRules { get; set; }
	public string? DefaultValue { get; set; }
	public List<ListingAttributeValue> Values { get; set; } = new();
}

/// <summary>
/// قيمة محددة للخاصية
/// </summary>
public class ListingAttributeValue
{
	public Guid Id { get; set; }
	public string Value { get; set; } = string.Empty;
	public string? DisplayName { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? ColorHex { get; set; }
	public string? ImageUrl { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; } = true;
}

/// <summary>
/// قيمة خاصية مدخلة من المستخدم
/// </summary>
public class ListingAttributeInput
{
	public Guid AttributeId { get; set; }
	public string Code { get; set; } = string.Empty;
	public string? TextValue { get; set; }
	public decimal? NumberValue { get; set; }
	public bool? BooleanValue { get; set; }
	public DateTime? DateValue { get; set; }
	public List<string>? MultiSelectValues { get; set; }
}

/// <summary>
/// نموذج إنشاء العرض
/// </summary>
public class CreateListingModel
{
	public Guid CategoryId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public List<ListingAttributeInput> Attributes { get; set; } = new();
	public List<string> Images { get; set; } = new();
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Address { get; set; }
}

/// <summary>
/// حالة صفحة إنشاء العرض
/// </summary>
public enum CreateListingStep
{
	SelectCategory,
	FillDetails,
	AddImages,
	SetLocation,
	Review,
	Submitting,
	Success,
	Error
}
