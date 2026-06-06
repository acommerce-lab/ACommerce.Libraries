using System.Net.Http.Json;

namespace Ashare.Admin.Services;

/// <summary>
/// خدمة الدعم/المحتوى للوحة — الشكاوى + الصفحات القانونية + سجل التدقيق.
/// </summary>
public class AdminSupportService : AdminServiceBase
{
    public AdminSupportService(IConfiguration config, AdminAuthStateProvider auth) : base(config, auth) { }

    // ─── Complaints ─────────────────────────────────────────────

    public async Task<List<ComplaintAdminVm>> GetComplaintsAsync(string? status = null, string? category = null, int page = 1, int pageSize = 30)
    {
        await AuthAsync();
        var url = $"/api/complaints/admin/all?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (!string.IsNullOrEmpty(category)) url += $"&category={category}";
        try { return await Http.GetFromJsonAsync<List<ComplaintAdminVm>>(url) ?? new(); }
        catch { return new(); }
    }

    public async Task<ComplaintDetailVm?> GetComplaintAsync(Guid id)
    {
        await AuthAsync();
        try { return await Http.GetFromJsonAsync<ComplaintDetailVm>($"/api/complaints/admin/{id}"); }
        catch { return null; }
    }

    public async Task<List<ComplaintReplyVm>> GetRepliesAsync(Guid complaintId)
    {
        await AuthAsync();
        try { return await Http.GetFromJsonAsync<List<ComplaintReplyVm>>($"/api/complaints/{complaintId}/replies") ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> ReplyAsync(Guid complaintId, string message)
    {
        await AuthAsync();
        try { return (await Http.PostAsJsonAsync($"/api/complaints/{complaintId}/replies", new { message })).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> SetComplaintStatusAsync(Guid id, string status, string? resolution = null)
    {
        await AuthAsync();
        try { return (await Http.PutAsJsonAsync($"/api/complaints/admin/{id}", new { status, description = resolution })).IsSuccessStatusCode; }
        catch { return false; }
    }

    // ─── Legal Pages ────────────────────────────────────────────

    public async Task<List<LegalPageVm>> GetLegalPagesAsync()
    {
        await AuthAsync();
        try { return await Http.GetFromJsonAsync<List<LegalPageVm>>("/api/legalpages/all") ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> CreateLegalPageAsync(LegalPageVm page)
    {
        await AuthAsync();
        try { return (await Http.PostAsJsonAsync("/api/legalpages", page)).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> UpdateLegalPageAsync(Guid id, LegalPageVm page)
    {
        await AuthAsync();
        try { return (await Http.PutAsJsonAsync($"/api/legalpages/{id}", page)).IsSuccessStatusCode; }
        catch { return false; }
    }

    // ─── Audit Log ──────────────────────────────────────────────

    public async Task<AuditLogListVm> GetAuditLogsAsync(string? action = null, string? entityType = null, int page = 1, int pageSize = 30)
    {
        await AuthAsync();
        var url = $"/api/admin/auditlog?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(action)) url += $"&action={action}";
        if (!string.IsNullOrEmpty(entityType)) url += $"&entityType={entityType}";
        try { return await Http.GetFromJsonAsync<AuditLogListVm>(url) ?? new(); }
        catch { return new(); }
    }
}

// ════════════════════════════════════════════════════════════════
// View models
// ════════════════════════════════════════════════════════════════

public class ComplaintAdminVm
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int RepliesCount { get; set; }
}

public class ComplaintDetailVm
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string TicketNumber { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Category { get; set; } = "";
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ComplaintReplyVm
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsStaff { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LegalPageVm
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";
    public string TitleAr { get; set; } = "";
    public string TitleEn { get; set; } = "";
    public string Url { get; set; } = "";
    public string Icon { get; set; } = "bi-file-text";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AuditLogListVm
{
    public List<AuditLogVm> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class AuditLogVm
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public int Level { get; set; }
}
