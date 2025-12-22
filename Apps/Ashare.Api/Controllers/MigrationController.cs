using Ashare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api.Controllers;

/// <summary>
/// وحدة تحكم الترحيل
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MigrationController : ControllerBase
{
    private readonly OffersMigrationService _migrationService;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(
        OffersMigrationService migrationService,
        ILogger<MigrationController> logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// ترحيل العروض من النظام القديم
    /// </summary>
    [HttpPost("offers")]
    public async Task<ActionResult<MigrationResult>> MigrateOffers(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting offers migration via API...");

        var result = await _migrationService.MigrateOffersAsync(cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// بذر العروض من البيانات المحفوظة (بدون اتصال بالنظام القديم)
    /// </summary>
    [HttpPost("offers/seed")]
    public async Task<ActionResult<MigrationResult>> SeedOffers(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting offers seeding from static data...");

        var result = await _migrationService.SeedOffersFromStaticDataAsync(cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
