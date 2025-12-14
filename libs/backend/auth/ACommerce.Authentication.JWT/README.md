# ACommerce.Authentication.Providers.JWT

JWT (JSON Web Token) authentication provider implementation.

## Overview

Production-ready JWT authentication provider with token generation, validation, refresh token support, and integration with ASP.NET Core Identity.

## Key Features

✅ **JWT Generation** - Access and refresh tokens  
✅ **Token Validation** - Signature and expiration checks  
✅ **Refresh Tokens** - Secure token renewal  
✅ **Claims Management** - Custom claims support  
✅ **ASP.NET Core Integration** - Middleware support  
✅ **Configurable** - Flexible configuration options  

## Configuration

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-32-characters",
    "Issuer": "https://api.ACommerce.sa",
    "Audience": "https://ACommerce.sa",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true,
    "ClockSkew": 5
  }
}
```

## Setup

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Or manually:
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});

// Register providers
builder.Services.AddScoped<IAuthenticationProvider, JwtAuthenticationProvider>();
builder.Services.AddScoped<ITokenProvider, JwtTokenProvider>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## Usage

### Generate Tokens
```csharp
public class AuthService
{
    private readonly ITokenProvider _tokenProvider;
    
    public async Task<AuthenticationResult> Login(string username, string password)
    {
        // Validate credentials
        var user = await ValidateUser(username, password);
        
        if (user == null)
            return AuthenticationResult.Failure("Invalid credentials");
        
        // Generate claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        // Generate tokens
        var accessToken = await _tokenProvider.GenerateAccessTokenAsync(user, claims);
        var refreshToken = await _tokenProvider.GenerateRefreshTokenAsync(user.Id);
        
        return new AuthenticationResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }
}
```

### Validate Tokens
```csharp
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ITokenProvider _tokenProvider;
    
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // Token automatically validated by [Authorize]
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Or manually validate
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var validation = await _tokenProvider.ValidateTokenAsync(token, TokenType.AccessToken);
        
        if (!validation.IsValid)
            return Unauthorized();
        
        var user = await _userService.GetByIdAsync(userId);
        return Ok(user);
    }
}
```

### Refresh Tokens
```csharp
[HttpPost("refresh")]
public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
{
    var result = await _authProvider.RefreshTokenAsync(request.RefreshToken);
    
    if (!result.IsSuccess)
        return BadRequest(new { message = result.Error });
    
    return Ok(result);
}
```

### Revoke Tokens
```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    await _authProvider.RevokeTokenAsync(token);
    
    return NoContent();
}
```

## Token Structure

### Access Token
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id",
    "name": "username",
    "email": "user@example.com",
    "role": "Admin",
    "iss": "https://api.ACommerce.sa",
    "aud": "https://ACommerce.sa",
    "exp": 1234567890,
    "iat": 1234567800
  },
  "signature": "..."
}
```

## Client Integration

### JavaScript/TypeScript
```javascript
// Login
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});

const { accessToken, refreshToken } = await response.json();

// Store tokens
localStorage.setItem('accessToken', accessToken);
localStorage.setItem('refreshToken', refreshToken);

// Use token
const apiResponse = await fetch('/api/users/me', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});

// Refresh token
const refreshResponse = await fetch('/api/auth/refresh', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ refreshToken })
});
```

### .NET MAUI / Xamarin
```csharp
public class AuthService
{
    private readonly HttpClient _httpClient;
    
    public async Task<AuthenticationResult> LoginAsync(string username, string password)
    {
        var request = new AuthenticationRequest
        {
            Username = username,
            Password = password
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
            
            // Store tokens securely
            await SecureStorage.SetAsync("access_token", result.AccessToken);
            await SecureStorage.SetAsync("refresh_token", result.RefreshToken);
            
            return result;
        }
        
        return AuthenticationResult.Failure("Login failed");
    }
    
    public async Task<string> GetAccessTokenAsync()
    {
        return await SecureStorage.GetAsync("access_token");
    }
}
```

## Security Best Practices

✅ **Secure Secret Key** - Min 32 characters, store in secrets/env  
✅ **Short Access Token Lifetime** - 15-30 minutes recommended  
✅ **Secure Refresh Tokens** - Store hashed in database  
✅ **HTTPS Only** - Never send tokens over HTTP  
✅ **Token Rotation** - Issue new refresh token on refresh  
✅ **Revocation List** - Track revoked tokens (Redis recommended)  

## Installation
```bash
dotnet add package ACommerce.Authentication.Providers.JWT
```

## Dependencies

- ACommerce.Authentication.Abstractions
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.IdentityModel.Tokens.Jwt

## License

MIT