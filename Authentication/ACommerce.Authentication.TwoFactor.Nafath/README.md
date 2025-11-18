# ACommerce.Authentication.TwoFactor.Nafath

Nafath authentication integration for Saudi Arabia's National Single Sign-On.

## Overview

Complete integration with Nafath (النفاذ الوطني الموحد), Saudi Arabia's national authentication platform. Supports authentication flows, status polling, and real-time updates via SignalR.

## Key Features

✅ **Nafath Integration** - Official API integration  
✅ **Real-time Updates** - SignalR for status updates  
✅ **Status Polling** - Automatic polling with configurable intervals  
✅ **Mobile Deep Links** - Open Nafath app directly  
✅ **QR Code Support** - Display QR for mobile scanning  
✅ **Timeout Handling** - Automatic timeout and cleanup  
✅ **Sandbox Support** - Test mode for development  

## Configuration

### appsettings.json
```json
{
  "NafathSettings": {
    "ApiUrl": "https://api.nafath.sa",
    "AppId": "your-app-id",
    "AppSecret": "your-app-secret",
    "CallbackUrl": "https://yourdomain.com/api/auth/nafath/callback",
    "IsSandbox": false,
    "TimeoutSeconds": 120,
    "PollingIntervalSeconds": 2,
    "EnableRealtime": true,
    "UseMobileDeepLink": true
  }
}
```

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Nafath
builder.Services.AddNafathAuthentication(builder.Configuration);

// Add SignalR for real-time updates
builder.Services.AddSignalR();

var app = builder.Build();

// Map Nafath hub
app.MapHub<NafathHub>("/hubs/nafath");

app.MapControllers();
app.Run();
```

## Authentication Flow

### 1. Initiate Authentication
```csharp
[HttpPost("nafath/initiate")]
public async Task<ActionResult<NafathInitiationResult>> InitiateNafath(
    [FromBody] NafathAuthenticationRequest request)
{
    var result = await _nafathProvider.InitiateAuthenticationAsync(request);
    
    if (!result.IsSuccess)
        return BadRequest(new { message = result.Error });
    
    return Ok(new
    {
        transactionId = result.TransactionId,
        random = result.Random, // Display to user
        qrCodeUrl = result.QrCodeUrl,
        mobileDeepLink = result.MobileDeepLink,
        expiresAt = result.ExpiresAt
    });
}
```

### 2. Poll Status (Backend)
```csharp
// Automatic polling in background service
public class NafathPollingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var activeTransactions = await _transactionRepo.GetActiveAsync();
            
            foreach (var transaction in activeTransactions)
            {
                var status = await _nafathProvider.PollAuthenticationStatusAsync(
                    transaction.TransactionId);
                
                if (status.Status == NafathStatus.Completed)
                {
                    // Complete authentication
                    await CompleteAuthentication(transaction);
                    
                    // Notify via SignalR
                    await _realtimeHub.SendToUserAsync(
                        transaction.UserId,
                        "NafathCompleted",
                        new { transactionId = transaction.TransactionId });
                }
                else if (status.Status == NafathStatus.Rejected || 
                         status.Status == NafathStatus.Expired)
                {
                    // Notify failure
                    await _realtimeHub.SendToUserAsync(
                        transaction.UserId,
                        "NafathFailed",
                        new { transactionId = transaction.TransactionId, status = status.Status });
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
```

### 3. Complete Authentication
```csharp
[HttpPost("nafath/complete")]
public async Task<ActionResult<AuthenticationResult>> CompleteNafath(
    [FromBody] NafathCompletionRequest request)
{
    var result = await _nafathProvider.CompleteAuthenticationAsync(
        request.TransactionId);
    
    if (!result.IsSuccess)
        return BadRequest(new { message = result.Error });
    
    // Generate JWT tokens
    var authResult = await _authProvider.GenerateTokensForUser(result.UserId);
    
    return Ok(authResult);
}
```

## Frontend Integration

### React / Vue / Angular
```typescript
// 1. Initiate Nafath
const initiateNafath = async (nationalId: string) => {
  const response = await fetch('/api/auth/nafath/initiate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nationalId })
  });
  
  const data = await response.json();
  
  // Display random code to user
  displayRandomCode(data.random);
  
  // Show QR code
  displayQRCode(data.qrCodeUrl);
  
  // Or open mobile app
  if (isMobile) {
    window.location.href = data.mobileDeepLink;
  }
  
  // Connect to SignalR
  await connectToNafathHub(data.transactionId);
};

// 2. Connect to SignalR
const connectToNafathHub = async (transactionId: string) => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/nafath')
    .build();
  
  connection.on('ReceiveMessage', (method, data) => {
    if (method === 'NafathCompleted') {
      // Complete authentication
      completeNafath(data.transactionId);
    } else if (method === 'NafathFailed') {
      // Show error
      showError(`Authentication failed: ${data.status}`);
    }
  });
  
  await connection.start();
  
  // Join transaction group
  await connection.invoke('JoinTransaction', transactionId);
};

// 3. Complete authentication
const completeNafath = async (transactionId: string) => {
  const response = await fetch('/api/auth/nafath/complete', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ transactionId })
  });
  
  const { accessToken, refreshToken } = await response.json();
  
  // Store tokens
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
  
  // Redirect to app
  window.location.href = '/dashboard';
};
```

### .NET MAUI
```csharp
public class NafathAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HubConnection _hubConnection;
    
    public async Task<bool> AuthenticateWithNafathAsync(string nationalId)
    {
        // 1. Initiate
        var initResponse = await _httpClient.PostAsJsonAsync("/api/auth/nafath/initiate",
            new { nationalId });
        
        var initData = await initResponse.Content.ReadFromJsonAsync<NafathInitiationResult>();
        
        // 2. Show random code
        await ShowRandomCodeDialog(initData.Random);
        
        // 3. Open Nafath app
        if (await Launcher.CanOpenAsync(initData.MobileDeepLink))
        {
            await Launcher.OpenAsync(initData.MobileDeepLink);
        }
        
        // 4. Connect to SignalR
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://api.ACommerce.sa/hubs/nafath")
            .Build();
        
        var tcs = new TaskCompletionSource<bool>();
        
        _hubConnection.On<string, object>("ReceiveMessage", (method, data) =>
        {
            if (method == "NafathCompleted")
            {
                tcs.SetResult(true);
            }
            else if (method == "NafathFailed")
            {
                tcs.SetResult(false);
            }
        });
        
        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinTransaction", initData.TransactionId);
        
        // 5. Wait for completion
        var success = await tcs.Task;
        
        if (success)
        {
            // 6. Complete authentication
            var completeResponse = await _httpClient.PostAsJsonAsync("/api/auth/nafath/complete",
                new { transactionId = initData.TransactionId });
            
            var tokens = await completeResponse.Content.ReadFromJsonAsync<AuthenticationResult>();
            
            // Store tokens
            await SecureStorage.SetAsync("access_token", tokens.AccessToken);
            await SecureStorage.SetAsync("refresh_token", tokens.RefreshToken);
            
            return true;
        }
        
        return false;
    }
}
```

## UI Components

### Random Code Display
```html
<div class="nafath-code">
  <h3>رمز التحقق</h3>
  <div class="code">{{ randomCode }}</div>
  <p>أدخل هذا الرمز في تطبيق نفاذ</p>
</div>
```

### QR Code
```html
<div class="nafath-qr">
  <img :src="qrCodeUrl" alt="Nafath QR Code" />
  <p>امسح الرمز باستخدام تطبيق نفاذ</p>
</div>
```

### Status Indicator
```html
<div class="nafath-status">
  <div v-if="status === 'pending'">
    <spinner />
    <p>في انتظار التحقق...</p>
  </div>
  <div v-if="status === 'completed'">
    <check-icon />
    <p>تم التحقق بنجاح</p>
  </div>
  <div v-if="status === 'rejected'">
    <error-icon />
    <p>تم رفض التحقق</p>
  </div>
</div>
```

## Nafath Status Flow
```
1. Pending    → Initial state
2. Waiting    → User opened Nafath app
3. Completed  → User approved authentication
4. Rejected   → User rejected authentication
5. Expired    → Timeout (default 120 seconds)
6. Cancelled  → Cancelled by application
```

## Testing (Sandbox Mode)
```json
{
  "NafathSettings": {
    "IsSandbox": true,
    "ApiUrl": "https://sandbox.nafath.sa"
  }
}
```

In sandbox mode:
- Any national ID works
- Automatic completion after 10 seconds
- No real Nafath app needed

## Troubleshooting

### Issue: Timeout

**Solution:** Increase timeout or check user's Nafath app
```json
{
  "NafathSettings": {
    "TimeoutSeconds": 180
  }
}
```

### Issue: Polling too frequent

**Solution:** Increase polling interval
```json
{
  "NafathSettings": {
    "PollingIntervalSeconds": 5
  }
}
```

### Issue: Real-time not working

**Solution:** Check SignalR configuration and CORS

## Installation
```bash
dotnet add package ACommerce.Authentication.TwoFactor.Nafath
```

## Dependencies

- ACommerce.Authentication.Abstractions
- ACommerce.Realtime.Abstractions
- ACommerce.Realtime.SignalR

## License

MIT