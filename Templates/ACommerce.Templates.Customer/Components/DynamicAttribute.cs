namespace ACommerce.Templates.Customer.Components;

public class DynamicAttribute
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public List<string>? Values { get; set; }
    public string? SelectedValue { get; set; }
    public List<string>? DisabledValues { get; set; }
    public Dictionary<string, string>? ValueImages { get; set; }
    public AttributeDisplayType Type { get; set; } = AttributeDisplayType.Text;
    public bool IsHighlighted { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public enum AttributeDisplayType
{
    Text,
    ColorSwatch,
    ButtonGroup,
    Dropdown,
    ImageSwatch,
    Checkbox,
    Radio
}

public enum AttributeLayout
{
    List,
    Grid
}
