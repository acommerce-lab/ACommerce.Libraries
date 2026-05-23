using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ACommerce.RideLifecycle;
using ACommerce.DriverMatching.Models; // GeoPoint

namespace Rukkab.Rider.Api.Controllers
{
    [ApiController]
    [Route("api/rides")]
    public class RidesController : ControllerBase
    {
        private readonly IRideOrchestrator _orchestrator;

        public RidesController(IRideOrchestrator orchestrator) => _orchestrator = orchestrator;

        [HttpGet]
        public async Task<IActionResult> GetRides([FromQuery] string? riderId)
        {
            if (string.IsNullOrEmpty(riderId)) return Ok(Array.Empty<object>());
            var rides = await _orchestrator.GetRidesForRiderAsync(riderId);
            return Ok(rides);
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestRide([FromBody] RequestRideDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.RiderId)) return BadRequest("riderId is required");

            var pickup = new GeoPoint(dto.PickupLat, dto.PickupLng);
            GeoPoint? dropoff = dto.DropoffLat.HasValue && dto.DropoffLng.HasValue ? new GeoPoint(dto.DropoffLat.Value, dto.DropoffLng.Value) : null;

            var ride = await _orchestrator.RequestRideAsync(dto.RiderId, pickup, dropoff);
            return Ok(ride);
        }

        // Minimal DTO used only by this controller
        public class RequestRideDto
        {
            public double PickupLat { get; set; }
            public double PickupLng { get; set; }
            public double? DropoffLat { get; set; }
            public double? DropoffLng { get; set; }
            public string RiderId { get; set; } = string.Empty;
        }
    }
}
