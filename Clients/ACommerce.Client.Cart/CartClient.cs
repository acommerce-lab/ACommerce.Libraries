using ACommerce.Client.Cart.Models;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Cart;

/// <summary>
/// Client للتعامل مع سلة التسوق
/// </summary>
public sealed class CartClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace";

	public CartClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على السلة
	/// </summary>
	public async Task<CartResponse?> GetCartAsync(
		string userIdOrSessionId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<CartResponse>(
			ServiceName,
			$"/api/cart/{userIdOrSessionId}",
			cancellationToken);
	}

	/// <summary>
	/// إضافة منتج للسلة
	/// </summary>
	public async Task<CartResponse?> AddToCartAsync(
		AddToCartRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<AddToCartRequest, CartResponse>(
			ServiceName,
			"/api/cart/add",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث كمية منتج في السلة
	/// </summary>
	public async Task<CartResponse?> UpdateCartItemAsync(
		Guid itemId,
		UpdateCartItemRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateCartItemRequest, CartResponse>(
			ServiceName,
			$"/api/cart/items/{itemId}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// حذف منتج من السلة
	/// </summary>
	public async Task RemoveItemAsync(
		Guid itemId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"/api/cart/items/{itemId}",
			cancellationToken);
	}

	/// <summary>
	/// إفراغ السلة
	/// </summary>
	public async Task ClearCartAsync(
		string userIdOrSessionId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"/api/cart/{userIdOrSessionId}",
			cancellationToken);
	}

	/// <summary>
	/// تطبيق كود خصم
	/// </summary>
	public async Task<CartResponse?> ApplyCouponAsync(
		string userIdOrSessionId,
		ApplyCouponRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<ApplyCouponRequest, CartResponse>(
			ServiceName,
			$"/api/cart/{userIdOrSessionId}/coupon",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إزالة كود الخصم
	/// </summary>
	public async Task<CartResponse?> RemoveCouponAsync(
		string userIdOrSessionId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.DeleteAsync<CartResponse>(
			ServiceName,
			$"/api/cart/{userIdOrSessionId}/coupon",
			cancellationToken);
	}
}

// Extension لـ DynamicHttpClient لدعم DELETE مع Response
public static class HttpClientExtensions
{
	public static async Task<T?> DeleteAsync<T>(
		this DynamicHttpClient httpClient,
		string serviceName,
		string path,
		CancellationToken cancellationToken = default)
	{
		// سنضيف هذا للـ DynamicHttpClient لاحقاً
		throw new NotImplementedException("DeleteAsync<T> not implemented yet");
	}
}
