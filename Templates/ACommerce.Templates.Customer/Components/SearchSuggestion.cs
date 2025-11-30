namespace ACommerce.Templates.Customer.Components;

public class SearchSuggestion
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsHistory { get; set; }
    public string? Category { get; set; }
    public int ResultCount { get; set; }
}
