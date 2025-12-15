using System.Net.Http.Json;
using System.Text.Json;

namespace Ashare.Admin.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AdminAuthStateProvider _authStateProvider;
    private readonly IConfiguration _configuration;

    public AuthService(HttpClient httpClient, AdminAuthStateProvider authStateProvider, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _configuration = configuration;
        
        var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ashare-api-130415035604.me-central2.run.app";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/admin/login", new { email, password });
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AdminApiLoginResponse>();
                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    var user = new AdminUserInfo
                    {
                        Id = result.ProfileId ?? Guid.NewGuid().ToString(),
                        Email = email,
                        FullName = result.FullName ?? "مدير النظام",
                        RoleName = "مدير عام",
                        Permissions = new List<string>
                        {
                            "dashboard.view",
                            "users.view", "users.create", "users.edit", "users.delete",
                            "listings.view", "listings.approve", "listings.reject", "listings.edit", "listings.delete",
                            "orders.view", "orders.edit", "orders.refund",
                            "vendors.view", "vendors.approve", "vendors.suspend",
                            "reports.view", "reports.export",
                            "settings.view", "settings.edit",
                            "admin_users.view", "admin_users.create", "admin_users.edit", "admin_users.delete",
                            "roles.view", "roles.create", "roles.edit", "roles.delete"
                        }
                    };

                    await _authStateProvider.MarkUserAsAuthenticated(user, result.Token);
                    return new LoginResult { Success = true };
                }
            }

            return new LoginResult 
            { 
                Success = false, 
                ErrorMessage = "البريد الإلكتروني أو كلمة المرور غير صحيحة" 
            };
        }
        catch (Exception ex)
        {
            return new LoginResult 
            { 
                Success = false, 
                ErrorMessage = $"حدث خطأ في الاتصال: {ex.Message}" 
            };
        }
    }

    public async Task<LoginResult> LoginLocalAsync(string email, string password)
    {
        if (email == "admin@ashare.sa" && password == "Admin@123")
        {
            var user = new AdminUserInfo
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                FullName = "مدير النظام",
                RoleName = "مدير عام",
                Permissions = new List<string>
                {
                    "dashboard.view",
                    "users.view", "users.create", "users.edit", "users.delete",
                    "listings.view", "listings.approve", "listings.reject", "listings.edit", "listings.delete",
                    "orders.view", "orders.edit", "orders.refund",
                    "vendors.view", "vendors.approve", "vendors.suspend",
                    "reports.view", "reports.export",
                    "settings.view", "settings.edit",
                    "admin_users.view", "admin_users.create", "admin_users.edit", "admin_users.delete",
                    "roles.view", "roles.create", "roles.edit", "roles.delete"
                }
            };

            await _authStateProvider.MarkUserAsAuthenticated(user, "local_token_" + Guid.NewGuid());
            return new LoginResult { Success = true };
        }

        return new LoginResult 
        { 
            Success = false, 
            ErrorMessage = "البريد الإلكتروني أو كلمة المرور غير صحيحة" 
        };
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.MarkUserAsLoggedOut();
    }

    public async Task<AdminUserInfo?> GetCurrentUserAsync()
    {
        return await _authStateProvider.GetCurrentUserAsync();
    }

    public async Task<bool> HasPermissionAsync(string permission)
    {
        var user = await GetCurrentUserAsync();
        return user?.Permissions.Contains(permission) ?? false;
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ApiLoginResponse
{
    public bool Success { get; set; }
    public ApiLoginData? Data { get; set; }
}

public class ApiLoginData
{
    public string Token { get; set; } = string.Empty;
    public ApiUserData User { get; set; } = new();
}

public class ApiUserData
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<string>? Permissions { get; set; }
}

public class AdminApiLoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ProfileId { get; set; }
    public string? FullName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
    public bool IsNewUser { get; set; }
}
