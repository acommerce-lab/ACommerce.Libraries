using ACommerce.Admin.Reports.DTOs;
using ACommerce.Profiles.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Reports.Queries;

public record GetUserActivityReportQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<UserActivityReportDto>;

public class GetUserActivityReportQueryHandler : IRequestHandler<GetUserActivityReportQuery, UserActivityReportDto>
{
    private readonly IBaseAsyncRepository<Profile> _profileRepository;

    public GetUserActivityReportQueryHandler(IBaseAsyncRepository<Profile> profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<UserActivityReportDto> Handle(GetUserActivityReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var profiles = await _profileRepository.GetAll()
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalNewUsers = profiles.Count;

        var dailyBreakdown = profiles
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new UsersByDayDto
            {
                Date = g.Key,
                NewUsers = g.Count(),
                ActiveUsers = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        var typeBreakdown = profiles
            .GroupBy(p => p.Type.ToString())
            .Select(g => new UsersByTypeDto
            {
                ProfileType = g.Key,
                Count = g.Count(),
                Percentage = totalNewUsers > 0 ? (decimal)g.Count() / totalNewUsers * 100 : 0
            })
            .ToList();

        return new UserActivityReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalNewUsers = totalNewUsers,
            TotalActiveUsers = totalNewUsers,
            DailyBreakdown = dailyBreakdown,
            TypeBreakdown = typeBreakdown
        };
    }
}
