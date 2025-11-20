using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ACommerce.SharedKernel.CQRS.Behaviors;

/// <summary>
/// Behavior للتحقق من الصحة تلقائياً
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;
	private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

	public ValidationBehavior(
		IEnumerable<IValidator<TRequest>> validators,
		ILogger<ValidationBehavior<TRequest, TResponse>> logger)
	{
		_validators = validators ?? throw new ArgumentNullException(nameof(validators));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_validators.Any())
		{
			return await next();
		}

		var requestName = typeof(TRequest).Name;

		_logger.LogDebug("Validating {RequestName}", requestName);

		var context = new ValidationContext<TRequest>(request);

		var validationResults = await Task.WhenAll(
			_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

		var failures = validationResults
			.SelectMany(r => r.Errors)
			.Where(f => f != null)
			.ToList();

		if (failures.Count != 0)
		{
			_logger.LogWarning(
				"Validation failed for {RequestName} with {ErrorCount} errors: {Errors}",
				requestName,
				failures.Count,
				string.Join(", ", failures.Select(f => f.ErrorMessage)));

			throw new ValidationException(failures);
		}

		_logger.LogDebug("Validation passed for {RequestName}", requestName);

		return await next();
	}
}
