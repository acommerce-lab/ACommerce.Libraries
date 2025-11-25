namespace ACommerce.Blazor.Shop.Services;

/// <summary>
/// خدمة إدارة الثيم (فاتح/داكن)
/// </summary>
public class ThemeService
{
	private string _currentTheme = "light";

	public event Action? OnThemeChanged;

	public string CurrentTheme => _currentTheme;

	public bool IsDark => _currentTheme == "dark";

	public void ToggleTheme()
	{
		_currentTheme = _currentTheme == "light" ? "dark" : "light";
		OnThemeChanged?.Invoke();
	}

	public void SetTheme(string theme)
	{
		if (theme != "light" && theme != "dark")
			return;

		_currentTheme = theme;
		OnThemeChanged?.Invoke();
	}
}
