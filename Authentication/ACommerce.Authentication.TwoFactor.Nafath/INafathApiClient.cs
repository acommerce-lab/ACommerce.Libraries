using ACommerce.Authentication.Abstractions;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public interface INafathApiClient
{
    Task<NafathInitiationResponse> InitiateAuthenticationAsync(
        string nationalId,
        CancellationToken cancellationToken = default);

    Task<NafathStatusResponse> CheckStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}

public record NafathInitiationResponse
{
    public bool Success { get; init; }
    public string? TransactionId { get; init; }
    public string? VerificationCode { get; init; } // ✅ أضف هذا
    public TwoFactorError? Error { get; init; }
}

public record NafathStatusResponse
{
    public bool IsCompleted { get; init; }
    public string? Status { get; init; }
}
