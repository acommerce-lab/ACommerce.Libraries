namespace Ashare.Shared.Services;

/// <summary>
/// خدمة البيانات للمساحات المشتركة (Mock Data)
/// </summary>
public class SpaceDataService
{
    private static readonly List<SpaceCategory> _categories =
    [
        new() { Id = Guid.NewGuid(), Name = "قاعات اجتماعات", NameEn = "Meeting Rooms", Icon = "bi-people", Color = "#0EA5E9" },
        new() { Id = Guid.NewGuid(), Name = "مكاتب مشتركة", NameEn = "Co-Working", Icon = "bi-laptop", Color = "#8B5CF6" },
        new() { Id = Guid.NewGuid(), Name = "قاعات فعاليات", NameEn = "Event Halls", Icon = "bi-calendar-event", Color = "#F59E0B" },
        new() { Id = Guid.NewGuid(), Name = "استوديوهات", NameEn = "Studios", Icon = "bi-camera-video", Color = "#EC4899" },
        new() { Id = Guid.NewGuid(), Name = "مساحات تجارية", NameEn = "Commercial", Icon = "bi-shop", Color = "#10B981" },
        new() { Id = Guid.NewGuid(), Name = "سكن مشترك", NameEn = "Co-Living", Icon = "bi-house", Color = "#6366F1" },
    ];

    private static readonly Guid _ownerId = Guid.NewGuid();

    private static readonly List<SpaceItem> _spaces =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Name = "قاعة الأعمال الذهبية",
            NameEn = "Golden Business Hall",
            Description = "قاعة اجتماعات فاخرة مجهزة بأحدث التقنيات. تتميز بموقعها المتميز في قلب حي العليا، وتوفر بيئة عمل احترافية مثالية للاجتماعات المهمة والعروض التقديمية.",
            CategoryId = _categories[0].Id,
            CategoryName = "قاعات اجتماعات",
            Location = "الرياض، حي العليا",
            City = "الرياض",
            PricePerHour = 150,
            PricePerDay = 1000,
            PricePerMonth = 15000,
            Currency = "ر.س",
            Capacity = 20,
            Area = 50,
            Rating = 4.8m,
            ReviewsCount = 124,
            Images = [
                "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800",
                "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=800",
                "https://images.unsplash.com/photo-1497215842964-222b430dc094?w=800"
            ],
            Amenities = ["واي فاي", "شاشة عرض", "سبورة", "مكيف", "قهوة وشاي", "موقف سيارات"],
            Rules = ["الالتزام بالمواعيد", "الحفاظ على نظافة المكان", "عدم التدخين داخل القاعة"],
            OwnerId = _ownerId,
            OwnerName = "أحمد العتيبي",
            OwnerJoinDate = new DateTime(2022, 1, 15),
            IsNew = true,
            IsFeatured = true,
            CreatedAt = DateTime.Now.AddDays(-5)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "مساحة العمل المشترك",
            NameEn = "Shared Workspace Hub",
            Description = "بيئة عمل ملهمة مع مكاتب مريحة وخدمات متكاملة. مثالية للمستقلين والشركات الناشئة الباحثين عن مساحة عمل احترافية بتكلفة معقولة.",
            CategoryId = _categories[1].Id,
            CategoryName = "مكاتب مشتركة",
            Location = "جدة، حي الروضة",
            City = "جدة",
            PricePerHour = 50,
            PricePerDay = 300,
            PricePerMonth = 3000,
            Currency = "ر.س",
            Capacity = 50,
            Area = 200,
            Rating = 4.6m,
            ReviewsCount = 89,
            Images = [
                "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=800",
                "https://images.unsplash.com/photo-1497215842964-222b430dc094?w=800"
            ],
            Amenities = ["واي فاي", "طابعة", "مطبخ", "غرفة اجتماعات", "موقف سيارات"],
            Rules = ["احترام خصوصية الآخرين", "الهدوء أثناء العمل"],
            OwnerId = _ownerId,
            OwnerName = "سارة الغامدي",
            OwnerJoinDate = new DateTime(2021, 6, 10),
            IsFeatured = true,
            CreatedAt = DateTime.Now.AddDays(-30)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "قاعة المناسبات الكبرى",
            NameEn = "Grand Events Hall",
            Description = "قاعة واسعة للحفلات والمؤتمرات الكبيرة. مجهزة بأحدث أنظمة الصوت والإضاءة لتقديم تجربة استثنائية لضيوفك.",
            CategoryId = _categories[2].Id,
            CategoryName = "قاعات فعاليات",
            Location = "الرياض، حي الملقا",
            City = "الرياض",
            PricePerHour = 500,
            PricePerDay = 5000,
            PricePerMonth = 50000,
            Currency = "ر.س",
            Capacity = 300,
            Area = 500,
            Rating = 4.9m,
            ReviewsCount = 56,
            Images = [
                "https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800"
            ],
            Amenities = ["إضاءة احترافية", "نظام صوت", "مسرح", "كراسي", "مكيف"],
            Rules = ["الحجز المسبق بأسبوع على الأقل", "دفعة مقدمة 50%"],
            OwnerId = _ownerId,
            OwnerName = "محمد القحطاني",
            OwnerJoinDate = new DateTime(2020, 3, 1),
            IsNew = true,
            CreatedAt = DateTime.Now.AddDays(-3)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "استوديو التصوير الإبداعي",
            NameEn = "Creative Photo Studio",
            Description = "استوديو مجهز للتصوير الفوتوغرافي والفيديو. يوفر بيئة مثالية للمصورين ومنشئي المحتوى مع جميع المعدات الاحترافية.",
            CategoryId = _categories[3].Id,
            CategoryName = "استوديوهات",
            Location = "الدمام، حي الفيصلية",
            City = "الدمام",
            PricePerHour = 200,
            PricePerDay = 1500,
            PricePerMonth = 12000,
            Currency = "ر.س",
            Capacity = 10,
            Area = 80,
            Rating = 4.7m,
            ReviewsCount = 43,
            Images = [
                "https://images.unsplash.com/photo-1478737270239-2f02b77fc618?w=800"
            ],
            Amenities = ["إضاءة استوديو", "خلفيات متنوعة", "غرفة تغيير", "معدات تصوير"],
            OwnerId = _ownerId,
            OwnerName = "نورة الشهري",
            OwnerJoinDate = new DateTime(2023, 2, 20),
            CreatedAt = DateTime.Now.AddDays(-60)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "محل تجاري في مول",
            NameEn = "Mall Retail Space",
            Description = "موقع استراتيجي في مول تجاري حيوي. فرصة ممتازة للعلامات التجارية للوصول إلى جمهور واسع من المتسوقين.",
            CategoryId = _categories[4].Id,
            CategoryName = "مساحات تجارية",
            Location = "الرياض، غرناطة مول",
            City = "الرياض",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 15000,
            Currency = "ر.س",
            Capacity = 0,
            Area = 100,
            Rating = 4.5m,
            ReviewsCount = 28,
            Images = [
                "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800"
            ],
            Amenities = ["واجهة زجاجية", "مكيف", "كهرباء", "موقف سيارات"],
            Rules = ["عقد سنوي كحد أدنى", "التزام بنظام المول"],
            OwnerId = _ownerId,
            OwnerName = "فهد الدوسري",
            OwnerJoinDate = new DateTime(2019, 11, 5),
            CreatedAt = DateTime.Now.AddDays(-120)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "شقة سكن مشترك للطلاب",
            NameEn = "Student Co-Living Apartment",
            Description = "سكن مشترك مريح بالقرب من الجامعات. بيئة آمنة ومريحة للطلاب مع جميع الخدمات الأساسية.",
            CategoryId = _categories[5].Id,
            CategoryName = "سكن مشترك",
            Location = "الرياض، حي النزهة",
            City = "الرياض",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 1500,
            Currency = "ر.س",
            Capacity = 4,
            Area = 120,
            Rating = 4.4m,
            ReviewsCount = 67,
            Images = [
                "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
            ],
            Amenities = ["غرفة مفروشة", "مطبخ مشترك", "واي فاي", "غسالة", "نظافة أسبوعية"],
            Rules = ["للطلاب فقط", "عدم التدخين", "الهدوء بعد الساعة 10 مساءً"],
            OwnerId = _ownerId,
            OwnerName = "خالد السالم",
            OwnerJoinDate = new DateTime(2022, 8, 1),
            IsNew = true,
            CreatedAt = DateTime.Now.AddDays(-7)
        }
    ];

    private static readonly List<BookingItem> _bookings =
    [
        new()
        {
            Id = Guid.NewGuid(),
            SpaceId = _spaces[0].Id,
            SpaceName = _spaces[0].Name,
            SpaceImage = _spaces[0].Images.FirstOrDefault(),
            Date = DateTime.Today.AddDays(3),
            StartTime = new TimeOnly (9, 0, 0),
            EndTime = new TimeOnly(12, 0, 0),
            GuestsCount = 8,
            TotalPrice = 450,
            Currency = "ر.س",
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now.AddDays(-2)
        },
        new()
        {
            Id = Guid.NewGuid(),
            SpaceId = _spaces[1].Id,
            SpaceName = _spaces[1].Name,
            SpaceImage = _spaces[1].Images.FirstOrDefault(),
            Date = DateTime.Today.AddDays(-10),
            StartTime = new TimeOnly(8, 0, 0),
            EndTime = new TimeOnly(18, 0, 0),
            GuestsCount = 1,
            TotalPrice = 300,
            Currency = "ر.س",
            Status = BookingStatus.Completed,
            CreatedAt = DateTime.Now.AddDays(-15),
            IsReviewed = true
        }
    ];

    private static readonly List<SpaceReview> _reviews =
    [
        new()
        {
            Id = Guid.NewGuid(),
            SpaceId = _spaces[0].Id,
            UserId = Guid.NewGuid(),
            UserName = "محمد أحمد",
            Rating = 5,
            Comment = "قاعة ممتازة ومجهزة بالكامل. الخدمة كانت رائعة والموقع سهل الوصول إليه.",
            Date = DateTime.Now.AddDays(-5)
        },
        new()
        {
            Id = Guid.NewGuid(),
            SpaceId = _spaces[0].Id,
            UserId = Guid.NewGuid(),
            UserName = "سارة علي",
            Rating = 4,
            Comment = "تجربة جيدة بشكل عام. القاعة نظيفة ومرتبة.",
            Date = DateTime.Now.AddDays(-10)
        },
        new()
        {
            Id = Guid.NewGuid(),
            SpaceId = _spaces[1].Id,
            UserId = Guid.NewGuid(),
            UserName = "أحمد خالد",
            Rating = 5,
            Comment = "مساحة عمل رائعة! الإنترنت سريع جداً والبيئة ملهمة.",
            Date = DateTime.Now.AddDays(-20)
        }
    ];

    private static readonly HashSet<Guid> _favorites = new();

    // Categories
    public List<SpaceCategory> GetCategories() => _categories;

    // Spaces
    public List<SpaceItem> GetFeaturedSpaces() => _spaces.Where(s => s.IsFeatured).ToList();
    public List<SpaceItem> GetNewSpaces() => _spaces.Where(s => s.IsNew).OrderByDescending(s => s.CreatedAt).ToList();
    public List<SpaceItem> GetAllSpaces() => _spaces;
    public SpaceItem? GetSpaceById(Guid id) => _spaces.FirstOrDefault(s => s.Id == id);

    public List<SpaceItem> SearchSpaces(string? query = null, Guid? categoryId = null, string? city = null, decimal? maxPrice = null)
    {
        var result = _spaces.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
            result = result.Where(s => s.Name.Contains(query) || s.Description.Contains(query));

        if (categoryId.HasValue)
            result = result.Where(s => s.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(city))
            result = result.Where(s => s.City == city);

        if (maxPrice.HasValue)
            result = result.Where(s => s.PricePerHour <= maxPrice || s.PricePerDay <= maxPrice);

        return result.ToList();
    }

    // Favorites
    public HashSet<Guid> GetFavorites() => _favorites;

    public void ToggleFavorite(Guid spaceId)
    {
        if (_favorites.Contains(spaceId))
            _favorites.Remove(spaceId);
        else
            _favorites.Add(spaceId);
    }

    // Reviews
    public List<SpaceReview> GetSpaceReviews(Guid spaceId) =>
        _reviews.Where(r => r.SpaceId == spaceId).OrderByDescending(r => r.Date).ToList();

    public void AddReview(Guid spaceId, int rating, string comment)
    {
        _reviews.Add(new SpaceReview
        {
            Id = Guid.NewGuid(),
            SpaceId = spaceId,
            UserId = Guid.NewGuid(),
            UserName = "أنت",
            Rating = rating,
            Comment = comment,
            Date = DateTime.Now
        });

        // Update space rating
        var space = _spaces.FirstOrDefault(s => s.Id == spaceId);
        if (space != null)
        {
            var spaceReviews = _reviews.Where(r => r.SpaceId == spaceId).ToList();
            space.Rating = (decimal)spaceReviews.Average(r => r.Rating);
            space.ReviewsCount = spaceReviews.Count;
        }
    }

    // Bookings
    public List<BookingItem> GetBookings() => _bookings;

    public void CreateBooking(BookingItem booking)
    {
        booking.Id = Guid.NewGuid();
        booking.CreatedAt = DateTime.Now;
        _bookings.Add(booking);
    }

    public BookingItem CreateBooking(Guid spaceId, DateTime date, TimeOnly startTime, TimeOnly endTime)
    {
        var space = GetSpaceById(spaceId);
        if (space == null) throw new Exception("Space not found");

        var booking = new BookingItem
        {
            Id = Guid.NewGuid(),
            SpaceId = spaceId,
            SpaceName = space.Name,
            SpaceImage = space.Images.FirstOrDefault(),
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            TotalPrice = space.PricePerHour * (decimal)(endTime - startTime).TotalHours,
            Currency = space.Currency,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };

        _bookings.Add(booking);
        return booking;
    }

    public void CancelBooking(Guid bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
        }
    }

    // Stats
    public int GetBookingsCount() => _bookings.Count(b => b.Status != BookingStatus.Cancelled);
    public int GetNotificationsCount() => 3;
}

// ══════════════════════════════════════════════════════════════════
// Models
// ══════════════════════════════════════════════════════════════════

public class SpaceCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Icon { get; set; }
    public string? Image { get; set; }
    public string? Color { get; set; }
}

public class SpaceItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }
    public string Currency { get; set; } = "SAR";
    public int Capacity { get; set; }
    public int Area { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int ViewCount { get; set; }
    public List<string> Images { get; set; } = [];
    public List<string> Amenities { get; set; } = [];
    public List<string>? Rules { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime OwnerJoinDate { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// الخصائص الديناميكية من الفئة
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    public string DisplayPrice => PricePerHour > 0
        ? $"{PricePerHour:N0} {Currency}/ساعة"
        : PricePerDay > 0
            ? $"{PricePerDay:N0} {Currency}/يوم"
            : $"{PricePerMonth:N0} {Currency}/شهر";
}

public class BookingItem
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public string? SpaceImage { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "ر.س";
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }

    // Payment-related properties
    public decimal DepositPaid { get; set; }
    public string? PaymentId { get; set; }
    public string? RentType { get; set; }
}

public class SpaceReview
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Completed = 4
}
