# ACommerce.Authentication.AspNetCore

ASP.NET Core authentication extensions and middleware.

## Overview

Complete ASP.NET Core authentication infrastructure including middleware, filters, services, and extensions. Simplifies authentication setup and provides common authentication patterns.

## Key Features

✅ **Authentication Middleware** - JWT, Cookie, External providers  
✅ **Authorization Policies** - Role, claim, permission-based  
✅ **Action Filters** - Require authentication, 2FA, verified email  
✅ **Service Extensions** - Easy setup and configuration  
✅ **Token Management** - Automatic token refresh  
✅ **User Context** - Access current user easily  

## Setup

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add ACommerce Authentication
builder.Services.AddACommerceAuthentication(builder.Configuration);

// Or manual setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "JWT_OR_COOKIE";
    options.DefaultChallengeScheme = "JWT_OR_COOKIE";
})
.AddJwtBearer("JWT", options =>
{
    // JWT configuration
})
.AddCookie("COOKIE", options =>
{
    // Cookie configuration
})
.AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            return "JWT";
        return "COOKIE";
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireVerifiedEmail", policy =>
        policy.RequireClaim("email_verified", "true"));
    
    options.AddPolicy("RequireTwoFactor", policy =>
        policy.RequireClaim("amr", "mfa"));
});

// Add ACommerce services
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITokenRefreshService, TokenRefreshService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## Services

### IUserContextService

Access current authenticated user
```csharp
public interface IUserContextService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasClaim(string claimType, string claimValue);
    Task<User?> GetCurrentUserAsync();
}
```

**Usage:**
```csharp
public class OrdersController : ControllerBase
{
    private readonly IUserContextService _userContext;
    
    [HttpGet("my-orders")]
    [Authorize]
    public async Task<ActionResult<List<Order>>> GetMyOrders()
    {
        var userId = _userContext.UserId;
        var orders = await _orderService.GetByUserAsync(userId);
        return Ok(orders);
    }
}
```

### ITokenRefreshService

Automatic token refresh
```csharp
public interface ITokenRefreshService
{
    Task<string?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string token);
    Task CleanupExpiredTokensAsync();
}
```

**Middleware:**
```csharp
public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(
        HttpContext context,
        ITokenRefreshService tokenService)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);
            
            // Check if token is about to expire (within 5 minutes)
            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(token))
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var expiresAt = jwtToken.ValidTo;
                
                if (expiresAt < DateTime.UtcNow.AddMinutes(5))
                {
                    var refreshToken = context.Request.Cookies["refresh_token"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var newToken = await tokenService.RefreshTokenAsync(refreshToken);
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            context.Response.Headers.Add("X-New-Token", newToken);
                        }
                    }
                }
            }
        }
        
        await _next(context);
    }
}
```

## Action Filters

### RequireAuthenticationAttribute
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthenticationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Authentication required"
            });
        }
    }
}
```

### RequireTwoFactorAttribute
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireTwoFactorAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var has2FA = user.HasClaim(c => c.Type == "amr" && c.Value == "mfa");
        
        if (!has2FA)
        {
            context.Result = new ObjectResult(new
            {
                message = "Two-factor authentication required",
                requiresTwoFactor = true
            })
            {
                StatusCode = 403
            };
        }
    }
}
```

### RequireVerifiedEmailAttribute
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireVerifiedEmailAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var emailVerified = user.HasClaim(c =>
            c.Type == "email_verified" && c.Value == "true");
        
        if (!emailVerified)
        {
            context.Result = new ObjectResult(new
            {
                message = "Email verification required",
                requiresEmailVerification = true
            })
            {
                StatusCode = 403
            };
        }
    }
}
```

### RequirePermissionAttribute
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : ActionFilterAttribute
{
    private readonly string _permission;
    
    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var hasPermission = user.HasClaim(c =>
            c.Type == "permission" && c.Value == _permission);
        
        if (!hasPermission)
        {
            context.Result = new ObjectResult(new
            {
                message = $"Permission '{_permission}' required"
            })
            {
                StatusCode = 403
            };
        }
    }
}
```

## Usage Examples

### Basic Authentication
```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        // Public endpoint
        return Ok(await _productService.GetAllAsync());
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto)
    {
        // Requires authentication
        return Ok(await _productService.CreateAsync(dto));
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Requires Admin role
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
```

### Custom Filters
```csharp
[ApiController]
[Route("api/admin")]
[RequireAuthentication]
[RequireTwoFactor]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult> GetDashboard()
    {
        // Requires: Authentication + 2FA + Admin role
        return Ok();
    }
    
    [HttpPost("users")]
    [RequirePermission("users.create")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        // Requires: Authentication + 2FA + "users.create" permission
        return Ok();
    }
}
```

### Email Verification Flow
```csharp
[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    [HttpGet("profile")]
    [Authorize]
    [RequireVerifiedEmail]
    public async Task<ActionResult<UserProfile>> GetProfile()
    {
        // Requires: Authentication + Verified email
        var userId = _userContext.UserId;
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }
    
    [HttpPost("verify-email")]
    [Authorize]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var userId = _userContext.UserId;
        var result = await _emailVerificationService.VerifyAsync(userId, dto.Code);
        
        if (result.IsSuccess)
        {
            // Add email_verified claim and refresh token
            var newToken = await _authService.AddEmailVerifiedClaimAsync(userId);
            return Ok(new { accessToken = newToken });
        }
        
        return BadRequest(new { message = result.Error });
    }
}
```

## Extension Methods

### HttpContext Extensions
```csharp
public static class HttpContextExtensions
{
    public static string? GetUserId(this HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    
    public static string? GetUserName(this HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.Name)?.Value;
    }
    
    public static string? GetEmail(this HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.Email)?.Value;
    }
    
    public static bool IsInRole(this HttpContext context, string role)
    {
        return context.User.IsInRole(role);
    }
    
    public static string? GetBearerToken(this HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length);
        }
        return null;
    }
}
```

### ClaimsPrincipal Extensions
```csharp
public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        return principal.HasClaim(c => c.Type == "permission" && c.Value == permission);
    }
    
    public static bool IsEmailVerified(this ClaimsPrincipal principal)
    {
        return principal.HasClaim(c =>
            c.Type == "email_verified" && c.Value == "true");
    }
    
    public static bool HasTwoFactor(this ClaimsPrincipal principal)
    {
        return principal.HasClaim(c => c.Type == "amr" && c.Value == "mfa");
    }
}
```

## Installation
```bash
dotnet add package ACommerce.Authentication.AspNetCore
```

## Dependencies

- ACommerce.Authentication.Abstractions
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Authentication.Cookies

## License

MIT