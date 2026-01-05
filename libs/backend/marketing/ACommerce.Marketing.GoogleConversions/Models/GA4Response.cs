using System.Text.Json.Serialization;

namespace ACommerce.Marketing.GoogleConversions.Models;

public class GA4ValidationResponse
{
    [JsonPropertyName("validationMessages")]
    public List<GA4ValidationMessage>? ValidationMessages { get; set; }
}

public class GA4ValidationMessage
{
    [JsonPropertyName("fieldPath")]
    public string? FieldPath { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("validationCode")]
    public string? ValidationCode { get; set; }
}
