namespace ACommerce.Catalog.Simple.Api.DTOs;

/// <summary>
/// ??? ???? ?? ??????
/// </summary>
public class SimpleCategoryDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Image { get; set; }
	public List<SimpleAttributeDto> Attributes { get; set; } = new();
	public DateTime CreatedAt { get; set; }
}

/// <summary>
/// ????? ????? ?? ?????
/// </summary>
public class SimpleAttributeDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<SimpleAttributeValueDto> Values { get; set; } = new();
}

/// <summary>
/// ???? ?????
/// </summary>
public class SimpleAttributeValueDto
{
	public Guid Id { get; set; }
	public string Value { get; set; } = string.Empty;
	public string? ColorHex { get; set; }
	public string? ImageUrl { get; set; }
}

/// <summary>
/// ??? ????? ??? ????
/// </summary>
public class CreateSimpleCategoryRequest
{
	public required string Name { get; set; }
	public string? Description { get; set; }
	public string? Image { get; set; }
	public List<CreateSimpleAttributeRequest>? Attributes { get; set; }
}

/// <summary>
/// ??? ????? ????? ?????
/// </summary>
public class CreateSimpleAttributeRequest
{
	public required string Name { get; set; }
	public List<string>? Values { get; set; }
}

