namespace ACommerce.Templates.Customer.Pages;

public class TicketListItem
{
    public string Id { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
    public string StatusLabel { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UnreadReplies { get; set; }
    public string? LinkedOrderId { get; set; }
    public string? LinkedOrderNumber { get; set; }
}

public class TicketMessage
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsFromSupport { get; set; }
    public string? SupportAgentName { get; set; }
    public List<TicketAttachment>? Attachments { get; set; }
}

public class TicketAttachment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class TicketDetail
{
    public string Id { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? LinkedOrderId { get; set; }
    public string? LinkedOrderNumber { get; set; }
    public List<TicketMessage> Messages { get; set; } = new();
}

public class CreateTicketArgs
{
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkedOrderId { get; set; }
}

public class TicketReplyArgs
{
    public string TicketId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string>? AttachmentIds { get; set; }
}

public class TicketCategory
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public static class TicketCategories
{
    public static readonly List<TicketCategory> All = new()
    {
        new() { Value = "order_issue", Label = "مشكلة في الطلب", Icon = "bi-box-seam" },
        new() { Value = "payment", Label = "مشكلة في الدفع", Icon = "bi-credit-card" },
        new() { Value = "refund", Label = "طلب استرداد", Icon = "bi-cash-stack" },
        new() { Value = "app_bug", Label = "مشكلة تقنية", Icon = "bi-bug" },
        new() { Value = "suggestion", Label = "اقتراح أو ملاحظة", Icon = "bi-lightbulb" },
        new() { Value = "other", Label = "أخرى", Icon = "bi-three-dots" }
    };

    public static string GetLabel(string value) =>
        All.FirstOrDefault(c => c.Value == value)?.Label ?? value;

    public static string GetIcon(string value) =>
        All.FirstOrDefault(c => c.Value == value)?.Icon ?? "bi-tag";
}

public static class TicketStatuses
{
    public const string Open = "open";
    public const string InProgress = "in_progress";
    public const string Resolved = "resolved";
    public const string Closed = "closed";

    public static string GetLabel(string status) => status switch
    {
        Open => "مفتوحة",
        InProgress => "قيد المعالجة",
        Resolved => "تم الحل",
        Closed => "مغلقة",
        _ => status
    };

    public static string GetColor(string status) => status switch
    {
        Open => "warning",
        InProgress => "info",
        Resolved => "success",
        Closed => "secondary",
        _ => "secondary"
    };
}
