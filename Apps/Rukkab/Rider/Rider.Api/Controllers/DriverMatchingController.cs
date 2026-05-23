using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ACommerce.DriverMatching.Models;
using ACommerce.DriverMatching;

namespace Rukkab.Rider.Api.Controllers
{
    [ApiController]
    [Route("api/matching")]
    public class DriverMatchingController : ControllerBase
    {
        private readonly IDriverMatchingService _matching;

        public DriverMatchingController(IDriverMatchingService matching)
        {
            _matching = matching;
        }

        [HttpPost("find")]
        public async Task<IActionResult> Find([FromBody] DriverMatchRequest request)
        {
            var result = await _matching.FindNearestDriversAsync(request);
            return Ok(result);
        }
    }
}
