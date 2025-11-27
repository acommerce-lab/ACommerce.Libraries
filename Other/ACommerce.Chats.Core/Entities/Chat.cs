using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.Chats.Abstractions.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Chats.Core.Entities;

/// <summary>
/// ???? ??????? - ?????? IBaseEntity ?? SharedKernel! ?
/// </summary>
public class Chat : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	public required string Title { get; set; }
	public ChatType Type { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }

	/// <summary>
	/// ?????????
	/// </summary>
	public List<ChatParticipant> Participants { get; set; } = new();

	/// <summary>
	/// ???????
	/// </summary>
	public List<Message> Messages { get; set; } = new();

	/// <summary>
	/// ??????? ??????
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();
}

