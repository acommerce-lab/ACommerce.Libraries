using ACommerce.Transactions.Core.Enums;

namespace ACommerce.Transactions.Core.DTOs.OperationNotification;

public class CreateOperationNotificationDto
{
	public Guid OperationId { get; set; }
	public NotificationType Type { get; set; }
	public required string Template { get; set; }
	public string? Subject { get; set; }
	public List<string> Recipients { get; set; } = new();
	public List<string>? CcRecipients { get; set; }
	public int? DelayMinutes { get; set; }
	public int Priority { get; set; } = 1;
	public Dictionary<string, string>? Metadata { get; set; }
}

