using ACommerce.Versions.Contracts;
using ACommerce.Versions.DTOs;
using ACommerce.Versions.Entities;
using ACommerce.Versions.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACommerce.Versions.Services;

public class AppVersionService(DbContext dbContext, ILogger<AppVersionService> logger) : IAppVersionService
{
    private DbSet<AppVersion> AppVersions => dbContext.Set<AppVersion>();

    public async Task<IEnumerable<AppVersionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await AppVersions
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.ReleaseDate)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all app versions");
            throw;
        }
    }

    public async Task<IEnumerable<AppVersionDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await AppVersions
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderByDescending(x => x.ReleaseDate)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active app versions");
            throw;
        }
    }

    public async Task<AppVersionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<AppVersionDto>> GetByApplicationCodeAsync(string applicationCode, CancellationToken cancellationToken = default)
    {
        return await AppVersions
            .AsNoTracking()
            .Where(x => x.ApplicationCode == applicationCode && !x.IsDeleted)
            .OrderByDescending(x => x.ReleaseDate)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppVersionDto?> GetByVersionAsync(string applicationCode, string versionNumber, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ApplicationCode == applicationCode &&
                x.VersionNumber == versionNumber &&
                !x.IsDeleted, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<AppVersionDto?> GetLatestVersionAsync(string applicationCode, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions
            .AsNoTracking()
            .Where(x => x.ApplicationCode == applicationCode && x.Status == VersionStatus.Latest && x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.ReleaseDate)
            .FirstOrDefaultAsync(cancellationToken);

        // إذا لم يوجد إصدار بحالة Latest، نبحث عن أحدث إصدار مدعوم
        entity ??= await AppVersions
            .AsNoTracking()
            .Where(x => x.ApplicationCode == applicationCode &&
                       (x.Status == VersionStatus.Supported || x.Status == VersionStatus.Latest) &&
                       x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.ReleaseDate)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<AppVersionDto>> GetByStatusAsync(VersionStatus status, CancellationToken cancellationToken = default)
    {
        return await AppVersions
            .AsNoTracking()
            .Where(x => x.Status == status && !x.IsDeleted)
            .OrderByDescending(x => x.ReleaseDate)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<VersionCheckResult> CheckVersionAsync(VersionCheckRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking version for {ApplicationCode} v{Version}",
            request.ApplicationCode, request.CurrentVersion);

        var result = new VersionCheckResult();

        // الحصول على الإصدار الحالي
        var currentVersion = await AppVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ApplicationCode == request.ApplicationCode &&
                x.VersionNumber == request.CurrentVersion &&
                !x.IsDeleted, cancellationToken);

        // الحصول على أحدث إصدار
        var latestVersion = await AppVersions
            .AsNoTracking()
            .Where(x => x.ApplicationCode == request.ApplicationCode &&
                       x.Status == VersionStatus.Latest &&
                       x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.ReleaseDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentVersion is null)
        {
            // الإصدار غير موجود في قاعدة البيانات
            result.IsSupported = false;
            result.CurrentStatus = VersionStatus.Unsupported;
            result.MessageAr = "إصدار غير معروف. يرجى التحديث إلى أحدث إصدار.";
            result.MessageEn = "Unknown version. Please update to the latest version.";
            result.IsForceUpdate = true;
        }
        else
        {
            result.CurrentVersionInfo = MapToDto(currentVersion);
            result.CurrentStatus = currentVersion.Status;
            result.EndOfSupportDate = currentVersion.EndOfSupportDate;

            switch (currentVersion.Status)
            {
                case VersionStatus.Latest:
                    result.IsSupported = true;
                    result.UpdateAvailable = false;
                    result.MessageAr = "أنت تستخدم أحدث إصدار.";
                    result.MessageEn = "You are using the latest version.";
                    break;

                case VersionStatus.Supported:
                    result.IsSupported = true;
                    result.UpdateAvailable = latestVersion is not null;
                    result.MessageAr = "إصدارك مدعوم. يتوفر إصدار أحدث.";
                    result.MessageEn = "Your version is supported. A newer version is available.";
                    break;

                case VersionStatus.Deprecated:
                    result.IsSupported = true;
                    result.UpdateAvailable = true;
                    result.IsForceUpdate = currentVersion.IsForceUpdate;
                    var daysRemaining = currentVersion.EndOfSupportDate.HasValue
                        ? (currentVersion.EndOfSupportDate.Value - DateTime.UtcNow).Days
                        : 0;
                    result.MessageAr = $"إصدارك على وشك الإيقاف. يرجى التحديث خلال {daysRemaining} يوم.";
                    result.MessageEn = $"Your version is deprecated. Please update within {daysRemaining} days.";
                    break;

                case VersionStatus.Unsupported:
                    result.IsSupported = false;
                    result.UpdateAvailable = true;
                    result.IsForceUpdate = true;
                    result.MessageAr = "إصدارك غير مدعوم. يجب التحديث للاستمرار.";
                    result.MessageEn = "Your version is unsupported. Update required to continue.";
                    break;

                case VersionStatus.Development:
                    result.IsSupported = currentVersion.IsActive;
                    result.MessageAr = "إصدار تجريبي.";
                    result.MessageEn = "Development version.";
                    break;
            }
        }

        if (latestVersion is not null)
        {
            result.LatestVersion = MapToDto(latestVersion);
        }

        return result;
    }

    public async Task<AppVersionDto> CreateAsync(CreateAppVersionRequest request, CancellationToken cancellationToken = default)
    {
        // إذا كان الإصدار الجديد هو Latest، نغير الإصدار السابق إلى Supported
        if (request.Status == VersionStatus.Latest)
        {
            var previousLatest = await AppVersions
                .Where(x => x.ApplicationCode == request.ApplicationCode &&
                           x.Status == VersionStatus.Latest &&
                           !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var prev in previousLatest)
            {
                prev.Status = VersionStatus.Supported;
                prev.UpdatedAt = DateTime.UtcNow;
            }
        }

        var entity = new AppVersion
        {
            Id = Guid.NewGuid(),
            ApplicationCode = request.ApplicationCode,
            ApplicationNameAr = request.ApplicationNameAr,
            ApplicationNameEn = request.ApplicationNameEn,
            VersionNumber = request.VersionNumber,
            BuildNumber = request.BuildNumber,
            Status = request.Status,
            ReleaseDate = request.ReleaseDate,
            DeprecationStartDate = request.DeprecationStartDate,
            EndOfSupportDate = request.EndOfSupportDate,
            ReleaseNotesAr = request.ReleaseNotesAr,
            ReleaseNotesEn = request.ReleaseNotesEn,
            UpdateUrl = request.UpdateUrl,
            DownloadUrl = request.DownloadUrl,
            IsForceUpdate = request.IsForceUpdate,
            MinimumSupportedVersion = request.MinimumSupportedVersion,
            IsActive = request.IsActive,
            Metadata = request.Metadata,
            CreatedAt = DateTime.UtcNow
        };

        await AppVersions.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created new version {VersionNumber} for {ApplicationCode}",
            entity.VersionNumber, entity.ApplicationCode);

        return MapToDto(entity);
    }

    public async Task<AppVersionDto?> UpdateAsync(Guid id, UpdateAppVersionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null) return null;

        if (request.ApplicationNameAr is not null) entity.ApplicationNameAr = request.ApplicationNameAr;
        if (request.ApplicationNameEn is not null) entity.ApplicationNameEn = request.ApplicationNameEn;
        if (request.Status.HasValue) entity.Status = request.Status.Value;
        if (request.DeprecationStartDate.HasValue) entity.DeprecationStartDate = request.DeprecationStartDate;
        if (request.EndOfSupportDate.HasValue) entity.EndOfSupportDate = request.EndOfSupportDate;
        if (request.ReleaseNotesAr is not null) entity.ReleaseNotesAr = request.ReleaseNotesAr;
        if (request.ReleaseNotesEn is not null) entity.ReleaseNotesEn = request.ReleaseNotesEn;
        if (request.UpdateUrl is not null) entity.UpdateUrl = request.UpdateUrl;
        if (request.DownloadUrl is not null) entity.DownloadUrl = request.DownloadUrl;
        if (request.IsForceUpdate.HasValue) entity.IsForceUpdate = request.IsForceUpdate.Value;
        if (request.MinimumSupportedVersion is not null) entity.MinimumSupportedVersion = request.MinimumSupportedVersion;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        if (request.Metadata is not null) entity.Metadata = request.Metadata;

        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<AppVersionDto?> ChangeStatusAsync(Guid id, VersionStatus newStatus, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null) return null;

        var oldStatus = entity.Status;
        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        // إذا تم تغيير الحالة إلى Latest، نغير الإصدار السابق
        if (newStatus == VersionStatus.Latest && oldStatus != VersionStatus.Latest)
        {
            var previousLatest = await AppVersions
                .Where(x => x.ApplicationCode == entity.ApplicationCode &&
                           x.Status == VersionStatus.Latest &&
                           x.Id != id &&
                           !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var prev in previousLatest)
            {
                prev.Status = VersionStatus.Supported;
                prev.UpdatedAt = DateTime.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed version {Id} status from {OldStatus} to {NewStatus}",
            id, oldStatus, newStatus);

        return MapToDto(entity);
    }

    public async Task<AppVersionDto?> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null) return null;

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await AppVersions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted version {Id}", id);
        return true;
    }

    public async Task<IEnumerable<string>> GetApplicationCodesAsync(CancellationToken cancellationToken = default)
    {
        return await AppVersions
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => x.ApplicationCode)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private static AppVersionDto MapToDto(AppVersion entity) => new()
    {
        Id = entity.Id,
        ApplicationCode = entity.ApplicationCode,
        ApplicationNameAr = entity.ApplicationNameAr,
        ApplicationNameEn = entity.ApplicationNameEn,
        VersionNumber = entity.VersionNumber,
        BuildNumber = entity.BuildNumber,
        Status = entity.Status,
        StatusName = entity.Status.ToString(),
        ReleaseDate = entity.ReleaseDate,
        DeprecationStartDate = entity.DeprecationStartDate,
        EndOfSupportDate = entity.EndOfSupportDate,
        ReleaseNotesAr = entity.ReleaseNotesAr,
        ReleaseNotesEn = entity.ReleaseNotesEn,
        UpdateUrl = entity.UpdateUrl,
        DownloadUrl = entity.DownloadUrl,
        IsForceUpdate = entity.IsForceUpdate,
        MinimumSupportedVersion = entity.MinimumSupportedVersion,
        IsActive = entity.IsActive,
        Metadata = entity.Metadata,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
