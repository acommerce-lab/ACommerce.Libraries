using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Cart.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Cart.Controllers;

/// <summary>
/// متحكم سلة التسوق
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<CartController> _logger;

	public CartController(IMediator mediator, ILogger<CartController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// إضافة منتج للسلة
	/// </summary>
	[HttpPost("add")]
	public async Task<ActionResult<CartResponseDto>> AddToCart([FromBody] AddToCartDto dto)
	{
		// سيتم تنفيذ المنطق لاحقاً
		return Ok(new CartResponseDto());
	}

	/// <summary>
	/// الحصول على السلة
	/// </summary>
	[HttpGet("{userIdOrSessionId}")]
	public async Task<ActionResult<CartResponseDto>> GetCart(string userIdOrSessionId)
	{
		// سيتم تنفيذ المنطق لاحقاً
		return Ok(new CartResponseDto());
	}

	/// <summary>
	/// حذف بند من السلة
	/// </summary>
	[HttpDelete("items/{itemId}")]
	public async Task<IActionResult> RemoveItem(Guid itemId)
	{
		// سيتم تنفيذ المنطق لاحقاً
		return NoContent();
	}

	/// <summary>
	/// إفراغ السلة
	/// </summary>
	[HttpDelete("{userIdOrSessionId}")]
	public async Task<IActionResult> ClearCart(string userIdOrSessionId)
	{
		// سيتم تنفيذ المنطق لاحقاً
		return NoContent();
	}
}
