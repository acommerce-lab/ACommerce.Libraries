using ACommerce.OperationEngine.Core;

namespace ACommerce.Persistence.Operations;

/// <summary>
/// مفاتيح العلامات القياسية لمعترض الحفظ.
/// </summary>
public static class PersistenceTags
{
    /// <summary>مفتاح يُعلِن أن القيد يحتاج حفظ كيان.</summary>
    public static readonly TagKey PersistEntity = new("persist_entity");

    /// <summary>مفتاح نوع الكيان (اسم الـ Type بدون namespace).</summary>
    public static readonly TagKey EntityType = new("persist_entity_type");

    /// <summary>مفتاح المفتاح في ctx.Items حيث يوجد الكيان.</summary>
    public static readonly TagKey ContextKey = new("persist_context_key");

    /// <summary>مفتاح نمط الحفظ: add أو update.</summary>
    public static readonly TagKey Mode = new("persist_mode");
}

/// <summary>
/// قيم جاهزة لـ persist_mode.
/// </summary>
public static class PersistenceMode
{
    public static readonly TagValue Add = new("add");
    public static readonly TagValue Update = new("update");
    public static readonly TagValue SoftDelete = new("soft_delete");
}
