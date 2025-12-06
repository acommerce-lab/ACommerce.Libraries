using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ACommerce.Realtime.SignalR.Hubs;

namespace Ashare.Api.Controllers;

/// <summary>
/// Payment Callback Controller
/// يستقبل إعادة التوجيه من بوابة الدفع (Noon) ويرسل إشعار SignalR للتطبيق
/// </summary>
[ApiController]
[Route("host/payment")]
public class PaymentCallbackController : ControllerBase
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<PaymentCallbackController> _logger;

    public PaymentCallbackController(
        IHubContext<NotificationHub, INotificationClient> hubContext,
        ILogger<PaymentCallbackController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Payment callback endpoint - receives redirect from payment gateway
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? orderId,
        [FromQuery] string? status,
        [FromQuery] string? transactionId,
        [FromQuery] string? resultCode,
        [FromQuery] string? message,
        [FromQuery] string? error)
    {
        _logger.LogInformation(
            "Payment callback received: OrderId={OrderId}, Status={Status}, TransactionId={TransactionId}, ResultCode={ResultCode}",
            orderId, status, transactionId, resultCode);

        // Determine if payment was successful
        var isSuccess = status?.ToLower() == "success" ||
                       status?.ToLower() == "captured" ||
                       resultCode == "0";

        var paymentResult = new
        {
            Success = isSuccess,
            OrderId = orderId,
            TransactionId = transactionId ?? orderId,
            Status = status ?? "unknown",
            Message = isSuccess ? "تم الدفع بنجاح" : (message ?? error ?? "فشلت عملية الدفع"),
            Timestamp = DateTime.UtcNow
        };

        // Send SignalR notification to the payment group
        if (!string.IsNullOrEmpty(orderId))
        {
            var groupName = $"payment_{orderId}";

            _logger.LogInformation(
                "Sending payment notification to group {GroupName}: Success={Success}",
                groupName, isSuccess);

            try
            {
                await _hubContext.Clients.Group(groupName).ReceiveMessage(
                    isSuccess ? "PaymentSuccess" : "PaymentFailed",
                    paymentResult);

                _logger.LogInformation("Payment notification sent successfully to group {GroupName}", groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment notification to group {GroupName}", groupName);
            }
        }

        // Return HTML page telling user to close the browser
        var htmlContent = GenerateCallbackHtml(isSuccess, paymentResult.Message);
        return Content(htmlContent, "text/html; charset=utf-8");
    }

    /// <summary>
    /// Alternative POST callback for payment gateways that use POST
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> CallbackPost([FromForm] PaymentCallbackForm form)
    {
        return await Callback(
            form.OrderId,
            form.Status,
            form.TransactionId,
            form.ResultCode,
            form.Message,
            form.Error);
    }

    private static string GenerateCallbackHtml(bool isSuccess, string message)
    {
        var iconSvg = isSuccess
            ? """<svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#10b981" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg>"""
            : """<svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#ef4444" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>""";

        var statusText = isSuccess ? "تم الدفع بنجاح!" : "فشلت عملية الدفع";
        var statusColor = isSuccess ? "#10b981" : "#ef4444";
        var bgColor = isSuccess ? "#ecfdf5" : "#fef2f2";

        return $$"""
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>نتيجة الدفع - عشير</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .container {
            background: white;
            border-radius: 24px;
            padding: 48px 32px;
            text-align: center;
            max-width: 400px;
            width: 100%;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
        }
        .icon-container {
            width: 120px;
            height: 120px;
            border-radius: 50%;
            background: {{bgColor}};
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 24px;
        }
        h1 {
            color: {{statusColor}};
            font-size: 24px;
            margin-bottom: 12px;
        }
        .message {
            color: #6b7280;
            font-size: 16px;
            margin-bottom: 32px;
            line-height: 1.6;
        }
        .instruction {
            background: #f3f4f6;
            border-radius: 12px;
            padding: 16px;
            margin-bottom: 24px;
        }
        .instruction p {
            color: #374151;
            font-size: 14px;
        }
        .instruction strong {
            display: block;
            margin-bottom: 8px;
            color: #111827;
        }
        .close-btn {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 16px 32px;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            width: 100%;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        .close-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.3);
        }
        .close-btn:active {
            transform: translateY(0);
        }
        .logo {
            margin-top: 32px;
            color: #9ca3af;
            font-size: 14px;
        }
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(20px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .container {
            animation: fadeIn 0.5s ease-out;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="icon-container">
            {{iconSvg}}
        </div>
        <h1>{{statusText}}</h1>
        <p class="message">{{message}}</p>
        <div class="instruction">
            <strong>📱 يمكنك الآن العودة للتطبيق</strong>
            <p>تم إرسال إشعار بنتيجة الدفع إلى التطبيق تلقائياً</p>
        </div>
        <button class="close-btn" onclick="closeWindow()">
            إغلاق هذه الصفحة
        </button>
        <p class="logo">عشير - منصة المساحات المشتركة</p>
    </div>
    <script>
        function closeWindow() {
            // Try to close the window/tab
            window.close();

            // If close didn't work (some browsers block it), show message
            setTimeout(function() {
                document.querySelector('.instruction p').textContent =
                    'إذا لم تُغلق الصفحة تلقائياً، يرجى إغلاقها يدوياً والعودة للتطبيق';
            }, 500);
        }

        // Auto-close after 10 seconds
        setTimeout(closeWindow, 10000);
    </script>
</body>
</html>
""";
    }
}

public class PaymentCallbackForm
{
    public string? OrderId { get; set; }
    public string? Status { get; set; }
    public string? TransactionId { get; set; }
    public string? ResultCode { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
