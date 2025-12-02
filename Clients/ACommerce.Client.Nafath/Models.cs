namespace ACommerce.Client.Nafath;

#region Requests

/// <summary>
/// طلب بدء مصادقة نفاذ
/// </summary>
public sealed class NafathInitiateRequest
{
    /// <summary>
    /// رقم الهوية الوطنية
    /// </summary>
    public string NationalId { get; set; } = string.Empty;
}

/// <summary>
/// طلب إكمال المصادقة
/// </summary>
public sealed class NafathCompleteRequest
{
    public string TransactionId { get; set; } = string.Empty;
}

#endregion

#region Responses

/// <summary>
/// استجابة بدء المصادقة
/// </summary>
public sealed class NafathInitiateResponse
{
    public bool Success { get; set; }

    /// <summary>
    /// معرف العملية
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// الرقم العشوائي الذي يجب اختياره في تطبيق نفاذ
    /// </summary>
    public string? RandomNumber { get; set; }

    /// <summary>
    /// مدة صلاحية الطلب بالثواني
    /// </summary>
    public int ExpiresInSeconds { get; set; }

    public string? Message { get; set; }
}

/// <summary>
/// استجابة حالة المصادقة
/// </summary>
public sealed class NafathStatusResponse
{
    public string? TransactionId { get; set; }

    /// <summary>
    /// حالة الطلب: pending, completed, expired, rejected
    /// </summary>
    public string Status { get; set; } = "pending";

    public string? Message { get; set; }
}

/// <summary>
/// استجابة إكمال المصادقة
/// </summary>
public sealed class NafathCompleteResponse
{
    public bool Success { get; set; }

    /// <summary>
    /// توكن المصادقة
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// معرف البروفايل
    /// </summary>
    public string? ProfileId { get; set; }

    /// <summary>
    /// الاسم الكامل من نفاذ
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// تاريخ انتهاء التوكن
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    public string? Message { get; set; }
}

#endregion

#region Test (Development Only)

/// <summary>
/// طلب محاكاة webhook نفاذ (للاختبار فقط)
/// </summary>
public sealed class TestWebhookRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// استجابة محاكاة webhook
/// </summary>
public sealed class TestWebhookResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

#endregion
