using ACommerce.Notification.Operations.Abstractions;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.Realtime.Operations.Abstractions;

namespace ACommerce.Notification.Operations.Operations;

/// <summary>
/// قيود الإشعارات - كل إشعار = قيد بين النظام والمستخدم.
///
/// مثال:
///   var notify = NotifyOps.Send(transport, "user:123", "طلب جديد", "لديك طلب",
///       channels: new[] { myInAppChannel, myPushChannel },
///       notificationType: "new_order", priority: "high");
///
///   notify.Hooks.AfterComplete = ctx => db.SaveNotificationAsync(...);
///   await engine.ExecuteAsync(notify);
/// </summary>
public static class NotifyOps
{
    /// <summary>
    /// قيد: إرسال إشعار عبر قناة واحدة.
    /// From(System) → To(User): النظام يُصدر إشعاراً للمستخدم.
    /// </summary>
    public static Operation SendSingle(
        string userId,
        string title,
        string message,
        INotificationChannel channel,
        string notificationType = "info",
        string priority = "normal")
    {
        return Entry.Create("notify.send")
            .Describe($"Notify {userId}: {title}")
            .From("System", 1, (RT.Role, "sender"))
            .To($"User:{userId}", 1, (RT.Role, "recipient"), (RT.Delivery, "pending"))
            .Tag(NotifyTags.Channel, channel.ChannelName)
            .Tag(NotifyTags.NotificationType, notificationType)
            .Tag(NotifyTags.Priority, priority)
            .Validate(async ctx =>
            {
                var ok = await channel.ValidateAsync(userId, ctx.CancellationToken);
                if (!ok) ctx.AddValidationError("channel", $"{channel.ChannelName} not available for {userId}");
                return ok;
            })
            .Execute(async ctx =>
            {
                var sent = await channel.SendAsync(userId, title, message, null, ctx.CancellationToken);
                var recipient = ctx.Operation.GetPartiesByTag(RT.Role, "recipient").FirstOrDefault();
                if (recipient != null)
                {
                    recipient.RemoveTag(RT.Delivery);
                    recipient.AddTag(RT.Delivery, sent ? "sent" : "failed");
                }
                ctx.Set("sent", sent);
                ctx.Set("channel", channel.ChannelName);
            })
            .Build();
    }

    /// <summary>
    /// قيد مُركّب: إرسال إشعار عبر عدة قنوات.
    /// كل قناة = قيد فرعي. النجاح الجزئي مقبول.
    ///
    /// From(System, N) → To(User, N) حيث N = عدد القنوات
    /// + قيد فرعي لكل قناة
    ///
    /// العلامات تحمل قيماً من التطبيق:
    ///   [notification_type:new_order]  ← من منطق الأعمال
    ///   [priority:high]               ← من منطق الأعمال
    ///   [channel:inapp]               ← من اسم القناة
    ///   [channel:push]                ← من اسم القناة
    ///   [channel_delivery:pending → sent/failed]  ← تتغير مع التنفيذ
    /// </summary>
    public static Operation SendMultiChannel(
        string userId,
        string title,
        string message,
        IEnumerable<INotificationChannel> channels,
        string notificationType = "info",
        string priority = "normal",
        object? extraData = null)
    {
        var channelList = channels.ToList();

        var builder = Entry.Create("notify.multi")
            .Describe($"Multi-notify {userId}: {title} ({channelList.Count} channels)")
            .From("System", channelList.Count, (RT.Role, "sender"))
            .To($"User:{userId}", channelList.Count, (RT.Role, "recipient"))
            .Tag(NotifyTags.NotificationType, notificationType)
            .Tag(NotifyTags.Priority, priority);

        // علامة لكل قناة
        foreach (var ch in channelList)
            builder.Tag(NotifyTags.Channel, ch.ChannelName);

        builder.Execute(ctx =>
        {
            ctx.Set("title", title);
            ctx.Set("message", message);
            ctx.Set("userId", userId);
            if (extraData != null) ctx.Set("extraData", extraData);
        });

        // قيد فرعي لكل قناة
        foreach (var channel in channelList)
        {
            var ch = channel; // capture
            builder.WithSub($"notify.channel.{ch.ChannelName}", sub =>
            {
                sub.Party($"Channel:{ch.ChannelName}", 1,
                    ("direction", "debit"),
                    (NotifyTags.ChannelDelivery, "pending"));
                sub.Party($"User:{userId}", 1,
                    ("direction", "credit"),
                    (RT.Role, "recipient"));

                sub.Execute(async ctx =>
                {
                    var sent = await ch.SendAsync(userId, title, message, extraData, ctx.CancellationToken);
                    // تحديث علامة التسليم على طرف القناة
                    var chParty = ctx.Operation.GetPartiesByTag("direction", "debit").FirstOrDefault();
                    if (chParty != null)
                    {
                        chParty.RemoveTag(NotifyTags.ChannelDelivery);
                        chParty.AddTag(NotifyTags.ChannelDelivery, sent ? "sent" : "failed");
                    }
                    ctx.Set($"sent_{ch.ChannelName}", sent);
                });
            });
        }

        return builder.Build();
    }

    /// <summary>
    /// قيد: المستخدم قرأ الإشعار.
    /// معكوس لقيد الإرسال.
    /// From(User) → To(System): المستخدم يُقر بالقراءة.
    /// </summary>
    public static Operation MarkRead(string userId, Guid? originalNotifyOpId = null)
    {
        var builder = Entry.Create("notify.read")
            .From($"User:{userId}", 1, (RT.Delivery, "read"))
            .To("System", 1)
            .Execute(ctx =>
            {
                ctx.Set("readAt", DateTime.UtcNow);
                ctx.Set("userId", userId);
            });

        if (originalNotifyOpId != null)
            builder.Fulfills(originalNotifyOpId.Value);

        return builder.Build();
    }

    /// <summary>
    /// قيد: اشتراك في موضوع إشعارات.
    /// From(User) → To(Topic)
    /// </summary>
    public static Operation Subscribe(string userId, string topic)
    {
        return Entry.Create("notify.subscribe")
            .From($"User:{userId}", 1)
            .To($"Topic:{topic}", 1, (RT.Group, $"topic_{topic}"))
            .Tag("topic", topic)
            .Build();
    }
}
