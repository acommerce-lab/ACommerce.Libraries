using ACommerce.AccountingKernel.Engine;
using ACommerce.RealEstate.Api.Entities;
using ACommerce.RealEstate.Api.Entries;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.RealEstate.Api.Controllers;

/// <summary>
/// متحكم العقارات - كل عملية تمر عبر المحرك المحاسبي.
/// لاحظ: لا يوجد منطق أعمال هنا - فقط بناء القيد وتنفيذه.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly EntryEngine _engine;
    private readonly IPropertyStore _store;

    public PropertiesController(EntryEngine engine, IPropertyStore store)
    {
        _engine = engine;
        _store = store;
    }

    /// <summary>
    /// البحث عن عقارات (GET /api/properties?city=الرياض&purpose=rent)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] PropertySearchRequest request)
    {
        var entry = PropertyEntries.SearchProperties(request, _store);
        var result = await _engine.ExecuteAsync(entry);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var ctx = result.Context!;
        return Ok(new
        {
            items = ctx.Get<List<Property>>("results"),
            count = ctx.Get<int>("count"),
            entry = new { id = result.EntryId, type = result.EntryType, success = true }
        });
    }

    /// <summary>
    /// عرض تفاصيل عقار (GET /api/properties/{id})
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entry = PropertyEntries.ViewProperty(id, null, _store);
        var result = await _engine.ExecuteAsync(entry);

        if (!result.Success)
            return NotFound(new { error = result.ErrorMessage });

        var ctx = result.Context!;
        return Ok(ctx.Get<Property>("property"));
    }

    /// <summary>
    /// نشر عقار جديد (POST /api/properties)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePropertyRequest request)
    {
        var property = new Property
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            PropertyType = request.PropertyType,
            Purpose = request.Purpose,
            City = request.City,
            District = request.District,
            Price = request.Price,
            Area = request.Area,
            Rooms = request.Rooms,
            Bathrooms = request.Bathrooms,
            Floor = request.Floor,
            Furnished = request.Furnished,
            OwnerId = request.OwnerId,
            OwnerName = request.OwnerName,
            OwnerPhone = request.OwnerPhone
        };

        var entry = PropertyEntries.ListProperty(property, _store);
        var result = await _engine.ExecuteAsync(entry);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage, entry = new { id = result.EntryId, type = result.EntryType } });

        var ctx = result.Context!;
        var created = ctx.Get<Property>("property");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
        {
            property = created,
            entry = new { id = result.EntryId, type = result.EntryType, success = true }
        });
    }

    /// <summary>
    /// تحديث عقار (PUT /api/properties/{id})
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        var entry = PropertyEntries.UpdateProperty(id, p =>
        {
            if (request.Title != null) p.Title = request.Title;
            if (request.Description != null) p.Description = request.Description;
            if (request.Price.HasValue) p.Price = request.Price.Value;
            if (request.Status != null) p.Status = request.Status;
        }, _store);

        var result = await _engine.ExecuteAsync(entry);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var ctx = result.Context!;
        return Ok(ctx.Get<Property>("property"));
    }

    /// <summary>
    /// إرسال استفسار عن عقار (POST /api/properties/{id}/inquiries)
    /// </summary>
    [HttpPost("{id:guid}/inquiries")]
    public async Task<IActionResult> SendInquiry(Guid id, [FromBody] CreateInquiryRequest request)
    {
        var inquiry = new PropertyInquiry
        {
            PropertyId = id,
            UserId = request.UserId,
            UserName = request.UserName,
            UserPhone = request.UserPhone,
            Message = request.Message
        };

        var entry = PropertyEntries.SendInquiry(inquiry, _store);
        var result = await _engine.ExecuteAsync(entry);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new
        {
            inquiry = inquiry,
            entry = new { id = result.EntryId, type = result.EntryType, success = true },
            subEntries = result.SubResults.Select(s => new { s.EntryId, s.EntryType, s.Success })
        });
    }

    /// <summary>
    /// حذف عقار (DELETE /api/properties/{id})
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entry = PropertyEntries.RemoveProperty(id, _store);
        var result = await _engine.ExecuteAsync(entry);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { removed = true, entry = new { id = result.EntryId, type = result.EntryType } });
    }

}

// === DTOs ===

public class CreatePropertyRequest
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string Category { get; set; } = default!;
    public string PropertyType { get; set; } = default!;
    public string Purpose { get; set; } = default!;
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public int? Rooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Floor { get; set; }
    public bool? Furnished { get; set; }
    public Guid OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
}

public class UpdatePropertyRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Status { get; set; }
}

public class CreateInquiryRequest
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserPhone { get; set; }
    public string? Message { get; set; }
}
