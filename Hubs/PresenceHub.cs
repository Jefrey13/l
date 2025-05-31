using System;
using System.Threading.Tasks;
using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Hubs
{
    public class PresenceHub : Hub
    {
        private readonly IPresenceService _presence;

        public PresenceHub(IPresenceService presence) => _presence = presence;

        public override async Task OnConnectedAsync()
        {
            if (int.TryParse(Context.UserIdentifier, out var userId))
            {
                await _presence.UserConnectedAsync(userId);
                await Clients.Others.SendAsync("UserIsOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (int.TryParse(Context.UserIdentifier, out var userId))
            {
                await _presence.UserDisconnectedAsync(userId);
                await Clients.Others.SendAsync("UserIsOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}