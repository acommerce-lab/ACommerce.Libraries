using ACommerce.Admin.AuditLog.DTOs;
using ACommerce.Admin.AuditLog.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.AuditLog.Queries;

public record GetAuditLogsQuery(AuditLogFilterDto Filter) : IRequest<AuditLogListDto>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, AuditLogListDto>
{
    private readonly IBaseAsyncRepository<AuditLogEntry> _repository;

    public GetAuditLogsQueryHandler(IBaseAsyncRepository<AuditLogEntry> repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogListDto> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var allItems = await _repository.ListAllAsync(cancellationToken);
        var query = allItems.AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(e => e.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(e => e.CreatedAt <= filter.EndDate.Value);

        if (filter.UserId.HasValue)
            query = query.Where(e => e.UserId == filter.UserId.Value);

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(e => e.Action == filter.Action);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(e => e.EntityType == filter.EntityType);

        if (filter.Level.HasValue)
            query = query.Where(e => e.Level == filter.Level.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new AuditLogEntryDto
            {
                Id = e.Id,
                CreatedAt = e.CreatedAt,
                UserId = e.UserId,
                UserName = e.UserName,
                UserEmail = e.UserEmail,
                Action = e.Action,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                OldValues = e.OldValues,
                NewValues = e.NewValues,
                IpAddress = e.IpAddress,
                Level = e.Level,
                Details = e.Details
            })
            .ToListAsync(cancellationToken);

        return new AuditLogListDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }
}
