using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ACommerce.DriverMatching.Models;
using ACommerce.DriverMatching;

namespace Rukkab.Driver.Api.Controllers
{
    [ApiController]
    [Route("api/driver")]
    public class DriverStatusController : ControllerBase
    {
        private readonly IDriverMatchingService _matching;

        public DriverStatusController(IDriverMatchingService matching)
        {
            _matching = matching;
        }

        [HttpPost("location")]
        public async Task<IActionResult> UpdateLocation([FromQuery] string driverId, [FromBody] GeoPoint location, [FromQuery] DriverStatus status = DriverStatus.Available)
        {
            if (string.IsNullOrEmpty(driverId)) return BadRequest("driverId is required");

            await _matching.UpdateDriverLocationAsync(driverId, location, status);
            return Ok();
        }
    }
}
