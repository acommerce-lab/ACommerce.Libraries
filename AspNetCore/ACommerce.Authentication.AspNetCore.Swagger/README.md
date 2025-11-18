# ACommerce.Authentication.AspNetCore.Swagger

Swagger/OpenAPI extensions for authentication documentation and testing.

## Overview

Automatic Swagger configuration for authentication endpoints with support for JWT, OAuth, OpenID Connect, and API keys. Includes interactive authentication testing directly from Swagger UI.

## Key Features

✅ **Auto Documentation** - Automatic endpoint documentation  
✅ **Security Schemes** - JWT, OAuth2, OpenID Connect, ApiKey  
✅ **Interactive Testing** - Test authentication from Swagger UI  
✅ **Custom Operations** - Add authentication requirements  
✅ **XML Comments** - Include XML documentation  
✅ **Multiple Auth** - Support multiple authentication schemes  

## Configuration

### appsettings.json
```json
{
  "SwaggerSettings": {
    "Title": "ACommerce API",
    "Version": "v1",
    "Description": "ACommerce API with Authentication",
    "TermsOfService": "https://ACommerce.sa/terms",
    "Contact": {
      "Name": "ACommerce Support",
      "Email": "support@ACommerce.sa",
      "Url": "https://ACommerce.sa/support"
    },
    "License": {
      "Name": "MIT",
      "Url": "https://opensource.org/licenses/MIT"
    },
    "Authentication": {
      "Type": "JWT",
      "Schemes": {
        "JWT": {
          "Type": "http",
          "Scheme": "bearer",
          "BearerFormat": "JWT",
          "Description": "Enter your JWT token"
        },
        "OAuth2": {
          "Type": "oauth2",
          "Flows": {
            "AuthorizationCode": {
              "AuthorizationUrl": "https://auth.ACommerce.sa/connect/authorize",
              "TokenUrl": "https://auth.ACommerce.sa/connect/token",
              "Scopes": {
                "openid": "OpenID",
                "profile": "Profile",
                "email": "Email",
                "api": "API Access"
              }
            }
          }
        },
        "ApiKey": {
          "Type": "apiKey",
          "Name": "X-API-Key",
          "In": "header",
          "Description": "Enter your API key"
        }
      }
    },
    "EnableXmlComments": true,
    "XmlCommentsPath": "ACommerce.Api.xml",
    "IncludeXmlComments": true
  }
}
```

## Setup

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Add ACommerce Swagger with Authentication
builder.Services.AddACommerceSwagger(builder.Configuration);

// Or manual setup
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ACommerce API",
        Version = "v1",
        Description = "ACommerce API with Authentication",
        Contact = new OpenApiContact
        {
            Name = "ACommerce Support",
            Email = "support@ACommerce.sa",
            Url = new Uri("https://ACommerce.sa/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token in the format: Bearer {token}",
        In = ParameterLocation.Header,
        Name = "Authorization"
    });
    
    // OAuth2 Authentication
    options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://auth.ACommerce.sa/connect/authorize"),
                TokenUrl = new Uri("https://auth.ACommerce.sa/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "Profile" },
                    { "email", "Email" },
                    { "api", "API Access" },
                    { "offline_access", "Offline Access" }
                }
            }
        }
    });
    
    // API Key Authentication
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "Enter your API key"
    });
    
    // Global security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Custom operation filter for authentication
    options.OperationFilter<AuthenticationOperationFilter>();
    
    // Custom schema filter
    options.SchemaFilter<AuthenticationSchemaFilter>();
});

var app = builder.Build();

// Enable Swagger middleware
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ACommerce API v1");
        options.RoutePrefix = "swagger";
        
        // OAuth2 configuration
        options.OAuthClientId("swagger-ui");
        options.OAuthAppName("Swagger UI");
        options.OAuthUsePkce();
        
        // Custom CSS
        options.InjectStylesheet("/swagger-ui/custom.css");
        
        // Display request duration
        options.DisplayRequestDuration();
        
        // Enable deep linking
        options.EnableDeepLinking();
        
        // Enable filter
        options.EnableFilter();
        
        // Show extensions
        options.ShowExtensions();
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## Operation Filter

### AuthenticationOperationFilter

Automatically adds authentication requirements based on attributes
```csharp
public class AuthenticationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .Union(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();
        
        if (!authAttributes.Any())
        {
            return;
        }
        
        // Add 401 response
        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized - Authentication required"
        });
        
        // Add 403 response for role/policy requirements
        var requiresRole = authAttributes.Any(a => !string.IsNullOrEmpty(a.Roles));
        var requiresPolicy = authAttributes.Any(a => !string.IsNullOrEmpty(a.Policy));
        
        if (requiresRole || requiresPolicy)
        {
            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = "Forbidden - Insufficient permissions"
            });
        }
        
        // Add security requirement
        var scheme = "Bearer"; // Default to JWT
        
        // Check if API key is required
        var requiresApiKey = context.MethodInfo
            .GetCustomAttributes(true)
            .Any(a => a.GetType().Name == "ApiKeyAuthorizationAttribute");
        
        if (requiresApiKey)
        {
            scheme = "ApiKey";
        }
        
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = scheme
                        }
                    },
                    Array.Empty<string>()
                }
            }
        };
        
        // Add description about required roles/policies
        if (requiresRole)
        {
            var roles = authAttributes
                .Where(a => !string.IsNullOrEmpty(a.Roles))
                .Select(a => a.Roles)
                .ToList();
            
            operation.Description += $"\n\n**Required Roles:** {string.Join(", ", roles)}";
        }
        
        if (requiresPolicy)
        {
            var policies = authAttributes
                .Where(a => !string.IsNullOrEmpty(a.Policy))
                .Select(a => a.Policy)
                .ToList();
            
            operation.Description += $"\n\n**Required Policies:** {string.Join(", ", policies)}";
        }
    }
}
```

## XML Comments

### Enable in .csproj
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### Usage in Controllers
```csharp
/// <summary>
/// Authentication endpoints
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Login with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication tokens</returns>
    /// <response code="200">Successfully authenticated</response>
    /// <response code="400">Invalid credentials</response>
    /// <response code="429">Too many login attempts</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 429)]
    public async Task<ActionResult<AuthenticationResult>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Error });
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New authentication tokens</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<ActionResult<AuthenticationResult>> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Logout and revoke tokens
    /// </summary>
    /// <response code="204">Logged out successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.GetBearerToken();
        await _authService.RevokeTokenAsync(token);
        return NoContent();
    }
    
    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User information retrieved</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = _userContext.UserId;
        var user = await _userService.GetByIdAsync(userId);
        return Ok(user);
    }
}
```

## Custom Swagger Examples

### Request Examples
```csharp
public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Username = "user@example.com",
            Password = "Password123!",
            RememberMe = true
        };
    }
}

// In Startup
builder.Services.AddSwaggerExamplesFromAssemblyOf<LoginRequestExample>();

// In controller
[SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
```

### Response Examples
```csharp
public class AuthenticationResultExample : IExamplesProvider<AuthenticationResult>
{
    public AuthenticationResult GetExamples()
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            RefreshToken = "refresh_token_here",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            TokenType = "Bearer"
        };
    }
}

// In controller
[SwaggerResponseExample(200, typeof(AuthenticationResultExample))]
```

## Testing Authentication in Swagger

### JWT Bearer

1. Click **Authorize** button in Swagger UI
2. Enter token: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click **Authorize**
4. Test protected endpoints

### OAuth2 Flow

1. Click **Authorize** button
2. Select OAuth2 scheme
3. Select required scopes
4. Click **Authorize**
5. Login in popup window
6. Redirected back to Swagger with token

### API Key

1. Click **Authorize** button
2. Enter API key in `X-API-Key` field
3. Click **Authorize**
4. Test protected endpoints

## Custom Swagger Theme

### wwwroot/swagger-ui/custom.css
```css
/* Dark theme */
.swagger-ui {
    background-color: #1e1e1e;
}

.swagger-ui .topbar {
    background-color: #2d2d2d;
}

.swagger-ui .info .title {
    color: #61dafb;
}

/* Custom colors for methods */
.swagger-ui .opblock.opblock-post {
    background: rgba(73, 204, 144, .1);
    border-color: #49cc90;
}

.swagger-ui .opblock.opblock-get {
    background: rgba(97, 175, 254, .1);
    border-color: #61affe;
}

.swagger-ui .opblock.opblock-delete {
    background: rgba(249, 62, 62, .1);
    border-color: #f93e3e;
}

.swagger-ui .opblock.opblock-put {
    background: rgba(252, 161, 48, .1);
    border-color: #fca130;
}

/* Authorize button */
.swagger-ui .btn.authorize {
    background-color: #49cc90;
    border-color: #49cc90;
}

.swagger-ui .btn.authorize svg {
    fill: #fff;
}
```

## Advanced Configuration

### Multiple API Versions
```csharp
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ACommerce API v1",
        Version = "v1"
    });
    
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "ACommerce API v2",
        Version = "v2"
    });
    
    options.DocInclusionPredicate((version, apiDescription) =>
    {
        var actionApiVersionModel = apiDescription.ActionDescriptor
            .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);
        
        if (actionApiVersionModel == null)
            return true;
        
        return actionApiVersionModel.DeclaredApiVersions.Any(v =>
            $"v{v.ToString()}" == version);
    });
});

// In Swagger UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ACommerce API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "ACommerce API v2");
});
```

### Custom Security Scheme
```csharp
options.AddSecurityDefinition("CustomAuth", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "custom",
    Description = "Custom authentication scheme",
    In = ParameterLocation.Header,
    Name = "X-Custom-Auth"
});
```

## Installation
```bash
dotnet add package ACommerce.Authentication.AspNetCore.Swagger
dotnet add package Swashbuckle.AspNetCore
dotnet add package Swashbuckle.AspNetCore.Annotations
```

## Dependencies

- ACommerce.Authentication.AspNetCore
- Swashbuckle.AspNetCore (6.x)
- Swashbuckle.AspNetCore.Annotations (6.x)

## License

MIT