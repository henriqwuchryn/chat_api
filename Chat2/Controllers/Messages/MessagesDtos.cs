using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.model;

namespace Chat2.Controllers;

[AutoMap(typeof(Message))]
public class MessageDetailsDto:EntityDto
{
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserListItemDto Author { get; set; }
    public RoomListItemDto Room { get; set; }
}
[AutoMap(typeof(Message))]
public class MessageListItem:EntityDto
{
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserListItemDto Author { get; set; }
    public string RoomId { get; set; }
}

public class CreateMessageDto
{
    public string Body { get; set; }
}

public class EditMessageDto
{
    public string NewBody { get; set; }
}