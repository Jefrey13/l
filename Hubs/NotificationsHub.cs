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

        /// <summary>
        /// Envia a todos los Admins una notificación de que un cliente solicitó un agente.
        /// </summary>
        public Task BroadcastSupportRequested(object payload) =>
            Clients.Group("Admins").SendAsync("SupportRequested", payload);

        /// <summary>
        /// Envía al agente la notificación de conversación asignada.
        /// </summary>
        public Task BroadcastConversationAssigned(int agentUserId, object payload) =>
            Clients.Group(agentUserId.ToString())
                   .SendAsync("ConversationAssigned", payload);
    }
}