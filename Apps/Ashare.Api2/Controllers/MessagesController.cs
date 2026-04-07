using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.Realtime.Operations.Abstractions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api2.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IBaseAsyncRepository<Conversation> _convs;
    private readonly IBaseAsyncRepository<Message> _msgs;
    private readonly IBaseAsyncRepository<Listing> _listings;
    private readonly IRealtimeTransport _transport;
    private readonly OpEngine _engine;

    public MessagesController(
        IRepositoryFactory factory,
        IRealtimeTransport transport,
        OpEngine engine)
    {
        _convs = factory.CreateRepository<Conversation>();
        _msgs = factory.CreateRepository<Message>();
        _listings = factory.CreateRepository<Listing>();
        _transport = transport;
        _engine = engine;
    }

    public record StartConversationRequest(Guid ListingId, Guid CustomerId);

    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest req, CancellationToken ct)
    {
        var listing = await _listings.GetByIdAsync(req.ListingId, ct);
        if (listing == null) return NotFound(new { error = "listing_not_found" });

        if (!listing.IsMessagingAllowed)
            return BadRequest(new { error = "messaging_not_allowed_for_listing" });

        // البحث عن محادثة موجودة بين هذين الطرفين على هذا العرض
        var existing = await _convs.GetAllWithPredicateAsync(c =>
            c.ListingId == req.ListingId &&
            c.CustomerId == req.CustomerId &&
            c.OwnerId == listing.OwnerId);

        if (existing.Count > 0)
            return Ok(existing.First());

        var conv = new Conversation
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ListingId = req.ListingId,
            CustomerId = req.CustomerId,
            OwnerId = listing.OwnerId
        };
        await _convs.AddAsync(conv, ct);
        return CreatedAtAction(nameof(GetConversation), new { id = conv.Id }, conv);
    }

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> GetConversation(Guid id, CancellationToken ct)
    {
        var c = await _convs.GetByIdAsync(id, ct);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("conversations/by-user/{userId:guid}")]
    public async Task<IActionResult> ListByUser(Guid userId, CancellationToken ct)
    {
        var list = await _convs.GetAllWithPredicateAsync(c =>
            c.CustomerId == userId || c.OwnerId == userId);
        return Ok(list.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt));
    }

    public record SendMessageRequest(Guid ConversationId, Guid SenderId, string Content, string? MessageType);

    /// <summary>
    /// إرسال رسالة - قيد محاسبي:
    /// المُرسل (مدين) ← المُستقبل (دائن) برسالة، ثم بث عبر الزمن الحقيقي.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest req, CancellationToken ct)
    {
        var conv = await _convs.GetByIdAsync(req.ConversationId, ct);
        if (conv == null) return NotFound(new { error = "conversation_not_found" });

        if (req.SenderId != conv.CustomerId && req.SenderId != conv.OwnerId)
            return Forbid();

        var recipient = req.SenderId == conv.CustomerId ? conv.OwnerId : conv.CustomerId;

        var msg = new Message
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ConversationId = conv.Id,
            SenderId = req.SenderId,
            Content = req.Content,
            MessageType = req.MessageType ?? "text"
        };

        var op = Entry.Create("chat.send")
            .Describe($"Message from User:{req.SenderId} to User:{recipient}")
            .From($"User:{req.SenderId}", 1, ("role", "sender"))
            .To($"User:{recipient}", 1, ("role", "recipient"), ("delivery", "pending"))
            .Tag("conversation_id", conv.Id.ToString())
            .Tag("message_type", msg.MessageType)
            .Validate(ctx =>
            {
                if (string.IsNullOrWhiteSpace(req.Content))
                {
                    ctx.AddValidationError("content", "empty_content");
                    return false;
                }
                return true;
            })
            .Execute(async ctx =>
            {
                // حفظ الرسالة وتحديث المحادثة
                await _msgs.AddAsync(msg, ctx.CancellationToken);

                conv.LastMessageSnippet = req.Content.Length > 80 ? req.Content[..80] + "..." : req.Content;
                conv.LastMessageAt = DateTime.UtcNow;
                if (req.SenderId == conv.CustomerId) conv.UnreadOwnerCount++;
                else conv.UnreadCustomerCount++;
                await _convs.UpdateAsync(conv, ctx.CancellationToken);

                // بث عبر الزمن الحقيقي للمستقبل
                await _transport.SendToUserAsync(
                    recipient.ToString(),
                    "MessageReceived",
                    new { conversationId = conv.Id, message = msg },
                    ctx.CancellationToken);

                ctx.Set("messageId", msg.Id);
            })
            .Build();

        var result = await _engine.ExecuteAsync(op, ct);

        if (!result.Success)
            return BadRequest(new
            {
                error = "send_failed",
                opStatus = (result.Success ? "Success" : (result.IsPartial ? "Partial" : "Failed"))
            });

        return CreatedAtAction(nameof(GetMessage), new { id = msg.Id }, new { message = msg, opId = op.Id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMessage(Guid id, CancellationToken ct)
    {
        var m = await _msgs.GetByIdAsync(id, ct);
        return m == null ? NotFound() : Ok(m);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> ListInConversation(Guid conversationId, CancellationToken ct)
    {
        var list = await _msgs.GetAllWithPredicateAsync(m => m.ConversationId == conversationId);
        return Ok(list.OrderBy(m => m.CreatedAt));
    }
}
