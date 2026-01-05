namespace ACommerce.Bookings.Enums;

/// <summary>
/// حالات الضمان (Escrow)
/// </summary>
public enum EscrowStatus
{
    /// <summary>
    /// لا يوجد ضمان بعد
    /// </summary>
    None = 0,

    /// <summary>
    /// محتجز - المبلغ محتجز في الضمان
    /// </summary>
    Held = 1,

    /// <summary>
    /// محرر - تم تحرير المبلغ للمالك
    /// </summary>
    Released = 2,

    /// <summary>
    /// مسترد - تم استرداد المبلغ للمستأجر
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// معلق للنزاع - محتجز حتى حل النزاع
    /// </summary>
    HeldForDispute = 4,

    /// <summary>
    /// محرر جزئياً - تم تحرير جزء من المبلغ
    /// </summary>
    PartiallyReleased = 5
}
