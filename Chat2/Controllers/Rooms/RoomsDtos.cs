using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.model;

namespace Chat2.Controllers;

[AutoMap(typeof(Room))]
public class RoomDetailsDto : EntityDto
{
    public string Name { get; set; }
    public UserListItemDto Author { get; set; }
    public List<UserListItemDto> Users { get; set; }
}

public class RoomListItemDto : EntityDto
{
    public string Name { get; set; }
}

public class NewRoomDto
{
    public string RoomName { get; set; }
}

public class RenameRoomDto
{
    public string NewName { get; set; }
}