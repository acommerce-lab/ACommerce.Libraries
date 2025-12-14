namespace ACommerce.Authentication.Abstractions;

public record TwoFactorInitiationResult
{
    public required bool Success { get; init; }
    public string? TransactionId { get; init; }
    public string? VerificationCode { get; init; }
    public string? Message { get; init; }
    public TimeSpan? ExpiresIn { get; init; }
    public TwoFactorError? Error { get; init; }
    public Dictionary<string, string>? Data { get; init; }

    public static TwoFactorInitiationResult Ok(
        string transactionId,
        Dictionary<string, string>? data = null)
    {
        return new TwoFactorInitiationResult
        {
            Success = true,
            TransactionId = transactionId,
            Data = data
        };
    }

    public static TwoFactorInitiationResult Fail(TwoFactorError error)
    {
        return new TwoFactorInitiationResult
        {
            Success = false,
            Error = error
        };
    }
}