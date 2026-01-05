using ACommerce.Complaints.DTOs;
using ACommerce.Complaints.Entities;

namespace ACommerce.Complaints.Services;

public interface IComplaintsService
{
    // Complaints
    Task<ComplaintResponseDto> CreateAsync(string userId, CreateComplaintDto request, CancellationToken cancellationToken = default);
    Task<ComplaintResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ComplaintResponseDto?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintSummaryDto>> GetByUserIdAsync(string userId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ComplaintResponseDto?> UpdateAsync(Guid id, string userId, UpdateComplaintDto request, CancellationToken cancellationToken = default);
    Task<bool> CloseAsync(Guid id, string userId, CloseComplaintDto? request = null, CancellationToken cancellationToken = default);
    Task<bool> ReopenAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<bool> RateAsync(Guid id, string userId, RateComplaintDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default);

    // Replies
    Task<IEnumerable<ComplaintReplyResponseDto>> GetRepliesAsync(Guid complaintId, CancellationToken cancellationToken = default);
    Task<ComplaintReplyResponseDto> AddReplyAsync(Guid complaintId, string userId, string userName, CreateComplaintReplyDto request, CancellationToken cancellationToken = default);

    // Stats
    Task<ComplaintStatsDto> GetUserStatsAsync(string userId, CancellationToken cancellationToken = default);
}
