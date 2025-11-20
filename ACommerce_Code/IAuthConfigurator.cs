using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Auth.Core.Application.Contracts;

public interface IAuthConfigurator
{
	void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration);
	void ConfigureAuthorization(IServiceCollection services, IConfiguration configuration);
	void Register(IServiceCollection services, IConfiguration configuration);
}

