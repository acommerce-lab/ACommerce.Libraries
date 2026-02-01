namespace ACommerce.Templates.Customer.Pages;

public class OrderListItem
{
    public string Id { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Currency { get; set; } = "SAR";
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? VendorName { get; set; }
    public string? VendorLogo { get; set; }
    public int ItemsCount { get; set; }
    public string? FirstItemImage { get; set; }
    public string? FirstItemName { get; set; }
    public bool CanReorder { get; set; }
    public bool CanCancel { get; set; }
    public bool CanTrack { get; set; }
    public bool CanReview { get; set; }
    public bool CanChat { get; set; }
}

public class OrderDetail
{
    public string Id { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "SAR";
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentMethodLabel { get; set; } = string.Empty;
    public bool IsPaid { get; set; }

    public VendorInfo? Vendor { get; set; }
    public DeliveryAddress? Address { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();

    public bool CanReorder { get; set; }
    public bool CanCancel { get; set; }
    public bool CanTrack { get; set; }
    public bool CanReview { get; set; }
    public bool CanChat { get; set; }
    public bool CanCreateTicket { get; set; }
}

public class VendorInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Phone { get; set; }
    public double? Rating { get; set; }
}

public class DeliveryAddress
{
    public string Label { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string? Notes { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class OrderItem
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemOption>? Options { get; set; }
}

public class OrderItemOption
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal? ExtraPrice { get; set; }
}

public class OrderStatusHistory
{
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Note { get; set; }
}

public static class OrderStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Preparing = "preparing";
    public const string Ready = "ready";
    public const string OutForDelivery = "out_for_delivery";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";
    public const string Refunded = "refunded";

    public static string GetLabel(string status) => status switch
    {
        Pending => "بانتظار التأكيد",
        Confirmed => "تم التأكيد",
        Preparing => "جاري التحضير",
        Ready => "جاهز للاستلام",
        OutForDelivery => "في الطريق إليك",
        Delivered => "تم التوصيل",
        Cancelled => "ملغي",
        Refunded => "تم الاسترداد",
        _ => status
    };

    public static string GetIcon(string status) => status switch
    {
        Pending => "bi-clock",
        Confirmed => "bi-check-circle",
        Preparing => "bi-fire",
        Ready => "bi-bag-check",
        OutForDelivery => "bi-truck",
        Delivered => "bi-check-circle-fill",
        Cancelled => "bi-x-circle",
        Refunded => "bi-arrow-counterclockwise",
        _ => "bi-circle"
    };

    public static string GetColor(string status) => status switch
    {
        Pending => "warning",
        Confirmed => "info",
        Preparing => "primary",
        Ready => "success",
        OutForDelivery => "info",
        Delivered => "success",
        Cancelled => "danger",
        Refunded => "secondary",
        _ => "secondary"
    };

    public static bool IsActive(string status) =>
        status is Pending or Confirmed or Preparing or Ready or OutForDelivery;
}
