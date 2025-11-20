using FirebaseAdmin.Messaging;

namespace ACommerce.Notifications.Channels.Firebase.Models;

/// <summary>
/// ???? Token ???? Firebase ??????? ????
/// </summary>
public class FirebaseDeviceToken
{
	/// <summary>
	/// ???? ????????
	/// </summary>
	public required string UserId { get; init; }

	/// <summary>
	/// FCM Token
	/// </summary>
	public required string Token { get; init; }

	/// <summary>
	/// ??? ??????
	/// </summary>
	public DevicePlatform Platform { get; init; }

	/// <summary>
	/// ????? ???????
	/// </summary>
	public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// ????? ??? ???????
	/// </summary>
	public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// ?? Token ????
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// Device Info ?????
	/// </summary>
	public Dictionary<string, string>? Metadata { get; init; }
}

public enum DevicePlatform
{
	iOS,
	Android,
	Web
}

public class CustomBatchResponse
{
	public int SuccessCount { get; set; }
	public int FailureCount { get; set; }
	public List<SendResponse>? Responses { get; set; } // ?? ??? ??? ?????
}

