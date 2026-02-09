using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Products.Entities;

namespace HamtramckHardware.Api.Services;

/// <summary>
/// Seeds initial data for Hamtramck Hardware store
/// </summary>
public class HamtramckSeedDataService
{
    private readonly IBaseAsyncRepository<ProductCategory> _categoryRepository;
    private readonly IBaseAsyncRepository<Product> _productRepository;
    private readonly ILogger<HamtramckSeedDataService> _logger;

    public HamtramckSeedDataService(
        IBaseAsyncRepository<ProductCategory> categoryRepository,
        IBaseAsyncRepository<Product> productRepository,
        ILogger<HamtramckSeedDataService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedCategoriesAsync();
            await SeedProductsAsync();
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

        var products = new List<Product>
        {
            // Power Tools
            CreateProduct("p0000001-0000-0000-0000-000000000001", "DeWalt 20V MAX XR Brushless Drill/Driver Kit", "DCD791D2",
                categories["power-tools"], 149.99m, 179.99m, 15, 4.8m, 1247,
                "The DeWalt DCD791D2 20V MAX XR Lithium Ion Brushless Compact Drill/Driver Kit delivers up to 57% more runtime over brushed.",
                "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "20V MAX"}, {"Speed", "0-550/0-2,000 RPM"}, {"Chuck Size", "1/2 inch"}, {"Weight", "3.4 lbs"}, {"Warranty", "3 Years"} },
                true, true),

            CreateProduct("p0000002-0000-0000-0000-000000000002", "Milwaukee M18 FUEL Hammer Drill/Driver Kit", "2804-22",
                categories["power-tools"], 299.99m, 349.99m, 8, 4.9m, 892,
                "The M18 FUEL 1/2\" Hammer Drill/Driver is the industry's most powerful drill, providing up to 60% more power.",
                "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "18V"}, {"Torque", "1,200 in-lbs"}, {"Battery Type", "REDLITHIUM"}, {"Weight", "4.8 lbs"}, {"Warranty", "5 Years"} },
                true, false),

            CreateProduct("p0000003-0000-0000-0000-000000000003", "Makita 18V LXT Brushless Impact Driver", "XDT16Z",
                categories["power-tools"], 179.99m, null, 12, 4.7m, 654,
                "The 18V LXT Brushless 4-Speed Impact Driver delivers faster driving speed with Quick-Shift Mode.",
                "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "18V LXT"}, {"Speed", "0-3,800 RPM"}, {"Torque", "1,550 in-lbs"}, {"Weight", "3.3 lbs"}, {"Warranty", "3 Years"} },
                false, false),

            CreateProduct("p0000004-0000-0000-0000-000000000004", "Bosch 12V Flexiclick 5-In-1 Drill System", "GSR12V-300FCB22",
                categories["power-tools"], 199.99m, 229.99m, 6, 4.6m, 423,
                "The Bosch Flexiclick system features 4 interchangeable attachments that click in and out with ease.",
                "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "12V MAX"}, {"Speed", "0-400/0-1,300 RPM"}, {"Chuck Size", "1/4 inch hex"}, {"Weight", "2.2 lbs"}, {"Warranty", "3 Years"} },
                false, false),

            CreateProduct("p0000005-0000-0000-0000-000000000005", "DeWalt 20V MAX 7-1/4\" Circular Saw", "DCS570B",
                categories["power-tools"], 159.99m, null, 10, 4.7m, 534,
                "The DeWalt DCS570B 20V MAX 7-1/4\" Circular Saw features a powerful 5200 RPM motor for fast and smooth cuts.",
                "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "20V MAX"}, {"Blade Size", "7-1/4 inch"}, {"Speed", "5,200 RPM"}, {"Bevel Capacity", "57 degrees"}, {"Warranty", "3 Years"} },
                false, false),

            CreateProduct("p0000006-0000-0000-0000-000000000006", "Ryobi ONE+ 18V 6-Tool Combo Kit", "P1819",
                categories["power-tools"], 299.99m, 449.99m, 4, 4.7m, 1456,
                "This combo kit includes a drill, impact driver, circular saw, reciprocating saw, multi-tool, and LED worklight.",
                "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "18V ONE+"}, {"Tools Included", "6"}, {"Batteries", "2x 1.5Ah"}, {"Charger", "Included"}, {"Warranty", "3 Years"} },
                true, false),

            // Hand Tools
            CreateProduct("p0000007-0000-0000-0000-000000000007", "Klein Tools 11-in-1 Screwdriver/Nut Driver", "32500",
                categories["hand-tools"], 24.99m, null, 35, 4.9m, 2089,
                "Multi-bit screwdriver with 8 popular tips and 3 nut driver sizes stored in handle. Made in USA.",
                "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Functions", "11"}, {"Phillips", "#1, #2"}, {"Slotted", "3/16\", 1/4\""}, {"Handle", "Cushion-Grip"}, {"Made In", "USA"} },
                false, false),

            CreateProduct("p0000008-0000-0000-0000-000000000008", "CRAFTSMAN 320-Piece Mechanic's Tool Set", "CMMT12039",
                categories["hand-tools"], 199.99m, 299.99m, 7, 4.8m, 987,
                "This comprehensive 320-piece tool set includes sockets, wrenches, screwdrivers, pliers, and more.",
                "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Total Pieces", "320"}, {"Socket Sizes", "1/4\", 3/8\", 1/2\" Drive"}, {"Material", "Chrome Vanadium"}, {"Warranty", "Lifetime"} },
                true, false),

            CreateProduct("p0000009-0000-0000-0000-000000000009", "Husky 52\" 15-Drawer Mobile Workbench", "H52MWC15",
                categories["hand-tools"], 599.99m, 899.99m, 3, 4.6m, 423,
                "The Husky 52\" Mobile Workbench features 15 drawers and 18,000 cu. in. of storage.",
                "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Width", "52 inches"}, {"Drawers", "15"}, {"Storage", "18,000 cu. in."}, {"Weight Capacity", "600 lbs"}, {"Warranty", "Lifetime"} },
                true, false),

            // Plumbing
            CreateProduct("p0000010-0000-0000-0000-000000000010", "Copper Pipe Fittings Set 1/2\" - 25 Pieces", "CPF-25",
                categories["plumbing"], 24.99m, 29.99m, 25, 4.2m, 312,
                "Professional-grade copper fittings assortment includes elbows, tees, couplings, and caps.",
                "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Size", "1/2 inch"}, {"Quantity", "25 Pieces"}, {"Material", "Copper"}, {"Lead Free", "Yes"}, {"NSF Certified", "Yes"} },
                false, false),

            CreateProduct("p0000011-0000-0000-0000-000000000011", "SharkBite 1/2\" Push-to-Connect Coupling", "U008LFA",
                categories["plumbing"], 12.99m, null, 50, 4.6m, 892,
                "SharkBite push-to-connect fittings work with copper, PEX, CPVC, and HDPE pipe. No soldering required.",
                "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Size", "1/2 inch"}, {"Connection", "Push-to-Connect"}, {"Compatible Pipes", "Copper, PEX, CPVC"}, {"Pressure Rating", "200 PSI"}, {"Warranty", "25 Years"} },
                false, false),

            CreateProduct("p0000012-0000-0000-0000-000000000012", "Stanley FatMax Copper Pipe Cutter", "FMHT96545",
                categories["plumbing"], 34.99m, null, 18, 4.5m, 312,
                "The Stanley FatMax pipe cutter features a quick-adjust mechanism for fast pipe sizing.",
                "https://images.unsplash.com/photo-1581147036324-c17ac41f3f2c?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Capacity", "1/8\" to 1-1/8\""}, {"Material Cut", "Copper, Brass, Aluminum"}, {"Adjustment", "Quick-Adjust"}, {"Warranty", "Lifetime"} },
                false, false),

            CreateProduct("p0000013-0000-0000-0000-000000000013", "PVC Pipe 10ft Schedule 40 - 3/4\" Bundle of 5", "PVC-1040-5",
                categories["plumbing"], 18.99m, null, 30, 4.0m, 234,
                "Schedule 40 PVC pipe for plumbing and irrigation applications. NSF certified for potable water.",
                "https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Size", "3/4 inch"}, {"Length", "10 ft each"}, {"Schedule", "40"}, {"Quantity", "5 Pack"}, {"NSF Certified", "Yes"} },
                false, false),

            // Electrical
            CreateProduct("p0000014-0000-0000-0000-000000000014", "Southwire Romex 12/2 NM-B Wire 250ft", "63947622",
                categories["electrical"], 89.99m, 99.99m, 12, 4.8m, 567,
                "Southwire 12/2 NM-B Romex wire for residential wiring. Features SIMpull technology for easier pulling.",
                "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Gauge", "12 AWG"}, {"Conductors", "2 + Ground"}, {"Length", "250 ft"}, {"Voltage Rating", "600V"}, {"UL Listed", "Yes"} },
                true, false),

            CreateProduct("p0000015-0000-0000-0000-000000000015", "LED Shop Light 4ft Linkable - 2 Pack", "LSLL-4FT-2PK",
                categories["electrical"], 39.99m, null, 20, 4.5m, 456,
                "Energy-efficient 4ft LED shop lights deliver 4400 lumens per fixture. Link up to 4 fixtures together.",
                "https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Length", "4 ft"}, {"Lumens", "4,400 per fixture"}, {"Wattage", "40W"}, {"Color Temp", "4000K"}, {"Lifespan", "50,000 hours"} },
                false, false),

            CreateProduct("p0000016-0000-0000-0000-000000000016", "Leviton 15A Tamper-Resistant Outlet (10-Pack)", "T5320-WMP",
                categories["electrical"], 24.99m, null, 40, 4.7m, 1567,
                "Tamper-resistant duplex outlets with shutters that prevent foreign objects from being inserted.",
                "https://images.unsplash.com/photo-1558402529-d2638a7023e9?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Amperage", "15A"}, {"Voltage", "125V"}, {"Quantity", "10 Pack"}, {"Color", "White"}, {"UL Listed", "Yes"} },
                false, false),

            // Lawn & Garden
            CreateProduct("p0000017-0000-0000-0000-000000000017", "Flexzilla Garden Hose 5/8\" x 50ft", "HFZG550YW",
                categories["lawn-garden"], 29.99m, null, 0, 4.4m, 1234,
                "Flexzilla garden hose is lightweight, flexible, and kink resistant. Drinking water safe.",
                "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Diameter", "5/8 inch"}, {"Length", "50 ft"}, {"Material", "Hybrid Polymer"}, {"Kink Resistant", "Yes"}, {"Warranty", "Lifetime"} },
                false, false),

            CreateProduct("p0000018-0000-0000-0000-000000000018", "Ryobi ONE+ 18V Cordless Jet Fan Blower", "P21012",
                categories["lawn-garden"], 89.99m, 109.99m, 11, 4.1m, 543,
                "The RYOBI 18V ONE+ Jet Fan Blower delivers up to 200 CFM and 150 MPH of clearing force.",
                "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "18V ONE+"}, {"Air Volume", "200 CFM"}, {"Air Speed", "150 MPH"}, {"Weight", "2.0 lbs"}, {"Warranty", "3 Years"} },
                false, false),

            // Paint
            CreateProduct("p0000019-0000-0000-0000-000000000019", "3M Scotch Blue Painter's Tape 1.88\" x 60yd", "2090-48A",
                categories["paint"], 8.99m, null, 100, 4.9m, 2341,
                "ScotchBlue Original Multi-Surface Painter's Tape provides medium adhesion for most projects.",
                "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Width", "1.88 inches"}, {"Length", "60 yards"}, {"Adhesion", "Medium"}, {"Clean Removal", "14 days"}, {"Surfaces", "Multi-Surface"} },
                false, false),

            // Hardware
            CreateProduct("p0000020-0000-0000-0000-000000000020", "Gorilla Heavy Duty Construction Adhesive 9oz", "8010003",
                categories["hardware"], 7.49m, null, 60, 4.7m, 1823,
                "Gorilla Heavy Duty Construction Adhesive is a tough, versatile, all-weather adhesive.",
                "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Size", "9 oz"}, {"Type", "Construction Adhesive"}, {"Indoor/Outdoor", "Both"}, {"Waterproof", "Yes"}, {"Paintable", "Yes"} },
                false, false),

            CreateProduct("p0000021-0000-0000-0000-000000000021", "WD-40 Multi-Use Product with Smart Straw 12oz", "490057",
                categories["hardware"], 6.99m, null, 80, 4.8m, 3156,
                "WD-40 Multi-Use Product protects metal from rust and corrosion, penetrates stuck parts, and lubricates.",
                "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Size", "12 oz"}, {"Type", "Multi-Use Lubricant"}, {"Smart Straw", "Yes"}, {"Uses", "Lubricates, Protects, Cleans"}, {"Made In", "USA"} },
                false, false),

            CreateProduct("p0000022-0000-0000-0000-000000000022", "RIDGID 12-Gallon NXT Wet/Dry Shop Vacuum", "HD1200",
                categories["power-tools"], 79.99m, 119.99m, 8, 4.5m, 678,
                "The RIDGID 12-Gallon NXT Wet/Dry Shop Vacuum features a powerful motor delivering 5.0 Peak HP.",
                "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Capacity", "12 Gallons"}, {"Peak HP", "5.0"}, {"Hose Length", "7 ft"}, {"Cord Length", "20 ft"}, {"Warranty", "Lifetime"} },
                true, false),

            CreateProduct("p0000023-0000-0000-0000-000000000023", "Bosch 2.25 HP Combination Plunge & Fixed Base Router", "1617EVSPK",
                categories["power-tools"], 199.99m, null, 5, 4.8m, 834,
                "This combo kit includes both plunge and fixed bases for maximum versatility.",
                "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Horsepower", "2.25 HP"}, {"Speed", "8,000-25,000 RPM"}, {"Collet Sizes", "1/4\", 1/2\""}, {"Bases", "Plunge & Fixed"}, {"Warranty", "1 Year"} },
                false, false),

            CreateProduct("p0000024-0000-0000-0000-000000000024", "DeWalt 20V MAX XR 5.0Ah Battery 2-Pack", "DCB205-2",
                categories["power-tools"], 99.99m, null, 20, 4.9m, 2567,
                "DeWalt 20V MAX XR 5.0Ah Lithium Ion batteries provide up to 60% more capacity than standard 3.0Ah batteries.",
                "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Voltage", "20V MAX"}, {"Capacity", "5.0Ah"}, {"Quantity", "2 Pack"}, {"Technology", "XR Lithium Ion"}, {"Warranty", "3 Years"} },
                false, true),

            CreateProduct("p0000025-0000-0000-0000-000000000025", "DeWalt Titanium Drill Bit Set 29-Piece", "DW1369",
                categories["hand-tools"], 34.99m, null, 25, 4.7m, 1834,
                "DeWalt titanium pilot point drill bits feature a patented split point tip that starts on contact.",
                "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop",
                new Dictionary<string, string> { {"Quantity", "29 Pieces"}, {"Size Range", "1/16\" to 1/2\""}, {"Coating", "Titanium"}, {"Point Type", "Pilot Point"}, {"Case", "Included"} },
                false, false),
        };

        foreach (var product in products)
        {
            await _productRepository.AddAsync(product);
        }

        _logger.LogInformation("Seeded {Count} products", products.Count);
    }

    private Product CreateProduct(
        string id, string name, string sku, Guid categoryId,
        decimal price, decimal? compareAtPrice, int stockQty,
        decimal? rating, int reviewCount, string description,
        string imageUrl, Dictionary<string, string> specs,
        bool isFeatured, bool isNew)
    {
        return new Product
        {
            Id = Guid.Parse(id),
            Name = name,
            Sku = sku,
            CategoryId = categoryId,
            Price = price,
            CompareAtPrice = compareAtPrice,
            StockQuantity = stockQty,
            Status = stockQty > 0 ? "active" : "out_of_stock",
            Rating = rating,
            ReviewCount = reviewCount,
            ShortDescription = description,
            Description = description,
            FeaturedImageUrl = imageUrl,
            Images = new List<string> { imageUrl },
            Specifications = specs,
            IsFeatured = isFeatured,
            IsNew = isNew,
            CreatedAt = DateTime.UtcNow
        };
    }
}
