namespace Ashare.App.Services;

/// <summary>
/// خدمة إدارة المظهر (الوضع الفاتح/الداكن)
/// </summary>
public class ThemeService
{
    private bool _isDarkMode;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                OnThemeChanged?.Invoke();
            }
        }
    }

    public event Action? OnThemeChanged;

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }
}
