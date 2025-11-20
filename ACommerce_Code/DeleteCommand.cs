using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.SharedKernel.CQRS.Commands;

/// <summary>
/// أمر حذف كيان
/// </summary>
public class DeleteCommand<TEntity> : IRequest<Unit>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// معرف الكيان المراد حذفه
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// حذف منطقي (Soft Delete) أم نهائي (Hard Delete)
	/// </summary>
	public bool SoftDelete { get; set; } = true;
}
