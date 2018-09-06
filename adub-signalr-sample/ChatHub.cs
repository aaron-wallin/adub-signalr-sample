using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace adub_signalr_sample
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string to, string from, string message)
        {
            if (!string.IsNullOrWhiteSpace(to) || !string.IsNullOrWhiteSpace(from) || !string.IsNullOrWhiteSpace(message))
            {
                await Console.Out.WriteLineAsync($"Processing message from:{from} to:{to} message:{message}");

                if (to.ToLower() == "*")
                {                    
                    await Clients.All.SendAsync("ReceiveMessage", from, message);
                }
                else
                {
                    await Clients.Group(to.ToLower()).SendAsync("ReceiveMessage", from, message);
                }
            }
        }

        public async Task RegisterUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
                await Task.CompletedTask;

            await Console.Out.WriteLineAsync($"Registering {user} : {Context.ConnectionId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, user?.ToLower());
            await Clients.All.SendAsync("ReceiveMessage", "SYSTEM", $"A new user registered: {user?.ToLower()}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await Console.Out.WriteLineAsync("DISCONNECTED: " + exception.ToString());
            await base.OnDisconnectedAsync(exception);
        }
    }
}