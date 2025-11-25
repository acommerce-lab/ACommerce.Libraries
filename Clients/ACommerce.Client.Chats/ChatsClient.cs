using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Chats;

public sealed class ChatsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Chats"; // أو "Marketplace"

	public ChatsClient(DynamicHttpClient httpClient)
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
	public List<string> Participants { get; set; } = new();
	public MessageResponse? LastMessage { get; set; }
	public int UnreadCount { get; set; }
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
	public string Type { get; set; } = "Text"; // "Text", "Image", "File"
	public string? AttachmentUrl { get; set; }
	public bool IsRead { get; set; }
	public DateTime CreatedAt { get; set; }
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
