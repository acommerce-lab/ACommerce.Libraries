namespace ACommerce.Profiles.Events;

/// <summary>
/// Event raised when a new profile is created
/// </summary>
public record ProfileCreatedEvent
{
	public required string UserId { get; init; }
	public required string ProfileId { get; init; }
	public string? FullName { get; init; }
	public string? Email { get; init; }
	public string? PhoneNumber { get; init; }
	public List<ContactPointInfo> ContactPoints { get; init; } = new();
	public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Event raised when a contact point is added to a profile
/// </summary>
public record ContactPointAddedEvent
{
	public required string UserId { get; init; }
	public required string ProfileId { get; init; }
	public required string Type { get; init; }
	public required string Value { get; init; }
	public bool IsVerified { get; init; }
	public bool IsPrimary { get; init; }
	public DateTime AddedAt { get; init; }
}

/// <summary>
/// Event raised when a contact point is verified
/// </summary>
public record ContactPointVerifiedEvent
{
	public required string UserId { get; init; }
	public required string ProfileId { get; init; }
	public required string Type { get; init; }
	public required string Value { get; init; }
	public DateTime VerifiedAt { get; init; }
}

/// <summary>
/// Contact point information included in profile events
/// </summary>
public record ContactPointInfo
{
	public required string Type { get; init; }
	public required string Value { get; init; }
	public bool IsVerified { get; init; }
	public bool IsPrimary { get; init; }
}
