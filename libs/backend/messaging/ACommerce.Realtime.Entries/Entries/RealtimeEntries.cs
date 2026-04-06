using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Builder;
using ACommerce.Realtime.Entries.Abstractions;

namespace ACommerce.Realtime.Entries.Entries;

/// <summary>
/// قيود الزمن الحقيقي - كل تفاعل = قيد بين أطراف.
///
/// لا كيانات. لا قاعدة بيانات. فقط تجريدات.
/// مصمم التطبيق يقرر في hooks ماذا يحفظ وكيف.
///
/// مثال الاستخدام:
///   var entry = RealtimeEntries.SendToUser(transport, "user:123", "NewOrder", orderData);
///   entry.Hooks.AfterExecute = async ctx => await db.SaveNotificationAsync(...);
///   await engine.ExecuteAsync(entry);
/// </summary>
public static class RealtimeEntries
{
    // =========================================================================
    // إرسال لمستخدم محدد
    // =========================================================================

    /// <summary>
    /// قيد: إرسال رسالة لمستخدم عبر الزمن الحقيقي.
    /// المُرسل (مدين) يُصدر رسالة → المُستلم (دائن) يستحق استلامها.
    /// </summary>
    public static Entry SendToUser(
        IRealtimeTransport transport,
        string userId,
        string method,
        object data,
        string? senderId = null)
    {
        return EntryBuilder.Create("realtime.send.user")
            .Describe($"Send {method} to {userId}")
            .From(senderId ?? "System", "Message", 1)
            .To($"User:{userId}", "Message", 1)

            .Validate(ctx =>
            {
                if (string.IsNullOrEmpty(userId))
                {
                    ctx.AddValidationError("userId", "Required");
                    return false;
                }
                return true;
            })

            .Execute(async ctx =>
            {
                await transport.SendToUserAsync(userId, method, data, ctx.CancellationToken);
                ctx.Set("delivered", true);
                ctx.Set("method", method);
                ctx.Set("userId", userId);
            })

            .Build();
    }

    // =========================================================================
    // إرسال لمجموعة
    // =========================================================================

    /// <summary>
    /// قيد: إرسال لمجموعة (مثل: غرفة دردشة، موضوع اشتراك).
    /// المُرسل → كل أعضاء المجموعة.
    /// </summary>
    public static Entry SendToGroup(
        IRealtimeTransport transport,
        string groupName,
        string method,
        object data,
        string? senderId = null)
    {
        return EntryBuilder.Create("realtime.send.group")
            .Describe($"Send {method} to group {groupName}")
            .From(senderId ?? "System", "GroupMessage", 1)
            .To($"Group:{groupName}", "GroupMessage", 1)

            .Execute(async ctx =>
            {
                await transport.SendToGroupAsync(groupName, method, data, ctx.CancellationToken);
                ctx.Set("delivered", true);
                ctx.Set("groupName", groupName);
            })

            .Build();
    }

    // =========================================================================
    // بث عام
    // =========================================================================

    public static Entry Broadcast(
        IRealtimeTransport transport,
        string method,
        object data)
    {
        return EntryBuilder.Create("realtime.broadcast")
            .Describe($"Broadcast {method}")
            .From("System", "Broadcast", 1)
            .To("All", "Broadcast", 1)

            .Execute(async ctx =>
            {
                await transport.BroadcastAsync(method, data, ctx.CancellationToken);
                ctx.Set("delivered", true);
            })

            .Build();
    }

    // =========================================================================
    // اتصال/انقطاع مستخدم
    // =========================================================================

    /// <summary>
    /// قيد: مستخدم يتصل بالنظام.
    /// المستخدم (مدين) يقدم اتصاله → النظام (دائن) يسجله.
    /// </summary>
    public static Entry UserConnected(
        string userId,
        string connectionId,
        IConnectionStore? connectionStore = null)
    {
        return EntryBuilder.Create("realtime.connect")
            .Describe($"User {userId} connected")
            .From($"User:{userId}", "Connection", 1)
            .To("System", "Connection", 1)

            .Execute(async ctx =>
            {
                if (connectionStore != null)
                    await connectionStore.SetConnectionAsync(userId, connectionId, ctx.CancellationToken);
                ctx.Set("userId", userId);
                ctx.Set("connectionId", connectionId);
                ctx.Set("connectedAt", DateTime.UtcNow);
            })

            .Build();
    }

    /// <summary>
    /// قيد: مستخدم ينقطع.
    /// النظام (مدين) يفقد اتصالاً → المستخدم (دائن) ينسحب.
    /// </summary>
    public static Entry UserDisconnected(
        string userId,
        string connectionId,
        IConnectionStore? connectionStore = null)
    {
        return EntryBuilder.Create("realtime.disconnect")
            .Describe($"User {userId} disconnected")
            .From("System", "Disconnection", 1)
            .To($"User:{userId}", "Disconnection", 1)

            .Execute(async ctx =>
            {
                if (connectionStore != null)
                    await connectionStore.RemoveConnectionAsync(userId, ctx.CancellationToken);
                ctx.Set("userId", userId);
                ctx.Set("disconnectedAt", DateTime.UtcNow);
            })

            .Build();
    }

    // =========================================================================
    // اشتراك/إلغاء اشتراك في مجموعة
    // =========================================================================

    public static Entry SubscribeToGroup(
        IRealtimeTransport transport,
        string userId,
        string connectionId,
        string groupName)
    {
        return EntryBuilder.Create("realtime.subscribe")
            .Describe($"User {userId} subscribes to {groupName}")
            .From($"User:{userId}", "Subscription", 1)
            .To($"Group:{groupName}", "Subscription", 1)

            .Execute(async ctx =>
            {
                await transport.AddToGroupAsync(connectionId, groupName, ctx.CancellationToken);
                ctx.Set("subscribed", true);
            })

            .Build();
    }

    public static Entry UnsubscribeFromGroup(
        IRealtimeTransport transport,
        string userId,
        string connectionId,
        string groupName)
    {
        return EntryBuilder.Create("realtime.unsubscribe")
            .Describe($"User {userId} unsubscribes from {groupName}")
            .From($"Group:{groupName}", "Unsubscription", 1)
            .To($"User:{userId}", "Unsubscription", 1)

            .Execute(async ctx =>
            {
                await transport.RemoveFromGroupAsync(connectionId, groupName, ctx.CancellationToken);
                ctx.Set("unsubscribed", true);
            })

            .Build();
    }

    // =========================================================================
    // إشعار متعدد القنوات (مُركّب من قيود فرعية)
    // =========================================================================

    /// <summary>
    /// قيد مُركّب: إرسال إشعار عبر عدة قنوات.
    /// كل قناة = قيد فرعي. النجاح الجزئي مقبول.
    ///
    /// مثال:
    ///   var entry = RealtimeEntries.MultiChannelNotify(transport, "user:123", "NewBooking",
    ///       payload, channels: new[] { "inapp", "push" });
    ///   entry.Hooks.AfterComplete = ctx => SaveToDb(ctx);
    ///   await engine.ExecuteAsync(entry);
    /// </summary>
    public static Entry MultiChannelNotify(
        IRealtimeTransport transport,
        string userId,
        string notificationType,
        object payload,
        string[]? channels = null)
    {
        channels ??= new[] { "inapp" };

        var builder = EntryBuilder.Create("realtime.notify")
            .Describe($"Notify {userId}: {notificationType}")
            .From("System", "Notification", channels.Length)
            .To($"User:{userId}", "Notification", channels.Length)

            .Execute(ctx =>
            {
                ctx.Set("notificationType", notificationType);
                ctx.Set("userId", userId);
                ctx.Set("payload", payload);
            });

        // قيد فرعي لكل قناة
        foreach (var channel in channels)
        {
            builder.WithSubEntry($"deliver.{channel}", sub =>
            {
                sub.From($"Channel:{channel}", "Delivery", 1)
                   .To($"User:{userId}", "Delivery", 1)
                   .Execute(async ctx =>
                   {
                       var method = channel switch
                       {
                           "inapp" => "ReceiveNotification",
                           "push" => "ReceivePush",
                           "badge" => "UpdateBadgeCount",
                           _ => $"Receive_{channel}"
                       };
                       await transport.SendToUserAsync(userId, method, payload, ctx.CancellationToken);
                       ctx.Set($"delivered_{channel}", true);
                   });
            });
        }

        return builder.Build();
    }
}
