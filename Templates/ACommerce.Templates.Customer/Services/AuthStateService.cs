using System.Text.Json;
using ACommerce.Client.Core.Storage;

namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Service for managing authentication state and current user data
/// </summary>
public class AuthStateService
{
    private readonly IStorageService _storage;
    private const string TokenKey = "auth_token";
    private const string UserKey = "current_user";

    public bool IsAuthenticated { get; private set; }
    public bool IsInitialized { get; private set; }
    public UserData? CurrentUser { get; private set; }
    public string? Token { get; private set; }

    public event Action? OnAuthStateChanged;

    public AuthStateService(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            Token = await _storage.GetAsync(TokenKey);
            var userJson = await _storage.GetAsync(UserKey);
            if (!string.IsNullOrEmpty(userJson))
            {
                CurrentUser = JsonSerializer.Deserialize<UserData>(userJson);
            }
            IsAuthenticated = !string.IsNullOrEmpty(Token) && CurrentUser != null;
        }
        catch
        {
            IsAuthenticated = false;
        }
        finally
        {
            IsInitialized = true;
            NotifyStateChanged();
        }
    }

    public async Task LoginAsync(string token, UserData user)
    {
        Token = token;
        CurrentUser = user;
        IsAuthenticated = true;

        await _storage.SetAsync(TokenKey, token);
        await _storage.SetAsync(UserKey, JsonSerializer.Serialize(user));

        NotifyStateChanged();
    }

    public async Task UpdateUserAsync(UserData user)
    {
        CurrentUser = user;
        await _storage.SetAsync(UserKey, JsonSerializer.Serialize(user));
        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        Token = null;
        CurrentUser = null;
        IsAuthenticated = false;

        await _storage.RemoveAsync(TokenKey);
        await _storage.RemoveAsync(UserKey);

        NotifyStateChanged();
    }

    public async Task RefreshTokenAsync(string newToken)
    {
        Token = newToken;
        await _storage.SetAsync(TokenKey, newToken);
    }

    private void NotifyStateChanged() => OnAuthStateChanged?.Invoke();
}

/// <summary>
/// User data model stored in local storage
/// </summary>
public class UserData
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; }
    public bool IsProfileComplete { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
