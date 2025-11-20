using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public class NafathAuthenticationProvider : ITwoFactorAuthenticationProvider
{
    private readonly INafathApiClient _apiClient;
    private readonly ITwoFactorSessionStore _sessionStore;
    private readonly ILogger<NafathAuthenticationProvider> _logger;
    private IAuthenticationEventPublisher? _eventPublisher;

    public string ProviderName => "Nafath";

    public NafathAuthenticationProvider(
        INafathApiClient apiClient,
        ITwoFactorSessionStore sessionStore,
        ILogger<NafathAuthenticationProvider> logger)
    {
        _apiClient = apiClient;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public void SetEventPublisher(IAuthenticationEventPublisher? publisher)
    {
        _eventPublisher = publisher;
    }

    public async Task<TwoFactorInitiationResult> InitiateAsync(
        TwoFactorInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var nafathResponse = await _apiClient.InitiateAuthenticationAsync(
                request.Identifier,
                cancellationToken);

            if (!nafathResponse.Success)
            {
                return TwoFactorInitiationResult.Fail(
                    nafathResponse.Error ??
                    new TwoFactorError() {
                        Message = "Unknown error",
                        Code = "UNKNOWN_ERROR",
                        Details = "Nafath API did not provide error details"
                    });
            }

            var session = new TwoFactorSession
            {
                TransactionId = nafathResponse.TransactionId!,
                Identifier = request.Identifier,
                Provider = ProviderName,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                Status = TwoFactorSessionStatus.Pending,
                Metadata = request.Metadata
            };

            await _sessionStore.CreateSessionAsync(session, cancellationToken);

            return TwoFactorInitiationResult.Ok(
                session.TransactionId,
                new Dictionary<string, string>
                {
                    ["expiresAt"] = session.ExpiresAt.ToString("o")
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate Nafath");
            return TwoFactorInitiationResult.Fail(new()
            {
                Code = "INITIATION_FAILED",
                Message = ex.Message,
                Details = ex.StackTrace,
            });
        }
    }

    public async Task<TwoFactorVerificationResult> VerifyAsync(
        TwoFactorVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _sessionStore.GetSessionAsync(
                request.TransactionId,
                cancellationToken);

            if (session == null)
            {
                return TwoFactorVerificationResult.Fail(new()
                {
                    Message = "Transaction not found",
                    Code = "TRANSACTION_NOT_FOUND",
                    Details = $"No session found for TransactionId: {request.TransactionId}"
                });
            }

            if (session.Status == TwoFactorSessionStatus.Verified)
            {
                return TwoFactorVerificationResult.Ok(request.TransactionId);
            }

            if (session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return TwoFactorVerificationResult.Fail(new()
                {
                    Message = "Transaction expired",
                    Code = "TRANSACTION_EXPIRED",
                    Details = $"Session expired at {session.ExpiresAt:o}"
                });
            }

            var statusResponse = await _apiClient.CheckStatusAsync(
                request.TransactionId,
                cancellationToken);

            if (statusResponse.IsCompleted)
            {
                session = session with { Status = TwoFactorSessionStatus.Verified };
                await _sessionStore.UpdateSessionAsync(session, cancellationToken);
                return TwoFactorVerificationResult.Ok(request.TransactionId);
            }

            return TwoFactorVerificationResult.Fail(new()
            {
                Message = "Verification pending",
                Code = "VERIFICATION_PENDING",
                Details = $"Current status: {statusResponse.Status}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify");
            return TwoFactorVerificationResult.Fail(new()
            {
                Message = ex.Message,
                Code = "VERIFICATION_FAILED",
                Details = ex.StackTrace,
            });
        }
    }

    public async Task<bool> CancelAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _sessionStore.GetSessionAsync(
                transactionId,
                cancellationToken);

            if (session == null)
                return false;

            session = session with { Status = TwoFactorSessionStatus.Cancelled };
            await _sessionStore.UpdateSessionAsync(session, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel");
            return false;
        }
    }

    public async Task<bool> HandleWebhookAsync(
        NafathWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _sessionStore.GetSessionAsync(
                request.TransactionId,
                cancellationToken);

            if (session == null)
                return false;

            var newStatus = request.Status == "COMPLETED"
                ? TwoFactorSessionStatus.Verified
                : TwoFactorSessionStatus.Failed;

            session = session with { Status = newStatus };
            await _sessionStore.UpdateSessionAsync(session, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}