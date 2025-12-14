using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.SharedKernel.CQRS.Commands;

/// <summary>
/// أمر استعادة كيان محذوف منطقياً
/// </summary>
public class RestoreCommand<TEntity> : IRequest<Unit>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// معرف الكيان المراد استعادته
	/// </summary>
	public Guid Id { get; set; }
}
