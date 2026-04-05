namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// سياق تنفيذ القيد - يُمرر لكل دوال التحقق والتنفيذ.
/// يحمل معلومات القيد الحالي والقيد الأب ومخزن مؤقت للبيانات.
/// </summary>
public class EntryContext
{
    /// <summary>
    /// القيد الجاري تنفيذه
    /// </summary>
    public IEntry Entry { get; }

    /// <summary>
    /// القيد الأب (إن كان هذا قيداً فرعياً)
    /// </summary>
    public IEntry? ParentEntry { get; internal set; }

    /// <summary>
    /// مخزن مؤقت لتبادل البيانات بين مراحل دورة الحياة
    /// </summary>
    public Dictionary<string, object> Items { get; } = new();

    /// <summary>
    /// مزود الخدمات (للحصول على Repository أو أي خدمة)
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// رمز الإلغاء
    /// </summary>
    public CancellationToken CancellationToken { get; }

    public EntryContext(IEntry entry, IServiceProvider services, CancellationToken cancellationToken = default)
    {
        Entry = entry;
        Services = services;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// الحصول على خدمة من DI
    /// </summary>
    public T GetService<T>() where T : notnull
        => (T)Services.GetService(typeof(T))!;

    /// <summary>
    /// تخزين قيمة مؤقتة (تنتقل بين Validate → Execute → PostValidate)
    /// </summary>
    public void Set<T>(string key, T value) => Items[key] = value!;

    /// <summary>
    /// استرجاع قيمة مؤقتة
    /// </summary>
    public T Get<T>(string key) => (T)Items[key];

    /// <summary>
    /// محاولة استرجاع قيمة
    /// </summary>
    public bool TryGet<T>(string key, out T? value)
    {
        if (Items.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// إضافة خطأ تحقق واحد (يُجمع تلقائياً في EntryResult.ValidationErrors)
    /// </summary>
    public void AddValidationError(string error)
    {
        if (!Items.ContainsKey("_validationErrors"))
            Items["_validationErrors"] = new List<string>();
        ((List<string>)Items["_validationErrors"]).Add(error);
        Items["_validationError"] = error; // آخر خطأ كرسالة رئيسية
    }

    /// <summary>
    /// إضافة خطأ تحقق مرتبط بحقل (field: message)
    /// </summary>
    public void AddValidationError(string field, string message)
        => AddValidationError($"{field}: {message}");

    /// <summary>
    /// تنظيف البيانات الكبيرة من السياق بعد الاستخدام (لتوفير الذاكرة)
    /// </summary>
    public void Release(string key) => Items.Remove(key);
}
