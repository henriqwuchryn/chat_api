using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("[controller]")]
public class RoomsController : BaseController
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly IMapper _mapper;

    public RoomsController(
        Context context,
        IConfiguration config,
        UserManager<User> userManager,
        IMapper mapper) : base(userManager, context)
    {
        _context = context;
        _config = config;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetAllRooms()
    {
        var roomList = _context.Rooms.ToList();
        var roomListItemDtoList = roomList.Select(room => _mapper.Map<RoomListItemDto>(room)).ToList();
        return Ok(roomListItemDtoList);
    }

    [HttpGet]
    [Route("{roomId}")]
    public IActionResult GetRoomById(string roomId)
    {
        var room = _context.Rooms.Include("Users").First(r => r.Id == roomId);
        if (room != null)
        {
            var getRoomDto = _mapper.Map<RoomDetailsDto>(room);
            return Ok(getRoomDto);
        }

        return NotFound();
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
            AuthorId = user.Id,
        };
        _context.Rooms.Add(room);
        user.Rooms ??= new List<Room>();
        user.Rooms.Add(room);
        await _context.SaveChangesAsync();
        await UserManager.UpdateAsync(user);
        return Ok();
    }

    [HttpPost]
    [Authorize]
    [Route("{roomId}/Join")]
    public async Task<IActionResult> JoinRoom(string roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        var user = await GetUserOrFailAsync();
        var userContext = await _context.Users.FindAsync(user.Id);

        if (room != null)
        {
            userContext.Rooms ??= new List<Room>();
            userContext.Rooms.Add(room);
            await UserManager.UpdateAsync(user);
            return Ok();
        }

        return NotFound();
    }

    [HttpPatch]
    [Authorize]
    [Route("/Me/Room/{roomId}")]
    public async Task<IActionResult> RenameRoom(string roomId, PatchRoomDto patchRoomDto)
    {
        var user = await GetUserOrFailAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        if (room?.AuthorId != user.Id)
        {
            return Unauthorized();
        }

        if (patchRoomDto.Name is "" or null)
        {
            return Problem("Value can't be empty");
        }

        room.Name = patchRoomDto.Name;
                      room.Description = patchRoomDto.Description;
                      await _context.SaveChangesAsync();
                      return Ok();
    }

    [HttpDelete]
    [Route("/Me/Room/{roomId}")]
    public async Task<IActionResult> DeleteRoom(string roomId)
    {
        var user = await GetUserOrFailAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        if (room.AuthorId == user.Id)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok();
        }

        return Unauthorized();
    }
}