using ACommerce.Locations.Abstractions.DTOs;

namespace ACommerce.Locations.Abstractions.Contracts;

/// <summary>
/// خدمة إدارة العناوين
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// الحصول على عناوين كيان
    /// </summary>
    Task<List<AddressResponseDto>> GetAddressesByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default);

    /// <summary>
    /// الحصول على عنوان بالمعرف
    /// </summary>
    Task<AddressResponseDto?> GetAddressByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// الحصول على العنوان الافتراضي لكيان
    /// </summary>
    Task<AddressResponseDto?> GetDefaultAddressAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default);

    /// <summary>
    /// إنشاء عنوان جديد
    /// </summary>
    Task<AddressResponseDto> CreateAddressAsync(
        CreateAddressDto dto,
        CancellationToken ct = default);

    /// <summary>
    /// تحديث عنوان
    /// </summary>
    Task<AddressResponseDto> UpdateAddressAsync(
        Guid id,
        UpdateAddressDto dto,
        CancellationToken ct = default);

    /// <summary>
    /// حذف عنوان
    /// </summary>
    Task DeleteAddressAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تعيين عنوان كافتراضي
    /// </summary>
    Task SetDefaultAddressAsync(Guid id, CancellationToken ct = default);
}
