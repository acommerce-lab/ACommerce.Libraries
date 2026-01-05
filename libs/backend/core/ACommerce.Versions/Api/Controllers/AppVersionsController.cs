using ACommerce.Versions.Contracts;
using ACommerce.Versions.DTOs;
using ACommerce.Versions.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Versions.Api.Controllers;

[ApiController]
[Route("api/versions")]
public class AppVersionsController(IAppVersionService versionService) : ControllerBase
{
    #region Public Endpoints (للعميل)

    /// <summary>
    /// فحص الإصدار للعميل
    /// </summary>
    [HttpPost("check")]
    [AllowAnonymous]
    public async Task<ActionResult<VersionCheckResult>> CheckVersion(
        [FromBody] VersionCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.CheckVersionAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على أحدث إصدار لتطبيق
    /// </summary>
    [HttpGet("latest/{applicationCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<AppVersionDto>> GetLatest(
        string applicationCode,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetLatestVersionAsync(applicationCode, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// الحصول على إصدار معين
    /// </summary>
    [HttpGet("{applicationCode}/{versionNumber}")]
    [AllowAnonymous]
    public async Task<ActionResult<AppVersionDto>> GetByVersion(
        string applicationCode,
        string versionNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetByVersionAsync(applicationCode, versionNumber, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    #endregion

    #region Admin Endpoints (للإدارة)

    /// <summary>
    /// الحصول على جميع الإصدارات
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AppVersionDto>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على الإصدارات النشطة
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AppVersionDto>>> GetActive(
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetActiveAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على إصدار بواسطة المعرف
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppVersionDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// الحصول على إصدارات تطبيق معين
    /// </summary>
    [HttpGet("app/{applicationCode}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AppVersionDto>>> GetByApplication(
        string applicationCode,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetByApplicationCodeAsync(applicationCode, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على الإصدارات حسب الحالة
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AppVersionDto>>> GetByStatus(
        VersionStatus status,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetByStatusAsync(status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على قائمة التطبيقات
    /// </summary>
    [HttpGet("applications")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<string>>> GetApplications(
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.GetApplicationCodesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// إنشاء إصدار جديد
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppVersionDto>> Create(
        [FromBody] CreateAppVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// تحديث إصدار
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppVersionDto>> Update(
        Guid id,
        [FromBody] UpdateAppVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.UpdateAsync(id, request, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// تغيير حالة الإصدار
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppVersionDto>> ChangeStatus(
        Guid id,
        [FromBody] VersionStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.ChangeStatusAsync(id, newStatus, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// تفعيل/تعطيل إصدار
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppVersionDto>> ToggleActive(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await versionService.ToggleActiveAsync(id, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// حذف إصدار
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await versionService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    #endregion
}
