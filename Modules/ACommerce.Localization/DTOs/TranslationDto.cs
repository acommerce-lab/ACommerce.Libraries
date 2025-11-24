namespace ACommerce.Localization.DTOs;

public class CreateTranslationDto
{
	public required string EntityType { get; set; }
	public required Guid EntityId { get; set; }
	public required string FieldName { get; set; }
	public required string Language { get; set; }
	public required string TranslatedText { get; set; }
}

public class TranslationResponseDto
{
	public Guid Id { get; set; }
	public string EntityType { get; set; } = string.Empty;
	public Guid EntityId { get; set; }
	public string FieldName { get; set; } = string.Empty;
	public string Language { get; set; } = string.Empty;
	public string TranslatedText { get; set; } = string.Empty;
	public bool IsVerified { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class LanguageDto
{
	public Guid Id { get; set; }
	public string Code { get; set; } = string.Empty;
	public string NativeName { get; set; } = string.Empty;
	public string EnglishName { get; set; } = string.Empty;
	public string Direction { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public bool IsDefault { get; set; }
}
