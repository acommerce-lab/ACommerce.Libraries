using System.Net.Http.Json;
using ACommerce.Client.Categories;
using ACommerce.Client.Products;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Orders;
using ACommerce.Client.Core.Http;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة API لمنصة عشير - تربط التطبيق بالباك اند
/// تستبدل SpaceDataService بالبيانات الحقيقية
/// </summary>
public class AshareApiService
{
	private readonly CategoriesClient _categoriesClient;
	private readonly CategoryAttributesClient _categoryAttributesClient;
	private readonly ProductsClient _productsClient;
	private readonly ProductListingsClient _listingsClient;
	private readonly OrdersClient _ordersClient;

    // Local cache for favorites (can be persisted to local storage)
    private readonly HashSet<Guid> _favorites = [];

	public AshareApiService(
		CategoriesClient categoriesClient,
		CategoryAttributesClient categoryAttributesClient,
		ProductsClient productsClient,
		ProductListingsClient listingsClient,
		OrdersClient ordersClient)
	{
		_categoriesClient = categoriesClient;
		_categoryAttributesClient = categoryAttributesClient;
		_productsClient = productsClient;
		_listingsClient = listingsClient;
		_ordersClient = ordersClient;
	}

	// ═══════════════════════════════════════════════════════════════════
	// Categories (Space Types)
	// ═══════════════════════════════════════════════════════════════════

	/// <summary>
	/// الحصول على جميع فئات المساحات
	/// يستخدم CategoryAttributesClient للحصول على الفئات بالـ IDs الصحيحة
	/// </summary>
	public async Task<List<SpaceCategory>> GetCategoriesAsync()
	{
		try
		{
			// استخدام CategoryAttributesClient بدلاً من CategoriesClient
			// لضمان تطابق الـ IDs مع CategoryAttributeMappings
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
			Console.WriteLine($"Error fetching categories: {ex.Message}");
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
			Console.WriteLine($"Error fetching featured spaces: {ex.Message}");
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
			Console.WriteLine($"Error fetching new spaces: {ex.Message}");
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
			Console.WriteLine($"Error fetching all spaces: {ex.Message}");
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
			Console.WriteLine($"Error fetching space by id: {ex.Message}");
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
			Console.WriteLine($"Error fetching spaces by category: {ex.Message}");
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
			// إنشاء عرض ProductListing
			// ملاحظة: VendorId يُعيَّن تلقائياً في الباك اند من التوكن
			var listingRequest = new CreateListingRequest
			{
				// VendorId: يُستخرج تلقائياً من التوكن في الباك اند
				ProductId = Guid.Empty, // TODO: Get product template based on category
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
			return listing != null ? MapListingToSpaceItem(listing) : null;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error creating space: {ex.Message}");
			return null;
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
			var result = await _ordersClient.SearchAsync(new OrderSearchRequest());
			return result?.Items?.Select(MapToBookingItem).ToList() ?? new List<BookingItem>();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error fetching bookings: {ex.Message}");
			return new List<BookingItem>();
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
			CreatedAt = dto.CreatedAt
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
