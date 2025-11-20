using ACommerce.Accounting.Core.DTOs.AccountingEntry;
using ACommerce.Accounting.Core.Entities;
using ACommerce.Accounting.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Accounting.Core.Api.Controllers;

/// <summary>
/// ????? ?????? ????????? (Accounting Entries) - ????? ??????! ??
/// </summary>
[ApiController]
[Route("api/accounting/entries")]
[Produces("application/json")]
public class AccountingEntriesController : BaseCrudController<
	AccountingEntry,
	CreateAccountingEntryDto,
	UpdateAccountingEntryDto,
	AccountingEntryResponseDto,
	PartialUpdateAccountingEntryDto>
{
	public AccountingEntriesController(
		IMediator mediator,
		ILogger<AccountingEntriesController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ????? ??????
	/// GET /api/accounting/entries/by-number/{number}
	/// </summary>
	[HttpGet("by-number/{number}")]
	[ProducesResponseType(typeof(AccountingEntryResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<AccountingEntryResponseDto>> GetByNumber(string number)
	{
		try
		{
			_logger.LogDebug("Getting entry by number {Number}", number);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AccountingEntry.Number),
						Value = number,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1
			};

			var query = new SmartSearchQuery<AccountingEntry, AccountingEntryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				return NotFound(new { message = "Entry not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting entry by number {Number}", number);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????
	/// GET /api/accounting/entries/by-status/{status}
	/// </summary>
	[HttpGet("by-status/{status}")]
	[ProducesResponseType(typeof(PagedResult<AccountingEntryResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AccountingEntryResponseDto>>> GetByStatus(
		EntryStatus status,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting entries by status {Status}", status);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AccountingEntry.Status),
						Value = status,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(AccountingEntry.Date),
				Ascending = false,
				PageNumber = pageNumber,
				PageSize = pageSize
			};

			var query = new SmartSearchQuery<AccountingEntry, AccountingEntryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting entries by status {Status}", status);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ???????
	/// GET /api/accounting/entries/by-period
	/// </summary>
	[HttpGet("by-period")]
	[ProducesResponseType(typeof(PagedResult<AccountingEntryResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AccountingEntryResponseDto>>> GetByPeriod(
		[FromQuery] int fiscalYear,
		[FromQuery] int fiscalPeriod,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting entries for year {Year} period {Period}", fiscalYear, fiscalPeriod);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AccountingEntry.FiscalYear),
						Value = fiscalYear,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(AccountingEntry.FiscalPeriod),
						Value = fiscalPeriod,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(AccountingEntry.Date),
				Ascending = false,
				PageNumber = pageNumber,
				PageSize = pageSize
			};

			var query = new SmartSearchQuery<AccountingEntry, AccountingEntryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting entries by period");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ???
	/// POST /api/accounting/entries/{id}/approve
	/// </summary>
	[HttpPost("{id}/approve")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveEntryRequest request)
	{
		try
		{
			_logger.LogDebug("Approving entry {EntryId}", id);

			var updateDto = new PartialUpdateAccountingEntryDto
			{
				Status = EntryStatus.Approved,
				ApprovedDate = DateTime.UtcNow,
				ApprovedByUserId = request.UserId
			};

			var command = new PartialUpdateCommand<AccountingEntry, PartialUpdateAccountingEntryDto>
			{
				Id = id,
				Data = updateDto
			};

			await _mediator.Send(command);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving entry {EntryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???
	/// POST /api/accounting/entries/{id}/post
	/// </summary>
	[HttpPost("{id}/post")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Post(Guid id, [FromBody] PostEntryRequest request)
	{
		try
		{
			_logger.LogDebug("Posting entry {EntryId}", id);

			var updateDto = new PartialUpdateAccountingEntryDto
			{
				Status = EntryStatus.Posted,
				PostedDate = DateTime.UtcNow,
				PostedByUserId = request.UserId
			};

			var command = new PartialUpdateCommand<AccountingEntry, PartialUpdateAccountingEntryDto>
			{
				Id = id,
				Data = updateDto
			};

			await _mediator.Send(command);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error posting entry {EntryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ??? ???
	/// POST /api/accounting/entries/{id}/reverse
	/// </summary>
	[HttpPost("{id}/reverse")]
	[ProducesResponseType(typeof(AccountingEntryResponseDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<AccountingEntryResponseDto>> Reverse(
		Guid id,
		[FromBody] ReverseEntryRequest request)
	{
		try
		{
			_logger.LogDebug("Reversing entry {EntryId}", id);

			// TODO: ????? ???? ??? ?????
			// 1. ??? ????? ??????
			// 2. ????? ??? ????? (??? ?????? ???????)
			// 3. ?????? ????

			return StatusCode(501, new { message = "Reverse functionality not yet implemented" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reversing entry {EntryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs
public class UpdateAccountingEntryDto
{
	public string? Description { get; set; }
	public DateTime? Date { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateAccountingEntryDto
{
	public EntryStatus? Status { get; set; }
	public DateTime? PostedDate { get; set; }
	public string? PostedByUserId { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string? ApprovedByUserId { get; set; }
}

public class ApproveEntryRequest
{
	public required string UserId { get; set; }
}

public class PostEntryRequest
{
	public required string UserId { get; set; }
}

public class ReverseEntryRequest
{
	public required string UserId { get; set; }
	public required string Reason { get; set; }
	public DateTime? ReversalDate { get; set; }
}

