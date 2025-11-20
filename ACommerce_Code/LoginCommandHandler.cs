using ACommerce.Auth.Core.Application.Contracts;
using ACommerce.Auth.Core.Domain.Models;
using MediatR;

namespace ACommerce.Auth.Core.Application.Features.Commands.Login;

public class LoginCommandHandler(IAuthService authService)
	: IRequestHandler<LoginCommand, AuthResult>
{
	public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
		=> await authService.LoginAsync(request.Username, request.Password);
}

