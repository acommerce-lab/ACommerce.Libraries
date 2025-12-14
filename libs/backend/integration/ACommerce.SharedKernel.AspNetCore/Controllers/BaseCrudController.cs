using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.SharedKernel.AspNetCore.Controllers;

/// <summary>
/// المتحكم الأساسي الكامل (CRUD - Create, Read, Update, Delete)
/// يجمع بين Query و Command Controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseCrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto, TPartialUpdateDto> : ControllerBase
	where TEntity : class, IBaseEntity
{
	protected readonly IMediator _mediator;
	protected readonly ILogger _logger;

	protected BaseCrudController(
		IMediator mediator,
		ILogger logger)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	// ====================================================================================
	// Query Operations (Read)
	// ====================================================================================

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

	// ====================================================================================
	// Command Operations (Write)
	// ====================================================================================

	/// <summary>
	/// إنشاء كيان جديد
	/// POST /api/{controller}
	/// </summary>
	[HttpPost]
	//[ProducesResponseType(typeof(TEntity), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public virtual async Task<ActionResult<TEntity>> Create(
		[FromBody] TCreateDto dto)
	{
		try
		{
			_logger.LogDebug("Creating new {EntityName}", typeof(TEntity).Name);

			var command = new CreateCommand<TEntity, TCreateDto> { Data = dto };
			var result = await _mediator.Send(command);

			_logger.LogInformation("Created {EntityName} with id {EntityId}", typeof(TEntity).Name, result.Id);

			return CreatedAtAction(
				actionName: nameof(GetById),
				routeValues: new { id = result.Id },
				value: result);
		}
		catch (FluentValidation.ValidationException vex)
		{
			_logger.LogWarning("Validation failed for {EntityName}: {Errors}", typeof(TEntity).Name, vex.Errors);
			return BadRequest(new
			{
				message = "Validation failed",
				errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating {EntityName}", typeof(TEntity).Name);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// تحديث كيان كامل
	/// PUT /api/{controller}/{id}
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public virtual async Task<IActionResult> Update(
		Guid id,
		[FromBody] TUpdateDto dto)
	{
		try
		{
			_logger.LogDebug("Updating {EntityName} with id {EntityId}", typeof(TEntity).Name, id);

			var command = new UpdateCommand<TEntity, TUpdateDto> { Id = id, Data = dto };
			await _mediator.Send(command);

			_logger.LogInformation("Updated {EntityName} with id {EntityId}", typeof(TEntity).Name, id);

			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			_logger.LogWarning("{EntityName} with id {EntityId} not found", typeof(TEntity).Name, id);
			return NotFound(new { message = $"{typeof(TEntity).Name} not found" });
		}
		catch (FluentValidation.ValidationException vex)
		{
			_logger.LogWarning("Validation failed for {EntityName}: {Errors}", typeof(TEntity).Name, vex.Errors);
			return BadRequest(new
			{
				message = "Validation failed",
				errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating {EntityName} with id {EntityId}", typeof(TEntity).Name, id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// تحديث جزئي لكيان
	/// PATCH /api/{controller}/{id}
	/// </summary>
	[HttpPatch("{id}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public virtual async Task<IActionResult> PartialUpdate(
		Guid id,
		[FromBody] TPartialUpdateDto dto)
	{
		try
		{
			_logger.LogDebug("Partially updating {EntityName} with id {EntityId}", typeof(TEntity).Name, id);

			var command = new PartialUpdateCommand<TEntity, TPartialUpdateDto> { Id = id, Data = dto };
			await _mediator.Send(command);

			_logger.LogInformation("Partially updated {EntityName} with id {EntityId}", typeof(TEntity).Name, id);

			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			_logger.LogWarning("{EntityName} with id {EntityId} not found", typeof(TEntity).Name, id);
			return NotFound(new { message = $"{typeof(TEntity).Name} not found" });
		}
		catch (FluentValidation.ValidationException vex)
		{
			_logger.LogWarning("Validation failed for {EntityName}: {Errors}", typeof(TEntity).Name, vex.Errors);
			return BadRequest(new
			{
				message = "Validation failed",
				errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error partially updating {EntityName} with id {EntityId}", typeof(TEntity).Name, id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// حذف كيان
	/// DELETE /api/{controller}/{id}
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public virtual async Task<IActionResult> Delete(
		Guid id,
		[FromQuery] bool softDelete = true)
	{
		try
		{
			_logger.LogDebug(
				"{DeleteType} deleting {EntityName} with id {EntityId}",
				softDelete ? "Soft" : "Hard",
				typeof(TEntity).Name,
				id);

			var command = new DeleteCommand<TEntity> { Id = id, SoftDelete = softDelete };
			await _mediator.Send(command);

			_logger.LogInformation(
				"{DeleteType} deleted {EntityName} with id {EntityId}",
				softDelete ? "Soft" : "Hard",
				typeof(TEntity).Name,
				id);

			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			_logger.LogWarning("{EntityName} with id {EntityId} not found", typeof(TEntity).Name, id);
			return NotFound(new { message = $"{typeof(TEntity).Name} not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting {EntityName} with id {EntityId}", typeof(TEntity).Name, id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// استعادة كيان محذوف منطقياً
	/// POST /api/{controller}/{id}/restore
	/// </summary>
	[HttpPost("{id}/restore")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public virtual async Task<IActionResult> Restore(Guid id)
	{
		try
		{
			_logger.LogDebug("Restoring {EntityName} with id {EntityId}", typeof(TEntity).Name, id);

			var command = new RestoreCommand<TEntity> { Id = id };
			await _mediator.Send(command);

			_logger.LogInformation("Restored {EntityName} with id {EntityId}", typeof(TEntity).Name, id);

			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			_logger.LogWarning("{EntityName} with id {EntityId} not found", typeof(TEntity).Name, id);
			return NotFound(new { message = $"{typeof(TEntity).Name} not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error restoring {EntityName} with id {EntityId}", typeof(TEntity).Name, id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}
}
