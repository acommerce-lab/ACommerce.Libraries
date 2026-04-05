using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Builder;

namespace ACommerce.AccountingKernel.Examples;

/// <summary>
/// مثال: كيف تُصمم الإشعارات بالنمط المحاسبي.
/// هذا الكود يُبنى على المحرك الموحد ويستفيد من SharedKernel.
///
/// القيد الرئيسي: "إشعار طلب جديد"
/// ├── مدين: النظام (يُرسل)
/// ├── دائن: المستخدم (يستحق)
/// │
/// ├── قيد فرعي: deliver.email
/// │   ├── مدين: قناة Email (مُكلّفة)
/// │   ├── دائن: المستخدم (يستحق إيميل)
/// │   └── منطق: إرسال SMTP
/// │
/// ├── قيد فرعي: deliver.push
/// │   ├── مدين: قناة Firebase
/// │   ├── دائن: المستخدم
/// │   └── منطق: إرسال FCM
/// │
/// └── قيد فرعي: confirm.read
///     ├── مدين: المستخدم (يُقر بالقراءة)
///     ├── دائن: النظام (يسجل القراءة)
///     └── منطق: يُنفذ لاحقاً عندما يفتح المستخدم الإشعار
/// </summary>
public static class NotificationEntryExample
{
    /// <summary>
    /// بناء قيد إرسال إشعار
    /// </summary>
    public static Entry BuildSendNotification(
        string userId,
        string title,
        string message,
        string[] channels)
    {
        var channelCount = channels.Length;

        var entry = EntryBuilder.Create("notification.send")
            .Describe($"Send notification to {userId}: {title}")
            .From("System", "Notification", channelCount)
            .To($"User:{userId}", "Notification", channelCount)

            // التحقق: هل المستخدم نشط؟ هل القنوات صالحة؟
            .Validate(ctx =>
            {
                // هنا يمكن الحصول على خدمة التحقق من DI
                // var userService = ctx.GetService<IUserService>();
                // return await userService.IsActiveAsync(userId);
                return channels.Length > 0;
            })

            // تخزين البيانات في السياق لاستخدامها لاحقاً
            .Execute(ctx =>
            {
                ctx.Set("title", title);
                ctx.Set("message", message);
                ctx.Set("userId", userId);
            })

            // قيد فرعي لكل قناة (ديناميكياً)
            .WithSubEntries(channels, ch => $"deliver.{ch.ToLower()}", (sub, channel) =>
            {
                sub.From($"Channel:{channel}", "Delivery", 1)
                   .To($"User:{userId}", "Delivery", 1)
                   .Execute(async ctx =>
                   {
                       // المنطق المخصص لكل قناة
                       // var channelService = ctx.GetService<INotificationChannel>();
                       // await channelService.SendAsync(notification);

                       // نحفظ نتيجة الإرسال
                       ctx.Set("_delivered", true);
                   })
                   .OnFailed(ctx =>
                   {
                       // عند فشل قناة: تسجيل للمحاولة لاحقاً
                       ctx.Set("_retry_needed", true);
                       return Task.CompletedTask;
                   });
            })

            // قيد فرعي لتأكيد القراءة (يُنفذ لاحقاً بحدث منفصل)
            .WithSubEntry("confirm.read", sub =>
            {
                sub.From($"User:{userId}", "Acknowledgment", 1)
                   .To("System", "Acknowledgment", 1)
                   .Validate(_ => false) // لن يُنفذ الآن - ينتظر حدث القراءة
                   .WithMetadata("deferred", true);
            })

            // توثيق القيد في سجل العمليات
            .WithAudit()
            .WithMetadata("notification_type", "order_created")
            .Build();

        return entry;
    }

    /// <summary>
    /// بناء قيد تأكيد قراءة إشعار
    /// </summary>
    public static Entry BuildConfirmRead(string userId, Guid notificationId)
    {
        return EntryBuilder.Create("notification.read")
            .Describe($"User {userId} read notification {notificationId}")
            .From($"User:{userId}", "Acknowledgment", 1)
            .To("System", "Acknowledgment", 1)
            .Execute(ctx =>
            {
                // var repo = ctx.GetService<IBaseAsyncRepository<Notification>>();
                // await repo.PartialUpdateAsync(notificationId, new() { ["IsRead"] = true });
                ctx.Set("readAt", DateTime.UtcNow);
            })
            .WithAudit()
            .WithMetadata("original_notification_id", notificationId)
            .Build();
    }

    /// <summary>
    /// مثال: رسالة دردشة
    /// </summary>
    public static Entry BuildChatMessage(
        string senderId,
        string receiverId,
        string content,
        string messageType = "Text")
    {
        return EntryBuilder.Create("chat.message")
            .Describe($"Message from {senderId} to {receiverId}")
            .From($"User:{senderId}", "Message", 1)
            .To($"User:{receiverId}", "Message", 1)

            // حفظ الرسالة في DB عبر SharedKernel
            .WithPersistence()
            // .AddEntity<Message>(ctx => new Message { ... })

            .Execute(ctx =>
            {
                ctx.Set("content", content);
                ctx.Set("messageType", messageType);
                ctx.Set("sentAt", DateTime.UtcNow);
            })

            // قيد فرعي: تسليم عبر SignalR
            .WithSubEntry("deliver.realtime", sub =>
            {
                sub.From("System", "Delivery", 1)
                   .To($"User:{receiverId}", "Delivery", 1)
                   .Execute(async ctx =>
                   {
                       // var hub = ctx.GetService<IRealtimeHub>();
                       // await hub.SendToUserAsync(receiverId, "ReceiveMessage", message);
                       ctx.Set("_delivered_realtime", true);
                   });
            })

            // قيد فرعي: تأكيد الاستلام
            .WithSubEntry("confirm.delivered", sub =>
            {
                sub.From($"User:{receiverId}", "Ack", 1)
                   .To($"User:{senderId}", "Ack", 1)
                   .WithMetadata("deferred", true);
            })

            // قيد فرعي: تأكيد القراءة
            .WithSubEntry("confirm.read", sub =>
            {
                sub.From($"User:{receiverId}", "ReadReceipt", 1)
                   .To($"User:{senderId}", "ReadReceipt", 1)
                   .WithMetadata("deferred", true);
            })

            .WithAudit()
            .Build();
    }

    /// <summary>
    /// مثال: تسجيل دخول
    /// </summary>
    public static Entry BuildLogin(string userId, string provider = "Password")
    {
        return EntryBuilder.Create("auth.login")
            .Describe($"User {userId} login via {provider}")
            .From($"User:{userId}", "Credentials", 1)
            .To("System", "Session", 1)

            // التحقق من البيانات
            .Validate(async ctx =>
            {
                // var authService = ctx.GetService<IAuthService>();
                // return await authService.ValidateCredentialsAsync(userId, password);
                return true;
            })

            // قيد فرعي: التحقق الثنائي (إن لزم)
            .WithSubEntry("verify.2fa", sub =>
            {
                sub.From("System", "OTP", 1)
                   .To($"User:{userId}", "OTP", 1)
                   .Validate(ctx =>
                   {
                       // هل المستخدم يحتاج 2FA؟
                       // return user.TwoFactorEnabled;
                       return true;
                   })
                   .Execute(async ctx =>
                   {
                       // var otpService = ctx.GetService<IOtpService>();
                       // await otpService.SendOtpAsync(userId);
                       ctx.Set("otp_sent", true);
                   });
            })

            // قيد فرعي: إصدار التوكن
            .WithSubEntry("issue.token", sub =>
            {
                sub.From("System", "Token", 1)
                   .To($"User:{userId}", "Token", 1)
                   .Execute(ctx =>
                   {
                       // var tokenService = ctx.GetService<ITokenService>();
                       // var token = tokenService.GenerateToken(userId);
                       ctx.Set("token", "jwt_token_here");
                       ctx.Set("expiresAt", DateTime.UtcNow.AddHours(24));
                   });
            })

            .WithAudit()
            .WithMetadata("provider", provider)
            .WithMetadata("ip_address", "0.0.0.0")
            .Build();
    }
}
