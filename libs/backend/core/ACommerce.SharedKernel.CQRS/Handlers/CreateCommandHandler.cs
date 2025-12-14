using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Commands;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج أمر الإنشاء
/// </summary>
public class CreateCommandHandler<TEntity, TDto> : IRequestHandler<CreateCommand<TEntity, TDto>, TEntity>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<CreateCommandHandler<TEntity, TDto>> _logger;

	public CreateCommandHandler(
		IBaseAsyncRepository<TEntity> repository,
		IMapper mapper,
		ILogger<CreateCommandHandler<TEntity, TDto>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<TEntity> Handle(CreateCommand<TEntity, TDto> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug("Creating new {EntityName}", typeof(TEntity).Name);

		// تحويل DTO إلى Entity
		var entity = _mapper.Map<TEntity>(request.Data);

		// الإضافة
		var createdEntity = await _repository.AddAsync(entity, cancellationToken);

		_logger.LogInformation(
			"Created {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			createdEntity.Id);

		return createdEntity;
	}
}
