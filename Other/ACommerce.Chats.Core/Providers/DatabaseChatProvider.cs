using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Enums;
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
public class DatabaseChatProvider : IChatProvider
{
	// ? Repository ?? SharedKernel
	private readonly IBaseAsyncRepository<Chat> _chatRepo;
	private readonly IBaseAsyncRepository<ChatParticipant> _participantRepo;

	// ? CQRS ?? SharedKernel
	private readonly IMediator _mediator;

	private readonly ILogger<DatabaseChatProvider> _logger;

	public DatabaseChatProvider(
		IBaseAsyncRepository<Chat> chatRepo,
		IBaseAsyncRepository<ChatParticipant> participantRepo,
		IMediator mediator,
		ILogger<DatabaseChatProvider> logger)
	{
		_chatRepo = chatRepo ?? throw new ArgumentNullException(nameof(chatRepo));
		_participantRepo = participantRepo ?? throw new ArgumentNullException(nameof(participantRepo));
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<ChatDto> CreateChatAsync(
		CreateChatDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Creating new chat: {Title}", dto.Title);

		var chat = new Chat
		{
			Title = dto.Title,
			Type = dto.Type,
			Description = dto.Description,
			ImageUrl = dto.ImageUrl
		};

		// ? ??????? SharedKernel Repository
		var created = await _chatRepo.AddAsync(chat, cancellationToken);

		// ????? ?????????
		foreach (var userId in dto.ParticipantUserIds)
		{
			var participant = new ChatParticipant
			{
				ChatId = created.Id,
				UserId = userId,
				Role = userId == dto.CreatorUserId
					? ParticipantRole.Owner
					: ParticipantRole.Member
			};

			// ? ??????? SharedKernel Repository
			await _participantRepo.AddAsync(participant, cancellationToken);
		}

		// ? ????? Event ???????? MediatR
		await _mediator.Publish(new ChatCreatedEvent
		{
			ChatId = created.Id,
			CreatorUserId = dto.CreatorUserId,
			ParticipantUserIds = dto.ParticipantUserIds,
			CreatedAt = created.CreatedAt
		}, cancellationToken);

		_logger.LogInformation("Chat created successfully: {ChatId}", created.Id);

		return MapToChatDto(created);
	}

	public async Task<ChatDto?> GetChatAsync(
		Guid chatId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting chat: {ChatId}", chatId);

		// ? ??????? SharedKernel Repository
		var chat = await _chatRepo.GetByIdAsync(chatId, cancellationToken);

		return chat != null ? MapToChatDto(chat) : null;
	}

	public async Task<PagedResult<ChatDto>> GetUserChatsAsync(
		string userId,
		PaginationRequest request,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting chats for user: {UserId}", userId);

		// ? ?????? ??? ?????? ???????? ???? ????? ???? ????????
		var participations = await _participantRepo.GetAllWithPredicateAsync(
			p => p.UserId == userId,
			includeDeleted: false);

		var chatIds = participations.Select(p => p.ChatId).ToList();

		if (chatIds.Count == 0)
		{
			return new PagedResult<ChatDto>
			{
				Items = new List<ChatDto>(),
				TotalCount = 0,
				PageNumber = request.PageNumber,
				PageSize = request.PageSize
			};
		}

		// ? ??? ????????
		var chats = await _chatRepo.GetAllWithPredicateAsync(
			c => chatIds.Contains(c.Id),
			includeDeleted: false);

		var orderedChats = chats.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt).ToList();

		var skip = (request.PageNumber - 1) * request.PageSize;
		var pagedChats = orderedChats.Skip(skip).Take(request.PageSize).ToList();

		return new PagedResult<ChatDto>
		{
			Items = pagedChats.Select(MapToChatDto).ToList(),
			TotalCount = orderedChats.Count,
			PageNumber = request.PageNumber,
			PageSize = request.PageSize
		};
	}

	public async Task<ChatDto> UpdateChatAsync(
		Guid chatId,
		UpdateChatDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Updating chat: {ChatId}", chatId);

		// ? ??????? SharedKernel Repository
		var chat = await _chatRepo.GetByIdAsync(chatId, cancellationToken);

		if (chat == null)
		{
			throw new InvalidOperationException($"Chat {chatId} not found");
		}

		if (dto.Title != null) chat.Title = dto.Title;
		if (dto.Description != null) chat.Description = dto.Description;
		if (dto.ImageUrl != null) chat.ImageUrl = dto.ImageUrl;

		// ? ??????? SharedKernel Repository
		await _chatRepo.UpdateAsync(chat, cancellationToken);

		_logger.LogInformation("Chat updated successfully: {ChatId}", chatId);

		return MapToChatDto(chat);
	}

	public async Task DeleteChatAsync(
		Guid chatId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Deleting chat: {ChatId}", chatId);

		// ? ??????? SharedKernel Repository (Soft Delete)
		await _chatRepo.DeleteAsync(chatId, cancellationToken);

		_logger.LogInformation("Chat deleted successfully: {ChatId}", chatId);
	}

	public async Task<ParticipantDto> AddParticipantAsync(
		Guid chatId,
		AddParticipantDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Adding participant {UserId} to chat {ChatId}", dto.UserId, chatId);

		// ?????? ?? ??? ???? ??????? ??????
		var existing = await _participantRepo.GetAllWithPredicateAsync(
			p => p.ChatId == chatId && p.UserId == dto.UserId,
			includeDeleted: false);

		if (existing.Any())
		{
			throw new InvalidOperationException("User is already a participant");
		}

		var participant = new ChatParticipant
		{
			ChatId = chatId,
			UserId = dto.UserId,
			Role = dto.Role
		};

		// ? ??????? SharedKernel Repository
		var created = await _participantRepo.AddAsync(participant, cancellationToken);

		// ? ????? Event
		await _mediator.Publish(new ParticipantJoinedEvent
		{
			ChatId = chatId,
			UserId = dto.UserId,
			JoinedAt = created.CreatedAt
		}, cancellationToken);

		_logger.LogInformation("Participant added: {UserId} to {ChatId}", dto.UserId, chatId);

		return MapToParticipantDto(created);
	}

	public async Task RemoveParticipantAsync(
		Guid chatId,
		string userId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Removing participant {UserId} from chat {ChatId}", userId, chatId);

		var participants = await _participantRepo.GetAllWithPredicateAsync(
			p => p.ChatId == chatId && p.UserId == userId,
			includeDeleted: false);

		var participant = participants.FirstOrDefault();

		if (participant != null)
		{
			// ? ??????? SharedKernel Repository
			await _participantRepo.DeleteAsync(participant.Id, cancellationToken);

			_logger.LogInformation("Participant removed: {UserId} from {ChatId}", userId, chatId);
		}
	}

	public async Task<List<ParticipantDto>> GetParticipantsAsync(
		Guid chatId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting participants for chat: {ChatId}", chatId);

		// ? ??????? SharedKernel Repository
		var participants = await _participantRepo.GetAllWithPredicateAsync(
			p => p.ChatId == chatId,
			includeDeleted: false);

		return participants.Select(MapToParticipantDto).ToList();
	}

	// ============================================================================
	// Mapping Methods
	// ============================================================================

	private ChatDto MapToChatDto(Chat chat)
	{
		return new ChatDto
		{
			Id = chat.Id,
			Title = chat.Title,
			Type = chat.Type,
			Description = chat.Description,
			ImageUrl = chat.ImageUrl,
			ParticipantsCount = chat.Participants?.Count ?? 0,
			UnreadMessagesCount = 0, // TODO: ???? ??? ??????? ??? ????????
			LastMessage = null, // TODO: ??? ??? ?????
			CreatedAt = chat.CreatedAt,
			UpdatedAt = chat.UpdatedAt
		};
	}

	private ParticipantDto MapToParticipantDto(ChatParticipant participant)
	{
		return new ParticipantDto
		{
			Id = participant.Id,
			ChatId = participant.ChatId,
			UserId = participant.UserId,
			UserName = string.Empty, // TODO: ??? ?? User Service
			UserAvatar = null,
			Role = participant.Role,
			IsOnline = false, // TODO: ?? Presence Service
			LastSeenAt = participant.LastSeenMessageAt,
			LastSeenMessageId = participant.LastSeenMessageId,
			UnreadMessagesCount = 0, // TODO: ????
			IsMuted = participant.IsMuted,
			IsPinned = participant.IsPinned,
			JoinedAt = participant.CreatedAt
		};
	}
}

