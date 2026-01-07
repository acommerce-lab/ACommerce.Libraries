using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Auth;
using Restaurant.Core.Services;

namespace Restaurant.Driver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;

    public AuthController(AuthenticationService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// طلب إرسال رمز OTP للهاتف (السائق)
    /// </summary>
    [HttpPost("otp/send")]
    public async Task<ActionResult<SendOtpResponse>> SendOtp([FromBody] DriverLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(new SendOtpResponse
            {
                Success = false,
                Message = "رقم الهاتف مطلوب"
            });
        }

        // التحقق من أن السائق مسجل في النظام
        // TODO: التحقق من قاعدة البيانات

        var result = await _authService.SendOtpAsync(request.PhoneNumber, "Driver");
        return Ok(result);
    }

    /// <summary>
    /// التحقق من رمز OTP وتسجيل الدخول
    /// </summary>
    [HttpPost("otp/verify")]
    public async Task<ActionResult<LoginResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Error = "جميع الحقول مطلوبة"
            });
        }

        var result = await _authService.VerifyOtpAsync(request);

        if (!result.Success)
            return BadRequest(result);

        // التحقق من أن المستخدم سائق
        if (result.User?.UserType != "Driver")
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Error = "هذا الرقم غير مسجل كسائق"
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// الحصول على معلومات السائق الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetMe()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token);

        if (user == null)
            return Unauthorized(new { message = "غير مصرح" });

        if (user.UserType != "Driver")
            return Forbid();

        return Ok(user);
    }

    /// <summary>
    /// تحديث معلومات السائق
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token);

        if (user == null)
            return Unauthorized(new { message = "غير مصرح" });

        if (user.UserType != "Driver")
            return Forbid();

        var success = await _authService.UpdateProfileAsync(user.Id, request);

        if (!success)
            return BadRequest(new { message = "فشل تحديث البيانات" });

        return Ok(new { message = "تم تحديث البيانات بنجاح" });
    }

    /// <summary>
    /// تحديث حالة التوفر
    /// </summary>
    [HttpPost("availability")]
    [Authorize]
    public async Task<ActionResult> UpdateAvailability([FromBody] DriverAvailabilityRequest request)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token);

        if (user == null)
            return Unauthorized(new { message = "غير مصرح" });

        if (user.UserType != "Driver")
            return Forbid();

        // TODO: تحديث حالة التوفر في قاعدة البيانات
        return Ok(new
        {
            message = request.IsAvailable ? "أنت متاح الآن" : "أنت غير متاح الآن",
            isAvailable = request.IsAvailable
        });
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }
}

public class DriverAvailabilityRequest
{
    public bool IsAvailable { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
