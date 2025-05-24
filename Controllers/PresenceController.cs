using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PresenceController : ControllerBase
    {
        private readonly IPresenceService _presenceService;
        public PresenceController(IPresenceService presenceService) => _presenceService = presenceService;

        [HttpGet("{userId}", Name = "Get last user online conexión")]
        public async Task<IActionResult> GetLastOnline(int userId, CancellationToken ct = default)
        {
            var last = await _presenceService.GetLastOnlineAsync(userId, ct);
            var isOnline = last.HasValue;

            return Ok(new { isOnline, lastOnline = last });
        }
    }
}