namespace ACommerce.Templates.Customer.Themes;

/// <summary>
/// خيارات الثيم القابلة للتخصيص من التطبيق المستهلك
/// </summary>
public class ThemeOptions
{
	/// <summary>
	/// الألوان الأساسية
	/// </summary>
	public ColorPalette Colors { get; set; } = new();

	/// <summary>
	/// الخطوط
	/// </summary>
	public Typography Typography { get; set; } = new();

	/// <summary>
	/// المسافات والأبعاد
	/// </summary>
	public Spacing Spacing { get; set; } = new();

	/// <summary>
	/// الحدود والظلال
	/// </summary>
	public BordersAndShadows Borders { get; set; } = new();

	/// <summary>
	/// اتجاه النص (RTL/LTR)
	/// </summary>
	public TextDirection Direction { get; set; } = TextDirection.RTL;

	/// <summary>
	/// اسم الثيم
	/// </summary>
	public string Name { get; set; } = "Default";

	/// <summary>
	/// الوضع الداكن/الفاتح
	/// </summary>
	public ThemeMode Mode { get; set; } = ThemeMode.Light;
}

public class ColorPalette
{
	// الألوان الرئيسية
	public string Primary { get; set; } = "#6366F1";       // Indigo
	public string PrimaryDark { get; set; } = "#4F46E5";
	public string PrimaryLight { get; set; } = "#818CF8";

	public string Secondary { get; set; } = "#EC4899";     // Pink
	public string SecondaryDark { get; set; } = "#DB2777";
	public string SecondaryLight { get; set; } = "#F472B6";

	public string Accent { get; set; } = "#F59E0B";        // Amber
	public string AccentDark { get; set; } = "#D97706";
	public string AccentLight { get; set; } = "#FBBF24";

	// ألوان الخلفية
	public string Background { get; set; } = "#FFFFFF";
	public string BackgroundAlt { get; set; } = "#F9FAFB";
	public string Surface { get; set; } = "#FFFFFF";
	public string SurfaceAlt { get; set; } = "#F3F4F6";

	// ألوان النصوص
	public string TextPrimary { get; set; } = "#111827";
	public string TextSecondary { get; set; } = "#6B7280";
	public string TextMuted { get; set; } = "#9CA3AF";
	public string TextOnPrimary { get; set; } = "#FFFFFF";

	// ألوان الحالات
	public string Success { get; set; } = "#10B981";
	public string Warning { get; set; } = "#F59E0B";
	public string Error { get; set; } = "#EF4444";
	public string Info { get; set; } = "#3B82F6";

	// ألوان الحدود
	public string Border { get; set; } = "#E5E7EB";
	public string BorderLight { get; set; } = "#F3F4F6";
	public string BorderDark { get; set; } = "#D1D5DB";

	// ألوان خاصة
	public string Sale { get; set; } = "#EF4444";
	public string New { get; set; } = "#10B981";
	public string Featured { get; set; } = "#F59E0B";
}

public class Typography
{
	public string FontFamily { get; set; } = "'Cairo', 'Segoe UI', sans-serif";
	public string FontFamilyHeading { get; set; } = "'Cairo', 'Segoe UI', sans-serif";
	public string FontFamilyMono { get; set; } = "'Fira Code', monospace";

	// أحجام الخطوط
	public string FontSizeXs { get; set; } = "0.75rem";    // 12px
	public string FontSizeSm { get; set; } = "0.875rem";   // 14px
	public string FontSizeBase { get; set; } = "1rem";     // 16px
	public string FontSizeLg { get; set; } = "1.125rem";   // 18px
	public string FontSizeXl { get; set; } = "1.25rem";    // 20px
	public string FontSize2Xl { get; set; } = "1.5rem";    // 24px
	public string FontSize3Xl { get; set; } = "1.875rem";  // 30px
	public string FontSize4Xl { get; set; } = "2.25rem";   // 36px

	// أوزان الخطوط
	public string FontWeightLight { get; set; } = "300";
	public string FontWeightNormal { get; set; } = "400";
	public string FontWeightMedium { get; set; } = "500";
	public string FontWeightSemibold { get; set; } = "600";
	public string FontWeightBold { get; set; } = "700";

	// ارتفاع السطر
	public string LineHeightTight { get; set; } = "1.25";
	public string LineHeightNormal { get; set; } = "1.5";
	public string LineHeightRelaxed { get; set; } = "1.75";
}

public class Spacing
{
	public string Xs { get; set; } = "0.25rem";   // 4px
	public string Sm { get; set; } = "0.5rem";    // 8px
	public string Md { get; set; } = "1rem";      // 16px
	public string Lg { get; set; } = "1.5rem";    // 24px
	public string Xl { get; set; } = "2rem";      // 32px
	public string Xxl { get; set; } = "3rem";     // 48px

	// حجم الحاوية القصوى
	public string ContainerMaxWidth { get; set; } = "1280px";
	public string ContainerPadding { get; set; } = "1rem";
}

public class BordersAndShadows
{
	public string RadiusSm { get; set; } = "0.25rem";    // 4px
	public string RadiusMd { get; set; } = "0.5rem";     // 8px
	public string RadiusLg { get; set; } = "0.75rem";    // 12px
	public string RadiusXl { get; set; } = "1rem";       // 16px
	public string RadiusFull { get; set; } = "9999px";

	public string ShadowSm { get; set; } = "0 1px 2px 0 rgba(0, 0, 0, 0.05)";
	public string ShadowMd { get; set; } = "0 4px 6px -1px rgba(0, 0, 0, 0.1)";
	public string ShadowLg { get; set; } = "0 10px 15px -3px rgba(0, 0, 0, 0.1)";
	public string ShadowXl { get; set; } = "0 20px 25px -5px rgba(0, 0, 0, 0.1)";
}

public enum TextDirection
{
	LTR,
	RTL
}

public enum ThemeMode
{
	Light,
	Dark,
	System
}
