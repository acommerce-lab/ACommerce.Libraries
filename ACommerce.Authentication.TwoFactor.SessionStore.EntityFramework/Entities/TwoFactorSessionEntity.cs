namespace ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Entities;

/// <summary>
/// Entity Framework entity for two-factor sessions
/// </summary>
public class TwoFactorSessionEntity
{
    public string TransactionId { get; set; } = default!;
    public string Identifier { get; set; } = default!;
    public string Provider { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? VerificationCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? MetadataJson { get; set; }
}