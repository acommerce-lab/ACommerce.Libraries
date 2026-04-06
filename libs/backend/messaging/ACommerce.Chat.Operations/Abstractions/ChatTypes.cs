namespace ACommerce.Chat.Operations.Abstractions;

/// <summary>
/// نوع المحادثة
/// </summary>
public sealed class ConversationType
{
    public string Value { get; }
    private ConversationType(string value) => Value = value;

    public static readonly ConversationType Direct = new("direct");
    public static readonly ConversationType Group = new("group");
    public static readonly ConversationType Channel = new("channel");
    public static readonly ConversationType Support = new("support");

    public static ConversationType Custom(string value) => new(value);
    public override string ToString() => Value;
    public static implicit operator string(ConversationType ct) => ct.Value;
}

/// <summary>
/// نوع الرسالة
/// </summary>
public sealed class MessageType
{
    public string Value { get; }
    private MessageType(string value) => Value = value;

    public static readonly MessageType Text = new("text");
    public static readonly MessageType Image = new("image");
    public static readonly MessageType File = new("file");
    public static readonly MessageType Voice = new("voice");
    public static readonly MessageType Video = new("video");
    public static readonly MessageType Location = new("location");
    public static readonly MessageType System = new("system");

    public static MessageType Custom(string value) => new(value);
    public override string ToString() => Value;
    public static implicit operator string(MessageType mt) => mt.Value;
}

/// <summary>
/// دور المشارك
/// </summary>
public sealed class ParticipantRole
{
    public string Value { get; }
    private ParticipantRole(string value) => Value = value;

    public static readonly ParticipantRole Owner = new("owner");
    public static readonly ParticipantRole Admin = new("admin");
    public static readonly ParticipantRole Member = new("member");
    public static readonly ParticipantRole Guest = new("guest");

    public static ParticipantRole Custom(string value) => new(value);
    public override string ToString() => Value;
    public static implicit operator string(ParticipantRole pr) => pr.Value;
}
