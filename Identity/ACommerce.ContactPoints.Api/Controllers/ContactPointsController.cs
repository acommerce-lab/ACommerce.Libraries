using Microsoft.AspNetCore.Mvc;
using MediatR;
using ACommerce.ContactPoints.Entities;
using ACommerce.ContactPoints.DTOs;

namespace ACommerce.ContactPoints.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactPointsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactPointsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// الحصول على نقاط الاتصال للمستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<List<ContactPointResponseDto>>> GetMyContactPoints()
    {
        // TODO: Implement with CQRS
        return Ok(new List<ContactPointResponseDto>());
    }

    /// <summary>
    /// الحصول على نقطة اتصال محددة
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactPointResponseDto>> GetById(Guid id)
    {
        // TODO: Implement with CQRS
        return NotFound();
    }

    /// <summary>
    /// إنشاء نقطة اتصال جديدة
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContactPointResponseDto>> Create([FromBody] CreateContactPointDto dto)
    {
        // TODO: Implement with CQRS
        return CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, new ContactPointResponseDto());
    }

    /// <summary>
    /// تحديث نقطة اتصال
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactPointDto dto)
    {
        // TODO: Implement with CQRS
        return NoContent();
    }

    /// <summary>
    /// حذف نقطة اتصال
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // TODO: Implement with CQRS
        return NoContent();
    }
}
