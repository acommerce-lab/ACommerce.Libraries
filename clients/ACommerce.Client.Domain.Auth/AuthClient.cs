using ACommerce.Client.Http;
using ACommerce.Client.Operations;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.OperationEngine.Wire;

namespace ACommerce.Client.Domain.Auth;

/// <summary>
/// عميل المصادقة - يبني قيوداً محلية ويرسلها عبر ClientOpEngine.
///
/// كل طريقة تبدأ ببناء Entry محاسبية (نفس Entry الخادم تماماً)،
/// تضيف روابط الـ Subject (المستخدم) و Issuer، وتدفع للـ dispatcher.
/// </summary>
public class AuthClient
{
    private readonly ClientOpEngine _engine;

    public AuthClient(ClientOpEngine engine) => _engine = engine;

    /// <summary>طلب رمز OTP عبر SMS</summary>
    public async Task<OperationEnvelope<SmsOtpRequestResult>> RequestSmsAsync(
        RequestSmsOtpIntent intent, CancellationToken ct = default)
    {
        var op = Entry.Create("auth.sms.request")
            .Describe($"Request SMS OTP for {intent.PhoneNumber}")
            .From("Client:anonymous", 1, ("role", "subject"))
            .To("Server:ashare", 1, ("role", "issuer"))
            .Tag("phone_number", intent.PhoneNumber)
            .Tag("credential", "otp")
            .Build();

        return await _engine.ExecuteAsync<SmsOtpRequestResult>(op, intent, ct);
    }

    /// <summary>التحقق من الكود وإصدار JWT</summary>
    public async Task<OperationEnvelope<AuthTokenResult>> VerifySmsAsync(
        VerifySmsOtpIntent intent, CancellationToken ct = default)
    {
        var op = Entry.Create("auth.sms.verify")
            .Describe($"Verify SMS OTP for user {intent.UserId}")
            .From($"User:{intent.UserId}", 1, ("role", "subject"))
            .To("Server:ashare", 1, ("role", "issuer"))
            .Tag("challenge_id", intent.ChallengeId)
            .Tag("credential", "otp")
            .Build();

        return await _engine.ExecuteAsync<AuthTokenResult>(op, intent, ct);
    }

    /// <summary>طلب رمز OTP عبر البريد</summary>
    public async Task<OperationEnvelope<SmsOtpRequestResult>> RequestEmailAsync(
        RequestEmailOtpIntent intent, CancellationToken ct = default)
    {
        var op = Entry.Create("auth.email.request")
            .Describe($"Request email OTP for {intent.Email}")
            .From("Client:anonymous", 1, ("role", "subject"))
            .To("Server:ashare", 1, ("role", "issuer"))
            .Tag("email", intent.Email)
            .Tag("credential", "otp")
            .Build();

        return await _engine.ExecuteAsync<SmsOtpRequestResult>(op, intent, ct);
    }

    /// <summary>تجديد الرمز</summary>
    public async Task<OperationEnvelope<AuthTokenResult>> RefreshAsync(
        RefreshTokenIntent intent, CancellationToken ct = default)
    {
        var op = Entry.Create("auth.refresh")
            .Describe($"Refresh token for user {intent.UserId}")
            .From($"User:{intent.UserId}", 1, ("role", "subject"), ("token_kind", "refresh"))
            .To("Server:ashare", 1, ("role", "issuer"), ("token_kind", "access"))
            .Tag("credential", "refresh_token")
            .Build();

        return await _engine.ExecuteAsync<AuthTokenResult>(op, intent, ct);
    }

    /// <summary>تسجيل خروج</summary>
    public async Task<OperationEnvelope<object>> SignOutAsync(
        SignOutIntent intent, CancellationToken ct = default)
    {
        var op = Entry.Create("auth.signout")
            .Describe("Sign out")
            .From("Client:anonymous", 1, ("role", "subject"))
            .To("Server:ashare", 1, ("role", "issuer"))
            .Tag("token_kind", "access")
            .Build();

        return await _engine.ExecuteAsync<object>(op, intent, ct);
    }

    /// <summary>
    /// تسجيل routes الـ Auth في الـ registry.
    /// يُستدعى من AddAshareClient(routes =&gt; routes.MapAuth()).
    /// </summary>
    public static void RegisterRoutes(HttpRouteRegistry routes)
    {
        routes.Map("auth.sms.request",  HttpMethod.Post, "/api/auth/sms/request");
        routes.Map("auth.sms.verify",   HttpMethod.Post, "/api/auth/sms/verify");
        routes.Map("auth.email.request", HttpMethod.Post, "/api/auth/email/request");
        routes.Map("auth.refresh",      HttpMethod.Post, "/api/auth/refresh");
        routes.Map("auth.signout",      HttpMethod.Post, "/api/auth/signout");
        routes.Map("auth.token.validate", HttpMethod.Post, "/api/auth/token/validate");
    }
}
