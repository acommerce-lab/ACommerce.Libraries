using ACommerce.Authentication.TwoFactor.Abstractions.Events;
using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.Messaging.Handlers;

/// <summary>
/// Background service that listens to authentication events and publishes notifications
/// </summary>
public class AuthenticationMessagingHandler(
    IMessageConsumer consumer,
    IMessagePublisher publisher,
    ILogger<AuthenticationMessagingHandler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Auth Messaging] 🎧 Starting authentication messaging handler");

        // ✅ Listen to TwoFactorSucceeded events
        await consumer.SubscribeAsync<TwoFactorSucceededEvent>(
            "auth.events.authentication.twofactorsucceeded",
            HandleTwoFactorSucceeded,
            stoppingToken);

        // ✅ Listen to TwoFactorFailed events
        await consumer.SubscribeAsync<TwoFactorFailedEvent>(
            "auth.events.authentication.twofactorfailed",
            HandleTwoFactorFailed,
            stoppingToken);

        // ✅ Listen to TwoFactorInitiated events
        await consumer.SubscribeAsync<TwoFactorInitiatedEvent>(
            "auth.events.authentication.twofactorinitiated",
            HandleTwoFactorInitiated,
            stoppingToken);

        // ✅ Listen to TwoFactorExpired events
        await consumer.SubscribeAsync<TwoFactorExpiredEvent>(
            "auth.events.authentication.twofactorexpired",
            HandleTwoFactorExpired,
            stoppingToken);

        logger.LogInformation("[Auth Messaging] ✅ Authentication messaging handler started");

        // Keep running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<bool> HandleTwoFactorSucceeded(
        TwoFactorSucceededEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Auth Messaging] ✅ 2FA succeeded for {Identifier}, publishing notification",
                evt.Identifier);

            // Publish notification command
            await publisher.PublishAsync(
                new
                {
                    UserId = evt.Identifier,
                    Title = "تم تسجيل الدخول بنجاح",
                    Message = $"تم التحقق من هويتك عبر {evt.Provider} بنجاح",
                    Type = 5, // SecurityAlert
                    Priority = 2, // High
                    Channels = new[] { 2 }, // Email
                    Data = new Dictionary<string, object>
                    {
                        ["transactionId"] = evt.TransactionId,
                        ["provider"] = evt.Provider,
                        ["timestamp"] = evt.OccurredAt,
                        ["email"] = evt.Identifier
                    }
                },
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[Auth Messaging] 💥 Failed to handle TwoFactorSucceeded event");
            return false;
        }
    }

    private async Task<bool> HandleTwoFactorFailed(
        TwoFactorFailedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogWarning(
                "[Auth Messaging] ⚠️ 2FA failed for {Identifier}, publishing alert",
                evt.Identifier);

            await publisher.PublishAsync(
                new
                {
                    UserId = evt.Identifier,
                    Title = "فشل التحقق من الهوية",
                    Message = $"فشلت محاولة التحقق من هويتك: {evt.Reason}",
                    Type = 5, // SecurityAlert
                    Priority = 3, // Urgent
                    Channels = new[] { 2 }, // Email
                    Data = new Dictionary<string, object>
                    {
                        ["transactionId"] = evt.TransactionId,
                        ["reason"] = evt.Reason,
                        ["timestamp"] = evt.OccurredAt,
                        ["email"] = evt.Identifier
                    }
                },
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[Auth Messaging] 💥 Failed to handle TwoFactorFailed event");
            return false;
        }
    }

    private async Task<bool> HandleTwoFactorInitiated(
        TwoFactorInitiatedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Auth Messaging] 🔐 2FA initiated for {Identifier}",
                evt.Identifier);

            await publisher.PublishAsync(
                new
                {
                    UserId = evt.Identifier,
                    Title = "طلب تحقق جديد",
                    Message = $"تم بدء عملية التحقق من الهوية عبر {evt.Provider}",
                    Type = 1, // Info
                    Priority = 1, // Normal
                    Channels = new[] { 2 }, // Email
                    Data = new Dictionary<string, object>
                    {
                        ["transactionId"] = evt.TransactionId,
                        ["provider"] = evt.Provider,
                        ["timestamp"] = evt.OccurredAt,
                        ["email"] = evt.Identifier
                    }
                },
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[Auth Messaging] 💥 Failed to handle TwoFactorInitiated event");
            return false;
        }
    }

    private async Task<bool> HandleTwoFactorExpired(
        TwoFactorExpiredEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogWarning(
                "[Auth Messaging] ⏰ 2FA expired for {Identifier}",
                evt.Identifier);

            await publisher.PublishAsync(
                new
                {
                    UserId = evt.Identifier,
                    Title = "انتهت صلاحية التحقق",
                    Message = $"انتهت صلاحية طلب التحقق عبر {evt.Provider}",
                    Type = 2, // Warning
                    Priority = 1, // Normal
                    Channels = new[] { 2 }, // Email
                    Data = new Dictionary<string, object>
                    {
                        ["transactionId"] = evt.TransactionId,
                        ["provider"] = evt.Provider,
                        ["timestamp"] = evt.OccurredAt,
                        ["email"] = evt.Identifier
                    }
                },
                topic: TopicNames.Command("notify", "send"),
                metadata: metadata with { SourceService = "auth" },
                CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[Auth Messaging] 💥 Failed to handle TwoFactorExpired event");
            return false;
        }
    }
}