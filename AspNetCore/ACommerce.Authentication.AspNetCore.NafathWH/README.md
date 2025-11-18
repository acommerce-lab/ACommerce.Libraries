# ACommerce.Authentication.AspNetCore.NafathWH

Nafath webhook handler for authentication callbacks.

## Overview

Automatic webhook handling for Nafath authentication callbacks. Processes authentication status updates, validates signatures, and triggers events.

## Key Features

✅ **Automatic Webhook Processing** - Handle Nafath callbacks  
✅ **Signature Validation** - Verify webhook authenticity  
✅ **Event Publishing** - Publish domain events  
✅ **Retry Logic** - Handle failed webhooks  
✅ **Logging** - Comprehensive webhook logging  
✅ **Rate Limiting** - Prevent webhook flooding  

## Configuration

### appsettings.json
```json
{
  "NafathWebhookSettings": {
    "WebhookPath": "/webhooks/nafath",
    "SignatureHeader": "X-Nafath-Signature",
    "SignatureSecret": "your-webhook-secret",
    "ValidateSignature": true,
    "EnableLogging": true,
    "EnableRateLimiting": true,
    "MaxRequestsPerMinute": 60,
    "RetryFailedWebhooks": true,
    "MaxRetries": 3,
    "RetryDelaySeconds": 60
  }
}
```

## Setup

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Nafath Webhook Handler
builder.Services.AddNafathWebhookHandler(builder.Configuration);

// Add required services
builder.Services.AddScoped<INafathAuthenticationService, NafathAuthenticationService>();
builder.Services.AddScoped<IRealtimeHub, SignalRRealtimeHub<NafathHub, INafathClient>>();

var app = builder.Build();

// Map webhook endpoint
app.MapPost("/webhooks/nafath", async (
    HttpContext context,
    INafathWebhookHandler handler) =>
{
    await handler.HandleWebhookAsync(context);
});

// Or use controller
app.MapControllers();

app.Run();
```

## Webhook Controller
```csharp
[ApiController]
[Route("webhooks")]
public class NafathWebhookController : ControllerBase
{
    private readonly INafathWebhookHandler _webhookHandler;
    private readonly ILogger<NafathWebhookController> _logger;
    
    public NafathWebhookController(
        INafathWebhookHandler webhookHandler,
        ILogger<NafathWebhookController> logger)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
    }
    
    /// <summary>
    /// Nafath authentication webhook callback
    /// </summary>
    [HttpPost("nafath")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> NafathCallback()
    {
        try
        {
            _logger.LogInformation("Received Nafath webhook");
            
            await _webhookHandler.HandleWebhookAsync(HttpContext);
            
            return Ok(new { status = "processed" });
        }
        catch (InvalidSignatureException ex)
        {
            _logger.LogWarning(ex, "Invalid webhook signature");
            return Unauthorized(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Nafath webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
```

## Webhook Handler Implementation
```csharp
public interface INafathWebhookHandler
{
    Task HandleWebhookAsync(HttpContext context);
}

public class NafathWebhookHandler : INafathWebhookHandler
{
    private readonly INafathAuthenticationService _nafathService;
    private readonly IRealtimeHub _realtimeHub;
    private readonly IMediator _mediator;
    private readonly ILogger<NafathWebhookHandler> _logger;
    private readonly NafathWebhookSettings _settings;
    
    public NafathWebhookHandler(
        INafathAuthenticationService nafathService,
        IRealtimeHub realtimeHub,
        IMediator mediator,
        ILogger<NafathWebhookHandler> logger,
        IOptions<NafathWebhookSettings> settings)
    {
        _nafathService = nafathService;
        _realtimeHub = realtimeHub;
        _mediator = mediator;
        _logger = logger;
        _settings = settings.Value;
    }
    
    public async Task HandleWebhookAsync(HttpContext context)
    {
        // Read request body
        var body = await ReadRequestBodyAsync(context.Request);
        
        // Validate signature
        if (_settings.ValidateSignature)
        {
            ValidateSignature(context.Request.Headers, body);
        }
        
        // Parse webhook payload
        var webhook = JsonSerializer.Deserialize<NafathWebhookPayload>(body);
        
        if (webhook == null)
        {
            throw new InvalidOperationException("Invalid webhook payload");
        }
        
        _logger.LogInformation(
            "Processing Nafath webhook: TransactionId={TransactionId}, Status={Status}",
            webhook.TransactionId,
            webhook.Status);
        
        // Process based on status
        switch (webhook.Status.ToLowerInvariant())
        {
            case "completed":
                await HandleCompletedAsync(webhook);
                break;
                
            case "rejected":
                await HandleRejectedAsync(webhook);
                break;
                
            case "expired":
                await HandleExpiredAsync(webhook);
                break;
                
            case "cancelled":
                await HandleCancelledAsync(webhook);
                break;
                
            default:
                _logger.LogWarning("Unknown webhook status: {Status}", webhook.Status);
                break;
        }
        
        // Log webhook
        if (_settings.EnableLogging)
        {
            await LogWebhookAsync(webhook, body);
        }
    }
    
    private async Task HandleCompletedAsync(NafathWebhookPayload webhook)
    {
        // Complete authentication
        var result = await _nafathService.CompleteAuthenticationAsync(webhook.TransactionId);
        
        if (result.IsSuccess)
        {
            // Publish event
            await _mediator.Publish(new NafathAuthenticationCompletedEvent
            {
                TransactionId = webhook.TransactionId,
                UserId = result.UserId,
                CompletedAt = DateTime.UtcNow
            });
            
            // Notify via SignalR
            await _realtimeHub.SendToUserAsync(
                result.UserId,
                "NafathCompleted",
                new
                {
                    transactionId = webhook.TransactionId,
                    status = "completed"
                });
            
            _logger.LogInformation(
                "Nafath authentication completed: TransactionId={TransactionId}, UserId={UserId}",
                webhook.TransactionId,
                result.UserId);
        }
        else
        {
            _logger.LogError(
                "Failed to complete Nafath authentication: {Error}",
                result.Error);
        }
    }
    
    private async Task HandleRejectedAsync(NafathWebhookPayload webhook)
    {
        // Get transaction
        var transaction = await _nafathService.GetTransactionAsync(webhook.TransactionId);
        
        if (transaction != null)
        {
            // Update status
            await _nafathService.UpdateTransactionStatusAsync(
                webhook.TransactionId,
                NafathStatus.Rejected);
            
            // Publish event
            await _mediator.Publish(new NafathAuthenticationRejectedEvent
            {
                TransactionId = webhook.TransactionId,
                UserId = transaction.UserId,
                RejectedAt = DateTime.UtcNow
            });
            
            // Notify via SignalR
            await _realtimeHub.SendToUserAsync(
                transaction.UserId,
                "NafathFailed",
                new
                {
                    transactionId = webhook.TransactionId,
                    status = "rejected",
                    message = "المستخدم رفض عملية التحقق"
                });
            
            _logger.LogInformation(
                "Nafath authentication rejected: TransactionId={TransactionId}",
                webhook.TransactionId);
        }
    }
    
    private async Task HandleExpiredAsync(NafathWebhookPayload webhook)
    {
        var transaction = await _nafathService.GetTransactionAsync(webhook.TransactionId);
        
        if (transaction != null)
        {
            await _nafathService.UpdateTransactionStatusAsync(
                webhook.TransactionId,
                NafathStatus.Expired);
            
            await _mediator.Publish(new NafathAuthenticationExpiredEvent
            {
                TransactionId = webhook.TransactionId,
                UserId = transaction.UserId,
                ExpiredAt = DateTime.UtcNow
            });
            
            await _realtimeHub.SendToUserAsync(
                transaction.UserId,
                "NafathFailed",
                new
                {
                    transactionId = webhook.TransactionId,
                    status = "expired",
                    message = "انتهت صلاحية عملية التحقق"
                });
            
            _logger.LogInformation(
                "Nafath authentication expired: TransactionId={TransactionId}",
                webhook.TransactionId);
        }
    }
    
    private async Task HandleCancelledAsync(NafathWebhookPayload webhook)
    {
        var transaction = await _nafathService.GetTransactionAsync(webhook.TransactionId);
        
        if (transaction != null)
        {
            await _nafathService.UpdateTransactionStatusAsync(
                webhook.TransactionId,
                NafathStatus.Cancelled);
            
            await _mediator.Publish(new NafathAuthenticationCancelledEvent
            {
                TransactionId = webhook.TransactionId,
                UserId = transaction.UserId,
                CancelledAt = DateTime.UtcNow
            });
            
            await _realtimeHub.SendToUserAsync(
                transaction.UserId,
                "NafathFailed",
                new
                {
                    transactionId = webhook.TransactionId,
                    status = "cancelled",
                    message = "تم إلغاء عملية التحقق"
                });
            
            _logger.LogInformation(
                "Nafath authentication cancelled: TransactionId={TransactionId}",
                webhook.TransactionId);
        }
    }
    
    private void ValidateSignature(IHeaderDictionary headers, string body)
    {
        var signatureHeader = _settings.SignatureHeader;
        
        if (!headers.TryGetValue(signatureHeader, out var signature))
        {
            throw new InvalidSignatureException("Missing signature header");
        }
        
        var computedSignature = ComputeSignature(body);
        
        if (signature != computedSignature)
        {
            throw new InvalidSignatureException("Invalid signature");
        }
    }
    
    private string ComputeSignature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.SignatureSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
    
    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        
        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return body;
    }
    
    private async Task LogWebhookAsync(NafathWebhookPayload webhook, string rawPayload)
    {
        var log = new NafathWebhookLog
        {
            TransactionId = webhook.TransactionId,
            Status = webhook.Status,
            Payload = rawPayload,
            ReceivedAt = DateTime.UtcNow
        };
        
        await _webhookLogRepository.AddAsync(log);
    }
}
```

## Webhook Payload
```csharp
public class NafathWebhookPayload
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("nationalId")]
    public string? NationalId { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}
```

## Events
```csharp
public class NafathAuthenticationCompletedEvent : INotification
{
    public string TransactionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

public class NafathAuthenticationRejectedEvent : INotification
{
    public string TransactionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime RejectedAt { get; set; }
}

public class NafathAuthenticationExpiredEvent : INotification
{
    public string TransactionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
}

public class NafathAuthenticationCancelledEvent : INotification
{
    public string TransactionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}
```

## Testing Webhooks

### Local Testing with ngrok
```bash
# Start ngrok
ngrok http 5000

# Configure Nafath webhook URL
https://your-ngrok-url.ngrok.io/webhooks/nafath
```

### Manual Testing
```bash
curl -X POST https://your-api.com/webhooks/nafath \
  -H "Content-Type: application/json" \
  -H "X-Nafath-Signature: base64-signature" \
  -d '{
    "transactionId": "txn-12345",
    "status": "completed",
    "nationalId": "1234567890",
    "timestamp": "2024-01-15T10:30:00Z"
  }'
```

### Test Controller
```csharp
[ApiController]
[Route("api/test")]
[Authorize(Roles = "Admin")]
public class WebhookTestController : ControllerBase
{
    private readonly INafathWebhookHandler _webhookHandler;
    
    [HttpPost("nafath-webhook")]
    public async Task<IActionResult> TestNafathWebhook(
        [FromBody] NafathWebhookPayload payload)
    {
        // Create test context
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.ContentType = "application/json";
        
        var json = JsonSerializer.Serialize(payload);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        
        // Add signature
        var signature = ComputeTestSignature(json);
        context.Request.Headers["X-Nafath-Signature"] = signature;
        
        // Process webhook
        await _webhookHandler.HandleWebhookAsync(context);
        
        return Ok(new { status = "processed" });
    }
}
```

## Monitoring

### Webhook Logging
```csharp
public class NafathWebhookLog
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public string? Error { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
```

### Dashboard Endpoint
```csharp
[HttpGet("admin/webhooks")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<PagedResult<NafathWebhookLog>>> GetWebhookLogs(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 50)
{
    var logs = await _webhookLogRepository.GetPagedAsync(pageNumber, pageSize);
    return Ok(logs);
}
```

## Installation
```bash
dotnet add package ACommerce.Authentication.AspNetCore.NafathWH
```

## Dependencies

- ACommerce.Authentication.TwoFactor.Nafath
- ACommerce.Authentication.AspNetCore
- ACommerce.Realtime.Abstractions

## License

MIT