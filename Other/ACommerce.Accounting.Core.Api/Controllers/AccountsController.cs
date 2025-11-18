using ACommerce.Accounting.Core.DTOs.Account;
using ACommerce.Accounting.Core.Entities;
using ACommerce.Accounting.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Accounting.Core.Api.Controllers;

/// <summary>
/// ????? ???????? ????????? (Accounts)
/// </summary>
[ApiController]
[Route("api/accounting/accounts")]
[Produces("application/json")]
public class AccountsController : BaseCrudController<
	Account,
	CreateAccountDto,
	UpdateAccountDto,
	AccountResponseDto,
	PartialUpdateAccountDto>
{
	public AccountsController(
		IMediator mediator,
		ILogger<AccountsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ????? ??????
	/// GET /api/accounting/accounts/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(AccountResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<AccountResponseDto>> GetByCode(
		string code,
		[FromQuery] Guid? chartOfAccountsId = null)
	{
		try
		{
			_logger.LogDebug("Getting account by code {Code}", code);

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(Account.Code),
					Value = code,
					Operator = FilterOperator.Equals
				}
			};

			if (chartOfAccountsId.HasValue)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(Account.ChartOfAccountsId),
					Value = chartOfAccountsId.Value,
					Operator = FilterOperator.Equals
				});
			}

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				PageSize = 1
			};

			var query = new SmartSearchQuery<Account, AccountResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				return NotFound(new { message = "Account not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting account by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????
	/// GET /api/accounting/accounts/by-type/{type}
	/// </summary>
	[HttpGet("by-type/{type}")]
	[ProducesResponseType(typeof(List<AccountResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<AccountResponseDto>>> GetByType(AccountType type)
	{
		try
		{
			_logger.LogDebug("Getting accounts by type {Type}", type);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Account.Type),
						Value = type,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(Account.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
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
			_logger.LogError(ex, "Error getting accounts by type {Type}", type);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???????
	/// GET /api/accounting/accounts/{id}/sub-accounts
	/// </summary>
	[HttpGet("{id}/sub-accounts")]
	[ProducesResponseType(typeof(List<AccountResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<AccountResponseDto>>> GetSubAccounts(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting sub-accounts for {AccountId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Account.ParentAccountId),
						Value = id,
						Operator = FilterOperator.Equals
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
			_logger.LogError(ex, "Error getting sub-accounts for {AccountId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???????? ???
	/// GET /api/accounting/accounts/leaf
	/// </summary>
	[HttpGet("leaf")]
	[ProducesResponseType(typeof(List<AccountResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<AccountResponseDto>>> GetLeafAccounts()
	{
		try
		{
			_logger.LogDebug("Getting leaf accounts");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Account.IsLeaf),
						Value = true,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(Account.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(Account.Code),
				Ascending = true,
				PageSize = 1000
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
			_logger.LogError(ex, "Error getting leaf accounts");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs
public class UpdateAccountDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsLeaf { get; set; }
	public bool? AllowPosting { get; set; }
	public Guid? DefaultCostCenterId { get; set; }
	public bool? RequiresCostCenter { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateAccountDto
{
	public string? Name { get; set; }
	public bool? IsActive { get; set; }
}

