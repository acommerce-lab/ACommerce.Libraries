using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Complaints;

public sealed class ComplaintsClient(IApiClient httpClient)
{
	private const string ServiceName = "Marketplace";

	#region Complaints

	/// <summary>
	/// إنشاء شكوى جديدة
	/// </summary>
	public async Task<ComplaintResponse?> CreateComplaintAsync(
		CreateComplaintRequest request,
		CancellationToken cancellationToken = default)
	{
		return await httpClient.PostAsync<CreateComplaintRequest, ComplaintResponse>(
			ServiceName,
			"/api/complaints",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على شكوى بالمعرف
	/// </summary>
	public async Task<ComplaintResponse?> GetComplaintAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<ComplaintResponse>(
			ServiceName,
			$"/api/complaints/{id}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على شكوى برقم التذكرة
	/// </summary>
	public async Task<ComplaintResponse?> GetByTicketNumberAsync(
		string ticketNumber,
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<ComplaintResponse>(
			ServiceName,
			$"/api/complaints/ticket/{ticketNumber}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على شكاواي
	/// </summary>
	public async Task<List<ComplaintSummary>?> GetMyComplaintsAsync(
		string? status = null,
		int page = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		var query = $"?page={page}&pageSize={pageSize}";
		if (!string.IsNullOrEmpty(status))
			query += $"&status={status}";

		return await httpClient.GetAsync<List<ComplaintSummary>>(
			ServiceName,
			$"/api/complaints/me{query}",
			cancellationToken);
	}

	/// <summary>
	/// تحديث شكوى
	/// </summary>
	public async Task<ComplaintResponse?> UpdateComplaintAsync(
		Guid id,
		UpdateComplaintRequest request,
		CancellationToken cancellationToken = default)
	{
		return await httpClient.PutAsync<UpdateComplaintRequest, ComplaintResponse>(
			ServiceName,
			$"/api/complaints/{id}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إغلاق شكوى
	/// </summary>
	public async Task CloseComplaintAsync(
		Guid id,
		string? resolution = null,
		CancellationToken cancellationToken = default)
	{
		await httpClient.PostAsync<object>(
			ServiceName,
			$"/api/complaints/{id}/close",
			new { resolution },
			cancellationToken);
	}

	/// <summary>
	/// إعادة فتح شكوى
	/// </summary>
	public async Task ReopenComplaintAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		await httpClient.PostAsync<object>(
			ServiceName,
			$"/api/complaints/{id}/reopen",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// تقييم الحل
	/// </summary>
	public async Task RateComplaintAsync(
		Guid id,
		int rating,
		string? feedback = null,
		CancellationToken cancellationToken = default)
	{
		await httpClient.PostAsync<object>(
			ServiceName,
			$"/api/complaints/{id}/rate",
			new { rating, feedback },
			cancellationToken);
	}

	/// <summary>
	/// حذف شكوى
	/// </summary>
	public async Task DeleteComplaintAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		await httpClient.DeleteAsync(
			ServiceName,
			$"/api/complaints/{id}",
			cancellationToken);
	}

	#endregion

	#region Replies

	/// <summary>
	/// الحصول على ردود الشكوى
	/// </summary>
	public async Task<List<ComplaintReplyResponse>?> GetRepliesAsync(
		Guid complaintId,
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<ComplaintReplyResponse>>(
			ServiceName,
			$"/api/complaints/{complaintId}/replies",
			cancellationToken);
	}

	/// <summary>
	/// إضافة رد على الشكوى
	/// </summary>
	public async Task<ComplaintReplyResponse?> AddReplyAsync(
		Guid complaintId,
		string message,
		List<string>? attachments = null,
		CancellationToken cancellationToken = default)
	{
		var request = new CreateReplyRequest { Message = message, Attachments = attachments };
		return await httpClient.PostAsync<CreateReplyRequest, ComplaintReplyResponse>(
			ServiceName,
			$"/api/complaints/{complaintId}/replies",
			request,
			cancellationToken);
	}

	#endregion

	#region Statistics

	/// <summary>
	/// الحصول على إحصائيات شكاواي
	/// </summary>
	public async Task<ComplaintStats?> GetMyStatsAsync(
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<ComplaintStats>(
			ServiceName,
			"/api/complaints/me/stats",
			cancellationToken);
	}

	#endregion
}

#region Models

public sealed class CreateComplaintRequest
{
	public string Type { get; set; } = "Complaint";
	public required string Title { get; set; }
	public required string Description { get; set; }
	public string Priority { get; set; } = "Medium";
	public string Category { get; set; } = "Other";
	public string? RelatedEntityType { get; set; }
	public Guid? RelatedEntityId { get; set; }
	public List<string>? Attachments { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public sealed class UpdateComplaintRequest
{
	public string? Title { get; set; }
	public string? Description { get; set; }
	public List<string>? Attachments { get; set; }
}

public sealed class CreateReplyRequest
{
	public required string Message { get; set; }
	public List<string>? Attachments { get; set; }
}

public sealed class ComplaintResponse
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

public sealed class ComplaintSummary
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

public sealed class ComplaintReplyResponse
{
	public Guid Id { get; set; }
	public Guid ComplaintId { get; set; }
	public string SenderId { get; set; } = string.Empty;
	public string SenderName { get; set; } = string.Empty;
	public bool IsStaff { get; set; }
	public string Message { get; set; } = string.Empty;
	public List<string> Attachments { get; set; } = [];
	public DateTime CreatedAt { get; set; }
}

public sealed class ComplaintStats
{
	public int TotalComplaints { get; set; }
	public int OpenComplaints { get; set; }
	public int InProgressComplaints { get; set; }
	public int ResolvedComplaints { get; set; }
	public int ClosedComplaints { get; set; }
	public double AverageRating { get; set; }
}

#endregion
