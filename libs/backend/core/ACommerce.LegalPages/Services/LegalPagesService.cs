using ACommerce.LegalPages.Contracts;
using ACommerce.LegalPages.DTOs;
using ACommerce.LegalPages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACommerce.LegalPages.Services;

public class LegalPagesService(DbContext dbContext, ILogger<LegalPagesService> logger) : ILegalPagesService
{
    private DbSet<LegalPage> LegalPages => dbContext.Set<LegalPage>();

    public async Task<IEnumerable<LegalPageDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await LegalPages
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all legal pages");
            throw;
        }
    }

    public async Task<IEnumerable<LegalPageDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Getting active legal pages...");
            var result = await LegalPages
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);
            logger.LogInformation("Found {Count} active legal pages", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active legal pages. Table may not exist.");
            throw;
        }
    }

    public async Task<LegalPageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await LegalPages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<LegalPageDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var entity = await LegalPages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key && !x.IsDeleted, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<LegalPageDto> CreateAsync(CreateLegalPageRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new LegalPage
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            TitleAr = request.TitleAr,
            TitleEn = request.TitleEn,
            Url = request.Url,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await LegalPages.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<LegalPageDto?> UpdateAsync(Guid id, UpdateLegalPageRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await LegalPages.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null) return null;

        if (request.TitleAr is not null) entity.TitleAr = request.TitleAr;
        if (request.TitleEn is not null) entity.TitleEn = request.TitleEn;
        if (request.Url is not null) entity.Url = request.Url;
        if (request.Icon is not null) entity.Icon = request.Icon;
        if (request.SortOrder.HasValue) entity.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await LegalPages.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static LegalPageDto MapToDto(LegalPage entity) => new()
    {
        Id = entity.Id,
        Key = entity.Key,
        TitleAr = entity.TitleAr,
        TitleEn = entity.TitleEn,
        Url = entity.Url,
        Icon = entity.Icon,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}
