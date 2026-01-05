using System.Net.Http.Json;
using System.Web;

namespace Ashare.App.Services;

/// <summary>
/// خدمة لالتقاط بيانات الإسناد التسويقي من الروابط العميقة
/// تحفظ البيانات محلياً وترسلها للخادم
/// </summary>
public class AttributionCaptureService : IAttributionCaptureService
{
    private readonly HttpClient _httpClient;
    private readonly MauiStorageService _storage;

    private const string SessionIdKey = "ashare_session_id";
    private const string AttributionCapturedKey = "ashare_attribution_captured";
    private const string LastClickIdKey = "ashare_last_click_id";

    public AttributionCaptureService(IHttpClientFactory httpClientFactory, MauiStorageService storage)
    {
        _httpClient = httpClientFactory.CreateClient("AshareApi");
        _storage = storage;
    }

    /// <summary>
    /// معرف الجلسة الحالي
    /// </summary>
    public string SessionId { get; private set; } = string.Empty;

    /// <summary>
    /// آخر Click ID تم التقاطه
    /// </summary>
    public string? LastClickId { get; private set; }

    /// <summary>
    /// تهيئة الخدمة وتحميل معرف الجلسة
    /// </summary>
    public async Task InitializeAsync()
    {
        SessionId = await _storage.GetAsync(SessionIdKey) ?? Guid.NewGuid().ToString("N");
        await _storage.SetAsync(SessionIdKey, SessionId);

        LastClickId = await _storage.GetAsync(LastClickIdKey);

        Console.WriteLine($"[Attribution] Initialized with SessionId: {SessionId}");
    }

    /// <summary>
    /// التقاط بيانات الإسناد من رابط عميق
    /// </summary>
    public async Task CaptureFromDeepLinkAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            Console.WriteLine($"[Attribution] Processing deep link: {url}");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return;

            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            // استخراج معاملات UTM
            var utmSource = queryParams["utm_source"];
            var utmMedium = queryParams["utm_medium"];
            var utmCampaign = queryParams["utm_campaign"];
            var utmContent = queryParams["utm_content"];
            var utmTerm = queryParams["utm_term"];

            // استخراج معرفات النقرات
            var fbclid = queryParams["fbclid"];
            var gclid = queryParams["gclid"];
            var ttclid = queryParams["ttclid"];
            var scClickId = queryParams["ScCid"] ?? queryParams["sc"];

            // تحديد Click ID الرئيسي
            var clickId = fbclid ?? gclid ?? ttclid ?? scClickId;

            // إذا لم يكن هناك أي معاملات تسويقية، لا حاجة للتتبع
            if (string.IsNullOrEmpty(utmSource) && string.IsNullOrEmpty(clickId))
            {
                Console.WriteLine("[Attribution] No attribution parameters found in URL");
                return;
            }

            // حفظ Click ID محلياً
            if (!string.IsNullOrEmpty(clickId))
            {
                LastClickId = clickId;
                await _storage.SetAsync(LastClickIdKey, clickId);
            }

            // إنشاء طلب الإسناد
            var request = new AttributionRequest
            {
                SessionId = SessionId,
                UtmSource = utmSource,
                UtmMedium = utmMedium,
                UtmCampaign = utmCampaign,
                UtmContent = utmContent,
                UtmTerm = utmTerm,
                ClickId = clickId,
                Fbclid = fbclid,
                Gclid = gclid,
                Ttclid = ttclid,
                ScClickId = scClickId,
                LandingPage = url,
                DeviceType = DeviceInfo.Platform.ToString(),
                DeviceModel = DeviceInfo.Model,
                OsVersion = DeviceInfo.VersionString
            };

            Console.WriteLine($"[Attribution] Sending: Source={utmSource}, Campaign={utmCampaign}, ClickId={clickId}");

            await SendAttributionAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Attribution] Error processing deep link: {ex.Message}");
        }
    }

    /// <summary>
    /// ربط معرف المستخدم بالإسناد بعد تسجيل الدخول
    /// </summary>
    public async Task AssociateUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(SessionId))
            return;

        try
        {
            Console.WriteLine($"[Attribution] Associating user {userId} with session {SessionId}");

            var response = await _httpClient.PostAsJsonAsync("/api/marketing/attribution/associate", new
            {
                sessionId = SessionId,
                userId
            });

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Attribution] User associated successfully");
            }
            else
            {
                Console.WriteLine($"[Attribution] Failed to associate user: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Attribution] Error associating user: {ex.Message}");
        }
    }

    private async Task SendAttributionAsync(AttributionRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/marketing/attribution", request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Attribution] Attribution captured successfully");
                await _storage.SetAsync(AttributionCapturedKey, DateTime.UtcNow.ToString("O"));
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Attribution] Server error: {response.StatusCode} - {content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Attribution] Failed to send attribution: {ex.Message}");
            // حفظ محلياً لإرسالها لاحقاً
            // TODO: Implement retry queue
        }
    }
}

/// <summary>
/// واجهة خدمة التقاط الإسناد
/// </summary>
public interface IAttributionCaptureService
{
    string SessionId { get; }
    string? LastClickId { get; }
    Task InitializeAsync();
    Task CaptureFromDeepLinkAsync(string url);
    Task AssociateUserAsync(string userId);
}

/// <summary>
/// طلب التقاط الإسناد
/// </summary>
public class AttributionRequest
{
    public string? SessionId { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    public string? ClickId { get; set; }
    public string? Fbclid { get; set; }
    public string? Gclid { get; set; }
    public string? Ttclid { get; set; }
    public string? ScClickId { get; set; }
    public string? LandingPage { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceModel { get; set; }
    public string? OsVersion { get; set; }
}
