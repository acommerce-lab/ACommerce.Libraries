using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Messaging.Handlers;

/// <summary>
/// Background service that listens to notification commands and sends notifications
/// </summary>
public class NotificationMessagingHandler(
    IMessageConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationMessagingHandler> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Notify Messaging] 🎧 Starting notification messaging handler");

        await consumer.SubscribeAsync<SendNotificationCommand>(
            TopicNames.Command("notify", "send"),
            HandleSendNotificationCommand,
            stoppingToken);

        logger.LogInformation("[Notify Messaging] ✅ Notification messaging handler started");

        // Keep running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<bool> HandleSendNotificationCommand(
        SendNotificationCommand command,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Notify Messaging] 📨 Received notification command for user {UserId}, CorrelationId: {CorrelationId}",
                command.UserId,
                metadata.CorrelationId);

            using var scope = scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider
                .GetRequiredService<INotificationService>();

            // Extract email from UserId or Data
            string? recipientEmail = null;

            if (command.UserId.Contains("@"))
            {
                recipientEmail = command.UserId;
            }
            else if (command.Data?.TryGetValue("email", out var emailObj) == true)
            {
                recipientEmail = emailObj?.ToString();
            }

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                logger.LogWarning(
                    "[Notify Messaging] ⚠️ No email found for user {UserId}",
                    command.UserId);
                return false;
            }

            // Build notification
            var notification = new Notification
            {
                UserId = command.UserId,
                Title = command.Title,
                Message = command.Message,
                Type = command.Type,
                Priority = command.Priority,
                Channels = command.Channels.Select(c => new ChannelDelivery
                {
                    Channel = c,
                    MaxRetries = 3
                }).ToList(),
                Data = command.Data?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? string.Empty)
                    ?? new Dictionary<string, string>()
            };

            // Ensure email is in Data
            notification.Data["email"] = recipientEmail;

            logger.LogDebug(
                "[Notify Messaging] 📧 Sending notification to email: {Email}",
                recipientEmail);

            // Send notification
            var result = await notificationService.SendAsync(
                notification,
                CancellationToken.None);

            if (result.Success)
            {
                logger.LogInformation(
                    "[Notify Messaging] ✅ Notification {NotificationId} sent successfully. Delivered: {Delivered}/{Total}",
                    result.NotificationId,
                    result.DeliveredChannels?.Count ?? 0,
                    notification.Channels.Count);
                return true;
            }
            else
            {
                logger.LogError(
                    "[Notify Messaging] ❌ Notification {NotificationId} failed. Error: {Error}",
                    result.NotificationId,
                    result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[Notify Messaging] 💥 Failed to process notification command for user {UserId}",
                command.UserId);
            return false;
        }
    }
}