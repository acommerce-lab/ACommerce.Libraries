using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Enums;
using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Core.Entities;
using ACommerce.Marketing.Analytics.Services;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Chats.Api.Controllers;

/// <summary>
/// متحكم المحادثات (Chats)
/// </summary>
[Authorize]
[ApiController]
[Route("api/chats")]
[Produces("application/json")]
public class ChatsController : BaseCrudController<
	Chat,
	CreateChatDto,
	UpdateChatDto,
	ChatDto,
	PartialUpdateChatDto>
{
	private readonly IChatProvider _chatProvider;
	private readonly IMessageProvider _messageProvider;
	private readonly IMarketingEventTracker? _marketingTracker;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly INotificationService? _notificationService;

	public ChatsController(
		IMediator mediator,
		IChatProvider chatProvider,
		IMessageProvider messageProvider,
		ILogger<ChatsController> logger,
		IHttpContextAccessor httpContextAccessor,
		IMarketingEventTracker? marketingTracker = null,
		INotificationService? notificationService = null)
		: base(mediator, logger)
	{
		_chatProvider = chatProvider ?? throw new ArgumentNullException(nameof(chatProvider));
		_messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
		_httpContextAccessor = httpContextAccessor;
		_marketingTracker = marketingTracker;
		_notificationService = notificationService;
	}

	// ? ?? ??? CRUD operations ?????? ?? BaseCrudController!
	// GET /api/chats
	// POST /api/chats
	// GET /api/chats/{id} - تم تجاوزه لإضافة بيانات المشاركين
	// PUT /api/chats/{id}
	// PATCH /api/chats/{id}
	// DELETE /api/chats/{id}

	/// <summary>
	/// الحصول على تفاصيل محادثة محددة مع بيانات المشاركين
	/// GET /api/chats/{id}
	/// </summary>
	[ProducesResponseType(typeof(ChatDetailDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public override async Task<ActionResult<ChatDto>> GetById(
		Guid id,
		[FromQuery] List<string>? includes = null,
		[FromQuery] bool includeDeleted = false)
	{
		try
		{
			var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			_logger.LogDebug("Getting chat {ChatId} for user {UserId}", id, currentUserId);

			// جلب بيانات المحادثة
			var chat = await _chatProvider.GetChatAsync(id);
			if (chat == null)
			{
				return NotFound(new { message = "Chat not found" });
			}

			// جلب المشاركين
			var participants = await _chatProvider.GetParticipantsAsync(id);

			// إيجاد الطرف الآخر (في المحادثات المباشرة)
			var otherParticipant = participants.FirstOrDefault(p => p.UserId != currentUserId);

			var result = new ChatDetailDto
			{
				Id = chat.Id,
				Title = chat.Title,
				Type = chat.Type.ToString(),
				Description = chat.Description,
				ImageUrl = chat.ImageUrl,
				ParticipantsCount = chat.ParticipantsCount,
				UnreadMessagesCount = chat.UnreadMessagesCount,
				CreatedAt = chat.CreatedAt,
				UpdatedAt = chat.UpdatedAt,
				// بيانات الطرف الآخر (للمحادثات المباشرة)
				OtherPartyId = otherParticipant?.UserId,
				OtherPartyName = otherParticipant?.UserId ?? "مستخدم", // TODO: جلب الاسم من خدمة المستخدمين
				OtherPartyAvatar = null, // TODO: جلب الصورة من خدمة المستخدمين
				IsOnline = false, // TODO: التحقق من حالة الاتصال
				Participants = participants.Select(p => p.UserId).ToList()
			};

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting chat {ChatId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ???????? ??????
	/// GET /api/chats/my-chats
	/// </summary>
	[HttpGet("my-chats")]
	[ProducesResponseType(typeof(PagedResult<ChatDto>), 200)]
	[ProducesResponseType(401)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ChatDto>>> GetMyChats(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Getting chats for user {UserId}", userId);

			var request = new PaginationRequest
			{
				PageNumber = pageNumber,
				PageSize = pageSize
			};

			var chats = await _chatProvider.GetUserChatsAsync(userId, request);

			return Ok(chats);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user chats");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ?? ?????
	/// POST /api/chats/{chatId}/messages
	/// </summary>
	[HttpPost("{chatId}/messages")]
	[ProducesResponseType(typeof(MessageDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(401)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<MessageDto>> SendMessage(
		Guid chatId,
		[FromBody] SendMessageRequest request)
	{
		try
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Sending message to chat {ChatId} by user {UserId}", chatId, userId);

			// تحويل نوع الرسالة من string إلى enum
			var messageType = MessageType.Text;
			if (!string.IsNullOrEmpty(request.Type))
			{
				Enum.TryParse<MessageType>(request.Type, ignoreCase: true, out messageType);
			}

			var dto = new SendMessageDto
			{
				SenderId = userId,
				Content = request.Content,
				Type = messageType,
				ReplyToMessageId = request.ReplyToMessageId,
				Attachments = request.Attachments
			};

			var message = await _messageProvider.SendMessageAsync(chatId, dto);

			// إرسال إشعارات للمشاركين الآخرين في المحادثة
			if (_notificationService != null)
			{
				try
				{
					var participants = await _chatProvider.GetParticipantsAsync(chatId);
					var otherParticipants = participants.Where(p => p.UserId != userId).ToList();

					foreach (var participant in otherParticipants)
					{
						await _notificationService.SendAsync(new Notification
						{
							Id = Guid.NewGuid(),
							UserId = participant.UserId,
							Title = "رسالة جديدة 💬",
							Message = request.Content.Length > 100 ? request.Content[..100] + "..." : request.Content,
							Type = NotificationType.ChatMessage,
							Priority = NotificationPriority.High,
							CreatedAt = DateTimeOffset.UtcNow,
							ActionUrl = $"/chat/{chatId}",
							Sound = "default",
							BadgeCount = 1,
							Channels = new List<ChannelDelivery>
							{
								new() { Channel = NotificationChannel.InApp },
								new() { Channel = NotificationChannel.Firebase }
							},
							Data = new Dictionary<string, string>
							{
								["type"] = "new_message",
								["chatId"] = chatId.ToString(),
								["messageId"] = message.Id.ToString(),
								["senderId"] = userId
							}
						});
					}
					_logger.LogDebug("Message notifications sent to {Count} participants", otherParticipants.Count);
				}
				catch (Exception notifyEx)
				{
					_logger.LogWarning(notifyEx, "Failed to send message notifications for chat {ChatId}", chatId);
				}
			}

			return CreatedAtAction(
				nameof(GetMessage),
				new { chatId, messageId = message.Id },
				message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ???????
	/// GET /api/chats/{chatId}/messages
	/// </summary>
	[HttpGet("{chatId}/messages")]
	[ProducesResponseType(typeof(PagedResult<MessageDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<MessageDto>>> GetMessages(
		Guid chatId,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting messages for chat {ChatId}", chatId);

			var request = new PaginationRequest
			{
				PageNumber = pageNumber,
				PageSize = pageSize
			};

			var messages = await _messageProvider.GetMessagesAsync(chatId, request);

			return Ok(messages);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting messages for chat {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ?????
	/// GET /api/chats/{chatId}/messages/{messageId}
	/// </summary>
	[HttpGet("{chatId}/messages/{messageId}")]
	[ProducesResponseType(typeof(MessageDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<MessageDto>> GetMessage(Guid chatId, Guid messageId)
	{
		try
		{
			_logger.LogDebug("Getting message {MessageId} from chat {ChatId}", messageId, chatId);

			var message = await _messageProvider.GetMessageAsync(messageId);

			if (message == null)
			{
				return NotFound(new { message = "Message not found" });
			}

			return Ok(message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting message {MessageId}", messageId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?????
	/// PUT /api/chats/{chatId}/messages/{messageId}
	/// </summary>
	[HttpPut("{chatId}/messages/{messageId}")]
	[ProducesResponseType(typeof(MessageDto), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(401)]
	[ProducesResponseType(403)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<MessageDto>> UpdateMessage(
		Guid chatId,
		Guid messageId,
		[FromBody] UpdateMessageDto dto)
	{
		try
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Updating message {MessageId} by user {UserId}", messageId, userId);

			// TODO: ?????? ?? ?? ???????? ?? ???? ???????

			var message = await _messageProvider.UpdateMessageAsync(messageId, dto);

			return Ok(message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating message {MessageId}", messageId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ??? ?????
	/// DELETE /api/chats/{chatId}/messages/{messageId}
	/// </summary>
	[HttpDelete("{chatId}/messages/{messageId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(401)]
	[ProducesResponseType(403)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> DeleteMessage(Guid chatId, Guid messageId)
	{
		try
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Deleting message {MessageId} by user {UserId}", messageId, userId);

			// TODO: ?????? ?? ?? ???????? ?? ???? ??????? ?? Admin

			await _messageProvider.DeleteMessageAsync(messageId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting message {MessageId}", messageId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??????? ???????
	/// POST /api/chats/{chatId}/mark-as-read
	/// </summary>
	[HttpPost("{chatId}/mark-as-read")]
	[ProducesResponseType(204)]
	[ProducesResponseType(401)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> MarkAsRead(
		Guid chatId,
		[FromBody] MarkAsReadRequest? request = null)
	{
		try
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Marking messages as read for user {UserId} in chat {ChatId}", userId, chatId);

			await _messageProvider.MarkAsReadAsync(chatId, userId, request?.LastMessageId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error marking messages as read");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ???????
	/// GET /api/chats/{chatId}/messages/search
	/// </summary>
	[HttpGet("{chatId}/messages/search")]
	[ProducesResponseType(typeof(PagedResult<MessageDto>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<MessageDto>>> SearchMessages(
		Guid chatId,
		[FromQuery] string query,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return BadRequest(new { message = "Search query is required" });
			}

			_logger.LogDebug("Searching messages in chat {ChatId} with query: {Query}", chatId, query);

			var request = new PaginationRequest
			{
				PageNumber = pageNumber,
				PageSize = pageSize
			};

			var messages = await _messageProvider.SearchMessagesAsync(chatId, query, request);

			return Ok(messages);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error searching messages in chat {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ???????
	/// POST /api/chats/{chatId}/participants
	/// </summary>
	[HttpPost("{chatId}/participants")]
	[ProducesResponseType(typeof(ParticipantDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(401)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ParticipantDto>> AddParticipant(
		Guid chatId,
		[FromBody] AddParticipantDto dto)
	{
		try
		{
			var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Adding participant {UserId} to chat {ChatId}", dto.UserId, chatId);

			// TODO: ?????? ?? ??????? ???????? ?????? ???????

			var participant = await _chatProvider.AddParticipantAsync(chatId, dto);

			return CreatedAtAction(
				nameof(GetParticipants),
				new { chatId },
				participant);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding participant to chat {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ?? ???????
	/// DELETE /api/chats/{chatId}/participants/{userId}
	/// </summary>
	[HttpDelete("{chatId}/participants/{userId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(401)]
	[ProducesResponseType(403)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> RemoveParticipant(Guid chatId, string userId)
	{
		try
		{
			var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			_logger.LogDebug("Removing participant {UserId} from chat {ChatId}", userId, chatId);

			// TODO: ?????? ?? ????????? (??? Owner/Admin ????? ????? ?????????)

			await _chatProvider.RemoveParticipantAsync(chatId, userId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing participant from chat {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// الحصول على قائمة المشاركين
	/// GET /api/chats/{chatId}/participants
	/// </summary>
	[HttpGet("{chatId}/participants")]
	[ProducesResponseType(typeof(List<ParticipantDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ParticipantDto>>> GetParticipants(Guid chatId)
	{
		try
		{
			_logger.LogDebug("Getting participants for chat {ChatId}", chatId);

			var participants = await _chatProvider.GetParticipantsAsync(chatId);

			return Ok(participants);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting participants for chat {ChatId}", chatId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// الحصول على أو إنشاء محادثة مباشرة مع مستخدم معين
	/// POST /api/chats/with-user/{targetUserId}
	/// إذا كانت هناك محادثة موجودة مع هذا المستخدم، يتم إرجاعها
	/// وإلا يتم إنشاء محادثة جديدة
	/// </summary>
	[HttpPost("with-user/{targetUserId}")]
	[ProducesResponseType(typeof(ChatDto), 200)]
	[ProducesResponseType(201)]
	[ProducesResponseType(401)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ChatDto>> GetOrCreateDirectChat(string targetUserId)
	{
		try
		{
			var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(currentUserId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			if (currentUserId == targetUserId)
			{
				return BadRequest(new { message = "Cannot create chat with yourself" });
			}

			_logger.LogDebug("Getting or creating direct chat between {CurrentUser} and {TargetUser}",
				currentUserId, targetUserId);

			// البحث عن محادثة مباشرة موجودة
			var paginationRequest = new PaginationRequest { PageNumber = 1, PageSize = 100 };
			var existingChats = await _chatProvider.GetUserChatsAsync(currentUserId, paginationRequest);

			// البحث في المحادثات المباشرة عن المحادثة مع المستخدم المستهدف
			var directChats = existingChats.Items?.Where(c => c.Type == ChatType.Direct).ToList() ?? new List<ChatDto>();

			foreach (var chat in directChats)
			{
				var participants = await _chatProvider.GetParticipantsAsync(chat.Id);
				if (participants.Count == 2 && participants.Any(p => p.UserId == targetUserId))
				{
					_logger.LogDebug("Found existing direct chat {ChatId}", chat.Id);
					return Ok(chat);
				}
			}

			// إنشاء محادثة جديدة
			_logger.LogDebug("Creating new direct chat");
			var createDto = new CreateChatDto
			{
				Title = "محادثة مباشرة",
				Type = ChatType.Direct,
				CreatorUserId = currentUserId,
				ParticipantUserIds = new List<string> { currentUserId, targetUserId }
			};

			var newChat = await _chatProvider.CreateChatAsync(createDto);

			// Track Lead event - user contacting another user (likely a host)
			if (_marketingTracker != null)
			{
				try
				{
					var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
					var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

					await _marketingTracker.TrackLeadAsync(new LeadTrackingRequest
					{
						ContentId = newChat.Id.ToString(),
						ContentName = "Direct Chat",
						LeadType = "contact_host",
						User = new UserTrackingContext
						{
							UserId = currentUserId,
							IpAddress = ipAddress,
							UserAgent = userAgent
						}
					});
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to track Lead event for chat {ChatId}", newChat.Id);
				}
			}

			return CreatedAtAction(nameof(GetById), new { id = newChat.Id }, newChat);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting or creating direct chat with user {TargetUserId}", targetUserId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// ============================================================================
// DTOs للمحادثات
// ============================================================================

public class UpdateChatDto
{
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
}

public class PartialUpdateChatDto
{
	public string? Title { get; set; }
}

public class SendMessageRequest
{
	public string Content { get; set; } = string.Empty;
	public string Type { get; set; } = "Text";
	public Guid? ReplyToMessageId { get; set; }
	public List<string>? Attachments { get; set; }
}

/// <summary>
/// DTO تفصيلي للمحادثة يتضمن بيانات المشاركين
/// </summary>
public class ChatDetailDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Type { get; set; } = "Direct";
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public int ParticipantsCount { get; set; }
	public int UnreadMessagesCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// بيانات الطرف الآخر (للمحادثات المباشرة)
	public string? OtherPartyId { get; set; }
	public string OtherPartyName { get; set; } = string.Empty;
	public string? OtherPartyAvatar { get; set; }
	public bool IsOnline { get; set; }

	// قائمة المشاركين
	public List<string> Participants { get; set; } = new();
}

