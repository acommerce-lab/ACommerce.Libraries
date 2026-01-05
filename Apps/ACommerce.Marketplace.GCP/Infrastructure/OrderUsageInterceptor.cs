using ACommerce.Marketplace.GCP.Services;
using ACommerce.Orders.Entities;
using MediatR;

namespace ACommerce.Marketplace.GCP.Infrastructure;

/// <summary>
/// MediatR behavior that intercepts order creation/confirmation
/// and reports usage to GCP Marketplace for billing.
///
/// THIS IS THE PRIMARY BILLING INTEGRATION POINT FOR ORDERS.
///
/// Integration with ACommerce.Orders:
/// - Intercepts CreateCommand<Order, CreateOrderDto>
/// - Reports OrdersCreated and TransactionValue metrics
///
/// To enable different billing models:
/// 1. Per-order: Just count orders
/// 2. Per-transaction-value: Report Total amount
/// 3. Per-item: Report number of items
/// 4. Hybrid: Report multiple metrics
/// </summary>
public class OrderUsageInterceptor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUsageReportingService _usageReporting;
    private readonly ILogger<OrderUsageInterceptor<TRequest, TResponse>> _logger;

    public OrderUsageInterceptor(
        IUsageReportingService usageReporting,
        ILogger<OrderUsageInterceptor<TRequest, TResponse>> logger)
    {
        _usageReporting = usageReporting;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Execute the request first
        var response = await next();

        // Check if this is an order creation that succeeded
        if (IsOrderCreation(request, response, out var order))
        {
            await ReportOrderUsageAsync(order);
        }

        return response;
    }

    private bool IsOrderCreation(TRequest request, TResponse response, out Order? order)
    {
        order = null;

        // Check if response is an Order entity
        if (response is Order createdOrder)
        {
            order = createdOrder;
            return true;
        }

        return false;
    }

    private async Task ReportOrderUsageAsync(Order order)
    {
        try
        {
            var metadata = new Dictionary<string, string>
            {
                ["order_id"] = order.Id.ToString(),
                ["order_number"] = order.OrderNumber,
                ["customer_id"] = order.CustomerId,
                ["currency"] = order.Currency
            };

            if (order.VendorId.HasValue)
            {
                metadata["vendor_id"] = order.VendorId.Value.ToString();
            }

            // Report order count
            await _usageReporting.RecordUsageAsync(
                UsageMetricType.OrdersCreated,
                1,
                metadata);

            // Report transaction value (in cents for precision)
            var valueInCents = (long)(order.Total * 100);
            await _usageReporting.RecordUsageAsync(
                UsageMetricType.TransactionValue,
                valueInCents,
                metadata);

            // Report items count
            if (order.Items?.Count > 0)
            {
                await _usageReporting.RecordUsageAsync(
                    "order_items",
                    order.Items.Count,
                    metadata);
            }

            _logger.LogInformation(
                "Reported order usage: OrderId={OrderId}, Total={Total} {Currency}",
                order.Id,
                order.Total,
                order.Currency);
        }
        catch (Exception ex)
        {
            // Don't fail orders due to usage reporting errors
            _logger.LogError(ex, "Failed to report order usage for {OrderId}", order.Id);
        }
    }
}

/// <summary>
/// Extension methods for registering order usage interceptor
/// </summary>
public static class OrderUsageInterceptorExtensions
{
    /// <summary>
    /// Adds order usage interceptor for GCP Marketplace billing
    /// </summary>
    public static IServiceCollection AddOrderUsageInterceptor(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(OrderUsageInterceptor<,>));
        return services;
    }
}
