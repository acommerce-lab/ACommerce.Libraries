using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Chats.Core.Entities;

/// <summary>
/// ???? ????? ??????? - ?????? IBaseEntity ?? SharedKernel! ?
/// </summary>
public class MessageRead : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	public Guid MessageId { get; set; }
	public Message? Message { get; set; }

	public required string UserId { get; set; }
	public DateTime ReadAt { get; set; }
}

