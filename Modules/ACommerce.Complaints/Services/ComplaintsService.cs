using ACommerce.Complaints.DTOs;
using ACommerce.Complaints.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Microsoft.Extensions.Logging;

namespace ACommerce.Complaints.Services;

public class ComplaintsService(
    IBaseAsyncRepository<Complaint> complaintsRepository,
    IBaseAsyncRepository<ComplaintReply> repliesRepository,
    ILogger<ComplaintsService> logger) : IComplaintsService
{
    public async Task<ComplaintResponseDto> CreateAsync(string userId, CreateComplaintDto request, CancellationToken cancellationToken = default)
    {
        var ticketNumber = GenerateTicketNumber();

        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TicketNumber = ticketNumber,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Category = request.Category,
            RelatedEntityType = request.RelatedEntityType,
            RelatedEntityId = request.RelatedEntityId,
            Attachments = request.Attachments ?? [],
            Metadata = request.Metadata ?? [],
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        await complaintsRepository.AddAsync(complaint, cancellationToken);
        logger.LogInformation("Created complaint {TicketNumber} for user {UserId}", ticketNumber, userId);

        return MapToResponse(complaint);
    }

    public async Task<ComplaintResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintsRepository.GetByIdAsync(id, cancellationToken);
        if (complaint == null) return null;

        var repliesCount = (await repliesRepository.ListAllAsync(cancellationToken))
            .Count(r => r.ComplaintId == id && !r.IsDeleted);

        return MapToResponse(complaint, repliesCount);
    }

    public async Task<ComplaintResponseDto?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        var complaints = await complaintsRepository.ListAllAsync(cancellationToken);
        var complaint = complaints.FirstOrDefault(c => c.TicketNumber == ticketNumber);
        if (complaint == null) return null;

        var repliesCount = (await repliesRepository.ListAllAsync(cancellationToken))
            .Count(r => r.ComplaintId == complaint.Id && !r.IsDeleted);

        return MapToResponse(complaint, repliesCount);
    }

    public async Task<IEnumerable<ComplaintSummaryDto>> GetByUserIdAsync(string userId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var allComplaints = await complaintsRepository.ListAllAsync(cancellationToken);
        var userComplaints = allComplaints
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Where(c => status == null || c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var allReplies = await repliesRepository.ListAllAsync(cancellationToken);

        return userComplaints.Select(c => new ComplaintSummaryDto
        {
            Id = c.Id,
            TicketNumber = c.TicketNumber,
            Type = c.Type,
            Title = c.Title,
            Status = c.Status,
            Priority = c.Priority,
            Category = c.Category,
            CreatedAt = c.CreatedAt,
            RepliesCount = allReplies.Count(r => r.ComplaintId == c.Id && !r.IsDeleted)
        });
    }

    public async Task<ComplaintResponseDto?> UpdateAsync(Guid id, string userId, UpdateComplaintDto request, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintsRepository.GetByIdAsync(id, cancellationToken);
        if (complaint == null || complaint.UserId != userId) return null;

        if (request.Title != null) complaint.Title = request.Title;
        if (request.Description != null) complaint.Description = request.Description;
        if (request.Status != null) complaint.Status = request.Status;
        if (request.Priority != null) complaint.Priority = request.Priority;
        if (request.Category != null) complaint.Category = request.Category;
        if (request.Attachments != null) complaint.Attachments = request.Attachments;

        complaint.UpdatedAt = DateTime.UtcNow;

        await complaintsRepository.UpdateAsync(complaint, cancellationToken);
        return MapToResponse(complaint);
    }

    public async Task<bool> CloseAsync(Guid id, string userId, CloseComplaintDto? request = null, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintsRepository.GetByIdAsync(id, cancellationToken);
        if (complaint == null || complaint.UserId != userId) return false;

        complaint.Status = "Closed";
        complaint.ClosedAt = DateTime.UtcNow;
        complaint.UpdatedAt = DateTime.UtcNow;

        await complaintsRepository.UpdateAsync(complaint, cancellationToken);
        logger.LogInformation("Closed complaint {TicketNumber}", complaint.TicketNumber);
        return true;
    }

    public async Task<bool> ReopenAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintsRepository.GetByIdAsync(id, cancellationToken);
        if (complaint == null || complaint.UserId != userId) return false;

        complaint.Status = "Open";
        complaint.ClosedAt = null;
        complaint.UpdatedAt = DateTime.UtcNow;

        await complaintsRepository.UpdateAsync(complaint, cancellationToken);
        logger.LogInformation("Reopened complaint {TicketNumber}", complaint.TicketNumber);
        return true;
    }

    public async Task<bool> RateAsync(Guid id, string userId, RateComplaintDto request, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintsRepository.GetByIdAsync(id, cancellationToken);
        if (complaint == null || complaint.UserId != userId) return false;

        complaint.UserRating = request.Rating;
        complaint.UserFeedback = request.Feedback;
        complaint.UpdatedAt = DateTime.UtcNow;

        await complaintsRepository.UpdateAsync(complaint, cancellationToken);
        logger.LogInformation("Rated complaint {TicketNumber} with {Rating} stars", complaint.TicketNumber, request.Rating);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var complaint = await complaintsRepository.GetByIdAsync(id, cancellationToken);
        if (complaint == null || complaint.UserId != userId) return false;

        await complaintsRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<IEnumerable<ComplaintReplyResponseDto>> GetRepliesAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        var allReplies = await repliesRepository.ListAllAsync(cancellationToken);
        return allReplies
            .Where(r => r.ComplaintId == complaintId && !r.IsDeleted && !r.IsInternal)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ComplaintReplyResponseDto
            {
                Id = r.Id,
                ComplaintId = r.ComplaintId,
                SenderId = r.SenderId,
                SenderName = r.SenderName,
                IsStaff = r.IsStaff,
                Message = r.Message,
                Attachments = r.Attachments,
                CreatedAt = r.CreatedAt
            });
    }

    public async Task<ComplaintReplyResponseDto> AddReplyAsync(Guid complaintId, string userId, string userName, CreateComplaintReplyDto request, CancellationToken cancellationToken = default)
    {
        var reply = new ComplaintReply
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaintId,
            SenderId = userId,
            SenderName = userName,
            IsStaff = false,
            Message = request.Message,
            Attachments = request.Attachments ?? [],
            IsInternal = false,
            CreatedAt = DateTime.UtcNow
        };

        await repliesRepository.AddAsync(reply, cancellationToken);
        logger.LogInformation("Added reply to complaint {ComplaintId} by user {UserId}", complaintId, userId);

        return new ComplaintReplyResponseDto
        {
            Id = reply.Id,
            ComplaintId = reply.ComplaintId,
            SenderId = reply.SenderId,
            SenderName = reply.SenderName,
            IsStaff = reply.IsStaff,
            Message = reply.Message,
            Attachments = reply.Attachments,
            CreatedAt = reply.CreatedAt
        };
    }

    public async Task<ComplaintStatsDto> GetUserStatsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var allComplaints = await complaintsRepository.ListAllAsync(cancellationToken);
        var userComplaints = allComplaints.Where(c => c.UserId == userId && !c.IsDeleted).ToList();

        return new ComplaintStatsDto
        {
            TotalComplaints = userComplaints.Count,
            OpenComplaints = userComplaints.Count(c => c.Status == "Open"),
            InProgressComplaints = userComplaints.Count(c => c.Status == "InProgress"),
            ResolvedComplaints = userComplaints.Count(c => c.Status == "Resolved"),
            ClosedComplaints = userComplaints.Count(c => c.Status == "Closed"),
            AverageRating = userComplaints.Where(c => c.UserRating.HasValue).Select(c => c.UserRating!.Value).DefaultIfEmpty(0).Average()
        };
    }

    private static string GenerateTicketNumber()
    {
        return $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }

    private static ComplaintResponseDto MapToResponse(Complaint complaint, int repliesCount = 0)
    {
        return new ComplaintResponseDto
        {
            Id = complaint.Id,
            UserId = complaint.UserId,
            TicketNumber = complaint.TicketNumber,
            Type = complaint.Type,
            Title = complaint.Title,
            Description = complaint.Description,
            Status = complaint.Status,
            Priority = complaint.Priority,
            Category = complaint.Category,
            RelatedEntityType = complaint.RelatedEntityType,
            RelatedEntityId = complaint.RelatedEntityId,
            Attachments = complaint.Attachments,
            AssignedToId = complaint.AssignedToId,
            AssignedToName = complaint.AssignedToName,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt,
            ClosedAt = complaint.ClosedAt,
            UserRating = complaint.UserRating,
            UserFeedback = complaint.UserFeedback,
            RepliesCount = repliesCount,
            Metadata = complaint.Metadata
        };
    }
}
