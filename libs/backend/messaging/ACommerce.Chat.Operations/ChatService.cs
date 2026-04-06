using ACommerce.Chat.Operations.Abstractions;
using ACommerce.Chat.Operations.Operations;
using ACommerce.OperationEngine.Core;
using ACommerce.Realtime.Operations.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Chat.Operations;

public class ChatService
{
    private readonly IRealtimeTransport _transport;
    private readonly IConnectionTracker? _tracker;
    private readonly OpEngine _engine;

    public ChatService(IRealtimeTransport transport, OpEngine engine, IConnectionTracker? tracker = null)
    {
        _transport = transport;
        _engine = engine;
        _tracker = tracker;
    }

    public Task<OperationResult> SendMessageAsync(
        PartyId sender, PartyId conversation, string content,
        MessageType? messageType = null, Guid? replyToId = null,
        PartyId[]? recipients = null,
        Func<OperationContext, Task>? afterExecute = null,
        CancellationToken ct = default)
    {
        var op = ChatOps.SendMessage(_transport, sender, conversation, content, messageType, replyToId, recipients);
        if (afterExecute != null) op.Hooks.AfterExecute = afterExecute;
        return _engine.ExecuteAsync(op, ct);
    }

    public Task<OperationResult> AcknowledgeDeliveryAsync(
        PartyId recipient, PartyId sender, PartyId conversation,
        Guid? originalOpId = null, CancellationToken ct = default)
        => _engine.ExecuteAsync(ChatOps.AcknowledgeDelivery(_transport, recipient, sender, conversation, originalOpId), ct);

    public Task<OperationResult> MarkAsReadAsync(
        PartyId reader, PartyId sender, PartyId conversation,
        Guid? originalOpId = null, CancellationToken ct = default)
        => _engine.ExecuteAsync(ChatOps.MarkAsRead(_transport, reader, sender, conversation, originalOpId), ct);

    public Task<OperationResult> SendTypingAsync(
        PartyId user, PartyId conversation, bool isTyping, CancellationToken ct = default)
        => _engine.ExecuteAsync(ChatOps.TypingIndicator(_transport, user, conversation, isTyping), ct);

    public Task<OperationResult> CreateConversationAsync(
        PartyId creator, ConversationType type, string? title = null,
        PartyId[]? participants = null,
        Func<OperationContext, Task>? afterExecute = null,
        CancellationToken ct = default)
    {
        var op = ChatOps.CreateConversation(creator, type, title, participants);
        if (afterExecute != null) op.Hooks.AfterExecute = afterExecute;
        return _engine.ExecuteAsync(op, ct);
    }

    public Task<OperationResult> JoinConversationAsync(
        PartyId user, string connectionId, PartyId conversation,
        ParticipantRole? role = null, CancellationToken ct = default)
        => _engine.ExecuteAsync(ChatOps.JoinConversation(_transport, user, connectionId, conversation, role), ct);

    public Task<OperationResult> UpdatePresenceAsync(
        PartyId user, PresenceStatus status, CancellationToken ct = default)
        => _engine.ExecuteAsync(ChatOps.UpdatePresence(_transport, user, status, _tracker), ct);
}

public static class ChatExtensions
{
    public static IServiceCollection AddChat<TTransport>(this IServiceCollection services)
        where TTransport : class, IRealtimeTransport
    {
        services.AddScoped<IRealtimeTransport, TTransport>();
        services.AddScoped<ChatService>();
        return services;
    }

    public static IServiceCollection AddChat(this IServiceCollection services)
    {
        services.AddScoped<ChatService>();
        return services;
    }
}
