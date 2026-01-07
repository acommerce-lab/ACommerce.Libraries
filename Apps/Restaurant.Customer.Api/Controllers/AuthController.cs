using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Auth;
using Restaurant.Core.Services;

namespace Restaurant.Customer.Api.Controllers;

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
    /// طلب إرسال رمز OTP للهاتف
    /// </summary>
    [HttpPost("otp/send")]
    public async Task<ActionResult<SendOtpResponse>> SendOtp([FromBody] SendOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(new SendOtpResponse
            {
                Success = false,
                Message = "رقم الهاتف مطلوب"
            });
        }

        var result = await _authService.SendOtpAsync(request.PhoneNumber, "Customer");
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

        return Ok(result);
    }

    /// <summary>
    /// تسجيل مستخدم جديد (إكمال البيانات بعد التحقق)
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Error = "الاسم ورقم التحقق مطلوبان"
            });
        }

        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetMe()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token);

        if (user == null)
            return Unauthorized(new { message = "غير مصرح" });

        return Ok(user);
    }

    /// <summary>
    /// تحديث معلومات المستخدم
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token);

        if (user == null)
            return Unauthorized(new { message = "غير مصرح" });

        var success = await _authService.UpdateProfileAsync(user.Id, request);

        if (!success)
            return BadRequest(new { message = "فشل تحديث البيانات" });

        return Ok(new { message = "تم تحديث البيانات بنجاح" });
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        // في حالة JWT، لا يمكن إلغاء الـ Token فعلياً
        // يمكن إضافة Token blacklist في المستقبل
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }
}
