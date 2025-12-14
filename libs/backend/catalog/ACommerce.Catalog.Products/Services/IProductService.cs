using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.DTOs.ProductPrice;

namespace ACommerce.Catalog.Products.Services;

/// <summary>
/// ???? ????? ????????
/// </summary>
public interface IProductService
{
	/// <summary>
	/// ?????? ??? ?????? ?????? ???????
	/// </summary>
	Task<ProductDetailResponseDto?> GetProductDetailAsync(
		Guid productId,
		string? currencyCode = null,
		string? market = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??? ??????
	/// </summary>
	Task AddCategoryAsync(
		Guid productId,
		Guid categoryId,
		bool isPrimary = false,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??? ?? ??????
	/// </summary>
	Task RemoveCategoryAsync(
		Guid productId,
		Guid categoryId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ????? ?????? ??????
	/// </summary>
	Task AddBrandAsync(
		Guid productId,
		Guid brandId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ????? ??????
	/// </summary>
	Task AddAttributeAsync(
		Guid productId,
		Guid attributeDefinitionId,
		Guid? attributeValueId = null,
		string? customValue = null,
		bool isVariant = false,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ????? ?? ??????
	/// </summary>
	Task RemoveAttributeAsync(
		Guid productId,
		Guid attributeId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??? ??????
	/// </summary>
	Task AddPriceAsync(
		CreateProductPriceDto dto,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??? ??????
	/// </summary>
	Task UpdatePriceAsync(
		Guid priceId,
		decimal? basePrice = null,
		decimal? salePrice = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ????? ?????? ??????
	/// </summary>
	Task<ProductPriceDto?> GetEffectivePriceAsync(
		Guid productId,
		string currencyCode,
		string? market = null,
		string? customerSegment = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ???? ?????
	/// </summary>
	Task AddRelatedProductAsync(
		Guid productId,
		Guid relatedProductId,
		string relationType = "related",
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??????? ?? ????????
	/// </summary>
	Task<List<ProductResponseDto>> SearchProductsAsync(
		ProductSearchRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ???????? ???????
	/// </summary>
	Task<List<ProductResponseDto>> GetFeaturedProductsAsync(
		int limit = 10,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ???????? ???????
	/// </summary>
	Task<List<ProductResponseDto>> GetNewProductsAsync(
		int limit = 10,
		CancellationToken cancellationToken = default);
}

