using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.SharedKernel.CQRS.Commands;

/// <summary>
/// أمر تحديث كيان موجود
/// </summary>
public class UpdateCommand<TEntity, TDto> : IRequest<Unit>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// معرف الكيان المراد تحديثه
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// البيانات الجديدة
	/// </summary>
	public required TDto Data { get; set; }
}
