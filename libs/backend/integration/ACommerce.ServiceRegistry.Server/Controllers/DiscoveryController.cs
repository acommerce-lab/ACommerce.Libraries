using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.ServiceRegistry.Server.Controllers;

/// <summary>
/// API لاكتشاف الخدمات (Service Discovery)
/// </summary>
[ApiController]
[Route("api/discovery")]
public class DiscoveryController : ControllerBase
{
	private readonly IServiceDiscovery _discovery;
	private readonly ILogger<DiscoveryController> _logger;

	public DiscoveryController(IServiceDiscovery discovery, ILogger<DiscoveryController> logger)
	{
		_discovery = discovery;
		_logger = logger;
	}

	/// <summary>
	/// اكتشاف خدمة (مع Load Balancing)
	/// </summary>
	[HttpPost("discover")]
	public async Task<ActionResult<ServiceEndpoint>> Discover([FromBody] ServiceQuery query)
	{
		var service = await _discovery.DiscoverAsync(query);
		if (service == null)
			return NotFound(new { error = $"No service found for: {query.ServiceName}" });

		return Ok(service);
	}

	/// <summary>
	/// اكتشاف جميع نسخ خدمة
	/// </summary>
	[HttpPost("discover-all")]
	public async Task<ActionResult<IEnumerable<ServiceEndpoint>>> DiscoverAll([FromBody] ServiceQuery query)
	{
		var services = await _discovery.DiscoverAllAsync(query);
		return Ok(services);
	}

	/// <summary>
	/// الحصول على خدمة بالاسم (طريقة مختصرة)
	/// </summary>
	[HttpGet("{serviceName}")]
	public async Task<ActionResult<ServiceEndpoint>> GetService(string serviceName)
	{
		var service = await _discovery.GetServiceAsync(serviceName);
		if (service == null)
			return NotFound(new { error = $"No service found for: {serviceName}" });

		return Ok(service);
	}

	/// <summary>
	/// الحصول على جميع نسخ خدمة
	/// </summary>
	[HttpGet("{serviceName}/instances")]
	public async Task<ActionResult<IEnumerable<ServiceEndpoint>>> GetAllInstances(string serviceName)
	{
		var services = await _discovery.GetAllInstancesAsync(serviceName);
		return Ok(services);
	}
}
