using ACommerce.ContactPoints.Entities;

namespace ACommerce.ContactPoints.DTOs;

public class CreateContactPointDto
{
    public required string UserId { get; set; }
    public ContactPointType Type { get; set; }
    public required string Value { get; set; }
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateContactPointDto
{
    public string? Value { get; set; }
    public string? Label { get; set; }
    public bool? IsDefault { get; set; }
}

public class ContactPointResponseDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ContactPointType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
