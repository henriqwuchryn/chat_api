using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chat2.model;

/// <summary>
///     User contains Id (string), UserName, PasswordHash.
/// </summary>
[Index("UserName", IsUnique = true)]
public class User : IdentityUser
{
    public virtual List<Room> Rooms { get; set; }
    public virtual List<Message> Messages { get; set; }
    public virtual List<Role> Roles { get; set; }
}