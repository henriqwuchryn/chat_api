using Chat.model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace Chat.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomsController : Controller
{
    private readonly IConfiguration _config;
    private Context _context;

    public RoomsController(Context context, IConfiguration config)
    {
        _context = context;
        _config = config;
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
    [Route("Join")]
    public void JoinRoom(JoinRoomDto joinRoomDto)
    {
        var room = _context.Rooms.First(r => r.Id == joinRoomDto.RoomId);
        var user = _context.Users.Include("Rooms").First(u => u.Id == joinRoomDto.UserId);
        user.Rooms.Add(room);
        _context.SaveChanges();
    }

    [HttpGet]
    [Route("{roomId}")]
    public IActionResult GetRoomById(int roomId)
    {
        var room = _context.Rooms.Include("Users").First(r => r.Id == roomId);
        var getRoomDto = new GetRoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Users = room.Users.Select(users => new RoomUsersDto
            {
                Id = Convert.ToInt32(users.Id),
                Name = users.Name
            }).ToList()
        };
        return Ok(getRoomDto);
    }

    [HttpPatch]
    [Route("{roomId}")]
    public void RenameRoom(int roomId, RenameRoomDto renameRoomDto)
    {
        var room = _context.Rooms.First(r => r.Id == roomId);
        room.Name = renameRoomDto.NewName;
        _context.SaveChanges();
    }

    [HttpDelete]
    [Route("{roomId}")]
    public void DeleteRoom(int roomId)
    {
        var room = _context.Rooms.First(r => r.Id == roomId);
        _context.Rooms.Remove(room);
        _context.SaveChanges();
    }
}

public class RoomDto
{
    public string RoomName { get; set; }
}

public class JoinRoomDto
{
    public int UserId { get; set; }
    public int RoomId { get; set; }
}

public class GetRoomDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<RoomUsersDto> Users { get; set; }
}

public class RoomUsersDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class RenameRoomDto
{
    public string NewName { get; set; }
}