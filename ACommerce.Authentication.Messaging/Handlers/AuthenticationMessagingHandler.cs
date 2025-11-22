using ACommerce.Authentication.Messaging.Commands;
using ACommerce.Authentication.Messaging.Options;
using ACommerce.Authentication.TwoFactor.Abstractions.Events;
using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Authentication.Messaging.Handlers;

public class AuthenticationMessagingHandler : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<AuthenticationMessagingHandler> _logger;
    private readonly AuthenticationMessagingOptions _options;

    public AuthenticationMessagingHandler(
        IMessageConsumer consumer,
        IMessagePublisher publisher,
        ILogger<AuthenticationMessagingHandler> logger,
        IOptions<AuthenticationMessagingOptions> options)
    {
        _consumer = consumer;
        _publisher = publisher;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[Auth Messaging] 🎧 Starting handler with channels: {Channels}",
            string.Join(", ", _options.NotificationChannels));

        // Subscribe based on configuration
        if (_options.NotifyOnSuccess)
        {
            await _consumer.SubscribeAsync<TwoFactorSucceededEvent>(
                "auth.events.authentication.twofactorsucceeded",
                HandleSucceeded,
                stoppingToken);
        }

        if (_options.NotifyOnFailure)
        {
            await _consumer.SubscribeAsync<TwoFactorFailedEvent>(
                "auth.events.authentication.twofactorfailed",
                HandleFailed,
                stoppingToken);
        }

        if (_options.NotifyOnInitiation)
        {
            await _consumer.SubscribeAsync<TwoFactorInitiatedEvent>(
                "auth.events.authentication.twofactorinitiated",
                HandleInitiated,
                stoppingToken);
        }

        if (_options.NotifyOnExpiration)
        {
            await _consumer.SubscribeAsync<TwoFactorExpiredEvent>(
                "auth.events.authentication.twofactorexpired",
                HandleExpired,
                stoppingToken);
        }

        _logger.LogInformation("[Auth Messaging] ✅ Handler started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<bool> HandleSucceeded(
        TwoFactorSucceededEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            _logger.LogInformation(
                "[Auth Messaging] ✅ 2FA succeeded for {Identifier}",
                evt.Identifier);

            var title = _options.TitleTemplates?.GetValueOrDefault("TwoFactorSucceeded")
                ?? "تم تسجيل الدخول بنجاح";

            var message = _options.MessageTemplates?.GetValueOrDefault("TwoFactorSucceeded")
                ?? $"تم التحقق من هويتك عبر {evt.Provider} بنجاح";

            var command = new SendNotificationCommand
            {
                UserId = evt.Identifier,
                Title = title,
                Message = message,
                Type = NotificationType.SecurityAlert,
                Priority = _options.SuccessPriority,
                Channels = _options.NotificationChannels,
                Data = BuildEventData(evt)
            };

            await _publisher.PublishAsync(
                command,
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Auth Messaging] Failed to handle TwoFactorSucceeded");
            return false;
        }
    }

    private async Task<bool> HandleFailed(
        TwoFactorFailedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            _logger.LogWarning(
                "[Auth Messaging] ⚠️ 2FA failed for {Identifier}: {Reason}",
                evt.Identifier,
                evt.Reason);

            var title = _options.TitleTemplates?.GetValueOrDefault("TwoFactorFailed")
                ?? "فشل التحقق من الهوية";

            var message = _options.MessageTemplates?.GetValueOrDefault("TwoFactorFailed")
                ?? $"فشلت محاولة التحقق من هويتك: {evt.Reason}";

            var command = new SendNotificationCommand
            {
                UserId = evt.Identifier,
                Title = title,
                Message = message,
                Type = NotificationType.SecurityAlert,
                Priority = _options.FailurePriority,
                Channels = _options.NotificationChannels,
                Data = BuildEventData(evt, extraData: new() { ["reason"] = evt.Reason })
            };

            await _publisher.PublishAsync(
                command,
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Auth Messaging] Failed to handle TwoFactorFailed");
            return false;
        }
    }

    private async Task<bool> HandleInitiated(
        TwoFactorInitiatedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            _logger.LogInformation(
                "[Auth Messaging] 🔐 2FA initiated for {Identifier}",
                evt.Identifier);

            var title = _options.TitleTemplates?.GetValueOrDefault("TwoFactorInitiated")
                ?? "طلب تحقق جديد";

            var message = _options.MessageTemplates?.GetValueOrDefault("TwoFactorInitiated")
                ?? $"تم بدء عملية التحقق من الهوية عبر {evt.Provider}";

            var command = new SendNotificationCommand
            {
                UserId = evt.Identifier,
                Title = title,
                Message = message,
                Type = NotificationType.Info,
                Priority = _options.InitiationPriority,
                Channels = _options.NotificationChannels,
                Data = BuildEventData(evt)
            };

            await _publisher.PublishAsync(
                command,
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Auth Messaging] Failed to handle TwoFactorInitiated");
            return false;
        }
    }

    private async Task<bool> HandleExpired(
        TwoFactorExpiredEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            _logger.LogWarning(
                "[Auth Messaging] ⏰ 2FA expired for {Identifier}",
                evt.Identifier);

            var title = _options.TitleTemplates?.GetValueOrDefault("TwoFactorExpired")
                ?? "انتهت صلاحية التحقق";

            var message = _options.MessageTemplates?.GetValueOrDefault("TwoFactorExpired")
                ?? $"انتهت صلاحية طلب التحقق عبر {evt.Provider}";

            var command = new SendNotificationCommand
            {
                UserId = evt.Identifier,
                Title = title,
                Message = message,
                Type = NotificationType.Warning,
                Priority = _options.ExpirationPriority,
                Channels = _options.NotificationChannels,
                Data = BuildEventData(evt)
            };

            await _publisher.PublishAsync(
                command,
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Auth Messaging] Failed to handle TwoFactorExpired");
            return false;
        }
    }

    private Dictionary<string, object> BuildEventData(
        object evt,
        Dictionary<string, object>? extraData = null)
    {
        var data = new Dictionary<string, object>();

        // Extract common properties using reflection
        var eventType = evt.GetType();
        var transactionIdProp = eventType.GetProperty("TransactionId");
        var identifierProp = eventType.GetProperty("Identifier");
        var providerProp = eventType.GetProperty("Provider");
        var occurredAtProp = eventType.GetProperty("OccurredAt");

        if (_options.IncludeTransactionDetails)
        {
            if (transactionIdProp != null)
                data["transactionId"] = transactionIdProp.GetValue(evt) ?? "";

            if (providerProp != null)
                data["provider"] = providerProp.GetValue(evt) ?? "";

            if (occurredAtProp != null)
                data["timestamp"] = occurredAtProp.GetValue(evt) ?? DateTimeOffset.UtcNow;
        }

        // Always include email
        if (identifierProp != null)
            data["email"] = identifierProp.GetValue(evt) ?? "";

        // Add extra data
        if (extraData != null)
        {
            foreach (var (key, value) in extraData)
            {
                data[key] = value;
            }
        }

        return data;
    }
}