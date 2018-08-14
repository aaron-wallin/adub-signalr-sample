using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace adub_signalr_sample
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Console.Out.WriteLineAsync($"Processing message from {user} : {message}");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await Console.Out.WriteLineAsync("DISCONNECTED: " + exception.ToString());
            await base.OnDisconnectedAsync(exception);
        }
    }
}