using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.Users.Abstractions.Models;
using ACommerce.Profiles.DTOs;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.Enums;
using ACommerce.SharedKernel.CQRS.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HamtramckHardware.Api.Controllers;

/// <summary>
/// Authentication controller for Hamtramck Hardware
/// Handles email/password registration and login
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserProvider _userProvider;
    private readonly IAuthenticationProvider _authProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserProvider userProvider,
        IAuthenticationProvider authProvider,
        IMediator mediator,
        ILogger<AuthController> logger)
    {
        _userProvider = userProvider;
        _authProvider = authProvider;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            // Check if email already exists
            var existingUser = await _userProvider.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "An account with this email already exists."
                });
            }

            // Create user in authentication system
            var createUserResult = await _userProvider.CreateUserAsync(new CreateUserRequest
            {
                Username = request.Email,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                Roles = ["Customer"],
                RequireEmailConfirmation = false
            });

            if (!createUserResult.Success)
            {
                _logger.LogWarning("User creation failed: {Error}", createUserResult.Error?.Message);
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = createUserResult.Error?.Message ?? "Failed to create account."
                });
            }

            var userId = createUserResult.User!.UserId;

            // Create profile
            var profileId = Guid.NewGuid();
            var profile = new CreateProfileDto
            {
                UserId = userId,
                FullName = request.FullName ?? "",
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Type = ProfileType.Customer
            };

            try
            {
                var createCommand = new CreateCommand<Profile, CreateProfileDto> { Data = profile };
                await _mediator.Send(createCommand);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Profile creation failed, but user was created");
            }

            // Generate JWT token using the authentication provider
            var authResult = await _authProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = userId,
                Claims = new Dictionary<string, string>
                {
                    [ClaimTypes.Email] = request.Email,
                    [ClaimTypes.Role] = "Customer"
                }
            });

            if (!authResult.Success)
            {
                _logger.LogError("Token generation failed: {Error}", authResult.Error?.Message);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Account created but token generation failed."
                });
            }

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken,
                ProfileId = profileId.ToString(),
                FullName = request.FullName,
                ExpiresAt = authResult.ExpiresAt?.UtcDateTime,
                Message = "Account created successfully!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for email: {Email}", request.Email);
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "An error occurred during registration."
            });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Find user by email
            var user = await _userProvider.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            // Verify password
            var isValidPassword = await _userProvider.VerifyPasswordAsync(user.UserId, request.Password);
            if (!isValidPassword)
            {
                _logger.LogWarning("Login failed - invalid password: {Email}", request.Email);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            // Check if user is locked
            if (user.IsLocked)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Account is locked. Please contact support."
                });
            }

            // Generate JWT token using the authentication provider
            var roleClaims = user.Roles.ToDictionary(
                role => $"{ClaimTypes.Role}_{role}",
                role => role
            );
            roleClaims[ClaimTypes.Email] = user.Email;

            var authResult = await _authProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = user.UserId,
                Claims = roleClaims
            });

            if (!authResult.Success)
            {
                _logger.LogError("Token generation failed: {Error}", authResult.Error?.Message);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Login failed due to token generation error."
                });
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken,
                ProfileId = user.UserId,
                FullName = user.Metadata?.GetValueOrDefault("FullName"),
                ExpiresAt = authResult.ExpiresAt?.UtcDateTime,
                Message = "Login successful!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for email: {Email}", request.Email);
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login."
            });
        }
    }

    /// <summary>
    /// Logout (invalidates token on client side)
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT tokens are stateless, so logout is handled client-side
        return Ok(new { success = true, message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ProfileResponse>> GetMe()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var user = await _userProvider.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new ProfileResponse
        {
            Id = user.UserId,
            FullName = user.Metadata?.GetValueOrDefault("FullName"),
            PhoneNumber = user.PhoneNumber,
            IsVerified = user.EmailVerified
        });
    }
}

#region DTOs

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ProfileId { get; set; }
    public string? FullName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
}

public class ProfileResponse
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsVerified { get; set; }
}

#endregion
