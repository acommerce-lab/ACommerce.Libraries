namespace ACommerce.Client.Domain.Auth;

// ═══════════════════════════════════════════════════════════════
// نوايا مصادقة - الكائنات التي تُرسل من واجهة الـ UI
// ═══════════════════════════════════════════════════════════════

public record RequestSmsOtpIntent(string PhoneNumber);

public record VerifySmsOtpIntent(Guid UserId, string ChallengeId, string Code);

public record RequestEmailOtpIntent(string Email);

public record RefreshTokenIntent(string UserId, string RefreshToken);

public record SignOutIntent(string AccessToken);

// ═══════════════════════════════════════════════════════════════
// DTOs للاستجابة (ما تُرجعه الـ Envelope.Data)
// ═══════════════════════════════════════════════════════════════

public record SmsOtpRequestResult(string ChallengeId, Guid UserId, string PhoneNumber, string? Message);

public record AuthTokenResult(
    Guid UserId,
    string PhoneNumber,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);

public record TokenValidationResponse(bool Valid, string? UserId, string? DisplayName);
