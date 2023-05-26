using Chat.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace Chat.Controllers;

[ApiController]
[Route("[controller]")]
public class MessagesController : Controller
{
    private readonly IConfiguration _config;
    private Context _context;

    public MessagesController(Context context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost]
    public IActionResult CreateMessage(MessageDto messageDto)
    {
        var message = new Message
        {
            Body = messageDto.Body,
            CreatedAt = DateTime.UtcNow,
            Author = _context.Users.First(user => user.Id == messageDto.AuthorId),
            Room = _context.Rooms.First(room => room.Id == messageDto.RoomId)
        };
        _context.Messages.Add(message);
        _context.SaveChanges();
        return Ok(message);
    }

    [HttpGet]
    [Route("{messageId}")]
    public IActionResult GetMessage(int messageId)
    {
        var message = _context.Messages.Include("Author").First(m => m.Id == messageId);
        var getmessage = new GetMessageDto
        {
            Body = message.Body,
            RoomId = message.RoomId,
            Author = new GetMessageAuthorDto
            {
                Id = message.AuthorId,
                Name = message.Author.Name,
                UserName = message.Author.UserName
            }
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
        return Ok($"Message nº {message.Id} was succesfully edited.");
    }

    [HttpDelete]
    [Route("{messageId}")]
    public void DeleteMessage(int messageId)
    {
        var message = _context.Messages.First(m => m.Id == messageId);
        _context.Messages.Remove(message);
        _context.SaveChanges();
    }
}

public class MessageDto
{
    public string Body { get; set; }
    public virtual int AuthorId { get; set; }
    public virtual int RoomId { get; set; }
}

public class GetMessageDto
{
    public string Body { get; set; }
    public GetMessageAuthorDto Author { get; set; }
    public int RoomId { get; set; }
}

public class GetMessageAuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string UserName { get; set; }
}

public class EditMessageDto
{
    public int Id { get; set; }
    public string NewBody { get; set; }
}