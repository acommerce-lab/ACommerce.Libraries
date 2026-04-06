using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.Realtime.Operations.Abstractions;

namespace ACommerce.Realtime.Operations.Operations;

/// <summary>
/// قيود الاتصال والانقطاع والمجموعات.
/// </summary>
public static class ConnectionOps
{
    /// <summary>
    /// المستخدم يتصل بالنظام.
    /// From(User) → To(System): المستخدم يُقدّم اتصاله للنظام.
    /// </summary>
    public static Operation Connect(string userId, string connectionId, IConnectionTracker? tracker = null)
    {
        return Entry.Create("realtime.connect")
            .Describe($"{userId} connected")
            .From($"User:{userId}", 1, (RT.ConnectionId, connectionId))
            .To("System", 1, (RT.Role, "host"))
            .Tag(RT.Presence, "online")
            .Execute(async ctx =>
            {
                if (tracker != null)
                    await tracker.TrackConnectionAsync(userId, connectionId, ctx.CancellationToken);
                ctx.Set("userId", userId);
                ctx.Set("connectionId", connectionId);
                ctx.Set("connectedAt", DateTime.UtcNow);
            })
            .Build();
    }

    /// <summary>
    /// المستخدم ينقطع.
    /// From(System) → To(User): النظام يُسجّل انقطاع المستخدم.
    /// </summary>
    public static Operation Disconnect(string userId, IConnectionTracker? tracker = null)
    {
        return Entry.Create("realtime.disconnect")
            .Describe($"{userId} disconnected")
            .From("System", 1)
            .To($"User:{userId}", 1)
            .Tag(RT.Presence, "offline")
            .Execute(async ctx =>
            {
                if (tracker != null)
                    await tracker.RemoveConnectionAsync(userId, ctx.CancellationToken);
                ctx.Set("userId", userId);
                ctx.Set("disconnectedAt", DateTime.UtcNow);
            })
            .Build();
    }

    /// <summary>
    /// المستخدم ينضم لمجموعة.
    /// From(User) → To(Group): المستخدم يشترك في المجموعة.
    ///
    /// القيمة "groupName" تأتي من التطبيق - ليست ثابتاً.
    /// يمكن أن تكون: "chat_123", "topic_news", "payment_456"
    /// </summary>
    public static Operation JoinGroup(string userId, string connectionId, string groupName,
        IRealtimeTransport transport)
    {
        return Entry.Create("realtime.join_group")
            .Describe($"{userId} joins {groupName}")
            .From($"User:{userId}", 1, (RT.ConnectionId, connectionId))
            .To($"Group:{groupName}", 1, (RT.Group, groupName))
            .Execute(async ctx =>
            {
                await transport.AddToGroupAsync(connectionId, groupName, ctx.CancellationToken);
                ctx.Set("groupName", groupName);
            })
            .Build();
    }

    /// <summary>
    /// المستخدم يغادر مجموعة.
    /// From(Group) → To(User): المجموعة تفقد عضواً.
    /// </summary>
    public static Operation LeaveGroup(string userId, string connectionId, string groupName,
        IRealtimeTransport transport)
    {
        return Entry.Create("realtime.leave_group")
            .Describe($"{userId} leaves {groupName}")
            .From($"Group:{groupName}", 1, (RT.Group, groupName))
            .To($"User:{userId}", 1)
            .Execute(async ctx =>
            {
                await transport.RemoveFromGroupAsync(connectionId, groupName, ctx.CancellationToken);
                ctx.Set("groupName", groupName);
            })
            .Build();
    }

    /// <summary>
    /// إرسال رسالة عامة لمستخدم.
    /// الـ method والـ data يأتيان من التطبيق - ليسا ثوابت.
    /// </summary>
    public static Operation SendToUser(string userId, string method, object data,
        IRealtimeTransport transport, string? senderId = null)
    {
        return Entry.Create("realtime.send")
            .Describe($"Send {method} to {userId}")
            .From(senderId ?? "System", 1, (RT.Role, "sender"))
            .To($"User:{userId}", 1, (RT.Role, "recipient"), (RT.Delivery, "pending"))
            .Tag(RT.Method, method)
            .Execute(async ctx =>
            {
                await transport.SendToUserAsync(userId, method, data, ctx.CancellationToken);
                // تحديث علامة التسليم على الطرف
                var recipient = ctx.Operation.GetPartiesByTag(RT.Role, "recipient").FirstOrDefault();
                if (recipient != null)
                {
                    recipient.RemoveTag(RT.Delivery, "pending");
                    recipient.AddTag(RT.Delivery, "sent");
                }
                ctx.Set("delivered", true);
            })
            .Build();
    }

    /// <summary>
    /// بث عام.
    /// </summary>
    public static Operation Broadcast(string method, object data, IRealtimeTransport transport)
    {
        return Entry.Create("realtime.broadcast")
            .From("System", 1)
            .To("All", 1)
            .Tag(RT.Method, method)
            .Execute(async ctx =>
            {
                await transport.BroadcastAsync(method, data, ctx.CancellationToken);
            })
            .Build();
    }
}
