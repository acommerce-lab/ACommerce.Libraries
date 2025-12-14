using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.Chats.Abstractions.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Chats.Core.Entities;

/// <summary>
/// ???? ??????? - ?????? IBaseEntity ?? SharedKernel! ?
/// </summary>
public class Message : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	public Guid ChatId { get; set; }
	public Chat? Chat { get; set; }

	public required string SenderId { get; set; }
	public required string Content { get; set; }
	public MessageType Type { get; set; } = MessageType.Text;

	public Guid? ReplyToMessageId { get; set; }
	public Message? ReplyToMessage { get; set; }

	[NotMapped]
	public List<string> Attachments { get; set; } = new();

	public bool IsEdited { get; set; }
	public DateTime? EditedAt { get; set; }

	/// <summary>
	/// ?? ??? ???????
	/// </summary>
	public List<MessageRead> ReadBy { get; set; } = new();

	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();
}

