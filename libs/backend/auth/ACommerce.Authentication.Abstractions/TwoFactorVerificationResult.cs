namespace ACommerce.Authentication.Abstractions;

public record TwoFactorVerificationResult
{
    public required bool Success { get; init; }
    public string? UserId { get; init; }
    public IReadOnlyDictionary<string, string>? UserClaims { get; init; }
    public TwoFactorError? Error { get; init; }
    public string? TransactionId { get; init; }
    public Dictionary<string, string>? Data { get; init; }

    public static TwoFactorVerificationResult Ok(
        string transactionId,
        Dictionary<string, string>? data = null)
    {
        return new TwoFactorVerificationResult
        {
            Success = true,
            TransactionId = transactionId,
            Data = data
        };
    }

    public static TwoFactorVerificationResult Fail(TwoFactorError error)
    {
        return new TwoFactorVerificationResult
        {
            Success = false,
            Error = error
        };
    }
}