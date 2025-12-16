using System.Security.Cryptography;
using System.Text;

namespace ACommerce.Marketing.MetaConversions.Helpers;

public static class HashHelper
{
    public static string? Sha256Hash(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var normalized = input.Trim().ToLowerInvariant();

        var bytes = Encoding.UTF8.GetBytes(normalized);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string? HashPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        
        if (digits.StartsWith("00966"))
            digits = "966" + digits[5..];
        else if (digits.StartsWith("0"))
            digits = "966" + digits[1..];
        else if (!digits.StartsWith("966"))
            digits = "966" + digits;

        return Sha256Hash(digits);
    }

    public static string? HashEmail(string? email)
    {
        return Sha256Hash(email?.ToLowerInvariant().Trim());
    }
}
