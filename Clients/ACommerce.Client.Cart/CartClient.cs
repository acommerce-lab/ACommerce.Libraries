using ACommerce.Client.Cart.Models;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Cart;

/// <summary>
/// Client للتعامل مع سلة التسوق
/// </summary>
public sealed class CartClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";
	private const string BasePath = "/api/cart";

	public CartClient(IApiClient httpClient)
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
			$"{BasePath}/{userIdOrSessionId}",
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
			$"{BasePath}/add",
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
			$"{BasePath}/items/{itemId}",
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
			$"{BasePath}/items/{itemId}",
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
			$"{BasePath}/{userIdOrSessionId}",
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
			$"{BasePath}/{userIdOrSessionId}/coupon",
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
			$"{BasePath}/{userIdOrSessionId}/coupon",
			cancellationToken);
	}
}

// Extension لـ IApiClient لدعم DELETE مع Response
public static class HttpClientExtensions
{
	public static async Task<T?> DeleteAsync<T>(
		this IApiClient httpClient,
		string serviceName,
		string path,
		CancellationToken cancellationToken = default)
	{
		// Delete غير مدعوم مع Response حالياً، نحتاج لإضافته للواجهة
		throw new NotImplementedException("DeleteAsync<T> not implemented yet");
	}
}
