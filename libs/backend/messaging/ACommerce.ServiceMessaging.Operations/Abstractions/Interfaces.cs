namespace ACommerce.ServiceMessaging.Operations.Abstractions;

/// <summary>
/// ناقل الرسائل بين الخدمات - لا Kafka. لا Redis. لا RabbitMQ.
/// المزود يُطبق هذه الواجهة.
/// </summary>
public interface IMessageBus
{
    Task PublishAsync(MessageEnvelope envelope, CancellationToken ct = default);
    Task SubscribeAsync(string topic, Func<MessageEnvelope, Task> handler, CancellationToken ct = default);
    Task UnsubscribeAsync(string topic, CancellationToken ct = default);
    Task<MessageEnvelope?> RequestAsync(MessageEnvelope request, TimeSpan? timeout = null, CancellationToken ct = default);
}

/// <summary>
/// مغلف الرسالة - يحمل البيانات + بيانات وصفية
/// </summary>
public class MessageEnvelope
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Topic { get; set; } = default!;
    public string? ReplyTopic { get; set; }
    public Guid? CorrelationId { get; set; }
    public string SourceService { get; set; } = default!;
    public string? TargetService { get; set; }
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public object Payload { get; set; } = default!;
    public string PayloadType { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// أولوية الرسالة بين الخدمات
/// </summary>
public sealed class MessagePriority
{
    public string Value { get; }
    private MessagePriority(string value) => Value = value;

    public static readonly MessagePriority Low = new("low");
    public static readonly MessagePriority Normal = new("normal");
    public static readonly MessagePriority High = new("high");
    public static readonly MessagePriority Critical = new("critical");

    public static MessagePriority Custom(string v) => new(v);
    public override string ToString() => Value;
    public static implicit operator string(MessagePriority mp) => mp.Value;
}

/// <summary>
/// هوية الخدمة
/// </summary>
public sealed class ServiceId
{
    public string Name { get; }
    public ServiceId(string name) => Name = name;
    public override string ToString() => Name;
    public static implicit operator string(ServiceId s) => s.Name;
}

/// <summary>
/// موضوع المراسلة
/// </summary>
public sealed class MessageTopic
{
    public string Value { get; }
    public MessageTopic(string value) => Value = value;

    public static MessageTopic Command(string domain, string action) => new($"{domain}.command.{action}");
    public static MessageTopic Event(string domain, string action) => new($"{domain}.event.{action}");
    public static MessageTopic Query(string domain, string action) => new($"{domain}.query.{action}");
    public static MessageTopic Of(string value) => new(value);

    public override string ToString() => Value;
    public static implicit operator string(MessageTopic t) => t.Value;
}
