using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.SharedKernel.CQRS.Commands;

/// <summary>
/// أمر تحديث جزئي لكيان
/// </summary>
public class PartialUpdateCommand<TEntity, TDto> : IRequest<Unit>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// معرف الكيان المراد تحديثه
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// البيانات المراد تحديثها (جزئياً)
	/// </summary>
	public required TDto Data { get; set; }
}
