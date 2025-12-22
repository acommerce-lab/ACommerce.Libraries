using System.Text.Json;
using System.Text.Json.Serialization;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Catalog.Listings.Enums;
using ACommerce.Files.Abstractions.Providers;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace Ashare.Api.Services;

/// <summary>
/// خدمة ترحيل العروض من النظام القديم
/// </summary>
public class OffersMigrationService
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IStorageProvider _storageProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OffersMigrationService> _logger;
    private readonly IConfiguration _configuration;

    // الرابط الصحيح للصور في النظام القديم
    private const string OldImagesBaseUrl = "http://ashare-001-site4.mtempurl.com/Images/";

    public OffersMigrationService(
        IRepositoryFactory repositoryFactory,
        IStorageProvider storageProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OffersMigrationService> logger)
    {
        _repositoryFactory = repositoryFactory;
        _storageProvider = storageProvider;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60); // زيادة الوقت لتحميل الصور
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// حذف جميع العروض المرحلة وإعادة بذرها
    /// </summary>
    public async Task<MigrationResult> DeleteAndReseedOffersAsync(CancellationToken cancellationToken = default)
    {
        var result = new MigrationResult();

        try
        {
            _logger.LogInformation("Deleting existing migrated offers...");

            // حذف العروض الموجودة من البذر السابق
            var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();
            var staticOfferIds = GetStaticOffers().Select(o => o.Id).ToList();

            var allOffers = await listingRepo.ListAllAsync(cancellationToken);
            var existingOffers = allOffers.Where(x => staticOfferIds.Contains(x.Id)).ToList();

            if (existingOffers.Any())
            {
                foreach (var offer in existingOffers)
                {
                    await listingRepo.DeleteAsync(offer, cancellationToken);
                }
                await listingRepo.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted {Count} existing offers", existingOffers.Count);
            }

            // إعادة البذر مع تحميل الصور
            return await SeedOffersFromStaticDataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete and reseed failed");
            result.Errors.Add($"Delete and reseed failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// بذر العروض من البيانات الثابتة مع تحميل الصور
    /// </summary>
    public async Task<MigrationResult> SeedOffersFromStaticDataAsync(CancellationToken cancellationToken = default)
    {
        var result = new MigrationResult();

        try
        {
            _logger.LogInformation("Starting offers seeding from static data with image upload...");

            var staticOffers = GetStaticOffers();
            result.TotalFound = staticOffers.Count;

            await SaveOffersWithImagesAsync(staticOffers, result, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seeding failed");
            result.Errors.Add($"Seeding failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// حفظ العروض مع تحميل الصور
    /// </summary>
    private async Task SaveOffersWithImagesAsync(List<OldOfferDto> offers, MigrationResult result, CancellationToken cancellationToken)
    {
        var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();
        var baseUrl = _configuration["HostSettings:BaseUrl"] ?? "https://ashareapi-hygabpf3ajfmevfs.canadaeast-01.azurewebsites.net";

        foreach (var oldOffer in offers)
        {
            try
            {
                // التحقق من عدم وجود العرض مسبقاً
                var exists = (await listingRepo.ListAllAsync(cancellationToken))
                    .Any(x => x.Id == oldOffer.Id);

                if (exists)
                {
                    _logger.LogDebug("Offer {Id} already exists, skipping", oldOffer.Id);
                    result.Skipped++;
                    continue;
                }

                // تحميل ورفع الصور
                var newImageUrls = new List<string>();
                if (oldOffer.ImageUrls != null)
                {
                    foreach (var imageName in oldOffer.ImageUrls)
                    {
                        try
                        {
                            var newUrl = await DownloadAndUploadImageAsync(imageName, baseUrl, cancellationToken);
                            if (!string.IsNullOrEmpty(newUrl))
                            {
                                newImageUrls.Add(newUrl);
                                _logger.LogDebug("Uploaded image: {ImageName} -> {NewUrl}", imageName, newUrl);
                            }
                        }
                        catch (Exception imgEx)
                        {
                            _logger.LogWarning(imgEx, "Failed to upload image {ImageName} for offer {OfferId}", imageName, oldOffer.Id);
                            // نستمر حتى لو فشل تحميل صورة واحدة
                        }
                    }
                }

                // تحويل العرض
                var newListing = MapToProductListing(oldOffer, newImageUrls);

                // حفظ العرض
                await listingRepo.AddAsync(newListing, cancellationToken);
                result.Migrated++;

                _logger.LogDebug("Migrated offer: {Title} with {ImageCount} images", oldOffer.Title, newImageUrls.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate offer {Id}: {Title}", oldOffer.Id, oldOffer.Title);
                result.Failed++;
                result.Errors.Add($"{oldOffer.Id}: {ex.Message}");
            }
        }

        //await listingRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Migration completed. Total: {Total}, Migrated: {Migrated}, Skipped: {Skipped}, Failed: {Failed}",
            result.TotalFound, result.Migrated, result.Skipped, result.Failed);
    }

    /// <summary>
    /// تحميل صورة من النظام القديم ورفعها إلى التخزين السحابي
    /// </summary>
    private async Task<string?> DownloadAndUploadImageAsync(string imageName, string baseUrl, CancellationToken cancellationToken)
    {
        // بناء رابط الصورة في النظام القديم
        var oldImageUrl = $"{OldImagesBaseUrl}{imageName}";

        _logger.LogDebug("Downloading image from: {Url}", oldImageUrl);

        // تحميل الصورة
        var response = await _httpClient.GetAsync(oldImageUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to download image {Url}: {StatusCode}", oldImageUrl, response.StatusCode);
            return null;
        }

        // قراءة المحتوى كـ Stream
        await using var imageStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // تحديد امتداد الملف
        var extension = Path.GetExtension(imageName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".jpg";
        }

        // إنشاء اسم فريد للملف
        var newFileName = $"{Guid.NewGuid()}{extension}";

        // رفع الصورة إلى التخزين السحابي
        var objectName = await _storageProvider.SaveAsync(
            imageStream,
            newFileName,
            "listings/migrated",
            cancellationToken);

        // بناء الرابط الجديد
        var proxyUrl = $"{baseUrl.TrimEnd('/')}/api/media/{objectName}";

        return proxyUrl;
    }

    /// <summary>
    /// تحويل العرض القديم إلى ProductListing
    /// </summary>
    private ProductListing MapToProductListing(OldOfferDto old, List<string> imageUrls)
    {
        // بناء AttributesJson
        var attributes = new Dictionary<string, object>
        {
            // نوع العقار
            ["property_type"] = old.PropertyTypeName switch
            {
                "عماره" => "building",
                "فيلا" => "villa",
                _ => "building"
            },

            // نوع الوحدة
            ["unit_type"] = old.UnitTypeName switch
            {
                "غرفة" => "room",
                "شقة" => "apartment",
                "استوديو" => "studio",
                "ملحق" => "annex",
                _ => "room"
            },

            // الطابق
            ["floor"] = old.FloorName switch
            {
                "الدور الأرضي" => "ground",
                "الدور الأول" => "first",
                "الدور الثاني" => "second",
                "الدور الثالث" => "third",
                "الدور الرابع" => "fourth",
                "الدور الخامس" => "fifth",
                "السطح" => "roof",
                _ => "fourth"
            },

            // نوع الإعلان (عرض/طلب)
            ["bill_type"] = old.BillType == 0 ? "offer" : "request",

            // نوع الإيجار
            ["rental_type"] = old.RentalTypeName switch
            {
                "مشترك" => "shared",
                "كامل" => "full",
                _ => "shared"
            },

            // وحدة الوقت
            ["time_unit"] = old.TimeUnitName switch
            {
                "يوم" => "day",
                "أسبوع" => "week",
                "شهر" => "month",
                "سنة" => "year",
                _ => "year"
            },

            // المدة
            ["duration"] = old.Duration,

            // المساحة
            ["area"] = old.Area,

            // عدد الغرف
            ["rooms"] = old.RoomCount,

            // عدد الحمامات
            ["bathrooms"] = old.BathroomCount,

            // تفضيلات التواصل
            ["is_phone_allowed"] = old.IsPhoneAllowed,
            ["is_whatsapp_allowed"] = old.IsWhatsappAllowed,
            ["is_messaging_allowed"] = old.IsMessagingAllowed,

            // المميزات
            ["features"] = old.Features ?? new List<string>()
        };

        return new ProductListing
        {
            Id = old.Id,
            Title = old.Title,
            Description = old.Description,
            Price = old.Price,
            IsActive = old.IsActive,
            Status = old.IsActive ? ListingStatus.Active : ListingStatus.Draft,

            // الموقع
            Latitude = old.Latitude,
            Longitude = old.Longitude,
            Address = old.LocationDescription,
            City = ExtractCityFromLocation(old.LocationDescription),

            // الفئة (سكني)
            CategoryId = AshareSeedDataService.CategoryIds.Residential,

            // الصور (من التخزين السحابي الجديد)
            ImagesJson = JsonSerializer.Serialize(imageUrls),
            FeaturedImage = imageUrls.FirstOrDefault(),

            // الخصائص
            AttributesJson = JsonSerializer.Serialize(attributes),

            // بيانات إضافية
            VendorId = Guid.Empty, // سيتم تحديثه لاحقاً
            ProductId = old.Id, // نستخدم نفس ID
            CurrencyId = AshareSeedDataService.CurrencyIds.SAR,
            Currency = "SAR",
            QuantityAvailable = 1,

            CreatedAt = DateTime.UtcNow,
            IsNew = true
        };
    }

    /// <summary>
    /// استخراج اسم المدينة من الموقع
    /// </summary>
    private static string? ExtractCityFromLocation(string? location)
    {
        if (string.IsNullOrEmpty(location)) return null;

        // "الرياض، حي العارض" -> "الرياض"
        var parts = location.Split('،', ',');
        return parts.FirstOrDefault()?.Trim();
    }

    #region Static Offers Data

    /// <summary>
    /// العروض المحفوظة من النظام القديم
    /// </summary>
    private static List<OldOfferDto> GetStaticOffers() => new()
    {
        new()
        {
            Id = Guid.Parse("19f9ee7a-92b2-4de4-d303-08de1e691911"),
            Title = "للمشاركة غرفه وصاله في حي العارض / بلاطة",
            Description = "غرفه وصاله العارض   الإيجار ٣٧٠٠٠ السنه دفعتين",
            Price = 37000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 120,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي العارض",
            Latitude = 24.8738765716553,
            Longitude = 46.6183471679688,
            ImageUrls = new() { "cfc014ba-8f13-4a16-acca-967c0193ec03_19f9ee7a-92b2-4de4-d303-08de1e691911_0cc3e783-3693-44d5-84c5-513886a441c9_A00-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("f80fabc9-90ad-4b97-d304-08de1e691911"),
            Title = "للمشاركة غرفتين وصاله ومطبخ وحمام في حي العارض / بلاطة",
            Description = "غرفتين وصاله ومطبخ وحمام العارض  ٤١٠٠٠ السنه دفعتين",
            Price = 41000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي العارض",
            Latitude = 24.8738765716553,
            Longitude = 46.6183471679688,
            ImageUrls = new() { "1f20d424-48c0-43e7-9c99-100545c583c1_f80fabc9-90ad-4b97-d304-08de1e691911_90c1f99e-6664-4ed6-98b3-117bcb1eabe9_A01-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("e56d8aa0-60d6-4423-d305-08de1e691911"),
            Title = "للمشاركة غرفة وصاله في حي العارض / بلاطة",
            Description = "غرفتين وصاله العارض  ٣ حمام مطبخ  مفصول   ٥٠٠٠٠ السنه دفعتين  ٤٨ دفعه واحده",
            Price = 25000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي العارض",
            Latitude = 24.8738765716553,
            Longitude = 46.6183471679688,
            ImageUrls = new() { "0105a027-0810-459d-af14-9bd2ae36ecd2_e56d8aa0-60d6-4423-d305-08de1e691911_edcd22ad-b8a6-4438-b9fa-e7daf5537248_A02-0.png" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("b0556a6d-ab60-47f1-d306-08de1e691911"),
            Title = "للمشاركة استديو غرفه وحمام في حي الملقا / بلاطة",
            Description = "استديو غرفه وحمام الملقا  ش الاماسي  ٢٨٠٠٠ دفعتين",
            Price = 28000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.801420211792,
            Longitude = 46.5973510742188,
            ImageUrls = new() { "a2cca061-450b-4e8b-821c-2e97e9b02c68_b0556a6d-ab60-47f1-d306-08de1e691911_e768164a-720e-487f-b976-b3886f014659_A03-0.jpg.png" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("c429e3b3-29bb-46b9-d307-08de1e691911"),
            Title = "للمشاركة غرفة وصاله في حي الملقا / بلاطة",
            Description = "غرفتين وصاله الملقا  وحمامين   ب٥٠٠٠٠ دفعتين  ش  وادي هجر",
            Price = 26000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.8163089752197,
            Longitude = 46.6153182983398,
            ImageUrls = new() { "345f65c4-999f-46d9-bd72-fab333695e04_c429e3b3-29bb-46b9-d307-08de1e691911_43b70356-537e-4ea4-848c-b98a089c6644_A04-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("943835b5-d889-46e2-d308-08de1e691911"),
            Title = "للمشاركة غرفة وصاله في حي الملقا / بلاطة",
            Description = "غرفه وصاله الملقا  ش وادي هجر  ٤٠٠٠٠",
            Price = 40000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.8163089752197,
            Longitude = 46.6153182983398,
            ImageUrls = new() { "667bec38-9345-4175-b5c3-cf457fee639b_943835b5-d889-46e2-d308-08de1e691911_8f6032b0-d7f0-4c49-8734-8dacb99b32c0_A05-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("efcb9d1c-ddc2-4f55-d309-08de1e691911"),
            Title = "للمشاركة غرفة وصاله في حي الملقا / بلاطة",
            Description = "غرفه وصاله الملقا  ش وادي هجر ٣٨٠٠٠",
            Price = 38000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.8163089752197,
            Longitude = 46.6153182983398,
            ImageUrls = new() { "f7e1a04b-d791-4d9c-82ff-923cb055011e_efcb9d1c-ddc2-4f55-d309-08de1e691911_176a13b6-a0e6-4848-8ba7-24e9bf6b4da6_A05-1.png" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("f929f094-8060-42eb-d30a-08de1e691911"),
            Title = "للمشاركة استديو في حي العارض / بلاطة",
            Description = "استديو العارض  ٢٦٠٠٠ السنه",
            Price = 26000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي العارض",
            Latitude = 24.8738765716553,
            Longitude = 46.6183471679688,
            ImageUrls = new() { "f581a455-36f0-45e8-9a47-3d1f5909ac99_f929f094-8060-42eb-d30a-08de1e691911_0deb6c99-51c9-4c2f-a3dd-559f2804262e_A06-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("ee649a36-b33b-49f1-d30b-08de1e691911"),
            Title = "للمشاركة استديو في حي غرناطه / بلاطة",
            Description = "استديو غرناطه  طريق الدمام  ٢٦٠٠٠ السنه",
            Price = 26000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي غرناطة",
            Latitude = 24.8043537139893,
            Longitude = 46.751033782959,
            ImageUrls = new() { "51c93cfc-800f-4540-af1f-a6af5a1bc99b_ee649a36-b33b-49f1-d30b-08de1e691911_872140b2-95af-4a6e-af0d-0d5d0d5a3658_A07-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("a789feb8-a0ca-4544-d30c-08de1e691911"),
            Title = "للمشاركة غرفه وصاله في حي الملقا / بلاطة",
            Description = "غرفه  وصاله   ش الاماسي  ٣٦٠٠٠ السنه",
            Price = 36000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.801420211792,
            Longitude = 46.5973510742188,
            ImageUrls = new() { "e5c18c56-da5d-4e0b-9925-32ca19e08920_a789feb8-a0ca-4544-d30c-08de1e691911_bcc2eecc-b759-4077-a284-05b7e1f81bff_A08-1.png" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("60c79933-4885-4570-d30d-08de1e691911"),
            Title = "للمشاركة استديو في حي الملقا / بلاطة",
            Description = "استديو الاماسي الملقا  ٣٠٠٠٠ السنه",
            Price = 30000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.80142,
            Longitude = 46.597351,
            ImageUrls = new() { "b7e96bbc-afde-4b7b-a533-f1149356ce65_60c79933-4885-4570-d30d-08de1e691911_6956b770-4bb4-4da7-a0e4-b83b75b98457_A09-0.png" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("55bf3145-a34f-4b8f-d30e-08de1e691911"),
            Title = "للمشاركة غرفه وصاله في حي الملقا / بلاطة",
            Description = "غرفه وصاله الملقا وادي هجر ب ٤٠٠٠٠",
            Price = 40000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الرابع",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "سنة",
            BillType = 0,
            Duration = 1,
            Area = 240,
            RoomCount = 1,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.816309,
            Longitude = 46.615318,
            ImageUrls = new() { "fb928d99-cd27-4e46-9241-83135dbe9002_55bf3145-a34f-4b8f-d30e-08de1e691911_12db9155-c5af-499c-ad2b-510b243be869_A10-0.png" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("e76cff93-801b-4f97-6b08-08de1f38333d"),
            Title = "للمشاركة غرفتين وصاله في حي الملقا /بلاطة",
            Description = "غرفتين وصاله حي الملقا شارع الدهنا مشترك شخصين  ٤٥ الف ",
            Price = 45000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الثالث",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "شهر",
            BillType = 0,
            Duration = 1,
            Area = 150,
            RoomCount = 3,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.792749,
            Longitude = 46.614655,
            ImageUrls = new() { "6aaa05c2-42b7-4a2a-9001-b59265555c0b_e76cff93-801b-4f97-6b08-08de1f38333d_3a9194ac-f87e-40a2-b8a1-8e6b00b8675f_A12-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("b246d638-2469-479e-6b09-08de1f38333d"),
            Title = "للمشاركة باليومي 3 غرف في حي الملقا / مفروشة",
            Description = "شقة ٣ غرف وصالة مؤثث ايجار يومي بـ550",
            Price = 550,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الثالث",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "يوم",
            BillType = 0,
            Duration = 1,
            Area = 150,
            RoomCount = 3,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الملقا",
            Latitude = 24.792631,
            Longitude = 46.614685,
            ImageUrls = new() { "0bee26d6-fcdb-4320-a60b-6a02d32feeeb_b246d638-2469-479e-6b09-08de1f38333d_9af93e8f-8197-4aa2-a10b-2646b121285c_A11-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("8bdb08de-e38b-43fa-6b0a-08de1f38333d"),
            Title = "للمشاركة باليومي غرفة في حي حطين / مفروشة",
            Description = "غرفة وصالة حي حطين ايجار يومي ٣٥٠",
            Price = 350,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الثالث",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "يوم",
            BillType = 0,
            Duration = 1,
            Area = 150,
            RoomCount = 3,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي حطين",
            Latitude = 24.770507,
            Longitude = 46.612811,
            ImageUrls = new() { "cfd2f424-5dd4-4975-8528-b6df3e137d6f_8bdb08de-e38b-43fa-6b0a-08de1f38333d_99ec82e2-d499-45e5-b2cf-08ab7507cc87_A13-0.jpg" },
            Features = new() { "موقف مجاني" }
        },
        new()
        {
            Id = Guid.Parse("1996d9b0-130c-4c21-6b0b-08de1f38333d"),
            Title = "للمشاركة غرفة في حي الصحافة / بلاطة",
            Description = "غرفة وصالة حي الصحافة مشترك 26 الف ريال",
            Price = 26000,
            IsActive = true,
            OfferTypeName = "سكني",
            UnitTypeName = "غرفة",
            FloorName = "الدور الثالث",
            PropertyTypeName = "عماره",
            RentalTypeName = "مشترك",
            TimeUnitName = "شهر",
            BillType = 0,
            Duration = 1,
            Area = 150,
            RoomCount = 3,
            BathroomCount = 1,
            IsPhoneAllowed = true,
            IsWhatsappAllowed = true,
            IsMessagingAllowed = true,
            LocationDescription = "الرياض، حي الصحافة",
            Latitude = 24.803333,
            Longitude = 46.63126,
            ImageUrls = new() { "c04b17d6-f3d0-41e5-9fe0-fe9369223bdd_1996d9b0-130c-4c21-6b0b-08de1f38333d_aa906ad0-efdf-4c86-839d-71b7501cadf0_A14-0.jpg" },
            Features = new() { "موقف مجاني" }
        }
    };

    #endregion

    #region DTOs للنظام القديم

    public class OldOfferDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("offerTypeName")]
        public string? OfferTypeName { get; set; }

        [JsonPropertyName("unitTypeName")]
        public string? UnitTypeName { get; set; }

        [JsonPropertyName("floorName")]
        public string? FloorName { get; set; }

        [JsonPropertyName("propertyTypeName")]
        public string? PropertyTypeName { get; set; }

        [JsonPropertyName("rentalTypeName")]
        public string? RentalTypeName { get; set; }

        [JsonPropertyName("timeUnitName")]
        public string? TimeUnitName { get; set; }

        [JsonPropertyName("billType")]
        public int BillType { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("area")]
        public int Area { get; set; }

        [JsonPropertyName("roomCount")]
        public int RoomCount { get; set; }

        [JsonPropertyName("bathroomCount")]
        public int BathroomCount { get; set; }

        [JsonPropertyName("isPhoneAllowed")]
        public bool IsPhoneAllowed { get; set; }

        [JsonPropertyName("isWhatsappAllowed")]
        public bool IsWhatsappAllowed { get; set; }

        [JsonPropertyName("isMessagingAllowed")]
        public bool IsMessagingAllowed { get; set; }

        [JsonPropertyName("locationDescription")]
        public string? LocationDescription { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("imageUrls")]
        public List<string>? ImageUrls { get; set; }

        [JsonPropertyName("features")]
        public List<string>? Features { get; set; }
    }

    #endregion
}

/// <summary>
/// نتيجة الترحيل
/// </summary>
public class MigrationResult
{
    public int TotalFound { get; set; }
    public int Migrated { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();

    public bool IsSuccess => Failed == 0 && Errors.Count == 0;
}
