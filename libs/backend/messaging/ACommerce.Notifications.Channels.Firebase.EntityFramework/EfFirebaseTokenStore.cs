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
/// يعمل مع أي DbContext يحتوي على DeviceTokenEntity (يُكتشف تلقائياً عبر IBaseEntity)
/// </summary>
public class EfFirebaseTokenStore : IFirebaseTokenStore
{
    private readonly DbContext _context;
    private readonly ILogger<EfFirebaseTokenStore> _logger;

    /// <summary>
    /// مدة صلاحية التوكن بالأيام (افتراضي: 30 يوم)
    /// </summary>
    private const int TokenValidityDays = 30;

    /// <summary>
    /// الحد الأقصى لعدد التوكنات النشطة لكل مستخدم
    /// </summary>
    private const int MaxTokensPerUser = 5;

    public EfFirebaseTokenStore(
        DbContext context,
        ILogger<EfFirebaseTokenStore> logger)
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

        // 1. تنظيف التوكنات المنتهية للمستخدم
        await CleanupExpiredTokensAsync(deviceToken.UserId, cancellationToken);

        // 2. Check if token already exists
        var existingEntity = await _context.Set<DeviceTokenEntity>()
            .FirstOrDefaultAsync(x => x.Token == deviceToken.Token, cancellationToken);

        var expiresAt = DateTime.UtcNow.AddDays(TokenValidityDays);

        if (existingEntity != null)
        {
            // Update existing token - تجديد الصلاحية
            existingEntity.UserId = deviceToken.UserId;
            existingEntity.Platform = deviceToken.Platform.ToString();
            existingEntity.LastUsedAt = DateTime.UtcNow;
            existingEntity.ExpiresAt = expiresAt;
            existingEntity.IsActive = deviceToken.IsActive;
            existingEntity.UpdatedAt = DateTime.UtcNow;

            if (deviceToken.Metadata != null)
            {
                existingEntity.AppVersion = deviceToken.Metadata.GetValueOrDefault("AppVersion");
                existingEntity.DeviceModel = deviceToken.Metadata.GetValueOrDefault("DeviceModel");
                existingEntity.MetadataJson = JsonSerializer.Serialize(deviceToken.Metadata);
            }

            _logger.LogInformation(
                "Updated existing device token for user {UserId}, expires at {ExpiresAt}",
                deviceToken.UserId,
                expiresAt);
        }
        else
        {
            // 3. التحقق من عدد التوكنات النشطة
            var activeTokenCount = await _context.Set<DeviceTokenEntity>()
                .CountAsync(x => x.UserId == deviceToken.UserId && x.IsActive && !x.IsDeleted, cancellationToken);

            // إذا تجاوز الحد، نحذف أقدم توكن
            if (activeTokenCount >= MaxTokensPerUser)
            {
                var oldestToken = await _context.Set<DeviceTokenEntity>()
                    .Where(x => x.UserId == deviceToken.UserId && x.IsActive && !x.IsDeleted)
                    .OrderBy(x => x.LastUsedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (oldestToken != null)
                {
                    oldestToken.IsActive = false;
                    oldestToken.IsDeleted = true;
                    oldestToken.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Deactivated oldest token for user {UserId} to make room for new one",
                        deviceToken.UserId);
                }
            }

            // Create new token
            var entity = new DeviceTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = deviceToken.UserId,
                Token = deviceToken.Token,
                Platform = deviceToken.Platform.ToString(),
                RegisteredAt = deviceToken.RegisteredAt.UtcDateTime,
                LastUsedAt = deviceToken.LastUsedAt.UtcDateTime,
                ExpiresAt = expiresAt,
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
                "Created new device token for user {UserId}, platform: {Platform}, expires at {ExpiresAt}",
                deviceToken.UserId,
                deviceToken.Platform,
                expiresAt);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// تنظيف التوكنات المنتهية للمستخدم
    /// </summary>
    private async Task CleanupExpiredTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var expiredTokens = await _context.Set<DeviceTokenEntity>()
            .Where(x => x.UserId == userId
                && !x.IsDeleted
                && x.ExpiresAt.HasValue
                && x.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Any())
        {
            foreach (var token in expiredTokens)
            {
                token.IsActive = false;
                token.IsDeleted = true;
                token.UpdatedAt = DateTime.UtcNow;
            }

            _logger.LogInformation(
                "Cleaned up {Count} expired tokens for user {UserId}",
                expiredTokens.Count,
                userId);
        }
    }

    public async Task<List<FirebaseDeviceToken>> GetUserTokensAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting device tokens for user {UserId}", userId);

        var now = DateTime.UtcNow;

        var entities = await _context.Set<DeviceTokenEntity>()
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && x.IsActive
                && !x.IsDeleted
                && (!x.ExpiresAt.HasValue || x.ExpiresAt.Value > now)) // استثناء المنتهية
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} active tokens for user {UserId}", entities.Count, userId);

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
