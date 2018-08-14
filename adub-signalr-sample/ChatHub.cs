using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace adub_signalr_sample
{
    public class ChatHub : Hub
    {
        //public ChatHub()
        //{

        //}

        private readonly IHttpContextAccessor _httpContextAccessor;  
  
        //public ChatHub(IHttpContextAccessor httpContextAccessor)  
        //{  
        //    this._httpContextAccessor = httpContextAccessor;
            //if(_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
            //    _httpContextAccessor.HttpContext.Response.Cookies.Append("JSESSIONID", Guid.NewGuid().ToString(), new CookieOptions());  
        //}  

    

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

        //public void SetCookie(string key, string value, int? expireTime)  
        //{  
        //    CookieOptions option = new CookieOptions();  
        //    if (expireTime.HasValue)  
        //        option.Expires = DateTime.Now.AddMinutes(expireTime.Value);  
        //    else  
        //        option.Expires = DateTime.Now.AddMilliseconds(10);  
        //    Response.Cookies.Append(key, value, option);  
        //}  
    }
}