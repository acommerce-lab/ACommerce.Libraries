using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج استعلام GetById
/// </summary>
public class GetByIdQueryHandler<TEntity, TDto> : IRequestHandler<GetByIdQuery<TEntity, TDto>, TDto?>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<GetByIdQueryHandler<TEntity, TDto>> _logger;

	public GetByIdQueryHandler(
		IBaseAsyncRepository<TEntity> repository,
		IMapper mapper,
		ILogger<GetByIdQueryHandler<TEntity, TDto>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<TDto?> Handle(GetByIdQuery<TEntity, TDto> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug(
			"Getting {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		var entity = await _repository.GetByIdAsync(request.Id, request.IncludeDeleted, cancellationToken);

		if (entity == null)
		{
			_logger.LogDebug(
				"{EntityName} with id {EntityId} not found",
				typeof(TEntity).Name,
				request.Id);

			return default;
		}

		var dto = _mapper.Map<TDto>(entity);

		_logger.LogDebug(
			"Retrieved {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		return dto;
	}
}
