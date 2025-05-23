using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PresenceController : ControllerBase
    {
        private readonly IPresenceService _presence;
        public PresenceController(IPresenceService presence) => _presence = presence;

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            var last = await _presence.GetLastOnlineAsync(userId);
            var isOnline = last.HasValue;
            return Ok(new { isOnline, lastOnline = last });
        }
    }
}
