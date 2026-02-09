using ACommerce.Templates.Customer.Models;

namespace HamtramckHardware.Shared.Services;

/// <summary>
/// Centralized data service for Hamtramck Hardware store.
/// Provides mock data - replace with API calls when backend is ready.
/// </summary>
public class HamtramckDataService
{
    public List<HomeCategoryItem> GetCategories() => new()
    {
        new() { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "Power Tools", Icon = "bi-tools" },
        new() { Id = Guid.Parse("b0000002-0000-0000-0000-000000000002"), Name = "Hand Tools", Icon = "bi-wrench" },
        new() { Id = Guid.Parse("b0000003-0000-0000-0000-000000000003"), Name = "Plumbing", Icon = "bi-droplet" },
        new() { Id = Guid.Parse("b0000004-0000-0000-0000-000000000004"), Name = "Electrical", Icon = "bi-lightning-charge" },
        new() { Id = Guid.Parse("b0000005-0000-0000-0000-000000000005"), Name = "Lawn & Garden", Icon = "bi-flower1" },
        new() { Id = Guid.Parse("b0000006-0000-0000-0000-000000000006"), Name = "Paint", Icon = "bi-palette" },
        new() { Id = Guid.Parse("b0000007-0000-0000-0000-000000000007"), Name = "Lumber", Icon = "bi-box" },
        new() { Id = Guid.Parse("b0000008-0000-0000-0000-000000000008"), Name = "Hardware", Icon = "bi-nut" },
    };

    public List<HomeProductItem> GetFeaturedProducts() => new()
    {
        new() { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "DeWalt 20V MAX XR Brushless Drill", Image = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=400&h=400&fit=crop", Price = 149.99m, OldPrice = 179.99m, Rating = 4.8, ReviewCount = 1247, Attributes = new() { { "Brand", "DeWalt" } } },
        new() { Id = Guid.Parse("a0000002-0000-0000-0000-000000000002"), Name = "Milwaukee M18 FUEL Hammer Drill", Image = "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 349.99m, Rating = 4.9, ReviewCount = 892, Attributes = new() { { "Brand", "Milwaukee" } } },
        new() { Id = Guid.Parse("a0000003-0000-0000-0000-000000000003"), Name = "Makita 18V LXT Impact Driver", Image = "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&h=400&fit=crop", Price = 179.99m, Rating = 4.7, ReviewCount = 654, Attributes = new() { { "Brand", "Makita" } } },
        new() { Id = Guid.Parse("a0000004-0000-0000-0000-000000000004"), Name = "Bosch Flexiclick 5-In-1 Drill", Image = "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 229.99m, Rating = 4.6, ReviewCount = 423, Attributes = new() { { "Brand", "Bosch" } } },
        new() { Id = Guid.Parse("a0000012-0000-0000-0000-000000000012"), Name = "Stanley Copper Pipe Cutter", Image = "https://images.unsplash.com/photo-1581147036324-c17ac41f3f2c?w=400&h=400&fit=crop", Price = 34.99m, Rating = 4.5, ReviewCount = 312, Attributes = new() { { "Brand", "Stanley" } } },
        new() { Id = Guid.Parse("a0000014-0000-0000-0000-000000000014"), Name = "Southwire Romex 12/2 Wire 250ft", Image = "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=400&h=400&fit=crop", Price = 89.99m, OldPrice = 99.99m, Rating = 4.8, ReviewCount = 567, Attributes = new() { { "Brand", "Southwire" } } },
    };

    public List<HomeProductItem> GetBestSellers() => new()
    {
        new() { Id = Guid.Parse("a0000019-0000-0000-0000-000000000019"), Name = "3M Scotch Blue Painter's Tape", Image = "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=400&h=400&fit=crop", Price = 8.99m, Rating = 4.9, ReviewCount = 2341, Attributes = new() { { "Brand", "3M" } } },
        new() { Id = Guid.Parse("a0000020-0000-0000-0000-000000000020"), Name = "Gorilla Construction Adhesive", Image = "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=400&h=400&fit=crop", Price = 7.49m, Rating = 4.7, ReviewCount = 1823, Attributes = new() { { "Brand", "Gorilla" } } },
        new() { Id = Guid.Parse("a0000021-0000-0000-0000-000000000021"), Name = "WD-40 Multi-Use Product 12oz", Image = "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400&h=400&fit=crop", Price = 6.99m, Rating = 4.8, ReviewCount = 3156, Attributes = new() { { "Brand", "WD-40" } } },
        new() { Id = Guid.Parse("a0000011-0000-0000-0000-000000000011"), Name = "SharkBite Push-to-Connect Coupling", Image = "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=400&h=400&fit=crop", Price = 12.99m, Rating = 4.6, ReviewCount = 892, Attributes = new() { { "Brand", "SharkBite" } } },
        new() { Id = Guid.Parse("a0000016-0000-0000-0000-000000000016"), Name = "Leviton Outlet 10-Pack", Image = "https://images.unsplash.com/photo-1558402529-d2638a7023e9?w=400&h=400&fit=crop", Price = 24.99m, Rating = 4.7, ReviewCount = 1567, Attributes = new() { { "Brand", "Leviton" } } },
        new() { Id = Guid.Parse("a0000007-0000-0000-0000-000000000007"), Name = "Klein Tools 11-in-1 Screwdriver", Image = "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=400&h=400&fit=crop", Price = 24.99m, Rating = 4.9, ReviewCount = 2089, Attributes = new() { { "Brand", "Klein" } } },
    };

    public List<HomeProductItem> GetDealsProducts() => new()
    {
        new() { Id = Guid.Parse("a0000006-0000-0000-0000-000000000006"), Name = "Ryobi ONE+ 18V 6-Tool Combo Kit", Image = "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 449.99m, Rating = 4.7, ReviewCount = 1456 },
        new() { Id = Guid.Parse("a0000008-0000-0000-0000-000000000008"), Name = "CRAFTSMAN 320-Piece Tool Set", Image = "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 299.99m, Rating = 4.8, ReviewCount = 987 },
        new() { Id = Guid.Parse("a0000009-0000-0000-0000-000000000009"), Name = "Husky 52\" Tool Chest & Cabinet", Image = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?w=400&h=400&fit=crop", Price = 599.99m, OldPrice = 899.99m, Rating = 4.6, ReviewCount = 423 },
        new() { Id = Guid.Parse("a0000022-0000-0000-0000-000000000022"), Name = "RIDGID 12-Gallon Shop Vacuum", Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=400&fit=crop", Price = 79.99m, OldPrice = 119.99m, Rating = 4.5, ReviewCount = 678 },
    };

    public List<HomeProductItem> GetAllProducts() => new()
    {
        new() { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "DeWalt 20V MAX XR Brushless Drill/Driver Kit", Image = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=400&h=400&fit=crop", Price = 149.99m, OldPrice = 179.99m, Rating = 4.8, ReviewCount = 1247, InStock = 15, Attributes = new() { { "Brand", "DeWalt" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000002-0000-0000-0000-000000000002"), Name = "Milwaukee M18 FUEL Hammer Drill/Driver Kit", Image = "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 349.99m, Rating = 4.9, ReviewCount = 892, InStock = 8, Attributes = new() { { "Brand", "Milwaukee" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000003-0000-0000-0000-000000000003"), Name = "Makita 18V LXT Brushless Impact Driver", Image = "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&h=400&fit=crop", Price = 179.99m, Rating = 4.7, ReviewCount = 654, InStock = 12, Attributes = new() { { "Brand", "Makita" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000004-0000-0000-0000-000000000004"), Name = "Bosch 12V Flexiclick 5-In-1 Drill System", Image = "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 229.99m, Rating = 4.6, ReviewCount = 423, InStock = 6, Attributes = new() { { "Brand", "Bosch" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000005-0000-0000-0000-000000000005"), Name = "DeWalt 20V MAX 7-1/4\" Circular Saw", Image = "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=400&h=400&fit=crop", Price = 159.99m, Rating = 4.7, ReviewCount = 534, InStock = 10, Attributes = new() { { "Brand", "DeWalt" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000006-0000-0000-0000-000000000006"), Name = "Ryobi ONE+ 18V 6-Tool Combo Kit", Image = "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 449.99m, Rating = 4.7, ReviewCount = 1456, InStock = 4, Attributes = new() { { "Brand", "Ryobi" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000007-0000-0000-0000-000000000007"), Name = "Klein Tools 11-in-1 Screwdriver/Nut Driver", Image = "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=400&h=400&fit=crop", Price = 24.99m, Rating = 4.9, ReviewCount = 2089, InStock = 35, Attributes = new() { { "Brand", "Klein" }, { "Category", "hand-tools" } } },
        new() { Id = Guid.Parse("a0000008-0000-0000-0000-000000000008"), Name = "CRAFTSMAN 320-Piece Mechanic's Tool Set", Image = "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 299.99m, Rating = 4.8, ReviewCount = 987, InStock = 7, Attributes = new() { { "Brand", "CRAFTSMAN" }, { "Category", "hand-tools" } } },
        new() { Id = Guid.Parse("a0000009-0000-0000-0000-000000000009"), Name = "Husky 52\" 15-Drawer Mobile Workbench", Image = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?w=400&h=400&fit=crop", Price = 599.99m, OldPrice = 899.99m, Rating = 4.6, ReviewCount = 423, InStock = 3, Attributes = new() { { "Brand", "Husky" }, { "Category", "hand-tools" } } },
        new() { Id = Guid.Parse("a0000010-0000-0000-0000-000000000010"), Name = "Copper Pipe Fittings Set 1/2\" - 25 Pieces", Image = "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=400&h=400&fit=crop", Price = 24.99m, OldPrice = 29.99m, Rating = 4.2, ReviewCount = 312, InStock = 25, Attributes = new() { { "Brand", "Stanley" }, { "Category", "plumbing" } } },
        new() { Id = Guid.Parse("a0000011-0000-0000-0000-000000000011"), Name = "SharkBite 1/2\" Push-to-Connect Coupling", Image = "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=400&h=400&fit=crop", Price = 12.99m, Rating = 4.6, ReviewCount = 892, InStock = 50, Attributes = new() { { "Brand", "SharkBite" }, { "Category", "plumbing" } } },
        new() { Id = Guid.Parse("a0000012-0000-0000-0000-000000000012"), Name = "Stanley FatMax Copper Pipe Cutter", Image = "https://images.unsplash.com/photo-1581147036324-c17ac41f3f2c?w=400&h=400&fit=crop", Price = 34.99m, Rating = 4.5, ReviewCount = 312, InStock = 18, Attributes = new() { { "Brand", "Stanley" }, { "Category", "plumbing" } } },
        new() { Id = Guid.Parse("a0000013-0000-0000-0000-000000000013"), Name = "PVC Pipe 10ft Schedule 40 - 3/4\" Bundle of 5", Image = "https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?w=400&h=400&fit=crop", Price = 18.99m, Rating = 4.0, ReviewCount = 234, InStock = 30, Attributes = new() { { "Brand", "Charlotte Pipe" }, { "Category", "plumbing" } } },
        new() { Id = Guid.Parse("a0000014-0000-0000-0000-000000000014"), Name = "Southwire Romex 12/2 NM-B Wire 250ft", Image = "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=400&h=400&fit=crop", Price = 89.99m, OldPrice = 99.99m, Rating = 4.8, ReviewCount = 567, InStock = 12, Attributes = new() { { "Brand", "Southwire" }, { "Category", "electrical" } } },
        new() { Id = Guid.Parse("a0000015-0000-0000-0000-000000000015"), Name = "LED Shop Light 4ft Linkable - 2 Pack", Image = "https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=400&h=400&fit=crop", Price = 39.99m, Rating = 4.5, ReviewCount = 456, InStock = 20, Attributes = new() { { "Brand", "Lithonia" }, { "Category", "electrical" } } },
        new() { Id = Guid.Parse("a0000016-0000-0000-0000-000000000016"), Name = "Leviton 15A Tamper-Resistant Outlet (10-Pack)", Image = "https://images.unsplash.com/photo-1558402529-d2638a7023e9?w=400&h=400&fit=crop", Price = 24.99m, Rating = 4.7, ReviewCount = 1567, InStock = 40, Attributes = new() { { "Brand", "Leviton" }, { "Category", "electrical" } } },
        new() { Id = Guid.Parse("a0000017-0000-0000-0000-000000000017"), Name = "Flexzilla Garden Hose 5/8\" x 50ft", Image = "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=400&h=400&fit=crop", Price = 29.99m, Rating = 4.4, ReviewCount = 1234, InStock = 0, Attributes = new() { { "Brand", "Flexzilla" }, { "Category", "lawn-garden" } } },
        new() { Id = Guid.Parse("a0000018-0000-0000-0000-000000000018"), Name = "Ryobi ONE+ 18V Cordless Jet Fan Blower", Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=400&fit=crop", Price = 89.99m, OldPrice = 109.99m, Rating = 4.1, ReviewCount = 543, InStock = 11, Attributes = new() { { "Brand", "Ryobi" }, { "Category", "lawn-garden" } } },
        new() { Id = Guid.Parse("a0000019-0000-0000-0000-000000000019"), Name = "3M Scotch Blue Painter's Tape 1.88\" x 60yd", Image = "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=400&h=400&fit=crop", Price = 8.99m, Rating = 4.9, ReviewCount = 2341, InStock = 100, Attributes = new() { { "Brand", "3M" }, { "Category", "paint" } } },
        new() { Id = Guid.Parse("a0000020-0000-0000-0000-000000000020"), Name = "Gorilla Heavy Duty Construction Adhesive 9oz", Image = "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=400&h=400&fit=crop", Price = 7.49m, Rating = 4.7, ReviewCount = 1823, InStock = 60, Attributes = new() { { "Brand", "Gorilla" }, { "Category", "hardware" } } },
        new() { Id = Guid.Parse("a0000021-0000-0000-0000-000000000021"), Name = "WD-40 Multi-Use Product with Smart Straw 12oz", Image = "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400&h=400&fit=crop", Price = 6.99m, Rating = 4.8, ReviewCount = 3156, InStock = 80, Attributes = new() { { "Brand", "WD-40" }, { "Category", "hardware" } } },
        new() { Id = Guid.Parse("a0000022-0000-0000-0000-000000000022"), Name = "RIDGID 12-Gallon NXT Wet/Dry Shop Vacuum", Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=400&fit=crop", Price = 79.99m, OldPrice = 119.99m, Rating = 4.5, ReviewCount = 678, InStock = 8, Attributes = new() { { "Brand", "RIDGID" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000023-0000-0000-0000-000000000023"), Name = "Bosch 2.25 HP Combination Plunge & Fixed Base Router", Image = "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=400&h=400&fit=crop", Price = 199.99m, Rating = 4.8, ReviewCount = 834, InStock = 5, Attributes = new() { { "Brand", "Bosch" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000024-0000-0000-0000-000000000024"), Name = "DeWalt 20V MAX XR 5.0Ah Battery 2-Pack", Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=400&fit=crop", Price = 99.99m, Rating = 4.9, ReviewCount = 2567, InStock = 20, Attributes = new() { { "Brand", "DeWalt" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000025-0000-0000-0000-000000000025"), Name = "DeWalt Titanium Drill Bit Set 29-Piece", Image = "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=400&h=400&fit=crop", Price = 34.99m, Rating = 4.7, ReviewCount = 1834, InStock = 25, Attributes = new() { { "Brand", "DeWalt" }, { "Category", "hand-tools" } } },
    };

    public HomeProductItem? GetProductById(Guid id) => GetAllProducts().FirstOrDefault(p => p.Id == id);

    public List<HomeProductItem> SearchProducts(string query) =>
        GetAllProducts().Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    public List<HomeProductItem> GetProductsByCategory(Guid categoryId)
    {
        var category = GetCategories().FirstOrDefault(c => c.Id == categoryId);
        if (category == null) return GetAllProducts();

        var slug = category.Name.ToLower().Replace(" & ", "-").Replace(" ", "-");
        return GetAllProducts().Where(p =>
            p.Attributes?.TryGetValue("Category", out var cat) == true && cat == slug
        ).ToList();
    }

    public ProductDetail? GetProductDetails(Guid id)
    {
        var products = GetProductDetailsList();
        return products.TryGetValue(id, out var product) ? product : null;
    }

    public List<HomeProductItem> GetRelatedProducts(Guid productId, int count = 4)
    {
        var product = GetProductById(productId);
        if (product == null) return new();

        return GetAllProducts()
            .Where(p => p.Id != productId)
            .Take(count)
            .ToList();
    }

    private Dictionary<Guid, ProductDetail> GetProductDetailsList() => new()
    {
        [Guid.Parse("a0000001-0000-0000-0000-000000000001")] = new("DeWalt", "DeWalt 20V MAX XR Brushless Drill/Driver Kit", "DCD791D2", 149.99m, 179.99m, true, 15, "The DeWalt DCD791D2 20V MAX XR Lithium Ion Brushless Compact Drill/Driver Kit delivers up to 57% more runtime over brushed. The high-speed transmission delivers two speeds for a range of fastening and drilling applications.", new() { "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop", "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&h=600&fit=crop" }, new() { {"Voltage", "20V MAX"}, {"Speed", "0-550/0-2,000 RPM"}, {"Chuck Size", "1/2 inch"}, {"Weight", "3.4 lbs"}, {"Warranty", "3 Years"} }, 4.8, 1247),
        [Guid.Parse("a0000002-0000-0000-0000-000000000002")] = new("Milwaukee", "Milwaukee M18 FUEL Hammer Drill/Driver Kit", "2804-22", 299.99m, 349.99m, true, 8, "The M18 FUEL 1/2\" Hammer Drill/Driver is the industry's most powerful drill, providing up to 60% more power.", new() { "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop" }, new() { {"Voltage", "18V"}, {"Torque", "1,200 in-lbs"}, {"Battery Type", "REDLITHIUM"}, {"Warranty", "5 Years"} }, 4.9, 892),
        [Guid.Parse("a0000003-0000-0000-0000-000000000003")] = new("Makita", "Makita 18V LXT Brushless Impact Driver", "XDT16Z", 179.99m, null, true, 12, "The 18V LXT Brushless 4-Speed Impact Driver delivers faster driving speed with Quick-Shift Mode.", new() { "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&h=600&fit=crop" }, new() { {"Voltage", "18V LXT"}, {"Torque", "1,550 in-lbs"}, {"Warranty", "3 Years"} }, 4.7, 654),
        [Guid.Parse("a0000004-0000-0000-0000-000000000004")] = new("Bosch", "Bosch 12V Flexiclick 5-In-1 Drill System", "GSR12V-300FCB22", 199.99m, 229.99m, true, 6, "The Bosch Flexiclick system features 4 interchangeable attachments.", new() { "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=600&h=600&fit=crop" }, new() { {"Voltage", "12V MAX"}, {"Warranty", "3 Years"} }, 4.6, 423),
        [Guid.Parse("a0000005-0000-0000-0000-000000000005")] = new("DeWalt", "DeWalt 20V MAX 7-1/4\" Circular Saw", "DCS570B", 159.99m, null, true, 10, "Powerful 5200 RPM motor for fast and smooth cuts.", new() { "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=600&h=600&fit=crop" }, new() { {"Voltage", "20V MAX"}, {"Blade Size", "7-1/4 inch"}, {"Speed", "5,200 RPM"} }, 4.7, 534),
        [Guid.Parse("a0000006-0000-0000-0000-000000000006")] = new("Ryobi", "Ryobi ONE+ 18V 6-Tool Combo Kit", "P1819", 299.99m, 449.99m, true, 4, "Includes drill, impact driver, circular saw, reciprocating saw, multi-tool, and LED worklight.", new() { "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=600&h=600&fit=crop" }, new() { {"Voltage", "18V ONE+"}, {"Tools Included", "6"}, {"Warranty", "3 Years"} }, 4.7, 1456),
        [Guid.Parse("a0000007-0000-0000-0000-000000000007")] = new("Klein Tools", "Klein Tools 11-in-1 Screwdriver/Nut Driver", "32500", 24.99m, null, true, 35, "Multi-bit screwdriver with 8 popular tips and 3 nut driver sizes. Made in USA.", new() { "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=600&h=600&fit=crop" }, new() { {"Functions", "11"}, {"Made In", "USA"} }, 4.9, 2089),
        [Guid.Parse("a0000008-0000-0000-0000-000000000008")] = new("CRAFTSMAN", "CRAFTSMAN 320-Piece Mechanic's Tool Set", "CMMT12039", 199.99m, 299.99m, true, 7, "Comprehensive tool set includes sockets, wrenches, screwdrivers, pliers, and more.", new() { "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=600&h=600&fit=crop" }, new() { {"Total Pieces", "320"}, {"Warranty", "Lifetime"} }, 4.8, 987),
        [Guid.Parse("a0000009-0000-0000-0000-000000000009")] = new("Husky", "Husky 52\" 15-Drawer Mobile Workbench", "H52MWC15", 599.99m, 899.99m, true, 3, "Features 15 drawers and 18,000 cu. in. of storage.", new() { "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?w=600&h=600&fit=crop" }, new() { {"Width", "52 inches"}, {"Drawers", "15"}, {"Warranty", "Lifetime"} }, 4.6, 423),
        [Guid.Parse("a0000010-0000-0000-0000-000000000010")] = new("Stanley", "Copper Pipe Fittings Set 1/2\" - 25 Pieces", "CPF-25", 24.99m, 29.99m, true, 25, "Professional-grade copper fittings assortment.", new() { "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&h=600&fit=crop" }, new() { {"Size", "1/2 inch"}, {"Quantity", "25 Pieces"} }, 4.2, 312),
        [Guid.Parse("a0000011-0000-0000-0000-000000000011")] = new("SharkBite", "SharkBite 1/2\" Push-to-Connect Coupling", "U008LFA", 12.99m, null, true, 50, "Works with copper, PEX, CPVC. No soldering required.", new() { "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&h=600&fit=crop" }, new() { {"Size", "1/2 inch"}, {"Warranty", "25 Years"} }, 4.6, 892),
        [Guid.Parse("a0000012-0000-0000-0000-000000000012")] = new("Stanley", "Stanley FatMax Copper Pipe Cutter", "FMHT96545", 34.99m, null, true, 18, "Quick-adjust mechanism for fast pipe sizing.", new() { "https://images.unsplash.com/photo-1581147036324-c17ac41f3f2c?w=600&h=600&fit=crop" }, new() { {"Capacity", "1/8\" to 1-1/8\""}, {"Warranty", "Lifetime"} }, 4.5, 312),
        [Guid.Parse("a0000013-0000-0000-0000-000000000013")] = new("Charlotte Pipe", "PVC Pipe 10ft Schedule 40 - 3/4\" Bundle of 5", "PVC-1040-5", 18.99m, null, true, 30, "Schedule 40 PVC pipe for plumbing.", new() { "https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?w=600&h=600&fit=crop" }, new() { {"Size", "3/4 inch"}, {"Length", "10 ft each"} }, 4.0, 234),
        [Guid.Parse("a0000014-0000-0000-0000-000000000014")] = new("Southwire", "Southwire Romex 12/2 NM-B Wire 250ft", "63947622", 89.99m, 99.99m, true, 12, "Features SIMpull technology for easier pulling. UL Listed.", new() { "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=600&h=600&fit=crop" }, new() { {"Gauge", "12 AWG"}, {"Length", "250 ft"} }, 4.8, 567),
        [Guid.Parse("a0000015-0000-0000-0000-000000000015")] = new("Lithonia", "LED Shop Light 4ft Linkable - 2 Pack", "LSLL-4FT-2PK", 39.99m, null, true, 20, "4400 lumens per fixture. Link up to 4 fixtures.", new() { "https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=600&h=600&fit=crop" }, new() { {"Length", "4 ft"}, {"Lumens", "4,400"} }, 4.5, 456),
        [Guid.Parse("a0000016-0000-0000-0000-000000000016")] = new("Leviton", "Leviton 15A Tamper-Resistant Outlet (10-Pack)", "T5320-WMP", 24.99m, null, true, 40, "Tamper-resistant with shutters. Meets NEC requirements.", new() { "https://images.unsplash.com/photo-1558402529-d2638a7023e9?w=600&h=600&fit=crop" }, new() { {"Amperage", "15A"}, {"Quantity", "10 Pack"} }, 4.7, 1567),
        [Guid.Parse("a0000017-0000-0000-0000-000000000017")] = new("Flexzilla", "Flexzilla Garden Hose 5/8\" x 50ft", "HFZG550YW", 29.99m, null, false, 0, "Lightweight, flexible, kink resistant. Drinking water safe.", new() { "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=600&h=600&fit=crop" }, new() { {"Diameter", "5/8 inch"}, {"Length", "50 ft"} }, 4.4, 1234),
        [Guid.Parse("a0000018-0000-0000-0000-000000000018")] = new("Ryobi", "Ryobi ONE+ 18V Cordless Jet Fan Blower", "P21012", 89.99m, 109.99m, true, 11, "Delivers up to 200 CFM and 150 MPH.", new() { "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop" }, new() { {"Voltage", "18V ONE+"}, {"Air Volume", "200 CFM"} }, 4.1, 543),
        [Guid.Parse("a0000019-0000-0000-0000-000000000019")] = new("3M", "3M Scotch Blue Painter's Tape 1.88\" x 60yd", "2090-48A", 8.99m, null, true, 100, "Medium adhesion. Removes cleanly for 14 days.", new() { "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=600&h=600&fit=crop" }, new() { {"Width", "1.88 inches"}, {"Length", "60 yards"} }, 4.9, 2341),
        [Guid.Parse("a0000020-0000-0000-0000-000000000020")] = new("Gorilla", "Gorilla Heavy Duty Construction Adhesive 9oz", "8010003", 7.49m, null, true, 60, "Tough, versatile, all-weather adhesive.", new() { "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=600&h=600&fit=crop" }, new() { {"Size", "9 oz"}, {"Waterproof", "Yes"} }, 4.7, 1823),
        [Guid.Parse("a0000021-0000-0000-0000-000000000021")] = new("WD-40", "WD-40 Multi-Use Product with Smart Straw 12oz", "490057", 6.99m, null, true, 80, "Protects, penetrates, displaces moisture, and lubricates.", new() { "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=600&h=600&fit=crop" }, new() { {"Size", "12 oz"}, {"Made In", "USA"} }, 4.8, 3156),
        [Guid.Parse("a0000022-0000-0000-0000-000000000022")] = new("RIDGID", "RIDGID 12-Gallon NXT Wet/Dry Shop Vacuum", "HD1200", 79.99m, 119.99m, true, 8, "Powerful 5.0 Peak HP motor with 3-in-1 functionality.", new() { "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop" }, new() { {"Capacity", "12 Gallons"}, {"Peak HP", "5.0"} }, 4.5, 678),
        [Guid.Parse("a0000023-0000-0000-0000-000000000023")] = new("Bosch", "Bosch 2.25 HP Combination Plunge & Fixed Base Router", "1617EVSPK", 199.99m, null, true, 5, "Includes both plunge and fixed bases.", new() { "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=600&h=600&fit=crop" }, new() { {"Horsepower", "2.25 HP"}, {"Warranty", "1 Year"} }, 4.8, 834),
        [Guid.Parse("a0000024-0000-0000-0000-000000000024")] = new("DeWalt", "DeWalt 20V MAX XR 5.0Ah Battery 2-Pack", "DCB205-2", 99.99m, null, true, 20, "Provides up to 60% more capacity than standard batteries.", new() { "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop" }, new() { {"Voltage", "20V MAX"}, {"Capacity", "5.0Ah"} }, 4.9, 2567),
        [Guid.Parse("a0000025-0000-0000-0000-000000000025")] = new("DeWalt", "DeWalt Titanium Drill Bit Set 29-Piece", "DW1369", 34.99m, null, true, 25, "Titanium pilot point drill bits with patented split point tip.", new() { "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop" }, new() { {"Quantity", "29 Pieces"}, {"Coating", "Titanium"} }, 4.7, 1834),
    };

    public record ProductDetail(string Brand, string Name, string Sku, decimal Price, decimal? OriginalPrice, bool InStock, int StockQuantity, string Description, List<string> Images, Dictionary<string, string> Specifications, double Rating, int ReviewCount);
}
