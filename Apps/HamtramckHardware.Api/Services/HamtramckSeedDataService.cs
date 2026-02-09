using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Products.Enums;
using ACommerce.Catalog.Currencies.Entities;

namespace HamtramckHardware.Api.Services;

/// <summary>
/// Seeds initial data for Hamtramck Hardware store
/// </summary>
public class HamtramckSeedDataService
{
    private readonly IBaseAsyncRepository<ProductCategory> _categoryRepository;
    private readonly IBaseAsyncRepository<Product> _productRepository;
    private readonly IBaseAsyncRepository<ProductPrice> _priceRepository;
    private readonly IBaseAsyncRepository<ProductInventory> _inventoryRepository;
    private readonly IBaseAsyncRepository<ProductCategoryMapping> _categoryMappingRepository;
    private readonly IBaseAsyncRepository<Currency> _currencyRepository;
    private readonly ILogger<HamtramckSeedDataService> _logger;

    // USD Currency ID (will be created if not exists)
    private Guid _usdCurrencyId;

    public HamtramckSeedDataService(
        IBaseAsyncRepository<ProductCategory> categoryRepository,
        IBaseAsyncRepository<Product> productRepository,
        IBaseAsyncRepository<ProductPrice> priceRepository,
        IBaseAsyncRepository<ProductInventory> inventoryRepository,
        IBaseAsyncRepository<ProductCategoryMapping> categoryMappingRepository,
        IBaseAsyncRepository<Currency> currencyRepository,
        ILogger<HamtramckSeedDataService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _priceRepository = priceRepository;
        _inventoryRepository = inventoryRepository;
        _categoryMappingRepository = categoryMappingRepository;
        _currencyRepository = currencyRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedCurrencyAsync();
            await SeedCategoriesAsync();
            await SeedProductsAsync();
            _logger.LogInformation("Seed data completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding data");
        }
    }

    private async Task SeedCurrencyAsync()
    {
        var currencies = await _currencyRepository.ListAllAsync();
        var usd = currencies.FirstOrDefault(c => c.Code == "USD");

        if (usd == null)
        {
            usd = new Currency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Name = "US Dollar",
                Symbol = "$",
                IsDefault = true,
                IsActive = true,
                DecimalPlaces = 2,
                CreatedAt = DateTime.UtcNow
            };
            await _currencyRepository.AddAsync(usd);
            _logger.LogInformation("Created USD currency");
        }

        _usdCurrencyId = usd.Id;
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
            new() { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Power Tools", Slug = "power-tools", Description = "Cordless and corded power tools", Icon = "bi-tools", SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000002-0000-0000-0000-000000000002"), Name = "Hand Tools", Slug = "hand-tools", Description = "Manual tools for every job", Icon = "bi-wrench", SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000003-0000-0000-0000-000000000003"), Name = "Plumbing", Slug = "plumbing", Description = "Pipes, fittings, and plumbing supplies", Icon = "bi-droplet", SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000004-0000-0000-0000-000000000004"), Name = "Electrical", Slug = "electrical", Description = "Wire, outlets, and electrical supplies", Icon = "bi-lightning-charge", SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000005-0000-0000-0000-000000000005"), Name = "Lawn & Garden", Slug = "lawn-garden", Description = "Outdoor and gardening equipment", Icon = "bi-flower1", SortOrder = 5, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000006-0000-0000-0000-000000000006"), Name = "Paint", Slug = "paint", Description = "Paints, stains, and painting supplies", Icon = "bi-palette", SortOrder = 6, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000007-0000-0000-0000-000000000007"), Name = "Lumber", Slug = "lumber", Description = "Wood and building materials", Icon = "bi-box", SortOrder = 7, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("c0000008-0000-0000-0000-000000000008"), Name = "Hardware", Slug = "hardware", Description = "Fasteners, hinges, and general hardware", Icon = "bi-nut", SortOrder = 8, CreatedAt = DateTime.UtcNow },
        };

        foreach (var category in categories)
        {
            await _categoryRepository.AddAsync(category);
        }

        _logger.LogInformation("Seeded {Count} categories", categories.Count);
    }

    private async Task SeedProductsAsync()
    {
        var existing = await _productRepository.ListAllAsync();

        if (existing.Any())
        {
            _logger.LogInformation("Products already exist, skipping seed");
            return;
        }

        var categories = (await _categoryRepository.ListAllAsync()).ToDictionary(c => c.Slug ?? "", c => c.Id);

        // Create products with their prices and inventory
        var productsData = new List<(string Id, string Sku, string Name, string CategorySlug, decimal BasePrice, decimal? SalePrice, int StockQty, string Description, string Image, bool IsFeatured, bool IsNew)>
        {
            // Power Tools
            ("p0000001-0000-0000-0000-000000000001", "DCD791D2", "DeWalt 20V MAX XR Brushless Drill/Driver Kit", "power-tools", 179.99m, 149.99m, 15, "The DeWalt DCD791D2 20V MAX XR Lithium Ion Brushless Compact Drill/Driver Kit delivers up to 57% more runtime over brushed.", "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop", true, true),
            ("p0000002-0000-0000-0000-000000000002", "2804-22", "Milwaukee M18 FUEL Hammer Drill/Driver Kit", "power-tools", 349.99m, 299.99m, 8, "The M18 FUEL 1/2\" Hammer Drill/Driver is the industry's most powerful drill, providing up to 60% more power.", "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop", true, false),
            ("p0000003-0000-0000-0000-000000000003", "XDT16Z", "Makita 18V LXT Brushless Impact Driver", "power-tools", 179.99m, null, 12, "The 18V LXT Brushless 4-Speed Impact Driver delivers faster driving speed with Quick-Shift Mode.", "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&h=600&fit=crop", false, false),
            ("p0000004-0000-0000-0000-000000000004", "GSR12V-300FCB22", "Bosch 12V Flexiclick 5-In-1 Drill System", "power-tools", 229.99m, 199.99m, 6, "The Bosch Flexiclick system features 4 interchangeable attachments.", "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=600&h=600&fit=crop", false, false),
            ("p0000005-0000-0000-0000-000000000005", "DCS570B", "DeWalt 20V MAX 7-1/4\" Circular Saw", "power-tools", 159.99m, null, 10, "Powerful 5200 RPM motor for fast and smooth cuts.", "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=600&h=600&fit=crop", false, false),
            ("p0000006-0000-0000-0000-000000000006", "P1819", "Ryobi ONE+ 18V 6-Tool Combo Kit", "power-tools", 449.99m, 299.99m, 4, "Includes drill, impact driver, circular saw, reciprocating saw, multi-tool, and LED worklight.", "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=600&h=600&fit=crop", true, false),
            // Hand Tools
            ("p0000007-0000-0000-0000-000000000007", "32500", "Klein Tools 11-in-1 Screwdriver/Nut Driver", "hand-tools", 24.99m, null, 35, "Multi-bit screwdriver with 8 popular tips and 3 nut driver sizes. Made in USA.", "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=600&h=600&fit=crop", false, false),
            ("p0000008-0000-0000-0000-000000000008", "CMMT12039", "CRAFTSMAN 320-Piece Mechanic's Tool Set", "hand-tools", 299.99m, 199.99m, 7, "Comprehensive tool set includes sockets, wrenches, screwdrivers, pliers, and more.", "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=600&h=600&fit=crop", true, false),
            ("p0000009-0000-0000-0000-000000000009", "H52MWC15", "Husky 52\" 15-Drawer Mobile Workbench", "hand-tools", 899.99m, 599.99m, 3, "Features 15 drawers and 18,000 cu. in. of storage.", "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?w=600&h=600&fit=crop", true, false),
            // Plumbing
            ("p0000010-0000-0000-0000-000000000010", "CPF-25", "Copper Pipe Fittings Set 1/2\" - 25 Pieces", "plumbing", 29.99m, 24.99m, 25, "Professional-grade copper fittings assortment includes elbows, tees, couplings.", "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&h=600&fit=crop", false, false),
            ("p0000011-0000-0000-0000-000000000011", "U008LFA", "SharkBite 1/2\" Push-to-Connect Coupling", "plumbing", 12.99m, null, 50, "Works with copper, PEX, CPVC, and HDPE pipe. No soldering required.", "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&h=600&fit=crop", false, false),
            ("p0000012-0000-0000-0000-000000000012", "FMHT96545", "Stanley FatMax Copper Pipe Cutter", "plumbing", 34.99m, null, 18, "Quick-adjust mechanism for fast pipe sizing. Cuts 1/8\" to 1-1/8\".", "https://images.unsplash.com/photo-1581147036324-c17ac41f3f2c?w=600&h=600&fit=crop", false, false),
            ("p0000013-0000-0000-0000-000000000013", "PVC-1040-5", "PVC Pipe 10ft Schedule 40 - 3/4\" Bundle of 5", "plumbing", 18.99m, null, 30, "Schedule 40 PVC pipe for plumbing and irrigation applications.", "https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?w=600&h=600&fit=crop", false, false),
            // Electrical
            ("p0000014-0000-0000-0000-000000000014", "63947622", "Southwire Romex 12/2 NM-B Wire 250ft", "electrical", 99.99m, 89.99m, 12, "Features SIMpull technology for easier pulling. UL Listed.", "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=600&h=600&fit=crop", true, false),
            ("p0000015-0000-0000-0000-000000000015", "LSLL-4FT-2PK", "LED Shop Light 4ft Linkable - 2 Pack", "electrical", 39.99m, null, 20, "Energy-efficient 4ft LED shop lights deliver 4400 lumens per fixture.", "https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=600&h=600&fit=crop", false, false),
            ("p0000016-0000-0000-0000-000000000016", "T5320-WMP", "Leviton 15A Tamper-Resistant Outlet (10-Pack)", "electrical", 24.99m, null, 40, "Tamper-resistant duplex outlets with shutters. Meets NEC requirements.", "https://images.unsplash.com/photo-1558402529-d2638a7023e9?w=600&h=600&fit=crop", false, false),
            // Lawn & Garden
            ("p0000017-0000-0000-0000-000000000017", "HFZG550YW", "Flexzilla Garden Hose 5/8\" x 50ft", "lawn-garden", 29.99m, null, 0, "Lightweight, flexible, and kink resistant. Drinking water safe.", "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=600&h=600&fit=crop", false, false),
            ("p0000018-0000-0000-0000-000000000018", "P21012", "Ryobi ONE+ 18V Cordless Jet Fan Blower", "lawn-garden", 109.99m, 89.99m, 11, "Delivers up to 200 CFM and 150 MPH of clearing force.", "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop", false, false),
            // Paint
            ("p0000019-0000-0000-0000-000000000019", "2090-48A", "3M Scotch Blue Painter's Tape 1.88\" x 60yd", "paint", 8.99m, null, 100, "Medium adhesion for most projects. Removes cleanly for 14 days.", "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=600&h=600&fit=crop", false, false),
            // Hardware
            ("p0000020-0000-0000-0000-000000000020", "8010003", "Gorilla Heavy Duty Construction Adhesive 9oz", "hardware", 7.49m, null, 60, "Tough, versatile, all-weather adhesive. Bonds virtually anything.", "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=600&h=600&fit=crop", false, false),
            ("p0000021-0000-0000-0000-000000000021", "490057", "WD-40 Multi-Use Product with Smart Straw 12oz", "hardware", 6.99m, null, 80, "Protects, penetrates, displaces moisture, and lubricates.", "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=600&h=600&fit=crop", false, false),
            ("p0000022-0000-0000-0000-000000000022", "HD1200", "RIDGID 12-Gallon NXT Wet/Dry Shop Vacuum", "power-tools", 119.99m, 79.99m, 8, "Powerful 5.0 Peak HP motor with 3-in-1 functionality.", "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop", true, false),
            ("p0000023-0000-0000-0000-000000000023", "1617EVSPK", "Bosch 2.25 HP Combination Plunge & Fixed Base Router", "power-tools", 199.99m, null, 5, "Includes both plunge and fixed bases for maximum versatility.", "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=600&h=600&fit=crop", false, false),
            ("p0000024-0000-0000-0000-000000000024", "DCB205-2", "DeWalt 20V MAX XR 5.0Ah Battery 2-Pack", "power-tools", 99.99m, null, 20, "Provides up to 60% more capacity than standard 3.0Ah batteries.", "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop", false, true),
            ("p0000025-0000-0000-0000-000000000025", "DW1369", "DeWalt Titanium Drill Bit Set 29-Piece", "hand-tools", 34.99m, null, 25, "Titanium pilot point drill bits with patented split point tip.", "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop", false, false),
        };

        foreach (var data in productsData)
        {
            var productId = Guid.Parse(data.Id);
            var categoryId = categories.TryGetValue(data.CategorySlug, out var catId) ? catId : Guid.Empty;

            // Create Product
            var product = new Product
            {
                Id = productId,
                Name = data.Name,
                Sku = data.Sku,
                Status = data.StockQty > 0 ? ProductStatus.Active : ProductStatus.OutOfStock,
                ShortDescription = data.Description,
                LongDescription = data.Description,
                FeaturedImage = data.Image,
                Images = new List<string> { data.Image },
                IsFeatured = data.IsFeatured,
                IsNew = data.IsNew,
                CreatedAt = DateTime.UtcNow
            };
            await _productRepository.AddAsync(product);

            // Create ProductPrice
            var price = new ProductPrice
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                CurrencyId = _usdCurrencyId,
                BasePrice = data.BasePrice,
                SalePrice = data.SalePrice,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _priceRepository.AddAsync(price);

            // Create ProductInventory
            var inventory = new ProductInventory
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                QuantityInStock = data.StockQty,
                QuantityReserved = 0,
                Status = data.StockQty > 0 ? StockStatus.InStock : StockStatus.OutOfStock,
                TrackInventory = true,
                CreatedAt = DateTime.UtcNow
            };
            await _inventoryRepository.AddAsync(inventory);

            // Create ProductCategoryMapping
            if (categoryId != Guid.Empty)
            {
                var categoryMapping = new ProductCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    CategoryId = categoryId,
                    IsPrimary = true,
                    SortOrder = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _categoryMappingRepository.AddAsync(categoryMapping);
            }
        }

        _logger.LogInformation("Seeded {Count} products", productsData.Count);
    }
}
