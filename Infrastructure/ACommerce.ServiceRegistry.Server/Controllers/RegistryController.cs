using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.ServiceRegistry.Server.Controllers;

/// <summary>
/// API لتسجيل وإدارة الخدمات
/// </summary>
[ApiController]
[Route("api/registry")]
public class RegistryController : ControllerBase
{
	private readonly IServiceRegistry _registry;
	private readonly ILogger<RegistryController> _logger;

	public RegistryController(IServiceRegistry registry, ILogger<RegistryController> logger)
	{
		_registry = registry;
		_logger = logger;
	}

	/// <summary>
	/// تسجيل خدمة جديدة
	/// </summary>
	[HttpPost("register")]
	public async Task<ActionResult<ServiceEndpoint>> Register([FromBody] ServiceRegistration registration)
	{
		try
		{
			var endpoint = await _registry.RegisterAsync(registration);
			return Ok(endpoint);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	/// <summary>
	/// إلغاء تسجيل خدمة
	/// </summary>
	[HttpDelete("{serviceId}")]
	public async Task<IActionResult> Deregister(string serviceId)
	{
		var result = await _registry.DeregisterAsync(serviceId);
		if (!result)
			return NotFound(new { error = "Service not found" });

		return Ok(new { message = "Service deregistered successfully" });
	}

	/// <summary>
	/// Heartbeat - تأكيد أن الخدمة ما زالت حية
	/// </summary>
	[HttpPost("{serviceId}/heartbeat")]
	public async Task<IActionResult> Heartbeat(string serviceId)
	{
		await _registry.HeartbeatAsync(serviceId);
		return Ok(new { message = "Heartbeat received" });
	}

	/// <summary>
	/// تحديث حالة صحة خدمة
	/// </summary>
	[HttpPut("{serviceId}/health")]
	public async Task<IActionResult> UpdateHealth(string serviceId, [FromBody] ServiceHealth health)
	{
		await _registry.UpdateHealthAsync(serviceId, health);
		return Ok(new { message = "Health updated" });
	}

	/// <summary>
	/// الحصول على خدمة محددة
	/// </summary>
	[HttpGet("{serviceId}")]
	public async Task<ActionResult<ServiceEndpoint>> GetById(string serviceId)
	{
		var service = await _registry.GetByIdAsync(serviceId);
		if (service == null)
			return NotFound(new { error = "Service not found" });

		return Ok(service);
	}

	/// <summary>
	/// الحصول على جميع الخدمات المسجلة
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<ServiceEndpoint>>> GetAll()
	{
		var services = await _registry.GetAllAsync();
		return Ok(services);
	}
}
