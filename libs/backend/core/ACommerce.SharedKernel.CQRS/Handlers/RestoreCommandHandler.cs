using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Commands;

namespace ACommerce.SharedKernel.CQRS.Handlers;

/// <summary>
/// معالج أمر الاستعادة
/// </summary>
public class RestoreCommandHandler<TEntity> : IRequestHandler<RestoreCommand<TEntity>, Unit>
	where TEntity : class, IBaseEntity
{
	private readonly IBaseAsyncRepository<TEntity> _repository;
	private readonly ILogger<RestoreCommandHandler<TEntity>> _logger;

	public RestoreCommandHandler(
		IBaseAsyncRepository<TEntity> repository,
		ILogger<RestoreCommandHandler<TEntity>> logger)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<Unit> Handle(RestoreCommand<TEntity> request, CancellationToken cancellationToken)
	{
		_logger.LogDebug(
			"Restoring {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		await _repository.RestoreAsync(request.Id, cancellationToken);

		_logger.LogInformation(
			"Restored {EntityName} with id {EntityId}",
			typeof(TEntity).Name,
			request.Id);

		return Unit.Value;
	}
}
