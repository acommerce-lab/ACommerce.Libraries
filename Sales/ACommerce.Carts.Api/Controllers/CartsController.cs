using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ACommerce.Carts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// الحصول على سلة التسوق الحالية
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyCart()
    {
        // TODO: Implement with CQRS
        return Ok(new { Items = new List<object>(), Total = 0m });
    }

    /// <summary>
    /// إضافة منتج للسلة
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
    {
        // TODO: Implement with CQRS
        return Ok();
    }

    /// <summary>
    /// تحديث كمية منتج في السلة
    /// </summary>
    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateQuantity(string productId, [FromBody] UpdateQuantityRequest request)
    {
        // TODO: Implement with CQRS
        return NoContent();
    }

    /// <summary>
    /// حذف منتج من السلة
    /// </summary>
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(string productId)
    {
        // TODO: Implement with CQRS
        return NoContent();
    }

    /// <summary>
    /// تفريغ السلة
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        // TODO: Implement with CQRS
        return NoContent();
    }
}

public class AddToCartRequest
{
    public required string ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}
