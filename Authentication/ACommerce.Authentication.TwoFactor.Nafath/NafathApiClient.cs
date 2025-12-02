using ACommerce.Authentication.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ACommerce.Authentication.TwoFactor.Nafath;

/// <summary>
/// Default implementation of Nafath API client
/// Supports both Test and Production modes
/// </summary>
public class NafathApiClient : INafathApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NafathApiClient> _logger;

    public NafathApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NafathApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Set base URL from configuration (using correct section path)
        var baseUrl = _configuration[$"{NafathOptions.SectionName}:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<NafathInitiationResponse> InitiateAuthenticationAsync(
        string nationalId,
        CancellationToken cancellationToken = default)
    {
        var mode = _configuration[$"{NafathOptions.SectionName}:Mode"]?.ToLower();
        var isTestMode = mode == "test";

        // ✅ Test Mode
        if (isTestMode)
        {
            return await HandleTestModeInitiation(nationalId, cancellationToken);
        }

        // ✅ Production Mode
        return await HandleProductionModeInitiation(nationalId, cancellationToken);
    }

    public async Task<NafathStatusResponse> CheckStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var mode = _configuration[$"{NafathOptions.SectionName}:Mode"]?.ToLower();
        var isTestMode = mode == "test";

        // ✅ Test Mode
        if (isTestMode)
        {
            _logger.LogInformation(
                "[Nafath] Test mode: Checking status for {TransactionId}",
                transactionId);

            // في test mode، نفترض أن الـ status دائماً pending
            // الـ webhook هو من سيحدث الحالة
            return new NafathStatusResponse
            {
                IsCompleted = false,
                Status = "PENDING"
            };
        }

        // ✅ Production Mode
        try
        {
            _logger.LogInformation(
                "[Nafath] Checking status for TransactionId: {TransactionId}",
                transactionId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"check-status/{transactionId}");
            request.Headers.Add("X-Authorization", _configuration[$"{NafathOptions.SectionName}:WebhookSecret"]);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[Nafath] Status check failed: {StatusCode}",
                    response.StatusCode);

                return new NafathStatusResponse
                {
                    IsCompleted = false,
                    Status = "ERROR"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<NafathStatusApiResponse>(
                cancellationToken: cancellationToken);

            return new NafathStatusResponse
            {
                IsCompleted = result?.Status == "COMPLETED",
                Status = result?.Status ?? "UNKNOWN"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Nafath] Failed to check status for TransactionId: {TransactionId}",
                transactionId);

            return new NafathStatusResponse
            {
                IsCompleted = false,
                Status = "ERROR"
            };
        }
    }

    private async Task<NafathInitiationResponse> HandleTestModeInitiation(
        string nationalId,
        CancellationToken cancellationToken)
    {
        // التحقق من رقم الهوية المسجل في الإعدادات
        var testNationalId = _configuration[$"{NafathOptions.SectionName}:TestNationalId"] ?? "2507643761";

        if (nationalId != testNationalId)
        {
            _logger.LogWarning(
                "[Nafath] Test mode: Invalid national ID {NationalId}, expected {TestNationalId}",
                nationalId, testNationalId);

            return new NafathInitiationResponse
            {
                Success = false,
                Error = new TwoFactorError
                {
                    Code = "INVALID_TEST_ID",
                    Message = "رقم الهوية غير صالح في وضع الاختبار",
                    Details = $"في وضع الاختبار، يجب استخدام رقم الهوية: {testNationalId}"
                }
            };
        }

        _logger.LogInformation(
            "[Nafath] Test mode: Creating fake transaction for {NationalId}",
            nationalId);

        // Simulate API delay
        await Task.Delay(100, cancellationToken);

        var transactionId = Guid.NewGuid().ToString();
        var verificationCode = "00"; // Nafath Test Code

        return new NafathInitiationResponse
        {
            Success = true,
            TransactionId = transactionId,
            VerificationCode = verificationCode,
            Identifier = nationalId,
            IsTestSession = true // ✅ علامة أنها جلسة اختبار
        };
    }

    private async Task<NafathInitiationResponse> HandleProductionModeInitiation(
        string nationalId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[Nafath] Initiating authentication for NationalId: {NationalId}",
                nationalId);

            var request = new HttpRequestMessage(HttpMethod.Post, "verify-by-nafath")
            {
                Content = JsonContent.Create(new { national_id = nationalId })
            };

            request.Headers.Add("X-Authorization", _configuration[$"{NafathOptions.SectionName}:WebhookSecret"]);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "[Nafath] API failed: {StatusCode}, {Content}",
                    response.StatusCode,
                    errorContent);

                return new NafathInitiationResponse
                {
                    Success = false,
                    Error = new TwoFactorError
                    {
                        Code = "NAFATH_API_ERROR",
                        Message = $"Nafath API returned {response.StatusCode}",
                        Details = errorContent
                    }
                };
            }

            var result = await response.Content.ReadFromJsonAsync<NafathApiWrapper>(
                cancellationToken: cancellationToken);

            if (result?.Data == null)
            {
                return new NafathInitiationResponse
                {
                    Success = false,
                    Error = new TwoFactorError
                    {
                        Code = "INVALID_RESPONSE",
                        Message = "Nafath API returned invalid response",
                        Details = "Response data is null"
                    }
                };
            }

            return new NafathInitiationResponse
            {
                Success = true,
                TransactionId = result.Data.TransactionId,
                VerificationCode = result.Data.Code,
                Identifier = nationalId,
                IsTestSession = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Nafath] Failed to initiate authentication for NationalId: {NationalId}",
                nationalId);

            return new NafathInitiationResponse
            {
                Success = false,
                Error = new TwoFactorError
                {
                    Code = "INITIATION_FAILED",
                    Message = ex.Message,
                    Details = ex.StackTrace
                }
            };
        }
    }

    // DTOs for Nafath API
    private record NafathApiWrapper
    {
        public NafathApiData? Data { get; init; }
    }

    private record NafathApiData
    {
        public required string TransactionId { get; init; }
        public string? Code { get; init; }
    }

    private record NafathStatusApiResponse
    {
        public string? Status { get; init; }
    }
}