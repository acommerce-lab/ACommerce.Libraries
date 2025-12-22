using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Localization;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة الترجمة لـ Ashare مع دعم العربية والإنجليزية والأردية
/// ترث من BaseLocalizationService وتضيف الترجمات الخاصة بـ Ashare
/// </summary>
public class LocalizationService : BaseLocalizationService
{
    public LocalizationService(IStorageService storageService) : base(storageService)
    {
    }

    protected override void ConfigureLanguages(List<LanguageInfo> languages)
    {
        // Arabic first (default)
        languages.Add(new LanguageInfo { Code = "ar", NativeName = "العربية", EnglishName = "Arabic", IsRtl = true });
        languages.Add(new LanguageInfo { Code = "en", NativeName = "English", EnglishName = "English", IsRtl = false });
        languages.Add(new LanguageInfo { Code = "ur", NativeName = "اردو", EnglishName = "Urdu", IsRtl = true });
    }

    protected override void ConfigureTranslations(Dictionary<string, Dictionary<string, string>> translations)
    {
        // Add Arabic translations
        AddArabicTranslations(translations["ar"]);
        
        // Add English translations
        AddEnglishTranslations(translations["en"]);
        
        // Add Urdu translations
        if (!translations.ContainsKey("ur"))
            translations["ur"] = new Dictionary<string, string>();
        AddUrduTranslations(translations["ur"]);
    }

    private static void AddArabicTranslations(Dictionary<string, string> ar)
    {
        // App (Required)
        ar["AppName"] = "عشير";
        ar["AppTagline"] = "شارك المساحات، شارك الإمكانيات";

        // Bookings (Ashare-specific)
        ar["Bookings"] = "حجوزاتي";
        ar["MyBookings"] = "حجوزاتي";
        ar["Upcoming"] = "القادمة";
        ar["Past"] = "السابقة";
        ar["NoUpcomingBookings"] = "لا توجد حجوزات قادمة";
        ar["NoPastBookings"] = "لا توجد حجوزات سابقة";
        ar["NoCancelledBookings"] = "لا توجد حجوزات ملغاة";
        ar["BookingPending"] = "قيد المراجعة";
        ar["BookingConfirmed"] = "مؤكد";
        ar["BookingCompleted"] = "مكتمل";
        ar["BookingCancelled"] = "ملغي";
        ar["CancelBooking"] = "إلغاء الحجز";
        ar["ConfirmCancelBooking"] = "هل أنت متأكد من إلغاء هذا الحجز؟";
        ar["RefundInfo"] = "سيتم استرداد المبلغ خلال 3-5 أيام عمل";
        ar["ConfirmCancel"] = "تأكيد الإلغاء";
        ar["WriteReview"] = "قيّم تجربتك";
        ar["RateYourExperience"] = "قيّم تجربتك";
        ar["YourComment"] = "تعليقك";
        ar["ShareExperience"] = "شارك تجربتك مع الآخرين...";
        ar["SubmitReview"] = "إرسال التقييم";

        // Nafath (Saudi-specific)
        ar["LoginWithNafath"] = "الدخول بنفاذ";
        ar["LoginWithEmail"] = "الدخول بالبريد الإلكتروني";
        ar["ContinueWithNafath"] = "المتابعة بنفاذ";
        ar["NationalId"] = "رقم الهوية الوطنية";
        ar["EnterNationalId"] = "أدخل رقم الهوية";
        ar["SelectNumberInNafathApp"] = "اختر هذا الرقم في تطبيق نفاذ";
        ar["SecondsRemaining"] = "ثانية متبقية";
        ar["LoginSuccessful"] = "تم تسجيل الدخول بنجاح";
        ar["LoginRejected"] = "تم رفض تسجيل الدخول";

        // Auth (Ashare-specific overrides)
        ar["LoginToYourAccount"] = "سجل دخولك للمتابعة";
        ar["EnterEmail"] = "أدخل بريدك الإلكتروني";
        ar["EnterPassword"] = "أدخل كلمة المرور";
        ar["EnterFullName"] = "أدخل اسمك الكامل";
        ar["ReEnterPassword"] = "أعد إدخال كلمة المرور";
        ar["LoginRequired"] = "يجب تسجيل الدخول";
        ar["LoginToCreateListing"] = "يجب تسجيل الدخول لإنشاء عرض جديد";

        // Spaces (Ashare-specific)
        ar["Spaces"] = "المساحات";
        ar["AllSpaces"] = "كل المساحات";
        ar["FeaturedSpaces"] = "المساحات المميزة";
        ar["NewSpaces"] = "أحدث المساحات";
        ar["NearbySpaces"] = "القريبة منك";
        ar["NoSpaces"] = "لا توجد مساحات";

        // Categories (Ashare-specific)
        ar["MeetingRooms"] = "قاعات اجتماعات";
        ar["CoWorking"] = "مكاتب مشتركة";
        ar["EventHalls"] = "قاعات فعاليات";
        ar["Studios"] = "استوديوهات";
        ar["Commercial"] = "مساحات تجارية";
        ar["CoLiving"] = "سكن مشترك";

        // Space Details
        ar["Amenities"] = "المرافق والخدمات";
        ar["Pricing"] = "الأسعار";
        ar["PerHour"] = "بالساعة";
        ar["PerDay"] = "باليوم";
        ar["PerMonth"] = "بالشهر";
        ar["Capacity"] = "السعة";
        ar["Area"] = "المساحة";
        ar["Person"] = "شخص";
        ar["SquareMeter"] = "م²";
        ar["Rules"] = "القواعد والشروط";
        ar["Owner"] = "مالك المساحة";
        ar["MemberSince"] = "عضو منذ";
        ar["Contact"] = "تواصل";
        ar["NoReviews"] = "لا توجد تقييمات بعد";
        ar["ViewMap"] = "عرض الخريطة";

        // Booking
        ar["BookNow"] = "احجز الآن";
        ar["BookSpace"] = "احجز المساحة";
        ar["BookingType"] = "نوع الحجز";
        ar["Hourly"] = "بالساعة";
        ar["Daily"] = "باليوم";
        ar["Monthly"] = "بالشهر";
        ar["FromTime"] = "من الساعة";
        ar["ToTime"] = "إلى الساعة";
        ar["Guests"] = "عدد الأشخاص";
        ar["Notes"] = "ملاحظات";
        ar["ConfirmBooking"] = "تأكيد الحجز";

        // Favorites
        ar["NoFavorites"] = "لا توجد مساحات مفضلة";
        ar["AddFavoritesHint"] = "أضف المساحات التي تعجبك لتجدها بسهولة لاحقاً";

        // Search
        ar["SearchSpaces"] = "ابحث عن مساحة...";
        ar["RecentSearches"] = "عمليات البحث الأخيرة";
        ar["ClearAll"] = "مسح الكل";
        ar["PopularCategories"] = "الفئات الشائعة";
        ar["PopularSearches"] = "عمليات بحث شائعة";
        ar["NearMe"] = "قريب مني";
        ar["LowestPrice"] = "الأقل سعراً";
        ar["HighestRated"] = "الأعلى تقييماً";
        ar["NoResultsFor"] = "لم نجد أي مساحات تطابق";
        ar["NewSearch"] = "بحث جديد";
        ar["Searching"] = "جاري البحث...";

        // Filters
        ar["PriceRange"] = "نطاق السعر";
        ar["From"] = "من";
        ar["To"] = "إلى";
        ar["MinRating"] = "الحد الأدنى للتقييم";
        ar["Newest"] = "الأحدث";
        ar["PriceLowToHigh"] = "السعر: الأقل";
        ar["PriceHighToLow"] = "السعر: الأعلى";
        ar["Rating"] = "التقييم";

        // Profile (Ashare-specific overrides)
        ar["CompleteYourProfile"] = "أكمل بياناتك";
        ar["WelcomeToAshare"] = "مرحباً بك في عشير";
        ar["PleaseCompleteYourProfile"] = "الرجاء إكمال بياناتك للمتابعة";
        ar["Addresses"] = "عناويني";
        ar["MyAddresses"] = "عناويني";
        ar["PaymentMethods"] = "طرق الدفع";
        ar["PrivacySecurity"] = "الخصوصية والأمان";
        ar["HelpCenter"] = "مركز المساعدة";
        ar["ContactUs"] = "تواصل معنا";
        ar["About"] = "عن عشير";
        ar["LegalAndPolicies"] = "القانونية والسياسات";

        // Profile Edit
        ar["AddressInfo"] = "بيانات العنوان";
        ar["AccountSettings"] = "إعدادات الحساب";
        ar["FirstName"] = "الاسم الأول";
        ar["LastName"] = "اسم العائلة";
        ar["DisplayName"] = "الاسم المعروض";
        ar["EnterFirstName"] = "أدخل اسمك الأول";
        ar["EnterLastName"] = "أدخل اسم العائلة";
        ar["EnterDisplayName"] = "أدخل الاسم المعروض";
        ar["DisplayNameHint"] = "هذا الاسم سيظهر للآخرين";
        ar["PhoneNumber"] = "رقم الجوال";
        ar["EnterAddress"] = "أدخل عنوانك";
        ar["Address"] = "العنوان";
        ar["Verified"] = "موثق";
        ar["VerifyNow"] = "توثيق الآن";
        ar["ProfileUpdated"] = "تم تحديث البيانات بنجاح";
        ar["AvatarUploadFailed"] = "فشل رفع الصورة";
        ar["FirstNameRequired"] = "الاسم الأول مطلوب";
        ar["FullNameRequired"] = "الاسم الكامل مطلوب";
        ar["AuthenticationFailed"] = "فشلت المصادقة";

        // Host
        ar["Host"] = "المضيف";
        ar["MySpaces"] = "مساحاتي";
        ar["AddNewSpace"] = "أضف مساحة جديدة";
        ar["Earnings"] = "أرباحي";
        ar["MyEarnings"] = "أرباحي";
        ar["NoSpacesYet"] = "لا توجد مساحات بعد";
        ar["AddYourFirstSpace"] = "أضف مساحتك الأولى وابدأ في استقبال الحجوزات";
        ar["LoginToViewSpaces"] = "سجل دخولك لعرض مساحاتك";
        ar["ConfirmDelete"] = "تأكيد الحذف";
        ar["DeleteSpaceConfirmation"] = "هل أنت متأكد من حذف \"{0}\"؟ لا يمكن التراجع عن هذا الإجراء.";
        ar["NoLocation"] = "بدون موقع";
        ar["SAR"] = "ر.س";

        // Subscriptions
        ar["SubscriptionPlans"] = "باقات الاشتراك";
        ar["MySubscription"] = "اشتراكي";
        ar["CurrentPlan"] = "الباقة الحالية";
        ar["ChangePlan"] = "تغيير الباقة";
        ar["ViewPlans"] = "عرض الباقات";
        ar["UpgradePlan"] = "ترقية الباقة";
        ar["Subscribe"] = "اشترك";
        ar["Upgrade"] = "ترقية";
        ar["GetStarted"] = "ابدأ الآن";
        ar["Annual"] = "سنوي";
        ar["Save20Percent"] = "وفر 20%";
        ar["CompareAllFeatures"] = "مقارنة جميع المزايا";
        ar["DaysRemaining"] = "أيام متبقية";
        ar["Days"] = "يوم";
        ar["StartDate"] = "تاريخ البدء";
        ar["EndDate"] = "تاريخ الانتهاء";

        // Plan Features
        ar["UnlimitedListings"] = "عروض غير محدودة";
        ar["MaxListingsFormat"] = "حتى {0} عرض";
        ar["MaxImagesFormat"] = "حتى {0} صورة لكل عرض";
        ar["CommissionFormat"] = "عمولة {0}% لكل عملية";
        ar["UnlimitedStorage"] = "تخزين غير محدود";
        ar["StorageGBFormat"] = "{0} جيجابايت تخزين";
        ar["StorageMBFormat"] = "{0} ميجابايت تخزين";
        ar["VerifiedBadge"] = "شارة التوثيق";
        ar["NoAnalytics"] = "بدون تحليلات";
        ar["BasicAnalytics"] = "تحليلات أساسية";
        ar["AdvancedAnalytics"] = "تحليلات متقدمة";
        ar["FullAnalytics"] = "تحليلات شاملة";
        ar["BasicSupport"] = "دعم أساسي";
        ar["StandardSupport"] = "دعم قياسي";
        ar["PrioritySupport"] = "دعم بأولوية";
        ar["DedicatedSupport"] = "دعم مخصص";
        ar["ApiAccess"] = "الوصول للـ API";

        // Subscription Status
        ar["StatusActive"] = "نشط";
        ar["StatusTrial"] = "فترة تجريبية";
        ar["StatusPastDue"] = "متأخر السداد";
        ar["StatusCancelled"] = "ملغي";
        ar["StatusExpired"] = "منتهي";
        ar["StatusSuspended"] = "موقوف";
        ar["NoActiveSubscription"] = "لا يوجد اشتراك نشط";
        ar["SubscribeToStartHosting"] = "اشترك الآن لبدء عرض مساحاتك";
        ar["SubscriptionExpiringWarning"] = "اشتراكك على وشك الانتهاء";
        ar["RenewNow"] = "جدد الآن";

        // Usage
        ar["UsageStatistics"] = "إحصائيات الاستخدام";
        ar["Listings"] = "العروض";
        ar["Storage"] = "التخزين";
        ar["FeaturedListings"] = "العروض المميزة";
        ar["TeamMembers"] = "أعضاء الفريق";
        ar["CommissionRate"] = "نسبة العمولة";
        ar["PerTransaction"] = "لكل عملية";
        ar["CommissionNote"] = "يتم خصم العمولة تلقائياً من كل عملية بيع ناجحة";

        // Invoices
        ar["RecentInvoices"] = "آخر الفواتير";
        ar["Invoices"] = "الفواتير";
        ar["BillingSettings"] = "إعدادات الفوترة";
        ar["Paid"] = "مدفوعة";
        ar["Failed"] = "فشلت";
        ar["Refunded"] = "مستردة";
        ar["Draft"] = "مسودة";
        ar["Support"] = "الدعم";

        // Chat
        ar["Chat"] = "المحادثات";
        ar["NoChats"] = "لا توجد محادثات";
        ar["StartChat"] = "بدء محادثة";
        ar["StartChatHint"] = "ابدأ محادثة من صفحة أي مساحة";
        ar["TypeMessage"] = "اكتب رسالة...";
        ar["Send"] = "إرسال";
        ar["Online"] = "متصل";
        ar["Offline"] = "غير متصل";
        ar["Typing"] = "يكتب...";
        ar["NoMessagesYet"] = "لا توجد رسائل بعد";
        ar["SendFirstMessage"] = "أرسل أول رسالة لبدء المحادثة";

        // Notifications
        ar["AllNotifications"] = "كل الإشعارات";
        ar["NoNotifications"] = "لا توجد إشعارات";
        ar["MarkAllRead"] = "تحديد الكل كمقروء";

        // Payment
        ar["SubscribeNow"] = "اشترك الآن";
        ar["BillingCycle"] = "دورة الفوترة";
        ar["Quarterly"] = "ربع سنوي";
        ar["FreeTrial"] = "فترة تجريبية مجانية";
        ar["VAT"] = "ضريبة القيمة المضافة";
        ar["StartFreeTrial"] = "ابدأ الفترة التجريبية";
        ar["PayNow"] = "ادفع الآن";
        ar["PaymentSecureNotice"] = "ستتم معالجة الدفع بشكل آمن عبر بوابة الدفع";
        ar["PaymentInitError"] = "تعذر بدء عملية الدفع. يرجى المحاولة مرة أخرى.";
        ar["PaymentConnectionError"] = "حدث خطأ في الاتصال. تحقق من اتصالك بالإنترنت وحاول مرة أخرى.";
        ar["PlanNotFound"] = "الباقة غير موجودة";
        ar["PaymentFailedRetry"] = "فشلت عملية الدفع، يرجى المحاولة مرة أخرى";
        ar["PaymentFailedSelectPlan"] = "فشلت عملية الدفع، اختر باقة للمتابعة";

        // Reviews
        ar["Reviews"] = "التقييمات";

        // Common
        ar["Loading"] = "جاري التحميل...";
        ar["Login"] = "تسجيل الدخول";
        ar["Guest"] = "زائر";
        ar["RedirectingToLogin"] = "جاري التوجيه لتسجيل الدخول...";
        ar["LoginToAccessProfile"] = "سجل دخولك للوصول لملفك الشخصي";
        ar["Retry"] = "إعادة المحاولة";
        ar["PageNotFound"] = "الصفحة غير موجودة";
        ar["ErrorLoadingPage"] = "حدث خطأ أثناء تحميل الصفحة";

        // Version Management
        ar["VersionUnsupported"] = "إصدار غير مدعوم";
        ar["VersionUnsupportedMessage"] = "إصدارك الحالي غير مدعوم. يجب التحديث للاستمرار في استخدام التطبيق.";
        ar["VersionDeprecatedMessage"] = "هذا الإصدار سينتهي دعمه قريباً. يرجى التحديث.";
        ar["VersionUpdateAvailable"] = "يتوفر إصدار جديد من التطبيق. قم بالتحديث للحصول على أحدث الميزات.";
        ar["YourVersion"] = "إصدارك";
        ar["LatestVersion"] = "أحدث إصدار";
        ar["EndOfSupport"] = "تاريخ الإيقاف";
        ar["WhatsNew"] = "ما الجديد؟";
        ar["UpdateNow"] = "تحديث الآن";
        ar["OpenStore"] = "فتح المتجر";
        ar["NeedHelp"] = "تحتاج مساعدة؟";
        ar["Later"] = "لاحقاً";
    }

    private static void AddEnglishTranslations(Dictionary<string, string> en)
    {
        // App (Required)
        en["AppName"] = "Ashare";
        en["AppTagline"] = "Share Spaces, Share Possibilities";

        // Bookings (Ashare-specific)
        en["Bookings"] = "Bookings";
        en["MyBookings"] = "My Bookings";
        en["Upcoming"] = "Upcoming";
        en["Past"] = "Past";
        en["NoUpcomingBookings"] = "No upcoming bookings";
        en["NoPastBookings"] = "No past bookings";
        en["NoCancelledBookings"] = "No cancelled bookings";
        en["BookingPending"] = "Pending";
        en["BookingConfirmed"] = "Confirmed";
        en["BookingCompleted"] = "Completed";
        en["BookingCancelled"] = "Cancelled";
        en["CancelBooking"] = "Cancel Booking";
        en["ConfirmCancelBooking"] = "Are you sure you want to cancel this booking?";
        en["RefundInfo"] = "Refund will be processed within 3-5 business days";
        en["ConfirmCancel"] = "Confirm Cancellation";
        en["WriteReview"] = "Write a Review";
        en["RateYourExperience"] = "Rate your experience";
        en["YourComment"] = "Your comment";
        en["ShareExperience"] = "Share your experience with others...";
        en["SubmitReview"] = "Submit Review";

        // Nafath (Saudi-specific)
        en["LoginWithNafath"] = "Login with Nafath";
        en["LoginWithEmail"] = "Login with Email";
        en["ContinueWithNafath"] = "Continue with Nafath";
        en["NationalId"] = "National ID";
        en["EnterNationalId"] = "Enter your National ID";
        en["SelectNumberInNafathApp"] = "Select this number in Nafath app";
        en["SecondsRemaining"] = "seconds remaining";
        en["LoginSuccessful"] = "Login successful";
        en["LoginRejected"] = "Login rejected";

        // Auth (Ashare-specific overrides)
        en["LoginToYourAccount"] = "Login to continue";
        en["EnterEmail"] = "Enter your email";
        en["EnterPassword"] = "Enter your password";
        en["EnterFullName"] = "Enter your full name";
        en["ReEnterPassword"] = "Re-enter your password";
        en["LoginRequired"] = "Login Required";
        en["LoginToCreateListing"] = "Please login to create a listing";

        // Spaces (Ashare-specific)
        en["Spaces"] = "Spaces";
        en["AllSpaces"] = "All Spaces";
        en["FeaturedSpaces"] = "Featured Spaces";
        en["NewSpaces"] = "New Spaces";
        en["NearbySpaces"] = "Nearby";
        en["NoSpaces"] = "No spaces found";

        // Categories (Ashare-specific)
        en["MeetingRooms"] = "Meeting Rooms";
        en["CoWorking"] = "Co-Working";
        en["EventHalls"] = "Event Halls";
        en["Studios"] = "Studios";
        en["Commercial"] = "Commercial";
        en["CoLiving"] = "Co-Living";

        // Space Details
        en["Amenities"] = "Amenities";
        en["Pricing"] = "Pricing";
        en["PerHour"] = "Per Hour";
        en["PerDay"] = "Per Day";
        en["PerMonth"] = "Per Month";
        en["Capacity"] = "Capacity";
        en["Area"] = "Area";
        en["Person"] = "person";
        en["SquareMeter"] = "m²";
        en["Rules"] = "Rules & Conditions";
        en["Owner"] = "Space Owner";
        en["MemberSince"] = "Member since";
        en["Contact"] = "Contact";
        en["NoReviews"] = "No reviews yet";
        en["ViewMap"] = "View Map";

        // Booking
        en["BookNow"] = "Book Now";
        en["BookSpace"] = "Book Space";
        en["BookingType"] = "Booking Type";
        en["Hourly"] = "Hourly";
        en["Daily"] = "Daily";
        en["Monthly"] = "Monthly";
        en["FromTime"] = "From";
        en["ToTime"] = "To";
        en["Guests"] = "Guests";
        en["Notes"] = "Notes";
        en["ConfirmBooking"] = "Confirm Booking";

        // Favorites
        en["NoFavorites"] = "No favorites yet";
        en["AddFavoritesHint"] = "Add spaces you like to find them easily later";

        // Search
        en["SearchSpaces"] = "Search for a space...";
        en["RecentSearches"] = "Recent Searches";
        en["ClearAll"] = "Clear All";
        en["PopularCategories"] = "Popular Categories";
        en["PopularSearches"] = "Popular Searches";
        en["NearMe"] = "Near Me";
        en["LowestPrice"] = "Lowest Price";
        en["HighestRated"] = "Highest Rated";
        en["NoResultsFor"] = "No spaces found matching";
        en["NewSearch"] = "New Search";
        en["Searching"] = "Searching...";

        // Filters
        en["PriceRange"] = "Price Range";
        en["From"] = "From";
        en["To"] = "To";
        en["MinRating"] = "Minimum Rating";
        en["Newest"] = "Newest";
        en["PriceLowToHigh"] = "Price: Low to High";
        en["PriceHighToLow"] = "Price: High to Low";
        en["Rating"] = "Rating";

        // Profile (Ashare-specific overrides)
        en["CompleteYourProfile"] = "Complete Your Profile";
        en["WelcomeToAshare"] = "Welcome to Ashare";
        en["PleaseCompleteYourProfile"] = "Please complete your profile to continue";
        en["Addresses"] = "My Addresses";
        en["MyAddresses"] = "My Addresses";
        en["PaymentMethods"] = "Payment Methods";
        en["PrivacySecurity"] = "Privacy & Security";
        en["HelpCenter"] = "Help Center";
        en["ContactUs"] = "Contact Us";
        en["About"] = "About Ashare";
        en["LegalAndPolicies"] = "Legal & Policies";

        // Profile Edit
        en["AddressInfo"] = "Address Info";
        en["AccountSettings"] = "Account Settings";
        en["FirstName"] = "First Name";
        en["LastName"] = "Last Name";
        en["DisplayName"] = "Display Name";
        en["EnterFirstName"] = "Enter your first name";
        en["EnterLastName"] = "Enter your last name";
        en["EnterDisplayName"] = "Enter display name";
        en["DisplayNameHint"] = "This name will be visible to others";
        en["PhoneNumber"] = "Phone Number";
        en["EnterAddress"] = "Enter your address";
        en["Address"] = "Address";
        en["Verified"] = "Verified";
        en["VerifyNow"] = "Verify Now";
        en["ProfileUpdated"] = "Profile updated successfully";
        en["AvatarUploadFailed"] = "Failed to upload avatar";
        en["FirstNameRequired"] = "First name is required";
        en["FullNameRequired"] = "Full name is required";
        en["AuthenticationFailed"] = "Authentication failed";

        // Host
        en["Host"] = "Host";
        en["MySpaces"] = "My Spaces";
        en["AddNewSpace"] = "Add New Space";
        en["Earnings"] = "Earnings";
        en["MyEarnings"] = "My Earnings";
        en["NoSpacesYet"] = "No spaces yet";
        en["AddYourFirstSpace"] = "Add your first space and start receiving bookings";
        en["LoginToViewSpaces"] = "Login to view your spaces";
        en["ConfirmDelete"] = "Confirm Delete";
        en["DeleteSpaceConfirmation"] = "Are you sure you want to delete \"{0}\"? This action cannot be undone.";
        en["NoLocation"] = "No location";
        en["SAR"] = "SAR";

        // Subscriptions
        en["SubscriptionPlans"] = "Subscription Plans";
        en["MySubscription"] = "My Subscription";
        en["CurrentPlan"] = "Current Plan";
        en["ChangePlan"] = "Change Plan";
        en["ViewPlans"] = "View Plans";
        en["UpgradePlan"] = "Upgrade Plan";
        en["Subscribe"] = "Subscribe";
        en["Upgrade"] = "Upgrade";
        en["GetStarted"] = "Get Started";
        en["Annual"] = "Annual";
        en["Save20Percent"] = "Save 20%";
        en["CompareAllFeatures"] = "Compare All Features";
        en["DaysRemaining"] = "Days Remaining";
        en["Days"] = "days";
        en["StartDate"] = "Start Date";
        en["EndDate"] = "End Date";

        // Plan Features
        en["UnlimitedListings"] = "Unlimited Listings";
        en["MaxListingsFormat"] = "Up to {0} listings";
        en["MaxImagesFormat"] = "Up to {0} images per listing";
        en["CommissionFormat"] = "{0}% commission per sale";
        en["UnlimitedStorage"] = "Unlimited Storage";
        en["StorageGBFormat"] = "{0} GB Storage";
        en["StorageMBFormat"] = "{0} MB Storage";
        en["VerifiedBadge"] = "Verified Badge";
        en["NoAnalytics"] = "No Analytics";
        en["BasicAnalytics"] = "Basic Analytics";
        en["AdvancedAnalytics"] = "Advanced Analytics";
        en["FullAnalytics"] = "Full Analytics";
        en["BasicSupport"] = "Basic Support";
        en["StandardSupport"] = "Standard Support";
        en["PrioritySupport"] = "Priority Support";
        en["DedicatedSupport"] = "Dedicated Support";
        en["ApiAccess"] = "API Access";

        // Subscription Status
        en["StatusActive"] = "Active";
        en["StatusTrial"] = "Trial";
        en["StatusPastDue"] = "Past Due";
        en["StatusCancelled"] = "Cancelled";
        en["StatusExpired"] = "Expired";
        en["StatusSuspended"] = "Suspended";
        en["NoActiveSubscription"] = "No Active Subscription";
        en["SubscribeToStartHosting"] = "Subscribe to start listing your spaces";
        en["SubscriptionExpiringWarning"] = "Your subscription is about to expire";
        en["RenewNow"] = "Renew Now";

        // Usage
        en["UsageStatistics"] = "Usage Statistics";
        en["Listings"] = "Listings";
        en["Storage"] = "Storage";
        en["FeaturedListings"] = "Featured Listings";
        en["TeamMembers"] = "Team Members";
        en["CommissionRate"] = "Commission Rate";
        en["PerTransaction"] = "per transaction";
        en["CommissionNote"] = "Commission is automatically deducted from each successful sale";

        // Invoices
        en["RecentInvoices"] = "Recent Invoices";
        en["Invoices"] = "Invoices";
        en["BillingSettings"] = "Billing Settings";
        en["Paid"] = "Paid";
        en["Failed"] = "Failed";
        en["Refunded"] = "Refunded";
        en["Draft"] = "Draft";
        en["Support"] = "Support";

        // Chat
        en["Chat"] = "Chat";
        en["NoChats"] = "No chats yet";
        en["StartChat"] = "Start Chat";
        en["StartChatHint"] = "Start a conversation from any space page";
        en["TypeMessage"] = "Type a message...";
        en["Send"] = "Send";
        en["Online"] = "Online";
        en["Offline"] = "Offline";
        en["Typing"] = "Typing...";
        en["NoMessagesYet"] = "No messages yet";
        en["SendFirstMessage"] = "Send a message to start the conversation";

        // Notifications
        en["AllNotifications"] = "All Notifications";
        en["NoNotifications"] = "No notifications";
        en["MarkAllRead"] = "Mark all as read";

        // Payment
        en["SubscribeNow"] = "Subscribe Now";
        en["BillingCycle"] = "Billing Cycle";
        en["Quarterly"] = "Quarterly";
        en["FreeTrial"] = "Free Trial";
        en["VAT"] = "VAT";
        en["StartFreeTrial"] = "Start Free Trial";
        en["PayNow"] = "Pay Now";
        en["PaymentSecureNotice"] = "Your payment will be securely processed through the payment gateway";
        en["PaymentInitError"] = "Unable to initialize payment. Please try again.";
        en["PaymentConnectionError"] = "Connection error. Please check your internet and try again.";
        en["PlanNotFound"] = "Plan not found";
        en["PaymentFailedRetry"] = "Payment failed, please try again";
        en["PaymentFailedSelectPlan"] = "Payment failed, select a plan to continue";

        // Reviews
        en["Reviews"] = "Reviews";

        // Common
        en["Loading"] = "Loading...";
        en["Login"] = "Login";
        en["Guest"] = "Guest";
        en["RedirectingToLogin"] = "Redirecting to login...";
        en["LoginToAccessProfile"] = "Login to access your profile";
        en["Retry"] = "Retry";
        en["PageNotFound"] = "Page not found";
        en["ErrorLoadingPage"] = "Error loading page";

        // Version Management
        en["VersionUnsupported"] = "Unsupported Version";
        en["VersionUnsupportedMessage"] = "Your current version is no longer supported. Please update to continue using the app.";
        en["VersionDeprecatedMessage"] = "This version will be deprecated soon. Please update.";
        en["VersionUpdateAvailable"] = "A new version is available. Update to get the latest features.";
        en["YourVersion"] = "Your version";
        en["LatestVersion"] = "Latest version";
        en["EndOfSupport"] = "End of support";
        en["WhatsNew"] = "What's New?";
        en["UpdateNow"] = "Update Now";
        en["OpenStore"] = "Open Store";
        en["NeedHelp"] = "Need help?";
        en["Later"] = "Later";
    }

    private static void AddUrduTranslations(Dictionary<string, string> ur)
    {
        // App (Required)
        ur["AppName"] = "عشیر";
        ur["AppTagline"] = "جگہیں شیئر کریں، امکانات شیئر کریں";

        // Navigation
        ur["Home"] = "ہوم";
        ur["Explore"] = "دریافت کریں";
        ur["Bookings"] = "میری بکنگز";
        ur["Favorites"] = "پسندیدہ";
        ur["Profile"] = "پروفائل";
        ur["Search"] = "تلاش";
        ur["Notifications"] = "اطلاعات";
        ur["Messages"] = "پیغامات";

        // Auth
        ur["Login"] = "لاگ ان";
        ur["Logout"] = "لاگ آؤٹ";
        ur["Register"] = "رجسٹر";
        ur["CreateAccount"] = "اکاؤنٹ بنائیں";
        ur["WelcomeBack"] = "خوش آمدید";
        ur["LoginToYourAccount"] = "جاری رکھنے کے لیے لاگ ان کریں";
        ur["Email"] = "ای میل";
        ur["Password"] = "پاس ورڈ";
        ur["ConfirmPassword"] = "پاس ورڈ کی تصدیق";
        ur["FullName"] = "پورا نام";
        ur["Phone"] = "فون نمبر";
        ur["EnterEmail"] = "اپنا ای میل درج کریں";
        ur["EnterPassword"] = "اپنا پاس ورڈ درج کریں";
        ur["EnterFullName"] = "اپنا پورا نام درج کریں";
        ur["ReEnterPassword"] = "پاس ورڈ دوبارہ درج کریں";
        ur["ForgotPassword"] = "پاس ورڈ بھول گئے؟";
        ur["RememberMe"] = "مجھے یاد رکھیں";
        ur["DontHaveAccount"] = "اکاؤنٹ نہیں ہے؟";
        ur["AlreadyHaveAccount"] = "پہلے سے اکاؤنٹ ہے؟";
        ur["Or"] = "یا";

        // Nafath
        ur["LoginWithNafath"] = "نفاذ سے لاگ ان";
        ur["LoginWithEmail"] = "ای میل سے لاگ ان";
        ur["ContinueWithNafath"] = "نفاذ کے ساتھ جاری رکھیں";
        ur["ContinueAsGuest"] = "مہمان کے طور پر جاری رکھیں";
        ur["NationalId"] = "قومی شناختی نمبر";
        ur["EnterNationalId"] = "اپنا قومی شناختی نمبر درج کریں";

        // Common
        ur["Loading"] = "لوڈ ہو رہا ہے...";
        ur["Error"] = "خرابی";
        ur["Success"] = "کامیابی";
        ur["Save"] = "محفوظ کریں";
        ur["Cancel"] = "منسوخ";
        ur["Delete"] = "حذف کریں";
        ur["Edit"] = "ترمیم";
        ur["Back"] = "واپس";
        ur["Next"] = "اگلا";
        ur["Done"] = "ہو گیا";
        ur["Close"] = "بند کریں";
        ur["Yes"] = "ہاں";
        ur["No"] = "نہیں";
        ur["OK"] = "ٹھیک ہے";
        ur["SAR"] = "ریال";
        ur["Retry"] = "دوبارہ کوشش کریں";
        ur["PageNotFound"] = "صفحہ نہیں ملا";
        ur["ErrorLoadingPage"] = "صفحہ لوڈ کرنے میں خرابی";

        // Categories
        ur["Categories"] = "زمرے";
        ur["MeetingRooms"] = "میٹنگ رومز";
        ur["CoWorking"] = "شیئرڈ آفس";
        ur["EventHalls"] = "ایونٹ ہالز";
        ur["Studios"] = "اسٹوڈیوز";
        ur["Commercial"] = "کمرشل";
        ur["CoLiving"] = "شیئرڈ رہائش";

        // Bookings
        ur["BookNow"] = "ابھی بک کریں";
        ur["MyBookings"] = "میری بکنگز";
        ur["Upcoming"] = "آنے والی";
        ur["Past"] = "گزشتہ";
        ur["Cancelled"] = "منسوخ";

        // Profile
        ur["Settings"] = "ترتیبات";
        ur["DarkMode"] = "ڈارک موڈ";
        ur["Language"] = "زبان";
        ur["Help"] = "مدد";
        ur["About"] = "عشیر کے بارے میں";

        // Host
        ur["Host"] = "میزبان";
        ur["MySpaces"] = "میری جگہیں";
        ur["AddNewSpace"] = "نئی جگہ شامل کریں";
        ur["NoSpacesYet"] = "ابھی تک کوئی جگہ نہیں";
        ur["AddYourFirstSpace"] = "اپنی پہلی جگہ شامل کریں اور بکنگ حاصل کرنا شروع کریں";
        ur["LoginToViewSpaces"] = "اپنی جگہیں دیکھنے کے لیے لاگ ان کریں";
        ur["ConfirmDelete"] = "حذف کی تصدیق";
        ur["DeleteSpaceConfirmation"] = "کیا آپ واقعی \"{0}\" کو حذف کرنا چاہتے ہیں؟ یہ عمل واپس نہیں ہو سکتا۔";
        ur["Active"] = "فعال";
        ur["Inactive"] = "غیر فعال";
        ur["NoLocation"] = "کوئی مقام نہیں";
        ur["Month"] = "ماہ";
        ur["LoginRequired"] = "لاگ ان درکار ہے";

        // Payment
        ur["PaymentFailedRetry"] = "ادائیگی ناکام ہوگئی، براہ کرم دوبارہ کوشش کریں";
        ur["PaymentFailedSelectPlan"] = "ادائیگی ناکام ہوگئی، جاری رکھنے کے لیے پلان منتخب کریں";

        // Version Management
        ur["VersionUnsupported"] = "غیر معاون ورژن";
        ur["VersionUnsupportedMessage"] = "آپ کا موجودہ ورژن اب معاون نہیں ہے۔ ایپ استعمال جاری رکھنے کے لیے براہ کرم اپ ڈیٹ کریں۔";
        ur["VersionDeprecatedMessage"] = "یہ ورژن جلد ختم ہو جائے گا۔ براہ کرم اپ ڈیٹ کریں۔";
        ur["VersionUpdateAvailable"] = "ایک نیا ورژن دستیاب ہے۔ تازہ ترین خصوصیات حاصل کرنے کے لیے اپ ڈیٹ کریں۔";
        ur["YourVersion"] = "آپ کا ورژن";
        ur["LatestVersion"] = "تازہ ترین ورژن";
        ur["EndOfSupport"] = "سپورٹ کا اختتام";
        ur["WhatsNew"] = "نیا کیا ہے؟";
        ur["UpdateNow"] = "ابھی اپ ڈیٹ کریں";
        ur["OpenStore"] = "اسٹور کھولیں";
        ur["NeedHelp"] = "مدد چاہیے؟";
        ur["Later"] = "بعد میں";
    }
}
