using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Abstractions.Models;
using ACommerce.Payments.Abstractions.Enums;
using ACommerce.Marketing.Analytics.Services;

namespace ACommerce.Payments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentProvider _paymentProvider;
    private readonly IMarketingEventTracker _marketingTracker;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentProvider paymentProvider,
        IMarketingEventTracker marketingTracker,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PaymentsController> logger)
    {
        _paymentProvider = paymentProvider;
        _marketingTracker = marketingTracker;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// إنشاء عملية دفع جديدة
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePayment(
        [FromBody] CreatePaymentApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating payment for order {OrderId}, amount {Amount} {Currency}",
                request.OrderId, request.Amount, request.Currency);

            var userId = User.FindFirst("sub")?.Value ??
                         User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                         "anonymous";

            var paymentRequest = new PaymentRequest
            {
                Amount = request.Amount,
                Currency = request.Currency,
                OrderId = request.OrderId.ToString(),
                CustomerId = userId,
                Method = ParsePaymentMethod(request.PaymentMethod),
                CallbackUrl = request.Metadata?.GetValueOrDefault("returnUrl"),
                Description = $"Payment for order {request.OrderId}",
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var result = await _paymentProvider.CreatePaymentAsync(paymentRequest, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Payment creation failed for order {OrderId}: {Error}",
                    request.OrderId, result.ErrorMessage);

                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = result.ErrorMessage ?? "فشل في إنشاء عملية الدفع"
                });
            }

            _logger.LogInformation("Payment created successfully. TransactionId: {TransactionId}, PaymentUrl: {PaymentUrl}",
                result.TransactionId, result.PaymentUrl);

            return Ok(new PaymentResponse
            {
                Success = true,
                PaymentId = result.TransactionId,
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = result.Status.ToString(),
                PaymentUrl = result.PaymentUrl,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
            return StatusCode(500, new PaymentResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء إنشاء عملية الدفع"
            });
        }
    }

    /// <summary>
    /// الحصول على حالة عملية دفع
    /// </summary>
    [HttpGet("{paymentId}")]
    public async Task<ActionResult<PaymentResponse>> GetPaymentStatus(
        string paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentProvider.GetPaymentStatusAsync(paymentId, cancellationToken);

            // إرسال حدث الشراء عند اكتمال الدفع بنجاح
            if (result.Success && result.Status == PaymentStatus.Completed)
            {
                var userId = User.FindFirst("sub")?.Value ??
                             User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                             "anonymous";

                _logger.LogInformation("📊 Payment completed! Queuing purchase event for {PaymentId}", paymentId);

                // إرسال للطابور الخلفي (لا يحجب)
                await _marketingTracker.TrackPurchaseAsync(new PurchaseTrackingRequest
                {
                    TransactionId = paymentId,
                    Value = result.Amount ?? 0,
                    Currency = result.Currency ?? "SAR",
                    ContentName = $"Payment {paymentId}",
                    ContentIds = new[] { result.OrderId ?? paymentId },
                    ContentType = "payment",
                    User = new UserTrackingContext
                    {
                        UserId = userId,
                        IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()
                    }
                });
            }

            return Ok(new PaymentResponse
            {
                Success = result.Success,
                PaymentId = result.TransactionId,
                Amount = result.Amount ?? 0,
                Currency = result.Currency ?? string.Empty,
                Status = result.Status.ToString(),
                Message = result.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for {PaymentId}", paymentId);
            return StatusCode(500, new PaymentResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء جلب حالة الدفع"
            });
        }
    }

    /// <summary>
    /// إلغاء عملية دفع
    /// </summary>
    [HttpPost("{paymentId}/cancel")]
    public async Task<ActionResult<PaymentResponse>> CancelPayment(
        string paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _paymentProvider.CancelPaymentAsync(paymentId, cancellationToken);

            return Ok(new PaymentResponse
            {
                Success = success,
                PaymentId = paymentId,
                Status = success ? "Cancelled" : "CancellationFailed",
                Message = success ? "تم إلغاء عملية الدفع" : "فشل في إلغاء عملية الدفع"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
            return StatusCode(500, new PaymentResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء إلغاء عملية الدفع"
            });
        }
    }

    /// <summary>
    /// استرجاع مبلغ
    /// </summary>
    [HttpPost("{paymentId}/refund")]
    public async Task<ActionResult<RefundApiResponse>> RefundPayment(
        string paymentId,
        [FromBody] RefundApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var refundRequest = new RefundRequest
            {
                TransactionId = paymentId,
                Amount = request.Amount,
                Reason = request.Reason
            };

            var result = await _paymentProvider.RefundAsync(refundRequest, cancellationToken);

            return Ok(new RefundApiResponse
            {
                Success = result.Success,
                RefundId = result.RefundId,
                PaymentId = paymentId,
                Amount = request.Amount,
                Status = result.Success ? "Refunded" : "Failed",
                Message = result.ErrorMessage,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
            return StatusCode(500, new RefundApiResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء استرجاع المبلغ"
            });
        }
    }

    /// <summary>
    /// Webhook لاستقبال تحديثات الدفع من بوابة الدفع
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(
        [FromBody] object payload,
        [FromHeader(Name = "X-Signature")] string? signature,
        CancellationToken cancellationToken)
    {
        try
        {
            var payloadString = System.Text.Json.JsonSerializer.Serialize(payload);

            var isValid = await _paymentProvider.ValidateWebhookAsync(
                payloadString,
                signature ?? string.Empty,
                cancellationToken);

            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook signature");
                return Unauthorized();
            }

            _logger.LogInformation("Received valid payment webhook");

            // TODO: Process webhook payload and update order/subscription status

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500);
        }
    }

    private static PaymentMethod? ParsePaymentMethod(string? method)
    {
        if (string.IsNullOrEmpty(method)) return null;

        return method.ToLower() switch
        {
            "creditcard" or "credit_card" or "card" => PaymentMethod.CreditCard,
            "debitcard" or "debit_card" => PaymentMethod.DebitCard,
            "mada" => PaymentMethod.Mada,
            "applepay" or "apple_pay" => PaymentMethod.ApplePay,
            "googlepay" or "google_pay" => PaymentMethod.GooglePay,
            "stcpay" or "stc_pay" => PaymentMethod.StcPay,
            "bank" or "banktransfer" or "bank_transfer" => PaymentMethod.BankTransfer,
            _ => null
        };
    }
}

#region Request/Response Models

public class CreatePaymentApiRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? PaymentMethod { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RefundApiRequest
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class RefundApiResponse
{
    public bool Success { get; set; }
    public string RefundId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
