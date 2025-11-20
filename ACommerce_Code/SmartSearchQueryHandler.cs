using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج استعلام SmartSearch
/// </summary>
public class SmartSearchQueryHandler<TEntity, TDto> : IRequestHandler<SmartSearchQuery<TEntity, TDto>, PagedResult<TDto>>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<SmartSearchQueryHandler<TEntity, TDto>> _logger;

	public SmartSearchQueryHandler(
		IBaseAsyncRepository<TEntity> repository,
		IMapper mapper,
		ILogger<SmartSearchQueryHandler<TEntity, TDto>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<PagedResult<TDto>> Handle(SmartSearchQuery<TEntity, TDto> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug(
			"Searching {EntityName} with page {PageNumber}, size {PageSize}",
			typeof(TEntity).Name,
			request.Request.PageNumber,
			request.Request.PageSize);

		// التحقق من صحة الطلب
		if (!request.Request.IsValid())
		{
			_logger.LogWarning("Invalid search request: {@Request}", request.Request);
			return PagedResult<TDto>.Empty();
		}

		// البحث
		var result = await _repository.SmartSearchAsync(request.Request, cancellationToken);

		// تحويل النتائج
		var dtoItems = _mapper.Map<IReadOnlyList<TDto>>(result.Items);

		_logger.LogInformation(
			"Found {TotalCount} {EntityName} items, returned page {PageNumber} with {ItemCount} items",
			result.TotalCount,
			typeof(TEntity).Name,
			result.PageNumber,
			dtoItems.Count);

		return new PagedResult<TDto>
		{
			Items = dtoItems,
			TotalCount = result.TotalCount,
			PageNumber = result.PageNumber,
			PageSize = result.PageSize,
			Metadata = result.Metadata
		};
	}
}
