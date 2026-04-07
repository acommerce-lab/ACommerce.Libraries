using ACommerce.Payments.Abstractions.Enums;
using ACommerce.Payments.Abstractions.Models;
using ACommerce.Templates.Customer.Services;

namespace HamtramckHardware.App.Services;

/// <summary>
/// Mock payment service for demo/testing purposes
/// </summary>
public class MockPaymentService : IPaymentService
{
    public bool IsAvailable => true;
    public string ProviderName => "Mock Payment";

    bool IPaymentService.CanOpenExternal => throw new NotImplementedException();

    bool IPaymentService.SupportsInAppPayment => throw new NotImplementedException();

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Simulate payment processing
        Console.WriteLine($"[MockPaymentService] Processing payment for ${request.Amount}");

        // Always succeed in mock mode
        return Task.FromResult(new PaymentResult
        {
            Status = PaymentStatus.Completed,
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

    Task<bool> IPaymentService.OpenPaymentPageAsync(string paymentUrl) => Task.FromResult(true);

    async Task<PaymentResult> IPaymentService.OpenPaymentInAppAsync(string paymentUrl, string callbackPattern)
        => await ProcessPaymentAsync(new()
        { 
            Amount = 1000,
            Currency = "USD",
            OrderId = "MOCK-ORDER-123",
            CustomerId = "MOCK-CUSTOMER-456",
            CallbackUrl = paymentUrl,
        });


}
