using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("/rooms/{roomId}/[controller]")]
public class MessagesController : Controller
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private UserManager<User> UserManager;

    public MessagesController(Context context, IConfiguration config, UserManager<User> userManager)
    {
        _context = context;
        _config = config;
        UserManager = userManager;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMessage(string roomId, CreateMessageDto createMessageDto)
    {
        var author = await UserManager.GetUserAsync(User);
        var room = await _context.Rooms.FindAsync(roomId);
        var message = new Message
        {
            Body = createMessageDto.Body,
            CreatedAt = DateTime.UtcNow,
            Author = author,
            Room = room
        };
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        var messageDto = new MessageDto
        {
            Author = new UserDetailsDto
            {
                Id = author.Id,
                UserName = author.UserName
            },
            Body = message.Body,
            CreatedAt = message.CreatedAt,
            Room = new RoomDetailsDto
            {
                Id = room.Id,
                Name = room.Name
            }
        };
        return Ok(messageDto);
    }

    [HttpGet]
    [Route("{messageId}")]
    public IActionResult GetMessage(string messageId)
    {
        var message = _context.Messages.Include("Author").Include("Room").First(m => m.Id == messageId);
        var getmessage = new MessageDetailsDto
        {
            Body = message.Body,
            RoomName = message.Room.Name,
            AuthorName = message.Author.UserName
        };
        return Ok(getmessage);
    }


    [HttpPatch]
    public IActionResult EditMessage(EditMessageDto editMessageDto)
    {
        var message = _context.Messages.First(m => m.Id == editMessageDto.Id);
        message.Body = editMessageDto.NewBody;
        message.Edited = true;
        _context.SaveChanges();
        return Ok();
    }

    [HttpDelete]
    [Route("{messageId}")]
    public void DeleteMessage(string messageId)
    {
        var message = _context.Messages.First(m => m.Id == messageId);
        _context.Messages.Remove(message);
        _context.SaveChanges();
    }
}