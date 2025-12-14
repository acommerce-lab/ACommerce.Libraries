using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.TwoFactor.SMS;

public class SmsAuthenticationProvider : ITwoFactorAuthenticationProvider
{
    private readonly ISmsProvider _smsProvider;
    private readonly ITwoFactorSessionStore _sessionStore;
    private readonly ILogger<SmsAuthenticationProvider> _logger;
    private IAuthenticationEventPublisher? _eventPublisher;

    public string ProviderName => "SMS";

    public SmsAuthenticationProvider(
        ISmsProvider smsProvider,
        ITwoFactorSessionStore sessionStore,
        ILogger<SmsAuthenticationProvider> logger)
    {
        _smsProvider = smsProvider;
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
            var code = GenerateCode();
            var transactionId = Guid.NewGuid().ToString();

            var session = new TwoFactorSession
            {
                TransactionId = transactionId,
                Identifier = request.Identifier,
                Provider = ProviderName,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                VerificationCode = code,
                Status = TwoFactorSessionStatus.Pending,
                Metadata = request.Metadata
            };

            await _sessionStore.CreateSessionAsync(session, cancellationToken);

            await _smsProvider.SendAsync(
                request.Identifier,
                $"Your code is: {code}",
                cancellationToken);

            return TwoFactorInitiationResult.Ok(transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS");
            return TwoFactorInitiationResult.Fail(new()
            {
                Message = ex.Message,
                Code = "SMS_SENDING_FAILED",
                Details = ex.StackTrace
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

            if (session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return TwoFactorVerificationResult.Fail(new()
                {
                    Message = "Code expired",
                    Code = "CODE_EXPIRED",
                    Details = "The verification code has expired"
                });
            }

            if (session.VerificationCode != request.Code)
            {
                return TwoFactorVerificationResult.Fail(new()
                {
                    Message = "Invalid code",
                    Code = "INVALID_CODE",
                    Details = "The provided verification code is incorrect"
                });
            }

            session = session with { Status = TwoFactorSessionStatus.Verified };
            await _sessionStore.UpdateSessionAsync(session, cancellationToken);

            return TwoFactorVerificationResult.Ok(request.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify");
            return TwoFactorVerificationResult.Fail(new()
            {
                Message = ex.Message,
                Code = "VERIFICATION_FAILED",
                Details = ex.StackTrace
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
        catch
        {
            return false;
        }
    }

    private static string GenerateCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }
}
