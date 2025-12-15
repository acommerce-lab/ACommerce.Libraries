using ACommerce.Admin.AuditLog.DTOs;
using ACommerce.Admin.AuditLog.Entities;
using ACommerce.SharedKernel.Abstractions;
using MediatR;

namespace ACommerce.Admin.AuditLog.Commands;

public record CreateAuditLogCommand(CreateAuditLogDto Dto) : IRequest<Guid>;

public class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand, Guid>
{
    private readonly IBaseAsyncRepository<AuditLogEntry> _repository;

    public CreateAuditLogCommandHandler(IBaseAsyncRepository<AuditLogEntry> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        var entry = new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            UserId = request.Dto.UserId,
            UserName = request.Dto.UserName,
            UserEmail = request.Dto.UserEmail,
            Action = request.Dto.Action,
            EntityType = request.Dto.EntityType,
            EntityId = request.Dto.EntityId,
            OldValues = request.Dto.OldValues,
            NewValues = request.Dto.NewValues,
            IpAddress = request.Dto.IpAddress,
            UserAgent = request.Dto.UserAgent,
            Level = request.Dto.Level,
            Details = request.Dto.Details,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entry, cancellationToken);
        return entry.Id;
    }
}
