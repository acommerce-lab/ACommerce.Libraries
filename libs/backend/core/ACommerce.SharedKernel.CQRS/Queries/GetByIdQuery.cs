using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.SharedKernel.CQRS.Queries;

/// <summary>
/// استعلام للحصول على كيان بواسطة المعرف
/// </summary>
public class GetByIdQuery<TEntity, TDto> : IRequest<TDto?>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// معرف الكيان
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// الخصائص المراد تضمينها (Navigation Properties)
	/// </summary>
	public List<string>? IncludeProperties { get; set; }

	/// <summary>
	/// تضمين المحذوفات؟
	/// </summary>
	public bool IncludeDeleted { get; set; } = false;
}
