using ACommerce.Auth.Core.Domain.Models;
using MediatR;

namespace ACommerce.Auth.Core.Application.Features.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<AuthResult>;


