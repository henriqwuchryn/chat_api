using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.Controllers.Messages;
using Chat2.Hubs;
using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("/Rooms/{roomId}/[controller]")]
public class MessagesController : BaseController
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _hub;

    public MessagesController(
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

    [HttpPost]
    [Authorize]
    [Route("/Me/Message")]
    public async Task<IActionResult> CreateMessage(string roomId, CreateMessageDto createMessageDto)
    {
        var author = await GetUserOrFailAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
            return BadRequest();
        var message = new Message
        {
            Body = createMessageDto.Body,
            CreatedAt = DateTime.UtcNow,
            Author = author,
            Room = room
        };
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        var messageDto = _mapper.Map<MessageDetailsDto>(message);
        await _hub.Clients.Group(room.Id).SendAsync("addNewMessage", messageDto);
        return CreatedAtAction(nameof(GetMessage), messageDto.Id, messageDto);
    }

    [HttpGet]
    [Route("{messageId}")]
    public IActionResult GetMessage(string messageId)
    {
        var message = _context.Messages.Include("Author").Include("Room").First(m => m.Id == messageId);
        var getmessage = _mapper.Map<MessageDetailsDto>(message);
        return Ok(getmessage);
    }

    [HttpGet]
    public IActionResult ListMessages(string roomId)
    {
        var room = _context.Rooms.Include("Messages").Include("Messages.Author").First(r => r.Id == roomId);
        var listMessages = room.Messages
            .ToList()
            .OrderByDescending(m => m.CreatedAt)
            .Reverse();
        var messageListItemDtoList = _mapper.Map<List<MessageListItem>>(listMessages);
        return Ok(messageListItemDtoList);
    }

    [HttpPatch]
    [Authorize]
    [Route("/Me/Message/{messageId}")]
    public async Task<IActionResult> EditMessage(string messageId, EditMessageDto editMessageDto)
    {
        var user = await GetUserOrFailAsync();
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return NotFound();
        if (message.Author != user)
            return Unauthorized();
        message.Body = editMessageDto.NewBody;
        message.Edited = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Authorize]
    [Route("/Me/Message/{messageId}")]
    public async Task<IActionResult> DeleteMessage(string messageId)
    {
        var user = await GetUserOrFailAsync();
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return NotFound();
        if (message.Author != user)
            return Unauthorized();
        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return Ok();
    }
}