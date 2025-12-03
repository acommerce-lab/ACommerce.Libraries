using System.Globalization;

namespace Ashare.Shared.Services;

/// <summary>
/// واجهة خدمة الترجمة
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// اللغة الحالية
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// اللغات المدعومة
    /// </summary>
    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

    /// <summary>
    /// هل الاتجاه من اليمين لليسار
    /// </summary>
    bool IsRtl { get; }

    /// <summary>
    /// تغيير اللغة
    /// </summary>
    Task SetLanguageAsync(string languageCode);

    /// <summary>
    /// الحصول على نص مترجم
    /// </summary>
    string this[string key] { get; }

    /// <summary>
    /// الحصول على نص مترجم مع معاملات
    /// </summary>
    string Get(string key, params object[] args);

    /// <summary>
    /// حدث تغيير اللغة
    /// </summary>
    event Action? OnLanguageChanged;
}

/// <summary>
/// معلومات اللغة
/// </summary>
public class LanguageInfo
{
    public string Code { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public bool IsRtl { get; set; }
}

/// <summary>
/// خدمة الترجمة
/// </summary>
public class LocalizationService : ILocalizationService
{
    private string _currentLanguage = "ar";
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private readonly IStorageService _storageService;

    public LocalizationService(IStorageService storageService)
    {
        _storageService = storageService;
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["ar"] = GetArabicStrings(),
            ["en"] = GetEnglishStrings(),
            ["ur"] = GetUrduStrings()
        };

        // تحميل اللغة المحفوظة
        _ = LoadSavedLanguageAsync();
    }

    public string CurrentLanguage => _currentLanguage;

    public IReadOnlyList<LanguageInfo> SupportedLanguages =>
    [
        new() { Code = "ar", NativeName = "العربية", EnglishName = "Arabic", IsRtl = true },
        new() { Code = "en", NativeName = "English", EnglishName = "English", IsRtl = false },
        new() { Code = "ur", NativeName = "اردو", EnglishName = "Urdu", IsRtl = true }
    ];

    public bool IsRtl => _currentLanguage is "ar" or "ur";

    public event Action? OnLanguageChanged;

    public string this[string key] => Get(key);

    public string Get(string key, params object[] args)
    {
        if (_translations.TryGetValue(_currentLanguage, out var langStrings) &&
            langStrings.TryGetValue(key, out var value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }

        // Fallback to English
        if (_translations.TryGetValue("en", out var enStrings) &&
            enStrings.TryGetValue(key, out var enValue))
        {
            return args.Length > 0 ? string.Format(enValue, args) : enValue;
        }

        return key;
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        if (_currentLanguage == languageCode) return;

        _currentLanguage = languageCode;
        CultureInfo.CurrentCulture = new CultureInfo(languageCode);
        CultureInfo.CurrentUICulture = new CultureInfo(languageCode);

        // Save preference
        try
        {
            await _storageService.SetAsync("app_language", languageCode);
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    private async Task LoadSavedLanguageAsync()
    {
        try
        {
            var saved = await _storageService.GetAsync("app_language");
            if (!string.IsNullOrEmpty(saved) && _translations.ContainsKey(saved))
            {
                _currentLanguage = saved;
            }
        }
        catch { }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Arabic Strings
    // ═══════════════════════════════════════════════════════════════════
    private static Dictionary<string, string> GetArabicStrings() => new()
    {
        // App
        ["AppName"] = "عشير",
        ["AppTagline"] = "شارك المساحات، شارك الإمكانيات",

        // Navigation
        ["Home"] = "الرئيسية",
        ["Explore"] = "استكشف",
        ["Bookings"] = "حجوزاتي",
        ["Favorites"] = "المفضلة",
        ["Profile"] = "حسابي",
        ["Search"] = "البحث",
        ["Notifications"] = "الإشعارات",
        ["Messages"] = "الرسائل",

        // Auth
        ["Login"] = "تسجيل الدخول",
        ["Logout"] = "تسجيل الخروج",
        ["Register"] = "إنشاء حساب",
        ["CreateAccount"] = "إنشاء حساب جديد",
        ["WelcomeBack"] = "مرحباً بعودتك",
        ["LoginToYourAccount"] = "سجل دخولك للمتابعة",
        ["Email"] = "البريد الإلكتروني",
        ["Password"] = "كلمة المرور",
        ["ConfirmPassword"] = "تأكيد كلمة المرور",
        ["FullName"] = "الاسم الكامل",
        ["Phone"] = "رقم الجوال",
        ["EnterEmail"] = "أدخل بريدك الإلكتروني",
        ["EnterPassword"] = "أدخل كلمة المرور",
        ["EnterFullName"] = "أدخل اسمك الكامل",
        ["ReEnterPassword"] = "أعد إدخال كلمة المرور",
        ["ForgotPassword"] = "نسيت كلمة المرور؟",
        ["RememberMe"] = "تذكرني",
        ["DontHaveAccount"] = "ليس لديك حساب؟",
        ["AlreadyHaveAccount"] = "لديك حساب بالفعل؟",
        ["IAccept"] = "أوافق على",
        ["TermsAndConditions"] = "الشروط والأحكام",
        ["And"] = "و",
        ["PrivacyPolicy"] = "سياسة الخصوصية",
        ["Or"] = "أو",

        // Nafath
        ["LoginWithNafath"] = "الدخول بنفاذ",
        ["LoginWithEmail"] = "الدخول بالبريد الإلكتروني",
        ["ContinueWithNafath"] = "المتابعة بنفاذ",
        ["ContinueAsGuest"] = "المتابعة كضيف",
        ["NationalId"] = "رقم الهوية الوطنية",
        ["EnterNationalId"] = "أدخل رقم الهوية",
        ["SelectNumberInNafathApp"] = "اختر هذا الرقم في تطبيق نفاذ",
        ["SecondsRemaining"] = "ثانية متبقية",
        ["LoginSuccessful"] = "تم تسجيل الدخول بنجاح",
        ["LoginRejected"] = "تم رفض تسجيل الدخول",
        ["SessionExpired"] = "انتهت صلاحية الجلسة",
        ["TryAgain"] = "حاول مرة أخرى",

        // Authentication Required
        ["LoginRequired"] = "يجب تسجيل الدخول",
        ["LoginToCreateListing"] = "يجب تسجيل الدخول لإنشاء عرض جديد",

        // Spaces
        ["Spaces"] = "المساحات",
        ["AllSpaces"] = "كل المساحات",
        ["FeaturedSpaces"] = "المساحات المميزة",
        ["NewSpaces"] = "أحدث المساحات",
        ["NearbySpaces"] = "القريبة منك",
        ["NoSpaces"] = "لا توجد مساحات",
        ["Featured"] = "مميز",
        ["New"] = "جديد",

        // Categories
        ["Categories"] = "الفئات",
        ["MeetingRooms"] = "قاعات اجتماعات",
        ["CoWorking"] = "مكاتب مشتركة",
        ["EventHalls"] = "قاعات فعاليات",
        ["Studios"] = "استوديوهات",
        ["Commercial"] = "مساحات تجارية",
        ["CoLiving"] = "سكن مشترك",

        // Space Details
        ["Description"] = "الوصف",
        ["Amenities"] = "المرافق والخدمات",
        ["Pricing"] = "الأسعار",
        ["PerHour"] = "بالساعة",
        ["PerDay"] = "باليوم",
        ["PerMonth"] = "بالشهر",
        ["Capacity"] = "السعة",
        ["Area"] = "المساحة",
        ["Person"] = "شخص",
        ["SquareMeter"] = "م²",
        ["Rules"] = "القواعد والشروط",
        ["Owner"] = "مالك المساحة",
        ["MemberSince"] = "عضو منذ",
        ["Contact"] = "تواصل",
        ["Reviews"] = "التقييمات",
        ["ViewAll"] = "عرض الكل",
        ["NoReviews"] = "لا توجد تقييمات بعد",
        ["ViewMap"] = "عرض الخريطة",

        // Booking
        ["BookNow"] = "احجز الآن",
        ["BookSpace"] = "احجز المساحة",
        ["BookingType"] = "نوع الحجز",
        ["Hourly"] = "بالساعة",
        ["Daily"] = "باليوم",
        ["Monthly"] = "بالشهر",
        ["Date"] = "التاريخ",
        ["FromTime"] = "من الساعة",
        ["ToTime"] = "إلى الساعة",
        ["Guests"] = "عدد الأشخاص",
        ["Notes"] = "ملاحظات",
        ["Optional"] = "اختياري",
        ["Total"] = "المجموع",
        ["ConfirmBooking"] = "تأكيد الحجز",
        ["Cancel"] = "إلغاء",

        // Bookings
        ["MyBookings"] = "حجوزاتي",
        ["Upcoming"] = "القادمة",
        ["Past"] = "السابقة",
        ["Cancelled"] = "الملغاة",
        ["NoUpcomingBookings"] = "لا توجد حجوزات قادمة",
        ["NoPastBookings"] = "لا توجد حجوزات سابقة",
        ["NoCancelledBookings"] = "لا توجد حجوزات ملغاة",
        ["BookingPending"] = "قيد المراجعة",
        ["BookingConfirmed"] = "مؤكد",
        ["BookingCompleted"] = "مكتمل",
        ["BookingCancelled"] = "ملغي",
        ["CancelBooking"] = "إلغاء الحجز",
        ["ConfirmCancelBooking"] = "هل أنت متأكد من إلغاء هذا الحجز؟",
        ["RefundInfo"] = "سيتم استرداد المبلغ خلال 3-5 أيام عمل",
        ["ConfirmCancel"] = "تأكيد الإلغاء",
        ["WriteReview"] = "قيّم تجربتك",
        ["RateYourExperience"] = "قيّم تجربتك",
        ["YourComment"] = "تعليقك",
        ["ShareExperience"] = "شارك تجربتك مع الآخرين...",
        ["SubmitReview"] = "إرسال التقييم",

        // Favorites
        ["NoFavorites"] = "لا توجد مساحات مفضلة",
        ["AddFavoritesHint"] = "أضف المساحات التي تعجبك لتجدها بسهولة لاحقاً",

        // Search
        ["SearchSpaces"] = "ابحث عن مساحة...",
        ["RecentSearches"] = "عمليات البحث الأخيرة",
        ["ClearAll"] = "مسح الكل",
        ["PopularCategories"] = "الفئات الشائعة",
        ["PopularSearches"] = "عمليات بحث شائعة",
        ["NearMe"] = "قريب مني",
        ["LowestPrice"] = "الأقل سعراً",
        ["HighestRated"] = "الأعلى تقييماً",
        ["Results"] = "نتيجة",
        ["NoResults"] = "لا توجد نتائج",
        ["NoResultsFor"] = "لم نجد أي مساحات تطابق",
        ["NewSearch"] = "بحث جديد",
        ["Searching"] = "جاري البحث...",

        // Filters
        ["Filters"] = "تصفية النتائج",
        ["Apply"] = "تطبيق",
        ["Reset"] = "إعادة تعيين",
        ["PriceRange"] = "نطاق السعر",
        ["From"] = "من",
        ["To"] = "إلى",
        ["MinRating"] = "الحد الأدنى للتقييم",
        ["SortBy"] = "ترتيب حسب",
        ["Newest"] = "الأحدث",
        ["PriceLowToHigh"] = "السعر: الأقل",
        ["PriceHighToLow"] = "السعر: الأعلى",
        ["Rating"] = "التقييم",

        // Profile
        ["EditProfile"] = "تعديل الملف الشخصي",
        ["CompleteYourProfile"] = "أكمل بياناتك",
        ["WelcomeToAshare"] = "مرحباً بك في عشير",
        ["PleaseCompleteYourProfile"] = "الرجاء إكمال بياناتك للمتابعة",
        ["Addresses"] = "عناويني",
        ["MyAddresses"] = "عناويني",
        ["PaymentMethods"] = "طرق الدفع",
        ["Settings"] = "الإعدادات",
        ["DarkMode"] = "الوضع الداكن",
        ["Language"] = "اللغة",
        ["PrivacySecurity"] = "الخصوصية والأمان",
        ["Help"] = "المساعدة",
        ["HelpCenter"] = "مركز المساعدة",
        ["ContactUs"] = "تواصل معنا",
        ["About"] = "عن عشير",
        ["Version"] = "الإصدار",

        // Profile Edit
        ["PersonalInfo"] = "البيانات الشخصية",
        ["ContactInfo"] = "بيانات التواصل",
        ["AddressInfo"] = "بيانات العنوان",
        ["AccountSettings"] = "إعدادات الحساب",
        ["FirstName"] = "الاسم الأول",
        ["LastName"] = "اسم العائلة",
        ["DisplayName"] = "الاسم المعروض",
        ["EnterFirstName"] = "أدخل اسمك الأول",
        ["EnterLastName"] = "أدخل اسم العائلة",
        ["EnterDisplayName"] = "أدخل الاسم المعروض",
        ["DisplayNameHint"] = "هذا الاسم سيظهر للآخرين",
        ["PhoneNumber"] = "رقم الجوال",
        ["EnterAddress"] = "أدخل عنوانك",
        ["Address"] = "العنوان",
        ["ChangePhoto"] = "تغيير الصورة",
        ["ChangePassword"] = "تغيير كلمة المرور",
        ["Verified"] = "موثق",
        ["VerifyNow"] = "توثيق الآن",
        ["SaveChanges"] = "حفظ التغييرات",
        ["Continue"] = "متابعة",
        ["ProfileUpdated"] = "تم تحديث البيانات بنجاح",
        ["AvatarUploadFailed"] = "فشل رفع الصورة",
        ["FirstNameRequired"] = "الاسم الأول مطلوب",
        ["FullNameRequired"] = "الاسم الكامل مطلوب",
        ["AuthenticationFailed"] = "فشلت المصادقة",

        // Host
        ["Host"] = "المضيف",
        ["MySpaces"] = "مساحاتي",
        ["AddNewSpace"] = "أضف مساحة جديدة",
        ["Earnings"] = "أرباحي",

        // Chat
        ["Chat"] = "المحادثات",
        ["NoChats"] = "لا توجد محادثات",
        ["StartChat"] = "بدء محادثة",
        ["StartChatHint"] = "ابدأ محادثة من صفحة أي مساحة",
        ["TypeMessage"] = "اكتب رسالة...",
        ["Send"] = "إرسال",
        ["Online"] = "متصل",
        ["Offline"] = "غير متصل",
        ["Typing"] = "يكتب...",
        ["Now"] = "الآن",
        ["NoMessagesYet"] = "لا توجد رسائل بعد",
        ["SendFirstMessage"] = "أرسل أول رسالة لبدء المحادثة",

        // Notifications
        ["AllNotifications"] = "كل الإشعارات",
        ["NoNotifications"] = "لا توجد إشعارات",
        ["MarkAllRead"] = "تحديد الكل كمقروء",

        // Common
        ["Loading"] = "جاري التحميل...",
        ["Error"] = "خطأ",
        ["Success"] = "نجاح",
        ["Retry"] = "إعادة المحاولة",
        ["Save"] = "حفظ",
        ["Delete"] = "حذف",
        ["Edit"] = "تعديل",
        ["Back"] = "رجوع",
        ["Next"] = "التالي",
        ["Done"] = "تم",
        ["Close"] = "إغلاق",
        ["Yes"] = "نعم",
        ["No"] = "لا",
        ["OK"] = "موافق",
        ["SAR"] = "ر.س",
        ["Hour"] = "ساعة",
        ["Day"] = "يوم",
        ["Month"] = "شهر",
    };

    // ═══════════════════════════════════════════════════════════════════
    // English Strings
    // ═══════════════════════════════════════════════════════════════════
    private static Dictionary<string, string> GetEnglishStrings() => new()
    {
        // App
        ["AppName"] = "Ashare",
        ["AppTagline"] = "Share Spaces, Share Possibilities",

        // Navigation
        ["Home"] = "Home",
        ["Explore"] = "Explore",
        ["Bookings"] = "Bookings",
        ["Favorites"] = "Favorites",
        ["Profile"] = "Profile",
        ["Search"] = "Search",
        ["Notifications"] = "Notifications",
        ["Messages"] = "Messages",

        // Auth
        ["Login"] = "Login",
        ["Logout"] = "Logout",
        ["Register"] = "Register",
        ["CreateAccount"] = "Create Account",
        ["WelcomeBack"] = "Welcome Back",
        ["LoginToYourAccount"] = "Login to continue",
        ["Email"] = "Email",
        ["Password"] = "Password",
        ["ConfirmPassword"] = "Confirm Password",
        ["FullName"] = "Full Name",
        ["Phone"] = "Phone Number",
        ["EnterEmail"] = "Enter your email",
        ["EnterPassword"] = "Enter your password",
        ["EnterFullName"] = "Enter your full name",
        ["ReEnterPassword"] = "Re-enter your password",
        ["ForgotPassword"] = "Forgot Password?",
        ["RememberMe"] = "Remember Me",
        ["DontHaveAccount"] = "Don't have an account?",
        ["AlreadyHaveAccount"] = "Already have an account?",
        ["IAccept"] = "I accept the",
        ["TermsAndConditions"] = "Terms & Conditions",
        ["And"] = "and",
        ["PrivacyPolicy"] = "Privacy Policy",
        ["Or"] = "Or",

        // Nafath
        ["LoginWithNafath"] = "Login with Nafath",
        ["LoginWithEmail"] = "Login with Email",
        ["ContinueWithNafath"] = "Continue with Nafath",
        ["ContinueAsGuest"] = "Continue as Guest",
        ["NationalId"] = "National ID",
        ["EnterNationalId"] = "Enter your National ID",
        ["SelectNumberInNafathApp"] = "Select this number in Nafath app",
        ["SecondsRemaining"] = "seconds remaining",
        ["LoginSuccessful"] = "Login successful",
        ["LoginRejected"] = "Login rejected",
        ["SessionExpired"] = "Session expired",
        ["TryAgain"] = "Try Again",

        // Authentication Required
        ["LoginRequired"] = "Login Required",
        ["LoginToCreateListing"] = "Please login to create a listing",

        // Spaces
        ["Spaces"] = "Spaces",
        ["AllSpaces"] = "All Spaces",
        ["FeaturedSpaces"] = "Featured Spaces",
        ["NewSpaces"] = "New Spaces",
        ["NearbySpaces"] = "Nearby",
        ["NoSpaces"] = "No spaces found",
        ["Featured"] = "Featured",
        ["New"] = "New",

        // Categories
        ["Categories"] = "Categories",
        ["MeetingRooms"] = "Meeting Rooms",
        ["CoWorking"] = "Co-Working",
        ["EventHalls"] = "Event Halls",
        ["Studios"] = "Studios",
        ["Commercial"] = "Commercial",
        ["CoLiving"] = "Co-Living",

        // Space Details
        ["Description"] = "Description",
        ["Amenities"] = "Amenities",
        ["Pricing"] = "Pricing",
        ["PerHour"] = "Per Hour",
        ["PerDay"] = "Per Day",
        ["PerMonth"] = "Per Month",
        ["Capacity"] = "Capacity",
        ["Area"] = "Area",
        ["Person"] = "person",
        ["SquareMeter"] = "m²",
        ["Rules"] = "Rules & Conditions",
        ["Owner"] = "Space Owner",
        ["MemberSince"] = "Member since",
        ["Contact"] = "Contact",
        ["Reviews"] = "Reviews",
        ["ViewAll"] = "View All",
        ["NoReviews"] = "No reviews yet",
        ["ViewMap"] = "View Map",

        // Booking
        ["BookNow"] = "Book Now",
        ["BookSpace"] = "Book Space",
        ["BookingType"] = "Booking Type",
        ["Hourly"] = "Hourly",
        ["Daily"] = "Daily",
        ["Monthly"] = "Monthly",
        ["Date"] = "Date",
        ["FromTime"] = "From",
        ["ToTime"] = "To",
        ["Guests"] = "Guests",
        ["Notes"] = "Notes",
        ["Optional"] = "Optional",
        ["Total"] = "Total",
        ["ConfirmBooking"] = "Confirm Booking",
        ["Cancel"] = "Cancel",

        // Bookings
        ["MyBookings"] = "My Bookings",
        ["Upcoming"] = "Upcoming",
        ["Past"] = "Past",
        ["Cancelled"] = "Cancelled",
        ["NoUpcomingBookings"] = "No upcoming bookings",
        ["NoPastBookings"] = "No past bookings",
        ["NoCancelledBookings"] = "No cancelled bookings",
        ["BookingPending"] = "Pending",
        ["BookingConfirmed"] = "Confirmed",
        ["BookingCompleted"] = "Completed",
        ["BookingCancelled"] = "Cancelled",
        ["CancelBooking"] = "Cancel Booking",
        ["ConfirmCancelBooking"] = "Are you sure you want to cancel this booking?",
        ["RefundInfo"] = "Refund will be processed within 3-5 business days",
        ["ConfirmCancel"] = "Confirm Cancellation",
        ["WriteReview"] = "Write a Review",
        ["RateYourExperience"] = "Rate your experience",
        ["YourComment"] = "Your comment",
        ["ShareExperience"] = "Share your experience with others...",
        ["SubmitReview"] = "Submit Review",

        // Favorites
        ["NoFavorites"] = "No favorites yet",
        ["AddFavoritesHint"] = "Add spaces you like to find them easily later",

        // Search
        ["SearchSpaces"] = "Search for a space...",
        ["RecentSearches"] = "Recent Searches",
        ["ClearAll"] = "Clear All",
        ["PopularCategories"] = "Popular Categories",
        ["PopularSearches"] = "Popular Searches",
        ["NearMe"] = "Near Me",
        ["LowestPrice"] = "Lowest Price",
        ["HighestRated"] = "Highest Rated",
        ["Results"] = "results",
        ["NoResults"] = "No results",
        ["NoResultsFor"] = "No spaces found matching",
        ["NewSearch"] = "New Search",
        ["Searching"] = "Searching...",

        // Filters
        ["Filters"] = "Filters",
        ["Apply"] = "Apply",
        ["Reset"] = "Reset",
        ["PriceRange"] = "Price Range",
        ["From"] = "From",
        ["To"] = "To",
        ["MinRating"] = "Minimum Rating",
        ["SortBy"] = "Sort By",
        ["Newest"] = "Newest",
        ["PriceLowToHigh"] = "Price: Low to High",
        ["PriceHighToLow"] = "Price: High to Low",
        ["Rating"] = "Rating",

        // Profile
        ["EditProfile"] = "Edit Profile",
        ["CompleteYourProfile"] = "Complete Your Profile",
        ["WelcomeToAshare"] = "Welcome to Ashare",
        ["PleaseCompleteYourProfile"] = "Please complete your profile to continue",
        ["Addresses"] = "My Addresses",
        ["MyAddresses"] = "My Addresses",
        ["PaymentMethods"] = "Payment Methods",
        ["Settings"] = "Settings",
        ["DarkMode"] = "Dark Mode",
        ["Language"] = "Language",
        ["PrivacySecurity"] = "Privacy & Security",
        ["Help"] = "Help",
        ["HelpCenter"] = "Help Center",
        ["ContactUs"] = "Contact Us",
        ["About"] = "About Ashare",
        ["Version"] = "Version",

        // Profile Edit
        ["PersonalInfo"] = "Personal Info",
        ["ContactInfo"] = "Contact Info",
        ["AddressInfo"] = "Address Info",
        ["AccountSettings"] = "Account Settings",
        ["FirstName"] = "First Name",
        ["LastName"] = "Last Name",
        ["DisplayName"] = "Display Name",
        ["EnterFirstName"] = "Enter your first name",
        ["EnterLastName"] = "Enter your last name",
        ["EnterDisplayName"] = "Enter display name",
        ["DisplayNameHint"] = "This name will be visible to others",
        ["PhoneNumber"] = "Phone Number",
        ["EnterAddress"] = "Enter your address",
        ["Address"] = "Address",
        ["ChangePhoto"] = "Change Photo",
        ["ChangePassword"] = "Change Password",
        ["Verified"] = "Verified",
        ["VerifyNow"] = "Verify Now",
        ["SaveChanges"] = "Save Changes",
        ["Continue"] = "Continue",
        ["ProfileUpdated"] = "Profile updated successfully",
        ["AvatarUploadFailed"] = "Failed to upload avatar",
        ["FirstNameRequired"] = "First name is required",
        ["FullNameRequired"] = "Full name is required",
        ["AuthenticationFailed"] = "Authentication failed",

        // Host
        ["Host"] = "Host",
        ["MySpaces"] = "My Spaces",
        ["AddNewSpace"] = "Add New Space",
        ["Earnings"] = "Earnings",

        // Chat
        ["Chat"] = "Chat",
        ["NoChats"] = "No chats yet",
        ["StartChat"] = "Start Chat",
        ["StartChatHint"] = "Start a conversation from any space page",
        ["TypeMessage"] = "Type a message...",
        ["Send"] = "Send",
        ["Online"] = "Online",
        ["Offline"] = "Offline",
        ["Typing"] = "Typing...",
        ["Now"] = "Now",
        ["NoMessagesYet"] = "No messages yet",
        ["SendFirstMessage"] = "Send a message to start the conversation",

        // Notifications
        ["AllNotifications"] = "All Notifications",
        ["NoNotifications"] = "No notifications",
        ["MarkAllRead"] = "Mark all as read",

        // Common
        ["Loading"] = "Loading...",
        ["Error"] = "Error",
        ["Success"] = "Success",
        ["Retry"] = "Retry",
        ["Save"] = "Save",
        ["Delete"] = "Delete",
        ["Edit"] = "Edit",
        ["Back"] = "Back",
        ["Next"] = "Next",
        ["Done"] = "Done",
        ["Close"] = "Close",
        ["Yes"] = "Yes",
        ["No"] = "No",
        ["OK"] = "OK",
        ["SAR"] = "SAR",
        ["Hour"] = "hour",
        ["Day"] = "day",
        ["Month"] = "month",
    };

    // ═══════════════════════════════════════════════════════════════════
    // Urdu Strings
    // ═══════════════════════════════════════════════════════════════════
    private static Dictionary<string, string> GetUrduStrings() => new()
    {
        // App
        ["AppName"] = "عشیر",
        ["AppTagline"] = "جگہیں شیئر کریں، امکانات شیئر کریں",

        // Navigation
        ["Home"] = "ہوم",
        ["Explore"] = "دریافت کریں",
        ["Bookings"] = "میری بکنگز",
        ["Favorites"] = "پسندیدہ",
        ["Profile"] = "پروفائل",
        ["Search"] = "تلاش",
        ["Notifications"] = "اطلاعات",
        ["Messages"] = "پیغامات",

        // Auth
        ["Login"] = "لاگ ان",
        ["Logout"] = "لاگ آؤٹ",
        ["Register"] = "رجسٹر",
        ["CreateAccount"] = "اکاؤنٹ بنائیں",
        ["WelcomeBack"] = "خوش آمدید",
        ["LoginToYourAccount"] = "جاری رکھنے کے لیے لاگ ان کریں",
        ["Email"] = "ای میل",
        ["Password"] = "پاس ورڈ",
        ["ConfirmPassword"] = "پاس ورڈ کی تصدیق",
        ["FullName"] = "پورا نام",
        ["Phone"] = "فون نمبر",
        ["EnterEmail"] = "اپنا ای میل درج کریں",
        ["EnterPassword"] = "اپنا پاس ورڈ درج کریں",
        ["EnterFullName"] = "اپنا پورا نام درج کریں",
        ["ReEnterPassword"] = "پاس ورڈ دوبارہ درج کریں",
        ["ForgotPassword"] = "پاس ورڈ بھول گئے؟",
        ["RememberMe"] = "مجھے یاد رکھیں",
        ["DontHaveAccount"] = "اکاؤنٹ نہیں ہے؟",
        ["AlreadyHaveAccount"] = "پہلے سے اکاؤنٹ ہے؟",
        ["Or"] = "یا",

        // Nafath
        ["LoginWithNafath"] = "نفاذ سے لاگ ان",
        ["LoginWithEmail"] = "ای میل سے لاگ ان",
        ["ContinueWithNafath"] = "نفاذ کے ساتھ جاری رکھیں",
        ["ContinueAsGuest"] = "مہمان کے طور پر جاری رکھیں",
        ["NationalId"] = "قومی شناختی نمبر",
        ["EnterNationalId"] = "اپنا قومی شناختی نمبر درج کریں",

        // Common
        ["Loading"] = "لوڈ ہو رہا ہے...",
        ["Error"] = "خرابی",
        ["Success"] = "کامیابی",
        ["Save"] = "محفوظ کریں",
        ["Cancel"] = "منسوخ",
        ["Delete"] = "حذف کریں",
        ["Edit"] = "ترمیم",
        ["Back"] = "واپس",
        ["Next"] = "اگلا",
        ["Done"] = "ہو گیا",
        ["Close"] = "بند کریں",
        ["Yes"] = "ہاں",
        ["No"] = "نہیں",
        ["OK"] = "ٹھیک ہے",
        ["SAR"] = "ریال",

        // Categories
        ["Categories"] = "زمرے",
        ["MeetingRooms"] = "میٹنگ رومز",
        ["CoWorking"] = "شیئرڈ آفس",
        ["EventHalls"] = "ایونٹ ہالز",
        ["Studios"] = "اسٹوڈیوز",
        ["Commercial"] = "کمرشل",
        ["CoLiving"] = "شیئرڈ رہائش",

        // Bookings
        ["BookNow"] = "ابھی بک کریں",
        ["MyBookings"] = "میری بکنگز",
        ["Upcoming"] = "آنے والی",
        ["Past"] = "گزشتہ",
        ["Cancelled"] = "منسوخ",

        // Profile
        ["Settings"] = "ترتیبات",
        ["DarkMode"] = "ڈارک موڈ",
        ["Language"] = "زبان",
        ["Help"] = "مدد",
        ["About"] = "عشیر کے بارے میں",
    };
}
