namespace ACommerce.Localization.Contracts;

/// <summary>
/// واجهة للكيانات القابلة للترجمة
/// </summary>
public interface ITranslatable
{
	/// <summary>
	/// معرف الكيان
	/// </summary>
	Guid Id { get; }

	/// <summary>
	/// نوع الكيان
	/// </summary>
	string GetEntityType();

	/// <summary>
	/// الحقول القابلة للترجمة
	/// </summary>
	IEnumerable<string> GetTranslatableFields();
}

/// <summary>
/// خدمة الترجمة
/// </summary>
public interface ITranslationService
{
	/// <summary>
	/// الحصول على الترجمة
	/// </summary>
	Task<string?> GetTranslationAsync(
		string entityType,
		Guid entityId,
		string fieldName,
		string language,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// حفظ الترجمة
	/// </summary>
	Task SaveTranslationAsync(
		string entityType,
		Guid entityId,
		string fieldName,
		string language,
		string translatedText,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// حذف الترجمة
	/// </summary>
	Task DeleteTranslationAsync(
		string entityType,
		Guid entityId,
		string fieldName,
		string language,
		CancellationToken cancellationToken = default);
}
