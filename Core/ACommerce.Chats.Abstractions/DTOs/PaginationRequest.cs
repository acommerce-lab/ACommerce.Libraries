namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ??? ????? ???????
/// </summary>
public class PaginationRequest
{
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 50;
}

