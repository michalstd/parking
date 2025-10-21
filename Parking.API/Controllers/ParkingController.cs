using Microsoft.AspNetCore.Mvc;
using Parking.Api.DTOs;
using Parking.Api.Services;

namespace Parking.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly IParkingService _svc;
        public ParkingController(IParkingService svc) => _svc = svc;

        [HttpPost]
        public async Task<ActionResult<ParkResponse>> Park([FromBody] ParkRequest req)
        {
            try
            {
                var res = await _svc.ParkVehicleAsync(req);
                return Ok(res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ParkingStatusResponse>> Status()
        {
            var res = await _svc.GetStatusAsync();
            return Ok(res);
        }

        [HttpPost("exit")]
        public async Task<ActionResult<ExitResponse>> Exit([FromBody] ExitRequest req)
        {
            try
            {
                var res = await _svc.ExitVehicleAsync(req);
                return Ok(res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}