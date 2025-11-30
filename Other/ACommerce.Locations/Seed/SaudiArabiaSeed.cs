using ACommerce.Locations.Abstractions.Entities;

namespace ACommerce.Locations.Seed;

/// <summary>
/// بيانات المملكة العربية السعودية
/// </summary>
public static class SaudiArabiaSeed
{
    public static readonly Guid SaudiArabiaId = Guid.Parse("00000000-0000-0000-0001-000000000001");

    // Regions
    public static readonly Guid RiyadhRegionId = Guid.Parse("00000000-0000-0000-0002-000000000001");
    public static readonly Guid MakkahRegionId = Guid.Parse("00000000-0000-0000-0002-000000000002");
    public static readonly Guid MadinahRegionId = Guid.Parse("00000000-0000-0000-0002-000000000003");
    public static readonly Guid EasternRegionId = Guid.Parse("00000000-0000-0000-0002-000000000004");
    public static readonly Guid AsirRegionId = Guid.Parse("00000000-0000-0000-0002-000000000005");
    public static readonly Guid QassimRegionId = Guid.Parse("00000000-0000-0000-0002-000000000006");
    public static readonly Guid TabukRegionId = Guid.Parse("00000000-0000-0000-0002-000000000007");
    public static readonly Guid HailRegionId = Guid.Parse("00000000-0000-0000-0002-000000000008");
    public static readonly Guid NorthernBordersRegionId = Guid.Parse("00000000-0000-0000-0002-000000000009");
    public static readonly Guid JazanRegionId = Guid.Parse("00000000-0000-0000-0002-000000000010");
    public static readonly Guid NajranRegionId = Guid.Parse("00000000-0000-0000-0002-000000000011");
    public static readonly Guid BahaRegionId = Guid.Parse("00000000-0000-0000-0002-000000000012");
    public static readonly Guid JoufRegionId = Guid.Parse("00000000-0000-0000-0002-000000000013");

    // Major Cities
    public static readonly Guid RiyadhCityId = Guid.Parse("00000000-0000-0000-0003-000000000001");
    public static readonly Guid JeddahCityId = Guid.Parse("00000000-0000-0000-0003-000000000002");
    public static readonly Guid MakkahCityId = Guid.Parse("00000000-0000-0000-0003-000000000003");
    public static readonly Guid MadinahCityId = Guid.Parse("00000000-0000-0000-0003-000000000004");
    public static readonly Guid DammamCityId = Guid.Parse("00000000-0000-0000-0003-000000000005");
    public static readonly Guid KhobarCityId = Guid.Parse("00000000-0000-0000-0003-000000000006");
    public static readonly Guid DhahranCityId = Guid.Parse("00000000-0000-0000-0003-000000000007");
    public static readonly Guid AbhaCityId = Guid.Parse("00000000-0000-0000-0003-000000000008");
    public static readonly Guid TabukCityId = Guid.Parse("00000000-0000-0000-0003-000000000009");
    public static readonly Guid BuraidahCityId = Guid.Parse("00000000-0000-0000-0003-000000000010");

    public static Country GetSaudiArabia()
    {
        return new Country
        {
            Id = SaudiArabiaId,
            Name = "المملكة العربية السعودية",
            NameEn = "Saudi Arabia",
            Code = "SA",
            Code3 = "SAU",
            NumericCode = 682,
            PhoneCode = "+966",
            CurrencyCode = "SAR",
            CurrencyName = "ريال سعودي",
            CurrencySymbol = "ر.س",
            Flag = "🇸🇦",
            Latitude = 24.7136,
            Longitude = 46.6753,
            Timezone = "Asia/Riyadh",
            IsActive = true,
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<Region> GetRegions()
    {
        return
        [
            new Region
            {
                Id = RiyadhRegionId,
                Name = "منطقة الرياض",
                NameEn = "Riyadh Region",
                Code = "01",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 24.7136,
                Longitude = 46.6753,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = MakkahRegionId,
                Name = "منطقة مكة المكرمة",
                NameEn = "Makkah Region",
                Code = "02",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 21.4225,
                Longitude = 39.8262,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = MadinahRegionId,
                Name = "منطقة المدينة المنورة",
                NameEn = "Madinah Region",
                Code = "03",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 24.5247,
                Longitude = 39.5692,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = EasternRegionId,
                Name = "المنطقة الشرقية",
                NameEn = "Eastern Region",
                Code = "04",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 26.4207,
                Longitude = 50.0888,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = AsirRegionId,
                Name = "منطقة عسير",
                NameEn = "Asir Region",
                Code = "05",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 18.2164,
                Longitude = 42.5053,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 5,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = QassimRegionId,
                Name = "منطقة القصيم",
                NameEn = "Qassim Region",
                Code = "06",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 26.3260,
                Longitude = 43.9750,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 6,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = TabukRegionId,
                Name = "منطقة تبوك",
                NameEn = "Tabuk Region",
                Code = "07",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 28.3838,
                Longitude = 36.5550,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 7,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = HailRegionId,
                Name = "منطقة حائل",
                NameEn = "Hail Region",
                Code = "08",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 27.5114,
                Longitude = 41.7208,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 8,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = NorthernBordersRegionId,
                Name = "منطقة الحدود الشمالية",
                NameEn = "Northern Borders Region",
                Code = "09",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 30.9843,
                Longitude = 41.1183,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 9,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = JazanRegionId,
                Name = "منطقة جازان",
                NameEn = "Jazan Region",
                Code = "10",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 16.8892,
                Longitude = 42.5611,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 10,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = NajranRegionId,
                Name = "منطقة نجران",
                NameEn = "Najran Region",
                Code = "11",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 17.4924,
                Longitude = 44.1277,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 11,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = BahaRegionId,
                Name = "منطقة الباحة",
                NameEn = "Al Bahah Region",
                Code = "12",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 20.0129,
                Longitude = 41.4677,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 12,
                CreatedAt = DateTime.UtcNow
            },
            new Region
            {
                Id = JoufRegionId,
                Name = "منطقة الجوف",
                NameEn = "Al Jawf Region",
                Code = "13",
                Type = RegionType.Region,
                CountryId = SaudiArabiaId,
                Latitude = 29.7854,
                Longitude = 39.8712,
                Timezone = "Asia/Riyadh",
                IsActive = true,
                SortOrder = 13,
                CreatedAt = DateTime.UtcNow
            }
        ];
    }

    public static List<City> GetMajorCities()
    {
        return
        [
            // منطقة الرياض
            new City
            {
                Id = RiyadhCityId,
                Name = "الرياض",
                NameEn = "Riyadh",
                Code = "RUH",
                RegionId = RiyadhRegionId,
                Latitude = 24.7136,
                Longitude = 46.6753,
                Population = 7500000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            // منطقة مكة
            new City
            {
                Id = JeddahCityId,
                Name = "جدة",
                NameEn = "Jeddah",
                Code = "JED",
                RegionId = MakkahRegionId,
                Latitude = 21.5433,
                Longitude = 39.1728,
                Population = 4700000,
                IsActive = true,
                IsCapital = false,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new City
            {
                Id = MakkahCityId,
                Name = "مكة المكرمة",
                NameEn = "Makkah",
                Code = "MKH",
                RegionId = MakkahRegionId,
                Latitude = 21.4225,
                Longitude = 39.8262,
                Population = 2000000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            // منطقة المدينة
            new City
            {
                Id = MadinahCityId,
                Name = "المدينة المنورة",
                NameEn = "Madinah",
                Code = "MED",
                RegionId = MadinahRegionId,
                Latitude = 24.5247,
                Longitude = 39.5692,
                Population = 1500000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            // المنطقة الشرقية
            new City
            {
                Id = DammamCityId,
                Name = "الدمام",
                NameEn = "Dammam",
                Code = "DMM",
                RegionId = EasternRegionId,
                Latitude = 26.4207,
                Longitude = 50.0888,
                Population = 1100000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new City
            {
                Id = KhobarCityId,
                Name = "الخبر",
                NameEn = "Khobar",
                Code = "KHB",
                RegionId = EasternRegionId,
                Latitude = 26.2794,
                Longitude = 50.2083,
                Population = 600000,
                IsActive = true,
                IsCapital = false,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new City
            {
                Id = DhahranCityId,
                Name = "الظهران",
                NameEn = "Dhahran",
                Code = "DHA",
                RegionId = EasternRegionId,
                Latitude = 26.2861,
                Longitude = 50.1115,
                Population = 150000,
                IsActive = true,
                IsCapital = false,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            // منطقة عسير
            new City
            {
                Id = AbhaCityId,
                Name = "أبها",
                NameEn = "Abha",
                Code = "AHB",
                RegionId = AsirRegionId,
                Latitude = 18.2164,
                Longitude = 42.5053,
                Population = 400000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            // منطقة تبوك
            new City
            {
                Id = TabukCityId,
                Name = "تبوك",
                NameEn = "Tabuk",
                Code = "TBK",
                RegionId = TabukRegionId,
                Latitude = 28.3838,
                Longitude = 36.5550,
                Population = 600000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            // منطقة القصيم
            new City
            {
                Id = BuraidahCityId,
                Name = "بريدة",
                NameEn = "Buraidah",
                Code = "BRD",
                RegionId = QassimRegionId,
                Latitude = 26.3260,
                Longitude = 43.9750,
                Population = 700000,
                IsActive = true,
                IsCapital = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            }
        ];
    }

    public static List<Neighborhood> GetRiyadhNeighborhoods()
    {
        return
        [
            new Neighborhood { Id = Guid.NewGuid(), Name = "العليا", NameEn = "Al Olaya", CityId = RiyadhCityId, Latitude = 24.6910, Longitude = 46.6853, IsActive = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "السليمانية", NameEn = "Al Sulaimaniyah", CityId = RiyadhCityId, Latitude = 24.7031, Longitude = 46.6797, IsActive = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الملز", NameEn = "Al Malaz", CityId = RiyadhCityId, Latitude = 24.6592, Longitude = 46.7287, IsActive = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "النزهة", NameEn = "Al Nuzha", CityId = RiyadhCityId, Latitude = 24.7380, Longitude = 46.7089, IsActive = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الورود", NameEn = "Al Wurud", CityId = RiyadhCityId, Latitude = 24.7049, Longitude = 46.6643, IsActive = true, SortOrder = 5, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الربوة", NameEn = "Al Rabwah", CityId = RiyadhCityId, Latitude = 24.7294, Longitude = 46.7306, IsActive = true, SortOrder = 6, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الروضة", NameEn = "Al Rawdah", CityId = RiyadhCityId, Latitude = 24.7500, Longitude = 46.7500, IsActive = true, SortOrder = 7, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الياسمين", NameEn = "Al Yasmeen", CityId = RiyadhCityId, Latitude = 24.8250, Longitude = 46.6500, IsActive = true, SortOrder = 8, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "النرجس", NameEn = "Al Narjis", CityId = RiyadhCityId, Latitude = 24.8500, Longitude = 46.6250, IsActive = true, SortOrder = 9, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "العقيق", NameEn = "Al Aqiq", CityId = RiyadhCityId, Latitude = 24.7700, Longitude = 46.6300, IsActive = true, SortOrder = 10, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الصحافة", NameEn = "Al Sahafah", CityId = RiyadhCityId, Latitude = 24.7800, Longitude = 46.6400, IsActive = true, SortOrder = 11, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الغدير", NameEn = "Al Ghadeer", CityId = RiyadhCityId, Latitude = 24.7600, Longitude = 46.7800, IsActive = true, SortOrder = 12, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "المونسية", NameEn = "Al Munsiyah", CityId = RiyadhCityId, Latitude = 24.7900, Longitude = 46.8100, IsActive = true, SortOrder = 13, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "حطين", NameEn = "Hittin", CityId = RiyadhCityId, Latitude = 24.7650, Longitude = 46.6100, IsActive = true, SortOrder = 14, CreatedAt = DateTime.UtcNow },
            new Neighborhood { Id = Guid.NewGuid(), Name = "الملقا", NameEn = "Al Malqa", CityId = RiyadhCityId, Latitude = 24.8100, Longitude = 46.6000, IsActive = true, SortOrder = 15, CreatedAt = DateTime.UtcNow }
        ];
    }
}
