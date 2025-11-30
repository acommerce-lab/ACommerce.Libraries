using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.Spaces.DTOs.Amenity;
using ACommerce.Spaces.Entities;

namespace Ashare.Api.Controllers;

/// <summary>
/// إدارة المرافق والخدمات
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AmenitiesController : BaseCrudController<SpaceAmenity, CreateAmenityDto, UpdateAmenityDto, AmenityResponseDto, UpdateAmenityDto>
{
    public AmenitiesController(IMediator mediator, ILogger<AmenitiesController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// الحصول على مرافق المساحة
    /// </summary>
    [HttpGet("space/{spaceId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<AmenityResponseDto>>> GetBySpace(Guid spaceId)
    {
        _logger.LogDebug("Getting amenities for space: {SpaceId}", spaceId);
        return Ok(new List<AmenityResponseDto>());
    }

    /// <summary>
    /// الحصول على المرافق حسب الفئة
    /// </summary>
    [HttpGet("by-category/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<AmenityResponseDto>>> GetByCategory(string category)
    {
        _logger.LogDebug("Getting amenities by category: {Category}", category);
        return Ok(new List<AmenityResponseDto>());
    }

    /// <summary>
    /// الحصول على جميع الفئات
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        _logger.LogDebug("Getting all amenity categories");
        return Ok(new List<string>
        {
            "الأساسيات",
            "التقنية",
            "المطبخ",
            "الترفيه",
            "إمكانية الوصول",
            "الأمان",
            "مواقف السيارات"
        });
    }

    /// <summary>
    /// إضافة مرافق متعددة للمساحة
    /// </summary>
    [HttpPost("space/{spaceId}/bulk")]
    [Authorize]
    public async Task<ActionResult<List<AmenityResponseDto>>> AddBulk(Guid spaceId, [FromBody] List<CreateAmenityDto> amenities)
    {
        _logger.LogDebug("Adding {Count} amenities to space: {SpaceId}", amenities.Count, spaceId);
        return Ok(new List<AmenityResponseDto>());
    }
}
