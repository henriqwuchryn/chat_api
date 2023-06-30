using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat2.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine(Context.User.ToString());
        return base.OnConnectedAsync();
    }
}


//await Clients.All.SendAsync will send a message to all connected clients

//public async Task BroadcastToUser(string data, string userId)     
// => await Clients.User(userId).SendAsync("broadcasttouser", data);