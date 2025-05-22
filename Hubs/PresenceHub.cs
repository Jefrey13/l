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
            if (int.TryParse(Context.UserIdentifier, out var id))
                await _presence.UserConnectedAsync(id);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            if (int.TryParse(Context.UserIdentifier, out var id))
                await _presence.UserDisconnectedAsync(id);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
