using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ACommerce.Client.Categories;
using ACommerce.Client.Products;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Orders;
using ACommerce.Client.Subscriptions;
using ACommerce.Client.Payments;
using ACommerce.Client.Core.Http;
using ACommerce.Subscriptions.DTOs;
using Ashare.Shared.Models;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة API لمنصة عشير - تربط التطبيق بالباك اند
/// تستبدل SpaceDataService بالبيانات الحقيقية
/// مع تخزين مؤقت لتحسين الأداء
/// </summary>
public class AshareApiService
{
        private readonly CategoriesClient _categoriesClient;
        private readonly CategoryAttributesClient _categoryAttributesClient;
        private readonly ProductsClient _productsClient;
        private readonly ProductListingsClient _listingsClient;
        private readonly OrdersClient _ordersClient;
        private readonly SubscriptionClient _subscriptionClient;
        private readonly PaymentsClient _paymentsClient;
        private readonly ILogger<AshareApiService> _logger;

    private readonly HashSet<Guid> _favorites = [];

        public AshareApiService(
                CategoriesClient categoriesClient,
                CategoryAttributesClient categoryAttributesClient,
                ProductsClient productsClient,
                ProductListingsClient listingsClient,
                OrdersClient ordersClient,
                SubscriptionClient subscriptionClient,
                PaymentsClient paymentsClient,
                ILogger<AshareApiService> logger)
        {
                _categoriesClient = categoriesClient;
                _categoryAttributesClient = categoryAttributesClient;
                _productsClient = productsClient;
                _listingsClient = listingsClient;
                _ordersClient = ordersClient;
                _subscriptionClient = subscriptionClient;
                _paymentsClient = paymentsClient;
                _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════════
        // Categories (Space Types)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// الحصول على جميع فئات المساحات
        /// </summary>
        public async Task<List<SpaceCategory>> GetCategoriesAsync()
        {
                try
                {
                        var categories = await _categoryAttributesClient.GetAvailableCategoriesAsync();
                        return categories?.Select(c => new SpaceCategory
                        {
                                Id = c.Id,
                                Name = c.Name,
                                NameEn = c.Name,
                                Icon = c.Icon,
                                Image = c.Image,
                                Color = "#6366F1"
                        }).ToList() ?? new List<SpaceCategory>();
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching categories");
                        return new List<SpaceCategory>();
                }
        }

        /// <summary>
        /// الحصول على الخصائص لفئة معينة
        /// </summary>
        public async Task<List<AttributeDefinitionDto>> GetAttributesForCategoryAsync(Guid categoryId)
        {
                try
                {
                        var attributes = await _categoryAttributesClient.GetAttributesForCategoryAsync(categoryId);
                        return attributes?.Select(a => new AttributeDefinitionDto
                        {
                                Id = a.Id,
                                Name = a.Name,
                                Code = a.Code,
                                Type = a.Type,
                                Description = a.Description,
                                IsRequired = a.IsRequired,
                                IsFilterable = a.IsFilterable,
                                IsVisibleInList = a.IsVisibleInList,
                                IsVisibleInDetail = a.IsVisibleInDetail,
                                SortOrder = a.SortOrder,
                                ValidationRules = a.ValidationRules,
                                DefaultValue = a.DefaultValue,
                                Values = a.Values.Select(v => new AttributeValueDto
                                {
                                        Id = v.Id,
                                        Value = v.Value,
                                        DisplayName = v.DisplayName,
                                        Code = v.Code,
                                        Description = v.Description,
                                        ColorHex = v.ColorHex,
                                        ImageUrl = v.ImageUrl,
                                        SortOrder = v.SortOrder,
                                        IsActive = v.IsActive
                                }).ToList()
                        }).ToList() ?? new List<AttributeDefinitionDto>();
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching attributes for category: {ex.Message}");
                        return new List<AttributeDefinitionDto>();
                }
        }

        // ═══════════════════════════════════════════════════════════════════
        // Spaces (ProductListings) - العروض الفعلية من المعلنين
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// الحصول على المساحات المميزة
        /// </summary>
        public async Task<List<SpaceItem>> GetFeaturedSpacesAsync()
        {
                try
                {
                        var listings = await _listingsClient.GetFeaturedAsync(10);
                        return listings?.Select(MapListingToSpaceItem).ToList() ?? new List<SpaceItem>();
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching featured spaces");
                        return new List<SpaceItem>();
                }
        }

        /// <summary>
        /// الحصول على المساحات الجديدة
        /// </summary>
        public async Task<List<SpaceItem>> GetNewSpacesAsync()
        {
                try
                {
                        var listings = await _listingsClient.GetNewAsync(10);
                        return listings?.Select(MapListingToSpaceItem).ToList() ?? new List<SpaceItem>();
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching new spaces");
                        return new List<SpaceItem>();
                }
        }

        /// <summary>
        /// الحصول على جميع المساحات
        /// </summary>
        public async Task<List<SpaceItem>> GetAllSpacesAsync()
        {
                try
                {
                        var listings = await _listingsClient.GetAllAsync();
                        return listings?.Select(MapListingToSpaceItem).ToList() ?? new List<SpaceItem>();
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching all spaces");
                        return new List<SpaceItem>();
                }
        }

        /// <summary>
        /// الحصول على مساحة بالمعرف
        /// </summary>
        public async Task<SpaceItem?> GetSpaceByIdAsync(Guid id)
        {
                try
                {
                        var listing = await _listingsClient.GetByIdAsync(id);
                        return listing != null ? MapListingToSpaceItem(listing) : null;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching space by id {Id}", id);
                        return null;
                }
        }

        /// <summary>
        /// الحصول على المساحات حسب الفئة
        /// </summary>
        public async Task<List<SpaceItem>> GetSpacesByCategoryAsync(Guid categoryId)
        {
                try
                {
                        var listings = await _listingsClient.GetByCategoryAsync(categoryId);
                        return listings?.Select(MapListingToSpaceItem).ToList() ?? new List<SpaceItem>();
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching spaces by category {CategoryId}", categoryId);
                        return new List<SpaceItem>();
                }
        }

        /// <summary>
        /// البحث في المساحات
        /// </summary>
        public async Task<List<SpaceItem>> SearchSpacesAsync(
                string? query = null,
                Guid? categoryId = null,
                string? city = null,
                decimal? maxPrice = null)
        {
                try
                {
                        var searchRequest = new SearchListingsRequest
                        {
                                Query = query,
                                CategoryId = categoryId,
                                MaxPrice = maxPrice,
                                PageSize = 50
                        };

                        var result = await _listingsClient.SearchAsync(searchRequest);
                        return result?.Items?.Select(MapListingToSpaceItem).ToList() ?? new List<SpaceItem>();
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error searching spaces: {ex.Message}");
                        return new List<SpaceItem>();
                }
        }

        /// <summary>
        /// إنشاء عرض جديد (مساحة)
        /// ملاحظة: VendorId يُستخرج تلقائياً من المستخدم المصادق في الباك اند
        /// </summary>
        public async Task<SpaceItem?> CreateSpaceAsync(CreateSpaceRequest request)
        {
                try
                {
                        var listingRequest = new CreateListingRequest
                        {
                                ProductId = Guid.Empty,
                                CategoryId = request.CategoryId,
                                Title = request.Title,
                                Description = request.Description,
                                Price = request.PricePerMonth,
                                Currency = "SAR",
                                StockQuantity = 1,
                                Images = request.Images,
                                FeaturedImage = request.Images?.FirstOrDefault(),
                                Latitude = request.Latitude,
                                Longitude = request.Longitude,
                                Address = request.Address,
                                Attributes = request.Attributes
                        };

                        var listing = await _listingsClient.CreateAsync(listingRequest);
                        
                        if (listing != null)
                        {
                                _logger.LogInformation("Created new listing {ListingId}", listing.Id);
                                return MapListingToSpaceItem(listing);
                        }
                        
                        return null;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error creating space");
                        return null;
                }
        }
        
        /// <summary>
        /// إبطال الكاش يدوياً (لا يقوم بأي عملية حالياً)
        /// </summary>
        public void InvalidateCache()
        {
        }

        /// <summary>
        /// الحصول على إعلانات المستخدم الحالي (مساحاتي)
        /// </summary>
        public async Task<List<SpaceItem>> GetMySpacesAsync(Guid vendorId)
        {
                try
                {
                        // استخدام API المخصص للمستخدم الحالي (يستخرج VendorId من التوكن)
                        var listings = await _listingsClient.GetMyListingsAsync();
                        return listings?.Select(MapListingToSpaceItem).ToList() ?? new List<SpaceItem>();
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error fetching my spaces for vendor {VendorId}", vendorId);
                        return new List<SpaceItem>();
                }
        }

        /// <summary>
        /// حذف إعلان
        /// </summary>
        public async Task<bool> DeleteSpaceAsync(Guid spaceId)
        {
                try
                {
                        await _listingsClient.DeleteAsync(spaceId);
                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error deleting space {SpaceId}", spaceId);
                        return false;
                }
        }

        // ═══════════════════════════════════════════════════════════════════
        // Favorites (Local Storage)
        // ═══════════════════════════════════════════════════════════════════

        public HashSet<Guid> GetFavorites() => _favorites;

        public void ToggleFavorite(Guid spaceId)
        {
                if (_favorites.Contains(spaceId))
                        _favorites.Remove(spaceId);
                else
                        _favorites.Add(spaceId);

                // TODO: Persist to local storage
        }

        // ═══════════════════════════════════════════════════════════════════
        // Reviews
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// الحصول على تقييمات مساحة
        /// </summary>
        public async Task<List<SpaceReview>> GetSpaceReviewsAsync(Guid spaceId)
        {
                // TODO: Implement when Reviews API is available
                return new List<SpaceReview>();
        }

        /// <summary>
        /// إضافة تقييم
        /// </summary>
        public async Task AddReviewAsync(Guid spaceId, int rating, string comment)
        {
                // TODO: Implement when Reviews API is available
                await Task.CompletedTask;
        }

        // ═══════════════════════════════════════════════════════════════════
        // Bookings (Orders)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// الحصول على حجوزات المستخدم
        /// </summary>
        public async Task<List<BookingItem>> GetBookingsAsync()
        {
                try
                {
                        var serverBookings = new List<BookingItem>();
                        try
                        {
                                var result = await _ordersClient.SearchAsync(new OrderSearchRequest());
                                serverBookings = result?.Items?.Select(MapToBookingItem).ToList() ?? new List<BookingItem>();
                        }
                        catch (Exception ex)
                        {
                                Console.WriteLine($"Error fetching server bookings: {ex.Message}");
                        }

                        // Merge with local bookings (avoid duplicates)
                        var localBookings = _localBookings.Where(lb =>
                                !serverBookings.Any(sb => sb.Id == lb.Id)).ToList();

                        var allBookings = serverBookings.Concat(localBookings).OrderByDescending(b => b.CreatedAt).ToList();
                        return allBookings;
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching bookings: {ex.Message}");
                        return _localBookings.ToList();
                }
        }

        /// <summary>
        /// إنشاء حجز جديد
        /// </summary>
        public async Task<BookingItem?> CreateBookingAsync(Guid spaceId, DateTime date, TimeOnly startTime, TimeOnly endTime)
        {
                try
                {
                        var space = await GetSpaceByIdAsync(spaceId);
                        if (space == null) return null;

                        // Create order through the API
                        // Note: The actual order creation depends on the Order API structure
                        // This is a simplified implementation

                        var booking = new BookingItem
                        {
                                Id = Guid.NewGuid(),
                                SpaceId = spaceId,
                                SpaceName = space.Name,
                                SpaceImage = space.Images.FirstOrDefault(),
                                Date = date,
                                StartTime = startTime,
                                EndTime = endTime,
                                TotalPrice = space.PricePerHour * (decimal)(endTime - startTime).TotalHours,
                                Currency = space.Currency,
                                Status = BookingStatus.Pending,
                                CreatedAt = DateTime.Now
                        };

                        return booking;
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error creating booking: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// إلغاء حجز
        /// </summary>
        public async Task CancelBookingAsync(Guid bookingId)
        {
                try
                {
                        await _ordersClient.CancelAsync(bookingId);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error cancelling booking: {ex.Message}");
                }
        }

        // Local bookings cache for storing newly created bookings
        private static readonly List<BookingItem> _localBookings = new();

        /// <summary>
        /// إنشاء حجز محلياً بعد الدفع الناجح
        /// </summary>
        public void CreateBookingLocally(BookingItem booking)
        {
                booking.CreatedAt = DateTime.Now;
                _localBookings.Add(booking);
                Console.WriteLine($"[AshareApiService] Local booking created: {booking.Id}");
        }

        /// <summary>
        /// الحصول على الحجوزات المحلية مع الحجوزات من السيرفر
        /// </summary>
        public List<BookingItem> GetLocalBookings() => _localBookings.ToList();

        // ═══════════════════════════════════════════════════════════════════
        // Stats
        // ═══════════════════════════════════════════════════════════════════

        public async Task<int> GetBookingsCountAsync()
        {
                var bookings = await GetBookingsAsync();
                return bookings.Count(b => b.Status != BookingStatus.Cancelled);
        }

        public int GetNotificationsCount() => 0; // TODO: Implement with Notifications API

        // ═══════════════════════════════════════════════════════════════════
        // Subscriptions - الاشتراكات
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// الحصول على جميع الباقات المتاحة
        /// </summary>
        public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync()
        {
                try
                {
                        var plans = await _subscriptionClient.GetPlansAsync();
                        return plans ?? new List<SubscriptionPlanDto>();
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching subscription plans: {ex.Message}");
                        return new List<SubscriptionPlanDto>();
                }
        }

        /// <summary>
        /// الحصول على باقة بواسطة المعرف
        /// </summary>
        public async Task<SubscriptionPlanDto?> GetSubscriptionPlanBySlugAsync(string slug)
        {
                try
                {
                        return await _subscriptionClient.GetPlanBySlugAsync(slug);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching plan by slug: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// الحصول على اشتراك المضيف الحالي
        /// </summary>
        public async Task<SubscriptionDto?> GetHostSubscriptionAsync(Guid vendorId)
        {
                try
                {
                        return await _subscriptionClient.GetVendorSubscriptionAsync(vendorId);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching host subscription: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// الحصول على ملخص اشتراك المضيف
        /// </summary>
        public async Task<SubscriptionSummaryDto?> GetSubscriptionSummaryAsync(Guid vendorId)
        {
                try
                {
                        return await _subscriptionClient.GetSubscriptionSummaryAsync(vendorId);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching subscription summary: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// التحقق من إمكانية إضافة مساحة جديدة
        /// </summary>
        public async Task<CanAddListingResult?> CanAddSpaceAsync(Guid vendorId)
        {
                try
                {
                        return await _subscriptionClient.CanAddListingAsync(vendorId);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error checking can add space: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// الحصول على إحصائيات الاستخدام
        /// </summary>
        public async Task<VendorUsageStatsDto?> GetUsageStatsAsync(Guid vendorId)
        {
                try
                {
                        return await _subscriptionClient.GetUsageStatsAsync(vendorId);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching usage stats: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// إنشاء اشتراك جديد
        /// </summary>
        public async Task<SubscriptionDto?> CreateSubscriptionAsync(CreateSubscriptionDto dto)
        {
                try
                {
                        return await _subscriptionClient.CreateSubscriptionAsync(dto);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error creating subscription: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// تغيير الباقة
        /// </summary>
        public async Task<SubscriptionDto?> ChangePlanAsync(Guid subscriptionId, ChangePlanDto dto)
        {
                try
                {
                        return await _subscriptionClient.ChangePlanAsync(subscriptionId, dto);
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error changing plan: {ex.Message}");
                        return null;
                }
        }

        /// <summary>
        /// الحصول على فواتير المضيف
        /// </summary>
        public async Task<List<InvoiceSummaryDto>> GetHostInvoicesAsync(Guid vendorId)
        {
                try
                {
                        var invoices = await _subscriptionClient.GetVendorInvoicesAsync(vendorId);
                        return invoices ?? new List<InvoiceSummaryDto>();
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error fetching invoices: {ex.Message}");
                        return new List<InvoiceSummaryDto>();
                }
        }

        // ═══════════════════════════════════════════════════════════════════
        // Payments - المدفوعات
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// إنشاء دفعة اشتراك والحصول على رابط الدفع
        /// </summary>
        public async Task<CreateSubscriptionPaymentResponse?> CreateSubscriptionPaymentAsync(CreateSubscriptionPaymentRequest request)
        {
                try
                {
                        // أولاً: إنشاء الاشتراك
                        var subscription = await _subscriptionClient.CreateSubscriptionAsync(new CreateSubscriptionDto
                        {
                                PlanId = request.PlanId,
                                BillingCycle = request.BillingCycle,
                                AutoRenew = true,
                                // VendorId يُستخرج من التوكن في الباك اند
                        });

                        if (subscription == null)
                        {
                                return new CreateSubscriptionPaymentResponse
                                {
                                        Success = false,
                                        Message = "فشل في إنشاء الاشتراك"
                                };
                        }

                        // إذا كانت الباقة مجانية أو فترة تجريبية
                        if (subscription.Price == 0)
                        {
                                return new CreateSubscriptionPaymentResponse
                                {
                                        Success = true,
                                        SubscriptionId = subscription.Id,
                                        RequiresPayment = false,
                                        Amount = 0,
                                        Currency = subscription.Plan?.Currency ?? "SAR",
                                        Message = "تم تفعيل الاشتراك بنجاح"
                                };
                        }

                        // ثانياً: إنشاء عملية الدفع
                        var payment = await _paymentsClient.CreatePaymentAsync(new CreatePaymentRequest
                        {
                                OrderId = subscription.Id, // استخدام معرف الاشتراك كمعرف الطلب
                                Amount = subscription.Price,
                                Currency = subscription.Plan?.Currency ?? "SAR",
                                PaymentMethod = "Noon", // أو حسب الاختيار
                                Metadata = new Dictionary<string, string>
                                {
                                        ["subscriptionId"] = subscription.Id.ToString(),
                                        ["planId"] = request.PlanId.ToString(),
                                        ["billingCycle"] = request.BillingCycle.ToString(),
                                        ["returnUrl"] = request.ReturnUrl ?? "/host/payment/callback"
                                }
                        });

                        if (payment == null || string.IsNullOrEmpty(payment.PaymentUrl))
                        {
                                return new CreateSubscriptionPaymentResponse
                                {
                                        Success = false,
                                        SubscriptionId = subscription.Id,
                                        Message = "فشل في إنشاء رابط الدفع"
                                };
                        }

                        return new CreateSubscriptionPaymentResponse
                        {
                                Success = true,
                                SubscriptionId = subscription.Id,
                                PaymentId = payment.PaymentId,
                                PaymentUrl = payment.PaymentUrl,
                                RequiresPayment = true,
                                Amount = subscription.Price,
                                Currency = subscription.Plan?.Currency ?? "SAR"
                        };
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error creating subscription payment: {ex.Message}");
                        return new CreateSubscriptionPaymentResponse
                        {
                                Success = false,
                                Message = ex.Message
                        };
                }
        }

        /// <summary>
        /// التحقق من حالة الدفع وتفعيل الاشتراك
        /// </summary>
        public async Task<VerifyPaymentResponse?> VerifySubscriptionPaymentAsync(VerifyPaymentRequest request)
        {
                try
                {
                        // التحقق من حالة الدفع
                        var payment = await _paymentsClient.GetPaymentStatusAsync(request.PaymentId);

                        if (payment == null)
                        {
                                return new VerifyPaymentResponse
                                {
                                        Success = false,
                                        Status = "NotFound",
                                        Message = "لم يتم العثور على عملية الدفع"
                                };
                        }

                        var isCompleted = payment.Status.ToLower() switch
                        {
                                "completed" => true,
                                "captured" => true,
                                "success" => true,
                                _ => false
                        };

                        if (isCompleted)
                        {
                                // تفعيل الاشتراك إذا تم الدفع بنجاح
                                if (payment.OrderId.HasValue)
                                {
                                        Console.WriteLine($"[VerifySubscriptionPayment] Activating subscription: {payment.OrderId.Value}");
                                        var activatedSubscription = await _subscriptionClient.ActivateSubscriptionAsync(
                                                payment.OrderId.Value,
                                                payment.PaymentId);

                                        if (activatedSubscription != null)
                                        {
                                                Console.WriteLine($"[VerifySubscriptionPayment] ✅ Subscription activated successfully: {activatedSubscription.Id}");
                                                return new VerifyPaymentResponse
                                                {
                                                        Success = true,
                                                        Status = "Completed",
                                                        SubscriptionId = activatedSubscription.Id,
                                                        ActivatedAt = DateTime.Now,
                                                        Message = "تم الدفع وتفعيل الاشتراك بنجاح"
                                                };
                                        }
                                        else
                                        {
                                                Console.WriteLine("[VerifySubscriptionPayment] ⚠️ Payment succeeded but subscription activation failed");
                                        }
                                }

                                // الدفع نجح لكن لم نتمكن من تفعيل الاشتراك
                                return new VerifyPaymentResponse
                                {
                                        Success = true,
                                        Status = "Completed",
                                        SubscriptionId = payment.OrderId,
                                        ActivatedAt = DateTime.Now,
                                        Message = "تم الدفع بنجاح"
                                };
                        }

                        return new VerifyPaymentResponse
                        {
                                Success = false,
                                Status = payment.Status,
                                Message = GetPaymentStatusMessage(payment.Status)
                        };
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error verifying payment: {ex.Message}");
                        return new VerifyPaymentResponse
                        {
                                Success = false,
                                Status = "Error",
                                Message = ex.Message
                        };
                }
        }

        private static string GetPaymentStatusMessage(string status)
        {
                return status.ToLower() switch
                {
                        "pending" => "في انتظار الدفع",
                        "failed" => "فشلت عملية الدفع",
                        "cancelled" => "تم إلغاء عملية الدفع",
                        "expired" => "انتهت صلاحية عملية الدفع",
                        _ => "حالة غير معروفة"
                };
        }

        // ═══════════════════════════════════════════════════════════════════
        // Booking Payment - دفع عربون الحجز
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// إنشاء دفعة حجز (عربون)
        /// </summary>
        public async Task<CreateBookingPaymentResponse?> CreateBookingPaymentAsync(CreateBookingPaymentRequest request)
        {
                try
                {
                        // إنشاء معرف الحجز
                        var bookingId = Guid.NewGuid();

                        // إنشاء عملية الدفع
                        var paymentResponse = await _paymentsClient.CreatePaymentAsync(new CreatePaymentRequest
                        {
                                OrderId = bookingId,
                                Amount = request.DepositAmount,
                                Currency = "SAR",
                                PaymentMethod = "CreditCard",
                                Metadata = new Dictionary<string, string>
                                {
                                        { "space_id", request.SpaceId.ToString() },
                                        { "type", "booking_deposit" },
                                        { "total_price", request.TotalPrice.ToString() },
                                        { "rent_type", request.RentType ?? "monthly" },
                                        { "return_url", request.ReturnUrl ?? "" }
                                }
                        });

                        if (paymentResponse == null || string.IsNullOrEmpty(paymentResponse.PaymentUrl))
                        {
                                return new CreateBookingPaymentResponse
                                {
                                        Success = false,
                                        Message = "فشل في إنشاء عملية الدفع"
                                };
                        }

                        return new CreateBookingPaymentResponse
                        {
                                Success = true,
                                BookingId = bookingId.ToString(),
                                PaymentId = paymentResponse.PaymentId,
                                PaymentUrl = paymentResponse.PaymentUrl,
                                Amount = request.DepositAmount,
                                Currency = "SAR"
                        };
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error creating booking payment: {ex.Message}");
                        return new CreateBookingPaymentResponse
                        {
                                Success = false,
                                Message = ex.Message
                        };
                }
        }

        /// <summary>
        /// التحقق من حالة دفع الحجز
        /// </summary>
        public async Task<VerifyPaymentResponse?> VerifyBookingPaymentAsync(VerifyPaymentRequest request)
        {
                try
                {
                        var payment = await _paymentsClient.GetPaymentStatusAsync(request.PaymentId);

                        if (payment == null)
                        {
                                return new VerifyPaymentResponse
                                {
                                        Success = false,
                                        Status = "NotFound",
                                        Message = "لم يتم العثور على عملية الدفع"
                                };
                        }

                        var isCompleted = payment.Status.ToLower() switch
                        {
                                "completed" => true,
                                "captured" => true,
                                "success" => true,
                                _ => false
                        };

                        if (isCompleted)
                        {
                                // TODO: تفعيل الحجز في قاعدة البيانات
                                Console.WriteLine($"[VerifyBookingPayment] ✅ Payment completed for booking");

                                return new VerifyPaymentResponse
                                {
                                        Success = true,
                                        Status = "Completed",
                                        BookingId = payment.OrderId,
                                        ActivatedAt = DateTime.Now,
                                        Message = "تم دفع العربون بنجاح"
                                };
                        }

                        return new VerifyPaymentResponse
                        {
                                Success = false,
                                Status = payment.Status,
                                Message = GetPaymentStatusMessage(payment.Status)
                        };
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"Error verifying booking payment: {ex.Message}");
                        return new VerifyPaymentResponse
                        {
                                Success = false,
                                Status = "Error",
                                Message = ex.Message
                        };
                }
        }

        // ═══════════════════════════════════════════════════════════════════
        // Mapping Helpers
        // ═══════════════════════════════════════════════════════════════════

        private static SpaceCategory MapToSpaceCategory(CategoryDto dto)
        {
                return new SpaceCategory
                {
                        Id = dto.Id,
                        Name = dto.Name,
                        NameEn = dto.Name, // TODO: Add localization
                        Icon = dto.Icon,
                        Image = dto.Image,
                        Color = "#6366F1" // Default color, can be stored in metadata
                };
        }

        /// <summary>
        /// تحويل ProductListingDto إلى SpaceItem
        /// </summary>
        private static SpaceItem MapListingToSpaceItem(ProductListingDto dto)
        {
                return new SpaceItem
                {
                        Id = dto.Id,
                        Name = dto.Title,
                        NameEn = dto.Title,
                        Description = dto.Description ?? string.Empty,
                        CategoryId = dto.CategoryId,
                        CategoryName = dto.CategoryName,
                        PricePerHour = dto.Price,
                        PricePerDay = dto.Price * 8,
                        PricePerMonth = dto.Price * 30,
                        Currency = dto.Currency,
                        Images = dto.Images.Any() ? dto.Images : (dto.FeaturedImage != null ? new List<string> { dto.FeaturedImage } : new List<string>()),
                        Location = dto.Address,
                        City = dto.City,
                        Latitude = dto.Latitude,
                        Longitude = dto.Longitude,
                        IsNew = dto.IsNew,
                        IsFeatured = dto.IsFeatured,
                        Rating = dto.AverageRating,
                        ReviewsCount = dto.RatingsCount,
                        ViewCount = dto.ViewCount,
                        Attributes = dto.Attributes,
                        CreatedAt = dto.CreatedAt,
                        // ربط المالك من بيانات البائع
                        OwnerId = dto.VendorId,
                        OwnerName = dto.VendorName ?? string.Empty
                };
        }

        private static BookingItem MapToBookingItem(OrderDto dto)
        {
                return new BookingItem
                {
                        Id = dto.Id,
                        SpaceId = Guid.Empty, // TODO: Get from order items
                        SpaceName = dto.OrderNumber ?? "حجز",
                        Date = dto.CreatedAt,
                        TotalPrice = dto.TotalAmount,
                        Currency = dto.Currency ?? "ر.س",
                        Status = MapOrderStatus(dto.Status),
                        CreatedAt = dto.CreatedAt
                };
        }

        private static BookingStatus MapOrderStatus(string? status)
        {
                return status?.ToLower() switch
                {
                        "pending" => BookingStatus.Pending,
                        "confirmed" or "processing" => BookingStatus.Confirmed,
                        "cancelled" => BookingStatus.Cancelled,
                        "completed" or "delivered" => BookingStatus.Completed,
                        _ => BookingStatus.Pending
                };
        }
}

// ═══════════════════════════════════════════════════════════════════
// DTOs from API
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// تعريف الخاصية من الباك اند
/// </summary>
public class AttributeDefinitionDto
{
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "Text";
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsVisibleInList { get; set; }
        public bool IsVisibleInDetail { get; set; }
        public int SortOrder { get; set; }
        public string? ValidationRules { get; set; }
        public string? DefaultValue { get; set; }
        public List<AttributeValueDto> Values { get; set; } = new();
}

/// <summary>
/// قيمة الخاصية من الباك اند
/// </summary>
public class AttributeValueDto
{
        public Guid Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? ColorHex { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
}

/// <summary>
/// طلب إنشاء مساحة
/// </summary>
public class CreateSpaceRequest
{
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerMonth { get; set; }
        public List<string> Images { get; set; } = new();
        public Dictionary<string, object> Attributes { get; set; } = new();
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
}
