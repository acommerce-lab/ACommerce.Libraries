using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Exceptions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Commands;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج أمر التحديث
/// </summary>
public class UpdateCommandHandler<TEntity, TDto> : IRequestHandler<UpdateCommand<TEntity, TDto>, Unit>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<UpdateCommandHandler<TEntity, TDto>> _logger;

	public UpdateCommandHandler(
		IBaseAsyncRepository<TEntity> repository,
		IMapper mapper,
		ILogger<UpdateCommandHandler<TEntity, TDto>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<Unit> Handle(UpdateCommand<TEntity, TDto> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug("Updating {EntityName} with id {EntityId}", typeof(TEntity).Name, request.Id);

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

		// تطبيق التغييرات
		_mapper.Map(request.Data, entity);

		// الحفظ
		await _repository.UpdateAsync(entity, cancellationToken);

		_logger.LogInformation(
			"Updated {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		return Unit.Value;
	}
}
