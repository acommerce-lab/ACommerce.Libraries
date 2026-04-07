using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api2.Controllers;

[ApiController]
[Route("api/device-tokens")]
public class DeviceTokensController : ControllerBase
{
    private readonly IBaseAsyncRepository<DeviceToken> _repo;

    public DeviceTokensController(IRepositoryFactory factory)
    {
        _repo = factory.CreateRepository<DeviceToken>();
    }

    public record RegisterTokenRequest(Guid UserId, string Token, string Platform);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterTokenRequest req, CancellationToken ct)
    {
        // إذا كان موجوداً، نحدّث LastSeen
        var existing = await _repo.GetAllWithPredicateAsync(t => t.Token == req.Token);
        if (existing.Count > 0)
        {
            var t = existing.First();
            t.LastSeenAt = DateTime.UtcNow;
            t.IsActive = true;
            t.UserId = req.UserId;
            await _repo.UpdateAsync(t, ct);
            return Ok(t);
        }

        var entity = new DeviceToken
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UserId = req.UserId,
            Token = req.Token,
            Platform = req.Platform,
            LastSeenAt = DateTime.UtcNow
        };
        await _repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetByUser), new { userId = req.UserId }, entity);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
    {
        var list = await _repo.GetAllWithPredicateAsync(t => t.UserId == userId && t.IsActive);
        return Ok(list);
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> Unregister(string token, CancellationToken ct)
    {
        var matches = await _repo.GetAllWithPredicateAsync(t => t.Token == token);
        foreach (var t in matches)
        {
            t.IsActive = false;
            await _repo.UpdateAsync(t, ct);
        }
        return NoContent();
    }
}
