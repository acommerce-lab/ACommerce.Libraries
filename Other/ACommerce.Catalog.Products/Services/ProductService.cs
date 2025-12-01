using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.DTOs.ProductPrice;
using ACommerce.Catalog.Products.Entities;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Catalog.Products.Services;

/// <summary>
/// ????? ???? ????? ????????
/// </summary>
public class ProductService : IProductService
{
	private readonly IBaseAsyncRepository<Product> _productRepository;
	private readonly IBaseAsyncRepository<ProductCategory> _categoryRepository;
	private readonly IBaseAsyncRepository<ProductCategoryMapping> _categoryMappingRepository;
	private readonly IBaseAsyncRepository<ProductBrand> _brandRepository;
	private readonly IBaseAsyncRepository<ProductBrandMapping> _brandMappingRepository;
	private readonly IBaseAsyncRepository<ProductAttribute> _attributeRepository;
	private readonly IBaseAsyncRepository<ProductPrice> _priceRepository;
	private readonly IBaseAsyncRepository<ProductInventory> _inventoryRepository;
	private readonly IBaseAsyncRepository<ProductRelation> _relationRepository;
	private readonly ILogger<ProductService> _logger;

	public ProductService(
		IBaseAsyncRepository<Product> productRepository,
		IBaseAsyncRepository<ProductCategory> categoryRepository,
		IBaseAsyncRepository<ProductCategoryMapping> categoryMappingRepository,
		IBaseAsyncRepository<ProductBrand> brandRepository,
		IBaseAsyncRepository<ProductBrandMapping> brandMappingRepository,
		IBaseAsyncRepository<ProductAttribute> attributeRepository,
		IBaseAsyncRepository<ProductPrice> priceRepository,
		IBaseAsyncRepository<ProductInventory> inventoryRepository,
		IBaseAsyncRepository<ProductRelation> relationRepository,
		ILogger<ProductService> logger)
	{
		_productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
		_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
		_categoryMappingRepository = categoryMappingRepository ?? throw new ArgumentNullException(nameof(categoryMappingRepository));
		_brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
		_brandMappingRepository = brandMappingRepository ?? throw new ArgumentNullException(nameof(brandMappingRepository));
		_attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
		_priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
		_inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
		_relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<ProductDetailResponseDto?> GetProductDetailAsync(
		Guid productId,
		string? currencyCode = null,
		string? market = null,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting product detail for {ProductId}", productId);

		// TODO: ????? ???? ??? ???????? ???????
		// ?? ???? ???????? (Categories, Brands, Attributes, Prices, Inventory, etc.)

		throw new NotImplementedException("GetProductDetailAsync implementation pending");
	}

	public async Task AddCategoryAsync(
		Guid productId,
		Guid categoryId,
		bool isPrimary = false,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Adding category {CategoryId} to product {ProductId}", categoryId, productId);

		// ?????? ?? ???? ?????? ??????
		var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
		if (product == null)
			throw new ArgumentException($"Product {productId} not found");

		var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
		if (category == null)
			throw new ArgumentException($"Category {categoryId} not found");

		// ??? ???? primary? ????? primary ?? ???? ??????
		if (isPrimary)
		{
			var existingMappings = await _categoryMappingRepository.GetAllWithPredicateAsync(
				m => m.ProductId == productId && m.IsPrimary,
				includeDeleted: false);

			foreach (var mapping in existingMappings)
			{
				mapping.IsPrimary = false;
				await _categoryMappingRepository.UpdateAsync(mapping, cancellationToken);
			}
		}

		// ????? ?????
		var newMapping = new ProductCategoryMapping
		{
			ProductId = productId,
			CategoryId = categoryId,
			IsPrimary = isPrimary
		};

		await _categoryMappingRepository.AddAsync(newMapping, cancellationToken);

		_logger.LogInformation("Added category {CategoryId} to product {ProductId}", categoryId, productId);
	}

	public async Task RemoveCategoryAsync(
		Guid productId,
		Guid categoryId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Removing category {CategoryId} from product {ProductId}", categoryId, productId);

		var mapping = await _categoryMappingRepository.GetAllWithPredicateAsync(
			m => m.ProductId == productId && m.CategoryId == categoryId,
			includeDeleted: false);

		var categoryMapping = mapping.FirstOrDefault();
		if (categoryMapping != null)
		{
			await _categoryMappingRepository.DeleteAsync(categoryMapping.Id, cancellationToken);
			_logger.LogInformation("Removed category {CategoryId} from product {ProductId}", categoryId, productId);
		}
	}

	public async Task AddBrandAsync(
		Guid productId,
		Guid brandId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Adding brand {BrandId} to product {ProductId}", brandId, productId);

		// ?????? ?? ???? ?????? ????????
		var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
		if (product == null)
			throw new ArgumentException($"Product {productId} not found");

		var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
		if (brand == null)
			throw new ArgumentException($"Brand {brandId} not found");

		// ????? ?????
		var mapping = new ProductBrandMapping
		{
			ProductId = productId,
			BrandId = brandId
		};

		await _brandMappingRepository.AddAsync(mapping, cancellationToken);

		_logger.LogInformation("Added brand {BrandId} to product {ProductId}", brandId, productId);
	}

	public async Task AddAttributeAsync(
		Guid productId,
		Guid attributeDefinitionId,
		Guid? attributeValueId = null,
		string? customValue = null,
		bool isVariant = false,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Adding attribute {AttributeDefinitionId} to product {ProductId}", attributeDefinitionId, productId);

		var attribute = new ProductAttribute
		{
			ProductId = productId,
			AttributeDefinitionId = attributeDefinitionId,
			AttributeValueId = attributeValueId,
			CustomValue = customValue,
			IsVariant = isVariant
		};

		await _attributeRepository.AddAsync(attribute, cancellationToken);

		_logger.LogInformation("Added attribute {AttributeDefinitionId} to product {ProductId}", attributeDefinitionId, productId);
	}

	public async Task RemoveAttributeAsync(
		Guid productId,
		Guid attributeId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Removing attribute {AttributeId} from product {ProductId}", attributeId, productId);

		await _attributeRepository.DeleteAsync(attributeId, cancellationToken);

		_logger.LogInformation("Removed attribute {AttributeId} from product {ProductId}", attributeId, productId);
	}

	public async Task AddPriceAsync(
		CreateProductPriceDto dto,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Adding price for product {ProductId} in currency {CurrencyId}", dto.ProductId, dto.CurrencyId);

		var price = new ProductPrice
		{
			ProductId = dto.ProductId,
			CurrencyId = dto.CurrencyId,
			BasePrice = dto.BasePrice,
			SalePrice = dto.SalePrice,
			DiscountPercentage = dto.DiscountPercentage,
			SaleStartDate = dto.SaleStartDate,
			SaleEndDate = dto.SaleEndDate,
			Market = dto.Market,
			CustomerSegment = dto.CustomerSegment,
			MinQuantity = dto.MinQuantity,
			MaxQuantity = dto.MaxQuantity,
			Metadata = dto.Metadata ?? new Dictionary<string, string>()
		};

		await _priceRepository.AddAsync(price, cancellationToken);

		_logger.LogInformation("Added price for product {ProductId}", dto.ProductId);
	}

	public async Task UpdatePriceAsync(
		Guid priceId,
		decimal? basePrice = null,
		decimal? salePrice = null,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Updating price {PriceId}", priceId);

		var price = await _priceRepository.GetByIdAsync(priceId, cancellationToken);
		if (price == null)
			throw new ArgumentException($"Price {priceId} not found");

		if (basePrice.HasValue)
			price.BasePrice = basePrice.Value;

		if (salePrice.HasValue)
			price.SalePrice = salePrice.Value;

		await _priceRepository.UpdateAsync(price, cancellationToken);

		_logger.LogInformation("Updated price {PriceId}", priceId);
	}

	public async Task<ProductPriceDto?> GetEffectivePriceAsync(
		Guid productId,
		string currencyCode,
		string? market = null,
		string? customerSegment = null,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting effective price for product {ProductId} in {CurrencyCode}", productId, currencyCode);

		// TODO: ????? ???? ?????? ??? ????? ??????
		// ?? ????? ?? ????????: Currency, Market, CustomerSegment, Sale dates

		throw new NotImplementedException("GetEffectivePriceAsync implementation pending");
	}

	public async Task AddRelatedProductAsync(
		Guid productId,
		Guid relatedProductId,
		string relationType = "related",
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Adding related product {RelatedProductId} to product {ProductId}", relatedProductId, productId);

		var relation = new ProductRelation
		{
			SourceProductId = productId,
			RelatedProductId = relatedProductId,
			RelationType = relationType
		};

		await _relationRepository.AddAsync(relation, cancellationToken);

		_logger.LogInformation("Added related product {RelatedProductId} to product {ProductId}", relatedProductId, productId);
	}

	public async Task<List<ProductResponseDto>> SearchProductsAsync(
		ProductSearchRequest request,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Searching products with query {Query}", request.Query);

		// TODO: ????? ???? ????? ???????

		throw new NotImplementedException("SearchProductsAsync implementation pending");
	}

	public async Task<List<ProductResponseDto>> GetFeaturedProductsAsync(
		int limit = 10,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting featured products, limit: {Limit}", limit);

		var products = await _productRepository.GetAllWithPredicateAsync(
			p => p.IsFeatured && p.Status == Enums.ProductStatus.Active && !p.IsDeleted,
			orderBy: q => q.OrderByDescending(p => p.CreatedAt),
			take: limit);

		return products.Select(MapToResponseDto).ToList();
	}

	public async Task<List<ProductResponseDto>> GetNewProductsAsync(
		int limit = 10,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Getting new products, limit: {Limit}", limit);

		var now = DateTime.UtcNow;
		var products = await _productRepository.GetAllWithPredicateAsync(
			p => p.IsNew && p.Status == Enums.ProductStatus.Active && !p.IsDeleted
				&& (p.NewUntil == null || p.NewUntil > now),
			orderBy: q => q.OrderByDescending(p => p.CreatedAt),
			take: limit);

		return products.Select(MapToResponseDto).ToList();
	}

	private static ProductResponseDto MapToResponseDto(Product product)
	{
		return new ProductResponseDto
		{
			Id = product.Id,
			Name = product.Name,
			Sku = product.Sku,
			Type = product.Type,
			Status = product.Status,
			ShortDescription = product.ShortDescription,
			LongDescription = product.LongDescription,
			Barcode = product.Barcode,
			Weight = product.Weight,
			WeightUnitId = product.WeightUnitId,
			Length = product.Length,
			Width = product.Width,
			Height = product.Height,
			DimensionUnitId = product.DimensionUnitId,
			Images = product.Images ?? new List<string>(),
			FeaturedImage = product.FeaturedImage,
			Tags = product.Tags ?? new List<string>(),
			IsFeatured = product.IsFeatured,
			IsNew = product.IsNew,
			NewUntil = product.NewUntil,
			ParentProductId = product.ParentProductId,
			Metadata = product.Metadata ?? new Dictionary<string, string>(),
			CreatedAt = product.CreatedAt,
			UpdatedAt = product.UpdatedAt
		};
	}
}

