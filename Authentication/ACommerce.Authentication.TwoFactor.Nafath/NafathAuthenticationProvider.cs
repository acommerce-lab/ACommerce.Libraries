using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.Abstractions.Events;
using ACommerce.Authentication.TwoFactor.Abstractions;
using ACommerce.Authentication.TwoFactor.Abstractions.Events;
using Microsoft.Extensions.Logging;

public class NafathAuthenticationProvider : ITwoFactorAuthenticationProvider
{
    private readonly INafathApiClient _apiClient;
    private readonly ITwoFactorSessionStore _sessionStore;
    private readonly ILogger<NafathAuthenticationProvider> _logger;
    private IAuthenticationEventPublisher? _eventPublisher;  // ✅ Optional

    public string ProviderName => "Nafath";

    public void SetEventPublisher(IAuthenticationEventPublisher? publisher)
    {
        _eventPublisher = publisher;
    }

    public async Task<TwoFactorAuthenticationResult> InitiateAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ... existing code ...

            var transactionId = await CreateNafathTransaction(identifier);

            // ✅ Publish event
            if (_eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(
                    new TwoFactorInitiatedEvent
                    {
                        TransactionId = transactionId,
                        Identifier = identifier,
                        Provider = ProviderName
                    },
                    cancellationToken);
            }

            return new TwoFactorAuthenticationResult
            {
                Success = true,
                TransactionId = transactionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate Nafath authentication");

            // ✅ Publish failure event
            if (_eventPublisher != null)
            {
                await _eventPublisher.PublishAsync(
                    new TwoFactorFailedEvent
                    {
                        TransactionId = "",
                        Identifier = identifier,
                        Provider = ProviderName,
                        Reason = ex.Message
                    },
                    cancellationToken);
            }

            return new TwoFactorAuthenticationResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<bool> HandleWebhookAsync(
        NafathWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        // ... existing validation code ...

        var session = await _sessionStore.GetSessionAsync(request.TransactionId);

        if (session == null)
        {
            return false;
        }

        // Update session
        session = session with
        {
            Status = request.Status == "COMPLETED"
                ? TwoFactorSessionStatus.Verified
                : TwoFactorSessionStatus.Failed
        };

        await _sessionStore.UpdateSessionAsync(session);

        // ✅ Publish success/failure event
        if (_eventPublisher != null)
        {
            if (request.Status == "COMPLETED")
            {
                await _eventPublisher.PublishAsync(
                    new TwoFactorSucceededEvent
                    {
                        TransactionId = request.TransactionId,
                        Identifier = request.NationalId,
                        Provider = ProviderName
                    },
                    cancellationToken);
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
            }
        }

        return true;
    }
}