using ACommerce.Chat.Operations.Operations;
using ACommerce.OperationEngine.Core;
using ACommerce.Realtime.Operations.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Chat.Operations;

/// <summary>
/// واجهة الدردشة البسيطة.
///
/// services.AddChat&lt;SignalRTransport&gt;();
///
/// ثم:
///   await chat.SendMessageAsync("ahmed", "chat_123", "مرحبا");
///   await chat.MarkAsReadAsync("sara", "ahmed", "chat_123");
/// </summary>
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

    public async Task<OperationResult> SendMessageAsync(
        string senderId, string conversationId, string content,
        string messageType = "text", string? replyToId = null,
        string[]? recipientIds = null,
        Func<OperationContext, Task>? afterExecute = null,
        CancellationToken ct = default)
    {
        var op = ChatOps.SendMessage(_transport, senderId, conversationId, content,
            messageType, replyToId, recipientIds);

        if (afterExecute != null)
            op.Hooks.AfterExecute = afterExecute;

        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> AcknowledgeDeliveryAsync(
        string recipientId, string senderId, string conversationId,
        Guid? originalOpId = null, CancellationToken ct = default)
    {
        var op = ChatOps.AcknowledgeDelivery(_transport, recipientId, senderId, conversationId, originalOpId);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> MarkAsReadAsync(
        string readerId, string senderId, string conversationId,
        Guid? originalOpId = null, CancellationToken ct = default)
    {
        var op = ChatOps.MarkAsRead(_transport, readerId, senderId, conversationId, originalOpId);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> SendTypingAsync(
        string userId, string conversationId, bool isTyping, CancellationToken ct = default)
    {
        var op = ChatOps.TypingIndicator(_transport, userId, conversationId, isTyping);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> CreateConversationAsync(
        string creatorId, string type, string? title = null,
        string[]? participantIds = null,
        Func<OperationContext, Task>? afterExecute = null,
        CancellationToken ct = default)
    {
        var op = ChatOps.CreateConversation(creatorId, type, title, participantIds);
        if (afterExecute != null)
            op.Hooks.AfterExecute = afterExecute;
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> JoinConversationAsync(
        string userId, string connectionId, string conversationId,
        string role = "member", CancellationToken ct = default)
    {
        var op = ChatOps.JoinConversation(_transport, userId, connectionId, conversationId, role);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> UpdatePresenceAsync(
        string userId, string status, CancellationToken ct = default)
    {
        var op = ChatOps.UpdatePresence(_transport, userId, status, _tracker);
        return await _engine.ExecuteAsync(op, ct);
    }
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
        // يفترض أن IRealtimeTransport مُسجل مسبقاً (من AddRealtime)
        services.AddScoped<ChatService>();
        return services;
    }
}
