using ACommerce.Client.Core.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Client.ContactPoints;

/// <summary>
/// Client لإدارة نقاط الاتصال (Contact Points)
/// Email, Phone, Address, Social Media, etc.
/// </summary>
public sealed class ContactPointsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace"; // أو "Users"

	public ContactPointsClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على جميع نقاط الاتصال للمستخدم
	/// </summary>
	public async Task<List<ContactPoint>?> GetMyContactPointsAsync(
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ContactPoint>>(
			ServiceName,
			"/api/contact-points/me",
			cancellationToken);
	}

	/// <summary>
	/// إضافة نقطة اتصال جديدة
	/// </summary>
	public async Task<ContactPoint?> AddContactPointAsync(
		AddContactPointRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<AddContactPointRequest, ContactPoint>(
			ServiceName,
			"/api/contact-points",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث نقطة اتصال
	/// </summary>
	public async Task<ContactPoint?> UpdateContactPointAsync(
		Guid contactPointId,
		UpdateContactPointRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateContactPointRequest, ContactPoint>(
			ServiceName,
			$"/api/contact-points/{contactPointId}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// حذف نقطة اتصال
	/// </summary>
	public async Task DeleteContactPointAsync(
		Guid contactPointId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"/api/contact-points/{contactPointId}",
			cancellationToken);
	}

	/// <summary>
	/// تعيين نقطة اتصال كأساسية (Primary)
	/// </summary>
	public async Task<ContactPoint?> SetPrimaryAsync(
		Guid contactPointId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<object, ContactPoint>(
			ServiceName,
			$"/api/contact-points/{contactPointId}/set-primary",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// التحقق من نقطة اتصال (إرسال OTP)
	/// </summary>
	public async Task<VerificationResponse?> RequestVerificationAsync(
		Guid contactPointId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<object, VerificationResponse>(
			ServiceName,
			$"/api/contact-points/{contactPointId}/request-verification",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// التحقق من الكود (OTP)
	/// </summary>
	public async Task<ContactPoint?> VerifyCodeAsync(
		Guid contactPointId,
		string code,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<VerifyCodeRequest, ContactPoint>(
			ServiceName,
			$"/api/contact-points/{contactPointId}/verify",
			new VerifyCodeRequest { Code = code },
			cancellationToken);
	}
}

// ===== Models =====

public sealed class ContactPoint
{
	public Guid Id { get; set; }
	public string Type { get; set; } = string.Empty; // "Email", "Phone", "Address", "Social"
	public string Value { get; set; } = string.Empty;
	public bool IsPrimary { get; set; }
	public bool IsVerified { get; set; }
	public string Label { get; set; } = string.Empty; // "Home", "Work", "Mobile"
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
}

public sealed class AddContactPointRequest
{
	public string Type { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
	public string Label { get; set; } = string.Empty;
	public bool SetAsPrimary { get; set; }
}

public sealed class UpdateContactPointRequest
{
	public string? Value { get; set; }
	public string? Label { get; set; }
}

public sealed class VerificationResponse
{
	public string Message { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }
}

public sealed class VerifyCodeRequest
{
	public string Code { get; set; } = string.Empty;
}
