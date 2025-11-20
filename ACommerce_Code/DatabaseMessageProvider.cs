using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Events;
using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Chats.Core.Providers;

/// <summary>
/// ???? ??????? ???????? ????? ????????
/// ?????? ?? ??? SharedKernel! ??
/// </summary>
public class DatabaseMessageProvider : IMessageProvider
{
	// ? Repository ?? SharedKernel
	private readonly IBaseAsyncRepository<Message> _messageRepo;
	private readonly IBaseAsyncRepository<MessageRead> _messageReadRepo;
	private readonly IBaseAsyncRepository<ChatParticipant> _participantRepo;

	// ? CQRS ?? SharedKernel
	private readonly IMediator _mediator;

	private readonly ILogger<DatabaseMessageProvider> _logger;

	public DatabaseMessageProvider(
		IBaseAsyncRepository<Message> messageRepo,
		IBaseAsyncRepository<MessageRead> messageReadRepo,
		IBaseAsyncRepository<ChatParticipant> participantRepo,
		IMediator mediator,
		ILogger<DatabaseMessageProvider> logger)
	{
		_messageRepo = messageRepo ?? throw new ArgumentNullException(nameof(messageRepo));
		_messageReadRepo = messageReadRepo ?? throw new ArgumentNullException(nameof(messageReadRepo));
		_participantRepo = participantRepo ?? throw new ArgumentNullException(nameof(participantRepo));
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<MessageDto> SendMessageAsync(
		Guid chatId,
		SendMessageDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending message to chat {ChatId}", chatId);

		var message = new Message
		{
			ChatId = chatId,
			SenderId = dto.SenderId,
			Content = dto.Content,
			Type = dto.Type,
			ReplyToMessageId = dto.ReplyToMessageId,
			Attachments = dto.Attachments ?? new List<string>()
		};

		// ? ??????? SharedKernel Repository
		var created = await _messageRepo.AddAsync(message, cancellationToken);

		// ? ????? Event
		await _mediator.Publish(new MessageSentEvent
		{
			ChatId = chatId,
			MessageId = created.Id,
			SenderId = dto.SenderId,
			SentAt = created.CreatedAt
		}, cancellationToken);

		_logger.LogInformation("Message sent: {MessageId} to chat {ChatId}", created.Id, chatId);

		return MapToMessageDto(created);
	}

	public async Task<PagedResult<MessageDto>> GetMessagesAsync(
		Guid chatId,
		PaginationRequest request,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting messages for chat {ChatId}", chatId);

		// ? ??????? SharedKernel Repository
		var messages = await _messageRepo.GetAllWithPredicateAsync(
			m => m.ChatId == chatId,
			includeDeleted: false);

		var orderedMessages = messages
			.OrderByDescending(m => m.CreatedAt)
			.ToList();

		var skip = (request.PageNumber - 1) * request.PageSize;
		var pagedMessages = orderedMessages
			.Skip(skip)
			.Take(request.PageSize)
			.ToList();

		return new PagedResult<MessageDto>
		{
			Items = pagedMessages.Select(MapToMessageDto).ToList(),
			TotalCount = orderedMessages.Count,
			PageNumber = request.PageNumber,
			PageSize = request.PageSize
		};
	}

	public async Task<MessageDto?> GetMessageAsync(
		Guid messageId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting message {MessageId}", messageId);

		// ? ??????? SharedKernel Repository
		var message = await _messageRepo.GetByIdAsync(messageId, cancellationToken);

		return message != null ? MapToMessageDto(message) : null;
	}

	public async Task<MessageDto> UpdateMessageAsync(
		Guid messageId,
		UpdateMessageDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Updating message {MessageId}", messageId);

		// ? ??????? SharedKernel Repository
		var message = await _messageRepo.GetByIdAsync(messageId, cancellationToken);

		if (message == null)
		{
			throw new InvalidOperationException($"Message {messageId} not found");
		}

		message.Content = dto.Content;
		message.IsEdited = true;
		message.EditedAt = DateTime.UtcNow;

		// ? ??????? SharedKernel Repository
		await _messageRepo.UpdateAsync(message, cancellationToken);

		_logger.LogInformation("Message updated: {MessageId}", messageId);

		return MapToMessageDto(message);
	}

	public async Task DeleteMessageAsync(
		Guid messageId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Deleting message {MessageId}", messageId);

		// ? ??????? SharedKernel Repository (Soft Delete)
		await _messageRepo.DeleteAsync(messageId, cancellationToken);

		_logger.LogInformation("Message deleted: {MessageId}", messageId);
	}

	public async Task MarkAsReadAsync(
		Guid chatId,
		string userId,
		Guid? lastMessageId = null,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Marking messages as read for user {UserId} in chat {ChatId}", userId, chatId);

		// ????? LastSeenMessage ???????
		var participants = await _participantRepo.GetAllWithPredicateAsync(
			p => p.ChatId == chatId && p.UserId == userId,
			includeDeleted: false);

		var participant = participants.FirstOrDefault();

		if (participant != null)
		{
			participant.LastSeenMessageAt = DateTime.UtcNow;
			participant.LastSeenMessageId = lastMessageId;

			// ? ??????? SharedKernel Repository
			await _participantRepo.UpdateAsync(participant, cancellationToken);
		}

		// ????? MessageRead ??? ??? ???? lastMessageId ????
		if (lastMessageId.HasValue)
		{
			var messageRead = new MessageRead
			{
				MessageId = lastMessageId.Value,
				UserId = userId,
				ReadAt = DateTime.UtcNow
			};

			// ? ??????? SharedKernel Repository
			await _messageReadRepo.AddAsync(messageRead, cancellationToken);
		}

		_logger.LogInformation("Messages marked as read for user {UserId}", userId);
	}

	public async Task<PagedResult<MessageDto>> SearchMessagesAsync(
		Guid chatId,
		string searchQuery,
		PaginationRequest request,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Searching messages in chat {ChatId} with query: {Query}", chatId, searchQuery);

		// ? ??????? SharedKernel Repository
		var messages = await _messageRepo.GetAllWithPredicateAsync(
			m => m.ChatId == chatId && m.Content.Contains(searchQuery),
			includeDeleted: false);

		var orderedMessages = messages
			.OrderByDescending(m => m.CreatedAt)
			.ToList();

		var skip = (request.PageNumber - 1) * request.PageSize;
		var pagedMessages = orderedMessages
			.Skip(skip)
			.Take(request.PageSize)
			.ToList();

		return new PagedResult<MessageDto>
		{
			Items = pagedMessages.Select(MapToMessageDto).ToList(),
			TotalCount = orderedMessages.Count,
			PageNumber = request.PageNumber,
			PageSize = request.PageSize
		};
	}

	// ============================================================================
	// Mapping Methods
	// ============================================================================

	private MessageDto MapToMessageDto(Message message)
	{
		return new MessageDto
		{
			Id = message.Id,
			ChatId = message.ChatId,
			SenderId = message.SenderId,
			SenderName = string.Empty, // TODO: ??? ?? User Service
			SenderAvatar = null,
			Content = message.Content,
			Type = message.Type,
			ReplyToMessageId = message.ReplyToMessageId,
			ReplyToMessage = null, // TODO: ??? ??? ???
			Attachments = message.Attachments,
			IsEdited = message.IsEdited,
			EditedAt = message.EditedAt,
			ReadByCount = message.ReadBy?.Count ?? 0,
			CreatedAt = message.CreatedAt
		};
	}
}

