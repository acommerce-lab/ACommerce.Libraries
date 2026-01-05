namespace ACommerce.Complaints.DTOs;

#region Complaint DTOs

public class CreateComplaintDto
{
	public required string Type { get; set; } = "Complaint";
	public required string Title { get; set; }
	public required string Description { get; set; }
	public string Priority { get; set; } = "Medium";
	public string Category { get; set; } = "Other";
	public string? RelatedEntityType { get; set; }
	public Guid? RelatedEntityId { get; set; }
	public List<string>? Attachments { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateComplaintDto
{
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Status { get; set; }
	public string? Priority { get; set; }
	public string? Category { get; set; }
	public string? AssignedToId { get; set; }
	public string? AssignedToName { get; set; }
	public List<string>? Attachments { get; set; }
}

public class CloseComplaintDto
{
	public string? Resolution { get; set; }
}

public class RateComplaintDto
{
	public required int Rating { get; set; }
	public string? Feedback { get; set; }
}

public class ComplaintResponseDto
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string TicketNumber { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string Priority { get; set; } = string.Empty;
	public string Category { get; set; } = string.Empty;
	public string? RelatedEntityType { get; set; }
	public Guid? RelatedEntityId { get; set; }
	public List<string> Attachments { get; set; } = [];
	public string? AssignedToId { get; set; }
	public string? AssignedToName { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public DateTime? ClosedAt { get; set; }
	public int? UserRating { get; set; }
	public string? UserFeedback { get; set; }
	public int RepliesCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = [];
}

public class ComplaintSummaryDto
{
	public Guid Id { get; set; }
	public string TicketNumber { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string Priority { get; set; } = string.Empty;
	public string Category { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public int RepliesCount { get; set; }
}

#endregion

#region ComplaintReply DTOs

public class CreateComplaintReplyDto
{
	public required Guid ComplaintId { get; set; }
	public required string Message { get; set; }
	public List<string>? Attachments { get; set; }
	public bool IsInternal { get; set; }
}

public class ComplaintReplyResponseDto
{
	public Guid Id { get; set; }
	public Guid ComplaintId { get; set; }
	public string SenderId { get; set; } = string.Empty;
	public string SenderName { get; set; } = string.Empty;
	public bool IsStaff { get; set; }
	public string Message { get; set; } = string.Empty;
	public List<string> Attachments { get; set; } = [];
	public bool IsInternal { get; set; }
	public DateTime CreatedAt { get; set; }
}

#endregion

#region Statistics DTOs

public class ComplaintStatsDto
{
	public int TotalComplaints { get; set; }
	public int OpenComplaints { get; set; }
	public int InProgressComplaints { get; set; }
	public int ResolvedComplaints { get; set; }
	public int ClosedComplaints { get; set; }
	public double AverageRating { get; set; }
	public double AverageResolutionTimeHours { get; set; }
}

#endregion
