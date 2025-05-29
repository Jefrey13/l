using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CustomerService.API.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User.IsInRole("Admin"))
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
            }
            await base.OnDisconnectedAsync(exception);
        }
        public Task JoinConversation(string conversationId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        public Task LeaveConversation(string conversationId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

        public Task SendMessageToConversation(string conversationId, object message) =>
            Clients.Group(conversationId).SendAsync("ReceiveMessage", message);

        public Task BroadcastConversationCreated(object conversationDto) =>
        Clients.Group("Admin").SendAsync("ConversationCreated", conversationDto);   

        public Task BroadcastConversationUpdated(object conversationDto) =>
         Clients.All.SendAsync("ConversationUpdated", conversationDto);
    }
}