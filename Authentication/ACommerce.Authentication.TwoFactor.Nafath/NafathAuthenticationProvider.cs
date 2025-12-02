using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.TwoFactor.Abstractions;
using ACommerce.Authentication.TwoFactor.Abstractions.Events;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public class NafathAuthenticationProvider(
    INafathApiClient apiClient,
    ITwoFactorSessionStore sessionStore,
    ILogger<NafathAuthenticationProvider> logger)
    : ITwoFactorAuthenticationProvider
{
    private IAuthenticationEventPublisher? _eventPublisher;

    public string ProviderName => "Nafath";

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
            var nafathResponse = await apiClient.InitiateAuthenticationAsync(
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
                Metadata = request.Metadata,
                VerificationCode = nafathResponse.VerificationCode,
            };

            await sessionStore.CreateSessionAsync(session, cancellationToken);

            // ✅ في وضع الاختبار: محاكاة استجابة نفاذ بعد 10 ثوانٍ
            if (nafathResponse.IsTestSession)
            {
                logger.LogInformation(
                    "[Nafath] Test session - scheduling auto-webhook in 10 seconds for {TransactionId}",
                    session.TransactionId);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    logger.LogInformation(
                        "[Nafath] Auto-triggering webhook for test session {TransactionId}",
                        session.TransactionId);

                    await HandleWebhookAsync(new NafathWebhookRequest
                    {
                        TransactionId = session.TransactionId,
                        NationalId = nafathResponse.Identifier ?? request.Identifier,
                        Status = "COMPLETED"
                    });
                });
            }

            return TwoFactorInitiationResult.Ok(
                session.TransactionId,
                new Dictionary<string, string>
                {
                    ["verificationCode"] = nafathResponse.VerificationCode ?? "",
                    ["expiresAt"] = session.ExpiresAt.ToString("o")
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initiate Nafath");
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
            var session = await sessionStore.GetSessionAsync(
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

            var statusResponse = await apiClient.CheckStatusAsync(
                request.TransactionId,
                cancellationToken);

            if (statusResponse.IsCompleted)
            {
                session = session with { Status = TwoFactorSessionStatus.Verified };
                await sessionStore.UpdateSessionAsync(session, cancellationToken);
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
            logger.LogError(ex, "Failed to verify");
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
            var session = await sessionStore.GetSessionAsync(
                transactionId,
                cancellationToken);

            if (session == null)
                return false;

            session = session with { Status = TwoFactorSessionStatus.Cancelled };
            await sessionStore.UpdateSessionAsync(session, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel");
            return false;
        }
    }

    public async Task<bool> HandleWebhookAsync(
    NafathWebhookRequest request,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await sessionStore.GetSessionAsync(
                request.TransactionId,
                cancellationToken);

            if (session == null)
            {
                logger.LogWarning(
                    "Received webhook for unknown transaction: {TransactionId}",
                    request.TransactionId);
                return false;
            }

            // Update session based on webhook
            var newStatus = request.Status == "COMPLETED"
                ? TwoFactorSessionStatus.Verified
                : TwoFactorSessionStatus.Failed;

            session = session with { Status = newStatus };
            await sessionStore.UpdateSessionAsync(session, cancellationToken);

            logger.LogInformation(
                "Nafath webhook processed - TransactionId: {TransactionId}, Status: {Status}",
                request.TransactionId,
                request.Status);

            // ✅ Publish events if publisher is available
            if (_eventPublisher != null)
            {
                if (newStatus == TwoFactorSessionStatus.Verified)
                {
                    await _eventPublisher.PublishAsync(
                        new TwoFactorSucceededEvent
                        {
                            TransactionId = request.TransactionId,
                            Identifier = request.NationalId,
                            Provider = ProviderName
                        },
                        cancellationToken);

                    logger.LogInformation(
                        "Published TwoFactorSucceededEvent for {TransactionId}",
                        request.TransactionId);
                }
                else
                {
                    await _eventPublisher.PublishAsync(
                        new TwoFactorFailedEvent
                        {
                            TransactionId = request.TransactionId,
                            Identifier = request.NationalId,
                            Provider = ProviderName,
                            Reason = request.Status
                        },
                        cancellationToken);

                    logger.LogInformation(
                        "Published TwoFactorFailedEvent for {TransactionId}",
                        request.TransactionId);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process Nafath webhook");
            return false;
        }
    }
}