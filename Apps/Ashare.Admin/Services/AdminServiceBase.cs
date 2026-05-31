using System.Net.Http.Headers;

namespace Ashare.Admin.Services;

/// <summary>
/// أساس مشترك لخدمات الإدارة — يوفّر HttpClient مُصادَقاً يشير إلى Ashare API.
/// </summary>
public abstract class AdminServiceBase
{
    protected readonly HttpClient Http;
    private readonly AdminAuthStateProvider _auth;

    protected AdminServiceBase(IConfiguration configuration, AdminAuthStateProvider auth)
    {
        _auth = auth;
        var baseUrl = configuration["ApiSettings:BaseUrl"]
            ?? "https://ashare-api-130415035604.me-central2.run.app";
        Http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(30) };
    }

    /// <summary>تعيين ترويسة Bearer من التوكن الحالي قبل أي طلب.</summary>
    protected async Task AuthAsync()
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
