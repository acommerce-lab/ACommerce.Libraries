using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ACommerce.Marketing.SnapchatConversions.Helpers;

public static class HashHelper
{
    public static string? Sha256Hash(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string? HashEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return Sha256Hash(email.Trim().ToLowerInvariant());
    }

    public static string? HashPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Normalize to E.164 format
        var digits = Regex.Replace(phone, @"[^\d]", "");

        // Add Saudi Arabia country code if not present
        if (digits.StartsWith("05") || digits.StartsWith("5"))
        {
            digits = "966" + digits.TrimStart('0');
        }
        else if (!digits.StartsWith("966") && !digits.StartsWith("+966"))
        {
            digits = "966" + digits;
        }

        return Sha256Hash("+" + digits.TrimStart('+'));
    }
}
