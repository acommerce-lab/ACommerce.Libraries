using ACommerce.Client.Categories;
using ACommerce.Client.Products;
using ACommerce.Templates.Customer.Pages;

namespace HamtramckHardware.Shared.Services;

/// <summary>
/// Data service for Hamtramck Hardware store.
/// Connects to real backend APIs via client libraries.
/// Falls back to mock data if API is unavailable.
/// </summary>
public class HamtramckDataService
{
    private readonly ProductsClient _productsClient;
    private readonly CategoriesClient _categoriesClient;

    public HamtramckDataService(ProductsClient productsClient, CategoriesClient categoriesClient)
    {
        _productsClient = productsClient;
        _categoriesClient = categoriesClient;
    }

    #region Categories

    public async Task<List<HomeCategoryItem>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _categoriesClient.GetAllAsync();
            if (categories?.Any() == true)
            {
                return categories.Select(c => new HomeCategoryItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon ?? GetDefaultIcon(c.Name),
                    Image = c.Image,
                    ProductCount = c.ProductsCount
                }).ToList();
            }
        }
        catch { /* Fall back to mock data */ }

        return GetMockCategories();
    }

    public List<HomeCategoryItem> GetCategories() => GetCategoriesAsync().GetAwaiter().GetResult();

    #endregion

    #region Products

    public async Task<List<HomeProductItem>> GetFeaturedProductsAsync()
    {
        try
        {
            var products = await _productsClient.GetFeaturedAsync(10);
            if (products?.Any() == true)
                return products.Select(MapToHomeProductItem).ToList();
        }
        catch { /* Fall back to mock data */ }

        return GetMockFeaturedProducts();
    }

    public List<HomeProductItem> GetFeaturedProducts() => GetFeaturedProductsAsync().GetAwaiter().GetResult();

    public async Task<List<HomeProductItem>> GetBestSellersAsync()
    {
        try
        {
            var result = await _productsClient.SearchAsync(new ProductSearchRequest
            {
                PageSize = 10,
                OrderBy = "SalesCount",
                Ascending = false
            });
            if (result?.Items?.Any() == true)
                return result.Items.Select(MapToHomeProductItem).ToList();
        }
        catch { /* Fall back to mock data */ }

        return GetMockBestSellers();
    }

    public List<HomeProductItem> GetBestSellers() => GetBestSellersAsync().GetAwaiter().GetResult();

    public async Task<List<HomeProductItem>> GetDealsProductsAsync()
    {
        try
        {
            var result = await _productsClient.SearchAsync(new ProductSearchRequest
            {
                PageSize = 10,
                Filters = new List<FilterItem>
                {
                    new() { PropertyName = "HasDiscount", Value = true, Operator = 0 }
                }
            });
            if (result?.Items?.Any() == true)
                return result.Items.Select(MapToHomeProductItem).ToList();
        }
        catch { /* Fall back to mock data */ }

        return GetMockDealsProducts();
    }

    public List<HomeProductItem> GetDealsProducts() => GetDealsProductsAsync().GetAwaiter().GetResult();

    public async Task<List<HomeProductItem>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productsClient.GetAllAsync();
            if (products?.Any() == true)
                return products.Select(MapToHomeProductItem).ToList();
        }
        catch { /* Fall back to mock data */ }

        return GetMockAllProducts();
    }

    public List<HomeProductItem> GetAllProducts() => GetAllProductsAsync().GetAwaiter().GetResult();

    public async Task<HomeProductItem?> GetProductByIdAsync(Guid id)
    {
        try
        {
            var product = await _productsClient.GetByIdAsync(id);
            if (product != null)
                return MapToHomeProductItem(product);
        }
        catch { /* Fall back to mock data */ }

        return GetMockAllProducts().FirstOrDefault(p => p.Id == id);
    }

    public HomeProductItem? GetProductById(Guid id) => GetProductByIdAsync(id).GetAwaiter().GetResult();

    public async Task<List<HomeProductItem>> SearchProductsAsync(string query)
    {
        try
        {
            var result = await _productsClient.SearchAsync(new ProductSearchRequest
            {
                SearchTerm = query,
                PageSize = 50
            });
            if (result?.Items?.Any() == true)
                return result.Items.Select(MapToHomeProductItem).ToList();
        }
        catch { /* Fall back to mock data */ }

        return GetMockAllProducts()
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<HomeProductItem> SearchProducts(string query) => SearchProductsAsync(query).GetAwaiter().GetResult();

    public async Task<List<HomeProductItem>> GetProductsByCategoryAsync(Guid categoryId)
    {
        try
        {
            var result = await _productsClient.SearchAsync(new ProductSearchRequest
            {
                PageSize = 50,
                Filters = new List<FilterItem>
                {
                    new() { PropertyName = "CategoryId", Value = categoryId, Operator = 0 }
                }
            });
            if (result?.Items?.Any() == true)
                return result.Items.Select(MapToHomeProductItem).ToList();
        }
        catch { /* Fall back to mock data */ }

        return GetMockAllProducts();
    }

    public List<HomeProductItem> GetProductsByCategory(Guid categoryId) => GetProductsByCategoryAsync(categoryId).GetAwaiter().GetResult();

    #endregion

    #region Product Details

    public async Task<ProductDetail?> GetProductDetailsAsync(Guid id)
    {
        try
        {
            var product = await _productsClient.GetByIdAsync(id);
            if (product != null)
            {
                return new ProductDetail(
                    Brand: product.Metadata?.GetValueOrDefault("Brand") ?? "Unknown",
                    Name: product.Name,
                    Sku: product.Sku,
                    Price: product.Price ?? 0,
                    OriginalPrice: null,
                    InStock: true,
                    StockQuantity: 10,
                    Description: product.LongDescription ?? product.ShortDescription ?? "",
                    Images: product.Images?.Any() == true ? product.Images : new List<string> { product.FeaturedImage ?? "" },
                    Specifications: product.Metadata ?? new Dictionary<string, string>(),
                    Rating: 4.5,
                    ReviewCount: 0
                );
            }
        }
        catch { /* Fall back to mock data */ }

        return GetMockProductDetails().GetValueOrDefault(id);
    }

    public ProductDetail? GetProductDetails(Guid id) => GetProductDetailsAsync(id).GetAwaiter().GetResult();

    public async Task<List<HomeProductItem>> GetRelatedProductsAsync(Guid productId, int count = 4)
    {
        var allProducts = await GetAllProductsAsync();
        return allProducts.Where(p => p.Id != productId).Take(count).ToList();
    }

    public List<HomeProductItem> GetRelatedProducts(Guid productId, int count = 4) => GetRelatedProductsAsync(productId, count).GetAwaiter().GetResult();

    #endregion

    #region Mapping

    private static HomeProductItem MapToHomeProductItem(ProductDto product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Image = product.FeaturedImage ?? product.Images?.FirstOrDefault(),
        Price = product.Price ?? 0,
        OldPrice = 0,
        Rating = 4.5,
        ReviewCount = 0,
        Attributes = product.Metadata
    };

    private static string GetDefaultIcon(string categoryName) => categoryName.ToLower() switch
    {
        var n when n.Contains("tool") => "bi-tools",
        var n when n.Contains("plumb") => "bi-droplet",
        var n when n.Contains("electr") => "bi-lightning-charge",
        var n when n.Contains("garden") || n.Contains("lawn") => "bi-flower1",
        var n when n.Contains("paint") => "bi-palette",
        var n when n.Contains("lumber") || n.Contains("wood") => "bi-box",
        _ => "bi-grid"
    };

    #endregion

    #region Mock Data (Fallback)

    private static List<HomeCategoryItem> GetMockCategories() => new()
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

    private static List<HomeProductItem> GetMockFeaturedProducts() => new()
    {
        new() { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "DeWalt 20V MAX XR Brushless Drill", Image = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=400&h=400&fit=crop", Price = 149.99m, OldPrice = 179.99m, Rating = 4.8, ReviewCount = 1247, Attributes = new() { { "Brand", "DeWalt" } } },
        new() { Id = Guid.Parse("a0000002-0000-0000-0000-000000000002"), Name = "Milwaukee M18 FUEL Hammer Drill", Image = "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 349.99m, Rating = 4.9, ReviewCount = 892, Attributes = new() { { "Brand", "Milwaukee" } } },
        new() { Id = Guid.Parse("a0000003-0000-0000-0000-000000000003"), Name = "Makita 18V LXT Impact Driver", Image = "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&h=400&fit=crop", Price = 179.99m, Rating = 4.7, ReviewCount = 654, Attributes = new() { { "Brand", "Makita" } } },
        new() { Id = Guid.Parse("a0000004-0000-0000-0000-000000000004"), Name = "Bosch Flexiclick 5-In-1 Drill", Image = "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 229.99m, Rating = 4.6, ReviewCount = 423, Attributes = new() { { "Brand", "Bosch" } } },
    };

    private static List<HomeProductItem> GetMockBestSellers() => new()
    {
        new() { Id = Guid.Parse("a0000019-0000-0000-0000-000000000019"), Name = "3M Scotch Blue Painter's Tape", Image = "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=400&h=400&fit=crop", Price = 8.99m, Rating = 4.9, ReviewCount = 2341, Attributes = new() { { "Brand", "3M" } } },
        new() { Id = Guid.Parse("a0000020-0000-0000-0000-000000000020"), Name = "Gorilla Construction Adhesive", Image = "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=400&h=400&fit=crop", Price = 7.49m, Rating = 4.7, ReviewCount = 1823, Attributes = new() { { "Brand", "Gorilla" } } },
        new() { Id = Guid.Parse("a0000021-0000-0000-0000-000000000021"), Name = "WD-40 Multi-Use Product 12oz", Image = "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400&h=400&fit=crop", Price = 6.99m, Rating = 4.8, ReviewCount = 3156, Attributes = new() { { "Brand", "WD-40" } } },
        new() { Id = Guid.Parse("a0000007-0000-0000-0000-000000000007"), Name = "Klein Tools 11-in-1 Screwdriver", Image = "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=400&h=400&fit=crop", Price = 24.99m, Rating = 4.9, ReviewCount = 2089, Attributes = new() { { "Brand", "Klein" } } },
    };

    private static List<HomeProductItem> GetMockDealsProducts() => new()
    {
        new() { Id = Guid.Parse("a0000006-0000-0000-0000-000000000006"), Name = "Ryobi ONE+ 18V 6-Tool Combo Kit", Image = "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 449.99m, Rating = 4.7, ReviewCount = 1456 },
        new() { Id = Guid.Parse("a0000008-0000-0000-0000-000000000008"), Name = "CRAFTSMAN 320-Piece Tool Set", Image = "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 299.99m, Rating = 4.8, ReviewCount = 987 },
        new() { Id = Guid.Parse("a0000009-0000-0000-0000-000000000009"), Name = "Husky 52\" Tool Chest & Cabinet", Image = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?w=400&h=400&fit=crop", Price = 599.99m, OldPrice = 899.99m, Rating = 4.6, ReviewCount = 423 },
        new() { Id = Guid.Parse("a0000022-0000-0000-0000-000000000022"), Name = "RIDGID 12-Gallon Shop Vacuum", Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=400&fit=crop", Price = 79.99m, OldPrice = 119.99m, Rating = 4.5, ReviewCount = 678 },
    };

    private static List<HomeProductItem> GetMockAllProducts() => new()
    {
        new() { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "DeWalt 20V MAX XR Brushless Drill/Driver Kit", Image = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=400&h=400&fit=crop", Price = 149.99m, OldPrice = 179.99m, Rating = 4.8, ReviewCount = 1247, Attributes = new() { { "Brand", "DeWalt" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000002-0000-0000-0000-000000000002"), Name = "Milwaukee M18 FUEL Hammer Drill/Driver Kit", Image = "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 349.99m, Rating = 4.9, ReviewCount = 892, Attributes = new() { { "Brand", "Milwaukee" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000003-0000-0000-0000-000000000003"), Name = "Makita 18V LXT Brushless Impact Driver", Image = "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&h=400&fit=crop", Price = 179.99m, Rating = 4.7, ReviewCount = 654, Attributes = new() { { "Brand", "Makita" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000004-0000-0000-0000-000000000004"), Name = "Bosch 12V Flexiclick 5-In-1 Drill System", Image = "https://images.unsplash.com/photo-1590122959498-1ccae0d48bc8?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 229.99m, Rating = 4.6, ReviewCount = 423, Attributes = new() { { "Brand", "Bosch" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000005-0000-0000-0000-000000000005"), Name = "DeWalt 20V MAX 7-1/4\" Circular Saw", Image = "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=400&h=400&fit=crop", Price = 159.99m, Rating = 4.7, ReviewCount = 534, Attributes = new() { { "Brand", "DeWalt" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000006-0000-0000-0000-000000000006"), Name = "Ryobi ONE+ 18V 6-Tool Combo Kit", Image = "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=400&h=400&fit=crop", Price = 299.99m, OldPrice = 449.99m, Rating = 4.7, ReviewCount = 1456, Attributes = new() { { "Brand", "Ryobi" }, { "Category", "power-tools" } } },
        new() { Id = Guid.Parse("a0000007-0000-0000-0000-000000000007"), Name = "Klein Tools 11-in-1 Screwdriver/Nut Driver", Image = "https://images.unsplash.com/photo-1426927308491-6380b6a9936f?w=400&h=400&fit=crop", Price = 24.99m, Rating = 4.9, ReviewCount = 2089, Attributes = new() { { "Brand", "Klein" }, { "Category", "hand-tools" } } },
        new() { Id = Guid.Parse("a0000008-0000-0000-0000-000000000008"), Name = "CRAFTSMAN 320-Piece Mechanic's Tool Set", Image = "https://images.unsplash.com/photo-1581244277943-fe4a9c777189?w=400&h=400&fit=crop", Price = 199.99m, OldPrice = 299.99m, Rating = 4.8, ReviewCount = 987, Attributes = new() { { "Brand", "CRAFTSMAN" }, { "Category", "hand-tools" } } },
        new() { Id = Guid.Parse("a0000019-0000-0000-0000-000000000019"), Name = "3M Scotch Blue Painter's Tape 1.88\" x 60yd", Image = "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=400&h=400&fit=crop", Price = 8.99m, Rating = 4.9, ReviewCount = 2341, Attributes = new() { { "Brand", "3M" }, { "Category", "paint" } } },
        new() { Id = Guid.Parse("a0000020-0000-0000-0000-000000000020"), Name = "Gorilla Heavy Duty Construction Adhesive 9oz", Image = "https://images.unsplash.com/photo-1557166983-5939644c3b5e?w=400&h=400&fit=crop", Price = 7.49m, Rating = 4.7, ReviewCount = 1823, Attributes = new() { { "Brand", "Gorilla" }, { "Category", "hardware" } } },
        new() { Id = Guid.Parse("a0000021-0000-0000-0000-000000000021"), Name = "WD-40 Multi-Use Product with Smart Straw 12oz", Image = "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400&h=400&fit=crop", Price = 6.99m, Rating = 4.8, ReviewCount = 3156, Attributes = new() { { "Brand", "WD-40" }, { "Category", "hardware" } } },
    };

    private static Dictionary<Guid, ProductDetail> GetMockProductDetails() => new()
    {
        [Guid.Parse("a0000001-0000-0000-0000-000000000001")] = new("DeWalt", "DeWalt 20V MAX XR Brushless Drill/Driver Kit", "DCD791D2", 149.99m, 179.99m, true, 15, "The DeWalt DCD791D2 20V MAX XR Lithium Ion Brushless Compact Drill/Driver Kit delivers up to 57% more runtime over brushed.", new() { "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop" }, new() { {"Voltage", "20V MAX"}, {"Speed", "0-550/0-2,000 RPM"}, {"Chuck Size", "1/2 inch"} }, 4.8, 1247),
        [Guid.Parse("a0000002-0000-0000-0000-000000000002")] = new("Milwaukee", "Milwaukee M18 FUEL Hammer Drill/Driver Kit", "2804-22", 299.99m, 349.99m, true, 8, "The M18 FUEL 1/2\" Hammer Drill/Driver is the industry's most powerful drill.", new() { "https://images.unsplash.com/photo-1580901368919-7738efb0f87e?w=600&h=600&fit=crop" }, new() { {"Voltage", "18V"}, {"Torque", "1,200 in-lbs"} }, 4.9, 892),
        [Guid.Parse("a0000003-0000-0000-0000-000000000003")] = new("Makita", "Makita 18V LXT Brushless Impact Driver", "XDT16Z", 179.99m, null, true, 12, "The 18V LXT Brushless 4-Speed Impact Driver delivers faster driving speed.", new() { "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&h=600&fit=crop" }, new() { {"Voltage", "18V LXT"}, {"Torque", "1,550 in-lbs"} }, 4.7, 654),
    };

    #endregion

    public record ProductDetail(string Brand, string Name, string Sku, decimal Price, decimal? OriginalPrice, bool InStock, int StockQuantity, string Description, List<string> Images, Dictionary<string, string> Specifications, double Rating, int ReviewCount);
}
