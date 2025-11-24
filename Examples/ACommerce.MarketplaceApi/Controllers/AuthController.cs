using Microsoft.AspNetCore.Mvc;
using ACommerce.MarketplaceApi.Services;

namespace ACommerce.MarketplaceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly MockAuthService _authService;

	public AuthController(MockAuthService authService)
	{
		_authService = authService;
	}

	/// <summary>
	/// تسجيل الدخول
	/// </summary>
	[HttpPost("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		var result = _authService.Login(request.Email, request.Password);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// إنشاء حساب جديد
	/// </summary>
	[HttpPost("register")]
	public IActionResult Register([FromBody] RegisterRequest request)
	{
		var result = _authService.Register(request.Email, request.Password, request.FullName, request.Role);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// معلومات المستخدم الحالي
	/// </summary>
	[HttpGet("me")]
	public IActionResult GetCurrentUser()
	{
		var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

		if (string.IsNullOrEmpty(token))
			return Unauthorized(new { Message = "لم يتم تسجيل الدخول" });

		var user = _authService.GetUserByToken(token);

		if (user == null)
			return Unauthorized(new { Message = "رمز غير صالح" });

		return Ok(user);
	}

	/// <summary>
	/// قائمة المستخدمين التجريبيين (للمساعدة في الاختبار)
	/// </summary>
	[HttpGet("test-users")]
	public IActionResult GetTestUsers()
	{
		return Ok(new
		{
			Message = "مستخدمين تجريبيين - كلمة المرور للجميع: 123456",
			Users = new[]
			{
				new { Email = "customer@example.com", Role = "Customer", Name = "أحمد محمد" },
				new { Email = "vendor@example.com", Role = "Vendor", Name = "متجر الإلكترونيات" },
				new { Email = "admin@example.com", Role = "Admin", Name = "المدير" }
			}
		});
	}
}

public class LoginRequest
{
	public required string Email { get; set; }
	public required string Password { get; set; }
}

public class RegisterRequest
{
	public required string Email { get; set; }
	public required string Password { get; set; }
	public required string FullName { get; set; }
	public string Role { get; set; } = "Customer";
}
