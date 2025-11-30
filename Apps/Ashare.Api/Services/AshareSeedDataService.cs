using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Attributes.Entities;
using ACommerce.Catalog.Attributes.Enums;

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

	public static class AttributeIds
	{
		// Common attributes
		public static readonly Guid Area = Guid.Parse("20000000-0000-0000-0001-000000000001");
		public static readonly Guid Price = Guid.Parse("20000000-0000-0000-0001-000000000002");
		public static readonly Guid Location = Guid.Parse("20000000-0000-0000-0001-000000000003");
		public static readonly Guid Description = Guid.Parse("20000000-0000-0000-0001-000000000004");
		public static readonly Guid Images = Guid.Parse("20000000-0000-0000-0001-000000000005");

		// Residential attributes
		public static readonly Guid Rooms = Guid.Parse("20000000-0000-0000-0002-000000000001");
		public static readonly Guid Bathrooms = Guid.Parse("20000000-0000-0000-0002-000000000002");
		public static readonly Guid Floor = Guid.Parse("20000000-0000-0000-0002-000000000003");
		public static readonly Guid Furnished = Guid.Parse("20000000-0000-0000-0002-000000000004");
		public static readonly Guid Amenities = Guid.Parse("20000000-0000-0000-0002-000000000005");
		public static readonly Guid PropertyType = Guid.Parse("20000000-0000-0000-0002-000000000006");

		// Partner request attributes
		public static readonly Guid Gender = Guid.Parse("20000000-0000-0000-0003-000000000001");
		public static readonly Guid AgeRange = Guid.Parse("20000000-0000-0000-0003-000000000002");
		public static readonly Guid Occupation = Guid.Parse("20000000-0000-0000-0003-000000000003");
		public static readonly Guid Nationality = Guid.Parse("20000000-0000-0000-0003-000000000004");
		public static readonly Guid Smoking = Guid.Parse("20000000-0000-0000-0003-000000000005");

		// Commercial/Administrative attributes
		public static readonly Guid SpaceType = Guid.Parse("20000000-0000-0000-0004-000000000001");
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
		// سكني - يستخدم الخصائص العامة + خصائص السكن
		[CategoryIds.Residential] = new List<Guid>
		{
			AttributeIds.Area,
			AttributeIds.Price,
			AttributeIds.Location,
			AttributeIds.Description,
			AttributeIds.PropertyType,
			AttributeIds.Rooms,
			AttributeIds.Bathrooms,
			AttributeIds.Floor,
			AttributeIds.Furnished,
			AttributeIds.Amenities
		},
		// طلب سكن - الخصائص العامة + تفضيلات السكن
		[CategoryIds.LookingForHousing] = new List<Guid>
		{
			AttributeIds.Price, // الميزانية
			AttributeIds.Location,
			AttributeIds.Description,
			AttributeIds.PropertyType,
			AttributeIds.Rooms,
			AttributeIds.Furnished
		},
		// طلب شريك - الخصائص العامة + خصائص الشريك
		[CategoryIds.LookingForPartner] = new List<Guid>
		{
			AttributeIds.Price, // الميزانية
			AttributeIds.Location,
			AttributeIds.Description,
			AttributeIds.Gender,
			AttributeIds.AgeRange,
			AttributeIds.Occupation,
			AttributeIds.Nationality,
			AttributeIds.Smoking
		},
		// مساحة إدارية
		[CategoryIds.Administrative] = new List<Guid>
		{
			AttributeIds.Area,
			AttributeIds.Price,
			AttributeIds.Location,
			AttributeIds.Description,
			AttributeIds.SpaceType,
			AttributeIds.Capacity,
			AttributeIds.Parking,
			AttributeIds.WorkingHours,
			AttributeIds.Facilities
		},
		// مساحة تجارية
		[CategoryIds.Commercial] = new List<Guid>
		{
			AttributeIds.Area,
			AttributeIds.Price,
			AttributeIds.Location,
			AttributeIds.Description,
			AttributeIds.SpaceType,
			AttributeIds.Capacity,
			AttributeIds.Parking,
			AttributeIds.Facilities
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
		await SeedCategoriesAsync();
		await SeedAttributeDefinitionsAsync();
	}

	private async Task SeedCategoriesAsync()
	{
		var repo = _repositoryFactory.CreateRepository<ProductCategory>();

		var existing = await repo.GetAllWithPredicateAsync();
		if (existing.Any()) return;

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
			await repo.AddAsync(category);
		}
	}

	private async Task SeedAttributeDefinitionsAsync()
	{
		var repo = _repositoryFactory.CreateRepository<AttributeDefinition>();

		var existing = await repo.GetAllWithPredicateAsync();
		if (existing.Any()) return;

		var attributes = GetAllAttributeDefinitions();

		foreach (var attr in attributes)
		{
			await repo.AddAsync(attr);
		}
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
			new()
			{
				Id = AttributeIds.Area,
				Name = "المساحة (م²)",
				Code = "area",
				Type = AttributeType.Number,
				Description = "مساحة العقار بالمتر المربع",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 1,
				ValidationRules = "{\"min\": 10, \"max\": 10000}",
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = AttributeIds.Price,
				Name = "السعر الشهري (ريال)",
				Code = "price",
				Type = AttributeType.Number,
				Description = "الإيجار الشهري بالريال السعودي",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 2,
				ValidationRules = "{\"min\": 100, \"max\": 1000000}",
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = AttributeIds.Location,
				Name = "الموقع",
				Code = "location",
				Type = AttributeType.Text,
				Description = "عنوان أو وصف الموقع",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 3,
				CreatedAt = DateTime.UtcNow
			},
			new()
			{
				Id = AttributeIds.Description,
				Name = "الوصف",
				Code = "description",
				Type = AttributeType.LongText,
				Description = "وصف تفصيلي للعرض",
				IsRequired = false,
				IsFilterable = false,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 4,
				CreatedAt = DateTime.UtcNow
			}
		};
	}

	private List<AttributeDefinition> GetResidentialAttributes()
	{
		var propertyTypeId = Guid.NewGuid();

		return new List<AttributeDefinition>
		{
			new()
			{
				Id = AttributeIds.PropertyType,
				Name = "نوع العقار",
				Code = "property_type",
				Type = AttributeType.SingleSelect,
				Description = "نوع العقار السكني",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 10,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "apartment", DisplayName = "شقة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "villa", DisplayName = "فيلا", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "room", DisplayName = "غرفة", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "studio", DisplayName = "استوديو", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "duplex", DisplayName = "دوبلكس", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "floor", DisplayName = "دور كامل", SortOrder = 6, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.Rooms,
				Name = "عدد الغرف",
				Code = "rooms",
				Type = AttributeType.SingleSelect,
				Description = "عدد غرف النوم",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 11,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "1", DisplayName = "غرفة واحدة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "2", DisplayName = "غرفتين", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "3", DisplayName = "3 غرف", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "4", DisplayName = "4 غرف", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "5+", DisplayName = "5 غرف أو أكثر", SortOrder = 5, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.Bathrooms,
				Name = "عدد دورات المياه",
				Code = "bathrooms",
				Type = AttributeType.SingleSelect,
				Description = "عدد الحمامات",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 12,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "1", DisplayName = "1", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "2", DisplayName = "2", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "3", DisplayName = "3", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "4+", DisplayName = "4 أو أكثر", SortOrder = 4, CreatedAt = DateTime.UtcNow }
				}
			},
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
				SortOrder = 13,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "ground", DisplayName = "أرضي", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "1", DisplayName = "الأول", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "2", DisplayName = "الثاني", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "3", DisplayName = "الثالث", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "4+", DisplayName = "الرابع أو أعلى", SortOrder = 5, CreatedAt = DateTime.UtcNow }
				}
			},
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
				SortOrder = 14,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "furnished", DisplayName = "مفروش", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "semi_furnished", DisplayName = "نصف مفروش", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "unfurnished", DisplayName = "غير مفروش", SortOrder = 3, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.Amenities,
				Name = "المرافق",
				Code = "amenities",
				Type = AttributeType.MultiSelect,
				Description = "المرافق المتوفرة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 15,
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
					new() { Id = Guid.NewGuid(), Value = "balcony", DisplayName = "بلكونة", SortOrder = 10, CreatedAt = DateTime.UtcNow }
				}
			}
		};
	}

	private List<AttributeDefinition> GetPartnerAttributes()
	{
		return new List<AttributeDefinition>
		{
			new()
			{
				Id = AttributeIds.Gender,
				Name = "الجنس المطلوب",
				Code = "gender",
				Type = AttributeType.SingleSelect,
				Description = "جنس الشريك المطلوب",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 20,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "male", DisplayName = "ذكر", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "female", DisplayName = "أنثى", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "any", DisplayName = "لا يهم", SortOrder = 3, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.AgeRange,
				Name = "الفئة العمرية",
				Code = "age_range",
				Type = AttributeType.SingleSelect,
				Description = "الفئة العمرية المفضلة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 21,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "18-25", DisplayName = "18-25 سنة", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "26-35", DisplayName = "26-35 سنة", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "36-45", DisplayName = "36-45 سنة", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "46+", DisplayName = "46 سنة فأكثر", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "any", DisplayName = "لا يهم", SortOrder = 5, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.Occupation,
				Name = "المهنة",
				Code = "occupation",
				Type = AttributeType.SingleSelect,
				Description = "نوع المهنة المفضل",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 22,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "student", DisplayName = "طالب", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "employee", DisplayName = "موظف", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "professional", DisplayName = "مهني", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "any", DisplayName = "لا يهم", SortOrder = 4, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.Nationality,
				Name = "الجنسية",
				Code = "nationality",
				Type = AttributeType.SingleSelect,
				Description = "الجنسية المفضلة",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 23,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "saudi", DisplayName = "سعودي", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "gcc", DisplayName = "خليجي", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "arab", DisplayName = "عربي", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "any", DisplayName = "لا يهم", SortOrder = 4, CreatedAt = DateTime.UtcNow }
				}
			},
			new()
			{
				Id = AttributeIds.Smoking,
				Name = "التدخين",
				Code = "smoking",
				Type = AttributeType.SingleSelect,
				Description = "حالة التدخين",
				IsRequired = false,
				IsFilterable = true,
				IsVisibleInList = false,
				IsVisibleInDetail = true,
				SortOrder = 24,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "no", DisplayName = "غير مدخن", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "yes", DisplayName = "مدخن", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "any", DisplayName = "لا يهم", SortOrder = 3, CreatedAt = DateTime.UtcNow }
				}
			}
		};
	}

	private List<AttributeDefinition> GetCommercialAttributes()
	{
		return new List<AttributeDefinition>
		{
			new()
			{
				Id = AttributeIds.SpaceType,
				Name = "نوع المساحة",
				Code = "space_type",
				Type = AttributeType.SingleSelect,
				Description = "نوع المساحة التجارية/الإدارية",
				IsRequired = true,
				IsFilterable = true,
				IsVisibleInList = true,
				IsVisibleInDetail = true,
				SortOrder = 30,
				CreatedAt = DateTime.UtcNow,
				Values = new List<AttributeValue>
				{
					new() { Id = Guid.NewGuid(), Value = "office", DisplayName = "مكتب", SortOrder = 1, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "coworking", DisplayName = "مساحة عمل مشتركة", SortOrder = 2, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "meeting_room", DisplayName = "قاعة اجتماعات", SortOrder = 3, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "shop", DisplayName = "محل تجاري", SortOrder = 4, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "warehouse", DisplayName = "مستودع", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "showroom", DisplayName = "معرض", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "event_hall", DisplayName = "قاعة مناسبات", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "studio", DisplayName = "استوديو", SortOrder = 8, CreatedAt = DateTime.UtcNow }
				}
			},
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
				ValidationRules = "{\"min\": 1, \"max\": 1000}",
				CreatedAt = DateTime.UtcNow
			},
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
					new() { Id = Guid.NewGuid(), Value = "unavailable", DisplayName = "غير متوفر", SortOrder = 3, CreatedAt = DateTime.UtcNow }
				}
			},
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
					new() { Id = Guid.NewGuid(), Value = "flexible", DisplayName = "مرن", SortOrder = 3, CreatedAt = DateTime.UtcNow }
				}
			},
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
					new() { Id = Guid.NewGuid(), Value = "printer", DisplayName = "طابعة", SortOrder = 5, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "kitchen", DisplayName = "مطبخ/بوفيه", SortOrder = 6, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "reception", DisplayName = "استقبال", SortOrder = 7, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "storage", DisplayName = "تخزين", SortOrder = 8, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "ac", DisplayName = "تكييف مركزي", SortOrder = 9, CreatedAt = DateTime.UtcNow },
					new() { Id = Guid.NewGuid(), Value = "security", DisplayName = "نظام أمني", SortOrder = 10, CreatedAt = DateTime.UtcNow }
				}
			}
		};
	}
}
