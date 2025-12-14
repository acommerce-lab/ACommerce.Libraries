using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Exceptions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Commands;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج أمر التحديث الجزئي
/// </summary>
public class PartialUpdateCommandHandler<TEntity, TDto> : IRequestHandler<PartialUpdateCommand<TEntity, TDto>, Unit>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<PartialUpdateCommandHandler<TEntity, TDto>> _logger;

	public PartialUpdateCommandHandler(
		IBaseAsyncRepository<TEntity> repository,
		IMapper mapper,
		ILogger<PartialUpdateCommandHandler<TEntity, TDto>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<Unit> Handle(PartialUpdateCommand<TEntity, TDto> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug(
			"Partially updating {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		// جلب الكيان
		var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

		if (entity == null)
		{
			_logger.LogWarning(
				"{EntityName} with id {EntityId} not found",
				typeof(TEntity).Name,
				request.Id);

			throw new EntityNotFoundException(typeof(TEntity).Name, request.Id);
		}

		// تطبيق التغييرات الجزئية
		_mapper.Map(request.Data, entity);

		// الحفظ
		await _repository.UpdateAsync(entity, cancellationToken);

		_logger.LogInformation(
			"Partially updated {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		return Unit.Value;
	}
}
