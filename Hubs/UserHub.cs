using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Hubs
{
    public class UserHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User.IsInRole("Admin"))
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
            }

            await base.OnDisconnectedAsync(exception);
        }

    public Task NewUserValidation(string conversationDto) =>
            Clients.Group("Admin").SendAsync(conversationDto);
    }
}
