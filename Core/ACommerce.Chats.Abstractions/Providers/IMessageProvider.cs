using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.SharedKernel.Abstractions.Queries;

namespace ACommerce.Chats.Abstractions.Providers;

/// <summary>
/// ???? ????? ???????
/// </summary>
public interface IMessageProvider
{
	/// <summary>
	/// ????? ?????
	/// </summary>
	Task<MessageDto> SendMessageAsync(
		Guid chatId,
		SendMessageDto dto,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ????? ???????
	/// </summary>
	Task<PagedResult<MessageDto>> GetMessagesAsync(
		Guid chatId,
		PaginationRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ?????
	/// </summary>
	Task<MessageDto?> GetMessageAsync(
		Guid messageId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?????
	/// </summary>
	Task<MessageDto> UpdateMessageAsync(
		Guid messageId,
		UpdateMessageDto dto,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ??? ?????
	/// </summary>
	Task DeleteMessageAsync(
		Guid messageId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??????? ???????
	/// </summary>
	Task MarkAsReadAsync(
		Guid chatId,
		string userId,
		Guid? lastMessageId = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?? ???????
	/// </summary>
	Task<PagedResult<MessageDto>> SearchMessagesAsync(
		Guid chatId,
		string searchQuery,
		PaginationRequest request,
		CancellationToken cancellationToken = default);
}

