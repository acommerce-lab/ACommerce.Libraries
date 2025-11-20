using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Exceptions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Commands;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج أمر الحذف
/// </summary>
public class DeleteCommandHandler<TEntity> : IRequestHandler<DeleteCommand<TEntity>, Unit>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly ILogger<DeleteCommandHandler<TEntity>> _logger;

	public DeleteCommandHandler(
		IBaseAsyncRepository<TEntity> repository,
		ILogger<DeleteCommandHandler<TEntity>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<Unit> Handle(DeleteCommand<TEntity> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug(
			"Deleting {EntityName} with id {EntityId} (SoftDelete: {SoftDelete})",
			typeof(TEntity).Name,
			request.Id,
			request.SoftDelete);

		// التحقق من الوجود
		var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

		if (entity == null)
		{
			_logger.LogWarning(
				"{EntityName} with id {EntityId} not found",
				typeof(TEntity).Name,
				request.Id);

			throw new EntityNotFoundException(typeof(TEntity).Name, request.Id);
		}

		// الحذف
		if (request.SoftDelete)
		{
			await _repository.SoftDeleteAsync(request.Id, cancellationToken);
			_logger.LogInformation(
				"Soft deleted {EntityName} with id {EntityId}",
				typeof(TEntity).Name,
				request.Id);
		}
		else
		{
			await _repository.DeleteAsync(request.Id, cancellationToken);
			_logger.LogInformation(
				"Hard deleted {EntityName} with id {EntityId}",
				typeof(TEntity).Name,
				request.Id);
		}

		return Unit.Value;
	}
}
