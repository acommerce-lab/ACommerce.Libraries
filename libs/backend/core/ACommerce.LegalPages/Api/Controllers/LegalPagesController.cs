using ACommerce.LegalPages.Contracts;
using ACommerce.LegalPages.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.LegalPages.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LegalPagesController(ILegalPagesService legalPagesService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<LegalPageDto>>> GetActive(CancellationToken cancellationToken = default)
    {
        var result = await legalPagesService.GetActiveAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<LegalPageDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await legalPagesService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<LegalPageDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await legalPagesService.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("key/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<LegalPageDto>> GetByKey(string key, CancellationToken cancellationToken = default)
    {
        var result = await legalPagesService.GetByKeyAsync(key, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LegalPageDto>> Create([FromBody] CreateLegalPageRequest request, CancellationToken cancellationToken = default)
    {
        var result = await legalPagesService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LegalPageDto>> Update(Guid id, [FromBody] UpdateLegalPageRequest request, CancellationToken cancellationToken = default)
    {
        var result = await legalPagesService.UpdateAsync(id, request, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await legalPagesService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
