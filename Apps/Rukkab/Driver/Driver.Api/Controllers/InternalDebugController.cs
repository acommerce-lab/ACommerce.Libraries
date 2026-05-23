using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ACommerce.RideLifecycle;
using ACommerce.RideLifecycle.Models;

namespace Rukkab.Driver.Api.Controllers
{
    [ApiController]
    [Route("api/internal/debug")]
    public class InternalDebugController : ControllerBase
    {
        private readonly IRideOrchestrator _orchestrator;

        public InternalDebugController(IRideOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("import-ride")]
        public async Task<IActionResult> ImportRide([FromBody] Ride ride)
        {
            if (ride == null) return BadRequest("ride body is required");
            // If the registered orchestrator is the in-memory implementation we added ImportRideAsync to,
            // call it. Otherwise attempt best-effort: if the concrete implementation exposes a method
            // named ImportRideAsync, invoke it via reflection (development convenience only).
            if (_orchestrator is InMemoryRideOrchestrator mem)
            {
                await mem.ImportRideAsync(ride);
            }
            else
            {
                var method = _orchestrator.GetType().GetMethod("ImportRideAsync");
                if (method != null)
                {
                    var task = (System.Threading.Tasks.Task)method.Invoke(_orchestrator, new object[] { ride })!;
                    await task;
                }
                else
                {
                    return BadRequest("orchestrator does not support import");
                }
            }
            return Ok();
        }
    }
}
