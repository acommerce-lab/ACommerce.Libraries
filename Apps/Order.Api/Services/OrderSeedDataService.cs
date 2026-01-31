using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Vendors.Entities;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Profiles.Entities;

namespace Order.Api.Services;

/// <summary>
/// خدمة بذر البيانات الأولية لتطبيق اوردر
/// </summary>
public class OrderSeedDataService
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<OrderSeedDataService> _logger;

    public OrderSeedDataService(
        IRepositoryFactory repositoryFactory,
        ILogger<OrderSeedDataService> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedCategoriesAsync();
            await SeedVendorsAsync();
            await SeedOffersAsync();
            _logger.LogInformation("Seed data completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding data");
        }
    }

    private async Task SeedCategoriesAsync()
    {
        var categoryRepo = _repositoryFactory.CreateRepository<ProductCategory>();
        var existing = await categoryRepo.GetAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Categories already exist, skipping seed");
            return;
        }

        var categories = new List<ProductCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "قهوة", NameEn = "Coffee", Description = "مشروبات القهوة الساخنة والباردة", SortOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "وجبات", NameEn = "Meals", Description = "وجبات رئيسية ومقبلات", SortOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "حلويات", NameEn = "Desserts", Description = "حلويات ومخبوزات", SortOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "مشروبات", NameEn = "Beverages", Description = "عصائر ومشروبات باردة", SortOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "عروض خاصة", NameEn = "Special Offers", Description = "عروض محدودة الوقت", SortOrder = 5 }
        };

        foreach (var category in categories)
        {
            await categoryRepo.AddAsync(category);
        }

        _logger.LogInformation("Seeded {Count} categories", categories.Count);
    }

    private async Task SeedVendorsAsync()
    {
        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();
        var existing = await vendorRepo.GetAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Vendors already exist, skipping seed");
            return;
        }

        // إنشاء بروفايلات للتجار أولاً
        var profileRepo = _repositoryFactory.CreateRepository<Profile>();

        var vendors = new List<(Profile Profile, Vendor Vendor)>
        {
            (
                new Profile
                {
                    Id = Guid.NewGuid(),
                    FirstName = "كافيه",
                    LastName = "السعادة",
                    Email = "happiness@order.app",
                    PhoneNumber = "0501234567"
                },
                new Vendor
                {
                    Id = Guid.NewGuid(),
                    Name = "كافيه السعادة",
                    NameEn = "Happiness Cafe",
                    Description = "أفضل قهوة في المدينة مع أجواء مميزة",
                    Latitude = 24.7136,
                    Longitude = 46.6753,
                    CommissionRate = 10,
                    IsActive = true,
                    WorkingHoursJson = "{\"Saturday\":{\"Open\":\"07:00\",\"Close\":\"23:00\"},\"Sunday\":{\"Open\":\"07:00\",\"Close\":\"23:00\"},\"Monday\":{\"Open\":\"07:00\",\"Close\":\"23:00\"},\"Tuesday\":{\"Open\":\"07:00\",\"Close\":\"23:00\"},\"Wednesday\":{\"Open\":\"07:00\",\"Close\":\"23:00\"},\"Thursday\":{\"Open\":\"07:00\",\"Close\":\"00:00\"},\"Friday\":{\"Open\":\"14:00\",\"Close\":\"00:00\"}}"
                }
            ),
            (
                new Profile
                {
                    Id = Guid.NewGuid(),
                    FirstName = "مطعم",
                    LastName = "الأصيل",
                    Email = "aseel@order.app",
                    PhoneNumber = "0507654321"
                },
                new Vendor
                {
                    Id = Guid.NewGuid(),
                    Name = "مطعم الأصيل",
                    NameEn = "Al Aseel Restaurant",
                    Description = "أشهى المأكولات الشعبية والعربية",
                    Latitude = 24.7256,
                    Longitude = 46.6890,
                    CommissionRate = 12,
                    IsActive = true,
                    WorkingHoursJson = "{\"Saturday\":{\"Open\":\"11:00\",\"Close\":\"23:00\"},\"Sunday\":{\"Open\":\"11:00\",\"Close\":\"23:00\"},\"Monday\":{\"Open\":\"11:00\",\"Close\":\"23:00\"},\"Tuesday\":{\"Open\":\"11:00\",\"Close\":\"23:00\"},\"Wednesday\":{\"Open\":\"11:00\",\"Close\":\"23:00\"},\"Thursday\":{\"Open\":\"11:00\",\"Close\":\"00:00\"},\"Friday\":{\"Open\":\"13:00\",\"Close\":\"00:00\"}}"
                }
            ),
            (
                new Profile
                {
                    Id = Guid.NewGuid(),
                    FirstName = "حلويات",
                    LastName = "الرياض",
                    Email = "sweets@order.app",
                    PhoneNumber = "0509876543"
                },
                new Vendor
                {
                    Id = Guid.NewGuid(),
                    Name = "حلويات الرياض",
                    NameEn = "Riyadh Sweets",
                    Description = "أشهى الحلويات العربية والغربية",
                    Latitude = 24.7000,
                    Longitude = 46.7100,
                    CommissionRate = 8,
                    IsActive = true,
                    WorkingHoursJson = "{\"Saturday\":{\"Open\":\"09:00\",\"Close\":\"22:00\"},\"Sunday\":{\"Open\":\"09:00\",\"Close\":\"22:00\"},\"Monday\":{\"Open\":\"09:00\",\"Close\":\"22:00\"},\"Tuesday\":{\"Open\":\"09:00\",\"Close\":\"22:00\"},\"Wednesday\":{\"Open\":\"09:00\",\"Close\":\"22:00\"},\"Thursday\":{\"Open\":\"09:00\",\"Close\":\"23:00\"},\"Friday\":{\"Open\":\"16:00\",\"Close\":\"23:00\"}}"
                }
            )
        };

        foreach (var (profile, vendor) in vendors)
        {
            await profileRepo.AddAsync(profile);
            vendor.ProfileId = profile.Id;
            await vendorRepo.AddAsync(vendor);
        }

        _logger.LogInformation("Seeded {Count} vendors", vendors.Count);
    }

    private async Task SeedOffersAsync()
    {
        var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();
        var existing = await listingRepo.GetAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Offers already exist, skipping seed");
            return;
        }

        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();
        var categoryRepo = _repositoryFactory.CreateRepository<ProductCategory>();
        var productRepo = _repositoryFactory.CreateRepository<Product>();

        var vendors = (await vendorRepo.GetAllAsync()).ToList();
        var categories = (await categoryRepo.GetAllAsync()).ToList();

        if (!vendors.Any() || !categories.Any())
        {
            _logger.LogWarning("No vendors or categories found, skipping offers seed");
            return;
        }

        var coffeeCategory = categories.FirstOrDefault(c => c.NameEn == "Coffee");
        var mealsCategory = categories.FirstOrDefault(c => c.NameEn == "Meals");
        var dessertsCategory = categories.FirstOrDefault(c => c.NameEn == "Desserts");
        var specialCategory = categories.FirstOrDefault(c => c.NameEn == "Special Offers");

        var happinessCafe = vendors.FirstOrDefault(v => v.NameEn == "Happiness Cafe");
        var aseelRestaurant = vendors.FirstOrDefault(v => v.NameEn == "Al Aseel Restaurant");
        var riyadhSweets = vendors.FirstOrDefault(v => v.NameEn == "Riyadh Sweets");

        var offers = new List<(Product Product, ProductListing Listing)>();

        // عروض كافيه السعادة
        if (happinessCafe != null && coffeeCategory != null)
        {
            offers.AddRange(new[]
            {
                CreateOffer("لاتيه + كرواسون", "Latte + Croissant",
                    "عرض الصباح: لاتيه مع كرواسون طازج",
                    25, 18, happinessCafe.Id, coffeeCategory.Id,
                    "ملاحظة: العرض متاح من 7 صباحاً حتى 11 صباحاً"),
                CreateOffer("2 كابتشينو", "2 Cappuccino",
                    "اثنين كابتشينو بسعر واحد ونصف",
                    36, 27, happinessCafe.Id, coffeeCategory.Id,
                    "مناسب للمشاركة مع صديق"),
                CreateOffer("آيس موكا كبير", "Large Iced Mocha",
                    "آيس موكا حجم كبير مع كريمة إضافية",
                    22, 16, happinessCafe.Id, coffeeCategory.Id, null)
            });
        }

        // عروض مطعم الأصيل
        if (aseelRestaurant != null && mealsCategory != null)
        {
            offers.AddRange(new[]
            {
                CreateOffer("وجبة كبسة دجاج", "Chicken Kabsa Meal",
                    "كبسة دجاج مع سلطة ومشروب غازي",
                    45, 35, aseelRestaurant.Id, mealsCategory.Id,
                    "الكمية محدودة - 50 وجبة يومياً"),
                CreateOffer("شاورما عربي", "Arabic Shawarma",
                    "2 شاورما + بطاطس + مشروب",
                    30, 22, aseelRestaurant.Id, mealsCategory.Id, null),
                CreateOffer("برجر لحم أنقوس", "Angus Beef Burger",
                    "برجر لحم أنقوس مع جبنة شيدر وبطاطس",
                    42, 32, aseelRestaurant.Id, mealsCategory.Id,
                    "اللحم طازج 100%")
            });
        }

        // عروض حلويات الرياض
        if (riyadhSweets != null && dessertsCategory != null)
        {
            offers.AddRange(new[]
            {
                CreateOffer("كيلو كنافة", "1KG Kunafa",
                    "كنافة نابلسية طازجة",
                    80, 60, riyadhSweets.Id, dessertsCategory.Id,
                    "تحضر عند الطلب"),
                CreateOffer("تشيز كيك + قهوة", "Cheesecake + Coffee",
                    "قطعة تشيز كيك مع قهوة تركية",
                    35, 25, riyadhSweets.Id, dessertsCategory.Id, null),
                CreateOffer("بقلاوة مشكلة", "Mixed Baklava",
                    "نصف كيلو بقلاوة مشكلة",
                    55, 42, riyadhSweets.Id, dessertsCategory.Id,
                    "تشكيلة من أفضل أنواع البقلاوة")
            });
        }

        foreach (var (product, listing) in offers)
        {
            await productRepo.AddAsync(product);
            listing.ProductId = product.Id;
            await listingRepo.AddAsync(listing);
        }

        _logger.LogInformation("Seeded {Count} offers", offers.Count);
    }

    private (Product Product, ProductListing Listing) CreateOffer(
        string name, string nameEn, string description,
        decimal originalPrice, decimal offerPrice,
        Guid vendorId, Guid categoryId, string? merchantNotes)
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = name,
            NameEn = nameEn,
            Description = description,
            CategoryId = categoryId
        };

        var listing = new ProductListing
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VendorId = vendorId,
            CategoryId = categoryId,
            Title = name,
            TitleEn = nameEn,
            Description = merchantNotes != null ? $"{description}\n\n📝 {merchantNotes}" : description,
            Price = offerPrice,
            OriginalPrice = originalPrice,
            Status = ACommerce.Catalog.Listings.Enums.ListingStatus.Active,
            IsActive = true,
            AvailableQuantity = 100,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        return (product, listing);
    }
}
