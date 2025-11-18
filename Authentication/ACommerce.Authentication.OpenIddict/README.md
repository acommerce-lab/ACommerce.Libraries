# ACommerce.Authentication.Providers.OpenIddict

OpenIddict authentication provider for OAuth 2.0 and OpenID Connect flows.

## Overview

Production-ready OpenIddict implementation supporting multiple OAuth 2.0 flows, OpenID Connect, token management, and client credentials. Perfect for building identity servers and securing APIs.

## Key Features

✅ **OAuth 2.0 Flows** - Authorization Code, Client Credentials, Password, Refresh Token  
✅ **OpenID Connect** - Identity token support  
✅ **Scope Management** - Custom scopes and permissions  
✅ **Client Management** - Multiple client applications  
✅ **Token Introspection** - Validate external tokens  
✅ **PKCE Support** - Enhanced security for mobile apps  

## Configuration

### appsettings.json
```json
{
  "OpenIddictSettings": {
    "Issuer": "https://auth.ACommerce.sa",
    "AccessTokenLifetime": 3600,
    "IdentityTokenLifetime": 300,
    "RefreshTokenLifetime": 1209600,
    "EnableTokenRevocation": true,
    "EnableTokenIntrospection": true,
    "RequireHttpsMetadata": true,
    "AllowInsecureHttp": false,
    "Scopes": [
      "openid",
      "profile",
      "email",
      "api",
      "offline_access"
    ]
  }
}
```

## Setup

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add OpenIddict
builder.Services.AddOpenIddictAuthentication(builder.Configuration);

// Or manual setup:
builder.Services.AddOpenIddict()
    
    // Register Entity Framework stores
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    
    // Register ASP.NET Core components
    .AddServer(options =>
    {
        // Enable OAuth 2.0 flows
        options.SetTokenEndpointUris("/connect/token")
               .SetAuthorizationEndpointUris("/connect/authorize")
               .SetUserinfoEndpointUris("/connect/userinfo")
               .SetIntrospectionEndpointUris("/connect/introspect")
               .SetRevocationEndpointUris("/connect/revoke");
        
        // Enable flows
        options.AllowAuthorizationCodeFlow()
               .AllowClientCredentialsFlow()
               .AllowPasswordFlow()
               .AllowRefreshTokenFlow();
        
        // Register scopes
        options.RegisterScopes("openid", "profile", "email", "api", "offline_access");
        
        // Configure token lifetimes
        options.SetAccessTokenLifetime(TimeSpan.FromHours(1));
        options.SetIdentityTokenLifetime(TimeSpan.FromMinutes(5));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(14));
        
        // Add encryption and signing credentials
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();
        
        // Register ASP.NET Core host
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough();
    })
    
    // Register validation components
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## Database Models

### OpenIddictApplication (Client)
```csharp
public class OpenIddictApplication
{
    public string Id { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string DisplayName { get; set; }
    public string Type { get; set; } // public, confidential
    public string Permissions { get; set; } // JSON array
    public string RedirectUris { get; set; } // JSON array
    public string PostLogoutRedirectUris { get; set; }
}
```

### OpenIddictAuthorization
```csharp
public class OpenIddictAuthorization
{
    public string Id { get; set; }
    public string ApplicationId { get; set; }
    public string Subject { get; set; } // UserId
    public string Type { get; set; } // permanent, ad-hoc
    public string Scopes { get; set; } // JSON array
    public string Status { get; set; } // valid, revoked
    public DateTime CreationDate { get; set; }
}
```

### OpenIddictToken
```csharp
public class OpenIddictToken
{
    public string Id { get; set; }
    public string ApplicationId { get; set; }
    public string AuthorizationId { get; set; }
    public string Subject { get; set; } // UserId
    public string Type { get; set; } // access_token, refresh_token, id_token
    public string Payload { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreationDate { get; set; }
}
```

## Client Registration
```csharp
public class ClientSeeder
{
    public static async Task SeedClientsAsync(IServiceProvider services)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();
        
        // Web Application (Authorization Code Flow)
        if (await manager.FindByClientIdAsync("webapp") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "webapp",
                ClientSecret = "webapp-secret",
                DisplayName = "Web Application",
                Type = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    "scp:api"
                },
                RedirectUris = { new Uri("https://webapp.ACommerce.sa/callback") },
                PostLogoutRedirectUris = { new Uri("https://webapp.ACommerce.sa/") }
            });
        }
        
        // Mobile App (Authorization Code + PKCE)
        if (await manager.FindByClientIdAsync("mobileapp") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "mobileapp",
                DisplayName = "Mobile Application",
                Type = OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    "scp:api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                RedirectUris = { new Uri("ACommerceapp://callback") }
            });
        }
        
        // Service (Client Credentials)
        if (await manager.FindByClientIdAsync("service") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "service",
                ClientSecret = "service-secret",
                DisplayName = "Background Service",
                Type = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    "scp:api"
                }
            });
        }
    }
}
```

## Authentication Endpoints

### Token Endpoint
```csharp
[HttpPost("~/connect/token")]
public async Task<IActionResult> Exchange()
{
    var request = HttpContext.GetOpenIddictServerRequest();
    
    // Password Flow
    if (request.IsPasswordGrantType())
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = "invalid_grant",
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = 
                        "Invalid credentials"
                }));
        }
        
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);
        
        identity.AddClaim(Claims.Subject, user.Id);
        identity.AddClaim(Claims.Name, user.UserName);
        identity.AddClaim(Claims.Email, user.Email);
        
        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(Claims.Role, role);
        }
        
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(GetDestinations);
        
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    // Client Credentials Flow
    if (request.IsClientCredentialsGrantType())
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
        
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);
        
        identity.AddClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
        identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
        
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(GetDestinations);
        
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    // Refresh Token Flow
    if (request.IsRefreshTokenGrantType())
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var user = await _userManager.FindByIdAsync(result.Principal.GetClaim(Claims.Subject));
        
        if (user == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = "invalid_grant",
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = 
                        "Token no longer valid"
                }));
        }
        
        // Recreate identity
        var identity = new ClaimsIdentity(result.Principal.Claims);
        identity.SetDestinations(GetDestinations);
        
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    throw new InvalidOperationException("The specified grant type is not supported.");
}
```

### Authorization Endpoint
```csharp
[HttpGet("~/connect/authorize")]
[HttpPost("~/connect/authorize")]
public async Task<IActionResult> Authorize()
{
    var request = HttpContext.GetOpenIddictServerRequest();
    
    // Check if user is authenticated
    var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
    if (!result.Succeeded)
    {
        return Challenge(
            authenticationSchemes: IdentityConstants.ApplicationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
            });
    }
    
    // Retrieve application
    var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
    if (application == null)
    {
        return Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = "invalid_client",
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = 
                    "Invalid client application"
            }));
    }
    
    // Create claims identity
    var identity = new ClaimsIdentity(
        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
        nameType: Claims.Name,
        roleType: Claims.Role);
    
    var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
    var user = await _userManager.FindByIdAsync(userId);
    
    identity.AddClaim(Claims.Subject, user.Id);
    identity.AddClaim(Claims.Name, user.UserName);
    identity.AddClaim(Claims.Email, user.Email);
    
    identity.SetScopes(request.GetScopes());
    identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
    identity.SetDestinations(GetDestinations);
    
    return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
}
```

## Client Integration

### Web Application (Authorization Code)
```javascript
// Redirect to authorization endpoint
const authorizeUrl = new URL('https://auth.ACommerce.sa/connect/authorize');
authorizeUrl.searchParams.append('client_id', 'webapp');
authorizeUrl.searchParams.append('redirect_uri', 'https://webapp.ACommerce.sa/callback');
authorizeUrl.searchParams.append('response_type', 'code');
authorizeUrl.searchParams.append('scope', 'openid profile email api offline_access');
authorizeUrl.searchParams.append('state', generateRandomState());

window.location.href = authorizeUrl.toString();

// Handle callback
const urlParams = new URLSearchParams(window.location.search);
const code = urlParams.get('code');

// Exchange code for token
const tokenResponse = await fetch('https://auth.ACommerce.sa/connect/token', {
  method: 'POST',
  headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  body: new URLSearchParams({
    grant_type: 'authorization_code',
    client_id: 'webapp',
    client_secret: 'webapp-secret',
    code: code,
    redirect_uri: 'https://webapp.ACommerce.sa/callback'
  })
});

const tokens = await tokenResponse.json();
// { access_token, refresh_token, id_token, expires_in }
```

### Service (Client Credentials)
```csharp
public class ServiceAuthClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<string> GetAccessTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.ACommerce.sa/connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "service",
            ["client_secret"] = "service-secret",
            ["scope"] = "api"
        });
        
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        return result.AccessToken;
    }
}
```

## Token Introspection
```csharp
[HttpPost("~/connect/introspect")]
public async Task<IActionResult> Introspect()
{
    var request = HttpContext.GetOpenIddictServerRequest();
    
    var token = await _tokenManager.FindByReferenceIdAsync(request.Token);
    if (token == null)
    {
        return Ok(new { active = false });
    }
    
    var application = await _applicationManager.FindByIdAsync(
        await _tokenManager.GetApplicationIdAsync(token));
    
    return Ok(new
    {
        active = true,
        sub = await _tokenManager.GetSubjectAsync(token),
        client_id = await _applicationManager.GetClientIdAsync(application),
        scope = await _tokenManager.GetPayloadAsync(token),
        exp = (await _tokenManager.GetExpirationDateAsync(token))?.ToUnixTimeSeconds()
    });
}
```

## Installation
```bash
dotnet add package ACommerce.Authentication.Providers.OpenIddict
dotnet add package OpenIddict.AspNetCore
dotnet add package OpenIddict.EntityFrameworkCore
```

## Dependencies

- ACommerce.Authentication.Abstractions
- OpenIddict.AspNetCore (5.x)
- OpenIddict.EntityFrameworkCore (5.x)

## License

MIT