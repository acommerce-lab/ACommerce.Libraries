using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Chats;

public sealed class ChatsClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Chats"; // أو "Marketplace"

	public ChatsClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على المحادثات
	/// </summary>
	public async Task<List<ConversationResponse>?> GetConversationsAsync(
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ConversationResponse>>(
			ServiceName,
			"/api/chats/conversations",
			cancellationToken);
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
	/// </summary>
	public async Task<ConversationResponse?> GetConversationAsync(
		Guid conversationId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ConversationResponse>(
			ServiceName,
			$"/api/chats/conversations/{conversationId}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على الرسائل
	/// </summary>
	public async Task<List<MessageResponse>?> GetMessagesAsync(
		Guid conversationId,
		int page = 1,
		int pageSize = 50,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<MessageResponse>>(
			ServiceName,
			$"/api/chats/conversations/{conversationId}/messages?page={page}&pageSize={pageSize}",
			cancellationToken);
	}

	/// <summary>
	/// إرسال رسالة
	/// </summary>
	public async Task<MessageResponse?> SendMessageAsync(
		Guid conversationId,
		SendMessageRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<SendMessageRequest, MessageResponse>(
			ServiceName,
			$"/api/chats/conversations/{conversationId}/messages",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إرسال رسالة نصية بسيطة
	/// </summary>
	public Task<MessageResponse?> SendMessageAsync(
		Guid conversationId,
		string content,
		CancellationToken cancellationToken = default)
	{
		return SendMessageAsync(conversationId, new SendMessageRequest { Content = content }, cancellationToken);
	}

	/// <summary>
	/// بدء محادثة جديدة
	/// </summary>
	public async Task<ConversationResponse?> StartConversationAsync(
		StartConversationRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<StartConversationRequest, ConversationResponse>(
			ServiceName,
			"/api/chats/conversations",
			request,
			cancellationToken);
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
	/// </summary>
	public async Task MarkAsReadAsync(
		Guid conversationId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			$"/api/chats/conversations/{conversationId}/mark-read",
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
