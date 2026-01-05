using ACommerce.Client.Core.Interceptors;

namespace Ashare.Admin.Services;

/// <summary>
/// Token Provider للوحة التحكم - يحفظ Token في الذاكرة للاستخدام مع HTTP Client
/// هذا Singleton يُحدث عند تسجيل الدخول/الخروج
/// </summary>
public class AdminTokenProvider : ITokenProvider
{
    private string? _currentToken;
    private readonly object _lock = new();

    public Task<string?> GetTokenAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_currentToken);
        }
    }

    /// <summary>
    /// تحديث Token - يُستدعى من AdminAuthStateProvider
    /// </summary>
    public void SetToken(string? token)
    {
        lock (_lock)
        {
            // إزالة علامات الاقتباس إذا كانت موجودة
            if (!string.IsNullOrEmpty(token) && token.StartsWith("\"") && token.EndsWith("\""))
            {
                token = token[1..^1];
            }
            _currentToken = token;
        }
    }

    /// <summary>
    /// مسح Token - يُستدعى عند تسجيل الخروج
    /// </summary>
    public void ClearToken()
    {
        lock (_lock)
        {
            _currentToken = null;
        }
    }
}
