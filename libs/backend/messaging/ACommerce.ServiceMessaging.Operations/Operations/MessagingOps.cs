using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.Realtime.Operations.Abstractions;
using ACommerce.ServiceMessaging.Operations.Abstractions;

namespace ACommerce.ServiceMessaging.Operations.Operations;

/// <summary>
/// قيود المراسلات بين الخدمات.
/// </summary>
public static class MessagingOps
{
    public static Operation Publish(
        IMessageBus bus,
        ServiceId source,
        MessageTopic topic,
        object payload,
        MessagePriority? priority = null,
        Guid? correlationId = null)
    {
        var p = priority ?? MessagePriority.Normal;
        return Entry.Create("messaging.publish")
            .Describe($"{source} publishes to {topic}")
            .From(PartyId.Of("Service", source), 1, (RT.Role, "publisher"))
            .To(PartyId.Topic(topic), 1, (RT.Role, "topic"))
            .Tag("topic", topic)
            .Tag("priority", p)
            .Execute(async ctx =>
            {
                var envelope = new MessageEnvelope
                {
                    Topic = topic,
                    SourceService = source,
                    Payload = payload,
                    PayloadType = payload.GetType().Name,
                    Priority = p,
                    CorrelationId = correlationId
                };
                await bus.PublishAsync(envelope, ctx.CancellationToken);
                ctx.Set("envelope", envelope);
            })
            .Build();
    }

    public static Operation Subscribe(
        IMessageBus bus,
        ServiceId subscriber,
        MessageTopic topic,
        Func<MessageEnvelope, Task> handler)
    {
        return Entry.Create("messaging.subscribe")
            .From(PartyId.Of("Service", subscriber), 1)
            .To(PartyId.Topic(topic), 1)
            .Tag("topic", topic)
            .Execute(async ctx =>
            {
                await bus.SubscribeAsync(topic, handler, ctx.CancellationToken);
                ctx.Set("subscribed", true);
            })
            .Build();
    }

    public static Operation RequestReply(
        IMessageBus bus,
        ServiceId requester,
        ServiceId responder,
        MessageTopic topic,
        object request,
        TimeSpan? timeout = null)
    {
        var corrId = Guid.NewGuid();
        return Entry.Create("messaging.request")
            .Describe($"{requester} requests {responder} via {topic}")
            .From(PartyId.Of("Service", requester), 1, (RT.Role, "requester"))
            .To(PartyId.Of("Service", responder), 1, (RT.Role, "responder"))
            .Tag("topic", topic)
            .Tag("correlation", corrId.ToString())
            .Execute(async ctx =>
            {
                var envelope = new MessageEnvelope
                {
                    Topic = topic,
                    SourceService = requester,
                    TargetService = responder,
                    Payload = request,
                    PayloadType = request.GetType().Name,
                    CorrelationId = corrId,
                    ReplyTopic = $"{topic}.reply"
                };
                var response = await bus.RequestAsync(envelope, timeout, ctx.CancellationToken);
                ctx.Set("response", response);
                ctx.Set("correlationId", corrId);
            })
            .Build();
    }
}
