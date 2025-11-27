using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ACommerce.Payments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// إنشاء عملية دفع جديدة
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        // TODO: Implement with payment gateway
        return Ok(new { PaymentId = Guid.NewGuid(), Status = "Pending" });
    }

    /// <summary>
    /// الحصول على حالة عملية دفع
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentStatus(Guid id)
    {
        // TODO: Implement
        return Ok(new { PaymentId = id, Status = "Pending" });
    }

    /// <summary>
    /// Webhook لاستقبال تحديثات الدفع
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] object payload)
    {
        // TODO: Implement webhook handling
        return Ok();
    }
}

public class CreatePaymentRequest
{
    public required string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? PaymentMethod { get; set; }
}
