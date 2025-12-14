using ACommerce.Accounting.Core.DTOs.Account;
using ACommerce.Accounting.Core.DTOs.ChartOfAccounts;
using ACommerce.Accounting.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Accounting.Core.Api.Controllers;

/// <summary>
/// ????? ???? ???????? (Chart of Accounts)
/// </summary>
[ApiController]
[Route("api/accounting/chart-of-accounts")]
[Produces("application/json")]
public class ChartOfAccountsController : BaseCrudController<
	ChartOfAccounts,
	CreateChartOfAccountsDto,
	UpdateChartOfAccountsDto,
	ChartOfAccountsResponseDto,
	PartialUpdateChartOfAccountsDto>
{
	public ChartOfAccountsController(
		IMediator mediator,
		ILogger<ChartOfAccountsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ?????? ??? ?????? ??????????
	/// GET /api/accounting/chart-of-accounts/default
	/// </summary>
	[HttpGet("default")]
	[ProducesResponseType(typeof(ChartOfAccountsResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ChartOfAccountsResponseDto>> GetDefault()
	{
		try
		{
			_logger.LogDebug("Getting default chart of accounts");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ChartOfAccounts.IsDefault),
						Value = true,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(ChartOfAccounts.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1
			};

			var query = new SmartSearchQuery<ChartOfAccounts, ChartOfAccountsResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				return NotFound(new { message = "Default chart of accounts not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting default chart of accounts");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ??? ??????
	/// GET /api/accounting/chart-of-accounts/by-company/{companyId}
	/// </summary>
	[HttpGet("by-company/{companyId}")]
	[ProducesResponseType(typeof(List<ChartOfAccountsResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ChartOfAccountsResponseDto>>> GetByCompany(Guid companyId)
	{
		try
		{
			_logger.LogDebug("Getting chart of accounts by company {CompanyId}", companyId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ChartOfAccounts.CompanyId),
						Value = companyId,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 100
			};

			var query = new SmartSearchQuery<ChartOfAccounts, ChartOfAccountsResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting chart of accounts by company {CompanyId}", companyId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???????
	/// GET /api/accounting/chart-of-accounts/{id}/root-accounts
	/// </summary>
	[HttpGet("{id}/root-accounts")]
	[ProducesResponseType(typeof(List<AccountResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<AccountResponseDto>>> GetRootAccounts(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting root accounts for chart {ChartId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Account.ChartOfAccountsId),
						Value = id,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(Account.ParentAccountId),
						Value = null,
						Operator = FilterOperator.IsNull
					}
				},
				OrderBy = nameof(Account.Code),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<Account, AccountResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting root accounts for chart {ChartId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs
public class UpdateChartOfAccountsDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsDefault { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateChartOfAccountsDto
{
	public string? Name { get; set; }
	public bool? IsActive { get; set; }
}

