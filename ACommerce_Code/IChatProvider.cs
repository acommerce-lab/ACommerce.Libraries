using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Enums;
using ACommerce.SharedKernel.Abstractions.Queries;

namespace ACommerce.Chats.Abstractions.Providers;

/// <summary>
/// ???? ????? ???????
/// </summary>
public interface IChatProvider
{
	/// <summary>
	/// ????? ????? ?????
	/// </summary>
	Task<ChatDto> CreateChatAsync(
		CreateChatDto dto,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ?????
	/// </summary>
	Task<ChatDto?> GetChatAsync(
		Guid chatId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ?????? ????????
	/// </summary>
	Task<PagedResult<ChatDto>> GetUserChatsAsync(
		string userId,
		PaginationRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?????
	/// </summary>
	Task<ChatDto> UpdateChatAsync(
		Guid chatId,
		UpdateChatDto dto,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ??? ?????
	/// </summary>
	Task DeleteChatAsync(
		Guid chatId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?????
	/// </summary>
	Task<ParticipantDto> AddParticipantAsync(
		Guid chatId,
		AddParticipantDto dto,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?????
	/// </summary>
	Task RemoveParticipantAsync(
		Guid chatId,
		string userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ?????? ???????
	/// </summary>
	Task<List<ParticipantDto>> GetParticipantsAsync(
		Guid chatId,
		CancellationToken cancellationToken = default);
}

public class UpdateChatDto
{
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
}

public class AddParticipantDto
{
	public required string UserId { get; set; }
	public ParticipantRole Role { get; set; } = ParticipantRole.Member;
}

