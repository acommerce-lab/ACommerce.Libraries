using ACommerce.AccountingKernel.Extensions;
using ACommerce.RealEstate.Api.Entries;
using ACommerce.RealEstate.Api.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === 1. قاعدة بيانات SQLite عبر SharedKernel (نفس بنية عشير) ===
EntityDiscoveryRegistry.RegisterEntity<Property>();
EntityDiscoveryRegistry.RegisterEntity<PropertyInquiry>();

builder.Services.AddACommerceSQLite("Data Source=realestate.db");

// تسجيل Repository لكل كيان (نفس ما يفعله عشير)
builder.Services.AddScoped<IBaseAsyncRepository<Property>, BaseAsyncRepository<Property>>();
builder.Services.AddScoped<IBaseAsyncRepository<PropertyInquiry>, BaseAsyncRepository<PropertyInquiry>>();

// === 2. المحرك المحاسبي ===
builder.Services.AddAccountingKernel(options =>
{
    options.EnableAudit = false;
    options.EnableEntityPersistence = false; // نحفظ عبر IPropertyStore مباشرة
    options.EnableEvents = false;
    options.EnforceBalance = true;
});

// === 3. مخزن العقارات عبر SharedKernel ===
builder.Services.AddScoped<IPropertyStore, EfPropertyStore>();

builder.Services.AddControllers();

var app = builder.Build();

// === 4. إنشاء قاعدة البيانات وبذر البيانات ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContext>();
    db.Database.EnsureCreated();

    // تطبيق الفهارس يدوياً (لأن ApplicationDbContext لا يطبق IEntityTypeConfiguration تلقائياً هنا)
    try
    {
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Properties_City ON \"Properties_RealEstate\" (\"City\")");
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Properties_Purpose ON \"Properties_RealEstate\" (\"Purpose\")");
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Properties_Category ON \"Properties_RealEstate\" (\"Category\")");
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Properties_Price ON \"Properties_RealEstate\" (\"Price\")");
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Properties_Status ON \"Properties_RealEstate\" (\"Status\")");
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Properties_Composite ON \"Properties_RealEstate\" (\"City\",\"Purpose\",\"IsDeleted\",\"Status\")");
    }
    catch { /* الجداول بأسماء افتراضية */ }

    // فحص: هل البيانات موجودة مسبقاً؟
    var tableName = db.Model.FindEntityType(typeof(Property))?.GetTableName() ?? "Property";
    var count = db.Set<Property>().Count();

    if (count == 0)
    {
        Console.WriteLine("Seeding 50,000 properties...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var cities = new[] { "الرياض", "جدة", "الدمام", "مكة", "المدينة", "الخبر", "الطائف", "تبوك", "بريدة", "أبها" };
        var districts = new Dictionary<string, string[]>
        {
            ["الرياض"] = new[] { "النرجس", "الملقا", "العليا", "الورود", "الياسمين", "الربيع", "النخيل", "حطين", "الصحافة", "الملك فهد" },
            ["جدة"] = new[] { "الحمراء", "الشاطئ", "الروضة", "السلامة", "الفيصلية", "البوادي", "الأندلس", "المحمدية", "أبحر", "الزهراء" },
            ["الدمام"] = new[] { "الفيصلية", "الشاطئ", "المزروعية", "الجلوية", "النور", "الأمير محمد بن سعود", "الريان", "الخليج", "الفردوس", "النخيل" },
        };
        var defaultDistricts = new[] { "الوسط", "الشمال", "الجنوب", "الشرق", "الغرب" };
        var categories = new[] { "residential", "commercial", "land" };
        var types = new Dictionary<string, string[]>
        {
            ["residential"] = new[] { "apartment", "villa", "duplex", "studio", "room" },
            ["commercial"] = new[] { "office", "shop", "warehouse", "showroom" },
            ["land"] = new[] { "residential_land", "commercial_land", "agricultural_land" }
        };
        var purposes = new[] { "sale", "rent" };
        var rnd = new Random(42);

        var batch = new List<Property>(1000);
        for (int i = 0; i < 50000; i++)
        {
            var city = cities[rnd.Next(cities.Length)];
            var dists = districts.GetValueOrDefault(city, defaultDistricts);
            var cat = categories[rnd.Next(categories.Length)];
            var pTypes = types[cat];

            batch.Add(new Property
            {
                Id = Guid.NewGuid(),
                Title = $"عقار {i + 1} في {dists[rnd.Next(dists.Length)]}",
                Description = $"وصف تجريبي للعقار رقم {i + 1}",
                Category = cat,
                PropertyType = pTypes[rnd.Next(pTypes.Length)],
                Purpose = purposes[rnd.Next(purposes.Length)],
                City = city,
                District = dists[rnd.Next(dists.Length)],
                Price = cat switch
                {
                    "residential" => rnd.Next(1000, 100000),
                    "commercial" => rnd.Next(5000, 200000),
                    _ => rnd.Next(50000, 5000000)
                },
                Currency = "SAR",
                Area = rnd.Next(50, 1000),
                Rooms = cat == "land" ? null : rnd.Next(1, 8),
                Bathrooms = cat == "land" ? null : rnd.Next(1, 5),
                Floor = cat == "residential" ? rnd.Next(0, 20) : null,
                Furnished = cat == "land" ? null : rnd.Next(2) == 1,
                OwnerId = Guid.NewGuid(),
                OwnerName = $"مالك {rnd.Next(1000)}",
                OwnerPhone = $"05{rnd.Next(10000000, 99999999)}",
                Status = "active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(365))
            });

            if (batch.Count >= 1000)
            {
                db.Set<Property>().AddRange(batch);
                db.SaveChanges();
                batch.Clear();
                if ((i + 1) % 10000 == 0) Console.Write($"  {i + 1}...");
            }
        }
        if (batch.Count > 0)
        {
            db.Set<Property>().AddRange(batch);
            db.SaveChanges();
        }

        // إعادة إنشاء الفهارس بعد البذر
        try
        {
            db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Prop_City ON Property (City)");
            db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Prop_Purpose ON Property (Purpose)");
            db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Prop_Category ON Property (Category)");
            db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Prop_Price ON Property (Price)");
            db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Prop_Composite ON Property (City, Purpose, IsDeleted, Status)");
        }
        catch { }

        sw.Stop();
        Console.WriteLine($"\n  Seeded 50,000 properties in {sw.Elapsed.TotalSeconds:F1}s");
    }
    else
    {
        Console.WriteLine($"Database already has {count:N0} properties");
    }
}

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { name = "ACommerce RealEstate API (SQLite + AccountingKernel)", properties = "50,000" }));

Console.WriteLine("\n=== RealEstate API (SQLite + SharedKernel) ===");
Console.WriteLine("http://localhost:5199\n");
app.Run("http://localhost:5199");
