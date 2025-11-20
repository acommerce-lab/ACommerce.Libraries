namespace ACommerce.Messaging.Abstractions.Models;

/// <summary>
/// Standard topic naming conventions
/// </summary>
public static class TopicNames
{
    // ========================================
    // Events (Fire-and-Forget)
    // ========================================
    public const string UserAuthenticated = "auth.events.user.authenticated";
    public const string UserRegistered = "auth.events.user.registered";
    public const string ChatMessageSent = "chat.events.message.sent";
    public const string NotificationSent = "notify.events.notification.sent";

    // ========================================
    // Queries (Request-Response)
    // ========================================
    public const string GetUserById = "auth.queries.user.get";
    public const string GetChatHistory = "chat.queries.history.get";

    // ========================================
    // Commands (Action Request)
    // ========================================
    public const string SendNotification = "notify.commands.send";
    public const string CreateChatRoom = "chat.commands.room.create";

    // ========================================
    // Helper Methods
    // ========================================

    /// <summary>
    /// Build event topic: {service}.events.{entity}.{action}
    /// </summary>
    public static string Event(string service, string entity, string action)
        => $"{service}.events.{entity}.{action}";

    /// <summary>
    /// Build query topic: {service}.queries.{entity}.{action}
    /// </summary>
    public static string Query(string service, string entity, string action)
        => $"{service}.queries.{entity}.{action}";

    /// <summary>
    /// Build command topic: {service}.commands.{action}
    /// </summary>
    public static string Command(string service, string action)
        => $"{service}.commands.{action}";

    /// <summary>
    /// Build reply topic: {service}.replies.{correlationId}
    /// </summary>
    public static string Reply(string service, string correlationId)
        => $"{service}.replies.{correlationId}";
}
