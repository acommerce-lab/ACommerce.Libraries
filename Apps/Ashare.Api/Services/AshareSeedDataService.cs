using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Products.Enums;
using ACommerce.Catalog.Attributes.Entities;
using ACommerce.Catalog.Attributes.Enums;
using ACommerce.Catalog.Currencies.Entities;

namespace Ashare.Api.Services;

/// <summary>
/// خدمة البيانات الأولية لمنصة عشير
/// تضيف فئات المساحات والخصائص الديناميكية لكل فئة
/// </summary>
public class AshareSeedDataService
{
	private readonly IRepositoryFactory _repositoryFactory;

	// Pre-defined IDs for consistency
	public static class CategoryIds
	{
		public static readonly Guid Residential = Guid.Parse("10000000-0000-0000-0001-000000000001");
		public static readonly Guid LookingForHousing = Guid.Parse("10000000-0000-0000-0001-000000000002");
		public static readonly Guid LookingForPartner = Guid.Parse("10000000-0000-0000-0001-000000000003");
		public static readonly Guid Administrative = Guid.Parse("10000000-0000-0000-0001-000000000004");
		public static readonly Guid Commercial = Guid.Parse("10000000-0000-0000-0001-000000000005");
	}

	public static class CurrencyIds
	{
		public static readonly Guid SAR = Guid.Parse("40000000-0000-0000-0001-000000000001");
		public static readonly Guid USD = Guid.Parse("40000000-0000-0000-0001-000000000002");
	}

	public static class ProductIds
	{
		// Residential spaces
		public static readonly Guid Apartment1 = Guid.Parse("30000000-0000-0000-0001-000000000001");
		public static readonly Guid Apartment2 = Guid.Parse("30000000-0000-0000-0001-000000000002");
		public static readonly Guid Villa1 = Guid.Parse("30000000-0000-0000-0001-000000000003");

		// Commercial spaces
		public static readonly Guid Office1 = Guid.Parse("30000000-0000-0000-0002-000000000001");
		public static readonly Guid Coworking1 = Guid.Parse("30000000-0000-0000-0002-000000000002");
		public static readonly Guid MeetingRoom1 = Guid.Parse("30000000-0000-0000-0002-000000000003");

		// Administrative spaces
		public static readonly Guid AdminOffice1 = Guid.Parse("30000000-0000-0000-0003-000000000001");
		public static readonly Guid AdminOffice2 = Guid.Parse("30000000-0000-0000-0003-000000000002");
	}

	public static class AttributeIds
	{
		// ═══════════════════════════════════════════════════════════════════
		// Common attributes for all categories
		// ═══════════════════════════════════════════════════════════════════
		public static readonly Guid Title = Guid.Parse("20000000-0000-0000-0001-000000000001");
		public static readonly Guid Description = Guid.Parse("20000000-0000-0000-0001-000000000002");
		public static readonly Guid Price = Guid.Parse("20000000-0000-0000-0001-000000000003");
		public static readonly Guid Duration = Guid.Parse("20000000-0000-0000-0001-000000000004");
		public static readonly Guid TimeUnit = Guid.Parse("20000000-0000-0000-0001-000000000005");
		public static readonly Guid Location = Guid.Parse("20000000-0000-0000-0001-000000000006");
		public static readonly Guid City = Guid.Parse("20000000-0000-0000-0001-000000000007");
		public static readonly Guid Images = Guid.Parse("20000000-0000-0000-0001-000000000008");

		// ═══════════════════════════════════════════════════════════════════
		// Residential offer attributes (عرض سكني)
		// ═══════════════════════════════════════════════════════════════════
		public static readonly Guid PropertyType = Guid.Parse("20000000-0000-0000-0002-000000000001");
		public static readonly Guid UnitType = Guid.Parse("20000000-0000-0000-0002-000000000002");
		public static readonly Guid Floor = Guid.Parse("20000000-0000-0000-0002-000000000003");
		public static readonly Guid BillType = Guid.Parse("20000000-0000-0000-0002-000000000004");
		public static readonly Guid RentalType = Guid.Parse("20000000-0000-0000-0002-000000000005");
		public static readonly Guid Area = Guid.Parse("20000000-0000-0000-0002-000000000006");
		public static readonly Guid Rooms = Guid.Parse("20000000-0000-0000-0002-000000000007");
		public static readonly Guid Bathrooms = Guid.Parse("20000000-0000-0000-0002-000000000008");
		public static readonly Guid Furnished = Guid.Parse("20000000-0000-0000-0002-000000000009");
		public static readonly Guid Amenities = Guid.Parse("20000000-0000-0000-0002-000000000010");

		// ═══════════════════════════════════════════════════════════════════
		// Contact preferences (تفضيلات التواصل)
		// ═══════════════════════════════════════════════════════════════════
		public static readonly Guid IsPhoneAllowed = Guid.Parse("20000000-0000-0000-0002-000000000011");
		public static readonly Guid IsWhatsAppAllowed = Guid.Parse("20000000-0000-0000-0002-000000000012");
		public static readonly Guid IsMessagingAllowed = Guid.Parse("20000000-0000-0000-0002-000000000013");

		// ═══════════════════════════════════════════════════════════════════
		// Partner request attributes (طلب شريك سكن)
		// ═══════════════════════════════════════════════════════════════════
		public static readonly Guid PersonalName = Guid.Parse("20000000-0000-0000-0003-000000000001");
		public static readonly Guid Age = Guid.Parse("20000000-0000-0000-0003-000000000002");
		public static readonly Guid Gender = Guid.Parse("20000000-0000-0000-0003-000000000003");
		public static readonly Guid Nationality = Guid.Parse("20000000-0000-0000-0003-000000000004");
		public static readonly Guid Job = Guid.Parse("20000000-0000-0000-0003-000000000005");
		public static readonly Guid MinPrice = Guid.Parse("20000000-0000-0000-0003-000000000006");
		public static readonly Guid MaxPrice = Guid.Parse("20000000-0000-0000-0003-000000000007");
		public static readonly Guid Smoking = Guid.Parse("20000000-0000-0000-0003-000000000008");

		// ═══════════════════════════════════════════════════════════════════
		// Commercial/Administrative attributes (تجاري/إداري)
		// ═══════════════════════════════════════════════════════════════════
		public static readonly Guid CommercialPropertyType = Guid.Parse("20000000-0000-0000-0004-000000000001");
		public static readonly Guid Capacity = Guid.Parse("20000000-0000-0000-0004-000000000002");
		public static readonly Guid Parking = Guid.Parse("20000000-0000-0000-0004-000000000003");
		public static readonly Guid WorkingHours = Guid.Parse("20000000-0000-0000-0004-000000000004");
		public static readonly Guid Facilities = Guid.Parse("20000000-0000-0000-0004-000000000005");
	}

	/// <summary>
	/// خريطة الخصائص لكل فئة
	/// </summary>
	public static readonly Dictionary<Guid, List<Guid>> CategoryAttributeMappings = new()
	{
		// ═══════════════════════════════════════════════════════════════════
		// سكني (عرض سكن) - Residential Offer
		// ═══════════════════════════════════════════════════════════════════
		[CategoryIds.Residential] = new List<Guid>
		{
			AttributeIds.Title,           // عنوان العرض
			AttributeIds.Description,     // الوصف
			AttributeIds.Price,           // السعر
			AttributeIds.Duration,        // المدة
			AttributeIds.TimeUnit,        // وحدة الوقت (شهر/أسبوع/يوم)
			AttributeIds.PropertyType,    // نوع العقار (فيلا/عمارة)
			AttributeIds.UnitType,        // نوع الوحدة (شقة/استوديو/غرفة) - للعمارة فقط
			AttributeIds.Floor,           // الطابق
			AttributeIds.BillType,        // نوع الإعلان (عرض/طلب)
			AttributeIds.RentalType,      // نوع الإيجار (مشترك/كامل)
			AttributeIds.Area,            // المساحة (اختياري)
			AttributeIds.Rooms,           // عدد الغرف (للشقق فقط)
			AttributeIds.Bathrooms,       // عدد الحمامات (للشقق فقط)
			AttributeIds.Furnished,       // التأثيث
			AttributeIds.Amenities,       // المرافق
			AttributeIds.City,            // المدينة
			AttributeIds.Location,        // الموقع (عنوان + إحداثيات)
			AttributeIds.IsPhoneAllowed,  // السماح بالاتصال
			AttributeIds.IsWhatsAppAllowed, // السماح بواتساب
			AttributeIds.IsMessagingAllowed, // السماح بالرسائل
			AttributeIds.Images           // الصور
		},

		// ═══════════════════════════════════════════════════════════════════
		// طلب سكن - Looking for Housing (same as residential but reversed)
		// ═══════════════════════════════════════════════════════════════════
		[CategoryIds.LookingForHousing] = new List<Guid>
		{
			AttributeIds.Title,           // عنوان الطلب
			AttributeIds.Description,     // الوصف
			AttributeIds.MinPrice,        // أقل ميزانية
			AttributeIds.MaxPrice,        // أعلى ميزانية
			AttributeIds.PropertyType,    // نوع العقار المطلوب
			AttributeIds.UnitType,        // نوع الوحدة المطلوبة
			AttributeIds.Rooms,           // عدد الغرف المطلوب
			AttributeIds.Furnished,       // التأثيث المطلوب
			AttributeIds.City,            // المدينة
			AttributeIds.Location,        // الموقع المفضل
			AttributeIds.IsPhoneAllowed,
			AttributeIds.IsWhatsAppAllowed,
			AttributeIds.IsMessagingAllowed
		},

		// ═══════════════════════════════════════════════════════════════════
		// طلب شريك سكن - Looking for Roommate/Partner
		// ═══════════════════════════════════════════════════════════════════
		[CategoryIds.LookingForPartner] = new List<Guid>
		{
			AttributeIds.PersonalName,    // الاسم الشخصي
			AttributeIds.Age,             // العمر (رقم)
			AttributeIds.Gender,          // الجنس
			AttributeIds.Nationality,     // الجنسية
			AttributeIds.Job,             // المهنة/الوظيفة
			AttributeIds.City,            // المدينة
			AttributeIds.MinPrice,        // أقل ميزانية
			AttributeIds.MaxPrice,        // أعلى ميزانية
			AttributeIds.Furnished,       // هل يفضل مفروش؟
			AttributeIds.Smoking,         // حالة التدخين
			AttributeIds.Description,     // وصف إضافي
			AttributeIds.IsPhoneAllowed,
			AttributeIds.IsWhatsAppAllowed,
			AttributeIds.IsMessagingAllowed,
			AttributeIds.Images           // صور شخصية (اختياري)
		},

		// ═══════════════════════════════════════════════════════════════════
		// مساحة إدارية - Administrative Space
		// PropertyType = عمارة فقط، بدون غرف/حمامات
		// ═══════════════════════════════════════════════════════════════════
		[CategoryIds.Administrative] = new List<Guid>
		{
			AttributeIds.Title,
			AttributeIds.Description,
			AttributeIds.Price,
			AttributeIds.Duration,
			AttributeIds.TimeUnit,
			AttributeIds.PropertyType,    // عمارة فقط
			AttributeIds.Floor,
			AttributeIds.Area,
			AttributeIds.Capacity,        // السعة (عدد الأشخاص)
			AttributeIds.Parking,         // المواقف
			AttributeIds.WorkingHours,    // ساعات العمل
			AttributeIds.Facilities,      // التجهيزات
			AttributeIds.City,
			AttributeIds.Location,
			AttributeIds.IsPhoneAllowed,
			AttributeIds.IsWhatsAppAllowed,
			AttributeIds.IsMessagingAllowed,
			AttributeIds.Images
		},

		// ═══════════════════════════════════════════════════════════════════
		// مساحة تجارية - Commercial Space
		// PropertyType = محل/مجمع/مول، بدون غرف/حمامات
		// ═══════════════════════════════════════════════════════════════════
		[CategoryIds.Commercial] = new List<Guid>
		{
			AttributeIds.Title,
			AttributeIds.Description,
			AttributeIds.Price,
			AttributeIds.Duration,
			AttributeIds.TimeUnit,
			AttributeIds.CommercialPropertyType, // محل/مجمع/مول
			AttributeIds.Floor,
			AttributeIds.Area,
			AttributeIds.Capacity,
			AttributeIds.Parking,
			AttributeIds.Facilities,
			AttributeIds.City,
			AttributeIds.Location,
			AttributeIds.IsPhoneAllowed,
			AttributeIds.IsWhatsAppAllowed,
			AttributeIds.IsMessagingAllowed,
			AttributeIds.Images
		}
	};

	public AshareSeedDataService(IRepositoryFactory repositoryFactory)
	{
		_repositoryFactory = repositoryFactory;
	}

	/// <summary>
	/// الحصول على معرفات الخصائص لفئة معينة
	/// </summary>
	public static List<Guid> GetAttributeIdsForCategory(Guid categoryId)
	{
		return CategoryAttributeMappings.TryGetValue(categoryId, out var attributeIds)
			? attributeIds
			: new List<Guid>();
	}

	public async Task SeedAsync()
	{
		await SeedCurrenciesAsync();
		await SeedCategoriesAsync();
		await SeedAttributeDefinitionsAsync();
		await SeedCategoryAttributeMappingsAsync();
		await SeedProductsAsync();
		await SeedProductPricesAsync();
	}

	private async Task SeedCurrenciesAsync()
	{
		var repo = _repositoryFactory.CreateRepository<Currency>();

		var existing = await repo.GetAllWithPredicateAsync();
		var existingIds = existing.Select(c => c.Id).ToHashSet();

		// تأكد من وجود العملات المطلوبة بالمعرّفات الصحيحة
		if (!existingIds.Contains(CurrencyIds.SAR))
		{
			await repo.AddAsync(new Currency
			{
				Id = CurrencyIds.SAR,
				Code = "SAR",
				Name = "الريال السعودي",
				Symbol = "ر.س",
				DecimalPlaces = 2,
				SymbolBeforeAmount = false,
				ThousandsSeparator = ",",
				DecimalSeparator = ".",
				IsBaseCurrency = true,
				IsActive = true,
				SortOrder = 1,
				CreatedAt = DateTime.UtcNow
			});
		}

		if (!existingIds.Contains(CurrencyIds.USD))
		{
			await repo.AddAsync(new Currency
			{
				Id = CurrencyIds.USD,
				Code = "USD",
				Name = "الدولار الأمريكي",
				Symbol = "$",
				DecimalPlaces = 2,
				SymbolBeforeAmount = true,
				ThousandsSeparator = ",",
				DecimalSeparator = ".",
				IsBaseCurrency = false,
				IsActive = true,
				SortOrder = 2,
				CreatedAt = DateTime.UtcNow
			});
		}
	}

	private async Task SeedCategoriesAsync()
	{
		var repo = _repositoryFactory.CreateRepository<ProductCategory>();

		var existing = await repo.GetAllWithPredicateAsync();
		var existingIds = existing.Select(c => c.Id).ToHashSet();

		var categories = new List<ProductCategory>
		{
			new()
			{
				Id = CategoryIds.Residential,
				Name = "سكني",
				Slug = "residential",
				Description = "عرض سكن للإيجار أو المشاركة",
				Icon = "bi-house-door",
				SortOrder = 1,
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = CategoryIds.LookingForHousing,
				Name = "طلب سكن",
				Slug = "looking-for-housing",
				Description = "أبحث عن سكن للإيجار أو المشاركة",
				Icon = "bi-search",
				SortOrder = 2,
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = CategoryIds.LookingForPartner,
				Name = "طلب شريك سكن",
				Slug = "looking-for-partner",
				Description = "أبحث عن شريك للسكن المشترك",
				Icon = "bi-people",
				SortOrder = 3,
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = CategoryIds.Administrative,
				Name = "مساحة إدارية",
				Slug = "administrative",
				Description = "مكاتب ومساحات عمل مشتركة",
				Icon = "bi-building",
				SortOrder = 4,
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = CategoryIds.Commercial,
				Name = "مساحة تجارية",
				Slug = "commercial",
				Description = "محلات ومستودعات ومساحات تجارية",
				Icon = "bi-shop",
				SortOrder = 5,
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			}
		};

		foreach (var category in categories)
		{
			if (!existingIds.Contains(category.Id))
			{
				await repo.AddAsync(category);
			}
		}
	}

	private async Task SeedAttributeDefinitionsAsync()
	{
		var repo = _repositoryFactory.CreateRepository<AttributeDefinition>();

		var existing = await repo.GetAllWithPredicateAsync();
		var existingIds = existing.Select(a => a.Id).ToHashSet();

		var attributes = GetAllAttributeDefinitions();

		foreach (var attr in attributes)
		{
			if (!existingIds.Contains(attr.Id))
			{
				await repo.AddAsync(attr);
			}
		}
	}

	/// <summary>
	/// إضافة ربطات الفئات بالخصائص في قاعدة البيانات
	/// </summary>
	private async Task SeedCategoryAttributeMappingsAsync()
	{
		var repo = _repositoryFactory.CreateRepository<CategoryAttributeMapping>();

		var existing = await repo.GetAllWithPredicateAsync();
		var existingPairs = existing.Select(m => (m.CategoryId, m.AttributeDefinitionId)).ToHashSet();

		var mappingsToAdd = new List<CategoryAttributeMapping>();

		foreach (var (categoryId, attributeIds) in CategoryAttributeMappings)
		{
			for (int i = 0; i < attributeIds.Count; i++)
			{
				var attributeId = attributeIds[i];

				// تجنب الإضافة المكررة
				if (!existingPairs.Contains((categoryId, attributeId)))
				{
					mappingsToAdd.Add(new CategoryAttributeMapping
					{
						Id = Guid.NewGuid(),
						CategoryId = categoryId,
						AttributeDefinitionId = attributeId,
						SortOrder = i + 1,
						IsActive = true,
						CreatedAt = DateTime.UtcNow
					});
				}
			}
		}

		foreach (var mapping in mappingsToAdd)
		{
			await repo.AddAsync(mapping);
		}

		Console.WriteLine($"[Seed] Added {mappingsToAdd.Count} category-attribute mappings");
	}

	private List<AttributeDefinition> GetAllAttributeDefinitions()
	{
		var attributes = new List<AttributeDefinition>();

		// Common attributes for all categories
		attributes.AddRange(GetCommonAttributes());

		// Residential-specific attributes
		attributes.AddRange(GetResidentialAttributes());

		// Partner request attributes
		attributes.AddRange(GetPartnerAttributes());

		// Commercial/Administrative attributes
		attributes.AddRange(GetCommercialAttributes());

		return attributes;
	}

	private List<AttributeDefinition> GetCommonAttributes()
	{
		return new List<AttributeDefinition>
		{
			// ═══════════════════════════════════════════════════════════════════
			// العنوان
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Title,
				Name = "العنوان",
				Code = "title",
				Type = AttributeType.Text,
				Description = "عنوان الإعلان",
				IsRequired = true,
				IsFilterable = false,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 1,
				ValidationRules = "{\"minLength\": 10, \"maxLength\": 200}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// الوصف
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Description,
				Name = "الوصف",
				Code = "description",
				Type = AttributeType.LongText,
				Description = "وصف تفصيلي للإعلان",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 2,
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// السعر
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Price,
				Name = "السعر (ريال)",
				Code = "price",
				Type = AttributeType.Number,
				Description = "سعر الإيجار بالريال السعودي",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 3,
				ValidationRules = "{\"min\": 0, \"max\": 10000000}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// المدة
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Duration,
				Name = "مدة الإيجار",
				Code = "duration",
				Type = AttributeType.Number,
				Description = "مدة الإيجار",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 4,
				DefaultValue = "1",
				ValidationRules = "{\"min\": 1, \"max\": 365}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// وحدة الوقت
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.TimeUnit,
				Name = "وحدة الوقت",
				Code = "time_unit",
				Type = AttributeType.SingleSelect,
				Description = "وحدة قياس مدة الإيجار",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 5,
				DefaultValue = "month",
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "day", DisplayName = "يوم", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "week", DisplayName = "أسبوع", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "month", DisplayName = "شهر", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "year", DisplayName = "سنة", SortOrder = 4, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// المدينة
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.City,
				Name = "المدينة",
				Code = "city",
				Type = AttributeType.SingleSelect,
				Description = "المدينة",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 6,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "riyadh", DisplayName = "الرياض", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "jeddah", DisplayName = "جدة", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "makkah", DisplayName = "مكة المكرمة", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "madinah", DisplayName = "المدينة المنورة", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "dammam", DisplayName = "الدمام", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "khobar", DisplayName = "الخبر", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "dhahran", DisplayName = "الظهران", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "taif", DisplayName = "الطائف", SortOrder = 8, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "abha", DisplayName = "أبها", SortOrder = 9, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "tabuk", DisplayName = "تبوك", SortOrder = 10, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "buraidah", DisplayName = "بريدة", SortOrder = 11, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "khamis_mushait", DisplayName = "خميس مشيط", SortOrder = 12, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "hail", DisplayName = "حائل", SortOrder = 13, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "najran", DisplayName = "نجران", SortOrder = 14, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "jazan", DisplayName = "جازان", SortOrder = 15, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// الموقع (عنوان)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Location,
				Name = "العنوان التفصيلي",
				Code = "location",
				Type = AttributeType.Text,
				Description = "العنوان التفصيلي للموقع",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 7,
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// الصور (سيتم معالجتها بشكل منفصل)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Images,
				Name = "الصور",
				Code = "images",
				Type = AttributeType.Text, // JSON array of URLs
				Description = "صور الإعلان",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 100,
				CreatedAt = DateTime.UtcNow
			}
		};
	}

	private List<AttributeDefinition> GetResidentialAttributes()
	{
		return new List<AttributeDefinition>
		{
			// ═══════════════════════════════════════════════════════════════════
			// نوع العقار (فيلا/عمارة)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.PropertyType,
				Name = "نوع العقار",
				Code = "property_type",
				Type = AttributeType.SingleSelect,
				Description = "نوع العقار (فيلا أو عمارة)",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 10,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "villa", DisplayName = "فيلا", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "building", DisplayName = "عمارة", SortOrder = 2, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// نوع الوحدة (للعمارة فقط)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.UnitType,
				Name = "نوع الوحدة",
				Code = "unit_type",
				Type = AttributeType.SingleSelect,
				Description = "نوع الوحدة السكنية (للعمارة فقط)",
				IsRequired = false, // مطلوب فقط إذا كان PropertyType = building
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 11,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "apartment", DisplayName = "شقة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "studio", DisplayName = "استوديو", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "room", DisplayName = "غرفة", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "duplex", DisplayName = "دوبلكس", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "penthouse", DisplayName = "بنتهاوس", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "full_floor", DisplayName = "دور كامل", SortOrder = 6, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// الطابق
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Floor,
				Name = "الطابق",
				Code = "floor",
				Type = AttributeType.SingleSelect,
				Description = "رقم الطابق",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 12,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "basement", DisplayName = "قبو", SortOrder = 0, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "ground", DisplayName = "أرضي", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "1", DisplayName = "الأول", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "2", DisplayName = "الثاني", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "3", DisplayName = "الثالث", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "4", DisplayName = "الرابع", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "5+", DisplayName = "الخامس أو أعلى", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "roof", DisplayName = "ملحق سطح", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "full_building", DisplayName = "المبنى كامل", SortOrder = 8, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// نوع الإعلان (عرض أو طلب)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.BillType,
				Name = "نوع الإعلان",
				Code = "bill_type",
				Type = AttributeType.SingleSelect,
				Description = "هل هذا عرض أم طلب؟",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 13,
				DefaultValue = "offer",
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "offer", DisplayName = "عرض (للإيجار)", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "request", DisplayName = "طلب (أبحث عن)", SortOrder = 2, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// نوع الإيجار (مشترك أو كامل)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.RentalType,
				Name = "نوع الإيجار",
				Code = "rental_type",
				Type = AttributeType.SingleSelect,
				Description = "هل الإيجار مشترك أم للوحدة كاملة؟",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 14,
				DefaultValue = "full",
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "full", DisplayName = "إيجار كامل", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "shared", DisplayName = "إيجار مشترك", SortOrder = 2, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// المساحة
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Area,
				Name = "المساحة (م²)",
				Code = "area",
				Type = AttributeType.Number,
				Description = "مساحة العقار بالمتر المربع",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 15,
				ValidationRules = "{\"min\": 10, \"max\": 10000}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// عدد الغرف
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Rooms,
				Name = "عدد الغرف",
				Code = "rooms",
				Type = AttributeType.SingleSelect,
				Description = "عدد غرف النوم (للشقق والوحدات)",
				IsRequired = false, // لا يظهر للفيلا
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 16,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "1", DisplayName = "غرفة واحدة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "2", DisplayName = "غرفتين", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "3", DisplayName = "3 غرف", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "4", DisplayName = "4 غرف", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "5", DisplayName = "5 غرف", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "6+", DisplayName = "6 غرف أو أكثر", SortOrder = 6, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// عدد الحمامات
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Bathrooms,
				Name = "عدد دورات المياه",
				Code = "bathrooms",
				Type = AttributeType.SingleSelect,
				Description = "عدد الحمامات (للشقق والوحدات)",
				IsRequired = false, // لا يظهر للفيلا
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 17,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "1", DisplayName = "1", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "2", DisplayName = "2", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "3", DisplayName = "3", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "4", DisplayName = "4", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "5+", DisplayName = "5 أو أكثر", SortOrder = 5, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// التأثيث
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Furnished,
				Name = "التأثيث",
				Code = "furnished",
				Type = AttributeType.SingleSelect,
				Description = "حالة الأثاث",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 18,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "furnished", DisplayName = "مفروش", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "semi_furnished", DisplayName = "نصف مفروش", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "unfurnished", DisplayName = "غير مفروش", SortOrder = 3, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// المرافق والمميزات
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Amenities,
				Name = "المرافق والمميزات",
				Code = "amenities",
				Type = AttributeType.MultiSelect,
				Description = "المرافق المتوفرة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 19,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "ac", DisplayName = "تكييف", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "wifi", DisplayName = "إنترنت", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "parking", DisplayName = "موقف سيارة", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "elevator", DisplayName = "مصعد", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "security", DisplayName = "حراسة أمنية", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "gym", DisplayName = "صالة رياضية", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "pool", DisplayName = "مسبح", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "kitchen", DisplayName = "مطبخ", SortOrder = 8, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "washer", DisplayName = "غسالة", SortOrder = 9, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "balcony", DisplayName = "بلكونة", SortOrder = 10, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "garden", DisplayName = "حديقة", SortOrder = 11, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "maid_room", DisplayName = "غرفة خادمة", SortOrder = 12, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "driver_room", DisplayName = "غرفة سائق", SortOrder = 13, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "storage", DisplayName = "مخزن", SortOrder = 14, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// تفضيلات التواصل
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.IsPhoneAllowed,
				Name = "السماح بالاتصال الهاتفي",
				Code = "is_phone_allowed",
				Type = AttributeType.Boolean,
				Description = "هل تسمح بالتواصل عبر الاتصال الهاتفي؟",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 50,
				DefaultValue = "true",
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = AttributeIds.IsWhatsAppAllowed,
				Name = "السماح بالواتساب",
				Code = "is_whatsapp_allowed",
				Type = AttributeType.Boolean,
				Description = "هل تسمح بالتواصل عبر واتساب؟",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 51,
				DefaultValue = "true",
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = AttributeIds.IsMessagingAllowed,
				Name = "السماح بالرسائل",
				Code = "is_messaging_allowed",
				Type = AttributeType.Boolean,
				Description = "هل تسمح بالتواصل عبر الرسائل في التطبيق؟",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 52,
				DefaultValue = "true",
				CreatedAt = DateTime.UtcNow
			}
		};
	}

	private List<AttributeDefinition> GetPartnerAttributes()
	{
		return new List<AttributeDefinition>
		{
			// ═══════════════════════════════════════════════════════════════════
			// الاسم الشخصي (بدلاً من العنوان)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.PersonalName,
				Name = "الاسم",
				Code = "personal_name",
				Type = AttributeType.Text,
				Description = "اسمك الشخصي",
				IsRequired = true,
				IsFilterable = false,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 20,
				ValidationRules = "{\"minLength\": 2, \"maxLength\": 100}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// العمر (رقم وليس اختيار)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Age,
				Name = "العمر",
				Code = "age",
				Type = AttributeType.Number,
				Description = "عمرك بالسنوات",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 21,
				ValidationRules = "{\"min\": 18, \"max\": 80}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// الجنس
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Gender,
				Name = "الجنس",
				Code = "gender",
				Type = AttributeType.SingleSelect,
				Description = "جنسك",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 22,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "male", DisplayName = "ذكر", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "female", DisplayName = "أنثى", SortOrder = 2, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// الجنسية
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Nationality,
				Name = "الجنسية",
				Code = "nationality",
				Type = AttributeType.SingleSelect,
				Description = "جنسيتك",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 23,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "saudi", DisplayName = "سعودي", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "emirati", DisplayName = "إماراتي", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "kuwaiti", DisplayName = "كويتي", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "qatari", DisplayName = "قطري", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "bahraini", DisplayName = "بحريني", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "omani", DisplayName = "عماني", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "egyptian", DisplayName = "مصري", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "jordanian", DisplayName = "أردني", SortOrder = 8, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "syrian", DisplayName = "سوري", SortOrder = 9, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "lebanese", DisplayName = "لبناني", SortOrder = 10, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "yemeni", DisplayName = "يمني", SortOrder = 11, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "sudanese", DisplayName = "سوداني", SortOrder = 12, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "moroccan", DisplayName = "مغربي", SortOrder = 13, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "tunisian", DisplayName = "تونسي", SortOrder = 14, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "algerian", DisplayName = "جزائري", SortOrder = 15, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "iraqi", DisplayName = "عراقي", SortOrder = 16, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "palestinian", DisplayName = "فلسطيني", SortOrder = 17, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "indian", DisplayName = "هندي", SortOrder = 18, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "pakistani", DisplayName = "باكستاني", SortOrder = 19, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "bangladeshi", DisplayName = "بنغلاديشي", SortOrder = 20, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "filipino", DisplayName = "فلبيني", SortOrder = 21, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "indonesian", DisplayName = "إندونيسي", SortOrder = 22, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "other", DisplayName = "أخرى", SortOrder = 99, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// المهنة/الوظيفة
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Job,
				Name = "المهنة/الوظيفة",
				Code = "job",
				Type = AttributeType.Text,
				Description = "مهنتك أو وظيفتك الحالية",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 24,
				ValidationRules = "{\"maxLength\": 100}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// أقل ميزانية
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.MinPrice,
				Name = "أقل ميزانية (ريال)",
				Code = "min_price",
				Type = AttributeType.Number,
				Description = "الحد الأدنى لميزانيتك الشهرية",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 25,
				ValidationRules = "{\"min\": 0, \"max\": 1000000}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// أعلى ميزانية
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.MaxPrice,
				Name = "أعلى ميزانية (ريال)",
				Code = "max_price",
				Type = AttributeType.Number,
				Description = "الحد الأقصى لميزانيتك الشهرية",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 26,
				ValidationRules = "{\"min\": 0, \"max\": 1000000}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// التدخين
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Smoking,
				Name = "التدخين",
				Code = "smoking",
				Type = AttributeType.SingleSelect,
				Description = "هل أنت مدخن؟",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 27,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "no", DisplayName = "غير مدخن", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "yes", DisplayName = "مدخن", SortOrder = 2, CreatedAt = DateTime.UtcNow }
				}
			}
		};
	}

	private List<AttributeDefinition> GetCommercialAttributes()
	{
		return new List<AttributeDefinition>
		{
			// ═══════════════════════════════════════════════════════════════════
			// نوع العقار التجاري (محل/مجمع/مول)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.CommercialPropertyType,
				Name = "نوع العقار التجاري",
				Code = "commercial_property_type",
				Type = AttributeType.SingleSelect,
				Description = "نوع العقار التجاري",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 30,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "shop", DisplayName = "محل تجاري", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "complex", DisplayName = "مجمع تجاري", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "mall", DisplayName = "مول", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "warehouse", DisplayName = "مستودع", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "showroom", DisplayName = "معرض", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "restaurant", DisplayName = "مطعم/مقهى", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "kiosk", DisplayName = "كشك", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "office", DisplayName = "مكتب إداري", SortOrder = 8, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "coworking", DisplayName = "مساحة عمل مشتركة", SortOrder = 9, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "meeting_room", DisplayName = "قاعة اجتماعات", SortOrder = 10, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "event_hall", DisplayName = "قاعة مناسبات", SortOrder = 11, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "clinic", DisplayName = "عيادة", SortOrder = 12, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "gym", DisplayName = "صالة رياضية", SortOrder = 13, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "salon", DisplayName = "صالون", SortOrder = 14, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "workshop", DisplayName = "ورشة", SortOrder = 15, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// السعة (عدد الأشخاص)
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Capacity,
				Name = "السعة",
				Code = "capacity",
				Type = AttributeType.Number,
				Description = "عدد الأشخاص الذي تستوعبهم المساحة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 31,
				ValidationRules = "{\"min\": 1, \"max\": 10000}",
				CreatedAt = DateTime.UtcNow
			},
			// ═══════════════════════════════════════════════════════════════════
			// المواقف
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Parking,
				Name = "المواقف",
				Code = "parking",
				Type = AttributeType.SingleSelect,
				Description = "توفر مواقف السيارات",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 32,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "available", DisplayName = "متوفر", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "limited", DisplayName = "محدود", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "unavailable", DisplayName = "غير متوفر", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "paid", DisplayName = "مدفوع", SortOrder = 4, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// ساعات العمل
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.WorkingHours,
				Name = "ساعات العمل",
				Code = "working_hours",
				Type = AttributeType.SingleSelect,
				Description = "أوقات الدخول المتاحة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 33,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "24h", DisplayName = "24 ساعة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "business", DisplayName = "أوقات العمل (8ص-6م)", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "extended", DisplayName = "ممتدة (8ص-10م)", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "flexible", DisplayName = "مرن", SortOrder = 4, CreatedAt = DateTime.UtcNow }
				}
			},
			// ═══════════════════════════════════════════════════════════════════
			// التجهيزات
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = AttributeIds.Facilities,
				Name = "التجهيزات",
				Code = "facilities",
				Type = AttributeType.MultiSelect,
				Description = "التجهيزات المتوفرة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 34,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "wifi", DisplayName = "إنترنت عالي السرعة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "projector", DisplayName = "جهاز عرض", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "whiteboard", DisplayName = "سبورة", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "video_conf", DisplayName = "نظام مؤتمرات فيديو", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "printer", DisplayName = "طابعة/ماسح ضوئي", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "kitchen", DisplayName = "مطبخ/بوفيه", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "reception", DisplayName = "استقبال", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "storage", DisplayName = "تخزين", SortOrder = 8, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "ac", DisplayName = "تكييف مركزي", SortOrder = 9, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "security", DisplayName = "نظام أمني", SortOrder = 10, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "elevator", DisplayName = "مصعد", SortOrder = 11, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "fire_safety", DisplayName = "نظام إطفاء حريق", SortOrder = 12, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "loading_dock", DisplayName = "رصيف تحميل", SortOrder = 13, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "generator", DisplayName = "مولد كهربائي", SortOrder = 14, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "cctv", DisplayName = "كاميرات مراقبة", SortOrder = 15, CreatedAt = DateTime.UtcNow }
				}
			}
		};
	}

	private async Task SeedProductsAsync()
	{
		var repo = _repositoryFactory.CreateRepository<Product>();

		var existing = await repo.GetAllWithPredicateAsync();
		if (existing.Any()) return;

		// إضافة المنتجات بدون الأسعار أولاً
		var products = new List<Product>
		{
			// ═══════════════════════════════════════════════════════════════════
			// مساحات سكنية
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = ProductIds.Apartment1,
				Name = "شقة مفروشة في حي النرجس",
				Sku = "RES-APT-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "شقة مفروشة بالكامل، 3 غرف، موقع مميز",
				LongDescription = "شقة مفروشة بالكامل في حي النرجس بالرياض. تتكون من 3 غرف نوم، صالة واسعة، مطبخ مجهز، وحمامين. الشقة في الدور الثاني مع مصعد. قريبة من المدارس والمستشفيات والمراكز التجارية.",
				FeaturedImage = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
				IsFeatured = true,
				IsNew = true,
				NewUntil = DateTime.UtcNow.AddDays(30),
				SortOrder = 1,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = ProductIds.Apartment2,
				Name = "استوديو فاخر في حي الملقا",
				Sku = "RES-STD-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "استوديو حديث، مناسب للأفراد",
				LongDescription = "استوديو فاخر في برج سكني راقٍ بحي الملقا. يتميز بإطلالة رائعة على المدينة، مفروش بالكامل بأثاث عصري، مع مطبخ أمريكي وحمام حديث. يشمل الإيجار الكهرباء والماء والإنترنت.",
				FeaturedImage = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
				IsFeatured = true,
				IsNew = true,
				NewUntil = DateTime.UtcNow.AddDays(30),
				SortOrder = 2,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = ProductIds.Villa1,
				Name = "فيلا واسعة للإيجار في حي الياسمين",
				Sku = "RES-VIL-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "فيلا 5 غرف مع حديقة ومسبح خاص",
				LongDescription = "فيلا فاخرة في حي الياسمين، تتكون من 5 غرف نوم، 3 صالات، مطبخ واسع، 4 حمامات، غرفة خادمة، موقف لسيارتين، حديقة خارجية مع مسبح خاص. الفيلا مؤثثة جزئياً.",
				FeaturedImage = "https://images.unsplash.com/photo-1613977257363-707ba9348227?w=800",
				IsFeatured = true,
				IsNew = false,
				SortOrder = 3,
				CreatedAt = DateTime.UtcNow.AddDays(-10)
			},

			// ═══════════════════════════════════════════════════════════════════
			// مساحات تجارية
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = ProductIds.Office1,
				Name = "مكتب مجهز في برج المملكة",
				Sku = "COM-OFF-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "مكتب فاخر 80 متر في برج المملكة",
				LongDescription = "مكتب مجهز بالكامل في برج المملكة الشهير. المساحة 80 متر مربع، يتسع لـ 6-8 أشخاص. يشمل أثاث مكتبي فاخر، إنترنت عالي السرعة، غرفة اجتماعات صغيرة، ومنطقة استراحة. موقف سيارة مجاني.",
				FeaturedImage = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800",
				IsFeatured = true,
				IsNew = true,
				NewUntil = DateTime.UtcNow.AddDays(14),
				SortOrder = 4,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = ProductIds.Coworking1,
				Name = "مكتب مشترك - مساحة عمل مرنة",
				Sku = "COM-COW-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "مقعد في مساحة عمل مشتركة حديثة",
				LongDescription = "انضم إلى مجتمع العمل المشترك في قلب الرياض! احصل على مكتب خاص في بيئة عمل ديناميكية. يشمل الاشتراك: مكتب وكرسي، إنترنت فائق السرعة، قهوة ومشروبات مجانية، استخدام غرف الاجتماعات، طابعة وماسح ضوئي.",
				FeaturedImage = "https://images.unsplash.com/photo-1527192491265-7e15c55b1ed2?w=800",
				IsFeatured = true,
				IsNew = true,
				NewUntil = DateTime.UtcNow.AddDays(7),
				SortOrder = 5,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = ProductIds.MeetingRoom1,
				Name = "قاعة اجتماعات VIP",
				Sku = "COM-MTG-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "قاعة اجتماعات فاخرة تتسع لـ 12 شخص",
				LongDescription = "قاعة اجتماعات VIP مجهزة بأحدث التقنيات. تتسع لـ 12 شخصاً حول طاولة اجتماعات فاخرة. تشمل: شاشة عرض 75 بوصة، نظام مؤتمرات فيديو، سبورة ذكية، نظام صوت احترافي، خدمة ضيافة. مثالية لاجتماعات العمل الهامة.",
				FeaturedImage = "https://images.unsplash.com/photo-1517502884422-41eaead166d4?w=800",
				IsFeatured = false,
				IsNew = false,
				SortOrder = 6,
				CreatedAt = DateTime.UtcNow.AddDays(-5)
			},

			// ═══════════════════════════════════════════════════════════════════
			// مساحات إدارية
			// ═══════════════════════════════════════════════════════════════════
			new()
			{
				Id = ProductIds.AdminOffice1,
				Name = "مكتب إداري في مجمع الأعمال",
				Sku = "ADM-OFF-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "مكتب إداري 120 متر مع استقبال",
				LongDescription = "مكتب إداري فسيح في مجمع الأعمال بحي العليا. المساحة الكلية 120 متر مربع، تشمل: منطقة استقبال، 3 مكاتب خاصة، قاعة اجتماعات صغيرة، مخزن، ومطبخ صغير. الموقع استراتيجي مع سهولة الوصول.",
				FeaturedImage = "https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=800",
				IsFeatured = false,
				IsNew = true,
				NewUntil = DateTime.UtcNow.AddDays(21),
				SortOrder = 7,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = ProductIds.AdminOffice2,
				Name = "طابق إداري كامل",
				Sku = "ADM-FLR-001",
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				ShortDescription = "طابق كامل 500 متر في برج تجاري",
				LongDescription = "فرصة استثنائية! طابق إداري كامل في برج تجاري راقٍ. المساحة 500 متر مربع قابلة للتقسيم حسب الحاجة. يشمل: 8 مواقف سيارات، مصعد خاص، نظام أمان متكامل، تكييف مركزي. مناسب للشركات الكبرى.",
				FeaturedImage = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=800",
				IsFeatured = true,
				IsNew = false,
				SortOrder = 8,
				CreatedAt = DateTime.UtcNow.AddDays(-15)
			}
		};

		foreach (var product in products)
		{
			await repo.AddAsync(product);
		}
	}

	private async Task SeedProductPricesAsync()
	{
		var priceRepo = _repositoryFactory.CreateRepository<ProductPrice>();

		var existing = await priceRepo.GetAllWithPredicateAsync();
		if (existing.Any()) return;

		// جلب المنتجات المحفوظة فعلياً للحصول على معرّفاتها الحقيقية
		var productRepo = _repositoryFactory.CreateRepository<Product>();
		var products = await productRepo.GetAllWithPredicateAsync();
		var productBySku = products.ToDictionary(p => p.Sku, p => p.Id);

		// جلب العملات المحفوظة فعلياً
		var currencyRepo = _repositoryFactory.CreateRepository<Currency>();
		var currencies = await currencyRepo.GetAllWithPredicateAsync();
		var sarCurrency = currencies.FirstOrDefault(c => c.Code == "SAR");

		if (sarCurrency == null || !productBySku.Any())
		{
			return; // لا يمكن إضافة الأسعار بدون عملات أو منتجات
		}

		// تعريف الأسعار مع SKU للمنتج
		var priceDefinitions = new Dictionary<string, decimal>
		{
			{ "RES-APT-001", 3500 },  // شقة مفروشة
			{ "RES-STD-001", 2500 },  // استوديو
			{ "RES-VIL-001", 15000 }, // فيلا
			{ "COM-OFF-001", 8000 },  // مكتب
			{ "COM-COW-001", 1500 },  // مساحة عمل مشتركة
			{ "COM-MTG-001", 500 },   // قاعة اجتماعات
			{ "ADM-OFF-001", 12000 }, // مكتب إداري
			{ "ADM-FLR-001", 50000 }  // طابق إداري
		};

		// إضافة الأسعار باستخدام معرّفات المنتجات الفعلية
		foreach (var priceDef in priceDefinitions)
		{
			if (productBySku.TryGetValue(priceDef.Key, out var productId))
			{
				await priceRepo.AddAsync(new ProductPrice
				{
					Id = Guid.NewGuid(),
					ProductId = productId,
					CurrencyId = sarCurrency.Id,
					BasePrice = priceDef.Value,
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				});
			}
		}
	}
}
