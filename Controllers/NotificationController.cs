using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRecipientService _recipientService;

        public NotificationsController(INotificationRecipientService recipientService)
            => _recipientService = recipientService;

        private int CurrentUserId
            => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet(Name = "GetUserNotifications")]
        [SwaggerOperation(Summary = "Retrieve paged list of your notifications")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] PaginationParams @params,
            [FromQuery] bool unreadOnly = false,
            CancellationToken ct = default)
        {
            if (unreadOnly)
            {
                var pagedUnread = await _recipientService
                    .GetUnreadByUserAsync(@params, CurrentUserId, ct);
                return Ok(new ApiResponse<PagedResponse<NotificationDto>>(pagedUnread, "Unread notifications retrieved."));
            }
            else
            {
                var pagedAll = await _recipientService
                    .GetByUserAsync(@params, CurrentUserId, ct);
                return Ok(new ApiResponse<PagedResponse<NotificationDto>>(pagedAll, "All notifications retrieved."));
            }
        }

        [HttpGet("unread-count", Name = "GetUnreadNotificationCount")]
        [SwaggerOperation(Summary = "Get count of unread notifications")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
        {
            var count = await _recipientService.GetUnreadCountAsync(CurrentUserId, ct);
            return Ok(new ApiResponse<int>(count, "Unread notifications count retrieved."));
        }

        [HttpPut("{recipientId}/read", Name = "MarkNotificationAsRead")]
        [SwaggerOperation(Summary = "Mark a notification as read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(
            [FromRoute] int recipientId,
            CancellationToken ct = default)
        {
            await _recipientService.MarkAsReadAsync(recipientId, ct);
            return NoContent();
        }

        [HttpPut("read-all", Name = "MarkAllNotificationsAsRead")]
        [SwaggerOperation(Summary = "Mark all notifications as read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
        {
            await _recipientService.MarkAllReadAsync(CurrentUserId, ct);
            return NoContent();
        }
    }
}