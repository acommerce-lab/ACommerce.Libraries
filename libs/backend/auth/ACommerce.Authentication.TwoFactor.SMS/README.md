# ACommerce.Authentication.TwoFactor.SMS

SMS-based two-factor authentication with support for multiple providers.

## Overview

Complete SMS 2FA implementation supporting Twilio, AWS SNS, and custom SMS providers. Includes code generation, validation, rate limiting, and retry logic.

## Key Features

✅ **Multiple Providers** - Twilio, AWS SNS, Custom HTTP  
✅ **Code Generation** - Secure random code generation  
✅ **Rate Limiting** - Prevent SMS flooding  
✅ **Retry Logic** - Automatic retry on failure  
✅ **Expiration** - Time-limited codes  
✅ **Verification Tracking** - Track attempts and blocks  

## Configuration

### appsettings.json
```json
{
  "SmsSettings": {
    "Provider": "Twilio",
    
    "Twilio": {
      "AccountSid": "your-account-sid",
      "AuthToken": "your-auth-token",
      "FromPhoneNumber": "+966501234567"
    },
    
    "AWS_SNS": {
      "AccessKey": "your-aws-access-key",
      "SecretKey": "your-aws-secret-key",
      "Region": "us-east-1"
    },
    
    "CustomHttp": {
      "ApiUrl": "https://sms-provider.com/api/send",
      "ApiKey": "your-api-key",
      "Method": "POST"
    },
    
    "CodeSettings": {
      "CodeLength": 6,
      "CodeType": "Numeric",
      "ExpirationMinutes": 5,
      "MaxAttempts": 3,
      "ResendDelaySeconds": 60,
      "BlockDurationMinutes": 30
    },
    
    "RateLimiting": {
      "MaxSmsPerHour": 5,
      "MaxSmsPerDay": 10
    },
    
    "Templates": {
      "Arabic": "رمز التحقق الخاص بك: {code}\nصالح لمدة {minutes} دقائق",
      "English": "Your verification code is: {code}\nValid for {minutes} minutes"
    }
  }
}
```

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add SMS 2FA
builder.Services.AddSmsTwoFactor(builder.Configuration);

// Or manual setup
builder.Services.Configure<SmsSettings>(
    builder.Configuration.GetSection("SmsSettings"));

builder.Services.AddScoped<ITwoFactorProvider, SmsTwoFactorProvider>();
builder.Services.AddScoped<ISmsProvider, TwilioSmsProvider>(); // or SnsSmsProvider, CustomHttpSmsProvider

var app = builder.Build();
app.Run();
```

## Usage

### Send Verification Code
```csharp
[HttpPost("send-sms-code")]
public async Task<ActionResult<TwoFactorResult>> SendSmsCode(
    [FromBody] SendSmsCodeRequest request)
{
    var twoFactorRequest = new TwoFactorRequest
    {
        UserId = request.UserId,
        Method = TwoFactorMethod.SMS,
        PhoneNumber = request.PhoneNumber
    };
    
    var result = await _twoFactorProvider.SendCodeAsync(twoFactorRequest);
    
    if (!result.IsSuccess)
    {
        return BadRequest(new { message = result.Error });
    }
    
    return Ok(new
    {
        success = true,
        expiresAt = result.ExpiresAt,
        resendAfter = DateTime.UtcNow.AddSeconds(60)
    });
}
```

### Verify Code
```csharp
[HttpPost("verify-sms-code")]
public async Task<ActionResult<AuthenticationResult>> VerifySmsCode(
    [FromBody] VerifySmsCodeRequest request)
{
    var result = await _twoFactorProvider.VerifyCodeAsync(
        request.UserId, 
        request.Code);
    
    if (!result.IsSuccess)
    {
        return BadRequest(new { message = result.Error });
    }
    
    // Complete authentication
    var authResult = await _authProvider.CompleteAuthenticationAsync(request.UserId);
    
    return Ok(authResult);
}
```

### Resend Code
```csharp
[HttpPost("resend-sms-code")]
public async Task<ActionResult<TwoFactorResult>> ResendSmsCode(
    [FromBody] ResendSmsCodeRequest request)
{
    // Check rate limit
    var canResend = await _twoFactorProvider.CanResendAsync(request.UserId);
    
    if (!canResend)
    {
        return BadRequest(new { message = "Please wait before requesting a new code" });
    }
    
    var twoFactorRequest = new TwoFactorRequest
    {
        UserId = request.UserId,
        Method = TwoFactorMethod.SMS,
        PhoneNumber = request.PhoneNumber
    };
    
    var result = await _twoFactorProvider.SendCodeAsync(twoFactorRequest);
    
    return Ok(result);
}
```

## Provider Implementations

### Twilio Provider
```csharp
public class TwilioSmsProvider : ISmsProvider
{
    private readonly TwilioSettings _settings;
    private readonly TwilioRestClient _client;
    
    public TwilioSmsProvider(IOptions<TwilioSettings> settings)
    {
        _settings = settings.Value;
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        _client = TwilioClient.GetRestClient();
    }
    
    public async Task<SmsResult> SendAsync(SmsMessage message)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message.Body,
                from: new PhoneNumber(_settings.FromPhoneNumber),
                to: new PhoneNumber(message.ToPhoneNumber),
                client: _client
            );
            
            return new SmsResult
            {
                IsSuccess = true,
                MessageId = messageResource.Sid,
                Status = messageResource.Status.ToString(),
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new SmsResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
    
    public async Task<SmsDeliveryStatus> GetStatusAsync(string messageId)
    {
        try
        {
            var message = await MessageResource.FetchAsync(messageId, client: _client);
            
            return new SmsDeliveryStatus
            {
                MessageId = messageId,
                Status = message.Status.ToString(),
                DeliveredAt = message.DateSent
            };
        }
        catch (Exception ex)
        {
            return new SmsDeliveryStatus
            {
                MessageId = messageId,
                Status = "Failed",
                Error = ex.Message
            };
        }
    }
}
```

### AWS SNS Provider
```csharp
public class SnsSmsProvider : ISmsProvider
{
    private readonly SnsSettings _settings;
    private readonly IAmazonSimpleNotificationService _snsClient;
    
    public SnsSmsProvider(IOptions<SnsSettings> settings)
    {
        _settings = settings.Value;
        _snsClient = new AmazonSimpleNotificationServiceClient(
            _settings.AccessKey,
            _settings.SecretKey,
            RegionEndpoint.GetBySystemName(_settings.Region)
        );
    }
    
    public async Task<SmsResult> SendAsync(SmsMessage message)
    {
        try
        {
            var request = new PublishRequest
            {
                Message = message.Body,
                PhoneNumber = message.ToPhoneNumber,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "AWS.SNS.SMS.SMSType",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = "Transactional"
                        }
                    }
                }
            };
            
            var response = await _snsClient.PublishAsync(request);
            
            return new SmsResult
            {
                IsSuccess = true,
                MessageId = response.MessageId,
                Status = "Sent",
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new SmsResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
    
    public Task<SmsDeliveryStatus> GetStatusAsync(string messageId)
    {
        // AWS SNS doesn't provide delivery status via API
        return Task.FromResult(new SmsDeliveryStatus
        {
            MessageId = messageId,
            Status = "Unknown"
        });
    }
}
```

### Custom HTTP Provider
```csharp
public class CustomHttpSmsProvider : ISmsProvider
{
    private readonly CustomHttpSettings _settings;
    private readonly HttpClient _httpClient;
    
    public CustomHttpSmsProvider(
        IOptions<CustomHttpSettings> settings,
        HttpClient httpClient)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
    }
    
    public async Task<SmsResult> SendAsync(SmsMessage message)
    {
        try
        {
            var request = new HttpRequestMessage(
                _settings.Method == "POST" ? HttpMethod.Post : HttpMethod.Get,
                _settings.ApiUrl
            );
            
            request.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");
            
            var payload = new
            {
                to = message.ToPhoneNumber,
                message = message.Body,
                sender = _settings.SenderName
            };
            
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CustomSmsResponse>(content);
                
                return new SmsResult
                {
                    IsSuccess = true,
                    MessageId = result.MessageId,
                    Status = result.Status,
                    SentAt = DateTime.UtcNow
                };
            }
            else
            {
                return new SmsResult
                {
                    IsSuccess = false,
                    Error = $"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}"
                };
            }
        }
        catch (Exception ex)
        {
            return new SmsResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
    
    public async Task<SmsDeliveryStatus> GetStatusAsync(string messageId)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.StatusUrl}?messageId={messageId}"
            );
            
            request.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");
            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CustomSmsStatusResponse>(content);
            
            return new SmsDeliveryStatus
            {
                MessageId = messageId,
                Status = result.Status,
                DeliveredAt = result.DeliveredAt
            };
        }
        catch (Exception ex)
        {
            return new SmsDeliveryStatus
            {
                MessageId = messageId,
                Status = "Unknown",
                Error = ex.Message
            };
        }
    }
}
```

## Code Generation
```csharp
public class SmsCodeGenerator
{
    private readonly SmsSettings _settings;
    private readonly Random _random = new Random();
    
    public string GenerateCode()
    {
        var length = _settings.CodeSettings.CodeLength;
        var type = _settings.CodeSettings.CodeType;
        
        return type switch
        {
            CodeType.Numeric => GenerateNumericCode(length),
            CodeType.Alphanumeric => GenerateAlphanumericCode(length),
            CodeType.AlphabeticUppercase => GenerateAlphabeticCode(length, true),
            CodeType.AlphabeticLowercase => GenerateAlphabeticCode(length, false),
            _ => GenerateNumericCode(length)
        };
    }
    
    private string GenerateNumericCode(int length)
    {
        var code = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            code.Append(_random.Next(0, 10));
        }
        return code.ToString();
    }
    
    private string GenerateAlphanumericCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
    
    private string GenerateAlphabeticCode(int length, bool uppercase)
    {
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        var chars = uppercase ? upperChars : lowerChars;
        
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
```

## Rate Limiting
```csharp
public class SmsRateLimiter
{
    private readonly IDistributedCache _cache;
    private readonly SmsSettings _settings;
    
    public async Task<bool> CanSendAsync(string userId)
    {
        var hourKey = $"sms_hour_{userId}_{DateTime.UtcNow:yyyyMMddHH}";
        var dayKey = $"sms_day_{userId}_{DateTime.UtcNow:yyyyMMdd}";
        
        var hourCount = await GetCountAsync(hourKey);
        var dayCount = await GetCountAsync(dayKey);
        
        if (hourCount >= _settings.RateLimiting.MaxSmsPerHour)
            return false;
        
        if (dayCount >= _settings.RateLimiting.MaxSmsPerDay)
            return false;
        
        await IncrementCountAsync(hourKey, TimeSpan.FromHours(1));
        await IncrementCountAsync(dayKey, TimeSpan.FromDays(1));
        
        return true;
    }
    
    private async Task<int> GetCountAsync(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return int.TryParse(value, out var count) ? count : 0;
    }
    
    private async Task IncrementCountAsync(string key, TimeSpan expiration)
    {
        var count = await GetCountAsync(key);
        await _cache.SetStringAsync(
            key,
            (count + 1).ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            }
        );
    }
}
```

## Verification Tracking
```csharp
public class SmsVerificationTracker
{
    private readonly IBaseAsyncRepository<SmsVerification> _repository;
    private readonly SmsSettings _settings;
    
    public async Task<SmsVerification> CreateAsync(string userId, string phoneNumber, string code)
    {
        var verification = new SmsVerification
        {
            UserId = userId,
            PhoneNumber = phoneNumber,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.CodeSettings.ExpirationMinutes),
            MaxAttempts = _settings.CodeSettings.MaxAttempts,
            Attempts = 0,
            IsVerified = false,
            IsBlocked = false
        };
        
        return await _repository.AddAsync(verification);
    }
    
    public async Task<VerificationResult> VerifyAsync(string userId, string code)
    {
        var verifications = await _repository.GetAllWithPredicateAsync(
            v => v.UserId == userId && !v.IsVerified && !v.IsBlocked);
        
        var verification = verifications
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefault();
        
        if (verification == null)
        {
            return VerificationResult.Failure("No pending verification found");
        }
        
        if (verification.IsBlocked)
        {
            return VerificationResult.Failure("Verification blocked due to too many attempts");
        }
        
        if (DateTime.UtcNow > verification.ExpiresAt)
        {
            return VerificationResult.Failure("Code has expired");
        }
        
        verification.Attempts++;
        
        if (verification.Code == code)
        {
            verification.IsVerified = true;
            verification.VerifiedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(verification);
            
            return VerificationResult.Success();
        }
        else
        {
            if (verification.Attempts >= verification.MaxAttempts)
            {
                verification.IsBlocked = true;
                verification.BlockedAt = DateTime.UtcNow;
                verification.BlockedUntil = DateTime.UtcNow.AddMinutes(
                    _settings.CodeSettings.BlockDurationMinutes);
            }
            
            await _repository.UpdateAsync(verification);
            
            return VerificationResult.Failure(
                $"Invalid code. {verification.MaxAttempts - verification.Attempts} attempts remaining");
        }
    }
}
```

## Frontend Integration

### React
```javascript
const [phoneNumber, setPhoneNumber] = useState('');
const [code, setCode] = useState('');
const [step, setStep] = useState('phone'); // 'phone' or 'code'
const [resendTimer, setResendTimer] = useState(0);

const sendCode = async () => {
  try {
    const response = await fetch('/api/auth/send-sms-code', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        userId: userId,
        phoneNumber: phoneNumber
      })
    });
    
    if (response.ok) {
      setStep('code');
      setResendTimer(60);
      
      // Start countdown
      const interval = setInterval(() => {
        setResendTimer(prev => {
          if (prev <= 1) {
            clearInterval(interval);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
  } catch (error) {
    console.error('Failed to send code:', error);
  }
};

const verifyCode = async () => {
  try {
    const response = await fetch('/api/auth/verify-sms-code', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        userId: userId,
        code: code
      })
    });
    
    if (response.ok) {
      const data = await response.json();
      // Store tokens and redirect
      localStorage.setItem('accessToken', data.accessToken);
      window.location.href = '/dashboard';
    } else {
      const error = await response.json();
      alert(error.message);
    }
  } catch (error) {
    console.error('Failed to verify code:', error);
  }
};

return (
  <div>
    {step === 'phone' ? (
      <div>
        <input
          type="tel"
          value={phoneNumber}
          onChange={(e) => setPhoneNumber(e.target.value)}
          placeholder="+966501234567"
        />
        <button onClick={sendCode}>إرسال رمز التحقق</button>
      </div>
    ) : (
      <div>
        <input
          type="text"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          placeholder="000000"
          maxLength={6}
        />
        <button onClick={verifyCode}>تحقق</button>
        {resendTimer > 0 ? (
          <p>إعادة الإرسال بعد {resendTimer} ثانية</p>
        ) : (
          <button onClick={sendCode}>إعادة إرسال الرمز</button>
        )}
      </div>
    )}
  </div>
);
```

## Installation
```bash
dotnet add package ACommerce.Authentication.TwoFactor.SMS
dotnet add package Twilio
dotnet add package AWSSDK.SimpleNotificationService
```

## Dependencies

- ACommerce.Authentication.Abstractions
- Twilio (6.x) - for Twilio provider
- AWSSDK.SimpleNotificationService (3.x) - for SNS provider

## License

MIT