using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Ashare.Admin.Services;

public class AdminAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public AdminAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("admin_token");
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(_anonymous);
            }

            var userJson = await _localStorage.GetItemAsStringAsync("admin_user");
            if (string.IsNullOrEmpty(userJson))
            {
                return new AuthenticationState(_anonymous);
            }

            var user = JsonSerializer.Deserialize<AdminUserInfo>(userJson);
            if (user == null)
            {
                return new AuthenticationState(_anonymous);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new("role_name", user.RoleName)
            };

            foreach (var permission in user.Permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(AdminUserInfo user, string token)
    {
        await _localStorage.SetItemAsStringAsync("admin_token", token);
        await _localStorage.SetItemAsStringAsync("admin_user", JsonSerializer.Serialize(user));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("role_name", user.RoleName)
        };

        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("admin_token");
        await _localStorage.RemoveItemAsync("admin_user");

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsStringAsync("admin_token");
    }

    public async Task<AdminUserInfo?> GetCurrentUserAsync()
    {
        var userJson = await _localStorage.GetItemAsStringAsync("admin_user");
        if (string.IsNullOrEmpty(userJson)) return null;
        return JsonSerializer.Deserialize<AdminUserInfo>(userJson);
    }
}

public class AdminUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
