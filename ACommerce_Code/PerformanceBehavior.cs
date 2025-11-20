using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ACommerce.SharedKernel.CQRS.Behaviors;

/// <summary>
/// Behavior لتتبع الأداء والتحذير من العمليات البطيئة
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
	private readonly int _slowThresholdMs;

	public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_slowThresholdMs = 500; // 500ms threshold
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var stopwatch = Stopwatch.StartNew();

		var response = await next();

		stopwatch.Stop();

		var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

		if (elapsedMilliseconds > _slowThresholdMs)
		{
			var requestName = typeof(TRequest).Name;

			_logger.LogWarning(
				"[PERFORMANCE] Long running request detected: {RequestName} took {ElapsedMilliseconds}ms (threshold: {Threshold}ms)",
				requestName,
				elapsedMilliseconds,
				_slowThresholdMs);
		}

		return response;
	}
}
