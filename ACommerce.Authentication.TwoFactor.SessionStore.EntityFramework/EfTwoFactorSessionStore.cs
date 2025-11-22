using ACommerce.Authentication.TwoFactor.Abstractions;
using ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework;

/// <summary>
/// Entity Framework implementation of ITwoFactorSessionStore
/// Works with any DbContext that contains TwoFactorSessionEntity
/// </summary>
public class EfTwoFactorSessionStore<TContext>(
    TContext context,
    ILogger<EfTwoFactorSessionStore<TContext>> logger) : ITwoFactorSessionStore
    where TContext : DbContext
{
    public async Task<TwoFactorSession?> GetSessionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[EF] Getting session {TransactionId}", transactionId);

        var entity = await context.Set<TwoFactorSessionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.TransactionId == transactionId,
                cancellationToken);

        if (entity == null)
        {
            logger.LogWarning("[EF] Session {TransactionId} not found", transactionId);
            return null;
        }

        // Check if expired
        if (entity.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("[EF] Session {TransactionId} has expired", transactionId);
            return null;
        }

        return MapToSession(entity);
    }

    public async Task<string> CreateSessionAsync(
        TwoFactorSession session,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[EF] Creating session {TransactionId} for {Identifier}",
            session.TransactionId,
            session.Identifier);

        var entity = MapToEntity(session);

        context.Set<TwoFactorSessionEntity>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.TransactionId;
    }

    public async Task UpdateSessionAsync(
        TwoFactorSession session,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[EF] Updating session {TransactionId} to status {Status}",
            session.TransactionId,
            session.Status);

        var entity = await context.Set<TwoFactorSessionEntity>()
            .FirstOrDefaultAsync(
                s => s.TransactionId == session.TransactionId,
                cancellationToken);

        if (entity == null)
        {
            logger.LogError(
                "[EF] Cannot update session {TransactionId} - not found",
                session.TransactionId);
            throw new InvalidOperationException(
                $"Session {session.TransactionId} not found");
        }

        entity.Status = session.Status.ToString();
        entity.VerificationCode = session.VerificationCode;
        entity.MetadataJson = session.Metadata != null
            ? JsonSerializer.Serialize(session.Metadata)
            : null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSessionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[EF] Deleting session {TransactionId}", transactionId);

        var entity = await context.Set<TwoFactorSessionEntity>()
            .FirstOrDefaultAsync(
                s => s.TransactionId == transactionId,
                cancellationToken);

        if (entity != null)
        {
            context.Set<TwoFactorSessionEntity>().Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private TwoFactorSession MapToSession(TwoFactorSessionEntity entity)
    {
        return new TwoFactorSession
        {
            TransactionId = entity.TransactionId,
            Identifier = entity.Identifier,
            Provider = entity.Provider,
            Status = Enum.Parse<TwoFactorSessionStatus>(entity.Status),
            VerificationCode = entity.VerificationCode,
            CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc),
            ExpiresAt = DateTime.SpecifyKind(entity.ExpiresAt, DateTimeKind.Utc),
            Metadata = string.IsNullOrEmpty(entity.MetadataJson)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson)
                  ?? []
        };
    }

    private TwoFactorSessionEntity MapToEntity(TwoFactorSession session)
    {
        return new TwoFactorSessionEntity
        {
            TransactionId = session.TransactionId,
            Identifier = session.Identifier,
            Provider = session.Provider,
            Status = session.Status.ToString(),
            VerificationCode = session.VerificationCode,
            CreatedAt = session.CreatedAt.UtcDateTime,
            ExpiresAt = session.ExpiresAt.UtcDateTime,
            MetadataJson = session.Metadata != null
                ? JsonSerializer.Serialize(session.Metadata)
                : null
        };
    }
}