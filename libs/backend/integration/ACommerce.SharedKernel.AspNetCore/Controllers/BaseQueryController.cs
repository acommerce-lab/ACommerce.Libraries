using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.SharedKernel.AspNetCore.Controllers;

/// <summary>
/// المتحكم الأساسي للاستعلامات (Query - Read Operations)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseQueryController<TEntity, TResponseDto> : ControllerBase
	where TEntity : class, IBaseEntity
{
	protected readonly IMediator _mediator;
	protected readonly ILogger _logger;

	protected BaseQueryController(
		IMediator mediator,
		ILogger logger)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// الحصول على كيان بواسطة المعرف
	/// GET /api/{controller}/{id}
	/// </summary>
	[HttpGet("{id}")]
	//[ProducesResponseType(typeof(TResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public virtual async Task<ActionResult<TResponseDto>> GetById(
		Guid id,
		[FromQuery] List<string>? includes = null,
		[FromQuery] bool includeDeleted = false)
	{
		try
		{
			_logger.LogDebug("Getting {EntityName} by id {EntityId}", typeof(TEntity).Name, id);

			var query = new GetByIdQuery<TEntity, TResponseDto>
			{
				Id = id,
				IncludeProperties = includes,
				IncludeDeleted = includeDeleted
			};

			var result = await _mediator.Send(query);

			if (result == null)
			{
				_logger.LogWarning("{EntityName} with id {EntityId} not found", typeof(TEntity).Name, id);
				return NotFound(new { message = $"{typeof(TEntity).Name} not found" });
			}

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting {EntityName} by id {EntityId}", typeof(TEntity).Name, id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// البحث الذكي مع التصفية والترتيب والتصفح
	/// POST /api/{controller}/search
	/// </summary>
	[HttpPost("search")]
	//[ProducesResponseType(typeof(PagedResult<TResponseDto>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public virtual async Task<ActionResult<PagedResult<TResponseDto>>> Search(
		[FromBody] SmartSearchRequest request)
	{
		try
		{
			_logger.LogDebug(
				"Searching {EntityName} with page {PageNumber}, size {PageSize}",
				typeof(TEntity).Name,
				request.PageNumber,
				request.PageSize);

			if (!request.IsValid())
			{
				_logger.LogWarning("Invalid search request: {@Request}", request);
				return BadRequest(new { message = "Invalid search request" });
			}

			var query = new SmartSearchQuery<TEntity, TResponseDto> { Request = request };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error searching {EntityName}", typeof(TEntity).Name);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// عد الكيانات
	/// GET /api/{controller}/count
	/// </summary>
	[HttpGet("count")]
	[ProducesResponseType(typeof(int), 200)]
	[ProducesResponseType(500)]
	public virtual async Task<ActionResult<int>> Count(
		[FromQuery] bool includeDeleted = false)
	{
		try
		{
			_logger.LogDebug("Counting {EntityName}", typeof(TEntity).Name);

			var searchRequest = new SmartSearchRequest
			{
				PageSize = 1,
				PageNumber = 1,
				IncludeDeleted = includeDeleted
			};

			var query = new SmartSearchQuery<TEntity, TResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result.TotalCount);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error counting {EntityName}", typeof(TEntity).Name);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}
}

