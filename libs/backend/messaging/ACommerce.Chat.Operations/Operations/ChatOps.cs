using ACommerce.Chat.Operations.Abstractions;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.Realtime.Operations.Abstractions;

namespace ACommerce.Chat.Operations.Operations;

/// <summary>
/// قيود الدردشة - كل تفاعل = قيد بين أطراف.
///
/// لا كيانات. لا قاعدة بيانات. المطور يحفظ ما يريد عبر Hooks.
///
/// مثال كامل:
///   var sendMsg = ChatOps.SendMessage(transport, "ahmed", "chat_123", "مرحبا", "text");
///   sendMsg.Hooks.AfterExecute = async ctx => {
///       var msg = new MyMessageEntity { ... };
///       await db.Messages.AddAsync(msg);  // المطور يحفظ بالشكل الذي يريده
///   };
///   await engine.ExecuteAsync(sendMsg);
/// </summary>
public static class ChatOps
{
    // =========================================================================
    // إرسال رسالة
    // =========================================================================

    /// <summary>
    /// قيد: إرسال رسالة في محادثة.
    ///
    /// From(المُرسل) → To(المحادثة)
    /// + قيد فرعي: تسليم (System → كل مستلم)
    /// + قيد فرعي مؤجل: إيصال قراءة (المستلم → المُرسل)
    ///
    /// العلامات تحمل قيماً من التطبيق:
    ///   [conversation:chat_123]        ← معرف المحادثة
    ///   [message_type:text]            ← نوع الرسالة
    ///   [reply_to:msg_456]             ← رسالة مُرد عليها (اختياري)
    ///   [delivery:pending → sent → delivered → read]  ← تتغير مع الحالة
    /// </summary>
    public static Operation SendMessage(
        IRealtimeTransport transport,
        string senderId,
        string conversationId,
        string content,
        string messageType = "text",
        string? replyToId = null,
        string[]? recipientIds = null)
    {
        var builder = Entry.Create("chat.message.send")
            .Describe($"{senderId} sends {messageType} to {conversationId}")
            .From($"User:{senderId}", 1,
                (RT.Role, "sender"))
            .To($"Conversation:{conversationId}", 1,
                (ChatTags.Conversation, conversationId),
                (ChatTags.ConversationType, "group"))  // يُعدّل لاحقاً حسب النوع
            .Tag(ChatTags.MessageType, messageType)
            .Tag(ChatTags.Conversation, conversationId);

        if (replyToId != null)
            builder.Tag(ChatTags.ReplyTo, replyToId);

        builder.Execute(async ctx =>
        {
            // بث الرسالة لمجموعة المحادثة
            var payload = new
            {
                senderId,
                conversationId,
                content,
                messageType,
                replyToId,
                sentAt = DateTime.UtcNow
            };
            await transport.SendToGroupAsync(
                $"chat_{conversationId}", "MessageReceived", payload, ctx.CancellationToken);

            ctx.Set("content", content);
            ctx.Set("sentAt", DateTime.UtcNow);
            ctx.Set("payload", payload);
        });

        // قيود فرعية: تسليم لكل مستلم (إن حُددوا)
        if (recipientIds != null)
        {
            builder.WithSub("chat.delivery", sub =>
            {
                foreach (var recipientId in recipientIds)
                {
                    sub.Party($"User:{recipientId}", 1,
                        ("direction", "credit"),
                        (RT.Delivery, "pending"),
                        (RT.Role, "recipient"));
                }
                sub.Party("System", recipientIds.Length, ("direction", "debit"));
            });
        }

        return builder.Build();
    }

    // =========================================================================
    // تأكيد الاستلام (✓)
    // =========================================================================

    /// <summary>
    /// قيد: المستلم يؤكد استلام الرسالة.
    /// هذا معكوس جزئي لقيد التسليم.
    ///
    /// From(المستلم) → To(المُرسل): "استلمت رسالتك"
    /// [delivery] تتغير من "pending" إلى "delivered"
    /// </summary>
    public static Operation AcknowledgeDelivery(
        IRealtimeTransport transport,
        string recipientId,
        string senderId,
        string conversationId,
        Guid? originalMessageOpId = null)
    {
        var builder = Entry.Create("chat.delivery.ack")
            .Describe($"{recipientId} acknowledges delivery")
            .From($"User:{recipientId}", 1, (RT.Delivery, "delivered"))
            .To($"User:{senderId}", 1, (RT.Role, "sender"))
            .Tag(ChatTags.Conversation, conversationId);

        if (originalMessageOpId != null)
            builder.PartiallyFulfills(originalMessageOpId.Value);

        builder.Execute(async ctx =>
        {
            // إرسال علامة ✓ للمُرسل
            await transport.SendToUserAsync(senderId, "DeliveryAck",
                new { recipientId, conversationId, ack = "delivered" }, ctx.CancellationToken);
        });

        return builder.Build();
    }

    // =========================================================================
    // إيصال القراءة (✓✓)
    // =========================================================================

    /// <summary>
    /// قيد: المستلم قرأ الرسالة.
    /// معكوس كلي لقيد التسليم.
    ///
    /// [delivery] تتغير من "delivered" إلى "read"
    /// </summary>
    public static Operation MarkAsRead(
        IRealtimeTransport transport,
        string readerId,
        string senderId,
        string conversationId,
        Guid? originalDeliveryOpId = null)
    {
        var builder = Entry.Create("chat.delivery.read")
            .Describe($"{readerId} read message in {conversationId}")
            .From($"User:{readerId}", 1, (RT.Delivery, "read"))
            .To($"User:{senderId}", 1, (RT.Role, "sender"))
            .Tag(ChatTags.Conversation, conversationId);

        if (originalDeliveryOpId != null)
            builder.Fulfills(originalDeliveryOpId.Value);

        builder.Execute(async ctx =>
        {
            // إرسال علامة ✓✓ للمُرسل
            await transport.SendToUserAsync(senderId, "ReadReceipt",
                new { readerId, conversationId, readAt = DateTime.UtcNow }, ctx.CancellationToken);
        });

        return builder.Build();
    }

    // =========================================================================
    // مؤشر الكتابة
    // =========================================================================

    /// <summary>
    /// قيد مؤقت: المستخدم يكتب.
    /// لا يُحفظ عادةً - فقط يُبث.
    /// [typing] تحمل القيمة: "typing" أو "stopped"
    /// </summary>
    public static Operation TypingIndicator(
        IRealtimeTransport transport,
        string userId,
        string conversationId,
        bool isTyping)
    {
        return Entry.Create("chat.typing")
            .From($"User:{userId}", 0)  // قيمة 0 = لا يحتاج توازن
            .To($"Conversation:{conversationId}", 0)
            .Tag(ChatTags.Typing, isTyping ? "typing" : "stopped")
            .Tag(ChatTags.Conversation, conversationId)
            .Execute(async ctx =>
            {
                await transport.SendToGroupAsync($"chat_{conversationId}", "UserTyping",
                    new { userId, isTyping }, ctx.CancellationToken);
            })
            .Build();
    }

    // =========================================================================
    // إدارة المحادثات
    // =========================================================================

    /// <summary>
    /// قيد: إنشاء محادثة.
    /// From(المُنشئ) → To(النظام): المُنشئ يُسجّل محادثة جديدة.
    /// </summary>
    public static Operation CreateConversation(
        string creatorId,
        string conversationType,
        string? title = null,
        string[]? participantIds = null)
    {
        var builder = Entry.Create("chat.conversation.create")
            .Describe($"{creatorId} creates {conversationType} conversation")
            .From($"User:{creatorId}", 1, (RT.Role, "creator"))
            .To("System", 1)
            .Tag(ChatTags.ConversationType, conversationType);

        if (title != null)
            builder.Meta("title", title);

        // قيد فرعي لكل مشارك
        if (participantIds != null)
        {
            foreach (var pid in participantIds)
            {
                builder.WithSub($"chat.participant.add.{pid}", sub =>
                {
                    sub.Party($"User:{pid}", 1, ("direction", "credit"), (RT.Role, "member"));
                    sub.Party("System", 1, ("direction", "debit"));
                });
            }
        }

        return builder.Build();
    }

    /// <summary>
    /// قيد: مشارك ينضم لمحادثة.
    /// </summary>
    public static Operation JoinConversation(
        IRealtimeTransport transport,
        string userId,
        string connectionId,
        string conversationId,
        string role = "member")
    {
        return Entry.Create("chat.conversation.join")
            .From($"User:{userId}", 1, (RT.Role, role))
            .To($"Conversation:{conversationId}", 1, (ChatTags.Conversation, conversationId))
            .Execute(async ctx =>
            {
                // إضافة للمجموعة في النقل
                await transport.AddToGroupAsync(connectionId, $"chat_{conversationId}", ctx.CancellationToken);
                // إبلاغ المجموعة
                await transport.SendToGroupAsync($"chat_{conversationId}", "ParticipantJoined",
                    new { userId, role }, ctx.CancellationToken);
            })
            .Build();
    }

    /// <summary>
    /// قيد: تحديث حالة الحضور.
    /// [presence] تحمل: "online", "offline", "away", "busy"
    /// </summary>
    public static Operation UpdatePresence(
        IRealtimeTransport transport,
        string userId,
        string status,
        IConnectionTracker? tracker = null)
    {
        return Entry.Create("chat.presence.update")
            .From($"User:{userId}", 0)
            .To("System", 0)
            .Tag(RT.Presence, status)
            .Execute(async ctx =>
            {
                if (tracker != null && status == "offline")
                    await tracker.RemoveConnectionAsync(userId, ctx.CancellationToken);

                await transport.BroadcastAsync("PresenceChanged",
                    new { userId, status, at = DateTime.UtcNow }, ctx.CancellationToken);
            })
            .Build();
    }
}
