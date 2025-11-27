using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Accounting.Core.Entities;

/// <summary>
/// ???? ???????
/// </summary>
public class CostCenter : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// ?????
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// ?????
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// ?????
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// ???? ???? ??????? ???? (null ??????? ???????)
	/// </summary>
	public Guid? ParentCostCenterId { get; set; }
	public CostCenter? ParentCostCenter { get; set; }

	/// <summary>
	/// ????? ??????? ???????
	/// </summary>
	public List<CostCenter> SubCostCenters { get; set; } = new();

	/// <summary>
	/// ??????? ?? ??????
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	/// ?? ????
	/// </summary>
	public bool IsActive { get; set; } = true;

    /// <summary>
    /// ??????? ??????
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();
}

