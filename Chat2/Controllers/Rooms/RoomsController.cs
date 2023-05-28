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

    public RoomsController(Context context, IConfiguration config, UserManager<User> userManager, IMapper mapper) :
        base(userManager)
    {
        _context = context;
        _config = config;
        _mapper = mapper;
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
    [Route("/me/room")]
    public async Task<IActionResult> CreateRoom(NewRoomDto newRoomDto)
    {
        var user = await GetUserOrFailAsync();
        _context.Rooms.Add(new Room
        {
            Name = newRoomDto.RoomName,
            AuthorId = user.Id
        });
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Authorize]
    [Route("{roomId}/Join")]
    public async Task<IActionResult> JoinRoom(string roomId)
    {
        var room = await _context.Rooms.FirstAsync(r => r.Id == roomId);
        var user = await GetUserOrFailAsync();

        if (room != null)
        {
            user.Rooms ??= new List<Room>();
            user.Rooms.Add(room);
            await UserManager.UpdateAsync(user);
            return Ok();
        }

        return NotFound();
    }

    [HttpPatch]
    [Authorize]
    [Route("/me/{roomId}")]
    public async Task<IActionResult> RenameRoom(string roomId, RenameRoomDto renameRoomDto)
    {
        var user = await GetUserOrFailAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        if (room.AuthorId == user.Id)
        {
            room.Name = renameRoomDto.NewName;
            await _context.SaveChangesAsync();
            return Ok();
        }

        return Unauthorized();
    }

    [HttpDelete]
    [Route("/me/{roomId}")]
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