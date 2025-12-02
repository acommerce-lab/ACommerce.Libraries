using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Nafath;

/// <summary>
/// Client للمصادقة عبر نفاذ - يمكن استخدامه في أي تطبيق
/// </summary>
public sealed class NafathClient(IApiClient httpClient, string serviceName = "Marketplace")
{
    /// <summary>
    /// بدء مصادقة نفاذ
    /// </summary>
    /// <param name="nationalId">رقم الهوية الوطنية</param>
    public async Task<NafathInitiateResponse?> InitiateAsync(
        string nationalId,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<NafathInitiateRequest, NafathInitiateResponse>(
            serviceName,
            "/api/auth/nafath/initiate",
            new NafathInitiateRequest { NationalId = nationalId },
            cancellationToken);
    }

    /// <summary>
    /// التحقق من حالة المصادقة (polling)
    /// </summary>
    public async Task<NafathStatusResponse?> CheckStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<NafathStatusResponse>(
            serviceName,
            $"/api/auth/nafath/status?transactionId={transactionId}",
            cancellationToken);
    }

    /// <summary>
    /// إكمال المصادقة بعد نجاح التحقق في تطبيق نفاذ
    /// </summary>
    public async Task<NafathCompleteResponse?> CompleteAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<NafathCompleteRequest, NafathCompleteResponse>(
            serviceName,
            "/api/auth/nafath/complete",
            new NafathCompleteRequest { TransactionId = transactionId },
            cancellationToken);
    }
}
