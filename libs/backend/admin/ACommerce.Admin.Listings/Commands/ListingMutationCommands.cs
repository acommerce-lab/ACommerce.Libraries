using ACommerce.Admin.Listings.DTOs;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Catalog.Listings.Enums;
using ACommerce.SharedKernel.Abstractions.Repositories;
using MediatR;

namespace ACommerce.Admin.Listings.Commands;

// ════════════════════════════════════════════════════════════════════
// Change status (approve / reject / suspend / restore-to-pending …)
// ════════════════════════════════════════════════════════════════════

public record ChangeListingStatusCommand(Guid ListingId, ListingStatus NewStatus, string? Reason = null)
    : IRequest<AdminListingMutationResult>;

public class ChangeListingStatusCommandHandler(IBaseAsyncRepository<ProductListing> repository)
    : IRequestHandler<ChangeListingStatusCommand, AdminListingMutationResult>
{
    public async Task<AdminListingMutationResult> Handle(ChangeListingStatusCommand request, CancellationToken ct)
    {
        var listing = await repository.GetByIdAsync(request.ListingId, ct);
        if (listing is null) return AdminListingMutationResult.NotFound(request.ListingId);

        var oldStatus = listing.Status;
        var oldActive = listing.IsActive;

        listing.Status = request.NewStatus;
        listing.UpdatedAt = DateTime.UtcNow;

        // عند الرفض/التعليق نُلغي تفعيل العرض حتى لا يظهر للعملاء.
        if (request.NewStatus is ListingStatus.Rejected or ListingStatus.Suspended)
            listing.IsActive = false;
        // عند الموافقة (Active) نُعيد التفعيل.
        else if (request.NewStatus == ListingStatus.Active)
            listing.IsActive = true;

        // حفظ سبب التغيير في Metadata (لا يوجد عمود مخصّص في الكيان).
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            listing.Metadata ??= new Dictionary<string, string>();
            listing.Metadata["admin_status_reason"] = request.Reason;
            listing.Metadata["admin_status_changed_at"] = DateTime.UtcNow.ToString("O");
        }

        await repository.UpdateAsync(listing, ct);

        return new AdminListingMutationResult
        {
            Success = true,
            ListingId = listing.Id,
            Title = listing.Title,
            OldStatus = oldStatus,
            NewStatus = listing.Status,
            OldIsActive = oldActive,
            NewIsActive = listing.IsActive
        };
    }
}

// ════════════════════════════════════════════════════════════════════
// Toggle active flag (visibility) without changing the review status
// ════════════════════════════════════════════════════════════════════

public record SetListingActiveCommand(Guid ListingId, bool IsActive)
    : IRequest<AdminListingMutationResult>;

public class SetListingActiveCommandHandler(IBaseAsyncRepository<ProductListing> repository)
    : IRequestHandler<SetListingActiveCommand, AdminListingMutationResult>
{
    public async Task<AdminListingMutationResult> Handle(SetListingActiveCommand request, CancellationToken ct)
    {
        var listing = await repository.GetByIdAsync(request.ListingId, ct);
        if (listing is null) return AdminListingMutationResult.NotFound(request.ListingId);

        var oldActive = listing.IsActive;
        listing.IsActive = request.IsActive;
        listing.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(listing, ct);

        return new AdminListingMutationResult
        {
            Success = true,
            ListingId = listing.Id,
            Title = listing.Title,
            OldStatus = listing.Status,
            NewStatus = listing.Status,
            OldIsActive = oldActive,
            NewIsActive = listing.IsActive
        };
    }
}

// ════════════════════════════════════════════════════════════════════
// Soft delete a listing
// ════════════════════════════════════════════════════════════════════

public record DeleteListingCommand(Guid ListingId) : IRequest<AdminListingMutationResult>;

public class DeleteListingCommandHandler(IBaseAsyncRepository<ProductListing> repository)
    : IRequestHandler<DeleteListingCommand, AdminListingMutationResult>
{
    public async Task<AdminListingMutationResult> Handle(DeleteListingCommand request, CancellationToken ct)
    {
        var listing = await repository.GetByIdAsync(request.ListingId, ct);
        if (listing is null) return AdminListingMutationResult.NotFound(request.ListingId);

        await repository.SoftDeleteAsync(listing, ct);

        return new AdminListingMutationResult
        {
            Success = true,
            ListingId = listing.Id,
            Title = listing.Title,
            OldStatus = listing.Status,
            NewStatus = listing.Status,
            OldIsActive = listing.IsActive,
            NewIsActive = false,
            Message = "Listing soft-deleted"
        };
    }
}
