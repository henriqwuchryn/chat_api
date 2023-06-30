using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.Hubs;
using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomsController : BaseController
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _hub;

    public RoomsController(
        Context context,
        IConfiguration config,
        UserManager<User> userManager,
        IMapper mapper,
        IHubContext<ChatHub> hub) : base(userManager, context)
    {
        _context = context;
        _config = config;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    [Route("/AllRooms")]
    public IActionResult GetAllRooms()
    {
        var roomList = _context.Rooms.ToList();
        var roomListItemDtoList = roomList.Select(room => _mapper.Map<RoomListItemDto>(room)).ToList();
        return Ok(roomListItemDtoList);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var user = await GetUserOrFailAsync();
        var roomList = user.Rooms ?? new List<Room>();
        var roomListItemDtoList = roomList.Select(room => _mapper.Map<RoomListItemDto>(room)).ToList();
        return Ok(roomListItemDtoList);
    }

    [HttpGet]
    [Route("{roomId}")]
    public IActionResult GetRoomById(string roomId)
    {
        var room = _context.Rooms.Include("Users").First(r => r.Id == roomId);
        if (room == null)
            return NotFound();
        var getRoomDto = _mapper.Map<RoomDetailsDto>(room);
        return Ok(getRoomDto);
    }

    [HttpPost]
    [Authorize]
    [Route("/Me/Room")]
    public async Task<IActionResult> CreateRoom(NewRoomDto newRoomDto)
    {
        var user = await GetUserOrFailAsync();
        var room = new Room
        {
            Name = newRoomDto.Name,
            Description = newRoomDto.Description,
            AuthorId = user.Id
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        await JoinRoom(room.Id);

        var roomDto = _mapper.Map<RoomDetailsDto>(room);
        await BroadcastToRoom(room.Id, "updateRoom", roomDto);
        return Ok();
    }

    [HttpPost]
    [Authorize]
    [Route("{roomId}/Join")]
    public async Task<IActionResult> JoinRoom(string roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        var user = await GetUserOrFailAsync();

        if (room == null)
            return NotFound();

        user.Rooms ??= new List<Room>();
        user.Rooms.Add(room);
        await _context.SaveChangesAsync();
        await UserManager.UpdateAsync(user);
        var roomDto = _mapper.Map<RoomDetailsDto>(room);
        await BroadcastToRoom(roomId, "updateRoom", roomDto);
        return Ok();
    }


    [HttpPatch]
    [Authorize]
    [Route("/Me/Room/{roomId}")]
    public async Task<IActionResult> RenameRoom(string roomId, PatchRoomDto patchRoomDto)
    {
        var user = await GetUserOrFailAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        if (room?.AuthorId != user.Id) return Unauthorized();

        if (patchRoomDto.Name is "" or null) return Problem("Value can't be empty");

        room.Name = patchRoomDto.Name;
        room.Description = patchRoomDto.Description;
        await _context.SaveChangesAsync();
        var roomDto = _mapper.Map<RoomDetailsDto>(room);
        await BroadcastToRoom(roomId, "updateRoom", roomDto);
        return Ok();
    }

    [HttpDelete]
    [Route("/Me/Room/{roomId}")]
    public async Task<IActionResult> DeleteRoom(string roomId)
    {
        var user = await GetUserOrFailAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
            return BadRequest();
        if (room.AuthorId != user.Id)
            return Unauthorized();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        var roomDto = _mapper.Map<RoomDetailsDto>(room);
        await BroadcastToRoom(roomId, "deleteRoom", roomDto);
        return Ok();
    }

    
    private async Task BroadcastToRoom(string roomId, string method, object data)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
            return;
        foreach (var user in room.Users)
        {
            await _hub.Clients.User(user.Id).SendAsync(method, data);
        }
    }
}