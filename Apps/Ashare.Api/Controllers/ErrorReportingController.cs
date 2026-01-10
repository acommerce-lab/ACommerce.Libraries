using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Ashare.Api.Controllers;

/// <summary>
/// Controller لتلقي تقارير الأخطاء من التطبيقات وإرسالها بالبريد الإلكتروني
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ErrorReportingController : ControllerBase
{
    private readonly ILogger<ErrorReportingController> _logger;
    private readonly IConfiguration _configuration;

    public ErrorReportingController(
        ILogger<ErrorReportingController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// استقبال تقرير خطأ من التطبيق
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> ReportError([FromBody] ErrorReport report, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[ErrorReporting] Received error report - Source: {Source}, Operation: {Operation}, Platform: {Platform}",
            report.Source,
            report.Operation,
            report.Platform);

        // إرسال الإيميل في background
        _ = Task.Run(async () =>
        {
            try
            {
                await SendErrorEmailAsync(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ErrorReporting] Failed to send error email");
            }
        });

        return Ok(new { received = true, timestamp = DateTime.UtcNow });
    }

    private async Task SendErrorEmailAsync(ErrorReport report)
    {
        var host = _configuration["Email:Smtp:Host"];
        var port = int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        var from = _configuration["Email:Smtp:From"] ?? "noreply@ashare.sa";
        var to = _configuration["ErrorReporting:Email"] ?? "asadrahwan@gmail.com";

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("[ErrorReporting] SMTP not configured, skipping email");
            return;
        }

        var subject = $"[Ashare Error] {report.Source} - {report.Operation} on {report.Platform}";

        var body = $@"
<html>
<body dir='rtl' style='font-family: Arial, sans-serif;'>
<h2 style='color: #dc3545;'>تقرير خطأ من تطبيق أشارك</h2>

<table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
    <tr style='background: #f8f9fa;'>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>المصدر</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.Source}</td>
    </tr>
    <tr>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>العملية</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.Operation}</td>
    </tr>
    <tr style='background: #f8f9fa;'>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>المنصة</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.Platform}</td>
    </tr>
    <tr>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>إصدار التطبيق</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.AppVersion}</td>
    </tr>
    <tr style='background: #f8f9fa;'>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>إصدار النظام</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.OsVersion}</td>
    </tr>
    <tr>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>الجهاز</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.DeviceModel}</td>
    </tr>
    <tr style='background: #f8f9fa;'>
        <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>التوقيت</td>
        <td style='padding: 10px; border: 1px solid #dee2e6;'>{report.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</td>
    </tr>
</table>

<h3 style='color: #dc3545; margin-top: 20px;'>رسالة الخطأ</h3>
<pre style='background: #f8f9fa; padding: 15px; border-radius: 5px; overflow-x: auto; direction: ltr;'>{report.ErrorMessage}</pre>

<h3 style='color: #dc3545; margin-top: 20px;'>تفاصيل إضافية</h3>
<pre style='background: #f8f9fa; padding: 15px; border-radius: 5px; overflow-x: auto; direction: ltr;'>{JsonSerializer.Serialize(report.AdditionalData, new JsonSerializerOptions { WriteIndented = true })}</pre>

{(string.IsNullOrEmpty(report.StackTrace) ? "" : $@"
<h3 style='color: #dc3545; margin-top: 20px;'>Stack Trace</h3>
<pre style='background: #fff3cd; padding: 15px; border-radius: 5px; overflow-x: auto; font-size: 12px; direction: ltr;'>{report.StackTrace}</pre>
")}

<hr style='margin-top: 30px;'>
<p style='color: #6c757d; font-size: 12px;'>
تم إرسال هذا التقرير تلقائياً من تطبيق أشارك<br>
معرف التقرير: {report.ReportId}
</p>
</body>
</html>
";

        using var smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mail = new MailMessage(from, to, subject, body)
        {
            IsBodyHtml = true
        };

        await smtpClient.SendMailAsync(mail, CancellationToken.None);
        _logger.LogInformation("[ErrorReporting] Error email sent successfully");
    }
}

/// <summary>
/// نموذج تقرير الخطأ من التطبيق
/// </summary>
public class ErrorReport
{
    public string ReportId { get; set; } = Guid.NewGuid().ToString();
    public string Source { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
    public string? OsVersion { get; set; }
    public string? DeviceModel { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
