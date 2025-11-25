using ACommerce.Client.Cart;
using ACommerce.Client.Cart.Models;

namespace ACommerce.Blazor.Shop.Services;

/// <summary>
/// خدمة حالة السلة - Shared State
/// </summary>
public class CartStateService
{
	private readonly CartClient _cartClient;
	private CartResponse? _currentCart;
	private string _userId = "guest-session-123"; // TODO: من Authentication

	public event Action? OnCartChanged;

	public CartStateService(CartClient cartClient)
	{
		_cartClient = cartClient;
	}

	public CartResponse? CurrentCart => _currentCart;

	public int ItemCount => _currentCart?.Items.Count ?? 0;

	public decimal Total => _currentCart?.Total ?? 0;

	/// <summary>
	/// تحميل السلة
	/// </summary>
	public async Task LoadCartAsync()
	{
		_currentCart = await _cartClient.GetCartAsync(_userId);
		OnCartChanged?.Invoke();
	}

	/// <summary>
	/// إضافة منتج للسلة
	/// </summary>
	public async Task<bool> AddToCartAsync(Guid listingId, int quantity = 1)
	{
		try
		{
			_currentCart = await _cartClient.AddToCartAsync(new AddToCartRequest
			{
				UserIdOrSessionId = _userId,
				ListingId = listingId,
				Quantity = quantity
			});

			OnCartChanged?.Invoke();
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// تحديث الكمية
	/// </summary>
	public async Task<bool> UpdateQuantityAsync(Guid itemId, int quantity)
	{
		try
		{
			_currentCart = await _cartClient.UpdateCartItemAsync(itemId, new UpdateCartItemRequest
			{
				Quantity = quantity
			});

			OnCartChanged?.Invoke();
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// حذف من السلة
	/// </summary>
	public async Task<bool> RemoveFromCartAsync(Guid itemId)
	{
		try
		{
			await _cartClient.RemoveItemAsync(itemId);
			await LoadCartAsync();
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// إفراغ السلة
	/// </summary>
	public async Task<bool> ClearCartAsync()
	{
		try
		{
			await _cartClient.ClearCartAsync(_userId);
			_currentCart = null;
			OnCartChanged?.Invoke();
			return true;
		}
		catch
		{
			return false;
		}
	}
}
