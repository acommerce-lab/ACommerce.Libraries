using System.Net.Http.Json;

namespace Ashare.Admin.Services;

/// <summary>
/// خدمة إشراف اللوحة على العروض والحجوزات — تستدعي نقاط الإدارة الجديدة.
/// </summary>
public class AdminModerationService : AdminServiceBase
{
    public AdminModerationService(IConfiguration config, AdminAuthStateProvider auth) : base(config, auth) { }

    // ─── Listings moderation ────────────────────────────────────

    public async Task<bool> ApproveListingAsync(Guid id)
        => await PostVoid($"/api/admin/listings/{id}/approve", new { });

    public async Task<bool> RejectListingAsync(Guid id, string reason)
        => await PostVoid($"/api/admin/listings/{id}/reject", new { reason });

    public async Task<bool> SuspendListingAsync(Guid id, string? reason = null)
        => await PostVoid($"/api/admin/listings/{id}/suspend", new { reason });

    public async Task<bool> SetListingActiveAsync(Guid id, bool isActive)
    {
        await AuthAsync();
        try
        {
            var resp = await Http.PatchAsJsonAsync($"/api/admin/listings/{id}/active", new { isActive });
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteListingAsync(Guid id)
    {
        await AuthAsync();
        try { return (await Http.DeleteAsync($"/api/admin/listings/{id}")).IsSuccessStatusCode; }
        catch { return false; }
    }

    // ─── Bookings ───────────────────────────────────────────────

    public async Task<BookingAdminListResult> GetBookingsAsync(string? status = null, string? search = null, int page = 1, int pageSize = 20)
    {
        await AuthAsync();
        var url = $"/api/bookings/admin/all?pageNumber={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        try
        {
            return await Http.GetFromJsonAsync<BookingAdminListResult>(url) ?? new();
        }
        catch { return new(); }
    }

    public async Task<bool> ConfirmBookingAsync(Guid id)
        => await PostVoid($"/api/bookings/{id}/confirm", new { });

    public async Task<bool> RejectBookingAsync(Guid id, string reason)
        => await PostVoid($"/api/bookings/{id}/reject", new { reason });

    public async Task<bool> CancelBookingAsync(Guid id, string reason)
        => await PostVoid($"/api/bookings/{id}/cancel", new { reason });

    public async Task<bool> RefundBookingAsync(Guid id, string reason)
        => await PostVoid($"/api/bookings/{id}/refund-escrow", new { reason });

    public async Task<bool> ReleaseEscrowAsync(Guid id)
        => await PostVoid($"/api/bookings/{id}/release-escrow", new { });

    // ─── helper ─────────────────────────────────────────────────

    private async Task<bool> PostVoid(string url, object body)
    {
        await AuthAsync();
        try { return (await Http.PostAsJsonAsync(url, body)).IsSuccessStatusCode; }
        catch { return false; }
    }
}

public class BookingAdminListResult
{
    public List<BookingAdminVm> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class BookingAdminVm
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public string? CustomerId { get; set; }
    public Guid HostId { get; set; }
    public string? SpaceName { get; set; }
    public string? SpaceLocation { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DepositAmount { get; set; }
    public string? Currency { get; set; }
    public string Status { get; set; } = "";
    public string EscrowStatus { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
