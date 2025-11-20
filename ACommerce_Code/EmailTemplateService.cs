using ACommerce.Notifications.Channels.Email.Options;
using Microsoft.Extensions.Logging;
using RazorEngineCore;

namespace ACommerce.Notifications.Channels.Email.Services;

/// <summary>
/// ???? ?????? ????? ?????? ??????????
/// </summary>
public class EmailTemplateService
{
	private readonly EmailOptions _options;
	private readonly ILogger<EmailTemplateService> _logger;
	private readonly Dictionary<string, IRazorEngineCompiledTemplate> _compiledTemplates = new();
	private readonly IRazorEngine _razorEngine = new RazorEngine();

	public EmailTemplateService(
		EmailOptions options,
		ILogger<EmailTemplateService> logger)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ????? HTML ?? ????
	/// </summary>
	public async Task<string> RenderTemplateAsync(
		string templateName,
		object model,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug("Rendering email template: {TemplateName}", templateName);

			// ?????? ?????? ??? ?????? ??????? ?? ??? cache
			if (!_compiledTemplates.TryGetValue(templateName, out var compiledTemplate))
			{
				var templatePath = Path.Combine(_options.TemplatesPath, $"{templateName}.cshtml");

				if (!File.Exists(templatePath))
				{
					_logger.LogWarning(
						"Template {TemplateName} not found at {TemplatePath}, using default",
						templateName,
						templatePath);

					templatePath = Path.Combine(_options.TemplatesPath, $"{_options.DefaultTemplate}.cshtml");

					if (!File.Exists(templatePath))
					{
						_logger.LogWarning("Default template not found, using inline template");
						return GenerateSimpleHtmlTemplate(model);
					}
				}

				var templateContent = await File.ReadAllTextAsync(templatePath, cancellationToken);
				compiledTemplate = await _razorEngine.CompileAsync(templateContent);

				// ??? ?? ??? cache
				_compiledTemplates[templateName] = compiledTemplate;
			}

			var html = await compiledTemplate.RunAsync(model);

			_logger.LogDebug("Template {TemplateName} rendered successfully", templateName);

			return html;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to render template {TemplateName}", templateName);
			return GenerateSimpleHtmlTemplate(model);
		}
	}

	/// <summary>
	/// ????? ???? HTML ???? ?? ???? ??? ???? ?????
	/// </summary>
	private string GenerateSimpleHtmlTemplate(object model)
	{
		var properties = model.GetType().GetProperties();
		var title = properties.FirstOrDefault(p => p.Name.Contains("Title", StringComparison.OrdinalIgnoreCase))
			?.GetValue(model)?.ToString() ?? "?????";

		var message = properties.FirstOrDefault(p => p.Name.Contains("Message", StringComparison.OrdinalIgnoreCase))
			?.GetValue(model)?.ToString() ?? "";

		return $@"
			<!DOCTYPE html>
			<html lang='ar' dir='rtl'>
			<head>
				<meta charset='UTF-8'>
				<meta name='viewport' content='width=device-width, initial-scale=1.0'>
				<style>
					body {{
						font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
						background-color: #f4f4f4;
						margin: 0;
						padding: 20px;
					}}
					.container {{
						max-width: 600px;
						margin: 0 auto;
						background: white;
						border-radius: 8px;
						box-shadow: 0 2px 4px rgba(0,0,0,0.1);
						overflow: hidden;
					}}
					.header {{
						background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
						color: white;
						padding: 30px;
						text-align: center;
					}}
					.content {{
						padding: 30px;
					}}
					.footer {{
						background: #f8f9fa;
						padding: 20px;
						text-align: center;
						font-size: 12px;
						color: #6c757d;
					}}
					h1 {{ margin: 0; font-size: 24px; }}
					p {{ line-height: 1.6; color: #333; }}
				</style>
			</head>
			<body>
				<div class='container'>
					<div class='header'>
						<h1>{title}</h1>
					</div>
					<div class='content'>
						<p>{message}</p>
					</div>
					<div class='footer'>
						<p>© 2025 ACommerce. ???? ?????? ??????.</p>
					</div>
				</div>
			</body>
			</html>";
	}
}

