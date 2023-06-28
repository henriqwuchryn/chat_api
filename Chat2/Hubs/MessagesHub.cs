using Chat2.Controllers;
using Chat2.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat2.Hubs;

public class MessagesHub : Hub
{
    private readonly MessagesController _messagesController;
    private readonly RoomsController _roomsController;
    private readonly Context _context;

    public MessagesHub(
        MessagesController messagesController,
        RoomsController roomsController,
        Context context)
    {
        _messagesController = messagesController;
        _roomsController = roomsController;
        _context = context;
    }

    public async Task UpdateRoomMessageList(string roomId)
    {
        var data = _messagesController.ListMessages(roomId);
        List<User>? userList = _context.Rooms.Find(roomId)?.Users;
        foreach (var user in userList)
        {
            await BroadcastToUser(data, user.Id);
        }
    }

    public async Task BroadcastToUser(IActionResult data, string userId)
        => await Clients.User(userId).SendAsync("updateMessageList", data);
}

//await Clients.All.SendAsync will send a message to all connected clients

//public async Task BroadcastToUser(string data, string userId)     
// => await Clients.User(userId).SendAsync("broadcasttouser", data);