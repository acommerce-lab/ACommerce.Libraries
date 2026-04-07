using ACommerce.Authentication.Operations.Abstractions;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;

namespace ACommerce.Authentication.Operations.Operations;

/// <summary>
/// قيود المصادقة - كل تفاعل = قيد بين أطراف.
///
/// SignIn: المستخدم يُقدم بيانات اعتماد → المُصدر يُقدم هوية.
/// SignOut: المستخدم ينهي جلسته → المُصدر يُلغيها.
/// Refresh: جلسة قديمة → رمز جديد.
/// Validate: رمز → هوية.
/// </summary>
public static class AuthOps
{
    // =========================================================================
    // تسجيل الدخول
    // =========================================================================

    /// <summary>
    /// قيد: تسجيل دخول.
    /// المُصدر (مدين) يُصدر هوية → المستخدم (دائن) يحصل على جلسة/رمز.
    /// </summary>
    public static Operation SignIn(
        AuthPartyId user,
        ICredential credential,
        IAuthenticator authenticator,
        ITokenIssuer? issuer = null)
    {
        return Entry.Create("auth.signin")
            .Describe($"{user} signs in via {authenticator.Name}")
            .From(AuthPartyId.Issuer(authenticator.Name), 1,
                (AuthTags.Role, "issuer"),
                (AuthTags.Status, AuthStatus.Pending))
            .To(user, 1,
                (AuthTags.Role, "subject"),
                (AuthTags.Credential, credential.CredentialType))
            .Tag(AuthTags.Authenticator, authenticator.Name)
            .Tag(AuthTags.Credential, credential.CredentialType)
            .Validate(async ctx =>
            {
                // تمرير التحقق إلى المُصادق
                try
                {
                    var principal = await authenticator.AuthenticateAsync(credential, ctx.CancellationToken);
                    ctx.Set("principal", principal);
                    return true;
                }
                catch (AuthenticationException ex)
                {
                    ctx.AddValidationError(AuthTags.Reason, ex.Reason);
                    return false;
                }
            })
            .Execute(async ctx =>
            {
                ctx.TryGet<IPrincipal>("principal", out var principal);

                // إصدار رمز إذا كان هناك مُصدر
                if (issuer != null && principal != null)
                {
                    var token = await issuer.IssueAsync(principal, ctx.CancellationToken);
                    ctx.Set("token", token);
                }

                // تحديث حالة الطرف المُصدر
                var issuerParty = ctx.Operation.GetPartiesByTag(AuthTags.Role, "issuer").FirstOrDefault();
                if (issuerParty != null)
                {
                    issuerParty.RemoveTag(AuthTags.Status);
                    issuerParty.AddTag(AuthTags.Status, AuthStatus.Authenticated);
                }
            })
            .Build();
    }

    // =========================================================================
    // تسجيل الخروج
    // =========================================================================

    /// <summary>
    /// قيد: تسجيل خروج.
    /// المستخدم (مدين) يُعيد جلسته → المُصدر (دائن) يُلغيها.
    /// </summary>
    public static Operation SignOut(
        AuthPartyId user,
        string sessionOrToken,
        ITokenIssuer? issuer = null,
        ISessionStore? sessionStore = null)
    {
        return Entry.Create("auth.signout")
            .Describe($"{user} signs out")
            .From(user, 1, (AuthTags.Role, "subject"))
            .To(AuthPartyId.Session(sessionOrToken), 1,
                (AuthTags.Role, "session"),
                (AuthTags.Status, AuthStatus.Revoked))
            .Tag(AuthTags.Session, sessionOrToken)
            .Execute(async ctx =>
            {
                if (issuer != null)
                    await issuer.RevokeAsync(sessionOrToken, ctx.CancellationToken);

                if (sessionStore != null)
                    await sessionStore.RemoveAsync(sessionOrToken, ctx.CancellationToken);

                ctx.Set("revokedAt", DateTimeOffset.UtcNow);
            })
            .Build();
    }

    // =========================================================================
    // تجديد الرمز
    // =========================================================================

    /// <summary>
    /// قيد: تجديد رمز.
    /// الرمز القديم (مدين) يُستهلك → رمز جديد (دائن) يُصدر.
    /// </summary>
    public static Operation RefreshToken(
        AuthPartyId user,
        string refreshToken,
        ITokenIssuer issuer)
    {
        return Entry.Create("auth.refresh")
            .Describe($"{user} refreshes token")
            .From(user, 1,
                (AuthTags.Role, "subject"),
                (AuthTags.TokenKind, "refresh"),
                (AuthTags.Status, AuthStatus.Expired))
            .To(AuthPartyId.Issuer(issuer.GetType().Name), 1,
                (AuthTags.Role, "issuer"),
                (AuthTags.TokenKind, "access"))
            .Tag(AuthTags.Credential, CredentialType.RefreshToken)
            .Execute(async ctx =>
            {
                try
                {
                    var newToken = await issuer.RefreshAsync(refreshToken, ctx.CancellationToken);
                    ctx.Set("token", newToken);

                    var issuerParty = ctx.Operation.GetPartiesByTag(AuthTags.Role, "issuer").FirstOrDefault();
                    if (issuerParty != null)
                    {
                        issuerParty.RemoveTag(AuthTags.Status);
                        issuerParty.AddTag(AuthTags.Status, AuthStatus.Authenticated);
                    }
                }
                catch (AuthenticationException ex)
                {
                    ctx.Set("error", ex.Reason);
                    throw;
                }
            })
            .Build();
    }

    // =========================================================================
    // التحقق من رمز
    // =========================================================================

    /// <summary>
    /// قيد: التحقق من رمز/بيانات اعتماد دون إنشاء جلسة.
    /// </summary>
    public static Operation Validate(
        ICredential credential,
        IAuthenticator authenticator)
    {
        return Entry.Create("auth.validate")
            .Describe($"Validate via {authenticator.Name}")
            .From(AuthPartyId.Issuer(authenticator.Name), 1,
                (AuthTags.Role, "issuer"),
                (AuthTags.Status, AuthStatus.Pending))
            .To(AuthPartyId.System, 1, (AuthTags.Role, "validator"))
            .Tag(AuthTags.Authenticator, authenticator.Name)
            .Tag(AuthTags.Credential, credential.CredentialType)
            .Execute(async ctx =>
            {
                try
                {
                    var principal = await authenticator.AuthenticateAsync(credential, ctx.CancellationToken);
                    ctx.Set("principal", principal);
                    ctx.Set("valid", true);

                    var issuerParty = ctx.Operation.GetPartiesByTag(AuthTags.Role, "issuer").FirstOrDefault();
                    if (issuerParty != null)
                    {
                        issuerParty.RemoveTag(AuthTags.Status);
                        issuerParty.AddTag(AuthTags.Status, AuthStatus.Authenticated);
                    }
                }
                catch (AuthenticationException ex)
                {
                    ctx.Set("valid", false);
                    ctx.Set("reason", ex.Reason);

                    var issuerParty = ctx.Operation.GetPartiesByTag(AuthTags.Role, "issuer").FirstOrDefault();
                    if (issuerParty != null)
                    {
                        issuerParty.RemoveTag(AuthTags.Status);
                        issuerParty.AddTag(AuthTags.Status, AuthStatus.Rejected);
                    }
                }
            })
            .Build();
    }
}
