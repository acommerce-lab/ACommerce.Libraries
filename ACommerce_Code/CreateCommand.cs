using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.SharedKernel.CQRS.Commands;

/// <summary>
/// أمر إنشاء كيان جديد
/// </summary>
public class CreateCommand<TEntity, TDto> : IRequest<TEntity>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// البيانات المطلوبة لإنشاء الكيان
	/// </summary>
	public required TDto Data { get; set; }
}
