using System.Runtime.InteropServices.JavaScript;
using Chat2.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomsController : Controller
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly Context _context;

    public RoomsController(Context context, IConfiguration config, UserManager<User> userManager)
    {
        _context = context;
        _config = config;
        _userManager = userManager;
    }

    [HttpPost]
    public void CreateRoom(RoomDto roomDto)
    {
        _context.Rooms.Add(new Room
        {
            Name = roomDto.RoomName
        });
        _context.SaveChanges();
    }

    [HttpPost]
    [Route("Join/{roomName}")]
    public async Task<IActionResult> JoinRoom(string roomName)
    {
        var room = await _context.Rooms.FirstAsync(r => r.Name == roomName);
        var user = await _userManager.GetUserAsync(User);

        if (room != null && user.Rooms != null)
        {
            user.Rooms.Add(room);
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        if (room != null)
        {
            user.Rooms = new List<Room>();
            user.Rooms.Add(room);
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        return NotFound();
    }

    [HttpGet]
    [Route("{roomName}")]
    public IActionResult GetRoomByName(string roomName)
    {
        var room = _context.Rooms.Include("Users").First(r => r.Name == roomName);
        var getRoomDto = new GetRoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Users = room.Users.Select(users => new RoomUsersDto
            {
                Id = users.Id,
                UserName = users.UserName
            }).ToList()
        };
        return Ok(getRoomDto);
    }

    [HttpPatch]
    [Route("{roomName}")]
    public void RenameRoom(string roomName, RenameRoomDto renameRoomDto)
    {
        var room = _context.Rooms.First(r => r.Name == roomName);
        room.Name = renameRoomDto.NewName;
        _context.SaveChanges();
    }

    [HttpDelete]
    [Route("{roomName}")]
    public void DeleteRoom(string roomName)
    {
        var room = _context.Rooms.First(r => r.Name == roomName);
        _context.Rooms.Remove(room);
        _context.SaveChanges();
    }
}

public class RoomDto
{
    public string RoomName { get; set; }
}

public class GetRoomDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<RoomUsersDto> Users { get; set; }
}

public class RoomUsersDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
}

public class RenameRoomDto
{
    public string NewName { get; set; }
}