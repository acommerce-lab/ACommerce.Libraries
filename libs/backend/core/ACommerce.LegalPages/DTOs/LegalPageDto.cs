namespace ACommerce.LegalPages.DTOs;

public sealed class LegalPageDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateLegalPageRequest
{
    public string Key { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-file-text";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateLegalPageRequest
{
    public string? TitleAr { get; set; }
    public string? TitleEn { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}
