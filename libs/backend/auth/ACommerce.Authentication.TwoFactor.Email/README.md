# ACommerce.Notifications.Channels.Email

Email notification channel with support for SMTP, SendGrid, and AWS SES.

## Overview

Complete email notification implementation supporting multiple providers, HTML templates, attachments, and batch sending. Integrates seamlessly with ACommerce.Notifications.Abstractions.

## Key Features

✅ **Multiple Providers** - SMTP, SendGrid, AWS SES  
✅ **HTML Templates** - Razor/Liquid template support  
✅ **Attachments** - File attachments support  
✅ **Batch Sending** - Send to multiple recipients  
✅ **Queue Support** - Background processing  
✅ **Retry Logic** - Automatic retry on failure  

## Configuration

### appsettings.json
```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "noreply@ACommerce.sa",
    "FromName": "ACommerce",
    "ReplyToEmail": "support@ACommerce.sa",
    
    "SMTP": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password",
      "EnableSsl": true,
      "Timeout": 30000
    },
    
    "SendGrid": {
      "ApiKey": "your-sendgrid-api-key"
    },
    
    "AWS_SES": {
      "AccessKey": "your-aws-access-key",
      "SecretKey": "your-aws-secret-key",
      "Region": "us-east-1"
    },
    
    "Templates": {
      "Engine": "Razor",
      "TemplatesPath": "EmailTemplates"
    },
    
    "Queue": {
      "Enabled": true,
      "MaxRetries": 3,
      "RetryDelaySeconds": 60
    }
  }
}
```

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Email Channel
builder.Services.AddEmailNotifications(builder.Configuration);

// Or manual setup
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<INotificationChannel, EmailNotificationChannel>();
builder.Services.AddScoped<IEmailProvider, SmtpEmailProvider>(); // or SendGridEmailProvider, SesEmailProvider
builder.Services.AddScoped<IEmailTemplateEngine, RazorTemplateEngine>();
```

## Usage

### Simple Email
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "Welcome to ACommerce",
    Message = "Thank you for joining us!",
    Channels = new List<NotificationChannel> { NotificationChannel.Email },
    Data = new Dictionary<string, string>
    {
        ["Email"] = "user@example.com"
    }
};

var result = await _notificationService.SendAsync(notification);
```

### Template Email
```csharp
var notification = new Notification
{
    UserId = "user-123",
    TemplateCode = "welcome_email",
    Channels = new List<NotificationChannel> { NotificationChannel.Email },
    Data = new Dictionary<string, string>
    {
        ["Email"] = "user@example.com",
        ["UserName"] = "Ahmed",
        ["ActivationLink"] = "https://ACommerce.sa/activate/abc123"
    }
};

var result = await _notificationService.SendAsync(notification);
```

### Email with Attachments
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "Invoice #1234",
    Message = "Please find your invoice attached",
    Channels = new List<NotificationChannel> { NotificationChannel.Email },
    Data = new Dictionary<string, string>
    {
        ["Email"] = "user@example.com",
        ["Attachments"] = JsonSerializer.Serialize(new[]
        {
            new { FileName = "invoice.pdf", FilePath = "/invoices/1234.pdf" }
        })
    }
};

var result = await _notificationService.SendAsync(notification);
```

### Batch Email
```csharp
var notifications = new List<Notification>
{
    new Notification
    {
        UserId = "user-1",
        Title = "Newsletter",
        Message = "Check out our latest updates",
        Data = new Dictionary<string, string> { ["Email"] = "user1@example.com" }
    },
    new Notification
    {
        UserId = "user-2",
        Title = "Newsletter",
        Message = "Check out our latest updates",
        Data = new Dictionary<string, string> { ["Email"] = "user2@example.com" }
    }
};

var results = await _notificationService.SendBatchAsync(notifications);
```

## Email Templates

### Razor Template (welcome_email.cshtml)
```html
@model WelcomeEmailModel

<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            padding: 20px;
        }
        .container {
            background-color: white;
            padding: 30px;
            border-radius: 10px;
            max-width: 600px;
            margin: 0 auto;
        }
        .button {
            background-color: #007bff;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            display: inline-block;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>مرحباً @Model.UserName!</h1>
        <p>نشكرك على الانضمام إلى عشير</p>
        <p>للبدء، يرجى تفعيل حسابك:</p>
        <a href="@Model.ActivationLink" class="button">تفعيل الحساب</a>
        <p style="color: #666; margin-top: 30px; font-size: 12px;">
            إذا لم تقم بإنشاء هذا الحساب، يرجى تجاهل هذا البريد.
        </p>
    </div>
</body>
</html>
```

### Template Model
```csharp
public class WelcomeEmailModel
{
    public string UserName { get; set; }
    public string ActivationLink { get; set; }
}
```

### Liquid Template (welcome_email.liquid)
```html
<!DOCTYPE html>
<html>
<body>
    <h1>مرحباً {{ UserName }}!</h1>
    <p>نشكرك على الانضمام إلى عشير</p>
    <a href="{{ ActivationLink }}">تفعيل الحساب</a>
</body>
</html>
```

## Provider Implementations

### SMTP Provider
```csharp
public class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _settings;
    
    public async Task<EmailResult> SendAsync(EmailMessage message)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            Timeout = _settings.Timeout
        };
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(message.FromEmail, message.FromName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };
        
        foreach (var to in message.ToEmails)
        {
            mailMessage.To.Add(to);
        }
        
        if (message.CcEmails?.Any() == true)
        {
            foreach (var cc in message.CcEmails)
            {
                mailMessage.CC.Add(cc);
            }
        }
        
        if (message.Attachments?.Any() == true)
        {
            foreach (var attachment in message.Attachments)
            {
                mailMessage.Attachments.Add(new Attachment(attachment.FilePath));
            }
        }
        
        try
        {
            await client.SendMailAsync(mailMessage);
            
            return new EmailResult
            {
                IsSuccess = true,
                MessageId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new EmailResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}
```

### SendGrid Provider
```csharp
public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridSettings _settings;
    private readonly SendGridClient _client;
    
    public SendGridEmailProvider(IOptions<SendGridSettings> settings)
    {
        _settings = settings.Value;
        _client = new SendGridClient(_settings.ApiKey);
    }
    
    public async Task<EmailResult> SendAsync(EmailMessage message)
    {
        var msg = new SendGridMessage
        {
            From = new EmailAddress(message.FromEmail, message.FromName),
            Subject = message.Subject,
            PlainTextContent = message.IsHtml ? null : message.Body,
            HtmlContent = message.IsHtml ? message.Body : null
        };
        
        msg.AddTos(message.ToEmails.Select(e => new EmailAddress(e)).ToList());
        
        if (message.CcEmails?.Any() == true)
        {
            msg.AddCcs(message.CcEmails.Select(e => new EmailAddress(e)).ToList());
        }
        
        if (message.Attachments?.Any() == true)
        {
            foreach (var attachment in message.Attachments)
            {
                var bytes = await File.ReadAllBytesAsync(attachment.FilePath);
                msg.AddAttachment(attachment.FileName, Convert.ToBase64String(bytes));
            }
        }
        
        try
        {
            var response = await _client.SendEmailAsync(msg);
            
            return new EmailResult
            {
                IsSuccess = response.IsSuccessStatusCode,
                MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                SentAt = DateTime.UtcNow,
                Error = response.IsSuccessStatusCode ? null : await response.Body.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            return new EmailResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}
```

### AWS SES Provider
```csharp
public class SesEmailProvider : IEmailProvider
{
    private readonly SesSettings _settings;
    private readonly IAmazonSimpleEmailService _sesClient;
    
    public SesEmailProvider(IOptions<SesSettings> settings)
    {
        _settings = settings.Value;
        _sesClient = new AmazonSimpleEmailServiceClient(
            _settings.AccessKey,
            _settings.SecretKey,
            RegionEndpoint.GetBySystemName(_settings.Region));
    }
    
    public async Task<EmailResult> SendAsync(EmailMessage message)
    {
        var request = new SendEmailRequest
        {
            Source = $"{message.FromName} <{message.FromEmail}>",
            Destination = new Destination
            {
                ToAddresses = message.ToEmails.ToList(),
                CcAddresses = message.CcEmails?.ToList() ?? new List<string>()
            },
            Message = new Message
            {
                Subject = new Content(message.Subject),
                Body = new Body
                {
                    Html = message.IsHtml ? new Content(message.Body) : null,
                    Text = !message.IsHtml ? new Content(message.Body) : null
                }
            }
        };
        
        try
        {
            var response = await _sesClient.SendEmailAsync(request);
            
            return new EmailResult
            {
                IsSuccess = true,
                MessageId = response.MessageId,
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new EmailResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}
```

## Queue Processing
```csharp
public class EmailQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EmailQueueProcessor> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var queue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
            var provider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
            
            var emails = await queue.DequeueAsync(10);
            
            foreach (var email in emails)
            {
                try
                {
                    var result = await provider.SendAsync(email);
                    
                    if (result.IsSuccess)
                    {
                        await queue.MarkAsProcessedAsync(email.Id);
                    }
                    else
                    {
                        await queue.MarkAsFailedAsync(email.Id, result.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email {EmailId}", email.Id);
                    await queue.MarkAsFailedAsync(email.Id, ex.Message);
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

## Testing

### Development Mode (Console)
```csharp
public class ConsoleEmailProvider : IEmailProvider
{
    private readonly ILogger<ConsoleEmailProvider> _logger;
    
    public Task<EmailResult> SendAsync(EmailMessage message)
    {
        _logger.LogInformation(
            "Email:\nTo: {To}\nSubject: {Subject}\nBody: {Body}",
            string.Join(", ", message.ToEmails),
            message.Subject,
            message.Body);
        
        return Task.FromResult(new EmailResult
        {
            IsSuccess = true,
            MessageId = Guid.NewGuid().ToString(),
            SentAt = DateTime.UtcNow
        });
    }
}
```

## Installation
```bash
dotnet add package ACommerce.Notifications.Channels.Email
```

## Dependencies

- ACommerce.Notifications.Abstractions
- MailKit (SMTP)
- SendGrid (SendGrid provider)
- AWSSDK.SimpleEmail (SES provider)

## License

MIT