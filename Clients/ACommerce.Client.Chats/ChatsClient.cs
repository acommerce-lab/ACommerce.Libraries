using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Chats;

public sealed class ChatsClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";
	private const string BasePath = "/api/chats";

	public ChatsClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على محادثاتي
	/// GET /api/chats/my-chats
	/// </summary>
	public async Task<PagedChatsResult?> GetMyChatsAsync(
		int pageNumber = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<PagedChatsResult>(
			ServiceName,
			$"{BasePath}/my-chats?pageNumber={pageNumber}&pageSize={pageSize}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على المحادثات (للتوافق مع الكود القديم)
	/// </summary>
	public async Task<List<ConversationResponse>?> GetConversationsAsync(
		CancellationToken cancellationToken = default)
	{
		var result = await GetMyChatsAsync(1, 100, cancellationToken);
		return result?.Items;
	}

	/// <summary>
	/// الحصول على محادثاتي (اسم بديل)
	/// </summary>
	public Task<List<ConversationResponse>?> GetMyConversationsAsync(
		CancellationToken cancellationToken = default)
	{
		return GetConversationsAsync(cancellationToken);
	}

	/// <summary>
	/// الحصول على محادثة محددة
	/// GET /api/chats/{id}
	/// </summary>
	public async Task<ConversationResponse?> GetConversationAsync(
		Guid chatId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ConversationResponse>(
			ServiceName,
			$"{BasePath}/{chatId}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على الرسائل
	/// GET /api/chats/{chatId}/messages
	/// </summary>
	public async Task<PagedMessagesResult?> GetMessagesPagedAsync(
		Guid chatId,
		int pageNumber = 1,
		int pageSize = 50,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<PagedMessagesResult>(
			ServiceName,
			$"{BasePath}/{chatId}/messages?pageNumber={pageNumber}&pageSize={pageSize}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على الرسائل (للتوافق مع الكود القديم)
	/// </summary>
	public async Task<List<MessageResponse>?> GetMessagesAsync(
		Guid chatId,
		int page = 1,
		int pageSize = 50,
		CancellationToken cancellationToken = default)
	{
		var result = await GetMessagesPagedAsync(chatId, page, pageSize, cancellationToken);
		return result?.Items;
	}

	/// <summary>
	/// إرسال رسالة
	/// POST /api/chats/{chatId}/messages
	/// </summary>
	public async Task<MessageResponse?> SendMessageAsync(
		Guid chatId,
		SendMessageRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<SendMessageRequest, MessageResponse>(
			ServiceName,
			$"{BasePath}/{chatId}/messages",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إرسال رسالة نصية بسيطة
	/// </summary>
	public Task<MessageResponse?> SendMessageAsync(
		Guid chatId,
		string content,
		CancellationToken cancellationToken = default)
	{
		return SendMessageAsync(chatId, new SendMessageRequest { Content = content }, cancellationToken);
	}

	/// <summary>
	/// إنشاء محادثة جديدة
	/// POST /api/chats
	/// </summary>
	public async Task<ConversationResponse?> CreateChatAsync(
		CreateChatRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateChatRequest, ConversationResponse>(
			ServiceName,
			BasePath,
			request,
			cancellationToken);
	}

	/// <summary>
	/// بدء محادثة جديدة (للتوافق مع الكود القديم)
	/// </summary>
	public async Task<ConversationResponse?> StartConversationAsync(
		StartConversationRequest request,
		CancellationToken cancellationToken = default)
	{
		var createRequest = new CreateChatRequest
		{
			Title = request.Title ?? "محادثة جديدة",
			ParticipantIds = request.ParticipantIds
		};
		return await CreateChatAsync(createRequest, cancellationToken);
	}

	/// <summary>
	/// إنشاء محادثة جديدة (اسم بديل)
	/// </summary>
	public Task<ConversationResponse?> CreateConversationAsync(
		CreateConversationRequest request,
		CancellationToken cancellationToken = default)
	{
		var participants = request.ParticipantIds;
		if (!string.IsNullOrEmpty(request.RecipientIdentifier) && !participants.Contains(request.RecipientIdentifier))
		{
			participants = new List<string>(participants) { request.RecipientIdentifier };
		}
		return StartConversationAsync(new StartConversationRequest
		{
			ParticipantIds = participants,
			Title = request.Title,
			InitialMessage = request.InitialMessage
		}, cancellationToken);
	}

	/// <summary>
	/// تعليم الرسائل كمقروءة
	/// POST /api/chats/{chatId}/mark-as-read
	/// </summary>
	public async Task MarkAsReadAsync(
		Guid chatId,
		Guid? lastMessageId = null,
		CancellationToken cancellationToken = default)
	{
		var request = lastMessageId.HasValue
			? new { LastMessageId = lastMessageId }
			: new { LastMessageId = (Guid?)null };
		await _httpClient.PostAsync<object>(
			ServiceName,
			$"{BasePath}/{chatId}/mark-as-read",
			request,
			cancellationToken);
	}

	/// <summary>
	/// البحث في الرسائل
	/// GET /api/chats/{chatId}/messages/search
	/// </summary>
	public async Task<PagedMessagesResult?> SearchMessagesAsync(
		Guid chatId,
		string query,
		int pageNumber = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<PagedMessagesResult>(
			ServiceName,
			$"{BasePath}/{chatId}/messages/search?query={Uri.EscapeDataString(query)}&pageNumber={pageNumber}&pageSize={pageSize}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على المشاركين في المحادثة
	/// GET /api/chats/{chatId}/participants
	/// </summary>
	public async Task<List<ParticipantResponse>?> GetParticipantsAsync(
		Guid chatId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ParticipantResponse>>(
			ServiceName,
			$"{BasePath}/{chatId}/participants",
			cancellationToken);
	}

	/// <summary>
	/// إضافة مشارك للمحادثة
	/// POST /api/chats/{chatId}/participants
	/// </summary>
	public async Task<ParticipantResponse?> AddParticipantAsync(
		Guid chatId,
		AddParticipantRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<AddParticipantRequest, ParticipantResponse>(
			ServiceName,
			$"{BasePath}/{chatId}/participants",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إزالة مشارك من المحادثة
	/// DELETE /api/chats/{chatId}/participants/{userId}
	/// </summary>
	public async Task RemoveParticipantAsync(
		Guid chatId,
		string userId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"{BasePath}/{chatId}/participants/{userId}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على أو إنشاء محادثة مباشرة مع مستخدم معين
	/// إذا كانت هناك محادثة موجودة، يتم إرجاعها
	/// وإلا يتم إنشاء محادثة جديدة
	/// POST /api/chats/with-user/{userId}
	/// </summary>
	public async Task<ConversationResponse?> GetOrCreateConversationAsync(
		string targetUserId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<object, ConversationResponse>(
			ServiceName,
			$"{BasePath}/with-user/{targetUserId}",
			new { },
			cancellationToken);
	}
}

public sealed class ConversationResponse
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string OtherPartyName { get; set; } = string.Empty;
	public string? OtherPartyAvatar { get; set; }
	public bool IsOnline { get; set; }
	public List<string> Participants { get; set; } = new();
	public MessageResponse? LastMessage { get; set; }
	public string LastMessageText => LastMessage?.Content ?? string.Empty;
	public DateTime LastMessageAt => LastMessage?.CreatedAt ?? CreatedAt;
	public int UnreadCount { get; set; }
	public bool HasUnread => UnreadCount > 0;
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class MessageResponse
{
	public Guid Id { get; set; }
	public Guid ConversationId { get; set; }
	public string SenderId { get; set; } = string.Empty;
	public string SenderName { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	/// <summary>
	/// Alias for Content property
	/// </summary>
	public string Text { get => Content; set => Content = value; }
	public string Type { get; set; } = "Text"; // "Text", "Image", "File"
	public string? AttachmentUrl { get; set; }
	public string? AttachmentName { get; set; }
	public bool IsRead { get; set; }
	public bool IsMine { get; set; }
	public DateTime CreatedAt { get; set; }
	/// <summary>
	/// Alias for CreatedAt property
	/// </summary>
	public DateTime SentAt { get => CreatedAt; set => CreatedAt = value; }
}

public sealed class SendMessageRequest
{
	public string Content { get; set; } = string.Empty;
	public string Type { get; set; } = "Text";
	public string? AttachmentUrl { get; set; }
}

public sealed class StartConversationRequest
{
	public List<string> ParticipantIds { get; set; } = new();
	public string? Title { get; set; }
	public string? InitialMessage { get; set; }
}

public sealed class CreateConversationRequest
{
	public List<string> ParticipantIds { get; set; } = new();
	/// <summary>
	/// Alternative to ParticipantIds - specify a single recipient by username/email
	/// </summary>
	public string? RecipientIdentifier { get; set; }
	public string? Title { get; set; }
	public string? InitialMessage { get; set; }
}

/// <summary>
/// نتيجة المحادثات المقسمة لصفحات
/// </summary>
public sealed class PagedChatsResult
{
	public List<ConversationResponse> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
}

/// <summary>
/// نتيجة الرسائل المقسمة لصفحات
/// </summary>
public sealed class PagedMessagesResult
{
	public List<MessageResponse> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
}

/// <summary>
/// طلب إنشاء محادثة جديدة
/// </summary>
public sealed class CreateChatRequest
{
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public string Type { get; set; } = "Direct"; // Direct, Group, Channel, Support
	public List<string> ParticipantIds { get; set; } = new();
}

/// <summary>
/// بيانات المشارك
/// </summary>
public sealed class ParticipantResponse
{
	public Guid Id { get; set; }
	public Guid ChatId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string Role { get; set; } = "Member"; // Owner, Admin, Member, Guest
	public DateTime? LastSeenMessageAt { get; set; }
	public Guid? LastSeenMessageId { get; set; }
	public bool IsMuted { get; set; }
	public bool IsPinned { get; set; }
	public DateTime CreatedAt { get; set; }
}

/// <summary>
/// طلب إضافة مشارك
/// </summary>
public sealed class AddParticipantRequest
{
	public string UserId { get; set; } = string.Empty;
	public string Role { get; set; } = "Member";
}
