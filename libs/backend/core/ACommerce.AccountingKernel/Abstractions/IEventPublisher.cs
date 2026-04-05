namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// ناشر الأحداث - تجريد بدلاً من الاعتماد المباشر على MediatR.
/// يمكن تطبيقه عبر MediatR أو أي ناقل أحداث آخر.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(object evt, CancellationToken cancellationToken = default);
}
