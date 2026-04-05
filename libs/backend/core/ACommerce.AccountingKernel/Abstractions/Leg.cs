namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// التطبيق الافتراضي لطرف القيد
/// </summary>
public class Leg : ILeg
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Party { get; }
    public LegDirection Direction { get; }
    public string ResourceType { get; }
    public decimal Value { get; }
    public LegStatus Status { get; set; } = LegStatus.Pending;
    public Dictionary<string, object> Payload { get; } = new();

    public Leg(string party, LegDirection direction, string resourceType, decimal value = 1m)
    {
        Party = party;
        Direction = direction;
        ResourceType = resourceType;
        Value = value;
    }

    /// <summary>
    /// إضافة كيان للحمولة - يُحفظ عبر SharedKernel عند التنفيذ
    /// </summary>
    public Leg WithEntity(string key, object entity)
    {
        Payload[key] = entity;
        return this;
    }

    /// <summary>
    /// إضافة بيانات وصفية
    /// </summary>
    public Leg WithData(string key, object value)
    {
        Payload[key] = value;
        return this;
    }
}
