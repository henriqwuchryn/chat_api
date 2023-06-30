using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.model;

namespace Chat2.Controllers;

[AutoMap(typeof(User))]
public class UserDetailsDto : EntityDto
{
    public string UserName { get; set; }
    public List<RoomListItemDto> Rooms { get; set; }
}

[AutoMap(typeof(User))]
public class UserListItemDto : EntityDto
{
    public string UserName { get; set; }
}

public class UpdatePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}