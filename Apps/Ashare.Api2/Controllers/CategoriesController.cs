using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api2.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IBaseAsyncRepository<Category> _repo;

    public CategoriesController(IRepositoryFactory factory)
    {
        _repo = factory.CreateRepository<Category>();
    }

    [HttpGet]
    public async Task<IActionResult> ListAll(CancellationToken ct)
    {
        var all = await _repo.GetAllWithPredicateAsync(c => c.IsActive);
        return Ok(all.OrderBy(c => c.SortOrder));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var c = await _repo.GetByIdAsync(id, ct);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var matches = await _repo.GetAllWithPredicateAsync(c => c.Slug == slug);
        var c = matches.FirstOrDefault();
        return c == null ? NotFound() : Ok(c);
    }
}
