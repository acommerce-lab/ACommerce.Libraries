using System.Text.Json;
using ACommerce.Notifications.Channels.Firebase.EntityFramework.Entities;
using ACommerce.Notifications.Channels.Firebase.Models;
using ACommerce.Notifications.Channels.Firebase.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.Firebase.EntityFramework;

/// <summary>
/// Entity Framework implementation of IFirebaseTokenStore
/// يخزن Device Tokens في قاعدة البيانات
/// </summary>
public class EfFirebaseTokenStore<TContext> : IFirebaseTokenStore
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<EfFirebaseTokenStore<TContext>> _logger;

    public EfFirebaseTokenStore(
        TContext context,
        ILogger<EfFirebaseTokenStore<TContext>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveTokenAsync(
        FirebaseDeviceToken deviceToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Saving device token for user {UserId}, platform: {Platform}",
            deviceToken.UserId,
            deviceToken.Platform);

        // Check if token already exists
        var existingEntity = await _context.Set<DeviceTokenEntity>()
            .FirstOrDefaultAsync(x => x.Token == deviceToken.Token, cancellationToken);

        if (existingEntity != null)
        {
            // Update existing token
            existingEntity.UserId = deviceToken.UserId;
            existingEntity.Platform = deviceToken.Platform.ToString();
            existingEntity.LastUsedAt = DateTime.UtcNow;
            existingEntity.IsActive = deviceToken.IsActive;
            existingEntity.UpdatedAt = DateTime.UtcNow;

            if (deviceToken.Metadata != null)
            {
                existingEntity.AppVersion = deviceToken.Metadata.GetValueOrDefault("AppVersion");
                existingEntity.DeviceModel = deviceToken.Metadata.GetValueOrDefault("DeviceModel");
                existingEntity.MetadataJson = JsonSerializer.Serialize(deviceToken.Metadata);
            }

            _logger.LogInformation(
                "Updated existing device token for user {UserId}",
                deviceToken.UserId);
        }
        else
        {
            // Create new token
            var entity = new DeviceTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = deviceToken.UserId,
                Token = deviceToken.Token,
                Platform = deviceToken.Platform.ToString(),
                RegisteredAt = deviceToken.RegisteredAt.UtcDateTime,
                LastUsedAt = deviceToken.LastUsedAt.UtcDateTime,
                IsActive = deviceToken.IsActive,
                CreatedAt = DateTime.UtcNow,
                AppVersion = deviceToken.Metadata?.GetValueOrDefault("AppVersion"),
                DeviceModel = deviceToken.Metadata?.GetValueOrDefault("DeviceModel"),
                MetadataJson = deviceToken.Metadata != null
                    ? JsonSerializer.Serialize(deviceToken.Metadata)
                    : null
            };

            _context.Set<DeviceTokenEntity>().Add(entity);

            _logger.LogInformation(
                "Created new device token for user {UserId}, platform: {Platform}",
                deviceToken.UserId,
                deviceToken.Platform);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<FirebaseDeviceToken>> GetUserTokensAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting device tokens for user {UserId}", userId);

        var entities = await _context.Set<DeviceTokenEntity>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task DeleteTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting device token");

        var entity = await _context.Set<DeviceTokenEntity>()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (entity != null)
        {
            // Soft delete
            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Device token deleted for user {UserId}", entity.UserId);
        }
    }

    public async Task DeactivateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deactivating device token");

        var entity = await _context.Set<DeviceTokenEntity>()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (entity != null)
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Device token deactivated for user {UserId}",
                entity.UserId);
        }
    }

    public async Task<int> GetActiveDeviceCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<DeviceTokenEntity>()
            .CountAsync(
                x => x.UserId == userId && x.IsActive && !x.IsDeleted,
                cancellationToken);
    }

    private static FirebaseDeviceToken MapToModel(DeviceTokenEntity entity)
    {
        var metadata = !string.IsNullOrEmpty(entity.MetadataJson)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson)
            : new Dictionary<string, string>();

        // Ensure AppVersion and DeviceModel are in metadata
        if (metadata != null)
        {
            if (!string.IsNullOrEmpty(entity.AppVersion))
                metadata["AppVersion"] = entity.AppVersion;
            if (!string.IsNullOrEmpty(entity.DeviceModel))
                metadata["DeviceModel"] = entity.DeviceModel;
        }

        return new FirebaseDeviceToken
        {
            UserId = entity.UserId,
            Token = entity.Token,
            Platform = Enum.TryParse<DevicePlatform>(entity.Platform, out var platform)
                ? platform
                : DevicePlatform.Android,
            RegisteredAt = new DateTimeOffset(entity.RegisteredAt, TimeSpan.Zero),
            LastUsedAt = new DateTimeOffset(entity.LastUsedAt, TimeSpan.Zero),
            IsActive = entity.IsActive,
            Metadata = metadata
        };
    }
}
