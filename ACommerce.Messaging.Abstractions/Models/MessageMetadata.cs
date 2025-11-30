using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Messaging.Abstractions.Models;

/// <summary>
/// Message metadata (correlation, tracing, etc.)
/// </summary>
public record MessageMetadata
{
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public string? UserId { get; init; }
    public string? TenantId { get; init; }
    public string? SourceService { get; init; }
    public int Priority { get; init; } = 0;
    public TimeSpan? TimeToLive { get; init; }
    [NotMapped] public Dictionary<string, string> Headers { get; init; } = new();
}