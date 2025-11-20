using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Messaging.Abstractions.Models;

/// <summary>
/// Message envelope containing payload and routing information
/// </summary>
public class MessageEnvelope<T> : IBaseEntity where T : class
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Topic { get; init; }
    public required T Payload { get; init; }
    public required string SourceService { get; init; }
    public string? TargetService { get; init; }
    public MessageMetadata Metadata { get; init; } = new();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
