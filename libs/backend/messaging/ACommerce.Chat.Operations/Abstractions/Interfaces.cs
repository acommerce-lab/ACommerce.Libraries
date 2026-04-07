using ACommerce.OperationEngine.Core;
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
    public static readonly TagKey Conversation = new("conversation");

    /// <summary>
    /// نوع المحادثة. القيم: "direct", "group", "channel", "support"
    /// </summary>
    public static readonly TagKey ConversationType = new("conversation_type");

    /// <summary>
    /// نوع الرسالة. القيم: "text", "image", "file", "voice", "video", "location"
    /// </summary>
    public static readonly TagKey MessageType = new("message_type");

    /// <summary>
    /// معرف الرسالة المُرد عليها. القيمة: Guid كنص
    /// </summary>
    public static readonly TagKey ReplyTo = new("reply_to");

    /// <summary>
    /// حالة الكتابة. القيم: "typing", "stopped"
    /// </summary>
    public static readonly TagKey Typing = new("typing");
}
