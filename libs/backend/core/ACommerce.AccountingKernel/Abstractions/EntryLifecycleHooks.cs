namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// أحداث دورة حياة القيد - MVVM-style.
/// يمكن ربط أي وظيفة خارجية بأي مرحلة من دورة الحياة.
///
/// الترتيب:
///   BeforeValidate → Validate → AfterValidate
///   → BeforeExecute → Execute → AfterExecute
///   → [SubEntries]
///   → BeforePostValidate → PostValidate → AfterPostValidate
///   → BeforeComplete/BeforeFail → OnCompleted/OnFailed → AfterComplete/AfterFail
///
/// عند الخطأ:
///   BeforeError → OnError → AfterError
/// </summary>
public class EntryLifecycleHooks
{
    // === Validate ===
    public Func<EntryContext, Task>? BeforeValidate { get; set; }
    public Func<EntryContext, Task>? AfterValidate { get; set; }

    // === Execute ===
    public Func<EntryContext, Task>? BeforeExecute { get; set; }
    public Func<EntryContext, Task>? AfterExecute { get; set; }

    // === SubEntries ===
    public Func<EntryContext, Task>? BeforeSubEntries { get; set; }
    public Func<EntryContext, Task>? AfterSubEntries { get; set; }

    // === PostValidate ===
    public Func<EntryContext, Task>? BeforePostValidate { get; set; }
    public Func<EntryContext, Task>? AfterPostValidate { get; set; }

    // === Complete ===
    public Func<EntryContext, Task>? BeforeComplete { get; set; }
    public Func<EntryContext, Task>? AfterComplete { get; set; }

    // === Fail ===
    public Func<EntryContext, Task>? BeforeFail { get; set; }
    public Func<EntryContext, Task>? AfterFail { get; set; }

    // === Error (exception) ===
    public Func<EntryContext, Exception, Task>? BeforeError { get; set; }
    public Func<EntryContext, Exception, Task>? AfterError { get; set; }

    /// <summary>
    /// تنفيذ hook إن وُجد (مع حماية من null)
    /// </summary>
    internal async Task InvokeAsync(Func<EntryContext, Task>? hook, EntryContext ctx)
    {
        if (hook != null) await hook(ctx);
    }

    internal async Task InvokeAsync(Func<EntryContext, Exception, Task>? hook, EntryContext ctx, Exception ex)
    {
        if (hook != null) await hook(ctx, ex);
    }
}
