namespace HamtramckHardware.Shared.Services;

/// <summary>
/// Theme service for Hamtramck Hardware
/// Colors inspired by hardware store aesthetics - industrial orange and steel gray
/// </summary>
public class ThemeService
{
    public bool IsDarkMode { get; private set; } = false;

    public event Action? OnThemeChanged;

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        OnThemeChanged?.Invoke();
    }

    public void SetDarkMode(bool isDark)
    {
        if (IsDarkMode != isDark)
        {
            IsDarkMode = isDark;
            OnThemeChanged?.Invoke();
        }
    }
}

/// <summary>
/// Hamtramck Hardware color palette
/// </summary>
public static class HamtramckColors
{
    // Primary - Industrial Orange (Hardware/Safety)
    public const string Primary = "#E65100";
    public const string PrimaryDark = "#BF360C";
    public const string PrimaryLight = "#FF6D00";

    // Secondary - Steel Gray
    public const string Secondary = "#455A64";
    public const string SecondaryDark = "#263238";
    public const string SecondaryLight = "#607D8B";

    // Accent - Yellow (Caution/Tools)
    public const string Accent = "#FFC107";

    // Status Colors
    public const string Success = "#4CAF50";
    public const string Error = "#F44336";
    public const string Warning = "#FF9800";
    public const string Info = "#2196F3";

    // Background
    public const string Background = "#FAFAFA";
    public const string Surface = "#FFFFFF";
    public const string SurfaceDark = "#37474F";
}
