using ACommerce.Chat.Operations.Abstractions;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.Realtime.Operations.Abstractions;

namespace ACommerce.Chat.Operations.Operations;

/// <summary>
/// قيود الدردشة - كل متغير مكتوب. لا نصوص حرة.
/// </summary>
public static class ChatOps
{
    public static Operation SendMessage(
        IRealtimeTransport transport,
        PartyId sender,
        PartyId conversation,
        string content,
        MessageType? messageType = null,
        Guid? replyToId = null,
        PartyId[]? recipients = null)
    {
        var msgType = messageType ?? MessageType.Text;

        var builder = Entry.Create("chat.message.send")
            .Describe($"{sender} sends {msgType} to {conversation}")
            .From(sender, 1, (RT.Role, "sender"))
            .To(conversation, 1, (ChatTags.Conversation, conversation.Id))
            .Tag(ChatTags.MessageType, msgType)
            .Tag(ChatTags.Conversation, conversation.Id);

        if (replyToId != null)
            builder.Tag(ChatTags.ReplyTo, replyToId.Value.ToString());

        builder.Execute(async ctx =>
        {
            var payload = new
            {
                senderId = sender.Id,
                conversationId = conversation.Id,
                content,
                messageType = msgType.Value,
                replyToId,
                sentAt = DateTime.UtcNow
            };
            await transport.SendToGroupAsync(
                $"chat_{conversation.Id}", "MessageReceived", payload, ctx.CancellationToken);
            ctx.Set("content", content);
            ctx.Set("sentAt", DateTime.UtcNow);
            ctx.Set("payload", payload);
        });

        if (recipients != null)
        {
            builder.WithSub("chat.delivery", sub =>
            {
                foreach (var r in recipients)
                    sub.Party(r, 1, ("direction", "credit"),
                        (RT.Delivery, DeliveryStatus.Pending), (RT.Role, "recipient"));
                sub.Party(PartyId.System, recipients.Length, ("direction", "debit"));
            });
        }

        return builder.Build();
    }

    public static Operation AcknowledgeDelivery(
        IRealtimeTransport transport,
        PartyId recipient,
        PartyId sender,
        PartyId conversation,
        Guid? originalOpId = null)
    {
        var builder = Entry.Create("chat.delivery.ack")
            .Describe($"{recipient} ack delivery")
            .From(recipient, 1, (RT.Delivery, DeliveryStatus.Delivered))
            .To(sender, 1, (RT.Role, "sender"))
            .Tag(ChatTags.Conversation, conversation.Id);

        if (originalOpId != null)
            builder.PartiallyFulfills(originalOpId.Value);

        builder.Execute(async ctx =>
        {
            await transport.SendToUserAsync(sender.Id, "DeliveryAck",
                new { recipientId = recipient.Id, conversationId = conversation.Id },
                ctx.CancellationToken);
        });

        return builder.Build();
    }

    public static Operation MarkAsRead(
        IRealtimeTransport transport,
        PartyId reader,
        PartyId sender,
        PartyId conversation,
        Guid? originalOpId = null)
    {
        var builder = Entry.Create("chat.delivery.read")
            .Describe($"{reader} read in {conversation}")
            .From(reader, 1, (RT.Delivery, DeliveryStatus.Read))
            .To(sender, 1, (RT.Role, "sender"))
            .Tag(ChatTags.Conversation, conversation.Id);

        if (originalOpId != null)
            builder.Fulfills(originalOpId.Value);

        builder.Execute(async ctx =>
        {
            await transport.SendToUserAsync(sender.Id, "ReadReceipt",
                new { readerId = reader.Id, conversationId = conversation.Id, readAt = DateTime.UtcNow },
                ctx.CancellationToken);
        });

        return builder.Build();
    }

    public static Operation TypingIndicator(
        IRealtimeTransport transport,
        PartyId user,
        PartyId conversation,
        bool isTyping)
    {
        return Entry.Create("chat.typing")
            .From(user, 0)
            .To(conversation, 0)
            .Tag(ChatTags.Typing, isTyping ? "typing" : "stopped")
            .Tag(ChatTags.Conversation, conversation.Id)
            .Execute(async ctx =>
            {
                await transport.SendToGroupAsync($"chat_{conversation.Id}", "UserTyping",
                    new { userId = user.Id, isTyping }, ctx.CancellationToken);
            })
            .Build();
    }

    public static Operation CreateConversation(
        PartyId creator,
        ConversationType type,
        string? title = null,
        PartyId[]? participants = null)
    {
        var builder = Entry.Create("chat.conversation.create")
            .Describe($"{creator} creates {type} conversation")
            .From(creator, 1, (RT.Role, "creator"))
            .To(PartyId.System, 1)
            .Tag(ChatTags.ConversationType, type);

        if (title != null)
            builder.Meta("title", title);

        if (participants != null)
        {
            foreach (var p in participants)
            {
                builder.WithSub($"chat.participant.add", sub =>
                {
                    sub.Party(p, 1, ("direction", "credit"), (RT.Role, ParticipantRole.Member));
                    sub.Party(PartyId.System, 1, ("direction", "debit"));
                });
            }
        }

        return builder.Build();
    }

    public static Operation JoinConversation(
        IRealtimeTransport transport,
        PartyId user,
        string connectionId,
        PartyId conversation,
        ParticipantRole? role = null)
    {
        var r = role ?? ParticipantRole.Member;
        return Entry.Create("chat.conversation.join")
            .From(user, 1, (RT.Role, r))
            .To(conversation, 1, (ChatTags.Conversation, conversation.Id))
            .Execute(async ctx =>
            {
                await transport.AddToGroupAsync(connectionId, $"chat_{conversation.Id}", ctx.CancellationToken);
                await transport.SendToGroupAsync($"chat_{conversation.Id}", "ParticipantJoined",
                    new { userId = user.Id, role = r.Value }, ctx.CancellationToken);
            })
            .Build();
    }

    public static Operation UpdatePresence(
        IRealtimeTransport transport,
        PartyId user,
        PresenceStatus status,
        IConnectionTracker? tracker = null)
    {
        return Entry.Create("chat.presence.update")
            .From(user, 0)
            .To(PartyId.System, 0)
            .Tag(RT.Presence, status)
            .Execute(async ctx =>
            {
                if (tracker != null && status == PresenceStatus.Offline)
                    await tracker.RemoveConnectionAsync(user.Id, ctx.CancellationToken);
                await transport.BroadcastAsync("PresenceChanged",
                    new { userId = user.Id, status = status.Value, at = DateTime.UtcNow },
                    ctx.CancellationToken);
            })
            .Build();
    }
}
