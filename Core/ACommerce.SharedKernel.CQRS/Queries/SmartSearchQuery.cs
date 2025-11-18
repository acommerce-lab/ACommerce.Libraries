using MediatR;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Queries;

namespace ACommerce.SharedKernel.CQRS.Queries;

/// <summary>
/// استعلام البحث الذكي
/// </summary>
public class SmartSearchQuery<TEntity, TDto> : IRequest<PagedResult<TDto>>
	where TEntity : class, IBaseEntity
{
	/// <summary>
	/// طلب البحث
	/// </summary>
	public SmartSearchRequest Request { get; set; } = new();
}
