using ACommerce.AppConfig.Contracts;
using ACommerce.AppConfig.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.AppConfig.Api.Controllers;

/// <summary>
/// نقاط الإدارة — CRUD على المفاتيح. تتطلب صلاحية Admin.
/// كل تعديل يبطل الـcaches الداخلية فوراً.
/// </summary>
[ApiController]
[Route("api/admin/appconfig")]
[Authorize(Roles = "Admin")]
public class AdminAppConfigController(
    IAppConfigService appConfig,
    IFeatureFlagsService featureFlags) : ControllerBase
{
    // ─── UiString ────────────────────────────────────────────────

    [HttpGet("strings")]
    public async Task<ActionResult<IReadOnlyList<UiStringDto>>> ListStrings(
        [FromQuery] string? language = null, CancellationToken ct = default)
        => Ok(await appConfig.ListUiStringsAsync(language, ct));

    [HttpPost("strings")]
    [ProducesResponseType(typeof(UiStringDto), 200)]
    public async Task<ActionResult<UiStringDto>> UpsertString(
        [FromBody] UpsertUiStringDto dto, CancellationToken ct = default)
        => Ok(await appConfig.UpsertUiStringAsync(dto, ct));

    [HttpDelete("strings/{id:guid}")]
    public async Task<IActionResult> DeleteString(Guid id, CancellationToken ct = default)
    {
        await appConfig.DeleteUiStringAsync(id, ct);
        return NoContent();
    }

    // ─── ThemeToken ──────────────────────────────────────────────

    [HttpGet("theme")]
    public async Task<ActionResult<IReadOnlyList<ThemeTokenDto>>> ListTheme(CancellationToken ct = default)
        => Ok(await appConfig.ListThemeTokensAsync(ct));

    [HttpPost("theme")]
    public async Task<ActionResult<ThemeTokenDto>> UpsertTheme(
        [FromBody] UpsertThemeTokenDto dto, CancellationToken ct = default)
        => Ok(await appConfig.UpsertThemeTokenAsync(dto, ct));

    [HttpDelete("theme/{id:guid}")]
    public async Task<IActionResult> DeleteTheme(Guid id, CancellationToken ct = default)
    {
        await appConfig.DeleteThemeTokenAsync(id, ct);
        return NoContent();
    }

    // ─── AppConfigEntry ──────────────────────────────────────────

    [HttpGet("entries")]
    public async Task<ActionResult<IReadOnlyList<AppConfigEntryDto>>> ListEntries(CancellationToken ct = default)
        => Ok(await appConfig.ListAppConfigEntriesAsync(ct));

    [HttpPost("entries")]
    public async Task<ActionResult<AppConfigEntryDto>> UpsertEntry(
        [FromBody] UpsertAppConfigEntryDto dto, CancellationToken ct = default)
        => Ok(await appConfig.UpsertAppConfigEntryAsync(dto, ct));

    [HttpDelete("entries/{id:guid}")]
    public async Task<IActionResult> DeleteEntry(Guid id, CancellationToken ct = default)
    {
        await appConfig.DeleteAppConfigEntryAsync(id, ct);
        return NoContent();
    }

    // ─── FeatureFlag ─────────────────────────────────────────────

    [HttpGet("features")]
    public async Task<ActionResult<IReadOnlyList<FeatureFlagDto>>> ListFlags(CancellationToken ct = default)
        => Ok(await featureFlags.ListAsync(ct));

    [HttpGet("features/{key}")]
    [ProducesResponseType(typeof(FeatureFlagDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FeatureFlagDto>> GetFlag(string key, CancellationToken ct = default)
    {
        var flag = await featureFlags.GetByKeyAsync(key, ct);
        return flag is null ? NotFound() : Ok(flag);
    }

    [HttpPost("features")]
    public async Task<ActionResult<FeatureFlagDto>> UpsertFlag(
        [FromBody] UpsertFeatureFlagDto dto, CancellationToken ct = default)
        => Ok(await featureFlags.UpsertAsync(dto, ct));

    [HttpDelete("features/{id:guid}")]
    public async Task<IActionResult> DeleteFlag(Guid id, CancellationToken ct = default)
    {
        await featureFlags.DeleteAsync(id, ct);
        return NoContent();
    }
}
