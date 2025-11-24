using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.Profiles.Core.Events;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Messaging.Handlers;

/// <summary>
/// Handler that updates notification recipient mappings when profiles change
/// </summary>
public class NotificationProfileSyncHandler(
    IMessageConsumer consumer,
    ILogger<NotificationProfileSyncHandler> logger)
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[Notification Profile Sync] 🎧 Starting handler");

        // Listen to ProfileCreated events
        await consumer.SubscribeAsync<ProfileCreatedEvent>(
            "profiles.events.profile.created",
            HandleProfileCreated,
            cancellationToken);

        // Listen to ContactPointAdded events
        await consumer.SubscribeAsync<ContactPointAddedEvent>(
            "profiles.events.contactpoint.added",
            HandleContactPointAdded,
            cancellationToken);

        // Listen to ContactPointVerified events
        await consumer.SubscribeAsync<ContactPointVerifiedEvent>(
            "profiles.events.contactpoint.verified",
            HandleContactPointVerified,
            cancellationToken);

        logger.LogInformation("[Notification Profile Sync] ✅ Handler started");
    }

    private async Task<bool> HandleProfileCreated(
        ProfileCreatedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Notification Profile Sync] 👤 Profile created for {UserId}",
                evt.UserId);

            // TODO: Store contact points in recipient database
            // For now, just log
            foreach (var cp in evt.ContactPoints)
            {
                logger.LogInformation(
                    "[Notification Profile Sync] 📧 Registered {Type}: {Value} for {UserId}",
                    cp.Type,
                    MaskValue(cp.Value),
                    evt.UserId);

                // TODO: Call IRecipientService.AddContactPoint()
                /*
                await _recipientService.AddContactPointAsync(new()
                {
                    UserId = evt.UserId,
                    Type = cp.Type,
                    Value = cp.Value,
                    IsVerified = cp.IsVerified,
                    IsPrimary = cp.IsPrimary
                });
                */
            }

            logger.LogInformation(
                "[Notification Profile Sync] ✅ Synced {Count} contact points for {UserId}",
                evt.ContactPoints.Count,
                evt.UserId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Notification Profile Sync] Failed to handle ProfileCreated");
            return false;
        }
    }

    private async Task<bool> HandleContactPointAdded(
        ContactPointAddedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Notification Profile Sync] ➕ Contact point added: {Type} for {UserId}",
                evt.Type,
                evt.UserId);

            // TODO: Add to recipient database
            /*
            await _recipientService.AddContactPointAsync(new()
            {
                UserId = evt.UserId,
                Type = evt.Type,
                Value = evt.Value,
                IsVerified = evt.IsVerified
            });
            */

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Notification Profile Sync] Failed to handle ContactPointAdded");
            return false;
        }
    }

    private async Task<bool> HandleContactPointVerified(
        ContactPointVerifiedEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Notification Profile Sync] ✅ Contact point verified: {Type} for {UserId}",
                evt.Type,
                evt.UserId);

            // TODO: Update verification status in recipient database
            /*
            await _recipientService.VerifyContactPointAsync(
                evt.UserId,
                evt.Type,
                evt.Value);
            */

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Notification Profile Sync] Failed to handle ContactPointVerified");
            return false;
        }
    }

    private string MaskValue(string value)
    {
        if (value.Contains('@'))
        {
            var parts = value.Split('@');
            return $"{parts[0].Substring(0, Math.Min(3, parts[0].Length))}***@{parts[1]}";
        }

        if (value.Length > 4)
        {
            return $"***{value.Substring(value.Length - 4)}";
        }

        return "***";
    }
}