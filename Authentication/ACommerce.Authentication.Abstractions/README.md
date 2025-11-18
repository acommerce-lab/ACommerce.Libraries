# ACommerce.Authentication.Abstractions

Provider-agnostic authentication abstractions supporting multiple authentication methods.

## Overview

Comprehensive authentication abstractions supporting JWT, OAuth, OpenIddict, Microsoft Identity, and custom providers. Includes two-factor authentication (2FA) with multiple methods including Nafath for Saudi Arabia.

## Key Features

✅ **Multi-Provider** - JWT, OAuth, OpenIddict, Microsoft Identity  
✅ **2FA Support** - SMS, Email, Authenticator App, Nafath  
✅ **Token Management** - Access, Refresh, Identity tokens  
✅ **Provider Agnostic** - Interface-based design  
✅ **Extensible** - Easy to add custom providers  

## Core Interfaces

### IAuthenticationProvider
Main authentication provider interface

**Methods:**
- `AuthenticateAsync(credentials)` - Basic authentication
- `RefreshTokenAsync(refreshToken)` - Token refresh
- `RevokeTokenAsync(token)` - Token revocation
- `ValidateTokenAsync(token)` - Token validation
- `GetUserInfoAsync(token)` - User information retrieval

### ITwoFactorProvider
Two-factor authentication interface

**Methods:**
- `SendCodeAsync(userId, method)` - Send 2FA code
- `VerifyCodeAsync(userId, code)` - Verify 2FA code
- `GetQrCodeAsync(userId)` - Get QR code for authenticator apps
- `ValidateAuthenticatorCodeAsync(userId, code)` - Validate authenticator code

### ITokenProvider
Token generation and validation

**Methods:**
- `GenerateAccessTokenAsync(user, claims)` - Generate access token
- `GenerateRefreshTokenAsync(userId)` - Generate refresh token
- `GenerateIdentityTokenAsync(user, claims)` - Generate identity token
- `ValidateTokenAsync(token, type)` - Validate token
- `RevokeTokenAsync(token)` - Revoke token
- `GetTokenClaimsAsync(token)` - Extract claims from token

### INafathProvider
Nafath-specific authentication (Saudi Arabia)

**Methods:**
- `InitiateAuthenticationAsync(nationalId)` - Start Nafath flow
- `PollAuthenticationStatusAsync(transactionId)` - Check status
- `CompleteAuthenticationAsync(transactionId)` - Complete authentication
- `CancelAuthenticationAsync(transactionId)` - Cancel authentication

## Domain Models

### AuthenticationRequest
```csharp
public class AuthenticationRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string? Provider { get; set; } // jwt, oauth, openiddict
    public Dictionary<string, string>? AdditionalParameters { get; set; }
}
```

### AuthenticationResult
```csharp
public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? IdentityToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public List<TwoFactorMethod> AvailableMethods { get; set; }
    public string? UserId { get; set; }
    public string? Error { get; set; }
}
```

### TwoFactorRequest
```csharp
public class TwoFactorRequest
{
    public string UserId { get; set; }
    public TwoFactorMethod Method { get; set; } // SMS, Email, Authenticator, Nafath
    public string? PhoneNumber { get; set; } // For SMS
    public string? Email { get; set; } // For Email
    public string? NationalId { get; set; } // For Nafath
}
```

### TwoFactorResult
```csharp
public class TwoFactorResult
{
    public bool IsSuccess { get; set; }
    public string? TransactionId { get; set; } // For Nafath
    public DateTime? ExpiresAt { get; set; }
    public string? QrCodeUrl { get; set; } // For Authenticator
    public string? Error { get; set; }
}
```

### TokenValidationResult
```csharp
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public List<Claim> Claims { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Error { get; set; }
}
```

## Enums

### AuthenticationProvider
```csharp
public enum AuthenticationProvider
{
    JWT = 1,
    OAuth = 2,
    OpenIddict = 3,
    MicrosoftIdentity = 4,
    Custom = 99
}
```

### TwoFactorMethod
```csharp
public enum TwoFactorMethod
{
    SMS = 1,
    Email = 2,
    AuthenticatorApp = 3,
    Nafath = 4
}
```

### TokenType
```csharp
public enum TokenType
{
    AccessToken = 1,
    RefreshToken = 2,
    IdentityToken = 3
}
```

### NafathStatus
```csharp
public enum NafathStatus
{
    Pending = 1,
    Waiting = 2,
    Completed = 3,
    Rejected = 4,
    Expired = 5,
    Cancelled = 6
}
```

## Usage Examples

### Basic Authentication
```csharp
var request = new AuthenticationRequest
{
    Username = "user@example.com",
    Password = "password123",
    Provider = "jwt"
};

var result = await _authProvider.AuthenticateAsync(request);

if (result.IsSuccess)
{
    if (result.RequiresTwoFactor)
    {
        // Handle 2FA
        await Handle2FA(result.UserId, result.AvailableMethods);
    }
    else
    {
        // Authentication complete
        var accessToken = result.AccessToken;
    }
}
```

### Two-Factor Authentication
```csharp
// Send 2FA code
var twoFactorRequest = new TwoFactorRequest
{
    UserId = "user-id",
    Method = TwoFactorMethod.SMS,
    PhoneNumber = "+966501234567"
};

var sendResult = await _twoFactorProvider.SendCodeAsync(twoFactorRequest);

// Verify code
var verifyResult = await _twoFactorProvider.VerifyCodeAsync("user-id", "123456");

if (verifyResult.IsSuccess)
{
    // Complete authentication
}
```

### Nafath Authentication
```csharp
// Initiate Nafath
var nafathRequest = new NafathAuthenticationRequest
{
    NationalId = "1234567890"
};

var initResult = await _nafathProvider.InitiateAuthenticationAsync(nafathRequest);

if (initResult.IsSuccess)
{
    var transactionId = initResult.TransactionId;
    
    // Poll for status
    while (true)
    {
        var status = await _nafathProvider.PollAuthenticationStatusAsync(transactionId);
        
        if (status.Status == NafathStatus.Completed)
        {
            // Complete authentication
            var completeResult = await _nafathProvider.CompleteAuthenticationAsync(transactionId);
            break;
        }
        else if (status.Status == NafathStatus.Rejected || status.Status == NafathStatus.Expired)
        {
            // Handle failure
            break;
        }
        
        await Task.Delay(2000); // Poll every 2 seconds
    }
}
```

### Token Refresh
```csharp
var refreshResult = await _authProvider.RefreshTokenAsync(refreshToken);

if (refreshResult.IsSuccess)
{
    var newAccessToken = refreshResult.AccessToken;
    var newRefreshToken = refreshResult.RefreshToken;
}
```

### Token Validation
```csharp
var validationResult = await _tokenProvider.ValidateTokenAsync(token, TokenType.AccessToken);

if (validationResult.IsValid)
{
    var userId = validationResult.UserId;
    var claims = validationResult.Claims;
}
```

## Available Implementations

- **ACommerce.Authentication.Providers.JWT** - JWT authentication
- **ACommerce.Authentication.Providers.OAuth** - OAuth 2.0
- **ACommerce.Authentication.Providers.OpenIddict** - OpenIddict integration
- **ACommerce.Authentication.Providers.MicrosoftIdentity** - Microsoft Identity
- **ACommerce.Authentication.TwoFactor.SMS** - SMS 2FA (Twilio, AWS SNS)
- **ACommerce.Authentication.TwoFactor.Email** - Email 2FA
- **ACommerce.Authentication.TwoFactor.Authenticator** - Authenticator App 2FA
- **ACommerce.Authentication.TwoFactor.Nafath** - Nafath integration

## Installation
```bash
dotnet add package ACommerce.Authentication.Abstractions
```

## Dependencies

- SharedKernel.Abstractions

## License

MIT