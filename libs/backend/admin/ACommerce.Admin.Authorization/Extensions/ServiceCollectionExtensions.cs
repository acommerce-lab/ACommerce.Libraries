using ACommerce.Admin.Authorization.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ACommerce.Admin.Authorization.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdminAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminRoleService, AdminRoleService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "ACommerce.Admin",
                ValidAudience = configuration["Jwt:Audience"] ?? "ACommerce.Admin",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    configuration["Jwt:Secret"] ?? "AdminSecretKeyForJwtTokenGeneration123!"))
            };
        });

        return services;
    }
}
