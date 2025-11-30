using ACommerce.Templates.Customer.Components;
using ACommerce.Templates.Customer.Pages;

namespace ACommerce.App.Customer.Services;

/// <summary>
/// Mock data service for demonstration purposes
/// </summary>
public class MockDataService
{
    public int GetCartItemCount() => 3;
    public int GetNotificationCount() => 5;

    public List<HomeCategoryItem> GetCategories() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "إلكترونيات", Icon = "bi-laptop" },
        new() { Id = Guid.NewGuid(), Name = "ملابس", Icon = "bi-bag" },
        new() { Id = Guid.NewGuid(), Name = "أحذية", Icon = "bi-shoe" },
        new() { Id = Guid.NewGuid(), Name = "مجوهرات", Icon = "bi-gem" },
        new() { Id = Guid.NewGuid(), Name = "أثاث", Icon = "bi-house" },
        new() { Id = Guid.NewGuid(), Name = "رياضة", Icon = "bi-bicycle" },
    };

    public List<BannerItem> GetBanners() => new()
    {
        new() { Id = "1", Title = "تخفيضات الصيف", Subtitle = "خصم حتى 50%", ImageUrl = "https://via.placeholder.com/400x200/2563eb/ffffff?text=Summer+Sale" },
        new() { Id = "2", Title = "منتجات جديدة", Subtitle = "اكتشف الأحدث", ImageUrl = "https://via.placeholder.com/400x200/22c55e/ffffff?text=New+Arrivals" },
    };

    public List<HomeProductItem> GetFeaturedProducts() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "ساعة ذكية برو", Image = "https://via.placeholder.com/200x200/333/fff?text=Watch", Price = 599, OldPrice = 799, Rating = 4.5, ReviewCount = 128, Attributes = new() { { "اللون", "أسود" }, { "الحجم", "42mm" } } },
        new() { Id = Guid.NewGuid(), Name = "سماعة لاسلكية", Image = "https://via.placeholder.com/200x200/333/fff?text=Headphones", Price = 299, Rating = 4.8, ReviewCount = 256 },
        new() { Id = Guid.NewGuid(), Name = "حقيبة جلدية", Image = "https://via.placeholder.com/200x200/8b4513/fff?text=Bag", Price = 450, OldPrice = 550, Rating = 4.2, ReviewCount = 64 },
        new() { Id = Guid.NewGuid(), Name = "نظارة شمسية", Image = "https://via.placeholder.com/200x200/000/fff?text=Sunglasses", Price = 199, Rating = 4.6, ReviewCount = 89 },
    };

    public List<HomeProductItem> GetNewProducts() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "هاتف Galaxy S24", Image = "https://via.placeholder.com/200x200/1a1a2e/fff?text=Phone", Price = 3499, Rating = 4.9, ReviewCount = 45 },
        new() { Id = Guid.NewGuid(), Name = "لابتوب MacBook Pro", Image = "https://via.placeholder.com/200x200/c0c0c0/333?text=Laptop", Price = 7999, Rating = 4.7, ReviewCount = 23 },
        new() { Id = Guid.NewGuid(), Name = "كاميرا Sony Alpha", Image = "https://via.placeholder.com/200x200/333/fff?text=Camera", Price = 4599, Rating = 4.8, ReviewCount = 12 },
    };

    public List<HomeProductItem> GetBestSellers() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "شاحن سريع", Image = "https://via.placeholder.com/200x200/fff/333?text=Charger", Price = 79, Rating = 4.4, ReviewCount = 512 },
        new() { Id = Guid.NewGuid(), Name = "كابل USB-C", Image = "https://via.placeholder.com/200x200/333/fff?text=Cable", Price = 29, Rating = 4.3, ReviewCount = 1024 },
        new() { Id = Guid.NewGuid(), Name = "حافظة هاتف", Image = "https://via.placeholder.com/200x200/2563eb/fff?text=Case", Price = 49, OldPrice = 69, Rating = 4.1, ReviewCount = 789 },
        new() { Id = Guid.NewGuid(), Name = "سماعات AirPods", Image = "https://via.placeholder.com/200x200/fff/333?text=AirPods", Price = 899, Rating = 4.6, ReviewCount = 356 },
        new() { Id = Guid.NewGuid(), Name = "ماوس لاسلكي", Image = "https://via.placeholder.com/200x200/333/fff?text=Mouse", Price = 149, Rating = 4.5, ReviewCount = 234 },
        new() { Id = Guid.NewGuid(), Name = "لوحة مفاتيح", Image = "https://via.placeholder.com/200x200/1a1a2e/fff?text=Keyboard", Price = 249, OldPrice = 299, Rating = 4.4, ReviewCount = 178 },
    };

    public ProductDetailsItem GetProductDetails(Guid id) => new()
    {
        Id = id,
        Name = "ساعة ذكية برو ماكس",
        Brand = "TechWear",
        ShortDescription = "ساعة ذكية متطورة مع مراقبة صحية متقدمة وبطارية تدوم 7 أيام",
        LongDescription = "<p>ساعة ذكية متطورة تجمع بين الأناقة والتقنية. تتميز بشاشة AMOLED عالية الدقة ومقاومة للماء حتى 50 متر.</p><ul><li>مراقبة معدل ضربات القلب على مدار الساعة</li><li>تتبع النوم المتقدم</li><li>أكثر من 100 وضع رياضي</li><li>GPS مدمج</li></ul>",
        Images = new List<string>
        {
            "https://via.placeholder.com/600x600/333/fff?text=Watch+Front",
            "https://via.placeholder.com/600x600/333/fff?text=Watch+Side",
            "https://via.placeholder.com/600x600/333/fff?text=Watch+Back",
            "https://via.placeholder.com/600x600/333/fff?text=Watch+Box"
        },
        Price = 599,
        OldPrice = 799,
        Rating = 4.5,
        ReviewCount = 128,
        StockQuantity = 15,
        Sku = "SW-PRO-MAX-001",
        DeliveryInfo = "التوصيل خلال 2-3 أيام عمل",
        ReturnPolicy = "إرجاع مجاني خلال 14 يوم",
        Warranty = "ضمان لمدة سنتين",
        Attributes = new List<DynamicAttribute>
        {
            new() { Id = "color", Label = "اللون", Type = AttributeDisplayType.ColorSwatch, Values = new() { "#000000", "#ffffff", "#2563eb", "#22c55e" }, SelectedValue = "#000000" },
            new() { Id = "size", Label = "الحجم", Type = AttributeDisplayType.ButtonGroup, Values = new() { "40mm", "42mm", "44mm" }, SelectedValue = "42mm", DisabledValues = new() { "40mm" } },
            new() { Id = "band", Label = "نوع السوار", Type = AttributeDisplayType.Dropdown, Values = new() { "سيليكون", "جلد", "معدن" }, SelectedValue = "سيليكون" }
        },
        Specifications = new Dictionary<string, string>
        {
            { "الشاشة", "AMOLED 1.4 بوصة" },
            { "الدقة", "466 × 466 بكسل" },
            { "البطارية", "420 mAh" },
            { "مقاومة الماء", "5ATM (50 متر)" },
            { "الاتصال", "Bluetooth 5.2, Wi-Fi, GPS" },
            { "المستشعرات", "معدل ضربات القلب، SpO2، مقياس التسارع" },
            { "التوافق", "iOS 13+, Android 8+" },
            { "الوزن", "45 جرام" }
        }
    };

    public List<CartItemModel> GetCartItems() => new()
    {
        new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Name = "ساعة ذكية برو", ImageUrl = "https://via.placeholder.com/100x100/333/fff?text=Watch", Price = 599, Quantity = 1, Attributes = new() { { "اللون", "أسود" }, { "الحجم", "42mm" } } },
        new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Name = "سماعة لاسلكية", ImageUrl = "https://via.placeholder.com/100x100/333/fff?text=Headphones", Price = 299, Quantity = 2 },
        new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Name = "شاحن سريع", ImageUrl = "https://via.placeholder.com/100x100/fff/333?text=Charger", Price = 79, OldPrice = 99, Quantity = 1 },
    };

    public List<NotificationItem> GetNotifications() => new()
    {
        new() { Id = Guid.NewGuid(), Title = "تم شحن طلبك", Message = "طلبك رقم #12345 في الطريق إليك", Type = NotificationType.Delivery, Timestamp = DateTime.Now.AddHours(-2), IsRead = false },
        new() { Id = Guid.NewGuid(), Title = "عرض خاص", Message = "خصم 30% على جميع الإلكترونيات اليوم فقط!", Type = NotificationType.Promotion, Timestamp = DateTime.Now.AddHours(-5), IsRead = false },
        new() { Id = Guid.NewGuid(), Title = "تم تأكيد الدفع", Message = "تم استلام دفعتك بنجاح للطلب #12344", Type = NotificationType.Payment, Timestamp = DateTime.Now.AddDays(-1), IsRead = true },
        new() { Id = Guid.NewGuid(), Title = "رسالة جديدة", Message = "لديك رسالة جديدة من متجر TechStore", Type = NotificationType.Message, Timestamp = DateTime.Now.AddDays(-1), IsRead = true },
        new() { Id = Guid.NewGuid(), Title = "قيّم مشترياتك", Message = "شاركنا رأيك في المنتجات التي اشتريتها", Type = NotificationType.Review, Timestamp = DateTime.Now.AddDays(-3), IsRead = true },
    };

    public List<ConversationItem> GetConversations() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "متجر TechStore", LastMessage = "شكراً لتواصلك معنا", LastMessageTime = DateTime.Now.AddMinutes(-30), UnreadCount = 2, IsOnline = true },
        new() { Id = Guid.NewGuid(), Name = "دعم العملاء", LastMessage = "تم حل المشكلة بنجاح", LastMessageTime = DateTime.Now.AddHours(-3), UnreadCount = 0, IsOnline = true },
        new() { Id = Guid.NewGuid(), Name = "متجر Fashion Hub", LastMessage = "المنتج متوفر الآن", LastMessageTime = DateTime.Now.AddDays(-1), UnreadCount = 1, IsOnline = false },
    };

    public UserProfileInfo GetUserProfile() => new()
    {
        Id = Guid.NewGuid(),
        Name = "أحمد محمد",
        Email = "ahmed@example.com",
        Phone = "+966501234567",
        AvatarUrl = null,
        MemberSince = DateTime.Now.AddYears(-2),
        IsVerified = true
    };

    public List<PaymentMethodModel> GetPaymentMethods() => new()
    {
        new() { Id = "card", Name = "بطاقة ائتمان/مدى", Description = "Visa, Mastercard, مدى", Icon = "bi-credit-card" },
        new() { Id = "apple_pay", Name = "Apple Pay", Icon = "bi-apple" },
        new() { Id = "stc_pay", Name = "STC Pay", Icon = "bi-phone" },
        new() { Id = "cod", Name = "الدفع عند الاستلام", Description = "رسوم إضافية 10 ر.س", Icon = "bi-cash" }
    };

    public List<AddressModel> GetSavedAddresses() => new()
    {
        new() { Id = Guid.NewGuid(), Name = "أحمد محمد", Phone = "+966501234567", Street = "شارع الملك فهد، مبنى 123", City = "الرياض", Region = "منطقة الرياض", PostalCode = "12345", IsDefault = true },
        new() { Id = Guid.NewGuid(), Name = "أحمد محمد", Phone = "+966501234567", Street = "شارع الأمير سلطان، فيلا 45", City = "جدة", Region = "منطقة مكة المكرمة", PostalCode = "23456", IsDefault = false }
    };
}
