using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace ACommerce.Authentication.AspNetCore.Swagger;

/// <summary>
/// Custom operation filter to enhance Swagger documentation for 2FA endpoints
/// </summary>
public class TwoFactorOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];

        switch (actionName?.ToLower())
        {
            case "initiatetwofactor":
                operation.Summary = "بدء المصادقة الثنائية";
                operation.Description = "يبدأ عملية المصادقة الثنائية ويرسل رمز التحقق للمستخدم (عبر SMS، نفاذ)";

                if (operation.Tags == null || operation.Tags.Count == 0)
                {
                    operation.Tags = new List<OpenApiTag>
                    {
                        new OpenApiTag { Name = "المصادقة الثنائية" }
                    };
                }

                AddResponseExample(operation, "200", new
                {
                    success = true,
                    transactionId = "550e8400-e29b-41d4-a716-446655440000",
                    verificationCode = "00",
                    message = "يرجى التحقق من هويتك عبر تطبيق نفاذ",
                    expiresIn = 300,
                    provider = "Nafath"
                });

                AddResponseExample(operation, "400", new
                {
                    error = "NAFATH_API_ERROR",
                    message = "فشل الاتصال بخدمة نفاذ"
                });
                break;

            case "verifytwofactor":
                operation.Summary = "التحقق من المصادقة الثنائية";
                operation.Description = "يتحقق من رمز المصادقة الثنائية ويُصدر رمز الوصول (Access Token)";

                if (operation.Tags == null || operation.Tags.Count == 0)
                {
                    operation.Tags = new List<OpenApiTag>
                    {
                        new OpenApiTag { Name = "المصادقة الثنائية" }
                    };
                }

                AddResponseExample(operation, "200", new
                {
                    success = true,
                    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    refreshToken = "refresh_token_here",
                    tokenType = "Bearer",
                    expiresAt = "2024-12-31T23:59:59Z",
                    userId = "2507643761"
                });
                break;

            case "canceltwofactor":
                operation.Summary = "إلغاء المصادقة الثنائية";
                operation.Description = "يلغي جلسة المصادقة الثنائية الجارية";

                if (operation.Tags == null || operation.Tags.Count == 0)
                {
                    operation.Tags = new List<OpenApiTag>
                    {
                        new OpenApiTag { Name = "المصادقة الثنائية" }
                    };
                }
                break;

            case "refreshtoken":
                operation.Summary = "تحديث رمز الوصول";
                operation.Description = "يُصدر رمز وصول جديد باستخدام رمز التحديث (Refresh Token)";

                if (operation.Tags == null || operation.Tags.Count == 0)
                {
                    operation.Tags = new List<OpenApiTag>
                    {
                        new OpenApiTag { Name = "إدارة الرموز" }
                    };
                }
                break;

            case "getinfo":
                operation.Summary = "معلومات الخدمة";
                operation.Description = "يعرض معلومات عن مزودي المصادقة المُفعّلين";

                if (operation.Tags == null || operation.Tags.Count == 0)
                {
                    operation.Tags = new List<OpenApiTag>
                    {
                        new OpenApiTag { Name = "معلومات النظام" }
                    };
                }
                break;
        }
    }

    private void AddResponseExample(OpenApiOperation operation, string statusCode, object example)
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = statusCode == "200" ? "Success" : "Error"
            });
        }

        var response = operation.Responses[statusCode];

        if (response.Content == null)
        {
            response.Content = new Dictionary<string, OpenApiMediaType>();
        }

        if (!response.Content.ContainsKey("application/json"))
        {
            response.Content["application/json"] = new OpenApiMediaType();
        }

        var jsonString = JsonSerializer.Serialize(example, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        response.Content["application/json"].Example = OpenApiAnyFactory.CreateFromJson(jsonString);
    }
}