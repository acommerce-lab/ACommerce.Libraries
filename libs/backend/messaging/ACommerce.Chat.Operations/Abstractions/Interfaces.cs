namespace ACommerce.Chat.Operations.Abstractions;

/// <summary>
/// واجهة الرسالة - لا كيان!
/// المطور يُنشئ كيانه بالأعمدة التي يريدها ويُطبق هذه الواجهة.
///
/// مثال:
///   public class MyMessage : IBaseEntity, IMessage {
///       public Guid Id { get; set; }
///       public string SenderId { get; set; }
///       public string Content { get; set; }
///       public string MessageType { get; set; }
///       public DateTime SentAt { get; set; }
///       // + أعمدة إضافية يحتاجها المطور
///       public string CustomField1 { get; set; }
///   }
/// </summary>
public interface IMessage
{
    Guid Id { get; }
    string SenderId { get; }
    string Content { get; }
    string MessageType { get; }
    DateTime SentAt { get; }
}

/// <summary>
/// واجهة المحادثة.
/// </summary>
public interface IConversation
{
    Guid Id { get; }
    string ConversationType { get; }  // "direct", "group", "channel", "support"
    string? Title { get; }
}

/// <summary>
/// واجهة المشارك.
/// </summary>
public interface IParticipant
{
    string UserId { get; }
    string Role { get; }  // "owner", "admin", "member", "guest"
    Guid ConversationId { get; }
}

/// <summary>
/// مفاتيح العلامات الخاصة بالدردشة.
/// المفاتيح ثابتة، القيم من التطبيق.
/// </summary>
public static class ChatTags
{
    /// <summary>
    /// معرف المحادثة. القيمة: Guid كنص
    /// </summary>
    public const string Conversation = "conversation";

    /// <summary>
    /// نوع المحادثة. القيم: "direct", "group", "channel", "support"
    /// </summary>
    public const string ConversationType = "conversation_type";

    /// <summary>
    /// نوع الرسالة. القيم: "text", "image", "file", "voice", "video", "location"
    /// </summary>
    public const string MessageType = "message_type";

    /// <summary>
    /// معرف الرسالة المُرد عليها. القيمة: Guid كنص
    /// </summary>
    public const string ReplyTo = "reply_to";

    /// <summary>
    /// حالة الكتابة. القيم: "typing", "stopped"
    /// </summary>
    public const string Typing = "typing";
}
