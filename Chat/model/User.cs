using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chat.model;

[Index("UserName", IsUnique = true)]
public class User : IdentityUser<int>
{
    
    public string Name { get; set; }
    public virtual List<Room> Rooms { get; set; }
    public virtual List<Message> Messages { get; set; }
    public virtual List<Role> Roles { get; set; }
}