using ACommerce.AccountingKernel.Extensions;
using ACommerce.RealEstate.Api.Entries;

var builder = WebApplication.CreateBuilder(args);

// المحرك المحاسبي - المطور يحدد ما يريد عبر الخيارات
builder.Services.AddAccountingKernel(options =>
{
    options.EnableAudit = false;            // لا توثيق في هذا المثال
    options.EnableEntityPersistence = false; // الحفظ يتم عبر IPropertyStore مباشرة
    options.EnableEvents = false;           // لا أحداث في هذا المثال
    options.EnforceBalance = true;          // فحص التوازن مفعّل
});

// مخزن العقارات (في الذاكرة مع بيانات تجريبية)
builder.Services.AddSingleton<IPropertyStore, InMemoryPropertyStore>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapControllers();

// عرض الروابط المتاحة عند البدء
app.MapGet("/", () => Results.Ok(new
{
    name = "ACommerce RealEstate API (AccountingKernel Demo)",
    endpoints = new
    {
        search = "GET  /api/properties?city=الرياض&purpose=rent",
        getById = "GET  /api/properties/{id}",
        create = "POST /api/properties",
        update = "PUT  /api/properties/{id}",
        delete = "DELETE /api/properties/{id}",
        inquiry = "POST /api/properties/{id}/inquiries"
    }
}));

Console.WriteLine("\n=== ACommerce RealEstate API ===");
Console.WriteLine("Built with AccountingKernel pattern");
Console.WriteLine("http://localhost:5199");
Console.WriteLine("Try: curl http://localhost:5199/api/properties?city=الرياض\n");

app.Run("http://localhost:5199");
