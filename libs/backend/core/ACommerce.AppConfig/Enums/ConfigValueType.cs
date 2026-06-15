namespace ACommerce.AppConfig.Enums;

/// <summary>
/// نوع قيمة AppConfigEntry — يحدد كيف يفسّر العميل النص المخزّن في حقل Value.
/// </summary>
public enum ConfigValueType
{
    String = 0,
    Integer = 1,
    Boolean = 2,
    Decimal = 3,
    Json = 4,
    Color = 5,
    Url = 6
}

/// <summary>
/// وضع الثيم — كل ThemeToken مرتبط بـ Light أو Dark.
/// </summary>
public enum ThemeMode
{
    Light = 0,
    Dark = 1
}
