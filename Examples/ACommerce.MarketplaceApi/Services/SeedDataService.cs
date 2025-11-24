using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Profiles.Entities;
using ACommerce.Vendors.Entities;
using ACommerce.Catalog.Listings.Entities;

namespace ACommerce.MarketplaceApi.Services;

/// <summary>
/// خدمة لإضافة بيانات تجريبية للاختبار
/// </summary>
public class SeedDataService
{
	private readonly IRepositoryFactory _repositoryFactory;
	private readonly MockAuthService _authService;

	public SeedDataService(IRepositoryFactory repositoryFactory, MockAuthService authService)
	{
		_repositoryFactory = repositoryFactory;
		_authService = authService;
	}

	public async Task SeedAsync()
	{
		await SeedProfilesAsync();
		await SeedVendorsAsync();
		await SeedProductsAsync();
		await SeedListingsAsync();
	}

	private async Task SeedProfilesAsync()
	{
		var repo = _repositoryFactory.GetRepository<Profile>();

		// تحقق إذا كانت البيانات موجودة
		var existing = await repo.GetAllAsync();
		if (existing.Any()) return;

		// إنشاء Profiles من المستخدمين التجريبيين
		var users = _authService.GetAllUsers();

		foreach (var user in users)
		{
			var profile = new Profile
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				Type = user.Role switch
				{
					"Customer" => ProfileType.Customer,
					"Vendor" => ProfileType.Vendor,
					"Admin" => ProfileType.Admin,
					_ => ProfileType.Customer
				},
				FullName = user.FullName,
				BusinessName = user.Role == "Vendor" ? user.FullName : null,
				IsActive = true,
				IsVerified = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await repo.AddAsync(profile);
		}
	}

	private async Task SeedVendorsAsync()
	{
		var repo = _repositoryFactory.GetRepository<Vendor>();
		var profileRepo = _repositoryFactory.GetRepository<Profile>();

		var existing = await repo.GetAllAsync();
		if (existing.Any()) return;

		// الحصول على Profile البائع
		var vendorUser = _authService.GetAllUsers().First(u => u.Role == "Vendor");
		var profiles = await profileRepo.GetAllAsync();
		var vendorProfile = profiles.FirstOrDefault(p => p.UserId == vendorUser.Id);

		if (vendorProfile == null) return;

		var vendor = new Vendor
		{
			Id = Guid.NewGuid(),
			ProfileId = vendorProfile.Id,
			StoreName = "متجر الإلكترونيات المتقدم",
			StoreSlug = "electronics-advanced",
			Description = "نوفر أحدث الأجهزة الإلكترونية بأفضل الأسعار",
			Status = VendorStatus.Active,
			CommissionType = CommissionType.Percentage,
			CommissionValue = 10.0m,
			AvailableBalance = 0,
			PendingBalance = 0,
			TotalSales = 0,
			TotalOrders = 0,
			Rating = 4.5m,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await repo.AddAsync(vendor);
	}

	private async Task SeedProductsAsync()
	{
		// سنضيف المنتجات من خلال Products API
		// هنا نتركها فارغة لأن Products API ستتولاها
		await Task.CompletedTask;
	}

	private async Task SeedListingsAsync()
	{
		var repo = _repositoryFactory.GetRepository<ProductListing>();
		var vendorRepo = _repositoryFactory.GetRepository<Vendor>();

		var existing = await repo.GetAllAsync();
		if (existing.Any()) return;

		var vendors = await vendorRepo.GetAllAsync();
		var vendor = vendors.FirstOrDefault();

		if (vendor == null) return;

		// إنشاء منتجات تجريبية (Product IDs وهمية للتجربة)
		var productIds = new[]
		{
			Guid.Parse("11111111-1111-1111-1111-111111111111"),
			Guid.Parse("22222222-2222-2222-2222-222222222222"),
			Guid.Parse("33333333-3333-3333-3333-333333333333")
		};

		var listings = new[]
		{
			new ProductListing
			{
				Id = Guid.NewGuid(),
				VendorId = vendor.Id,
				ProductId = productIds[0],
				VendorSku = "PHONE-001",
				Status = ListingStatus.Active,
				Price = 2999.00m,
				CompareAtPrice = 3499.00m,
				QuantityAvailable = 50,
				QuantityReserved = 0,
				ProcessingTime = 2,
				TotalSales = 0,
				Rating = 4.8m,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new ProductListing
			{
				Id = Guid.NewGuid(),
				VendorId = vendor.Id,
				ProductId = productIds[1],
				VendorSku = "LAPTOP-001",
				Status = ListingStatus.Active,
				Price = 4999.00m,
				CompareAtPrice = 5999.00m,
				QuantityAvailable = 30,
				QuantityReserved = 0,
				ProcessingTime = 3,
				TotalSales = 0,
				Rating = 4.7m,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new ProductListing
			{
				Id = Guid.NewGuid(),
				VendorId = vendor.Id,
				ProductId = productIds[2],
				VendorSku = "WATCH-001",
				Status = ListingStatus.Active,
				Price = 1299.00m,
				CompareAtPrice = 1699.00m,
				QuantityAvailable = 100,
				QuantityReserved = 0,
				ProcessingTime = 1,
				TotalSales = 0,
				Rating = 4.6m,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		foreach (var listing in listings)
		{
			await repo.AddAsync(listing);
		}
	}
}
