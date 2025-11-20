using ACommerce.Realtime.Abstractions.Contracts;
using ACommerce.Realtime.SignalR.Hubs;
using Microsoft.Extensions.Logging;

namespace ACommerce.Chats.Core.Hubs;

/// <summary>
/// SignalR Hub ????????
/// ??? ?? BaseRealtimeHub ?? ACommerce.Realtime! ?
/// </summary>
public class ChatHub : BaseRealtimeHub<IChatClient>
{
	public ChatHub(ILogger<ChatHub> logger) : base(logger)
	{
	}

	/// <summary>
	/// ???????? ??????
	/// </summary>
	public async Task JoinChat(string chatId)
	{
		var userId = GetUserId();
		if (string.IsNullOrEmpty(userId))
		{
			Logger.LogWarning("Anonymous user tried to join chat {ChatId}", chatId);
			return;
		}

		await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
		Logger.LogInformation("User {UserId} joined chat {ChatId}", userId, chatId);
	}

	/// <summary>
	/// ?????? ?????
	/// </summary>
	public async Task LeaveChat(string chatId)
	{
		var userId = GetUserId();
		if (string.IsNullOrEmpty(userId))
		{
			return;
		}

		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
		Logger.LogInformation("User {UserId} left chat {ChatId}", userId, chatId);
	}

	/// <summary>
	/// ????? ???? ???????
	/// </summary>
	public async Task SendTypingIndicator(string chatId, bool isTyping)
	{
		var userId = GetUserId();
		if (string.IsNullOrEmpty(userId))
		{
			return;
		}

		await Clients.OthersInGroup($"chat_{chatId}")
			.ReceiveMessage("UserTyping", new
			{
				ChatId = chatId,
				UserId = userId,
				IsTyping = isTyping
			});
	}

	protected override async Task OnUserConnectedAsync(string userId)
	{
		Logger.LogInformation("User {UserId} connected to chat system", userId);

		// ???? ????? ????? ???????? ?? ???????? ???? ????
		await Clients.All.ReceiveMessage("UserPresenceChanged", new
		{
			UserId = userId,
			IsOnline = true
		});
	}

	protected override async Task OnUserDisconnectedAsync(string userId, Exception? exception)
	{
		Logger.LogInformation("User {UserId} disconnected from chat system", userId);

		// ???? ????? ????? ???????? ?? ???????? ???? ??? ????
		await Clients.All.ReceiveMessage("UserPresenceChanged", new
		{
			UserId = userId,
			IsOnline = false
		});
	}
}

/// <summary>
/// ????? ??? Client ????????
/// </summary>
public interface IChatClient : IRealtimeClient
{
	// ???? ????? methods ?????? ??? ??? ???
}

