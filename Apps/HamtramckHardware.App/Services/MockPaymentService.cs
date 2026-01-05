using ACommerce.Templates.Customer.Services;

namespace HamtramckHardware.App.Services;

/// <summary>
/// Mock payment service for demo/testing purposes
/// </summary>
public class MockPaymentService : IPaymentService
{
    public bool IsAvailable => true;
    public string ProviderName => "Mock Payment";

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Simulate payment processing
        Console.WriteLine($"[MockPaymentService] Processing payment for ${request.Amount}");

        // Always succeed in mock mode
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = $"MOCK-{Guid.NewGuid():N}",
            Message = "Payment processed successfully (Mock)"
        });
    }

    public Task<bool> InitializeAsync()
    {
        Console.WriteLine("[MockPaymentService] Initialized");
        return Task.FromResult(true);
    }
}
