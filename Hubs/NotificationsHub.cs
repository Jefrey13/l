using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomerService.API.Hubs
{
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out var uid))
            {
                // canal individual
                await Groups.AddToGroupAsync(Context.ConnectionId, uid.ToString());
                // si es admin, también al grupo "Admins"
                if (Context.User.IsInRole("Admin"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out var uid))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, uid.ToString());

                // Quitar del grupo de admins (si estaba)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}