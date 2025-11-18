using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ACommerce.SharedKernel.CQRS.Behaviors;

/// <summary>
/// Behavior للتسجيل التلقائي
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

	public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var requestName = typeof(TRequest).Name;
		var requestGuid = Guid.NewGuid().ToString();

		_logger.LogInformation(
			"[START] {RequestName} ({RequestGuid})",
			requestName,
			requestGuid);

		var stopwatch = Stopwatch.StartNew();

		try
		{
			var response = await next();

			stopwatch.Stop();

			_logger.LogInformation(
				"[END] {RequestName} ({RequestGuid}) completed in {ElapsedMilliseconds}ms",
				requestName,
				requestGuid,
				stopwatch.ElapsedMilliseconds);

			return response;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			_logger.LogError(
				ex,
				"[ERROR] {RequestName} ({RequestGuid}) failed after {ElapsedMilliseconds}ms",
				requestName,
				requestGuid,
				stopwatch.ElapsedMilliseconds);

			throw;
		}
	}
}
