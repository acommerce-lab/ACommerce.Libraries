using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ACommerce.Authentication.AspNetCore.Swagger;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddACommerceSwagger(
        this IServiceCollection services,
        Action<ACommerceSwaggerOptions>? configure = null)
    {
        var options = new ACommerceSwaggerOptions();
        configure?.Invoke(options);

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(options.Version, new OpenApiInfo
            {
                Title = options.Title,
                Version = options.Version,
                Description = options.Description,
                Contact = options.Contact != null ? new OpenApiContact
                {
                    Name = options.Contact.Name,
                    Email = options.Contact.Email,
                    Url = options.Contact.Url != null ? new Uri(options.Contact.Url) : null
                } : null,
                License = options.License != null ? new OpenApiLicense
                {
                    Name = options.License.Name,
                    Url = options.License.Url != null ? new Uri(options.License.Url) : null
                } : null
            });

            if (options.EnableJwtAuthentication)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "أدخل JWT Token في الحقل التالي بهذا الشكل: Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }

            if (options.EnableAnnotations)
            {
                c.EnableAnnotations();
            }

            if (options.IncludeXmlComments)
            {
                var xmlFiles = Directory.GetFiles(
                    AppContext.BaseDirectory,
                    "*.xml",
                    SearchOption.TopDirectoryOnly);

                foreach (var xmlFile in xmlFiles)
                {
                    c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
                }
            }

            c.OperationFilter<TwoFactorOperationFilter>();

            c.TagActionsBy(api => new[]
            {
                api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default"
            });

            c.DocInclusionPredicate((name, api) => true);
        });

        return services;
    }
}

public class ACommerceSwaggerOptions
{
    public string Title { get; set; } = "ACommerce Authentication API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "واجهة برمجة التطبيقات للمصادقة";
    public bool EnableJwtAuthentication { get; set; } = true;
    public bool EnableAnnotations { get; set; } = true;
    public bool IncludeXmlComments { get; set; } = false;
    public SwaggerContactInfo? Contact { get; set; }
    public SwaggerLicenseInfo? License { get; set; }
}

public class SwaggerContactInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
}

public class SwaggerLicenseInfo
{
    public string? Name { get; set; }
    public string? Url { get; set; }
}