namespace ACommerce.Authentication.TwoFactor.Nafath;

public record NafathWebhookRequest
{
    public required string TransactionId { get; init; }
    public required string NationalId { get; init; }
    public required string Status { get; init; }  // "COMPLETED", "FAILED", etc.
}

