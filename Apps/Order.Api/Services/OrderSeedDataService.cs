using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Vendors.Entities;
using ACommerce.Vendors.Enums;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.Enums;
using System.Text.Json;

namespace Order.Api.Services;

/// <summary>
/// خدمة بذر البيانات الأولية لتطبيق اوردر
/// </summary>
public class OrderSeedDataService
{
    private readonly IBaseAsyncRepository<ProductCategory> _categoryRepository;
    private readonly IBaseAsyncRepository<Vendor> _vendorRepository;
    private readonly IBaseAsyncRepository<Profile> _profileRepository;
    private readonly IBaseAsyncRepository<Product> _productRepository;
    private readonly IBaseAsyncRepository<ProductListing> _listingRepository;
    private readonly ILogger<OrderSeedDataService> _logger;

    public OrderSeedDataService(
        IBaseAsyncRepository<ProductCategory> categoryRepository,
        IBaseAsyncRepository<Vendor> vendorRepository,
        IBaseAsyncRepository<Profile> profileRepository,
        IBaseAsyncRepository<Product> productRepository,
        IBaseAsyncRepository<ProductListing> listingRepository,
        ILogger<OrderSeedDataService> logger)
    {
        _categoryRepository = categoryRepository;
        _vendorRepository = vendorRepository;
        _profileRepository = profileRepository;
        _productRepository = productRepository;
        _listingRepository = listingRepository;
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
        var existing = await _categoryRepository.ListAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Categories already exist, skipping seed");
            return;
        }

        var categories = new List<ProductCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "قهوة", Slug = "coffee", Description = "مشروبات القهوة الساخنة والباردة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "وجبات", Slug = "meals", Description = "وجبات رئيسية ومقبلات", SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "حلويات", Slug = "desserts", Description = "حلويات ومخبوزات", SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "مشروبات", Slug = "beverages", Description = "عصائر ومشروبات باردة", SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "عروض خاصة", Slug = "special-offers", Description = "عروض محدودة الوقت", SortOrder = 5, CreatedAt = DateTime.UtcNow }
        };

        foreach (var category in categories)
        {
            await _categoryRepository.AddAsync(category);
        }

        _logger.LogInformation("Seeded {Count} categories", categories.Count);
    }

    private async Task SeedVendorsAsync()
    {
        var existing = await _vendorRepository.ListAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Vendors already exist, skipping seed");
            return;
        }

        var workingHoursJson = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["Saturday"] = new { Open = "07:00", Close = "23:00" },
            ["Sunday"] = new { Open = "07:00", Close = "23:00" },
            ["Monday"] = new { Open = "07:00", Close = "23:00" },
            ["Tuesday"] = new { Open = "07:00", Close = "23:00" },
            ["Wednesday"] = new { Open = "07:00", Close = "23:00" },
            ["Thursday"] = new { Open = "07:00", Close = "00:00" },
            ["Friday"] = new { Open = "14:00", Close = "00:00" }
        });

        var vendorsData = new[]
        {
            new
            {
                FullName = "كافيه السعادة",
                StoreName = "كافيه السعادة",
                StoreSlug = "happiness-cafe",
                Description = "أفضل قهوة في المدينة مع أجواء مميزة",
                Email = "happiness@order.app",
                Phone = "0501234567",
                Latitude = 24.7136,
                Longitude = 46.6753,
                CommissionValue = 10m
            },
            new
            {
                FullName = "مطعم الأصيل",
                StoreName = "مطعم الأصيل",
                StoreSlug = "al-aseel-restaurant",
                Description = "أشهى المأكولات الشعبية والعربية",
                Email = "aseel@order.app",
                Phone = "0507654321",
                Latitude = 24.7256,
                Longitude = 46.6890,
                CommissionValue = 12m
            },
            new
            {
                FullName = "حلويات الرياض",
                StoreName = "حلويات الرياض",
                StoreSlug = "riyadh-sweets",
                Description = "أشهى الحلويات العربية والغربية",
                Email = "sweets@order.app",
                Phone = "0509876543",
                Latitude = 24.7000,
                Longitude = 46.7100,
                CommissionValue = 8m
            }
        };

        foreach (var data in vendorsData)
        {
            var profileId = Guid.NewGuid();

            // إنشاء البروفايل
            var profile = new Profile
            {
                Id = profileId,
                UserId = profileId.ToString(),
                FullName = data.FullName,
                Email = data.Email,
                PhoneNumber = data.Phone,
                Type = ProfileType.Vendor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _profileRepository.AddAsync(profile);

            // إنشاء البائع
            var vendor = new Vendor
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                StoreName = data.StoreName,
                StoreSlug = data.StoreSlug,
                Description = data.Description,
                Status = VendorStatus.Active,
                CommissionType = CommissionType.Percentage,
                CommissionValue = data.CommissionValue,
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["latitude"] = data.Latitude.ToString(),
                    ["longitude"] = data.Longitude.ToString(),
                    ["phone"] = data.Phone,
                    ["working_hours"] = workingHoursJson
                }
            };
            await _vendorRepository.AddAsync(vendor);
        }

        _logger.LogInformation("Seeded {Count} vendors", vendorsData.Length);
    }

    private async Task SeedOffersAsync()
    {
        var existing = await _listingRepository.ListAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Offers already exist, skipping seed");
            return;
        }

        var vendors = (await _vendorRepository.ListAllAsync()).ToList();
        var categories = (await _categoryRepository.ListAllAsync()).ToList();

        if (!vendors.Any() || !categories.Any())
        {
            _logger.LogWarning("No vendors or categories found, skipping offers seed");
            return;
        }

        var coffeeCategory = categories.FirstOrDefault(c => c.Slug == "coffee");
        var mealsCategory = categories.FirstOrDefault(c => c.Slug == "meals");
        var dessertsCategory = categories.FirstOrDefault(c => c.Slug == "desserts");

        var happinessCafe = vendors.FirstOrDefault(v => v.StoreSlug == "happiness-cafe");
        var aseelRestaurant = vendors.FirstOrDefault(v => v.StoreSlug == "al-aseel-restaurant");
        var riyadhSweets = vendors.FirstOrDefault(v => v.StoreSlug == "riyadh-sweets");

        var offers = new List<(Product Product, ProductListing Listing)>();

        // عروض كافيه السعادة
        if (happinessCafe != null && coffeeCategory != null)
        {
            offers.AddRange(new[]
            {
                CreateOffer("لاتيه + كرواسون",
                    "عرض الصباح: لاتيه مع كرواسون طازج",
                    25, 18, happinessCafe, coffeeCategory,
                    "ملاحظة: العرض متاح من 7 صباحاً حتى 11 صباحاً"),
                CreateOffer("2 كابتشينو",
                    "اثنين كابتشينو بسعر واحد ونصف",
                    36, 27, happinessCafe, coffeeCategory,
                    "مناسب للمشاركة مع صديق"),
                CreateOffer("آيس موكا كبير",
                    "آيس موكا حجم كبير مع كريمة إضافية",
                    22, 16, happinessCafe, coffeeCategory, null)
            });
        }

        // عروض مطعم الأصيل
        if (aseelRestaurant != null && mealsCategory != null)
        {
            offers.AddRange(new[]
            {
                CreateOffer("وجبة كبسة دجاج",
                    "كبسة دجاج مع سلطة ومشروب غازي",
                    45, 35, aseelRestaurant, mealsCategory,
                    "الكمية محدودة - 50 وجبة يومياً"),
                CreateOffer("شاورما عربي",
                    "2 شاورما + بطاطس + مشروب",
                    30, 22, aseelRestaurant, mealsCategory, null),
                CreateOffer("برجر لحم أنقوس",
                    "برجر لحم أنقوس مع جبنة شيدر وبطاطس",
                    42, 32, aseelRestaurant, mealsCategory,
                    "اللحم طازج 100%")
            });
        }

        // عروض حلويات الرياض
        if (riyadhSweets != null && dessertsCategory != null)
        {
            offers.AddRange(new[]
            {
                CreateOffer("كيلو كنافة",
                    "كنافة نابلسية طازجة",
                    80, 60, riyadhSweets, dessertsCategory,
                    "تحضر عند الطلب"),
                CreateOffer("تشيز كيك + قهوة",
                    "قطعة تشيز كيك مع قهوة تركية",
                    35, 25, riyadhSweets, dessertsCategory, null),
                CreateOffer("بقلاوة مشكلة",
                    "نصف كيلو بقلاوة مشكلة",
                    55, 42, riyadhSweets, dessertsCategory,
                    "تشكيلة من أفضل أنواع البقلاوة")
            });
        }

        foreach (var (product, listing) in offers)
        {
            await _productRepository.AddAsync(product);
            listing.ProductId = product.Id;
            await _listingRepository.AddAsync(listing);
        }

        _logger.LogInformation("Seeded {Count} offers", offers.Count);
    }

    private (Product Product, ProductListing Listing) CreateOffer(
        string name, string description,
        decimal originalPrice, decimal offerPrice,
        Vendor vendor, ProductCategory category, string? merchantNotes)
    {
        var productId = Guid.NewGuid();
        var sku = $"ORD-{productId.ToString()[..6].ToUpper()}";

        var product = new Product
        {
            Id = productId,
            Name = name,
            Sku = sku,
            ShortDescription = description,
            CreatedAt = DateTime.UtcNow
        };

        // استخراج إحداثيات المتجر من Metadata
        double? latitude = null;
        double? longitude = null;
        if (vendor.Metadata.TryGetValue("latitude", out var latStr) && double.TryParse(latStr, out var lat))
            latitude = lat;
        if (vendor.Metadata.TryGetValue("longitude", out var lngStr) && double.TryParse(lngStr, out var lng))
            longitude = lng;

        var listing = new ProductListing
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VendorId = vendor.Id,
            CategoryId = category.Id,
            Title = name,
            Description = merchantNotes != null ? $"{description}\n\n{merchantNotes}" : description,
            Price = offerPrice,
            CompareAtPrice = originalPrice,
            Status = ACommerce.Catalog.Listings.Enums.ListingStatus.Active,
            IsActive = true,
            QuantityAvailable = 100,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(30),
            Latitude = latitude,
            Longitude = longitude,
            CreatedAt = DateTime.UtcNow
        };

        return (product, listing);
    }
}
