namespace ACommerce.AppConfig.Services;

/// <summary>
/// مقارن إصدارات بسيط (Major.Minor.Patch) — يستخدمه FeatureFlag evaluation.
/// مكتفٍ بـ Major.Minor[.Patch][.Build] بدون pre-release/metadata.
/// </summary>
internal readonly struct SemanticVersion : IComparable<SemanticVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public int Build { get; }

    public SemanticVersion(int major, int minor, int patch, int build)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
    }

    public static bool TryParse(string? input, out SemanticVersion version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = input.Trim().TrimStart('v', 'V');
        var parts = trimmed.Split('.');

        int major = 0, minor = 0, patch = 0, build = 0;
        if (parts.Length >= 1 && !int.TryParse(parts[0], out major)) return false;
        if (parts.Length >= 2 && !int.TryParse(parts[1], out minor)) return false;
        if (parts.Length >= 3 && !int.TryParse(parts[2], out patch)) return false;
        if (parts.Length >= 4 && !int.TryParse(parts[3], out build)) return false;

        version = new SemanticVersion(major, minor, patch, build);
        return true;
    }

    public int CompareTo(SemanticVersion other)
    {
        var c = Major.CompareTo(other.Major); if (c != 0) return c;
        c = Minor.CompareTo(other.Minor);     if (c != 0) return c;
        c = Patch.CompareTo(other.Patch);     if (c != 0) return c;
        return Build.CompareTo(other.Build);
    }

    public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) < 0;
    public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) > 0;
    public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;
    public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;

    public override string ToString() => $"{Major}.{Minor}.{Patch}.{Build}";
}
