namespace Ashare.Shared.Services;

/// <summary>
/// خدمة البيانات للمساحات المشتركة (Mock Data)
/// </summary>
public class SpaceDataService
{
    // فئتان فقط حسب نموذج "عشير" للسكن المشترك
    private static readonly List<SpaceCategory> _categories =
    [
        new() { Id = Guid.NewGuid(), Name = "عشير عنده سكن", NameEn = "Ashir with housing", Icon = "bi-house-heart", Color = "#345454" },
        new() { Id = Guid.NewGuid(), Name = "عشير يدور سكن", NameEn = "Ashir seeking housing", Icon = "bi-person-walking", Color = "#F4844C" },
    ];

    private static readonly Guid _ownerId = Guid.NewGuid();

    // ملاحظة: _categories[0] = عشير عنده سكن، _categories[1] = عشير يدور سكن
    private static readonly List<SpaceItem> _spaces =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Name = "غرفة في شقة مشتركة - حي الملقا",
            NameEn = "Room in shared apartment - Al Malqa",
            Description = "أنا عشير عنده سكن في شقة بحي الملقا، غرفة خاصة فاضية وأبحث عن عشير يشاركني السكن. الشقة مفروشة بالكامل وفيها كل الاحتياجات الأساسية، ومناسبة لشخص هادئ وملتزم.",
            CategoryId = _categories[0].Id,
            CategoryName = "عشير عنده سكن",
            Location = "الرياض، حي الملقا",
            City = "الرياض",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 1500,
            Currency = "ر.س",
            Capacity = 1,
            Area = 18,
            Rating = 4.6m,
            ReviewsCount = 12,
            Images = [
                "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                "https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800"
            ],
            Amenities = ["غرفة مفروشة", "مطبخ مشترك", "واي فاي", "غسالة", "مكيف"],
            Rules = ["عدم التدخين داخل الشقة", "الهدوء بعد الساعة 11 مساءً", "احترام خصوصية الجميع"],
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
            Name = "موظف يبحث عن عشير في الرياض",
            NameEn = "Employee seeking Ashir in Riyadh",
            Description = "أنا عشير ما عنده سكن، موظف في الرياض وأبحث عن عشير عنده سكن (غرفة مستقلة) في شمال الرياض. ميزانيتي حتى 1800 ر.س شهرياً، شخص ملتزم بمواعيد العمل وهادئ.",
            CategoryId = _categories[1].Id,
            CategoryName = "عشير يدور سكن",
            Location = "الرياض، شمال الرياض",
            City = "الرياض",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 1800,
            Currency = "ر.س",
            Capacity = 1,
            Area = 0,
            Rating = 4.8m,
            ReviewsCount = 8,
            Images = [
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800"
            ],
            Amenities = [],
            Rules = ["مدة الإقامة سنة على الأقل", "أفضّل شخصاً غير مدخّن"],
            OwnerId = _ownerId,
            OwnerName = "سارة الغامدي",
            OwnerJoinDate = new DateTime(2023, 6, 10),
            IsFeatured = true,
            CreatedAt = DateTime.Now.AddDays(-10)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "غرفة في فيلا - حي النرجس",
            NameEn = "Room in villa - Al Narjis",
            Description = "غرفة واسعة في فيلا بحي النرجس، أبحث عن عشير يشاركني السكن. الفيلا فيها مجلس مشترك ومطبخ كبير وحديقة صغيرة، والموقع قريب من مترو الرياض.",
            CategoryId = _categories[0].Id,
            CategoryName = "عشير عنده سكن",
            Location = "الرياض، حي النرجس",
            City = "الرياض",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 2000,
            Currency = "ر.س",
            Capacity = 1,
            Area = 22,
            Rating = 4.9m,
            ReviewsCount = 21,
            Images = [
                "https://images.unsplash.com/photo-1493809842364-78817add7ffb?w=800"
            ],
            Amenities = ["غرفة مفروشة", "مكيف", "واي فاي", "موقف سيارات", "حديقة"],
            Rules = ["عدم التدخين", "احترام أوقات الراحة"],
            OwnerId = _ownerId,
            OwnerName = "محمد القحطاني",
            OwnerJoinDate = new DateTime(2020, 3, 1),
            IsNew = true,
            CreatedAt = DateTime.Now.AddDays(-3)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "غرفة سكن طلابي قرب الجامعة",
            NameEn = "Student room near university",
            Description = "غرفة سكن مشترك بالقرب من جامعة الملك سعود. بيئة هادئة مناسبة للدراسة، أبحث عن عشير طالب يشاركني الشقة.",
            CategoryId = _categories[0].Id,
            CategoryName = "عشير عنده سكن",
            Location = "الرياض، حي النزهة",
            City = "الرياض",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 1200,
            Currency = "ر.س",
            Capacity = 1,
            Area = 16,
            Rating = 4.4m,
            ReviewsCount = 67,
            Images = [
                "https://images.unsplash.com/photo-1555854877-bab0e564b8d5?w=800"
            ],
            Amenities = ["غرفة مفروشة", "مطبخ مشترك", "واي فاي", "غسالة"],
            Rules = ["للطلاب فقط", "عدم التدخين", "الهدوء بعد الساعة 10 مساءً"],
            OwnerId = _ownerId,
            OwnerName = "خالد السالم",
            OwnerJoinDate = new DateTime(2022, 8, 1),
            IsNew = true,
            CreatedAt = DateTime.Now.AddDays(-7)
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "طالب جامعي يبحث عن عشير في جدة",
            NameEn = "University student seeking Ashir in Jeddah",
            Description = "طالب جامعي في جدة، أبحث عن عشير عنده سكن قريب من جامعة الملك عبدالعزيز. ميزانيتي حتى 1200 ر.س، شخص ملتزم بدراسته وهادئ.",
            CategoryId = _categories[1].Id,
            CategoryName = "عشير يدور سكن",
            Location = "جدة، حي السلامة",
            City = "جدة",
            PricePerHour = 0,
            PricePerDay = 0,
            PricePerMonth = 1200,
            Currency = "ر.س",
            Capacity = 1,
            Area = 0,
            Rating = 4.5m,
            ReviewsCount = 5,
            Images = [
                "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=800"
            ],
            Amenities = [],
            Rules = ["مدة الإقامة فصل دراسي على الأقل"],
            OwnerId = _ownerId,
            OwnerName = "فهد الدوسري",
            OwnerJoinDate = new DateTime(2024, 1, 5),
            CreatedAt = DateTime.Now.AddDays(-15)
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

    public string DisplayPrice
    {
        get
        {
            // أولاً: التحقق من time_unit أو rent_type في الخصائص
            string? timeUnit = null;
            if (Attributes?.TryGetValue("time_unit", out var timeUnitObj) == true && timeUnitObj != null)
            {
                timeUnit = timeUnitObj.ToString()?.ToLower();
            }
            else if (Attributes?.TryGetValue("rent_type", out var rentTypeObj) == true && rentTypeObj != null)
            {
                timeUnit = rentTypeObj.ToString()?.ToLower();
            }

            if (!string.IsNullOrEmpty(timeUnit))
            {
                var price = Attributes!.TryGetValue("price", out var priceObj) && priceObj != null
                    ? decimal.TryParse(priceObj.ToString(), out var p) ? p : 0
                    : PricePerMonth > 0 ? PricePerMonth : PricePerDay > 0 ? PricePerDay : PricePerHour;

                return timeUnit switch
                {
                    "hour" or "hourly" => $"{price:N0} {Currency}/ساعة",
                    "day" or "daily" => $"{price:N0} {Currency}/يوم",
                    "week" or "weekly" => $"{price:N0} {Currency}/أسبوع",
                    "month" or "monthly" => $"{price:N0} {Currency}/شهر",
                    "year" or "yearly" or "annual" => $"{price:N0} {Currency}/سنة",
                    _ => $"{price:N0} {Currency}/شهر"
                };
            }

            // ثانياً: الطريقة التقليدية بناءً على الأسعار المعينة
            if (PricePerHour > 0)
                return $"{PricePerHour:N0} {Currency}/ساعة";
            if (PricePerDay > 0)
                return $"{PricePerDay:N0} {Currency}/يوم";
            return $"{PricePerMonth:N0} {Currency}/شهر";
        }
    }
}

public class BookingItem
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public Guid HostId { get; set; }
    public string? CustomerId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public string? SpaceImage { get; set; }
    public string? SpaceLocation { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "ر.س";
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CustomerNotes { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Payment-related properties
    public decimal DepositPaid { get; set; }
    public decimal DepositAmount => DepositPaid; // alias for backward compatibility
    public bool IsDepositPaid => DepositPaid > 0;
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
