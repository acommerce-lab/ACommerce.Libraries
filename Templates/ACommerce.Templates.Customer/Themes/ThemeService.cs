namespace ACommerce.Templates.Customer.Themes;

/// <summary>
/// خدمة إدارة الثيم
/// </summary>
public class ThemeService
{
	private ThemeOptions _currentTheme;

	public event Action? OnThemeChanged;

	public ThemeService(ThemeOptions? options = null)
	{
		_currentTheme = options ?? new ThemeOptions();
	}

	public ThemeOptions Current => _currentTheme;

	public void SetTheme(ThemeOptions theme)
	{
		_currentTheme = theme;
		OnThemeChanged?.Invoke();
	}

	public void ToggleDarkMode()
	{
		if (_currentTheme.Mode == ThemeMode.Light)
		{
			ApplyDarkMode();
		}
		else
		{
			ApplyLightMode();
		}
		OnThemeChanged?.Invoke();
	}

	private void ApplyDarkMode()
	{
		_currentTheme.Mode = ThemeMode.Dark;
		_currentTheme.Colors.Background = "#111827";
		_currentTheme.Colors.BackgroundAlt = "#1F2937";
		_currentTheme.Colors.Surface = "#1F2937";
		_currentTheme.Colors.SurfaceAlt = "#374151";
		_currentTheme.Colors.TextPrimary = "#F9FAFB";
		_currentTheme.Colors.TextSecondary = "#D1D5DB";
		_currentTheme.Colors.TextMuted = "#9CA3AF";
		_currentTheme.Colors.Border = "#374151";
		_currentTheme.Colors.BorderLight = "#4B5563";
		_currentTheme.Colors.BorderDark = "#1F2937";
	}

	private void ApplyLightMode()
	{
		_currentTheme.Mode = ThemeMode.Light;
		_currentTheme.Colors.Background = "#FFFFFF";
		_currentTheme.Colors.BackgroundAlt = "#F9FAFB";
		_currentTheme.Colors.Surface = "#FFFFFF";
		_currentTheme.Colors.SurfaceAlt = "#F3F4F6";
		_currentTheme.Colors.TextPrimary = "#111827";
		_currentTheme.Colors.TextSecondary = "#6B7280";
		_currentTheme.Colors.TextMuted = "#9CA3AF";
		_currentTheme.Colors.Border = "#E5E7EB";
		_currentTheme.Colors.BorderLight = "#F3F4F6";
		_currentTheme.Colors.BorderDark = "#D1D5DB";
	}

	/// <summary>
	/// إنشاء CSS Variables من الثيم
	/// </summary>
	public string GenerateCssVariables()
	{
		var c = _currentTheme.Colors;
		var t = _currentTheme.Typography;
		var s = _currentTheme.Spacing;
		var b = _currentTheme.Borders;

		return $@"
:root {{
	/* Colors - Primary */
	--ac-primary: {c.Primary};
	--ac-primary-dark: {c.PrimaryDark};
	--ac-primary-light: {c.PrimaryLight};

	/* Colors - Secondary */
	--ac-secondary: {c.Secondary};
	--ac-secondary-dark: {c.SecondaryDark};
	--ac-secondary-light: {c.SecondaryLight};

	/* Colors - Accent */
	--ac-accent: {c.Accent};
	--ac-accent-dark: {c.AccentDark};
	--ac-accent-light: {c.AccentLight};

	/* Colors - Background */
	--ac-bg: {c.Background};
	--ac-bg-alt: {c.BackgroundAlt};
	--ac-surface: {c.Surface};
	--ac-surface-alt: {c.SurfaceAlt};

	/* Colors - Text */
	--ac-text: {c.TextPrimary};
	--ac-text-secondary: {c.TextSecondary};
	--ac-text-muted: {c.TextMuted};
	--ac-text-on-primary: {c.TextOnPrimary};

	/* Colors - States */
	--ac-success: {c.Success};
	--ac-warning: {c.Warning};
	--ac-error: {c.Error};
	--ac-info: {c.Info};

	/* Colors - Borders */
	--ac-border: {c.Border};
	--ac-border-light: {c.BorderLight};
	--ac-border-dark: {c.BorderDark};

	/* Colors - Special */
	--ac-sale: {c.Sale};
	--ac-new: {c.New};
	--ac-featured: {c.Featured};

	/* Typography */
	--ac-font-family: {t.FontFamily};
	--ac-font-family-heading: {t.FontFamilyHeading};
	--ac-font-family-mono: {t.FontFamilyMono};

	--ac-font-xs: {t.FontSizeXs};
	--ac-font-sm: {t.FontSizeSm};
	--ac-font-base: {t.FontSizeBase};
	--ac-font-lg: {t.FontSizeLg};
	--ac-font-xl: {t.FontSizeXl};
	--ac-font-2xl: {t.FontSize2Xl};
	--ac-font-3xl: {t.FontSize3Xl};
	--ac-font-4xl: {t.FontSize4Xl};

	--ac-font-light: {t.FontWeightLight};
	--ac-font-normal: {t.FontWeightNormal};
	--ac-font-medium: {t.FontWeightMedium};
	--ac-font-semibold: {t.FontWeightSemibold};
	--ac-font-bold: {t.FontWeightBold};

	--ac-leading-tight: {t.LineHeightTight};
	--ac-leading-normal: {t.LineHeightNormal};
	--ac-leading-relaxed: {t.LineHeightRelaxed};

	/* Spacing */
	--ac-space-xs: {s.Xs};
	--ac-space-sm: {s.Sm};
	--ac-space-md: {s.Md};
	--ac-space-lg: {s.Lg};
	--ac-space-xl: {s.Xl};
	--ac-space-2xl: {s.Xxl};

	--ac-container-max: {s.ContainerMaxWidth};
	--ac-container-padding: {s.ContainerPadding};

	/* Borders & Shadows */
	--ac-radius-sm: {b.RadiusSm};
	--ac-radius-md: {b.RadiusMd};
	--ac-radius-lg: {b.RadiusLg};
	--ac-radius-xl: {b.RadiusXl};
	--ac-radius-full: {b.RadiusFull};

	--ac-shadow-sm: {b.ShadowSm};
	--ac-shadow-md: {b.ShadowMd};
	--ac-shadow-lg: {b.ShadowLg};
	--ac-shadow-xl: {b.ShadowXl};

	/* Direction */
	--ac-direction: {(_currentTheme.Direction == TextDirection.RTL ? "rtl" : "ltr")};
}}";
	}
}
