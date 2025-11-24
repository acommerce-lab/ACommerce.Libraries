namespace ACommerce.Configuration.DTOs;

public class CreateSettingDto
{
	public required string Key { get; set; }
	public required string Value { get; set; }
	public string Scope { get; set; } = "Global";
	public Guid? ScopeId { get; set; }
	public string DataType { get; set; } = "String";
	public string? Description { get; set; }
	public bool IsEncrypted { get; set; }
}

public class SettingResponseDto
{
	public Guid Id { get; set; }
	public string Key { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
	public string Scope { get; set; } = string.Empty;
	public Guid? ScopeId { get; set; }
	public string DataType { get; set; } = string.Empty;
	public string? Description { get; set; }
	public bool IsUserEditable { get; set; }
	public DateTime CreatedAt { get; set; }
}
