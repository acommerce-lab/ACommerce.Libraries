using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Orders.Entities;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Vendors.Entities;
using Order.Api.Models;
using System.Text.Json;

namespace Order.Api.Controllers;

/// <summary>
/// إدارة طلبات العميل مع دعم التوصيل للسيارة ومشاركة الموقع
/// </summary>
[ApiController]
[Route("api/customer/orders")]
[Authorize]
public class CustomerOrdersController : ControllerBase
{
    private readonly IBaseAsyncRepository<ACommerce.Orders.Entities.Order> _orderRepository;
    private readonly IBaseAsyncRepository<OrderItem> _orderItemRepository;
    private readonly IBaseAsyncRepository<ProductListing> _listingRepository;
    private readonly IBaseAsyncRepository<Vendor> _vendorRepository;
    private readonly ILogger<CustomerOrdersController> _logger;

    public CustomerOrdersController(
        IBaseAsyncRepository<ACommerce.Orders.Entities.Order> orderRepository,
        IBaseAsyncRepository<OrderItem> orderItemRepository,
        IBaseAsyncRepository<ProductListing> listingRepository,
        IBaseAsyncRepository<Vendor> vendorRepository,
        ILogger<CustomerOrdersController> logger)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _listingRepository = listingRepository;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    /// <summary>
    /// إنشاء طلب جديد
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "غير مصرح" });

        // التحقق من صحة البيانات
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { Message = "يجب إضافة عنصر واحد على الأقل" });

        // التحقق من العروض والمتجر
        var firstListing = await _listingRepository.GetByIdAsync(request.Items.First().OfferId);
        if (firstListing == null)
            return BadRequest(new { Message = "العرض غير موجود" });

        var vendor = await _vendorRepository.GetByIdAsync(firstListing.VendorId);
        if (vendor == null)
            return BadRequest(new { Message = "المتجر غير موجود" });

        // حساب المجموع وإنشاء عناصر الطلب
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var listing = await _listingRepository.GetByIdAsync(item.OfferId);
            if (listing == null)
                continue;

            if (listing.VendorId != firstListing.VendorId)
                return BadRequest(new { Message = "جميع العناصر يجب أن تكون من نفس المتجر" });

            var itemTotal = listing.Price * item.Quantity;
            subtotal += itemTotal;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                VendorId = listing.VendorId,
                ProductId = listing.ProductId,
                ProductName = listing.Title,
                Quantity = item.Quantity,
                UnitPrice = listing.Price,
                CreatedAt = DateTime.UtcNow
            });
        }

        // إنشاء الطلب
        var orderId = Guid.NewGuid();
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{orderId.ToString()[..6].ToUpper()}";

        // تخزين بيانات التوصيل في الـ Metadata
        var metadata = new Dictionary<string, string>
        {
            ["delivery_type"] = request.DeliveryType.ToString(),
            ["payment_method"] = request.PaymentMethod.ToString()
        };

        if (request.DeliveryLocation != null)
        {
            metadata["delivery_latitude"] = request.DeliveryLocation.Latitude.ToString();
            metadata["delivery_longitude"] = request.DeliveryLocation.Longitude.ToString();
            metadata["delivery_description"] = request.DeliveryLocation.LocationDescription ?? "";
        }

        if (request.CarInfo != null)
        {
            metadata["car_model"] = request.CarInfo.CarModel ?? "";
            metadata["car_color"] = request.CarInfo.CarColor ?? "";
            metadata["car_plate"] = request.CarInfo.PlateNumber ?? "";
        }

        // Store cash amount if provided
        if (request.CashAmount.HasValue)
        {
            metadata["cash_amount"] = request.CashAmount.Value.ToString();
            metadata["change_amount"] = (request.CashAmount.Value - subtotal).ToString();
        }

        var order = new ACommerce.Orders.Entities.Order
        {
            Id = orderId,
            OrderNumber = orderNumber,
            CustomerId = userId,
            VendorId = vendor.Id,
            Subtotal = subtotal,
            Total = subtotal,
            Currency = "SAR",
            Status = ACommerce.Orders.Enums.OrderStatus.Pending,
            CustomerNotes = request.CustomerNotes,
            PaymentMethod = request.PaymentMethod.ToString(),
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order);

        // إضافة العناصر
        foreach (var item in orderItems)
        {
            item.OrderId = order.Id;
            await _orderItemRepository.AddAsync(item);
        }

        _logger.LogInformation("Order {OrderId} created by {CustomerId} for vendor {VendorId}",
            order.Id, userId, vendor.Id);

        return Ok(new
        {
            OrderId = order.Id,
            OrderNumber = orderNumber,
            TotalAmount = subtotal,
            Status = "pending",
            DeliveryType = request.DeliveryType.ToString(),
            PaymentMethod = request.PaymentMethod.ToString(),
            Vendor = new
            {
                vendor.Id,
                Name = vendor.StoreName,
                Phone = vendor.Metadata.GetValueOrDefault("phone", "")
            },
            Message = "تم إنشاء الطلب بنجاح"
        });
    }

    /// <summary>
    /// تحديث موقع التوصيل (Live Location)
    /// </summary>
    [HttpPut("{orderId}/location")]
    public async Task<IActionResult> UpdateDeliveryLocation(Guid orderId, [FromBody] UpdateLocationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "غير مصرح" });

        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return NotFound(new { Message = "الطلب غير موجود" });

        if (order.CustomerId != userId)
            return Forbid();

        // تحديث الموقع في Metadata
        order.Metadata["delivery_latitude"] = request.Latitude.ToString();
        order.Metadata["delivery_longitude"] = request.Longitude.ToString();
        order.Metadata["delivery_description"] = request.LocationDescription ?? "";
        order.Metadata["location_updated_at"] = DateTime.UtcNow.ToString("o");
        order.Metadata["is_live_location"] = "true";

        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Location updated for order {OrderId}", orderId);

        return Ok(new { Message = "تم تحديث الموقع" });
    }

    /// <summary>
    /// الحصول على طلبات العميل
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "غير مصرح" });

        var orders = await _orderRepository.GetAllWithPredicateAsync(
            o => o.CustomerId == userId && !o.IsDeleted);

        var orderedList = orders.OrderByDescending(o => o.CreatedAt).ToList();

        var vendors = (await _vendorRepository.ListAllAsync()).ToDictionary(v => v.Id);

        var results = new List<object>();
        foreach (var order in orderedList.Skip((page - 1) * pageSize).Take(pageSize))
        {
            vendors.TryGetValue(order.VendorId ?? Guid.Empty, out var vendor);

            results.Add(new
            {
                order.Id,
                order.OrderNumber,
                TotalAmount = order.Total,
                Status = MapOrderStatus(order.Status),
                StatusAr = MapOrderStatusArabic(order.Status),
                order.CreatedAt,
                DeliveryType = order.Metadata.GetValueOrDefault("delivery_type", "Pickup"),
                PaymentMethod = order.Metadata.GetValueOrDefault("payment_method", "Cash"),
                CashAmount = decimal.TryParse(order.Metadata.GetValueOrDefault("cash_amount", ""), out var cashAmt) ? cashAmt : (decimal?)null,
                ChangeAmount = decimal.TryParse(order.Metadata.GetValueOrDefault("change_amount", ""), out var changeAmt) ? changeAmt : (decimal?)null,
                Vendor = vendor != null ? new
                {
                    vendor.Id,
                    Name = vendor.StoreName,
                    LogoUrl = vendor.Metadata.GetValueOrDefault("logo_url", ""),
                    ContactPhone = vendor.Metadata.GetValueOrDefault("phone", "")
                } : null
            });
        }

        return Ok(new
        {
            Items = results,
            Total = orderedList.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// الحصول على تفاصيل طلب
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetails(Guid orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "غير مصرح" });

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return NotFound(new { Message = "الطلب غير موجود" });

        if (order.CustomerId != userId)
            return Forbid();

        var vendor = order.VendorId.HasValue
            ? await _vendorRepository.GetByIdAsync(order.VendorId.Value)
            : null;

        var items = await _orderItemRepository.GetAllWithPredicateAsync(i => i.OrderId == orderId);

        var itemDetails = new List<object>();
        foreach (var item in items)
        {
            var listing = await _listingRepository.GetByIdAsync(item.ListingId);

            itemDetails.Add(new
            {
                item.Id,
                OfferTitle = listing?.Title ?? item.ProductName,
                item.Quantity,
                Price = item.UnitPrice,
                TotalPrice = item.Total
            });
        }

        return Ok(new
        {
            order.Id,
            order.OrderNumber,
            TotalAmount = order.Total,
            Status = MapOrderStatus(order.Status),
            StatusAr = MapOrderStatusArabic(order.Status),
            order.CreatedAt,
            Notes = order.CustomerNotes,
            DeliveryType = order.Metadata.GetValueOrDefault("delivery_type", "Pickup"),
            PaymentMethod = order.Metadata.GetValueOrDefault("payment_method", "Cash"),
            CashAmount = decimal.TryParse(order.Metadata.GetValueOrDefault("cash_amount", ""), out var cashAmt) ? cashAmt : (decimal?)null,
            ChangeAmount = decimal.TryParse(order.Metadata.GetValueOrDefault("change_amount", ""), out var changeAmt) ? changeAmt : (decimal?)null,
            DeliveryLocation = new
            {
                Latitude = double.TryParse(order.Metadata.GetValueOrDefault("delivery_latitude", "0"), out var lat) ? lat : 0,
                Longitude = double.TryParse(order.Metadata.GetValueOrDefault("delivery_longitude", "0"), out var lng) ? lng : 0,
                Description = order.Metadata.GetValueOrDefault("delivery_description", "")
            },
            CarInfo = new
            {
                CarModel = order.Metadata.GetValueOrDefault("car_model", ""),
                CarColor = order.Metadata.GetValueOrDefault("car_color", ""),
                PlateNumber = order.Metadata.GetValueOrDefault("car_plate", "")
            },
            Vendor = vendor != null ? new
            {
                vendor.Id,
                Name = vendor.StoreName,
                ContactPhone = vendor.Metadata.GetValueOrDefault("phone", ""),
                Latitude = vendor.Metadata.TryGetValue("latitude", out var vlat) && double.TryParse(vlat, out var vlatD) ? vlatD : (double?)null,
                Longitude = vendor.Metadata.TryGetValue("longitude", out var vlng) && double.TryParse(vlng, out var vlngD) ? vlngD : (double?)null
            } : null,
            Items = itemDetails
        });
    }

    /// <summary>
    /// إلغاء طلب
    /// </summary>
    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "غير مصرح" });

        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return NotFound(new { Message = "الطلب غير موجود" });

        if (order.CustomerId != userId)
            return Forbid();

        // لا يمكن إلغاء طلب قيد التحضير أو جاهز
        if (order.Status != ACommerce.Orders.Enums.OrderStatus.Pending)
            return BadRequest(new { Message = "لا يمكن إلغاء هذا الطلب" });

        order.Status = ACommerce.Orders.Enums.OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} cancelled by customer", orderId);

        return Ok(new { Message = "تم إلغاء الطلب" });
    }

    private static string MapOrderStatus(ACommerce.Orders.Enums.OrderStatus status) => status switch
    {
        ACommerce.Orders.Enums.OrderStatus.Pending => "pending",
        ACommerce.Orders.Enums.OrderStatus.Confirmed => "accepted",
        ACommerce.Orders.Enums.OrderStatus.Processing => "preparing",
        ACommerce.Orders.Enums.OrderStatus.Shipped => "ready",
        ACommerce.Orders.Enums.OrderStatus.Delivered => "delivered",
        ACommerce.Orders.Enums.OrderStatus.Cancelled => "cancelled",
        _ => "unknown"
    };

    private static string MapOrderStatusArabic(ACommerce.Orders.Enums.OrderStatus status) => status switch
    {
        ACommerce.Orders.Enums.OrderStatus.Pending => "في الانتظار",
        ACommerce.Orders.Enums.OrderStatus.Confirmed => "تم القبول",
        ACommerce.Orders.Enums.OrderStatus.Processing => "قيد التحضير",
        ACommerce.Orders.Enums.OrderStatus.Shipped => "جاهز للاستلام",
        ACommerce.Orders.Enums.OrderStatus.Delivered => "تم التسليم",
        ACommerce.Orders.Enums.OrderStatus.Cancelled => "ملغي",
        _ => "غير معروف"
    };
}

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public DeliveryType DeliveryType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? CustomerNotes { get; set; }
    public DeliveryLocation? DeliveryLocation { get; set; }
    public CarInfo? CarInfo { get; set; }
    public decimal? CashAmount { get; set; }
}

public class OrderItemRequest
{
    public Guid OfferId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationDescription { get; set; }
}
