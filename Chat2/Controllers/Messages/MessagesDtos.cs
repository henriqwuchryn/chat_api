using AutoMapper;
using Chat2.model;

namespace Chat2.Controllers;

[AutoMap(typeof(Message))]
public class MessageDetailsDto
{
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserListItemDto Author { get; set; }
    public RoomListItemDto Room { get; set; }
}

public class CreateMessageDto
{
    public string Body { get; set; }
}

public class EditMessageDto
{
    public string NewBody { get; set; }
}