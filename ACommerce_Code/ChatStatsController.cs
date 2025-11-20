using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Chats.Api.Controllers;

/// <summary>
/// ???????? ????????
/// </summary>
[Authorize]
[ApiController]
[Route("api/chats/stats")]
[Produces("application/json")]
public class ChatStatsController : ControllerBase
{
	private readonly IBaseAsyncRepository<Chat> _chatRepo;
	private readonly IBaseAsyncRepository<Message> _messageRepo;
	private readonly IBaseAsyncRepository<ChatParticipant> _participantRepo;
	private readonly IMediator _mediator;
	private readonly ILogger<ChatStatsController> _logger;

	public ChatStatsController(
		IBaseAsyncRepository<Chat> chatRepo,
		IBaseAsyncRepository<Message> messageRepo,
		IBaseAsyncRepository<ChatParticipant> participantRepo,
		IMediator mediator,
		ILogger<ChatStatsController> logger)
	{
		_chatRepo = chatRepo;
		_messageRepo = messageRepo;
		_participantRepo = participantRepo;
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// ???????? ????????
	/// GET /api/chats/stats/my-stats
	/// </summary>
	[HttpGet("my-stats")]
	[ProducesResponseType(typeof(UserChatStats), 200)]
	[ProducesResponseType(401)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<UserChatStats>> GetMyStats()
	{
		try
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Getting stats for user {UserId}", userId);

			// ??? ????????
			var participations = await _participantRepo.GetAllWithPredicateAsync(
				p => p.UserId == userId,
				includeDeleted: false);

			var chatIds = participations.Select(p => p.ChatId).ToList();

			// ??? ??????? ???????
			var sentMessages = await _messageRepo.GetAllWithPredicateAsync(
				m => m.SenderId == userId,
				includeDeleted: false);

			// ??? ??????? ??? ????????
			var unreadCount = 0; // TODO: ???? ??????? ??? ????????

			var stats = new UserChatStats
			{
				TotalChats = chatIds.Count,
				TotalMessagesSent = sentMessages.Count,
				UnreadMessagesCount = unreadCount
			};

			return Ok(stats);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user stats");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ???????? ?????
	/// GET /api/chats/stats/{chatId}
	/// </summary>
	[HttpGet("{chatId}")]
	[ProducesResponseType(typeof(ChatStats), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ChatStats>> GetChatStats(Guid chatId)
	{
		try
		{
			_logger.LogDebug("Getting stats for chat {ChatId}", chatId);

			var chat = await _chatRepo.GetByIdAsync(chatId);
			if (chat == null)
			{
				return NotFound(new { message = "Chat not found" });
			}

			var participants = await _participantRepo.GetAllWithPredicateAsync(
				p => p.ChatId == chatId,
				includeDeleted: false);

			var messages = await _messageRepo.GetAllWithPredicateAsync(
				m => m.ChatId == chatId,
				includeDeleted: false);

			var stats = new ChatStats
			{
				ChatId = chatId,
				ParticipantsCount = participants.Count,
				TotalMessages = messages.Count,
				CreatedAt = chat.CreatedAt
			};

			return Ok(stats);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting chat stats for {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs
public class UserChatStats
{
	public int TotalChats { get; set; }
	public int TotalMessagesSent { get; set; }
	public int UnreadMessagesCount { get; set; }
}

public class ChatStats
{
	public Guid ChatId { get; set; }
	public int ParticipantsCount { get; set; }
	public int TotalMessages { get; set; }
	public DateTime CreatedAt { get; set; }
}

