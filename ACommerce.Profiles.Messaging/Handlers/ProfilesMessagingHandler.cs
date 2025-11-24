using ACommerce.Authentication.TwoFactor.Abstractions.Events;
using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.Profiles.Core.DTOs;
using ACommerce.Profiles.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACommerce.Profiles.Messaging.Handlers;

/// <summary>
/// Handler that listens to authentication events and creates profiles
/// </summary>
public class ProfilesMessagingHandler(
    IMessageConsumer consumer,
    IServiceProvider serviceProvider,
    ILogger<ProfilesMessagingHandler> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Profiles Messaging] 🎧 Starting handler");

        // Listen to TwoFactorSucceeded (user registered/authenticated)
        await consumer.SubscribeAsync<TwoFactorSucceededEvent>(
            "auth.events.authentication.twofactorsucceeded",
            HandleTwoFactorSucceeded,
            stoppingToken);

        logger.LogInformation("[Profiles Messaging] ✅ Handler started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<bool> HandleTwoFactorSucceeded(
        TwoFactorSucceededEvent evt,
        MessageMetadata metadata)
    {
        try
        {
            logger.LogInformation(
                "[Profiles Messaging] 🔐 TwoFactor succeeded for {Identifier}",
                evt.Identifier);

            // Create scope for IProfileService
            using var scope = serviceProvider.CreateScope();
            var profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();

            // Check if profile already exists
            var existingProfile = await profileService.GetProfileByUserIdAsync(evt.Identifier);

            if (existingProfile != null)
            {
                logger.LogInformation(
                    "[Profiles Messaging] Profile already exists for {UserId}",
                    evt.Identifier);
                return true;
            }

            // Create profile
            var createDto = new CreateProfileDto
            {
                UserId = evt.Identifier,
                DisplayName = ExtractDisplayName(evt.Identifier),
                ContactPoints = new List<CreateContactPointDto>
                {
                    // If identifier is email, add it as contact point
                    IsEmail(evt.Identifier)
                        ? new CreateContactPointDto
                        {
                            Type = ContactPointType.Email,
                            Value = evt.Identifier,
                            IsPrimary = true,
                            IsVerified = true // Already verified via Nafath
                        }
                        : new CreateContactPointDto
                        {
                            Type = ContactPointType.PhoneNumber,
                            Value = evt.Identifier,
                            IsPrimary = true,
                            IsVerified = true
                        }
                }
            };

            var result = await profileService.CreateProfileAsync(createDto);

            if (!result.Success)
            {
                logger.LogError(
                    "[Profiles Messaging] Failed to create profile: {Error}",
                    result.Error?.Message);
                return false;
            }

            logger.LogInformation(
                "[Profiles Messaging] ✅ Created profile {ProfileId} for {UserId}",
                result.Profile?.Id,
                evt.Identifier);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Profiles Messaging] Failed to handle TwoFactorSucceeded");
            return false;
        }
    }

    private string ExtractDisplayName(string identifier)
    {
        // If email, use part before @
        if (IsEmail(identifier))
        {
            var atIndex = identifier.IndexOf('@');
            return atIndex > 0 ? identifier.Substring(0, atIndex) : identifier;
        }

        // If phone, use last 4 digits
        if (identifier.Length >= 4)
        {
            return $"User {identifier.Substring(identifier.Length - 4)}";
        }

        return "User";
    }

    private bool IsEmail(string identifier)
    {
        return identifier.Contains('@') && identifier.Contains('.');
    }
}