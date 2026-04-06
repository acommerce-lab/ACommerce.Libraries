using ACommerce.Notification.Operations.Abstractions;
using ACommerce.Notification.Operations.Operations;
using ACommerce.OperationEngine.Core;
using ACommerce.Realtime.Operations.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notification.Operations;

/// <summary>
/// تهيئة الإشعارات: تسجيل القنوات + تعريف الأنواع + واجهة بسيطة.
///
/// services.AddNotifications(config => {
///     config.AddChannel(new InAppChannel(transport));
///     config.AddChannel(new FirebaseChannel(apiKey));
///     config.DefineType("new_order", ["inapp", "push"], priority: "high");
///     config.DefineType("marketing", ["email"], priority: "low");
/// });
///
/// ثم:
///   await notifier.SendAsync("new_order", userId, new { orderId = 123 });
/// </summary>
public class NotificationConfig
{
    internal Dictionary<string, INotificationChannel> Channels { get; } = new();
    internal Dictionary<string, NotificationType> Types { get; } = new();

    public NotificationConfig AddChannel(INotificationChannel channel)
    {
        Channels[channel.ChannelName] = channel;
        return this;
    }

    public NotificationConfig DefineType(string typeName, string[] channels, string priority = "normal")
    {
        Types[typeName] = new NotificationType
        {
            Name = typeName,
            ChannelNames = channels,
            Priority = priority
        };
        return this;
    }
}

internal class NotificationType
{
    public string Name { get; set; } = default!;
    public string[] ChannelNames { get; set; } = Array.Empty<string>();
    public string Priority { get; set; } = "normal";
}

/// <summary>
/// واجهة المطور البسيطة.
///
///   await notifier.SendAsync("new_order", "user:123", new { orderId = 1 });
///   // ← يبني القيد + يختار القنوات + ينفذ عبر المحرك + يطلق Hooks
///
///   // أو مع hook مخصص:
///   await notifier.SendAsync("new_order", "user:123", data,
///       afterComplete: ctx => db.SaveNotificationAsync(...));
///
///   // أو للتخصيص الكامل: استخدم NotifyOps مباشرة من البنية التحتية
/// </summary>
public class Notifier
{
    private readonly NotificationConfig _config;
    private readonly OpEngine _engine;

    public Notifier(NotificationConfig config, OpEngine engine)
    {
        _config = config;
        _engine = engine;
    }

    /// <summary>
    /// إرسال إشعار بنوعه. القنوات تُحدد تلقائياً من التهيئة.
    /// </summary>
    public async Task<OperationResult> SendAsync(
        string typeName,
        string userId,
        object? data = null,
        string? title = null,
        string? message = null,
        Func<OperationContext, Task>? afterComplete = null,
        Func<OperationContext, Task>? afterFail = null,
        CancellationToken ct = default)
    {
        if (!_config.Types.TryGetValue(typeName, out var type))
            throw new ArgumentException($"Notification type '{typeName}' not defined. Use config.DefineType().");

        var channels = type.ChannelNames
            .Where(name => _config.Channels.ContainsKey(name))
            .Select(name => _config.Channels[name])
            .ToList();

        if (channels.Count == 0)
            throw new InvalidOperationException($"No channels available for type '{typeName}'. Registered: {string.Join(", ", _config.Channels.Keys)}");

        var op = NotifyOps.SendMultiChannel(
            userId,
            title ?? typeName,
            message ?? typeName,
            channels,
            notificationType: typeName,
            priority: type.Priority,
            extraData: data);

        if (afterComplete != null)
            op.Hooks.AfterComplete = afterComplete;
        if (afterFail != null)
            op.Hooks.AfterFail = afterFail;

        return await _engine.ExecuteAsync(op, ct);
    }

    /// <summary>
    /// إرسال إشعار مباشر (بدون نوع مُسبق) - لحالات خاصة
    /// </summary>
    public async Task<OperationResult> SendDirectAsync(
        string userId,
        string title,
        string message,
        string[] channelNames,
        string priority = "normal",
        object? data = null,
        CancellationToken ct = default)
    {
        var channels = channelNames
            .Where(name => _config.Channels.ContainsKey(name))
            .Select(name => _config.Channels[name])
            .ToList();

        var op = NotifyOps.SendMultiChannel(userId, title, message, channels,
            notificationType: "direct", priority: priority, extraData: data);

        return await _engine.ExecuteAsync(op, ct);
    }

    /// <summary>
    /// تأكيد قراءة إشعار
    /// </summary>
    public async Task<OperationResult> MarkReadAsync(string userId, Guid? originalOpId = null, CancellationToken ct = default)
    {
        var op = NotifyOps.MarkRead(userId, originalOpId);
        return await _engine.ExecuteAsync(op, ct);
    }
}

/// <summary>
/// تسجيل DI
/// </summary>
public static class NotificationExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services, Action<NotificationConfig> configure)
    {
        var config = new NotificationConfig();
        configure(config);
        services.AddSingleton(config);
        services.AddScoped<Notifier>();
        return services;
    }
}
